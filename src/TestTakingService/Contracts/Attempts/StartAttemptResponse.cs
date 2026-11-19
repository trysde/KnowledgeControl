using TestTakingService.Contracts.Snapshots;

namespace TestTakingService.Contracts.Attempts;

public record StartAttemptResponse(
    Guid AttemptUid,
    Guid TestUid,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? ExpiresAt,
    TestSnapshotDto TestSnapshot);