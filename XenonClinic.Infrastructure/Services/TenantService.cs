using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _context = context;
        _cache = cache;
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

    public async Task<TenantSettings?> GetCurrentTenantSettingsAsync()
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (tenantId == null) return null;

        var cacheKey = $"tenant_settings_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out TenantSettings? settings))
        {
            return settings;
        }

        settings = await _context.TenantSettings
            .FirstOrDefaultAsync(ts => ts.TenantId == tenantId);

        if (settings != null)
        {
            _cache.Set(cacheKey, settings, CacheDuration);
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
            _cache.Set(cacheKey, tenant, CacheDuration);
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
        tenant.Settings = new TenantSettings
        {
            TenantId = tenant.Id,
            DefaultLanguage = "en",
            DefaultCurrency = "AED",
            DefaultTimezone = "Arabian Standard Time"
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
    {
        if (!await HasAccessToTenantAsync(tenant.Id))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var user = await GetCurrentUserAsync();
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.UpdatedBy = user?.Id;

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_{tenant.Id}");

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
        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.UpdatedBy = user?.Id;

        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_{tenantId}");

        return true;
    }

    public async Task<TenantSettings> UpdateTenantSettingsAsync(TenantSettings settings)
    {
        if (!await HasAccessToTenantAsync(settings.TenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var user = await GetCurrentUserAsync();
        settings.LastModifiedAt = DateTime.UtcNow;
        settings.LastModifiedBy = user?.Id;

        _context.TenantSettings.Update(settings);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"tenant_settings_{settings.TenantId}");
        _cache.Remove($"tenant_{settings.TenantId}");

        return settings;
    }

    public async Task<bool> CanCreateCompanyAsync(int tenantId)
    {
        var tenant = await GetTenantByIdAsync(tenantId);
        if (tenant == null) return false;

        var companyCount = await _context.Companies
            .CountAsync(c => c.TenantId == tenantId);

        return companyCount < tenant.MaxCompanies;
    }

    public async Task<bool> CanCreateUserAsync(int tenantId)
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
