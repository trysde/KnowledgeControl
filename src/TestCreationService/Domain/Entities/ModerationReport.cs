using TestCreationService.Domain.Enums;

namespace TestCreationService.Domain.Entities;

public class ModerationReport
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public Guid ReporterUid { get; set; }
    public ModerationTargetType TargetType { get; set; }
    public Guid TargetUid { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ModerationReportStatus Status { get; set; } = ModerationReportStatus.Pending;
    public Guid? ModeratorUid { get; set; }
    public string? ModeratorComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}