namespace TestCreationService.Contracts.Questions;

public record AnswerOptionResponse(
    Guid Uid,
    string Text,
    bool IsCorrect,
    int Position);