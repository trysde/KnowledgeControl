namespace TestCreationService.Contracts.Moderation;

public record ReportRequest
{
    public required string TargetType { get; init; }
    public required Guid TargetUid { get; init; }
    public required string Reason { get; init; }
}