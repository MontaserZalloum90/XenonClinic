using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class TenantManagementService : ITenantManagementService
{
    private readonly PlatformDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantManagementService> _logger;

    public TenantManagementService(
        PlatformDbContext context,
        IAuditService auditService,
        ILogger<TenantManagementService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PagedResult<TenantListItemDto>> GetTenantsAsync(TenantListQuery query)
    {
        var dbQuery = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            dbQuery = dbQuery.Where(t =>
                t.Name.ToLower().Contains(search) ||
                t.Slug.ToLower().Contains(search) ||
                t.ContactEmail.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<TenantStatus>(query.Status, true, out var statusEnum))
        {
            dbQuery = dbQuery.Where(t => t.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(query.CompanyType) &&
            Enum.TryParse<CompanyType>(query.CompanyType, true, out var ctEnum))
        {
            dbQuery = dbQuery.Where(t => t.CompanyType == ctEnum);
        }

        var total = await dbQuery.CountAsync();

        var tenants = await dbQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TenantListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CompanyType = t.CompanyType.ToString(),
                ClinicType = t.ClinicType != null ? t.ClinicType.ToString() : null,
                Status = t.Status.ToString(),
                ContactEmail = t.ContactEmail,
                Country = t.Country,
                TrialEndDate = t.TrialEndDate,
                TrialDaysRemaining = t.TrialDaysRemaining,
                MaxBranches = t.MaxBranches,
                MaxUsers = t.MaxUsers,
                CurrentBranches = t.CurrentBranches,
                CurrentUsers = t.CurrentUsers,
                IsDatabaseProvisioned = t.IsDatabaseProvisioned,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<TenantListItemDto>
        {
            Items = tenants,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<Result<TenantFullDetailsDto>> GetTenantDetailsAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Admins)
            .Include(t => t.Subscriptions.OrderByDescending(s => s.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

        var recentHealth = await _context.TenantHealthChecks
            .Where(h => h.TenantId == tenantId)
            .OrderByDescending(h => h.CheckedAt)
            .Take(10)
            .Select(h => new TenantHealthHistoryDto
            {
                CheckedAt = h.CheckedAt,
                OverallStatus = h.OverallStatus.ToString(),
                DatabaseStatus = h.DatabaseStatus.ToString(),
                DatabaseLatencyMs = h.DatabaseLatencyMs,
                DatabaseError = h.DatabaseError
            })
            .ToListAsync();

        var recentUsage = await _context.UsageSnapshots
            .Where(u => u.TenantId == tenantId && u.SnapshotType == "Daily")
            .OrderByDescending(u => u.SnapshotDate)
            .Take(30)
            .Select(u => new TenantUsageHistoryDto
            {
                SnapshotDate = u.SnapshotDate,
                ActiveUsers = u.ActiveUsers,
                TotalUsers = u.TotalUsers,
                ActiveBranches = u.ActiveBranches,
                ApiCallsCount = u.ApiCallsCount,
                PatientsCount = u.PatientsCount,
                AppointmentsCount = u.AppointmentsCount
            })
            .ToListAsync();

        return new TenantFullDetailsDto
        {
            Tenant = new TenantInfoDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                LegalName = tenant.LegalName,
                CompanyType = tenant.CompanyType.ToString(),
                ClinicType = tenant.ClinicType?.ToString(),
                Status = tenant.Status.ToString(),
                ContactEmail = tenant.ContactEmail,
                ContactPhone = tenant.ContactPhone,
                Country = tenant.Country,
                Address = tenant.Address,
                TrialStartDate = tenant.TrialStartDate,
                TrialEndDate = tenant.TrialEndDate,
                TrialDaysRemaining = tenant.TrialDaysRemaining,
                IsTrialExpired = tenant.IsTrialExpired,
                MaxBranches = tenant.MaxBranches,
                MaxUsers = tenant.MaxUsers,
                CurrentBranches = tenant.CurrentBranches,
                CurrentUsers = tenant.CurrentUsers,
                IsDatabaseProvisioned = tenant.IsDatabaseProvisioned,
                DatabaseProvisionedAt = tenant.DatabaseProvisionedAt,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            },
            Admins = tenant.Admins.Select(a => new TenantAdminDto
            {
                Id = a.Id,
                Email = a.Email,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Role = a.Role,
                IsActive = a.IsActive,
                LastLoginAt = a.LastLoginAt,
                CreatedAt = a.CreatedAt
            }).ToList(),
            Subscriptions = tenant.Subscriptions.Select(s => new TenantSubscriptionDto
            {
                Id = s.Id,
                PlanCode = s.PlanCode.ToString(),
                Status = s.Status.ToString(),
                BillingCycle = s.BillingCycle.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TotalPrice = s.TotalPrice,
                Currency = s.Currency.ToString(),
                AutoRenew = s.AutoRenew,
                CreatedAt = s.CreatedAt
            }).ToList(),
            HealthHistory = recentHealth,
            UsageHistory = recentUsage
        };
    }

    public async Task<Result> SuspendTenantAsync(Guid tenantId, SuspendTenantRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return Result.Failure("Tenant not found");
        }

        if (tenant.Status == TenantStatus.Suspended)
        {
            return Result.Failure("Tenant is already suspended");
        }

        var previousStatus = tenant.Status;
        tenant.Status = TenantStatus.Suspended;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TenantSuspended", "Tenant", tenantId,
            oldValues: new { status = previousStatus.ToString() },
            newValues: new { status = TenantStatus.Suspended.ToString(), reason = request.Reason },
            tenantId: tenantId);

        _logger.LogInformation("Tenant {TenantId} suspended by admin", tenantId);

        return Result.Success();
    }

    public async Task<Result> ActivateTenantAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return Result.Failure("Tenant not found");
        }

        if (tenant.Status == TenantStatus.Active)
        {
            return Result.Failure("Tenant is already active");
        }

        var previousStatus = tenant.Status;
        tenant.Status = TenantStatus.Active;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TenantActivated", "Tenant", tenantId,
            oldValues: new { status = previousStatus.ToString() },
            newValues: new { status = TenantStatus.Active.ToString() },
            tenantId: tenantId);

        _logger.LogInformation("Tenant {TenantId} activated by admin", tenantId);

        return Result.Success();
    }

    public async Task<Result<ExtendTrialResponse>> ExtendTrialAsync(Guid tenantId, ExtendTrialRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

        if (tenant.Status != TenantStatus.Trial && tenant.Status != TenantStatus.Expired)
        {
            return "Can only extend trial for trial or expired tenants";
        }

        var previousEndDate = tenant.TrialEndDate;
        tenant.TrialEndDate = tenant.TrialEndDate.AddDays(request.Days);
        tenant.Status = TenantStatus.Trial;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TrialExtended", "Tenant", tenantId,
            oldValues: new { trialEndDate = previousEndDate },
            newValues: new { trialEndDate = tenant.TrialEndDate, extensionDays = request.Days, reason = request.Reason },
            tenantId: tenantId);

        _logger.LogInformation("Tenant {TenantId} trial extended by {Days} days", tenantId, request.Days);

        return new ExtendTrialResponse
        {
            NewTrialEndDate = tenant.TrialEndDate
        };
    }

    public async Task<Result<TenantUsageDto>> GetTenantUsageAsync(Guid tenantId, TenantUsageQuery query)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

        var startDate = DateTime.UtcNow.AddDays(-query.Days);

        var usage = await _context.UsageSnapshots
            .Where(u => u.TenantId == tenantId && u.SnapshotDate >= startDate)
            .OrderBy(u => u.SnapshotDate)
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

        var totalApiCalls = usage.Sum(u => u.ApiCallsCount);
        var totalApiErrors = usage.Sum(u => u.ApiErrorsCount);
        var avgDailyActiveUsers = usage.Where(u => u.SnapshotType == "Daily")
            .Select(u => u.ActiveUsers)
            .DefaultIfEmpty(0)
            .Average();

        return new TenantUsageDto
        {
            TenantId = tenantId,
            Period = new UsagePeriodDto
            {
                StartDate = startDate,
                EndDate = DateTime.UtcNow,
                Days = query.Days
            },
            Summary = new UsageSummaryDto
            {
                CurrentUsers = tenant.CurrentUsers,
                CurrentBranches = tenant.CurrentBranches,
                MaxUsers = tenant.MaxUsers,
                MaxBranches = tenant.MaxBranches,
                TotalApiCalls = totalApiCalls,
                TotalApiErrors = totalApiErrors,
                AvgDailyActiveUsers = Math.Round(avgDailyActiveUsers, 1)
            },
            History = dailyUsage
        };
    }
}
