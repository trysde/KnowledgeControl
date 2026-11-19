namespace TestCreationService.Contracts.Tests;

public record CreateTestRequest
{
    public required Guid CategoryUid { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public int? TimeLimitSeconds { get; init; }
    public int? MaxAttempts { get; init; }
    public int PassingScore { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public bool IsPublic { get; init; } = true;
    public required IReadOnlyList<TestQuestionItemRequest> Questions { get; init; }
}