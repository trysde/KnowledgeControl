using TestTakingService.Contracts.Snapshots;

namespace TestTakingService.Services.Interfaces;

public interface ITestCreationClient
{
    Task<TestGradingSnapshot> GetTestSnapshotAsync(Guid testUid, CancellationToken cancellationToken = default);
}