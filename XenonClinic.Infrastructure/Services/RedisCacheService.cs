using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Redis-based distributed cache service implementation
/// Supports both Redis and in-memory cache (for development/fallback)
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheExpirationSettings _expirationSettings;

    // Static accessors for backward compatibility - use instance properties when possible
    public static TimeSpan DefaultExpiration => TimeSpan.FromMinutes(30);
    public static TimeSpan ShortExpiration => TimeSpan.FromMinutes(5);
    public static TimeSpan LongExpiration => TimeSpan.FromHours(2);
    public static TimeSpan VeryLongExpiration => TimeSpan.FromHours(24);

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger,
        CacheExpirationSettings? expirationSettings = null)
    {
        _cache = cache;
        _logger = logger;
        _expirationSettings = expirationSettings ?? new CacheExpirationSettings();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Gets the configured default expiration time.
    /// </summary>
    public TimeSpan ConfiguredDefaultExpiration => _expirationSettings.DefaultExpirationMinutes > 0
        ? TimeSpan.FromMinutes(_expirationSettings.DefaultExpirationMinutes)
        : DefaultExpiration;

    /// <summary>
    /// Gets the configured short expiration time.
    /// </summary>
    public TimeSpan ConfiguredShortExpiration => _expirationSettings.ShortExpirationMinutes > 0
        ? TimeSpan.FromMinutes(_expirationSettings.ShortExpirationMinutes)
        : ShortExpiration;

    /// <summary>
    /// Gets the configured long expiration time.
    /// </summary>
    public TimeSpan ConfiguredLongExpiration => _expirationSettings.LongExpirationHours > 0
        ? TimeSpan.FromHours(_expirationSettings.LongExpirationHours)
        : LongExpiration;

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
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Cache operation cancelled for key: {Key}", key);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON deserialization error for cache key: {Key}", key);
            return default;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation retrieving cache key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving cache key: {Key}", key);
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
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Cache set operation cancelled for key: {Key}", key);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON serialization error for cache key: {Key}", key);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation setting cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting cache key: {Key}", key);
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
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Cache remove operation cancelled for key: {Key}", key);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation removing cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error removing cache key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove all cached items matching a pattern
    /// Note: This requires Redis SCAN command support
    /// For in-memory cache, this is a no-op
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // For production Redis, you would use SCAN to find and delete keys
        _logger.LogDebug("RemoveByPattern called for pattern: {Pattern}", pattern);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get or create a cached item - if not exists, create using factory
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
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
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Cache exists check cancelled for key: {Key}", key);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation checking cache key existence: {Key}", key);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking cache key existence: {Key}", key);
            return false;
        }
    }
}

/// <summary>
/// Cache key builder for consistent key generation with Redis prefix
/// </summary>
public static class RedisCacheKeyBuilder
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
        $"{Prefix}:{typeof(T).Name.ToLowerInvariant()}:branch:{branchId}:page:{page}:size:{pageSize}";

    // Auth keys
    public static string AuthConfig(int companyId) => $"{Prefix}:auth:company:{companyId}";
    public static string IdentityProviders(int companyId) => $"{Prefix}:auth:company:{companyId}:providers";
}

/// <summary>
/// Redis configuration options.
/// ConnectionString must be configured in appsettings - no default provided for security.
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Redis connection string. Must be configured via appsettings.json.
    /// Example: "localhost:6379" for development, "redis-server:6379,password=xxx" for production.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "XenonClinic_";
    public bool Enabled { get; set; } = true;
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    public void Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException(
                "Redis ConnectionString must be configured when Redis caching is enabled. " +
                "Set 'Redis:ConnectionString' in appsettings.json or disable Redis with 'Redis:Enabled': false.");
        }
    }
}

/// <summary>
/// Configurable cache expiration settings.
/// All durations can be configured via appsettings.json under "Cache:Expiration".
/// </summary>
public class CacheExpirationSettings
{
    /// <summary>
    /// Default cache expiration in minutes. Default: 30 minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Short cache expiration in minutes. Default: 5 minutes.
    /// Used for frequently changing data.
    /// </summary>
    public int ShortExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Long cache expiration in hours. Default: 2 hours.
    /// Used for relatively static data.
    /// </summary>
    public int LongExpirationHours { get; set; } = 2;

    /// <summary>
    /// Very long cache expiration in hours. Default: 24 hours.
    /// Used for rarely changing configuration data.
    /// </summary>
    public int VeryLongExpirationHours { get; set; } = 24;
}
