using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// In-memory implementation of the workflow cache service.
/// For production, consider using distributed cache like Redis.
/// </summary>
public class InMemoryWorkflowCacheService : IWorkflowCacheService, IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _tagIndex = new();
    private readonly ILogger<InMemoryWorkflowCacheService> _logger;
    private readonly Timer _cleanupTimer;
    private readonly SemaphoreSlim _cleanupLock = new(1, 1);

    // Statistics
    private long _hits;
    private long _misses;
    private long _evictions;

    // Configuration
    private readonly int _maxEntries;
    private readonly TimeSpan _defaultTtl;

    public InMemoryWorkflowCacheService(ILogger<InMemoryWorkflowCacheService> logger)
    {
        _logger = logger;
        _maxEntries = 10000;
        _defaultTtl = TimeSpan.FromMinutes(30);

        // Start cleanup timer (every minute)
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                Interlocked.Increment(ref _hits);
                entry.LastAccessed = DateTime.UtcNow;
                entry.AccessCount++;

                // Update sliding expiration if set
                if (entry.SlidingExpiration.HasValue)
                {
                    entry.AbsoluteExpiration = DateTime.UtcNow.Add(entry.SlidingExpiration.Value);
                }

                return Task.FromResult(entry.Value as T);
            }
            else
            {
                // Remove expired entry
                _cache.TryRemove(key, out _);
                RemoveFromTagIndex(key, entry.Tags);
            }
        }

        Interlocked.Increment(ref _misses);
        return Task.FromResult<T?>(null);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, options, cancellationToken);
        return value;
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        options ??= new CacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl };

        // Check capacity and evict if needed
        EnsureCapacity();

        var entry = new CacheEntry
        {
            Key = key,
            Value = value,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow,
            Priority = options.Priority,
            Tags = options.Tags,
            SlidingExpiration = options.SlidingExpiration
        };

        // Calculate absolute expiration
        if (options.AbsoluteExpiration.HasValue)
        {
            entry.AbsoluteExpiration = options.AbsoluteExpiration.Value;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            entry.AbsoluteExpiration = DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }
        else if (options.SlidingExpiration.HasValue)
        {
            entry.AbsoluteExpiration = DateTime.UtcNow.Add(options.SlidingExpiration.Value);
        }
        else
        {
            entry.AbsoluteExpiration = DateTime.UtcNow.Add(_defaultTtl);
        }

        // Add to cache
        _cache[key] = entry;

        // Update tag index
        AddToTagIndex(key, entry.Tags);

        _logger.LogDebug("Cached item {Key}, expires at {Expiration}", key, entry.AbsoluteExpiration);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryRemove(key, out var entry))
        {
            RemoveFromTagIndex(key, entry.Tags);
            _logger.LogDebug("Removed cached item {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$", RegexOptions.Compiled);
        var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (var key in keysToRemove)
        {
            if (_cache.TryRemove(key, out var entry))
            {
                RemoveFromTagIndex(key, entry.Tags);
            }
        }

        _logger.LogDebug("Removed {Count} cached items matching pattern {Pattern}", keysToRemove.Count, pattern);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            return Task.FromResult(!entry.IsExpired);
        }
        return Task.FromResult(false);
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            result[key] = await GetAsync<T>(key, cancellationToken);
        }
        return result;
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        foreach (var item in items)
        {
            await SetAsync(item.Key, item.Value, options, cancellationToken);
        }
    }

    public Task InvalidateEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        var pattern = $"wf:{entityType.ToLowerInvariant()}:{entityId}*";
        return RemoveByPatternAsync(pattern, cancellationToken);
    }

    public async Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // Remove all entries tagged with this tenant
        var tag = CacheTags.ForTenant(tenantId);
        if (_tagIndex.TryGetValue(tag, out var keys))
        {
            foreach (var key in keys.ToList())
            {
                await RemoveAsync(key, cancellationToken);
            }
        }

        _logger.LogInformation("Invalidated cache for tenant {TenantId}", tenantId);
    }

    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var validEntries = _cache.Values.Where(e => !e.IsExpired).ToList();
        var stats = new CacheStatistics
        {
            TotalEntries = validEntries.Count,
            TotalHits = _hits,
            TotalMisses = _misses,
            EvictedEntries = _evictions,
            MemoryUsedBytes = EstimateMemoryUsage(),
            OldestEntry = validEntries.MinBy(e => e.CreatedAt)?.CreatedAt,
            EntriesByTag = GetEntriesByTag()
        };

        return Task.FromResult(stats);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        _tagIndex.Clear();
        _logger.LogInformation("Cache cleared");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _cleanupLock?.Dispose();
    }

    #region Private Methods

    private void EnsureCapacity()
    {
        if (_cache.Count >= _maxEntries)
        {
            // Evict least recently used entries
            var entriesToEvict = _cache.Values
                .Where(e => e.Priority != CachePriority.NeverRemove)
                .OrderBy(e => e.Priority)
                .ThenBy(e => e.LastAccessed)
                .Take(_maxEntries / 10) // Evict 10%
                .ToList();

            foreach (var entry in entriesToEvict)
            {
                if (_cache.TryRemove(entry.Key, out _))
                {
                    RemoveFromTagIndex(entry.Key, entry.Tags);
                    Interlocked.Increment(ref _evictions);
                }
            }

            _logger.LogDebug("Evicted {Count} cache entries", entriesToEvict.Count);
        }
    }

    private void AddToTagIndex(string key, List<string> tags)
    {
        foreach (var tag in tags)
        {
            var keys = _tagIndex.GetOrAdd(tag, _ => new HashSet<string>());
            lock (keys)
            {
                keys.Add(key);
            }
        }
    }

    private void RemoveFromTagIndex(string key, List<string> tags)
    {
        foreach (var tag in tags)
        {
            if (_tagIndex.TryGetValue(tag, out var keys))
            {
                lock (keys)
                {
                    keys.Remove(key);
                }
            }
        }
    }

    private void CleanupExpiredEntries(object? state)
    {
        if (!_cleanupLock.Wait(0))
            return;

        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                if (_cache.TryRemove(key, out var entry))
                {
                    RemoveFromTagIndex(key, entry.Tags);
                }
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
            }
        }
        finally
        {
            _cleanupLock.Release();
        }
    }

    private long EstimateMemoryUsage()
    {
        // Rough estimate - in production use proper memory tracking
        long total = 0;
        foreach (var entry in _cache.Values)
        {
            total += entry.Key.Length * 2; // String chars
            if (entry.Value is string s)
            {
                total += s.Length * 2;
            }
            else
            {
                // Estimate based on JSON serialization
                try
                {
                    var json = JsonSerializer.Serialize(entry.Value);
                    total += json.Length * 2;
                }
                catch
                {
                    total += 1000; // Default estimate
                }
            }
        }
        return total;
    }

    private Dictionary<string, long> GetEntriesByTag()
    {
        var result = new Dictionary<string, long>();
        foreach (var kvp in _tagIndex)
        {
            result[kvp.Key] = kvp.Value.Count;
        }
        return result;
    }

    #endregion

    private class CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public CachePriority Priority { get; set; }
        public List<string> Tags { get; set; } = new();
        public int AccessCount { get; set; }

        public bool IsExpired => AbsoluteExpiration.HasValue && AbsoluteExpiration.Value < DateTime.UtcNow;
    }
}

/// <summary>
/// Decorator that adds caching to process definition service.
/// </summary>
public class CachedProcessDefinitionService : IProcessDefinitionService
{
    private readonly IProcessDefinitionService _inner;
    private readonly IWorkflowCacheService _cache;
    private readonly ILogger<CachedProcessDefinitionService> _logger;

    public CachedProcessDefinitionService(
        IProcessDefinitionService inner,
        IWorkflowCacheService cache,
        ILogger<CachedProcessDefinitionService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Domain.Entities.ProcessDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ProcessDefinition(id);
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var result = await _inner.GetByIdAsync(id, cancellationToken);
            return result!;
        }, CacheEntryOptions.MediumTerm, cancellationToken);
    }

    public async Task<Domain.Entities.ProcessDefinition?> GetByKeyAsync(string key, int? version = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ProcessDefinitionByKey(key, version);
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var result = await _inner.GetByKeyAsync(key, version, cancellationToken);
            return result!;
        }, CacheEntryOptions.MediumTerm, cancellationToken);
    }

    public async Task<Domain.Entities.ProcessDefinition> CreateAsync(CreateProcessDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _inner.CreateAsync(request, cancellationToken);

        // Invalidate list cache
        await _cache.RemoveByPatternAsync(CacheKeys.ProcessDefinitionPattern, cancellationToken);

        return result;
    }

    public async Task<Domain.Entities.ProcessDefinition> UpdateAsync(string id, UpdateProcessDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _inner.UpdateAsync(id, request, cancellationToken);

        // Invalidate specific and list caches
        await _cache.RemoveAsync(CacheKeys.ProcessDefinition(id), cancellationToken);
        await _cache.RemoveByPatternAsync($"{CacheKeys.ProcessDefinitionPattern}key:{result.Key}:*", cancellationToken);

        return result;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        await _cache.InvalidateEntityAsync("pd", id, cancellationToken);
    }

    public async Task<Domain.Entities.ProcessDefinition> DeployAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _inner.DeployAsync(id, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.ProcessDefinition(id), cancellationToken);
        return result;
    }

    public Task<ProcessDefinitionListResult> ListAsync(ProcessDefinitionListQuery query, CancellationToken cancellationToken = default)
    {
        // Don't cache list queries as they vary too much
        return _inner.ListAsync(query, cancellationToken);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return _inner.ExistsAsync(key, cancellationToken);
    }
}
