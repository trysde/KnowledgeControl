namespace TestTakingService.Contracts.Snapshots;

public record TestSnapshotDto(
    Guid TestUid,
    string Title,
    string? Description,
    int? TimeLimitSeconds,
    int PassingScore,
    IReadOnlyList<QuestionSnapshotDto> Questions);