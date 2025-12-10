using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class UsageService : IUsageService
{
    private readonly PlatformDbContext _context;

    public UsageService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenantUsageDto>> GetUsageAsync(Guid tenantId, int days = 30)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return "Tenant not found";
        }

        var startDate = DateTime.UtcNow.AddDays(-days);

        var usage = await _context.UsageSnapshots
            .Where(u => u.TenantId == tenantId && u.SnapshotDate >= startDate)
            .OrderByDescending(u => u.SnapshotDate)
            .ToListAsync();

        var dailyUsage = usage
            .Where(u => u.SnapshotType == "Daily")
            .OrderBy(u => u.SnapshotDate)
            .Select(u => new DailyUsageDto
            {
                Date = u.SnapshotDate.ToString("yyyy-MM-dd"),
                ActiveUsers = u.ActiveUsers,
                TotalUsers = u.TotalUsers,
                ActiveBranches = u.ActiveBranches,
                ApiCallsCount = u.ApiCallsCount,
                PatientsCount = u.PatientsCount,
                AppointmentsCount = u.AppointmentsCount
            })
            .ToList();

        var totalApiCalls = usage.Where(u => u.SnapshotType == "Daily").Sum(u => u.ApiCallsCount);
        var avgDailyActiveUsers = usage.Where(u => u.SnapshotType == "Daily")
            .Select(u => u.ActiveUsers)
            .DefaultIfEmpty(0)
            .Average();

        var latestSnapshot = usage.FirstOrDefault();

        return new TenantUsageDto
        {
            TenantId = tenantId,
            Period = new UsagePeriodDto
            {
                StartDate = startDate,
                EndDate = DateTime.UtcNow,
                Days = days
            },
            Summary = new UsageSummaryDto
            {
                CurrentUsers = tenant.CurrentUsers,
                CurrentBranches = tenant.CurrentBranches,
                MaxUsers = tenant.MaxUsers,
                MaxBranches = tenant.MaxBranches,
                TotalApiCalls = totalApiCalls,
                TotalApiErrors = 0,
                AvgDailyActiveUsers = Math.Round(avgDailyActiveUsers, 1)
            },
            History = dailyUsage
        };
    }

    public async Task<Result<UsageReportResponse>> ReportUsageAsync(Guid tenantId, UsageReportRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return "Tenant not found";
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

        // Also update tenant's current usage
        tenant.CurrentUsers = request.TotalUsers;
        tenant.CurrentBranches = request.ActiveBranches;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UsageReportResponse
        {
            SnapshotId = snapshot.Id
        };
    }
}
