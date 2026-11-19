namespace TestCreationService.Contracts.Tests;

public record TestResponse(
    Guid Uid,
    Guid AuthorUid,
    Guid CategoryUid,
    string CategoryName,
    string Title,
    string? Description,
    int? TimeLimitSeconds,
    int? MaxAttempts,
    int PassingScore,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool IsPublic,
    bool IsHidden,
    string Status,
    int QuestionsCount,
    int TotalPoints,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);