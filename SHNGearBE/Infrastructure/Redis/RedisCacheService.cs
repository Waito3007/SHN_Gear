using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SHNGearBE.Configurations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SHNGearBE.Infrastructure.Redis;

/// <summary>
/// Redis cache service implementation using IDistributedCache
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _instanceName;

    public RedisCacheService(IDistributedCache cache, IOptions<RedisConfiguration> config)
    {
        _cache = cache;
        _config = config.Value;
        _instanceName = _config.InstanceName;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Get value from cache by key
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        var cached = await _cache.GetStringAsync(fullKey);

        if (string.IsNullOrEmpty(cached))
            return default;

        return JsonSerializer.Deserialize<T>(cached, _jsonOptions);
    }

    /// <summary>
    /// Set value to cache with optional expiration
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var fullKey = GetFullKey(key);
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes)
        };

        await _cache.SetStringAsync(fullKey, serialized, options);
    }

    /// <summary>
    /// Remove value from cache by key
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        var fullKey = GetFullKey(key);
        await _cache.RemoveAsync(fullKey);
    }

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var cached = await _cache.GetStringAsync(fullKey);
        return !string.IsNullOrEmpty(cached);
    }

    /// <summary>
    /// Remove all keys matching pattern (requires StackExchange.Redis for pattern matching)
    /// Note: IDistributedCache doesn't support pattern-based deletion natively.
    /// For full pattern support, inject IConnectionMultiplexer directly.
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        // IDistributedCache doesn't support pattern-based removal natively
        // This is a placeholder - for full functionality, use StackExchange.Redis directly
        // For now, we'll just remove the exact key if it matches
        await RemoveAsync(pattern);
    }

    /// <summary>
    /// Get value from cache or create it using factory method
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);

        if (cached is not null)
            return cached;

        var value = await factory();

        if (value is not null)
            await SetAsync(key, value, expiration);

        return value;
    }

    /// <summary>
    /// Acquire a distributed lock
    /// </summary>
    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
    {
        var lockKey = GetFullKey($"lock:{key}");
        var lockValue = Guid.NewGuid().ToString();

        // Try to set the lock key only if it doesn't exist
        var existing = await _cache.GetStringAsync(lockKey);

        if (!string.IsNullOrEmpty(existing))
            return false;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        await _cache.SetStringAsync(lockKey, lockValue, options);
        return true;
    }

    /// <summary>
    /// Release a distributed lock
    /// </summary>
    public async Task ReleaseLockAsync(string key)
    {
        var lockKey = GetFullKey($"lock:{key}");
        await _cache.RemoveAsync(lockKey);
    }

    /// <summary>
    /// Get full cache key with instance prefix
    /// </summary>
    private string GetFullKey(string key) => $"{_instanceName}{key}";
}
