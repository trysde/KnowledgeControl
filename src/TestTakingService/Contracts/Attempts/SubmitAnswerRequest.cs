namespace TestTakingService.Contracts.Attempts;

public record SubmitAnswerRequest
{
    public required Guid QuestionUid { get; init; }
    public IReadOnlyList<Guid> SelectedOptionUids { get; init; } = [];
    public string? TextAnswer { get; init; }
}