using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/tenants")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class TenantsAdminController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantsAdminController> _logger;

    public TenantsAdminController(
        PlatformDbContext context,
        IAuditService auditService,
        ILogger<TenantsAdminController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tenants with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTenants(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? companyType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(search) ||
                t.Slug.ToLower().Contains(search) ||
                t.ContactEmail.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(companyType) && Enum.TryParse<CompanyType>(companyType, true, out var ctEnum))
        {
            query = query.Where(t => t.CompanyType == ctEnum);
        }

        var total = await query.CountAsync();

        var tenants = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.CompanyType,
                t.ClinicType,
                t.Status,
                t.ContactEmail,
                t.Country,
                t.TrialEndDate,
                t.TrialDaysRemaining,
                t.MaxBranches,
                t.MaxUsers,
                t.CurrentBranches,
                t.CurrentUsers,
                t.IsDatabaseProvisioned,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                items = tenants,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Get tenant details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Admins)
            .Include(t => t.Subscriptions.OrderByDescending(s => s.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        // Get recent health checks
        var recentHealth = await _context.TenantHealthChecks
            .Where(h => h.TenantId == id)
            .OrderByDescending(h => h.CheckedAt)
            .Take(10)
            .ToListAsync();

        // Get recent usage
        var recentUsage = await _context.UsageSnapshots
            .Where(u => u.TenantId == id && u.SnapshotType == "Daily")
            .OrderByDescending(u => u.SnapshotDate)
            .Take(30)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                tenant = new
                {
                    tenant.Id,
                    tenant.Name,
                    tenant.Slug,
                    tenant.LegalName,
                    tenant.CompanyType,
                    tenant.ClinicType,
                    tenant.Status,
                    tenant.ContactEmail,
                    tenant.ContactPhone,
                    tenant.Country,
                    tenant.Address,
                    tenant.TrialStartDate,
                    tenant.TrialEndDate,
                    tenant.TrialDaysRemaining,
                    tenant.IsTrialExpired,
                    tenant.MaxBranches,
                    tenant.MaxUsers,
                    tenant.CurrentBranches,
                    tenant.CurrentUsers,
                    tenant.IsDatabaseProvisioned,
                    tenant.DatabaseProvisionedAt,
                    tenant.CreatedAt,
                    tenant.UpdatedAt
                },
                admins = tenant.Admins.Select(a => new
                {
                    a.Id,
                    a.Email,
                    a.FirstName,
                    a.LastName,
                    a.Role,
                    a.IsActive,
                    a.LastLoginAt,
                    a.CreatedAt
                }),
                subscriptions = tenant.Subscriptions.Select(s => new
                {
                    s.Id,
                    s.PlanCode,
                    s.Status,
                    s.BillingCycle,
                    s.StartDate,
                    s.EndDate,
                    s.TotalPrice,
                    s.Currency,
                    s.AutoRenew,
                    s.CreatedAt
                }),
                healthHistory = recentHealth.Select(h => new
                {
                    h.CheckedAt,
                    h.OverallStatus,
                    h.DatabaseStatus,
                    h.DatabaseLatencyMs,
                    h.DatabaseError
                }),
                usageHistory = recentUsage.Select(u => new
                {
                    u.SnapshotDate,
                    u.ActiveUsers,
                    u.TotalUsers,
                    u.ActiveBranches,
                    u.ApiCallsCount,
                    u.PatientsCount,
                    u.AppointmentsCount
                })
            }
        });
    }

    /// <summary>
    /// Suspend a tenant
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id, [FromBody] SuspendRequest? request)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        if (tenant.Status == TenantStatus.Suspended)
        {
            return BadRequest(new { success = false, error = "Tenant is already suspended" });
        }

        var previousStatus = tenant.Status;
        tenant.Status = TenantStatus.Suspended;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TenantSuspended", "Tenant", id,
            oldValues: new { status = previousStatus.ToString() },
            newValues: new { status = TenantStatus.Suspended.ToString(), reason = request?.Reason },
            tenantId: id);

        _logger.LogInformation("Tenant {TenantId} suspended by admin", id);

        return Ok(new { success = true, message = "Tenant suspended successfully" });
    }

    /// <summary>
    /// Activate a tenant
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        if (tenant.Status == TenantStatus.Active)
        {
            return BadRequest(new { success = false, error = "Tenant is already active" });
        }

        var previousStatus = tenant.Status;
        tenant.Status = TenantStatus.Active;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TenantActivated", "Tenant", id,
            oldValues: new { status = previousStatus.ToString() },
            newValues: new { status = TenantStatus.Active.ToString() },
            tenantId: id);

        _logger.LogInformation("Tenant {TenantId} activated by admin", id);

        return Ok(new { success = true, message = "Tenant activated successfully" });
    }

    /// <summary>
    /// Extend trial period
    /// </summary>
    [HttpPost("{id:guid}/extend-trial")]
    public async Task<IActionResult> ExtendTrial(Guid id, [FromBody] ExtendTrialRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        if (tenant.Status != TenantStatus.Trial && tenant.Status != TenantStatus.Expired)
        {
            return BadRequest(new { success = false, error = "Can only extend trial for trial or expired tenants" });
        }

        var previousEndDate = tenant.TrialEndDate;
        tenant.TrialEndDate = tenant.TrialEndDate.AddDays(request.Days);
        tenant.Status = TenantStatus.Trial;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TrialExtended", "Tenant", id,
            oldValues: new { trialEndDate = previousEndDate },
            newValues: new { trialEndDate = tenant.TrialEndDate, extensionDays = request.Days, reason = request.Reason },
            tenantId: id);

        _logger.LogInformation("Tenant {TenantId} trial extended by {Days} days", id, request.Days);

        return Ok(new
        {
            success = true,
            message = $"Trial extended by {request.Days} days",
            data = new { newTrialEndDate = tenant.TrialEndDate }
        });
    }

    /// <summary>
    /// Get tenant usage summary
    /// </summary>
    [HttpGet("{id:guid}/usage")]
    public async Task<IActionResult> GetTenantUsage(Guid id, [FromQuery] int days = 30)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        var startDate = DateTime.UtcNow.AddDays(-days);

        var usage = await _context.UsageSnapshots
            .Where(u => u.TenantId == id && u.SnapshotDate >= startDate)
            .OrderBy(u => u.SnapshotDate)
            .Select(u => new
            {
                u.SnapshotDate,
                u.SnapshotType,
                u.ActiveUsers,
                u.TotalUsers,
                u.ActiveBranches,
                u.ApiCallsCount,
                u.ApiErrorsCount,
                u.StorageUsedBytes,
                u.DocumentsCount,
                u.PatientsCount,
                u.AppointmentsCount,
                u.InvoicesCount,
                u.TotalSessions,
                u.AvgSessionDurationMinutes
            })
            .ToListAsync();

        // Calculate aggregates
        var totalApiCalls = usage.Sum(u => u.ApiCallsCount);
        var totalApiErrors = usage.Sum(u => u.ApiErrorsCount);
        var avgDailyActiveUsers = usage.Where(u => u.SnapshotType == "Daily")
            .Select(u => u.ActiveUsers)
            .DefaultIfEmpty(0)
            .Average();

        return Ok(new
        {
            success = true,
            data = new
            {
                tenantId = id,
                period = new { startDate, endDate = DateTime.UtcNow, days },
                summary = new
                {
                    currentUsers = tenant.CurrentUsers,
                    currentBranches = tenant.CurrentBranches,
                    maxUsers = tenant.MaxUsers,
                    maxBranches = tenant.MaxBranches,
                    totalApiCalls,
                    totalApiErrors,
                    avgDailyActiveUsers = Math.Round(avgDailyActiveUsers, 1)
                },
                history = usage
            }
        });
    }
}

public class SuspendRequest
{
    public string? Reason { get; set; }
}

public class ExtendTrialRequest
{
    public int Days { get; set; } = 14;
    public string? Reason { get; set; }
}
