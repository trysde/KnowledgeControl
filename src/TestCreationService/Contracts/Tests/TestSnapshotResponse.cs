namespace TestCreationService.Contracts.Tests;

public record AnswerOptionSnapshotResponse(
    Guid Uid,
    string Text,
    bool IsCorrect,
    int Position);

public record QuestionSnapshotResponse(
    Guid QuestionUid,
    string Title,
    string Text,
    string QuestionType,
    int OrderIndex,
    int Points,
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