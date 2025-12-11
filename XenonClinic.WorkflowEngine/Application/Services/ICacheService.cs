using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Caching service for workflow engine performance optimization.
/// </summary>
public interface IWorkflowCacheService
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a value from the cache or creates it using the factory.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all values matching a pattern.
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values from the cache.
    /// </summary>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets multiple values in the cache.
    /// </summary>
    Task SetManyAsync<T>(IDictionary<string, T> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invalidates cache for a specific entity.
    /// </summary>
    Task InvalidateEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache for a tenant.
    /// </summary>
    Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

#region Cache DTOs

public class CacheEntryOptions
{
    /// <summary>
    /// Absolute expiration time.
    /// </summary>
    public DateTime? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Absolute expiration relative to now.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Sliding expiration - resets on each access.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Cache priority for eviction.
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;

    /// <summary>
    /// Tags for grouping cache entries.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Creates options with specified TTL.
    /// </summary>
    public static CacheEntryOptions WithTtl(TimeSpan ttl) => new()
    {
        AbsoluteExpirationRelativeToNow = ttl
    };

    /// <summary>
    /// Creates options with sliding expiration.
    /// </summary>
    public static CacheEntryOptions WithSliding(TimeSpan sliding) => new()
    {
        SlidingExpiration = sliding
    };

    /// <summary>
    /// Creates options for short-term caching (5 minutes).
    /// </summary>
    public static CacheEntryOptions ShortTerm => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    /// <summary>
    /// Creates options for medium-term caching (30 minutes).
    /// </summary>
    public static CacheEntryOptions MediumTerm => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    /// <summary>
    /// Creates options for long-term caching (4 hours).
    /// </summary>
    public static CacheEntryOptions LongTerm => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4)
    };
}

public enum CachePriority
{
    Low,
    Normal,
    High,
    NeverRemove
}

public class CacheStatistics
{
    public long TotalEntries { get; set; }
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRate => TotalHits + TotalMisses > 0 ? (double)TotalHits / (TotalHits + TotalMisses) : 0;
    public long MemoryUsedBytes { get; set; }
    public long EvictedEntries { get; set; }
    public DateTime? OldestEntry { get; set; }
    public Dictionary<string, long> EntriesByTag { get; set; } = new();
}

#endregion

#region Cache Key Builder

/// <summary>
/// Helper for building consistent cache keys.
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "wf:";

    // Process Definition keys
    public static string ProcessDefinition(string definitionId) => $"{Prefix}pd:{definitionId}";
    public static string ProcessDefinitionByKey(string key, int? version = null) =>
        version.HasValue ? $"{Prefix}pd:key:{key}:v{version}" : $"{Prefix}pd:key:{key}:latest";
    public static string ProcessDefinitionList(string tenantId) => $"{Prefix}pd:list:{tenantId}";

    // Process Instance keys
    public static string ProcessInstance(string instanceId) => $"{Prefix}pi:{instanceId}";
    public static string ProcessInstanceVariables(string instanceId) => $"{Prefix}pi:{instanceId}:vars";
    public static string ProcessInstanceTasks(string instanceId) => $"{Prefix}pi:{instanceId}:tasks";

    // Task keys
    public static string Task(string taskId) => $"{Prefix}task:{taskId}";
    public static string UserTasks(string userId) => $"{Prefix}user:{userId}:tasks";
    public static string GroupTasks(string groupId) => $"{Prefix}group:{groupId}:tasks";

    // User/Authorization keys
    public static string UserRoles(string userId) => $"{Prefix}user:{userId}:roles";
    public static string UserPermissions(string userId, string resourceType) => $"{Prefix}user:{userId}:perms:{resourceType}";
    public static string Role(string roleId) => $"{Prefix}role:{roleId}";

    // Tenant keys
    public static string Tenant(string tenantId) => $"{Prefix}tenant:{tenantId}";
    public static string TenantSettings(string tenantId) => $"{Prefix}tenant:{tenantId}:settings";

    // Form/Template keys
    public static string FormDefinition(string formId) => $"{Prefix}form:{formId}";
    public static string DocumentTemplate(string templateId) => $"{Prefix}doctemplate:{templateId}";
    public static string EmailTemplate(string templateId) => $"{Prefix}emailtemplate:{templateId}";

    // Statistics keys
    public static string ProcessStatistics(string definitionId) => $"{Prefix}stats:pd:{definitionId}";
    public static string TenantStatistics(string tenantId) => $"{Prefix}stats:tenant:{tenantId}";

    // Pattern helpers
    public static string ProcessDefinitionPattern => $"{Prefix}pd:*";
    public static string ProcessInstancePattern => $"{Prefix}pi:*";
    public static string TenantPattern(string tenantId) => $"{Prefix}*:{tenantId}:*";
    public static string UserPattern(string userId) => $"{Prefix}user:{userId}:*";
}

#endregion

#region Cache Tags

/// <summary>
/// Standard cache tags for grouping entries.
/// </summary>
public static class CacheTags
{
    public const string ProcessDefinitions = "process-definitions";
    public const string ProcessInstances = "process-instances";
    public const string Tasks = "tasks";
    public const string Users = "users";
    public const string Roles = "roles";
    public const string Tenants = "tenants";
    public const string Forms = "forms";
    public const string Templates = "templates";
    public const string Statistics = "statistics";

    public static string ForTenant(string tenantId) => $"tenant:{tenantId}";
    public static string ForProcess(string processId) => $"process:{processId}";
    public static string ForUser(string userId) => $"user:{userId}";
}

#endregion
