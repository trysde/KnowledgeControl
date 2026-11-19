namespace TestTakingService.Contracts.Snapshots;

public record AnswerOptionSnapshotDto(
    Guid Uid,
    string Text,
    int Position);

public record QuestionSnapshotDto(
    Guid QuestionUid,
    string Title,
    string Text,
    string QuestionType,
    int OrderIndex,
    int Points,
    IReadOnlyList<AnswerOptionSnapshotDto> Options);