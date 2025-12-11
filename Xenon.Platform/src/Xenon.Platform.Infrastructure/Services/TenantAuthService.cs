using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class TenantAuthService : ITenantAuthService
{
    private readonly PlatformDbContext _context;
    private readonly ITenantProvisioningService _provisioningService;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashingService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantAuthService> _logger;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public TenantAuthService(
        PlatformDbContext context,
        ITenantProvisioningService provisioningService,
        IJwtTokenService jwtService,
        IPasswordHashingService passwordService,
        IAuditService auditService,
        ILogger<TenantAuthService> logger)
    {
        _context = context;
        _provisioningService = provisioningService;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<TenantSignupResponse>> SignupAsync(TenantSignupRequest request)
    {
        // Validate email is not already in use GLOBALLY (not per-tenant).
        // This is intentional: tenant admins log in with just email (no tenant identifier),
        // so email must be unique across all tenants to avoid login ambiguity.
        // The same person can manage multiple tenants by being added as an admin with different emails,
        // or we could implement tenant selection after email verification in the future.
        var existingAdmin = await _context.TenantAdmins
            .AnyAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (existingAdmin)
        {
            return "Email already registered";
        }

        try
        {
            var tenant = await _provisioningService.CreateTenant(new CreateTenantRequest
            {
                CompanyName = request.CompanyName,
                CompanyType = Enum.TryParse<CompanyType>(request.CompanyType, true, out var ct) ? ct : CompanyType.Clinic,
                ClinicType = Enum.TryParse<ClinicType>(request.ClinicType, true, out var clt) ? clt : null,
                AdminEmail = request.Email,
                AdminPassword = request.Password,
                AdminFirstName = request.FirstName,
                AdminLastName = request.LastName,
                Phone = request.Phone,
                Country = request.Country,
                TrialDays = 30
            });

            // Provision database in background with proper tracking
            // Note: In production, this should use a job queue (Hangfire/Quartz) for better reliability
            _ = ProvisionTenantDatabaseAsync(tenant.Id);

            await _auditService.LogAsync("TenantCreated", "Tenant", tenant.Id,
                newValues: new { tenant.Name, tenant.Slug, tenant.ContactEmail });

            // Get the admin user
            var admin = await _context.TenantAdmins.FirstOrDefaultAsync(a => a.TenantId == tenant.Id);
            if (admin == null)
            {
                return "Failed to create admin user";
            }

            // Generate token
            var token = _jwtService.GenerateTenantToken(admin, tenant);

            _logger.LogInformation("Tenant {TenantSlug} created with admin {AdminEmail}", tenant.Slug, admin.Email);

            return new TenantSignupResponse
            {
                Tenant = new TenantSummaryDto
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Slug = tenant.Slug,
                    Status = tenant.Status.ToString(),
                    TrialEndDate = tenant.TrialEndDate,
                    TrialDaysRemaining = tenant.TrialDaysRemaining
                },
                User = new TenantUserDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Role = admin.Role
                },
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant for {Email}", request.Email);
            return "Failed to create tenant";
        }
    }

    public async Task<Result<TenantLoginResponse>> LoginAsync(TenantLoginRequest request, string? clientIpAddress = null)
    {
        var admin = await _context.TenantAdmins
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (admin == null)
        {
            return "Invalid email or password";
        }

        if (admin.IsLockedOut)
        {
            return "Account is locked. Please try again later.";
        }

        if (!admin.IsActive)
        {
            return "Account is disabled";
        }

        if (admin.Tenant.Status == TenantStatus.Suspended)
        {
            return "Tenant account is suspended";
        }

        if (admin.Tenant.Status == TenantStatus.Cancelled)
        {
            return "Tenant account is cancelled";
        }

        if (!_passwordService.VerifyPassword(request.Password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;
            if (admin.FailedLoginAttempts >= MaxFailedAttempts)
            {
                admin.LockoutEndAt = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            }
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("LoginFailed", "TenantAdmin", admin.Id,
                newValues: new { admin.Email, admin.FailedLoginAttempts },
                tenantId: admin.TenantId, isSuccess: false, errorMessage: "Invalid password");

            return "Invalid email or password";
        }

        // Reset failed attempts and update login info
        admin.FailedLoginAttempts = 0;
        admin.LockoutEndAt = null;
        admin.LastLoginAt = DateTime.UtcNow;
        admin.LastLoginIp = clientIpAddress;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateTenantToken(admin, admin.Tenant);

        await _auditService.LogAsync("LoginSuccess", "TenantAdmin", admin.Id,
            newValues: new { admin.Email }, tenantId: admin.TenantId);

        _logger.LogInformation("Tenant admin {Email} logged in for tenant {TenantSlug}",
            admin.Email, admin.Tenant.Slug);

        return new TenantLoginResponse
        {
            Tenant = new TenantSummaryDto
            {
                Id = admin.Tenant.Id,
                Name = admin.Tenant.Name,
                Slug = admin.Tenant.Slug,
                Status = admin.Tenant.Status.ToString(),
                TrialEndDate = admin.Tenant.TrialEndDate,
                TrialDaysRemaining = admin.Tenant.TrialDaysRemaining,
                MaxBranches = admin.Tenant.MaxBranches,
                MaxUsers = admin.Tenant.MaxUsers
            },
            User = new TenantUserDto
            {
                Id = admin.Id,
                Email = admin.Email,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Role = admin.Role
            },
            Token = token
        };
    }

    public async Task<Result<TenantContextDto>> GetCurrentTenantAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return "Tenant not found";
        }

        var activeSubscription = tenant.ActiveSubscription;

        return new TenantContextDto
        {
            Tenant = new TenantDetailDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                CompanyType = tenant.CompanyType.ToString(),
                ClinicType = tenant.ClinicType?.ToString(),
                Status = tenant.Status.ToString(),
                TrialStartDate = tenant.TrialStartDate,
                TrialEndDate = tenant.TrialEndDate,
                TrialDaysRemaining = tenant.TrialDaysRemaining,
                IsTrialExpired = tenant.IsTrialExpired
            },
            License = new LicenseSummaryDto
            {
                MaxBranches = tenant.MaxBranches,
                MaxUsers = tenant.MaxUsers,
                CurrentBranches = tenant.CurrentBranches,
                CurrentUsers = tenant.CurrentUsers,
                CanAddBranch = tenant.CurrentBranches < tenant.MaxBranches,
                CanAddUser = tenant.CurrentUsers < tenant.MaxUsers
            },
            Subscription = activeSubscription != null ? new SubscriptionSummaryDto
            {
                PlanCode = activeSubscription.PlanCode.ToString(),
                Status = activeSubscription.Status.ToString(),
                StartDate = activeSubscription.StartDate,
                EndDate = activeSubscription.EndDate,
                DaysRemaining = activeSubscription.DaysRemaining,
                BillingCycle = activeSubscription.BillingCycle.ToString(),
                AutoRenew = activeSubscription.AutoRenew
            } : null
        };
    }

    public async Task<bool> IsSlugAvailableAsync(string slug)
    {
        return await _provisioningService.IsSlugAvailable(slug);
    }

    /// <summary>
    /// Provisions tenant database with proper error tracking.
    /// In production, this should be replaced with a job queue (Hangfire/Quartz) for reliability.
    /// </summary>
    private async Task ProvisionTenantDatabaseAsync(Guid tenantId)
    {
        try
        {
            _logger.LogInformation("Starting database provisioning for tenant {TenantId}", tenantId);

            await _provisioningService.ProvisionTenantDatabase(tenantId);

            // Update tenant provisioning status
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.IsDatabaseProvisioned = true;
                tenant.DatabaseProvisionedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _auditService.LogAsync("DatabaseProvisioned", "Tenant", tenantId,
                newValues: new { ProvisionedAt = DateTime.UtcNow });

            _logger.LogInformation("Database provisioning completed for tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision database for tenant {TenantId}. Manual intervention required.", tenantId);

            await _auditService.LogAsync("DatabaseProvisioningFailed", "Tenant", tenantId,
                newValues: new { Error = ex.Message }, isSuccess: false, errorMessage: ex.Message);

            // In production: Enqueue a retry job or trigger an alert
        }
    }

    public async Task<Result> LogoutAsync(Guid adminId)
    {
        var admin = await _context.TenantAdmins.FindAsync(adminId);
        if (admin == null)
        {
            return "Admin not found";
        }

        admin.LastLogoutAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Logout", "TenantAdmin", admin.Id,
            newValues: new { admin.Email, LogoutAt = admin.LastLogoutAt },
            tenantId: admin.TenantId);

        _logger.LogInformation("Tenant admin {Email} logged out", admin.Email);

        return Result.Success();
    }

    public async Task<Result> InvalidateAllTokensAsync(Guid adminId)
    {
        var admin = await _context.TenantAdmins.FindAsync(adminId);
        if (admin == null)
        {
            return "Admin not found";
        }

        admin.TokenInvalidatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TokensInvalidated", "TenantAdmin", admin.Id,
            newValues: new { admin.Email, InvalidatedAt = admin.TokenInvalidatedAt },
            tenantId: admin.TenantId);

        _logger.LogInformation("All tokens invalidated for tenant admin {Email}", admin.Email);

        return Result.Success();
    }
}
