using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TripleTriad.Interfaces;

namespace TripleTriad.Caching;

public abstract class RedisCache<TValue> : ICache<TValue>
{
    private readonly IDistributedCache _cache;

    public abstract string KeyPrefix { get; }

    public RedisCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<TValue?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetAsync(KeyPrefix + key, cancellationToken);
        return json is not null
            ? JsonSerializer.Deserialize<TValue>(json)
            : default;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(KeyPrefix + key, cancellationToken);
    }

    public async Task SetAsync(string key, TValue value, CancellationToken cancellationToken = default)
    {
        await _cache.SetAsync(KeyPrefix + key, JsonSerializer.SerializeToUtf8Bytes(value), cancellationToken);
    }
}