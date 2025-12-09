using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Api.Controllers.Tenant;

[ApiController]
[Route("api/tenant/usage")]
[Authorize(AuthenticationSchemes = "TenantScheme")]
public class UsageController : ControllerBase
{
    private readonly PlatformDbContext _context;

    public UsageController(PlatformDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get usage metrics for the tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsage([FromQuery] int days = 30)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var startDate = DateTime.UtcNow.AddDays(-days);

        var usage = await _context.UsageSnapshots
            .Where(u => u.TenantId == tenantId && u.SnapshotDate >= startDate)
            .OrderByDescending(u => u.SnapshotDate)
            .ToListAsync();

        var dailyUsage = usage
            .Where(u => u.SnapshotType == "Daily")
            .OrderBy(u => u.SnapshotDate)
            .Select(u => new
            {
                date = u.SnapshotDate.ToString("yyyy-MM-dd"),
                u.ActiveUsers,
                u.TotalUsers,
                u.ActiveBranches,
                u.ApiCallsCount,
                u.PatientsCount,
                u.AppointmentsCount
            })
            .ToList();

        // Calculate totals
        var totalApiCalls = usage.Where(u => u.SnapshotType == "Daily").Sum(u => u.ApiCallsCount);
        var avgDailyActiveUsers = usage.Where(u => u.SnapshotType == "Daily")
            .Select(u => u.ActiveUsers)
            .DefaultIfEmpty(0)
            .Average();

        var latestSnapshot = usage.FirstOrDefault();

        return Ok(new
        {
            success = true,
            data = new
            {
                period = new { startDate, endDate = DateTime.UtcNow, days },
                current = latestSnapshot != null ? new
                {
                    latestSnapshot.ActiveUsers,
                    latestSnapshot.TotalUsers,
                    latestSnapshot.ActiveBranches,
                    latestSnapshot.PatientsCount,
                    latestSnapshot.AppointmentsCount,
                    latestSnapshot.InvoicesCount,
                    latestSnapshot.DocumentsCount,
                    latestSnapshot.StorageUsedBytes
                } : null,
                summary = new
                {
                    totalApiCalls,
                    avgDailyActiveUsers = Math.Round(avgDailyActiveUsers, 1)
                },
                history = dailyUsage
            }
        });
    }

    /// <summary>
    /// Report usage snapshot (called by ERP periodically)
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> ReportUsage([FromBody] UsageReportRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var snapshot = new UsageSnapshot
        {
            TenantId = tenantId,
            SnapshotDate = DateTime.UtcNow.Date,
            SnapshotType = request.SnapshotType ?? "Hourly",
            ActiveUsers = request.ActiveUsers,
            TotalUsers = request.TotalUsers,
            NewUsersCount = request.NewUsersCount,
            ActiveBranches = request.ActiveBranches,
            ApiCallsCount = request.ApiCallsCount,
            ApiErrorsCount = request.ApiErrorsCount,
            StorageUsedBytes = request.StorageUsedBytes,
            DocumentsCount = request.DocumentsCount,
            PatientsCount = request.PatientsCount,
            AppointmentsCount = request.AppointmentsCount,
            InvoicesCount = request.InvoicesCount,
            TotalSessions = request.TotalSessions,
            AvgSessionDurationMinutes = request.AvgSessionDurationMinutes
        };

        _context.UsageSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        // Also update tenant's current usage
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant != null)
        {
            tenant.CurrentUsers = request.TotalUsers;
            tenant.CurrentBranches = request.ActiveBranches;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true, snapshotId = snapshot.Id });
    }
}

public class UsageReportRequest
{
    public string? SnapshotType { get; set; } // Hourly, Daily
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
    public int NewUsersCount { get; set; }
    public int ActiveBranches { get; set; }
    public long ApiCallsCount { get; set; }
    public long ApiErrorsCount { get; set; }
    public long StorageUsedBytes { get; set; }
    public int DocumentsCount { get; set; }
    public int PatientsCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int InvoicesCount { get; set; }
    public int TotalSessions { get; set; }
    public double AvgSessionDurationMinutes { get; set; }
}
