namespace TestTakingService.Contracts.Attempts;

public record QuestionResultDto(
    Guid QuestionUid,
    string Title,
    string QuestionType,
    IReadOnlyList<Guid> SelectedOptionUids,
    string? TextAnswer,
    bool IsCorrect,
    decimal PointsEarned,
    decimal MaxPoints);

public record AttemptResultResponse(
    Guid ResultUid,
    Guid AttemptUid,
    Guid TestUid,
    Guid UserUid,
    decimal Score,
    decimal MaxScore,
    decimal Percent,
    int PassingScore,
    bool Passed,
    DateTimeOffset CompletedAt,
    IReadOnlyList<QuestionResultDto> QuestionResults);