namespace TestTakingService.Domain.ValueObjects;

public sealed class AttemptSnapshot
{
    public Guid TestUid { get; set; }
    public Guid AuthorUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public int? MaxAttempts { get; set; }
    public int PassingScore { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public List<AttemptQuestionSnapshot> Questions { get; set; } = [];
}

public sealed class AttemptQuestionSnapshot
{
    public Guid QuestionUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public string? CorrectTextAnswer { get; set; }
    public List<AttemptOptionSnapshot> Options { get; set; } = [];
}

public sealed class AttemptOptionSnapshot
{
    public Guid Uid { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool? IsCorrect { get; set; }
    public int Position { get; set; }
}