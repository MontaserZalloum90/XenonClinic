using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public interface IHealthCheckService
{
    Task<TenantHealthCheck> CheckTenantHealthAsync(Guid tenantId);
    Task<IEnumerable<TenantHealthCheck>> CheckAllTenantsHealthAsync();
    Task<PlatformHealthStatus> GetPlatformHealthAsync();
}

public record PlatformHealthStatus
{
    public HealthStatus OverallStatus { get; init; }
    public bool DatabaseHealthy { get; init; }
    public int? DatabaseLatencyMs { get; init; }
    public int TotalTenants { get; init; }
    public int HealthyTenants { get; init; }
    public int UnhealthyTenants { get; init; }
    public DateTime CheckedAt { get; init; }
}

public class HealthCheckService : IHealthCheckService
{
    private readonly PlatformDbContext _context;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(PlatformDbContext context, ILogger<HealthCheckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TenantHealthCheck> CheckTenantHealthAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        var healthCheck = new TenantHealthCheck
        {
            TenantId = tenantId,
            CheckedAt = DateTime.UtcNow
        };

        if (tenant == null)
        {
            healthCheck.DatabaseStatus = HealthStatus.Unknown;
            healthCheck.OverallStatus = HealthStatus.Unknown;
            healthCheck.DatabaseError = "Tenant not found";
            return healthCheck;
        }

        if (string.IsNullOrEmpty(tenant.DatabaseConnectionString))
        {
            healthCheck.DatabaseStatus = HealthStatus.Unknown;
            healthCheck.OverallStatus = HealthStatus.Unknown;
            healthCheck.DatabaseError = "Database not provisioned";
            return healthCheck;
        }

        try
        {
            var sw = Stopwatch.StartNew();
            await using var connection = new SqlConnection(tenant.DatabaseConnectionString);
            await connection.OpenAsync();

            // Simple query to test connectivity
            var cmd = new SqlCommand("SELECT 1", connection);
            await cmd.ExecuteScalarAsync();

            sw.Stop();

            healthCheck.DatabaseStatus = sw.ElapsedMilliseconds < 1000 ? HealthStatus.Healthy : HealthStatus.Degraded;
            healthCheck.DatabaseLatencyMs = (int)sw.ElapsedMilliseconds;
            healthCheck.OverallStatus = healthCheck.DatabaseStatus;
        }
        catch (Exception ex)
        {
            healthCheck.DatabaseStatus = HealthStatus.Unhealthy;
            healthCheck.OverallStatus = HealthStatus.Unhealthy;
            healthCheck.DatabaseError = ex.Message;
            _logger.LogWarning(ex, "Health check failed for tenant {TenantId}", tenantId);
        }

        // Save health check record
        _context.TenantHealthChecks.Add(healthCheck);
        await _context.SaveChangesAsync();

        return healthCheck;
    }

    public async Task<IEnumerable<TenantHealthCheck>> CheckAllTenantsHealthAsync()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsDatabaseProvisioned && t.Status != TenantStatus.Cancelled)
            .Select(t => t.Id)
            .ToListAsync();

        var results = new List<TenantHealthCheck>();

        foreach (var tenantId in tenants)
        {
            try
            {
                var result = await CheckTenantHealthAsync(tenantId);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check health for tenant {TenantId}", tenantId);
            }
        }

        return results;
    }

    public async Task<PlatformHealthStatus> GetPlatformHealthAsync()
    {
        var sw = Stopwatch.StartNew();
        var dbHealthy = true;
        int? dbLatency = null;

        try
        {
            // Test platform database
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            sw.Stop();
            dbLatency = (int)sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Platform database health check failed");
            dbHealthy = false;
        }

        // Get tenant health summary from recent checks (last 15 minutes)
        var recentCutoff = DateTime.UtcNow.AddMinutes(-15);
        var recentHealthChecks = await _context.TenantHealthChecks
            .Where(h => h.CheckedAt >= recentCutoff)
            .GroupBy(h => h.TenantId)
            .Select(g => g.OrderByDescending(h => h.CheckedAt).First())
            .ToListAsync();

        var totalTenants = await _context.Tenants.CountAsync(t => t.IsDatabaseProvisioned);
        var healthyTenants = recentHealthChecks.Count(h => h.OverallStatus == HealthStatus.Healthy);
        var unhealthyTenants = recentHealthChecks.Count(h => h.OverallStatus == HealthStatus.Unhealthy);

        var overallStatus = HealthStatus.Healthy;
        if (!dbHealthy)
            overallStatus = HealthStatus.Unhealthy;
        else if (unhealthyTenants > totalTenants * 0.1) // More than 10% unhealthy
            overallStatus = HealthStatus.Degraded;

        return new PlatformHealthStatus
        {
            OverallStatus = overallStatus,
            DatabaseHealthy = dbHealthy,
            DatabaseLatencyMs = dbLatency,
            TotalTenants = totalTenants,
            HealthyTenants = healthyTenants,
            UnhealthyTenants = unhealthyTenants,
            CheckedAt = DateTime.UtcNow
        };
    }
}
