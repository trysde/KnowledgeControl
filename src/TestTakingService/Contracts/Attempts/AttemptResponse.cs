using TestTakingService.Contracts.Snapshots;

namespace TestTakingService.Contracts.Attempts;

public record SavedAnswerDto(
    Guid QuestionUid,
    IReadOnlyList<Guid> SelectedOptionUids,
    string? TextAnswer,
    DateTimeOffset AnsweredAt);

public record AttemptResponse(
    Guid AttemptUid,
    Guid TestUid,
    Guid UserUid,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset? ExpiresAt,
    TestSnapshotDto TestSnapshot,
    IReadOnlyList<SavedAnswerDto> Answers);