using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/dashboard")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class DashboardController : ControllerBase
{
    private readonly PlatformDbContext _context;

    public DashboardController(PlatformDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get dashboard KPIs
    /// </summary>
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sevenDaysAgo = now.AddDays(-7);

        // Tenant counts
        var totalTenants = await _context.Tenants.CountAsync();
        var trialTenants = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Trial);
        var activeTenants = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Active);
        var suspendedTenants = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Suspended);
        var expiredTenants = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Expired);

        // New signups
        var newTenantsLast30Days = await _context.Tenants
            .CountAsync(t => t.CreatedAt >= thirtyDaysAgo);
        var newTenantsLast7Days = await _context.Tenants
            .CountAsync(t => t.CreatedAt >= sevenDaysAgo);

        // Trial conversions (placeholder - would need more logic)
        var trialsExpiringSoon = await _context.Tenants
            .CountAsync(t => t.Status == TenantStatus.Trial
                && t.TrialEndDate <= now.AddDays(7)
                && t.TrialEndDate > now);

        // Subscription revenue (monthly recurring)
        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .ToListAsync();

        var mrr = activeSubscriptions.Sum(s => s.TotalPrice / (int)s.BillingCycle);

        // Demo requests
        var newDemoRequests = await _context.DemoRequests
            .CountAsync(d => d.CreatedAt >= sevenDaysAgo);
        var pendingDemoRequests = await _context.DemoRequests
            .CountAsync(d => d.Status == "New" || d.Status == "Contacted");

        // Health summary
        var recentCutoff = now.AddMinutes(-15);
        var unhealthyTenants = await _context.TenantHealthChecks
            .Where(h => h.CheckedAt >= recentCutoff)
            .GroupBy(h => h.TenantId)
            .Select(g => g.OrderByDescending(h => h.CheckedAt).First())
            .CountAsync(h => h.OverallStatus == HealthStatus.Unhealthy);

        return Ok(new
        {
            success = true,
            data = new
            {
                tenants = new
                {
                    total = totalTenants,
                    trial = trialTenants,
                    active = activeTenants,
                    suspended = suspendedTenants,
                    expired = expiredTenants,
                    trialsExpiringSoon,
                    newLast30Days = newTenantsLast30Days,
                    newLast7Days = newTenantsLast7Days
                },
                revenue = new
                {
                    mrr,
                    currency = "AED",
                    activeSubscriptions = activeSubscriptions.Count
                },
                leads = new
                {
                    newLast7Days = newDemoRequests,
                    pending = pendingDemoRequests
                },
                health = new
                {
                    unhealthyTenants
                },
                updatedAt = now
            }
        });
    }
}
