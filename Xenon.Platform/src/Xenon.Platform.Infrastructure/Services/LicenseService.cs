using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Domain.ValueObjects;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class LicenseService : ILicenseService
{
    private readonly PlatformDbContext _context;

    public LicenseService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LicenseInfoDto>> GetLicenseAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

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

        return new LicenseInfoDto
        {
            TenantId = tenant.Id,
            TenantSlug = tenant.Slug,
            Status = tenant.Status.ToString(),
            CanOperate = canOperate,
            IsTrial = tenant.Status == TenantStatus.Trial,
            TrialDaysRemaining = tenant.TrialDaysRemaining,
            License = new LicenseGuardrailsDto
            {
                MaxBranches = guardrails.MaxBranches,
                MaxUsers = guardrails.MaxUsers,
                CurrentBranches = guardrails.CurrentBranches,
                CurrentUsers = guardrails.CurrentUsers,
                CanAddBranch = guardrails.CanAddBranch,
                CanAddUser = guardrails.CanAddUser,
                RemainingBranches = guardrails.RemainingBranches,
                RemainingUsers = guardrails.RemainingUsers,
                BranchUsagePercent = Math.Round(guardrails.BranchUsagePercent, 1),
                UserUsagePercent = Math.Round(guardrails.UserUsagePercent, 1)
            },
            Subscription = subscription != null ? new LicenseSubscriptionDto
            {
                Plan = subscription.PlanCode.ToString(),
                Status = subscription.Status.ToString(),
                ExpiresAt = subscription.EndDate,
                DaysRemaining = subscription.DaysRemaining,
                AutoRenew = subscription.AutoRenew
            } : null,
            CheckedAt = DateTime.UtcNow
        };
    }

    public async Task<Result<UsageUpdateResponse>> UpdateUsageAsync(Guid tenantId, UsageUpdateRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

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

        return new UsageUpdateResponse
        {
            CurrentBranches = tenant.CurrentBranches,
            CurrentUsers = tenant.CurrentUsers,
            MaxBranches = tenant.MaxBranches,
            MaxUsers = tenant.MaxUsers
        };
    }
}
