using TestCreationService.Domain.Enums;

namespace TestCreationService.Domain.Entities;

public class Test
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public Guid AuthorUid { get; set; }
    public long CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public int? MaxAttempts { get; set; }
    public int PassingScore { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsHidden { get; set; }
    public TestStatus Status { get; set; } = TestStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}