namespace TestCreationService.Domain.Entities;

public class Tag
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}