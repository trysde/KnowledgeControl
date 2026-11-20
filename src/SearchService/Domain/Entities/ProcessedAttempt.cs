namespace SearchService.Domain.Entities;

public class ProcessedAttempt
{
    public Guid AttemptUid { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}