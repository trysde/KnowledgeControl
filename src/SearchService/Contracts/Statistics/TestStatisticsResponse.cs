namespace SearchService.Contracts.Statistics;

public record TestStatisticsResponse(
    Guid TestUid,
    int TotalAttempts,
    int PassedAttempts,
    decimal PassRate,
    decimal AvgScore,
    int AvgDurationSec,
    DateTimeOffset? LastAttemptAt);