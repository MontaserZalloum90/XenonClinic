using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Memory cache implementation of ICacheService.
/// Uses IMemoryCache under the hood with pattern-based key tracking.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (evictedKey, _, _, _) =>
                    {
                        _keys.TryRemove(evictedKey.ToString()!, out _);
                    }
                }
            }
        };

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);

        _logger.LogDebug("Cached value for key: {Key}, expiration: {Expiration}", key, expiration ?? DefaultExpiration);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? value) && value is not null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return value;
        }

        _logger.LogDebug("Cache miss for key: {Key}, creating value", key);
        value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);

        _logger.LogDebug("Removed cache entry for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _keys.Keys
            .Where(k => MatchesPattern(k, pattern))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RefreshAsync(string key, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out object? value) && value is not null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            _cache.Set(key, value, options);
            _logger.LogDebug("Refreshed cache entry for key: {Key}, new expiration: {Expiration}", key, expiration);
        }

        return Task.CompletedTask;
    }

    private static bool MatchesPattern(string key, string pattern)
    {
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            return key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*"))
        {
            var suffix = pattern[1..];
            return key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        return key.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
