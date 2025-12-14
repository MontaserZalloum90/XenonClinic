using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Health;

/// <summary>
/// Custom health check for API status.
/// </summary>
public class ApiHealthCheck : IHealthCheck
{
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _startTime;

        var data = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow },
            { "version", GetType().Assembly.GetName().Version?.ToString() ?? "unknown" },
            { "uptime", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m" },
            { "memoryMB", process.WorkingSet64 / 1024 / 1024 },
            { "threadCount", process.Threads.Count },
            { "gcGen0", GC.CollectionCount(0) },
            { "gcGen1", GC.CollectionCount(1) },
            { "gcGen2", GC.CollectionCount(2) }
        };

        return Task.FromResult(HealthCheckResult.Healthy("API is running", data));
    }
}

/// <summary>
/// Health check for cache service availability.
/// </summary>
public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cacheService;

    public CacheHealthCheck(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow }
        };

        try
        {
            var testKey = $"health_check_{Guid.NewGuid():N}";
            var testValue = DateTime.UtcNow.Ticks;

            // Test write
            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));

            // Test read
            var retrieved = await _cacheService.GetAsync<long>(testKey);

            // Cleanup
            await _cacheService.RemoveAsync(testKey);

            if (retrieved != testValue)
            {
                return HealthCheckResult.Degraded("Cache read/write mismatch", data: data);
            }

            data["status"] = "operational";
            return HealthCheckResult.Healthy("Cache is healthy", data);
        }
        catch (Exception ex)
        {
            data["error"] = ex.Message;
            return HealthCheckResult.Unhealthy("Cache health check failed", ex, data);
        }
    }
}

/// <summary>
/// Health check for database connectivity with additional diagnostics.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly Data.ClinicDbContext _context;

    public DatabaseHealthCheck(Data.ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connectivity
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            var data = new Dictionary<string, object>
            {
                { "provider", _context.Database.ProviderName ?? "unknown" },
                { "timestamp", DateTime.UtcNow }
            };

            return HealthCheckResult.Healthy("Database connection is healthy", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}
