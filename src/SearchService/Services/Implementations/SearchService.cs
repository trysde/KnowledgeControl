using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using SearchService.Contracts.Catalog;
using SearchService.Contracts.Common;
using SearchService.Contracts.Search;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Persistence;
using SearchService.Services.Interfaces;

namespace SearchService.Services.Implementations;

public class SearchService(
    SearchDbContext dbContext,
    ISearchCache searchCache) : ISearchService
{
    private static readonly TimeSpan SearchCacheTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan CatalogCacheTtl = TimeSpan.FromMinutes(5);

    public async Task<PagedResponse<SearchResponse>> SearchAsync(
        SearchTestsRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(request.Q) ? null : request.Q.Trim();
        var normalizedCategory = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        var normalizedTag = string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag.Trim();
        var normalizedSort = string.IsNullOrWhiteSpace(request.Sort)
            ? "relevance"
            : request.Sort.Trim().ToLowerInvariant();

        var version = await searchCache.GetVersionAsync(cancellationToken);
        var rawQueryKey = $"q={normalizedQuery}|cat={normalizedCategory}|tag={normalizedTag}|author={request.Author}|sort={normalizedSort}|p={request.Page}|s={request.Size}";
        var queryHash = ComputeQueryHash(rawQueryKey);
        var cacheKey = $"v{version}:search:cache:{queryHash}";

        var cached = await searchCache.GetAsync<PagedResponse<SearchResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var baseQuery = dbContext.SearchIndices
            .AsNoTracking()
            .Where(x => x.IsPublic);

        if (normalizedCategory is not null)
        {
            baseQuery = baseQuery.Where(x => x.Subject != null && EF.Functions.ILike(x.Subject, normalizedCategory));
        }

        if (normalizedTag is not null)
        {
            var tagPattern = $"%{EscapeLikePattern(normalizedTag)}%";
            baseQuery = baseQuery.Where(x => x.Tags != null && EF.Functions.ILike(x.Tags, tagPattern));
        }

        if (request.Author.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.AuthorUid == request.Author.Value);
        }

        NpgsqlTsQuery? tsQuery = null;
        if (normalizedQuery is not null)
        {
            tsQuery = EF.Functions.WebSearchToTsQuery("russian", normalizedQuery);
            baseQuery = baseQuery.Where(x => x.SearchVector.Matches(tsQuery));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var joinedQuery = from idx in baseQuery
                          join stats in dbContext.TestStatistics.AsNoTracking()
                              on idx.TestUid equals stats.TestUid into statsGroup
                          from stats in statsGroup.DefaultIfEmpty()
                          select new SearchProjection
                          {
                              Index = idx,
                              TotalAttempts = stats != null ? stats.TotalAttempts : 0,
                              PassRate = stats != null ? stats.PassRate : 0m,
                              AvgScore = stats != null ? stats.AvgScore : 0m,
                              Rank = tsQuery != null ? idx.SearchVector.Rank(tsQuery) : 0f
                          };

        joinedQuery = normalizedSort switch
        {
            "popular" => joinedQuery
                .OrderByDescending(x => x.TotalAttempts)
                .ThenByDescending(x => x.Index.UpdatedAt),
            "newest" => joinedQuery
                .OrderByDescending(x => x.Index.UpdatedAt),
            "pass_rate" => joinedQuery
                .OrderByDescending(x => x.PassRate)
                .ThenByDescending(x => x.TotalAttempts),
            "avg_score" => joinedQuery
                .OrderByDescending(x => x.AvgScore)
                .ThenByDescending(x => x.TotalAttempts),
            _ => tsQuery != null
                ? joinedQuery
                    .OrderByDescending(x => x.Rank)
                    .ThenByDescending(x => x.TotalAttempts)
                    .ThenByDescending(x => x.Index.UpdatedAt)
                : joinedQuery
                    .OrderByDescending(x => x.Index.UpdatedAt)
        };

        var rows = await joinedQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(r => new SearchResponse(
                TestUid: r.Index.TestUid,
                Title: r.Index.Title,
                Description: r.Index.Description,
                AuthorUid: r.Index.AuthorUid,
                AuthorName: r.Index.AuthorName,
                Subject: r.Index.Subject,
                QuestionCount: r.Index.QuestionCount,
                TotalAttempts: r.TotalAttempts,
                PassRate: r.PassRate,
                AvgScore: r.AvgScore,
                Rank: r.Rank,
                UpdatedAt: new DateTimeOffset(r.Index.UpdatedAt, TimeSpan.Zero)))
            .ToList();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.Size);
        var result = new PagedResponse<SearchResponse>(items, request.Page, request.Size, totalCount, totalPages);

        await searchCache.SetAsync(cacheKey, result, SearchCacheTtl, cancellationToken);
        return result;
    }

    public async Task<PagedResponse<CatalogItemResponse>> GetCatalogAsync(
        string? category,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var normalizedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        var version = await searchCache.GetVersionAsync(cancellationToken);
        var cacheKey = $"v{version}:catalog:all:cat={normalizedCategory}:p={page}:s={size}";

        var cached = await searchCache.GetAsync<PagedResponse<CatalogItemResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var baseQuery = dbContext.SearchIndices
            .AsNoTracking()
            .Where(x => x.IsPublic);

        if (normalizedCategory is not null)
        {
            baseQuery = baseQuery.Where(x => x.Subject != null && EF.Functions.ILike(x.Subject, normalizedCategory));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var rows = await (from idx in baseQuery
                          join stats in dbContext.TestStatistics.AsNoTracking()
                              on idx.TestUid equals stats.TestUid into statsGroup
                          from stats in statsGroup.DefaultIfEmpty()
                          orderby idx.UpdatedAt descending
                          select new CatalogProjection
                          {
                              Index = idx,
                              TotalAttempts = stats != null ? stats.TotalAttempts : 0,
                              PassRate = stats != null ? stats.PassRate : 0m,
                              AvgScore = stats != null ? stats.AvgScore : 0m
                          })
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        var items = rows.Select(MapToCatalogItem).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)size);
        var result = new PagedResponse<CatalogItemResponse>(items, page, size, totalCount, totalPages);

        await searchCache.SetAsync(cacheKey, result, CatalogCacheTtl, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<CatalogItemResponse>> GetPopularAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var clampedLimit = Math.Clamp(limit, 1, 50);
        var version = await searchCache.GetVersionAsync(cancellationToken);
        var cacheKey = $"v{version}:catalog:popular:{clampedLimit}";

        var cached = await searchCache.GetAsync<List<CatalogItemResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var rows = await (from idx in dbContext.SearchIndices.AsNoTracking()
                          where idx.IsPublic
                          join stats in dbContext.TestStatistics.AsNoTracking()
                              on idx.TestUid equals stats.TestUid into statsGroup
                          from stats in statsGroup.DefaultIfEmpty()
                          let attempts = stats != null ? stats.TotalAttempts : 0
                          orderby attempts descending, idx.UpdatedAt descending
                          select new CatalogProjection
                          {
                              Index = idx,
                              TotalAttempts = attempts,
                              PassRate = stats != null ? stats.PassRate : 0m,
                              AvgScore = stats != null ? stats.AvgScore : 0m
                          })
            .Take(clampedLimit)
            .ToListAsync(cancellationToken);

        var items = rows.Select(MapToCatalogItem).ToList();
        await searchCache.SetAsync(cacheKey, items, CatalogCacheTtl, cancellationToken);
        return items;
    }

    public async Task<IReadOnlyList<CatalogItemResponse>> GetNewAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var clampedLimit = Math.Clamp(limit, 1, 50);
        var version = await searchCache.GetVersionAsync(cancellationToken);
        var cacheKey = $"v{version}:catalog:new:{clampedLimit}";

        var cached = await searchCache.GetAsync<List<CatalogItemResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var rows = await (from idx in dbContext.SearchIndices.AsNoTracking()
                          where idx.IsPublic
                          join stats in dbContext.TestStatistics.AsNoTracking()
                              on idx.TestUid equals stats.TestUid into statsGroup
                          from stats in statsGroup.DefaultIfEmpty()
                          orderby idx.UpdatedAt descending
                          select new CatalogProjection
                          {
                              Index = idx,
                              TotalAttempts = stats != null ? stats.TotalAttempts : 0,
                              PassRate = stats != null ? stats.PassRate : 0m,
                              AvgScore = stats != null ? stats.AvgScore : 0m
                          })
            .Take(clampedLimit)
            .ToListAsync(cancellationToken);

        var items = rows.Select(MapToCatalogItem).ToList();
        await searchCache.SetAsync(cacheKey, items, CatalogCacheTtl, cancellationToken);
        return items;
    }

    private static string ComputeQueryHash(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private static CatalogItemResponse MapToCatalogItem(CatalogProjection row) =>
        new(
            TestUid: row.Index.TestUid,
            Title: row.Index.Title,
            Description: row.Index.Description,
            AuthorUid: row.Index.AuthorUid,
            AuthorName: row.Index.AuthorName,
            Subject: row.Index.Subject,
            QuestionCount: row.Index.QuestionCount,
            TotalAttempts: row.TotalAttempts,
            PassRate: row.PassRate,
            AvgScore: row.AvgScore,
            UpdatedAt: new DateTimeOffset(row.Index.UpdatedAt, TimeSpan.Zero));

    private static string EscapeLikePattern(string value) =>
        value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    private sealed class SearchProjection
    {
        public required SearchIndex Index { get; init; }
        public int TotalAttempts { get; init; }
        public decimal PassRate { get; init; }
        public decimal AvgScore { get; init; }
        public float Rank { get; init; }
    }

    private sealed class CatalogProjection
    {
        public required SearchIndex Index { get; init; }
        public int TotalAttempts { get; init; }
        public decimal PassRate { get; init; }
        public decimal AvgScore { get; init; }
    }
}