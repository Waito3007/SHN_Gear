namespace SHNGearBE.Infrastructure.Redis;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task RemoveByPatternAsync(string pattern);

    // Convenience helpers
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task<bool> AcquireLockAsync(string key, TimeSpan expiry);
    Task ReleaseLockAsync(string key);
}
