using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/reports")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class ReportsController : ControllerBase
{
    private readonly PlatformDbContext _context;

    public ReportsController(PlatformDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get monthly report
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var startDate = new DateTime(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1);

        // Tenant stats
        var newTenants = await _context.Tenants
            .CountAsync(t => t.CreatedAt >= startDate && t.CreatedAt < endDate);

        var tenantsAtEndOfMonth = await _context.Tenants
            .CountAsync(t => t.CreatedAt < endDate);

        var activeTenants = await _context.Tenants
            .CountAsync(t => t.Status == TenantStatus.Active && t.CreatedAt < endDate);

        var trialTenants = await _context.Tenants
            .CountAsync(t => t.Status == TenantStatus.Trial && t.CreatedAt < endDate);

        // Trial conversions (tenants that went from Trial to Active in this month)
        var conversions = await _context.AuditLogs
            .CountAsync(a => a.Action == "TenantActivated"
                && a.CreatedAt >= startDate
                && a.CreatedAt < endDate);

        // Churned tenants (became expired/cancelled/suspended)
        var churned = await _context.AuditLogs
            .CountAsync(a => (a.Action == "TenantSuspended" || a.Action == "TenantExpired" || a.Action == "TenantCancelled")
                && a.CreatedAt >= startDate
                && a.CreatedAt < endDate);

        // Revenue
        var subscriptionsThisMonth = await _context.Subscriptions
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt < endDate)
            .ToListAsync();

        var newMrr = subscriptionsThisMonth.Sum(s => s.TotalPrice / (int)s.BillingCycle);
        var totalRevenue = subscriptionsThisMonth.Sum(s => s.TotalPrice);

        // Demo requests
        var demoRequests = await _context.DemoRequests
            .CountAsync(d => d.CreatedAt >= startDate && d.CreatedAt < endDate);

        var convertedDemoRequests = await _context.DemoRequests
            .CountAsync(d => d.CreatedAt >= startDate && d.CreatedAt < endDate && d.ConvertedTenantId != null);

        // Plan distribution
        var planDistribution = await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .GroupBy(s => s.PlanCode)
            .Select(g => new { plan = g.Key.ToString(), count = g.Count() })
            .ToListAsync();

        // Top tenants by usage
        var topTenantsByUsers = await _context.Tenants
            .Where(t => t.Status == TenantStatus.Active || t.Status == TenantStatus.Trial)
            .OrderByDescending(t => t.CurrentUsers)
            .Take(10)
            .Select(t => new
            {
                t.Name,
                t.Slug,
                t.CurrentUsers,
                t.CurrentBranches,
                t.Status
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                period = new { year = targetYear, month = targetMonth, startDate, endDate },
                tenants = new
                {
                    newSignups = newTenants,
                    totalAtEndOfMonth = tenantsAtEndOfMonth,
                    active = activeTenants,
                    trial = trialTenants,
                    conversions,
                    churned,
                    churnRate = tenantsAtEndOfMonth > 0
                        ? Math.Round((double)churned / tenantsAtEndOfMonth * 100, 2)
                        : 0
                },
                revenue = new
                {
                    newMrr,
                    totalRevenue,
                    currency = "AED",
                    newSubscriptions = subscriptionsThisMonth.Count
                },
                leads = new
                {
                    demoRequests,
                    converted = convertedDemoRequests,
                    conversionRate = demoRequests > 0
                        ? Math.Round((double)convertedDemoRequests / demoRequests * 100, 2)
                        : 0
                },
                planDistribution,
                topTenants = topTenantsByUsers
            }
        });
    }

    /// <summary>
    /// Export monthly report as CSV
    /// </summary>
    [HttpGet("monthly/export")]
    public async Task<IActionResult> ExportMonthlyReport(
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var startDate = new DateTime(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1);

        // Get all active tenants with their subscription info
        var tenants = await _context.Tenants
            .Include(t => t.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
            .Where(t => t.CreatedAt < endDate)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Name,Slug,Status,Company Type,Clinic Type,Contact Email,Country," +
                      "Created Date,Trial End Date,Current Users,Max Users,Current Branches,Max Branches," +
                      "Plan,Subscription Status,Monthly Revenue");

        foreach (var tenant in tenants)
        {
            var subscription = tenant.Subscriptions.FirstOrDefault();
            var monthlyRevenue = subscription != null ? subscription.TotalPrice / (int)subscription.BillingCycle : 0;

            csv.AppendLine(
                $"\"{tenant.Name}\"," +
                $"\"{tenant.Slug}\"," +
                $"\"{tenant.Status}\"," +
                $"\"{tenant.CompanyType}\"," +
                $"\"{tenant.ClinicType}\"," +
                $"\"{tenant.ContactEmail}\"," +
                $"\"{tenant.Country}\"," +
                $"\"{tenant.CreatedAt:yyyy-MM-dd}\"," +
                $"\"{tenant.TrialEndDate:yyyy-MM-dd}\"," +
                $"{tenant.CurrentUsers}," +
                $"{tenant.MaxUsers}," +
                $"{tenant.CurrentBranches}," +
                $"{tenant.MaxBranches}," +
                $"\"{subscription?.PlanCode}\"," +
                $"\"{subscription?.Status}\"," +
                $"{monthlyRevenue:F2}");
        }

        var fileName = $"xenon-monthly-report-{targetYear}-{targetMonth:D2}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }

    /// <summary>
    /// Get tenant consumption report
    /// </summary>
    [HttpGet("tenant-consumption")]
    public async Task<IActionResult> GetTenantConsumptionReport([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var consumption = await _context.UsageSnapshots
            .Where(u => u.SnapshotDate >= startDate && u.SnapshotType == "Daily")
            .GroupBy(u => u.TenantId)
            .Select(g => new
            {
                TenantId = g.Key,
                TotalApiCalls = g.Sum(u => u.ApiCallsCount),
                TotalApiErrors = g.Sum(u => u.ApiErrorsCount),
                AvgDailyActiveUsers = g.Average(u => u.ActiveUsers),
                TotalSessions = g.Sum(u => u.TotalSessions),
                LatestSnapshot = g.OrderByDescending(u => u.SnapshotDate).FirstOrDefault()
            })
            .ToListAsync();

        // Join with tenant info
        var tenantIds = consumption.Select(c => c.TenantId).ToList();
        var tenants = await _context.Tenants
            .Where(t => tenantIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id);

        var report = consumption.Select(c =>
        {
            tenants.TryGetValue(c.TenantId, out var tenant);
            return new
            {
                tenant = tenant != null ? new { tenant.Name, tenant.Slug, tenant.Status } : null,
                c.TotalApiCalls,
                c.TotalApiErrors,
                errorRate = c.TotalApiCalls > 0 ? Math.Round((double)c.TotalApiErrors / c.TotalApiCalls * 100, 2) : 0,
                avgDailyActiveUsers = Math.Round(c.AvgDailyActiveUsers, 1),
                c.TotalSessions,
                currentUsers = c.LatestSnapshot?.TotalUsers ?? 0,
                currentBranches = c.LatestSnapshot?.ActiveBranches ?? 0
            };
        })
        .OrderByDescending(r => r.TotalApiCalls)
        .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                period = new { startDate, endDate = DateTime.UtcNow, days },
                tenants = report
            }
        });
    }
}
