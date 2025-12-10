using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

public class BranchScopedService : IBranchScopedService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;

    public BranchScopedService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _context = context;
    }

    public async Task<int?> GetCurrentUserBranchIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.PrimaryBranchId;
    }

    public async Task<List<int>> GetUserBranchIdsAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return new List<int>();

        // Super admins have access to all branches
        if (user.IsSuperAdmin || await _userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin))
        {
            return await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();
        }

        // Tenant admins have access to all branches in their tenant
        if (await _userManager.IsInRoleAsync(user, RoleConstants.TenantAdmin) && user.TenantId.HasValue)
        {
            var companyIds = await _context.Companies
                .Where(c => c.TenantId == user.TenantId && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.Branches
                .Where(b => companyIds.Contains(b.CompanyId) && b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();
        }

        // Company admins have access to all branches in their company
        if (await _userManager.IsInRoleAsync(user, RoleConstants.CompanyAdmin) && user.CompanyId.HasValue)
        {
            return await _context.Branches
                .Where(b => b.CompanyId == user.CompanyId && b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();
        }

        // Regular admins (branch level) have access to assigned branches
        if (await _userManager.IsInRoleAsync(user, RoleConstants.Admin))
        {
            return await _context.UserBranches
                .Where(ub => ub.UserId == user.Id)
                .Join(_context.Branches.Where(b => b.IsActive),
                    ub => ub.BranchId,
                    b => b.Id,
                    (ub, b) => b.Id)
                .ToListAsync();
        }

        // Standard users get their assigned branches
        var branchIds = await _context.UserBranches
            .Where(ub => ub.UserId == user.Id)
            .Select(ub => ub.BranchId)
            .ToListAsync();

        return branchIds;
    }

    public async Task<bool> HasAccessToBranchAsync(int branchId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        // Super admins have access to all branches
        if (user.IsSuperAdmin || await _userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin))
        {
            return true;
        }

        // Get the branch to check tenant/company hierarchy
        var branch = await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null || !branch.IsActive) return false;

        // Tenant admins have access to all branches in their tenant
        if (await _userManager.IsInRoleAsync(user, RoleConstants.TenantAdmin) && user.TenantId.HasValue)
        {
            // Null-safe navigation for Company
            return branch.Company?.TenantId == user.TenantId;
        }

        // Company admins have access to all branches in their company
        if (await _userManager.IsInRoleAsync(user, RoleConstants.CompanyAdmin) && user.CompanyId.HasValue)
        {
            return branch.CompanyId == user.CompanyId;
        }

        // Branch-level admins have access to assigned branches
        if (await _userManager.IsInRoleAsync(user, RoleConstants.Admin) || await _userManager.IsInRoleAsync(user, RoleConstants.BranchAdmin))
        {
            return await _context.UserBranches
                .AnyAsync(ub => ub.UserId == user.Id && ub.BranchId == branchId);
        }

        // Standard users check their branch assignments
        var hasAccess = await _context.UserBranches
            .AnyAsync(ub => ub.UserId == user.Id && ub.BranchId == branchId);

        return hasAccess;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }
}
