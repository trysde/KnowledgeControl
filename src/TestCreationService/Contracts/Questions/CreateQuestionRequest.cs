namespace TestCreationService.Contracts.Questions;

public record CreateQuestionRequest
{
    public required Guid CategoryUid { get; init; }
    public required string Title { get; init; }
    public required string Text { get; init; }
    public required string QuestionType { get; init; }
    public string? CorrectTextAnswer { get; init; }
    public required string Visibility { get; init; }
    public IReadOnlyList<AnswerOptionRequest> AnswerOptions { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}