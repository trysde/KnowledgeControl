namespace TestCreationService.Contracts.Questions;

public record AnswerOptionRequest
{
    public required string Text { get; init; }
    public required bool IsCorrect { get; init; }
    public required int Position { get; init; }
}