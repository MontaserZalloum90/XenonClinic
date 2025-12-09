using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.Public;

[ApiController]
[Route("api/public/tenants")]
public class TenantsController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantProvisioningService _provisioningService;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashingService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        PlatformDbContext context,
        ITenantProvisioningService provisioningService,
        IJwtTokenService jwtService,
        IPasswordHashingService passwordService,
        IAuditService auditService,
        ILogger<TenantsController> logger)
    {
        _context = context;
        _provisioningService = provisioningService;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new trial tenant (signup)
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] TenantSignupRequest request)
    {
        // Validate email is not already in use
        var existingAdmin = await _context.TenantAdmins
            .AnyAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (existingAdmin)
        {
            return BadRequest(new { success = false, error = "Email already registered" });
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

            // Provision database in background (for now, do it synchronously)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _provisioningService.ProvisionTenantDatabase(tenant.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to provision database for tenant {TenantId}", tenant.Id);
                }
            });

            await _auditService.LogAsync("TenantCreated", "Tenant", tenant.Id,
                newValues: new { tenant.Name, tenant.Slug, tenant.ContactEmail });

            // Get the admin user
            var admin = await _context.TenantAdmins.FirstOrDefaultAsync(a => a.TenantId == tenant.Id);

            // Generate token
            var token = _jwtService.GenerateTenantToken(admin!, tenant);

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
                        tenant.Status,
                        tenant.TrialEndDate,
                        tenant.TrialDaysRemaining
                    },
                    user = new
                    {
                        admin!.Id,
                        admin.Email,
                        admin.FirstName,
                        admin.LastName,
                        admin.Role
                    },
                    token
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant");
            return StatusCode(500, new { success = false, error = "Failed to create tenant" });
        }
    }

    /// <summary>
    /// Tenant login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TenantLoginRequest request)
    {
        var admin = await _context.TenantAdmins
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (admin == null)
        {
            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        if (admin.IsLockedOut)
        {
            return Unauthorized(new { success = false, error = "Account is locked. Please try again later." });
        }

        if (!admin.IsActive)
        {
            return Unauthorized(new { success = false, error = "Account is disabled" });
        }

        if (admin.Tenant.Status == TenantStatus.Suspended)
        {
            return Unauthorized(new { success = false, error = "Tenant account is suspended" });
        }

        if (admin.Tenant.Status == TenantStatus.Cancelled)
        {
            return Unauthorized(new { success = false, error = "Tenant account is cancelled" });
        }

        if (!_passwordService.VerifyPassword(request.Password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;
            if (admin.FailedLoginAttempts >= 5)
            {
                admin.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
            }
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("LoginFailed", "TenantAdmin", admin.Id,
                newValues: new { admin.Email, admin.FailedLoginAttempts },
                tenantId: admin.TenantId, isSuccess: false, errorMessage: "Invalid password");

            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        // Reset failed attempts
        admin.FailedLoginAttempts = 0;
        admin.LockoutEndAt = null;
        admin.LastLoginAt = DateTime.UtcNow;
        admin.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateTenantToken(admin, admin.Tenant);

        await _auditService.LogAsync("LoginSuccess", "TenantAdmin", admin.Id,
            newValues: new { admin.Email }, tenantId: admin.TenantId);

        return Ok(new
        {
            success = true,
            data = new
            {
                tenant = new
                {
                    admin.Tenant.Id,
                    admin.Tenant.Name,
                    admin.Tenant.Slug,
                    admin.Tenant.Status,
                    admin.Tenant.TrialEndDate,
                    admin.Tenant.TrialDaysRemaining,
                    admin.Tenant.MaxBranches,
                    admin.Tenant.MaxUsers
                },
                user = new
                {
                    admin.Id,
                    admin.Email,
                    admin.FirstName,
                    admin.LastName,
                    admin.Role
                },
                token
            }
        });
    }

    /// <summary>
    /// Get current tenant context (requires tenant auth)
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "TenantScheme")]
    public async Task<IActionResult> GetCurrentTenant()
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

        var activeSubscription = tenant.ActiveSubscription;

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
                    tenant.CompanyType,
                    tenant.ClinicType,
                    tenant.Status,
                    tenant.TrialStartDate,
                    tenant.TrialEndDate,
                    tenant.TrialDaysRemaining,
                    tenant.IsTrialExpired
                },
                license = new
                {
                    tenant.MaxBranches,
                    tenant.MaxUsers,
                    tenant.CurrentBranches,
                    tenant.CurrentUsers,
                    canAddBranch = tenant.CurrentBranches < tenant.MaxBranches,
                    canAddUser = tenant.CurrentUsers < tenant.MaxUsers
                },
                subscription = activeSubscription != null ? new
                {
                    activeSubscription.PlanCode,
                    activeSubscription.Status,
                    activeSubscription.StartDate,
                    activeSubscription.EndDate,
                    activeSubscription.DaysRemaining,
                    activeSubscription.BillingCycle,
                    activeSubscription.AutoRenew
                } : null
            }
        });
    }

    /// <summary>
    /// Check if slug is available
    /// </summary>
    [HttpGet("check-slug")]
    public async Task<IActionResult> CheckSlug([FromQuery] string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { success = false, error = "Slug is required" });
        }

        var isAvailable = await _provisioningService.IsSlugAvailable(slug);

        return Ok(new { success = true, data = new { slug, isAvailable } });
    }
}

public class TenantSignupRequest
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    public string CompanyType { get; set; } = "Clinic";

    public string? ClinicType { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    public string? Country { get; set; }
}

public class TenantLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
