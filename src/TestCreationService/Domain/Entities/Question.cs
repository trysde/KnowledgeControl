using TestCreationService.Domain.Enums;

namespace TestCreationService.Domain.Entities;

public class Question
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public Guid AuthorUid { get; set; }
    public long CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string? CorrectTextAnswer { get; set; }
    public bool IsPublic { get; set; } = true;
    public int Version { get; set; } = 1;
    public bool IsDeleted { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}