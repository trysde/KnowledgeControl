namespace TestCreationService.Contracts.Tests;

public record TestQuestionItemRequest
{
    public required Guid QuestionUid { get; init; }
    public required int OrderIndex { get; init; }
    public int Points { get; init; } = 1;
}