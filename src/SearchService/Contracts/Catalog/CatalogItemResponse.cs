namespace SearchService.Contracts.Catalog;

public record CatalogItemResponse(
    Guid TestUid,
    string Title,
    string? Description,
    Guid AuthorUid,
    string AuthorName,
    string? Subject,
    int QuestionCount,
    int TotalAttempts,
    decimal PassRate,
    decimal AvgScore,
    DateTimeOffset UpdatedAt);