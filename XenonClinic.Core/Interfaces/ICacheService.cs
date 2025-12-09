namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Abstraction for caching operations.
/// Provides a consistent caching interface across the application.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value or default if not found</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Gets a value from the cache, or creates it using the factory if not found.
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all values matching a key pattern.
    /// </summary>
    /// <param name="pattern">The key pattern (e.g., "user_*")</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>True if the key exists</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Refreshes the expiration time for a cached item.
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="expiration">New expiration time</param>
    Task RefreshAsync(string key, TimeSpan expiration);
}

/// <summary>
/// Cache key builder for consistent key naming.
/// </summary>
public static class CacheKeys
{
    public const string UserPrefix = "user";
    public const string TenantPrefix = "tenant";
    public const string CompanyPrefix = "company";
    public const string BranchPrefix = "branch";
    public const string SettingsPrefix = "settings";
    public const string LookupPrefix = "lookup";

    public static string User(string userId) => $"{UserPrefix}:{userId}";
    public static string UserBranches(string userId) => $"{UserPrefix}:{userId}:branches";
    public static string Tenant(int tenantId) => $"{TenantPrefix}:{tenantId}";
    public static string TenantSettings(int tenantId) => $"{TenantPrefix}:{tenantId}:settings";
    public static string Company(int companyId) => $"{CompanyPrefix}:{companyId}";
    public static string CompanySettings(int companyId) => $"{CompanyPrefix}:{companyId}:settings";
    public static string Branch(int branchId) => $"{BranchPrefix}:{branchId}";
    public static string BranchSettings(int branchId) => $"{BranchPrefix}:{branchId}:settings";
    public static string Lookup(string lookupType) => $"{LookupPrefix}:{lookupType}";
    public static string Lookup(string lookupType, int branchId) => $"{LookupPrefix}:{lookupType}:{branchId}";
}
