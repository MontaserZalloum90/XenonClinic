using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XenonClinic.Web.Configuration;

/// <summary>
/// Health checks configuration for load balancers and monitoring
/// </summary>
public static class HealthChecksConfiguration
{
    /// <summary>
    /// Configure health check services
    /// </summary>
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // Database health check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecks.AddSqlServer(
                connectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "sqlserver" });
        }

        // Redis health check (if enabled)
        var redisConfig = configuration.GetSection("Redis");
        if (redisConfig.GetValue<bool>("Enabled"))
        {
            var redisConnection = redisConfig["ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection) && redisConnection != "localhost:6379")
            {
                healthChecks.AddRedis(
                    redisConnection,
                    name: "redis",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "cache", "redis" });
            }
        }

        // Memory health check
        healthChecks.AddCheck<MemoryHealthCheck>(
            "memory",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "memory" });

        // Application health check
        healthChecks.AddCheck<ApplicationHealthCheck>(
            "application",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "app", "self" });

        return services;
    }

    /// <summary>
    /// Configure health check endpoints
    /// </summary>
    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("app"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("self") || check.Tags.Contains("memory"),
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Memory health check to detect memory pressure
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var allocatedMb = allocated / 1024 / 1024;

        // Get memory info
        var gcInfo = GC.GetGCMemoryInfo();
        var totalAvailable = gcInfo.TotalAvailableMemoryBytes / 1024 / 1024;
        var highMemoryThreshold = gcInfo.HighMemoryLoadThresholdBytes / 1024 / 1024;

        var data = new Dictionary<string, object>
        {
            { "allocatedMb", allocatedMb },
            { "totalAvailableMb", totalAvailable },
            { "highMemoryThresholdMb", highMemoryThreshold },
            { "gen0Collections", GC.CollectionCount(0) },
            { "gen1Collections", GC.CollectionCount(1) },
            { "gen2Collections", GC.CollectionCount(2) }
        };

        // Check if memory usage is concerning
        var memoryUsagePercent = (double)allocated / gcInfo.TotalAvailableMemoryBytes * 100;

        if (memoryUsagePercent > 90)
        {
            _logger.LogWarning("High memory usage detected: {MemoryUsage}%", memoryUsagePercent);
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Memory usage is critical: {memoryUsagePercent:F1}%",
                data: data));
        }

        if (memoryUsagePercent > 75)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Memory usage is elevated: {memoryUsagePercent:F1}%",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory usage is healthy: {memoryUsagePercent:F1}%",
            data: data));
    }
}

/// <summary>
/// Application self-check
/// </summary>
public class ApplicationHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly DateTime StartTime = DateTime.UtcNow;

    public ApplicationHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - StartTime;

        var data = new Dictionary<string, object>
        {
            { "startTime", StartTime },
            { "uptime", uptime.ToString() },
            { "uptimeSeconds", uptime.TotalSeconds },
            { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" },
            { "machineName", Environment.MachineName },
            { "processId", Environment.ProcessId },
            { "threadCount", ThreadPool.ThreadCount }
        };

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Application running for {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m",
            data: data));
    }
}
