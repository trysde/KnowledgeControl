using System.Text.Json.Serialization;

namespace TestTakingService.Contracts.Snapshots;

public record AnswerOptionGradingSnapshot(
    Guid Uid,
    string Text,
    bool? IsCorrect,
    int Position);

public record QuestionGradingSnapshot(
    Guid QuestionUid,
    string Title,
    string Text,
    [property: JsonPropertyName("type")]
    string QuestionType,
    int OrderIndex,
    int Points,
    [property: JsonPropertyName("correctText")]
    string? CorrectTextAnswer,
    IReadOnlyList<AnswerOptionGradingSnapshot> Options);

public record TestGradingSnapshot(
    Guid TestUid,
    Guid AuthorUid,
    string Title,
    string? Description,
    int? TimeLimitSeconds,
    int? MaxAttempts,
    int PassingScore,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string Status,
    IReadOnlyList<QuestionGradingSnapshot> Questions);