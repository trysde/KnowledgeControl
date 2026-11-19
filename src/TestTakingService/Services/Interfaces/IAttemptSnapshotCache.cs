using TestTakingService.Domain.ValueObjects;

namespace TestTakingService.Services.Interfaces;

public interface IAttemptSnapshotCache
{
    Task SetAsync(Guid attemptUid, AttemptSnapshot snapshot, int? timeLimitSeconds, CancellationToken cancellationToken = default);
    Task<AttemptSnapshot?> GetAsync(Guid attemptUid, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid attemptUid, CancellationToken cancellationToken = default);
}