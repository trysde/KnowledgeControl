namespace TestCreationService.Domain.Entities;

public class QuestionTag
{
    public long QuestionId { get; set; }
    public long TagId { get; set; }

    public Question Question { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}