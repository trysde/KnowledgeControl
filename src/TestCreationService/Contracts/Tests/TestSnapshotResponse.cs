using System.Text.Json.Serialization;

namespace TestCreationService.Contracts.Tests;

public record AnswerOptionSnapshotResponse(
    Guid Uid,
    string Text,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsCorrect,
    int Position);

public record QuestionSnapshotResponse(
    Guid QuestionUid,
    string Title,
    string Text,
    [property: JsonPropertyName("type")]
    string QuestionType,
    int OrderIndex,
    int Points,
    [property: JsonPropertyName("correctText")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? CorrectTextAnswer,
    IReadOnlyList<AnswerOptionSnapshotResponse> Options);

public record TestSnapshotResponse(
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
    IReadOnlyList<QuestionSnapshotResponse> Questions);