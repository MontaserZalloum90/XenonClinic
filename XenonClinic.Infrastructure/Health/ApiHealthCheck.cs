using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XenonClinic.Infrastructure.Health;

/// <summary>
/// Custom health check for API status.
/// </summary>
public class ApiHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Add any custom health check logic here
        // For example: check external service connectivity, cache availability, etc.

        var data = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow },
            { "version", GetType().Assembly.GetName().Version?.ToString() ?? "unknown" }
        };

        return Task.FromResult(HealthCheckResult.Healthy("API is running", data));
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
