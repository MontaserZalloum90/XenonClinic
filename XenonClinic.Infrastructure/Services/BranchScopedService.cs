using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

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

        // Admins have access to all branches
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return true;
        }

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
