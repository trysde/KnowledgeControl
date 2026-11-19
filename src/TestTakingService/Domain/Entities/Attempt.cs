using TestTakingService.Domain.Enums;
using TestTakingService.Domain.ValueObjects;

namespace TestTakingService.Domain.Entities;

public class Attempt
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public Guid TestUid { get; set; }
    public Guid UserUid { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
    public AttemptSnapshot Snapshot { get; set; } = new();
    public int SnapshotVersion { get; set; } = 1;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public Result? Result { get; set; }
}