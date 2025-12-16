using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantService> _logger;

    // Reduced cache duration for permission-sensitive data (was 15 minutes)
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(2);
    // Longer cache for less sensitive data
    private static readonly TimeSpan DataCacheDuration = TimeSpan.FromMinutes(5);

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context,
        IMemoryCache cache,
        IAuditService auditService,
        ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _context = context;
        _cache = cache;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<int?> GetCurrentTenantIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.TenantId;
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (tenantId == null) return null;

        return await GetTenantByIdAsync(tenantId.Value);
    }

    public async Task<XenonClinic.Core.Entities.TenantSettings?> GetCurrentTenantSettingsAsync()
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (tenantId == null) return null;

        var cacheKey = $"tenant_settings_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out XenonClinic.Core.Entities.TenantSettings? settings))
        {
            return settings;
        }

        settings = await _context.TenantSettings
            .FirstOrDefaultAsync(ts => ts.TenantId == tenantId);

        if (settings != null)
        {
            _cache.Set(cacheKey, settings, DataCacheDuration);
        }

        return settings;
    }

    public async Task<bool> HasAccessToTenantAsync(int tenantId)
    {
        if (await IsSuperAdminAsync())
        {
            return true;
        }

        var userTenantId = await GetCurrentTenantIdAsync();
        return userTenantId == tenantId;
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        return user.IsSuperAdmin || await _userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin);
    }

    public async Task<List<Tenant>> GetAllTenantsAsync()
    {
        if (!await IsSuperAdminAsync())
        {
            throw new UnauthorizedAccessException("Only super admins can access all tenants");
        }

        return await _context.Tenants
            .Include(t => t.Companies)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
    {
        if (!await HasAccessToTenantAsync(tenantId))
        {
            return null;
        }

        var cacheKey = $"tenant_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            return tenant;
        }

        tenant = await _context.Tenants
            .Include(t => t.Settings)
            .Include(t => t.Companies)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant != null)
        {
            _cache.Set(cacheKey, tenant, DataCacheDuration);
        }

        return tenant;
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        if (!await IsSuperAdminAsync())
        {
            throw new UnauthorizedAccessException("Only super admins can create tenants");
        }

        var user = await GetCurrentUserAsync();
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.CreatedBy = user?.Id;

        // Create default settings for the tenant
        tenant.Settings = new XenonClinic.Core.Entities.TenantSettings
        {
            TenantId = tenant.Id,
            DefaultLanguage = "en",
            DefaultCurrency = "AED",
            DefaultTimezone = "Arabian Standard Time"
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(new AuditLogEntry
        {
            EventType = "TenantManagement",
            EventCategory = "Tenant",
            ResourceType = nameof(Tenant),
            ResourceId = tenant.Id.ToString(),
            Action = "Create",
            UserName = user?.UserName,
            NewValues = new Dictionary<string, object?>
            {
                ["Name"] = tenant.Name,
                ["Code"] = tenant.Code,
                ["MaxCompanies"] = tenant.MaxCompanies,
                ["MaxUsersPerTenant"] = tenant.MaxUsersPerTenant
            }
        });

        _logger.LogInformation("Tenant {TenantId} ({TenantName}) created by {UserId}",
            tenant.Id, tenant.Name, user?.Id);

        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
    {
        if (!await HasAccessToTenantAsync(tenant.Id))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var user = await GetCurrentUserAsync();

        // Get old values for audit
        var oldTenant = await _context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenant.Id);

        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.UpdatedBy = user?.Id;

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_{tenant.Id}");

        // Audit log
        await _auditService.LogAsync(new AuditLogEntry
        {
            EventType = "TenantManagement",
            EventCategory = "Tenant",
            ResourceType = nameof(Tenant),
            ResourceId = tenant.Id.ToString(),
            Action = "Update",
            UserName = user?.UserName,
            OldValues = oldTenant != null ? new Dictionary<string, object?>
            {
                ["Name"] = oldTenant.Name,
                ["IsActive"] = oldTenant.IsActive
            } : new(),
            NewValues = new Dictionary<string, object?>
            {
                ["Name"] = tenant.Name,
                ["IsActive"] = tenant.IsActive
            }
        });

        _logger.LogInformation("Tenant {TenantId} updated by {UserId}", tenant.Id, user?.Id);

        return tenant;
    }

    public async Task<bool> DeactivateTenantAsync(int tenantId)
    {
        if (!await IsSuperAdminAsync())
        {
            throw new UnauthorizedAccessException("Only super admins can deactivate tenants");
        }

        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) return false;

        var user = await GetCurrentUserAsync();
        var wasActive = tenant.IsActive;

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.UpdatedBy = user?.Id;

        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_{tenantId}");

        // Audit log
        await _auditService.LogAsync(new AuditLogEntry
        {
            EventType = "TenantManagement",
            EventCategory = "Tenant",
            ResourceType = nameof(Tenant),
            ResourceId = tenantId.ToString(),
            Action = "Deactivate",
            UserName = user?.UserName,
            OldValues = new Dictionary<string, object?> { ["IsActive"] = wasActive },
            NewValues = new Dictionary<string, object?> { ["IsActive"] = false }
        });

        _logger.LogWarning("Tenant {TenantId} deactivated by {UserId}", tenantId, user?.Id);

        return true;
    }

    public async Task<XenonClinic.Core.Entities.TenantSettings> UpdateTenantSettingsAsync(XenonClinic.Core.Entities.TenantSettings settings)
    {
        if (!await HasAccessToTenantAsync(settings.TenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var user = await GetCurrentUserAsync();

        // Get old values for audit
        var oldSettings = await _context.TenantSettings.AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.TenantId == settings.TenantId);

        settings.UpdatedAt = DateTime.UtcNow;
        settings.UpdatedBy = user?.Id;

        _context.TenantSettings.Update(settings);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_settings_{settings.TenantId}");
        _cache.Remove($"tenant_{settings.TenantId}");

        // Audit log
        await _auditService.LogAsync(new AuditLogEntry
        {
            EventType = "TenantManagement",
            EventCategory = "TenantSettings",
            ResourceType = nameof(XenonClinic.Core.Entities.TenantSettings),
            ResourceId = settings.Id.ToString(),
            Action = "Update",
            UserName = user?.UserName,
            OldValues = oldSettings != null ? new Dictionary<string, object?>
            {
                ["DefaultLanguage"] = oldSettings.DefaultLanguage,
                ["DefaultCurrency"] = oldSettings.DefaultCurrency
            } : new(),
            NewValues = new Dictionary<string, object?>
            {
                ["DefaultLanguage"] = settings.DefaultLanguage,
                ["DefaultCurrency"] = settings.DefaultCurrency
            }
        });

        _logger.LogInformation("Tenant settings for {TenantId} updated by {UserId}",
            settings.TenantId, user?.Id);

        return settings;
    }

    /// <summary>
    /// Checks if a company can be created within the tenant's license limits.
    /// Uses pessimistic locking to prevent race conditions.
    /// </summary>
    public async Task<bool> CanCreateCompanyAsync(int tenantId)
    {
        // Use a transaction with serializable isolation to prevent race conditions
        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            // Lock the tenant row for update
            var tenant = await _context.Tenants
                .FromSqlRaw("SELECT * FROM Tenants WITH (UPDLOCK, ROWLOCK) WHERE Id = {0}", tenantId)
                .FirstOrDefaultAsync();

            if (tenant == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Check access
            if (!await HasAccessToTenantAsync(tenantId))
            {
                await transaction.RollbackAsync();
                return false;
            }

            var companyCount = await _context.Companies
                .CountAsync(c => c.TenantId == tenantId);

            var canCreate = companyCount < tenant.MaxCompanies;

            await transaction.CommitAsync();

            if (!canCreate)
            {
                _logger.LogWarning(
                    "Tenant {TenantId} cannot create company: limit reached ({Current}/{Max})",
                    tenantId, companyCount, tenant.MaxCompanies);
            }

            return canCreate;
        }
        catch (Exception ex)
        {
            // Safely attempt to rollback, handling potential rollback failure
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Failed to rollback transaction for tenant {TenantId}", tenantId);
            }

            _logger.LogError(ex, "Error checking company creation limit for tenant {TenantId}", tenantId);

            // Fallback to non-locking check for databases that don't support UPDLOCK
            return await CanCreateCompanyFallbackAsync(tenantId);
        }
    }

    /// <summary>
    /// Fallback method for databases that don't support SQL Server-style locking hints.
    /// </summary>
    private async Task<bool> CanCreateCompanyFallbackAsync(int tenantId)
    {
        var tenant = await GetTenantByIdAsync(tenantId);
        if (tenant == null) return false;

        var companyCount = await _context.Companies
            .CountAsync(c => c.TenantId == tenantId);

        return companyCount < tenant.MaxCompanies;
    }

    /// <summary>
    /// Checks if a user can be created within the tenant's license limits.
    /// Uses pessimistic locking to prevent race conditions.
    /// </summary>
    public async Task<bool> CanCreateUserAsync(int tenantId)
    {
        // Use a transaction with serializable isolation to prevent race conditions
        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            // Lock the tenant row for update
            var tenant = await _context.Tenants
                .FromSqlRaw("SELECT * FROM Tenants WITH (UPDLOCK, ROWLOCK) WHERE Id = {0}", tenantId)
                .FirstOrDefaultAsync();

            if (tenant == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Check access
            if (!await HasAccessToTenantAsync(tenantId))
            {
                await transaction.RollbackAsync();
                return false;
            }

            var userCount = await _context.Users
                .CountAsync(u => u.TenantId == tenantId);

            var canCreate = userCount < tenant.MaxUsersPerTenant;

            await transaction.CommitAsync();

            if (!canCreate)
            {
                _logger.LogWarning(
                    "Tenant {TenantId} cannot create user: limit reached ({Current}/{Max})",
                    tenantId, userCount, tenant.MaxUsersPerTenant);
            }

            return canCreate;
        }
        catch (Exception ex)
        {
            // Safely attempt to rollback, handling potential rollback failure
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Failed to rollback transaction for tenant {TenantId}", tenantId);
            }

            _logger.LogError(ex, "Error checking user creation limit for tenant {TenantId}", tenantId);

            // Fallback to non-locking check for databases that don't support UPDLOCK
            return await CanCreateUserFallbackAsync(tenantId);
        }
    }

    /// <summary>
    /// Fallback method for databases that don't support SQL Server-style locking hints.
    /// </summary>
    private async Task<bool> CanCreateUserFallbackAsync(int tenantId)
    {
        var tenant = await GetTenantByIdAsync(tenantId);
        if (tenant == null) return false;

        var userCount = await _context.Users
            .CountAsync(u => u.TenantId == tenantId);

        return userCount < tenant.MaxUsersPerTenant;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }
}
