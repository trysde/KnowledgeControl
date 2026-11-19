namespace TestCreationService.Domain.Entities;

public class AnswerOption
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public long QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Position { get; set; }

    public Question Question { get; set; } = null!;
}