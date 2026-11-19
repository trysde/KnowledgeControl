namespace TestCreationService.Domain.Entities;

public class TestQuestion
{
    public long TestId { get; set; }
    public long QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; } = 1;
    public string Snapshot { get; set; } = "{}";

    public Test Test { get; set; } = null!;
    public Question Question { get; set; } = null!;
}