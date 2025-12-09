using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Domain.ValueObjects;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Api.Controllers.Tenant;

[ApiController]
[Route("api/tenant/license")]
[Authorize(AuthenticationSchemes = "TenantScheme")]
public class LicenseController : ControllerBase
{
    private readonly PlatformDbContext _context;

    public LicenseController(PlatformDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get license limits and current usage for the tenant (used by ERP)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLicense()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var tenant = await _context.Tenants
            .Include(t => t.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        // Check if tenant can operate
        var canOperate = tenant.Status == TenantStatus.Active ||
                        (tenant.Status == TenantStatus.Trial && !tenant.IsTrialExpired);

        var guardrails = new LicenseGuardrails
        {
            MaxBranches = tenant.MaxBranches,
            MaxUsers = tenant.MaxUsers,
            CurrentBranches = tenant.CurrentBranches,
            CurrentUsers = tenant.CurrentUsers
        };

        var subscription = tenant.ActiveSubscription;

        return Ok(new
        {
            success = true,
            data = new
            {
                tenantId = tenant.Id,
                tenantSlug = tenant.Slug,
                status = tenant.Status.ToString(),
                canOperate,
                isTrial = tenant.Status == TenantStatus.Trial,
                trialDaysRemaining = tenant.TrialDaysRemaining,
                license = new
                {
                    guardrails.MaxBranches,
                    guardrails.MaxUsers,
                    guardrails.CurrentBranches,
                    guardrails.CurrentUsers,
                    guardrails.CanAddBranch,
                    guardrails.CanAddUser,
                    guardrails.RemainingBranches,
                    guardrails.RemainingUsers,
                    branchUsagePercent = Math.Round(guardrails.BranchUsagePercent, 1),
                    userUsagePercent = Math.Round(guardrails.UserUsagePercent, 1)
                },
                subscription = subscription != null ? new
                {
                    plan = subscription.PlanCode.ToString(),
                    status = subscription.Status.ToString(),
                    expiresAt = subscription.EndDate,
                    daysRemaining = subscription.DaysRemaining,
                    autoRenew = subscription.AutoRenew
                } : null,
                checkedAt = DateTime.UtcNow
            }
        });
    }

    /// <summary>
    /// Update usage counters (called by ERP to report current usage)
    /// </summary>
    [HttpPost("usage")]
    public async Task<IActionResult> UpdateUsage([FromBody] UsageUpdateRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant not found" });
        }

        // Update usage counters
        if (request.CurrentBranches.HasValue)
        {
            tenant.CurrentBranches = request.CurrentBranches.Value;
        }

        if (request.CurrentUsers.HasValue)
        {
            tenant.CurrentUsers = request.CurrentUsers.Value;
        }

        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                currentBranches = tenant.CurrentBranches,
                currentUsers = tenant.CurrentUsers,
                maxBranches = tenant.MaxBranches,
                maxUsers = tenant.MaxUsers
            }
        });
    }
}

public class UsageUpdateRequest
{
    public int? CurrentBranches { get; set; }
    public int? CurrentUsers { get; set; }
}
