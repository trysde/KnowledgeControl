using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TestTakingService.Domain.ValueObjects;
using TestTakingService.Services.Interfaces;

namespace TestTakingService.Infrastructure.Caching;

public class RedisAttemptSnapshotCache(IDistributedCache cache) : IAttemptSnapshotCache
{
    private const int BufferSeconds = 600;
    private const int DefaultAttemptDurationSeconds = 24 * 60 * 60;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SetAsync(
        Guid attemptUid,
        AttemptSnapshot snapshot,
        int? timeLimitSeconds,
        CancellationToken cancellationToken = default)
    {
        var baseSeconds = timeLimitSeconds is > 0
            ? timeLimitSeconds.Value
            : DefaultAttemptDurationSeconds;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(baseSeconds + BufferSeconds)
        };

        var payload = JsonSerializer.Serialize(snapshot, JsonOptions);
        await cache.SetStringAsync(BuildKey(attemptUid), payload, options, cancellationToken);
    }

    public async Task<AttemptSnapshot?> GetAsync(
        Guid attemptUid,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildKey(attemptUid), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<AttemptSnapshot>(payload, JsonOptions);
    }

    public async Task RemoveAsync(
        Guid attemptUid,
        CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(BuildKey(attemptUid), cancellationToken);
    }

    private static string BuildKey(Guid attemptUid) => $"attempt:snapshot:{attemptUid}";
}