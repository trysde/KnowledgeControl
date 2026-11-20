namespace TestCreationService.Contracts.Tests;

public record TestQuestionItemResponse(Guid QuestionUid, int OrderIndex, int Points);

public record TestDetailResponse(
    Guid Uid,
    Guid CategoryUid,
    string Title,
    string? Description,
    int? TimeLimitSeconds,
    int? MaxAttempts,
    int PassingScore,
    bool IsPublic,
    string Status,
    IReadOnlyList<TestQuestionItemResponse> Questions);