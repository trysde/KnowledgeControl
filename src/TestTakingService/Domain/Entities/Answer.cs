namespace TestTakingService.Domain.Entities;

public class Answer
{
    public long Id { get; set; }
    public long AttemptId { get; set; }
    public Guid QuestionUid { get; set; }
    public List<Guid> SelectedOptionUids { get; set; } = [];
    public string? TextAnswer { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal? PointsEarned { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public Attempt Attempt { get; set; } = null!;
}