using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SearchService.Services.Interfaces;

namespace SearchService.Infrastructure.Caching;

public class RedisSearchCache(IDistributedCache cache) : ISearchCache
{
    private const string VersionKey = "catalog:version";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return default;

        return JsonSerializer.Deserialize<T>(payload, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl
        };

        var payload = JsonSerializer.Serialize(value, JsonOptions);
        await cache.SetStringAsync(key, payload, options, cancellationToken);
    }

    public async Task<long> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var raw = await cache.GetStringAsync(VersionKey, cancellationToken);
        if (long.TryParse(raw, out var version))
            return version;

        await cache.SetStringAsync(VersionKey, "1", cancellationToken);
        return 1;
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        var current = await GetVersionAsync(cancellationToken);
        await cache.SetStringAsync(VersionKey, (current + 1).ToString(), cancellationToken);
    }
}