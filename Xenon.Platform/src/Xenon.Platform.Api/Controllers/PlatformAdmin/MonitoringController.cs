using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/monitoring")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class MonitoringController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly IHealthCheckService _healthService;

    public MonitoringController(PlatformDbContext context, IHealthCheckService healthService)
    {
        _context = context;
        _healthService = healthService;
    }

    /// <summary>
    /// Get platform and tenant health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var platformHealth = await _healthService.GetPlatformHealthAsync();

        // Get per-tenant health from recent checks
        var recentCutoff = DateTime.UtcNow.AddMinutes(-15);
        var tenantHealthSummary = await _context.TenantHealthChecks
            .Where(h => h.CheckedAt >= recentCutoff)
            .GroupBy(h => h.TenantId)
            .Select(g => g.OrderByDescending(h => h.CheckedAt).First())
            .Include(h => h.Tenant)
            .ToListAsync();

        var healthyTenants = tenantHealthSummary
            .Where(h => h.OverallStatus == HealthStatus.Healthy)
            .Select(h => new { h.Tenant.Slug, h.Tenant.Name, h.CheckedAt, h.DatabaseLatencyMs })
            .ToList();

        var degradedTenants = tenantHealthSummary
            .Where(h => h.OverallStatus == HealthStatus.Degraded)
            .Select(h => new { h.Tenant.Slug, h.Tenant.Name, h.CheckedAt, h.DatabaseLatencyMs })
            .ToList();

        var unhealthyTenants = tenantHealthSummary
            .Where(h => h.OverallStatus == HealthStatus.Unhealthy)
            .Select(h => new { h.Tenant.Slug, h.Tenant.Name, h.CheckedAt, h.DatabaseError })
            .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                platform = new
                {
                    status = platformHealth.OverallStatus.ToString(),
                    database = new
                    {
                        healthy = platformHealth.DatabaseHealthy,
                        latencyMs = platformHealth.DatabaseLatencyMs
                    },
                    checkedAt = platformHealth.CheckedAt
                },
                tenants = new
                {
                    total = platformHealth.TotalTenants,
                    healthy = platformHealth.HealthyTenants,
                    degraded = degradedTenants.Count,
                    unhealthy = platformHealth.UnhealthyTenants,
                    unknown = platformHealth.TotalTenants - tenantHealthSummary.Count,
                    details = new
                    {
                        healthy = healthyTenants,
                        degraded = degradedTenants,
                        unhealthy = unhealthyTenants
                    }
                }
            }
        });
    }

    /// <summary>
    /// Trigger health check for all tenants
    /// </summary>
    [HttpPost("health/refresh")]
    public async Task<IActionResult> RefreshHealth()
    {
        var results = await _healthService.CheckAllTenantsHealthAsync();

        var summary = new
        {
            checked = results.Count(),
            healthy = results.Count(r => r.OverallStatus == HealthStatus.Healthy),
            degraded = results.Count(r => r.OverallStatus == HealthStatus.Degraded),
            unhealthy = results.Count(r => r.OverallStatus == HealthStatus.Unhealthy)
        };

        return Ok(new
        {
            success = true,
            message = "Health check completed",
            data = summary
        });
    }

    /// <summary>
    /// Get audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] Guid? tenantId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action.Contains(action));
        }

        if (tenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == tenantId);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= endDate);
        }

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.PerformedByEmail,
                a.PerformedByRole,
                a.TenantId,
                a.IpAddress,
                a.IsSuccess,
                a.ErrorMessage,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                items = logs,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }
}
