using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Provides centralized access to current user context.
/// Eliminates code duplication across services that need user information.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;
    private readonly IMemoryCache _cache;

    // Reduced cache duration for permission-sensitive data (was 5 minutes)
    // Short TTL ensures permission changes are reflected quickly
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private const string UserCacheKeyPrefix = "current_user_";
    private const string BranchesCacheKeyPrefix = "user_branches_";

    public CurrentUserContext(
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

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public async Task<IApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return null;

        var userId = _userManager.GetUserId(httpContext.User);
        if (string.IsNullOrEmpty(userId)) return null;

        var cacheKey = $"{UserCacheKeyPrefix}{userId}";

        if (_cache.TryGetValue(cacheKey, out ApplicationUser? cachedUser))
        {
            return cachedUser;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user != null)
        {
            _cache.Set(cacheKey, user, CacheDuration);
        }

        return user;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Id;
    }

    public async Task<int?> GetCurrentTenantIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.TenantId;
    }

    public async Task<int?> GetCurrentCompanyIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.CompanyId;
    }

    public async Task<int?> GetCurrentBranchIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.PrimaryBranchId;
    }

    public async Task<List<int>> GetAccessibleBranchIdsAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return new List<int>();

        var cacheKey = $"{BranchesCacheKeyPrefix}{user.Id}";

        if (_cache.TryGetValue(cacheKey, out List<int>? cachedBranches))
        {
            return cachedBranches ?? new List<int>();
        }

        var branches = new List<int>();

        // Super admins have access to all branches
        if (user.IsSuperAdmin)
        {
            branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();
        }
        // Tenant admins have access to all branches in their tenant
        else if (await IsInRoleAsync("TenantAdmin") && user.TenantId.HasValue)
        {
            branches = await _context.Branches
                .Include(b => b.Company)
                .Where(b => b.IsActive && b.Company != null && b.Company.TenantId == user.TenantId)
                .Select(b => b.Id)
                .ToListAsync();
        }
        // Company admins have access to all branches in their company
        else if (await IsInRoleAsync("CompanyAdmin") && user.CompanyId.HasValue)
        {
            branches = await _context.Branches
                .Where(b => b.IsActive && b.CompanyId == user.CompanyId)
                .Select(b => b.Id)
                .ToListAsync();
        }
        // Regular users only have access to their assigned branches
        else
        {
            branches = await _context.UserBranches
                .Where(ub => ub.UserId == user.Id)
                .Select(ub => ub.BranchId)
                .ToListAsync();

            // Always include primary branch
            if (user.PrimaryBranchId.HasValue && !branches.Contains(user.PrimaryBranchId.Value))
            {
                branches.Add(user.PrimaryBranchId.Value);
            }
        }

        _cache.Set(cacheKey, branches, CacheDuration);
        return branches;
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.IsSuperAdmin ?? false;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return false;

        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user == null) return false;

        return await _userManager.IsInRoleAsync(user, role);
    }
}
