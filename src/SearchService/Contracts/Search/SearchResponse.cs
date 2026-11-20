namespace SearchService.Contracts.Search;

public record SearchResponse(
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
    float Rank,
    DateTimeOffset UpdatedAt);