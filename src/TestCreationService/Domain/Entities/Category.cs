using static System.Net.Mime.MediaTypeNames;

namespace TestCreationService.Domain.Entities;

public class Category
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}