using SearchService.Contracts.Catalog;
using SearchService.Contracts.Common;
using SearchService.Contracts.Search;

namespace SearchService.Services.Interfaces;

public interface ISearchService
{
    Task<PagedResponse<SearchResponse>> SearchAsync(SearchTestsRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<CatalogItemResponse>> GetCatalogAsync(string? category, int page, int size, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogItemResponse>> GetPopularAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogItemResponse>> GetNewAsync(int limit = 10, CancellationToken cancellationToken = default);
}