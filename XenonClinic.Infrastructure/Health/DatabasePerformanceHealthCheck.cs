using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Health;

/// <summary>
/// Health check that monitors database performance and connection pool health.
/// </summary>
public class DatabasePerformanceHealthCheck : IHealthCheck
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DatabasePerformanceHealthCheck> _logger;
    private readonly DatabaseOptions _options;

    // Performance thresholds
    private const int WarningQueryTimeMs = 500;
    private const int DegradedQueryTimeMs = 1000;
    private const int UnhealthyQueryTimeMs = 5000;

    public DatabasePerformanceHealthCheck(
        ClinicDbContext context,
        ILogger<DatabasePerformanceHealthCheck> logger,
        IOptions<DatabaseOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            data["connectivity"] = "ok";

            // Test query performance with a simple query
            var queryStopwatch = Stopwatch.StartNew();
            var testQuery = await _context.Database
                .SqlQuery<int>($"SELECT 1 AS Value")
                .FirstOrDefaultAsync(cancellationToken);
            queryStopwatch.Stop();

            var queryTimeMs = queryStopwatch.ElapsedMilliseconds;
            data["simpleQueryTimeMs"] = queryTimeMs;

            // Get provider info
            data["provider"] = _context.Database.ProviderName ?? "unknown";

            // Check if any pending migrations exist
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();
            data["pendingMigrations"] = pendingCount;

            if (pendingCount > 0)
            {
                data["pendingMigrationNames"] = string.Join(", ", pendingMigrations);
            }

            // Get applied migrations count
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
            data["appliedMigrations"] = appliedMigrations.Count();

            stopwatch.Stop();
            data["totalCheckTimeMs"] = stopwatch.ElapsedMilliseconds;

            // Determine health status based on query time
            if (queryTimeMs >= UnhealthyQueryTimeMs)
            {
                _logger.LogError("Database query time critically slow: {QueryTime}ms", queryTimeMs);
                return HealthCheckResult.Unhealthy(
                    $"Database query time critically slow: {queryTimeMs}ms",
                    data: data);
            }

            if (queryTimeMs >= DegradedQueryTimeMs || pendingCount > 0)
            {
                var message = queryTimeMs >= DegradedQueryTimeMs
                    ? $"Database query time degraded: {queryTimeMs}ms"
                    : $"Database has {pendingCount} pending migrations";

                _logger.LogWarning("Database health degraded: {Message}", message);
                return HealthCheckResult.Degraded(message, data: data);
            }

            if (queryTimeMs >= WarningQueryTimeMs)
            {
                _logger.LogInformation("Database query time elevated: {QueryTime}ms", queryTimeMs);
            }

            return HealthCheckResult.Healthy("Database is healthy", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            stopwatch.Stop();
            data["totalCheckTimeMs"] = stopwatch.ElapsedMilliseconds;
            data["error"] = ex.Message;

            return HealthCheckResult.Unhealthy("Database health check failed", ex, data);
        }
    }
}

/// <summary>
/// Health check that monitors connection pool metrics.
/// </summary>
public class ConnectionPoolHealthCheck : IHealthCheck
{
    private readonly ILogger<ConnectionPoolHealthCheck> _logger;
    private static int _activeConnections;
    private static int _totalRequests;
    private static int _failedRequests;

    public ConnectionPoolHealthCheck(ILogger<ConnectionPoolHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["activeConnections"] = _activeConnections,
            ["totalRequests"] = _totalRequests,
            ["failedRequests"] = _failedRequests,
            ["successRate"] = _totalRequests > 0
                ? $"{((_totalRequests - _failedRequests) * 100.0 / _totalRequests):F2}%"
                : "N/A"
        };

        // Calculate error rate
        if (_totalRequests > 100 && _failedRequests > _totalRequests * 0.1)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Connection pool error rate high: {_failedRequests}/{_totalRequests}",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Connection pool healthy", data: data));
    }

    /// <summary>
    /// Records a successful connection request.
    /// </summary>
    public static void RecordSuccess()
    {
        Interlocked.Increment(ref _totalRequests);
    }

    /// <summary>
    /// Records a failed connection request.
    /// </summary>
    public static void RecordFailure()
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _failedRequests);
    }

    /// <summary>
    /// Increments active connection count.
    /// </summary>
    public static void IncrementActive()
    {
        Interlocked.Increment(ref _activeConnections);
    }

    /// <summary>
    /// Decrements active connection count.
    /// </summary>
    public static void DecrementActive()
    {
        Interlocked.Decrement(ref _activeConnections);
    }

    /// <summary>
    /// Resets all counters (for testing).
    /// </summary>
    public static void Reset()
    {
        _activeConnections = 0;
        _totalRequests = 0;
        _failedRequests = 0;
    }
}
