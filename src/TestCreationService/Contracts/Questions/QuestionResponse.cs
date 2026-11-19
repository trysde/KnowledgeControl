namespace TestCreationService.Contracts.Questions;

public record QuestionResponse(
    Guid Uid,
    Guid AuthorUid,
    Guid CategoryUid,
    string CategoryName,
    string Title,
    string Text,
    string QuestionType,
    string? CorrectTextAnswer,
    bool IsPublic,
    int Version,
    bool IsBlocked,
    IReadOnlyList<string> Tags,
    IReadOnlyList<AnswerOptionResponse> AnswerOptions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);