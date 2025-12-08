using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Interface for distributed caching operations
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis-based distributed cache service implementation
/// Supports both Redis and in-memory cache (for development/fallback)
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // Default cache durations
    public static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan ShortExpiration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan LongExpiration = TimeSpan.FromHours(2);
    public static readonly TimeSpan VeryLongExpiration = TimeSpan.FromHours(24);

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Get a cached item by key
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cache key: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Set a cached item with optional expiration
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);

            _logger.LogDebug("Cache set for key: {Key}, Expiration: {Expiration}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove a cached item by key
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove all cached items with a given prefix
    /// Note: This requires Redis SCAN command support
    /// For in-memory cache, this is a no-op
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // For production Redis, you would use SCAN to find and delete keys
        _logger.LogDebug("RemoveByPrefix called for prefix: {Prefix}", prefix);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get or set a cached item - if not exists, create using factory
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetAsync(key, cancellationToken);
            return value != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }
}

/// <summary>
/// Cache key builder for consistent key generation
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "xenonclinic";

    // Tenant/Company/Branch keys
    public static string Tenant(int tenantId) => $"{Prefix}:tenant:{tenantId}";
    public static string TenantSettings(int tenantId) => $"{Prefix}:tenant:{tenantId}:settings";
    public static string Company(int companyId) => $"{Prefix}:company:{companyId}";
    public static string CompanySettings(int companyId) => $"{Prefix}:company:{companyId}:settings";
    public static string Branch(int branchId) => $"{Prefix}:branch:{branchId}";

    // User keys
    public static string User(string userId) => $"{Prefix}:user:{userId}";
    public static string UserBranches(string userId) => $"{Prefix}:user:{userId}:branches";
    public static string UserPermissions(string userId) => $"{Prefix}:user:{userId}:permissions";

    // Patient keys
    public static string Patient(int patientId) => $"{Prefix}:patient:{patientId}";
    public static string PatientsByBranch(int branchId) => $"{Prefix}:branch:{branchId}:patients";

    // Appointment keys
    public static string Appointment(int appointmentId) => $"{Prefix}:appointment:{appointmentId}";
    public static string AppointmentsByDate(int branchId, DateOnly date) => $"{Prefix}:branch:{branchId}:appointments:{date:yyyy-MM-dd}";

    // Lookup keys
    public static string Lookups(string lookupType, int? tenantId = null) =>
        tenantId.HasValue
            ? $"{Prefix}:lookups:{lookupType}:tenant:{tenantId}"
            : $"{Prefix}:lookups:{lookupType}:global";

    // License keys
    public static string License(string moduleCode) => $"{Prefix}:license:{moduleCode}";
    public static string LicenseValidation(string moduleCode) => $"{Prefix}:license:{moduleCode}:validation";

    // Module keys
    public static string ModuleConfig(string moduleName) => $"{Prefix}:module:{moduleName}:config";

    // Generic list keys
    public static string List<T>(int branchId, int page = 1, int pageSize = 20) =>
        $"{Prefix}:{typeof(T).Name.ToLower()}:branch:{branchId}:page:{page}:size:{pageSize}";

    // Auth keys
    public static string AuthConfig(int companyId) => $"{Prefix}:auth:company:{companyId}";
    public static string IdentityProviders(int companyId) => $"{Prefix}:auth:company:{companyId}:providers";
}

/// <summary>
/// Redis configuration options
/// </summary>
public class RedisCacheOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "XenonClinic_";
    public bool Enabled { get; set; } = true;
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;
}
