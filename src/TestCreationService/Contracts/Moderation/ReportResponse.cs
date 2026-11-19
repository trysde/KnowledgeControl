namespace TestCreationService.Contracts.Moderation;

public record ReportResponse(
    Guid Uid,
    Guid ReporterUid,
    string TargetType,
    Guid TargetUid,
    string Reason,
    string Status,
    DateTimeOffset CreatedAt);