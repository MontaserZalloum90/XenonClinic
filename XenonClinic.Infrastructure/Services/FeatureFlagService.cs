using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Interface for feature flag operations
/// </summary>
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureKey, FeatureFlagContext? context = null, CancellationToken cancellationToken = default);
    Task<FeatureFlag?> GetFeatureFlagAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default);
    Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
    Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
    Task DeleteFeatureFlagAsync(string key, CancellationToken cancellationToken = default);
    Task<Dictionary<string, bool>> EvaluateAllFlagsAsync(FeatureFlagContext? context = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Feature flag service implementation with caching
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly ClinicDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly IWebHostEnvironment _environment;

    private const string CacheKeyPrefix = "featureflag:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public FeatureFlagService(
        ClinicDbContext context,
        IDistributedCache cache,
        ILogger<FeatureFlagService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Check if a feature is enabled for the given context
    /// </summary>
    public async Task<bool> IsEnabledAsync(
        string featureKey,
        FeatureFlagContext? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var flag = await GetFeatureFlagAsync(featureKey, cancellationToken);

            if (flag == null)
            {
                _logger.LogDebug("Feature flag not found: {Key}", featureKey);
                return false;
            }

            return EvaluateFlag(flag, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag: {Key}", featureKey);
            return false; // Fail closed - disable feature on error
        }
    }

    /// <summary>
    /// Get a specific feature flag
    /// </summary>
    public async Task<FeatureFlag?> GetFeatureFlagAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";

        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<FeatureFlag>(cached);
        }

        // Load from database
        var flag = await _context.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == key, cancellationToken);

        if (flag != null)
        {
            // Cache the result
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(flag),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
                cancellationToken);
        }

        return flag;
    }

    /// <summary>
    /// Get all feature flags
    /// </summary>
    public async Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .AsNoTracking()
            .OrderBy(f => f.Key)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create a new feature flag
    /// </summary>
    public async Task<FeatureFlag> CreateFeatureFlagAsync(
        FeatureFlag flag,
        CancellationToken cancellationToken = default)
    {
        flag.CreatedAt = DateTime.UtcNow;
        _context.FeatureFlags.Add(flag);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feature flag created: {Key}", flag.Key);
        return flag;
    }

    /// <summary>
    /// Update an existing feature flag
    /// </summary>
    public async Task<FeatureFlag> UpdateFeatureFlagAsync(
        FeatureFlag flag,
        CancellationToken cancellationToken = default)
    {
        flag.UpdatedAt = DateTime.UtcNow;
        _context.FeatureFlags.Update(flag);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"{CacheKeyPrefix}{flag.Key}", cancellationToken);

        _logger.LogInformation("Feature flag updated: {Key}", flag.Key);
        return flag;
    }

    /// <summary>
    /// Delete a feature flag
    /// </summary>
    public async Task DeleteFeatureFlagAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var flag = await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Key == key, cancellationToken);
        if (flag != null)
        {
            _context.FeatureFlags.Remove(flag);
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync($"{CacheKeyPrefix}{key}", cancellationToken);

            _logger.LogInformation("Feature flag deleted: {Key}", key);
        }
    }

    /// <summary>
    /// Evaluate all flags for a given context
    /// </summary>
    public async Task<Dictionary<string, bool>> EvaluateAllFlagsAsync(
        FeatureFlagContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var flags = await GetAllFeatureFlagsAsync(cancellationToken);
        var result = new Dictionary<string, bool>();

        foreach (var flag in flags)
        {
            result[flag.Key] = EvaluateFlag(flag, context);
        }

        return result;
    }

    /// <summary>
    /// Evaluate a single feature flag against a context
    /// </summary>
    private bool EvaluateFlag(FeatureFlag flag, FeatureFlagContext? context)
    {
        // Check if globally disabled
        if (!flag.IsEnabled)
            return false;

        // Check date range
        var now = DateTime.UtcNow;
        if (flag.StartDate.HasValue && now < flag.StartDate.Value)
            return false;

        if (flag.EndDate.HasValue && now > flag.EndDate.Value)
            return false;

        // Check environment
        if (!string.IsNullOrEmpty(flag.Environment))
        {
            var currentEnv = context?.Environment ?? _environment.EnvironmentName;
            if (!flag.Environment.Equals(currentEnv, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // If no context, use global flag status
        if (context == null)
            return flag.RolloutPercentage >= 100;

        // Check user-specific enablement
        if (!string.IsNullOrEmpty(flag.EnabledUserIds) && !string.IsNullOrEmpty(context.UserId))
        {
            var enabledUsers = flag.EnabledUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (enabledUsers.Contains(context.UserId, StringComparer.OrdinalIgnoreCase))
                return true;
        }

        // Check role-specific enablement
        if (!string.IsNullOrEmpty(flag.EnabledRoles) && context.Roles?.Any() == true)
        {
            var enabledRoles = flag.EnabledRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (enabledRoles.Intersect(context.Roles, StringComparer.OrdinalIgnoreCase).Any())
                return true;
        }

        // Check tenant-specific enablement
        if (!string.IsNullOrEmpty(flag.EnabledTenantIds) && context.TenantId.HasValue)
        {
            var enabledTenants = flag.EnabledTenantIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (enabledTenants.Contains(context.TenantId.Value.ToString()))
                return true;
        }

        // Check company-specific enablement
        if (!string.IsNullOrEmpty(flag.EnabledCompanyIds) && context.CompanyId.HasValue)
        {
            var enabledCompanies = flag.EnabledCompanyIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (enabledCompanies.Contains(context.CompanyId.Value.ToString()))
                return true;
        }

        // Percentage rollout
        if (flag.RolloutPercentage < 100 && !string.IsNullOrEmpty(context.UserId))
        {
            return IsInRolloutPercentage(flag.Key, context.UserId, flag.RolloutPercentage);
        }

        return flag.RolloutPercentage >= 100;
    }

    /// <summary>
    /// Deterministic percentage check based on user ID
    /// </summary>
    private static bool IsInRolloutPercentage(string featureKey, string userId, int percentage)
    {
        // Create a deterministic hash for consistent behavior per user per feature
        var input = $"{featureKey}:{userId}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var value = Math.Abs(BitConverter.ToInt32(hash, 0)) % 100;
        return value < percentage;
    }
}
