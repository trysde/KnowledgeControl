using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Persistence;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Services.Implementations;

public class IndexService(
    SearchDbContext dbContext,
    ISearchCache searchCache,
    ILogger<IndexService> logger) : IIndexService
{
    public async Task OnPublishedAsync(
        TestPublishedEvent message,
        CancellationToken cancellationToken = default)
    {
        await UpsertIndexAsync(
            testUid: message.TestUid,
            title: message.Title,
            description: message.Description,
            authorUid: message.AuthorUid,
            category: message.Category,
            tags: message.Tags,
            updatedAt: message.PublishedAt.UtcDateTime,
            cancellationToken: cancellationToken);

        logger.LogInformation("Тест {TestUid} добавлен/обновлён в поисковом индексе.", message.TestUid);
    }

    public async Task OnUpdatedAsync(
        TestUpdatedEvent message,
        CancellationToken cancellationToken = default)
    {
        await UpsertIndexAsync(
            testUid: message.TestUid,
            title: message.Title,
            description: message.Description,
            authorUid: message.AuthorUid,
            category: message.Category,
            tags: message.Tags,
            updatedAt: message.OccurredAt.UtcDateTime,
            cancellationToken: cancellationToken);

        logger.LogInformation("Поисковый индекс теста {TestUid} обновлён.", message.TestUid);
    }

    public async Task OnUnpublishedAsync(
        TestUnpublishedEvent message,
        CancellationToken cancellationToken = default)
    {
        var deletedCount = await dbContext.SearchIndices
            .Where(x => x.TestUid == message.TestUid)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            await searchCache.InvalidateAllAsync(cancellationToken);
        }

        logger.LogInformation("Тест {TestUid} удалён из поискового индекса (удалено записей: {Count}).",
            message.TestUid, deletedCount);
    }

    private async Task UpsertIndexAsync(
        Guid testUid,
        string title,
        string? description,
        Guid authorUid,
        string category,
        IReadOnlyList<string> tags,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.SearchIndices
            .FirstOrDefaultAsync(x => x.TestUid == testUid, cancellationToken);

        var formattedTags = FormatTags(tags);
        var trimmedTitle = Truncate(title.Trim(), 255);
        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        var trimmedSubject = string.IsNullOrWhiteSpace(category) ? null : Truncate(category.Trim(), 100);

        if (existing is null)
        {
            dbContext.SearchIndices.Add(new SearchIndex
            {
                TestUid = testUid,
                Title = trimmedTitle,
                Description = trimmedDescription,
                AuthorUid = authorUid,
                AuthorName = string.Empty,
                Subject = trimmedSubject,
                Tags = formattedTags,
                IsPublic = true,
                QuestionCount = 0,
                UpdatedAt = updatedAt
            });
        }
        else
        {
            // Защита от устаревших (out-of-order) событий из очереди
            if (updatedAt < existing.UpdatedAt)
            {
                logger.LogDebug("Пропущено устаревшее событие индексации для теста {TestUid}.", testUid);
                return;
            }

            existing.Title = trimmedTitle;
            existing.Description = trimmedDescription;
            existing.AuthorUid = authorUid;
            existing.Subject = trimmedSubject;
            existing.Tags = formattedTags;
            existing.IsPublic = true;
            existing.UpdatedAt = updatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await searchCache.InvalidateAllAsync(cancellationToken);
    }

    private static string? FormatTags(IReadOnlyList<string> tags)
    {
        if (tags.Count == 0)
            return null;

        var joined = string.Join(", ", tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));

        return string.IsNullOrEmpty(joined) ? null : Truncate(joined, 500);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}