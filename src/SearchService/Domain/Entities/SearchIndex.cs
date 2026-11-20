using NpgsqlTypes;

namespace SearchService.Domain.Entities;

public class SearchIndex
{
    public int Id { get; set; }
    public Guid TestUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid AuthorUid { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = true;
    public int QuestionCount { get; set; }
    public NpgsqlTsVector SearchVector { get; set; } = null!;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}