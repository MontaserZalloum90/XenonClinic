using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ICompanyService _companyService;

    public AdminService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context,
        ITenantService tenantService,
        ICompanyService companyService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _context = context;
        _tenantService = tenantService;
        _companyService = companyService;
    }

    // ==================== Tenant Management ====================

    public async Task<List<Tenant>> GetTenantsAsync(bool includeInactive = false)
    {
        if (!await _tenantService.IsSuperAdminAsync())
        {
            throw new UnauthorizedAccessException("Only super admins can access all tenants");
        }

        var query = _context.Tenants
            .Include(t => t.Companies)
            .Include(t => t.Settings)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<TenantStatistics> GetTenantStatisticsAsync(int tenantId)
    {
        if (!await _tenantService.HasAccessToTenantAsync(tenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        var companyIds = await _context.Companies
            .Where(c => c.TenantId == tenantId)
            .Select(c => c.Id)
            .ToListAsync();

        var branchIds = await _context.Branches
            .Where(b => companyIds.Contains(b.CompanyId))
            .Select(b => b.Id)
            .ToListAsync();

        return new TenantStatistics
        {
            TenantId = tenantId,
            TenantName = tenant.Name,
            CompanyCount = companyIds.Count,
            BranchCount = branchIds.Count,
            UserCount = await _context.Users.CountAsync(u => u.TenantId == tenantId),
            ActiveUserCount = await _context.Users.CountAsync(u => u.TenantId == tenantId && u.IsActive),
            PatientCount = await _context.Patients.CountAsync(p => branchIds.Contains(p.BranchId))
        };
    }

    // ==================== Company Management ====================

    public async Task<List<Company>> GetCompaniesAsync(int tenantId, bool includeInactive = false)
    {
        if (!await _tenantService.HasAccessToTenantAsync(tenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var query = _context.Companies
            .Include(c => c.Branches)
            .Where(c => c.TenantId == tenantId);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<CompanyStatistics> GetCompanyStatisticsAsync(int companyId)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
        {
            throw new InvalidOperationException("Company not found");
        }

        var branchIds = await _context.Branches
            .Where(b => b.CompanyId == companyId)
            .Select(b => b.Id)
            .ToListAsync();

        return new CompanyStatistics
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            BranchCount = branchIds.Count,
            UserCount = await _context.Users.CountAsync(u => u.CompanyId == companyId),
            ActiveUserCount = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.IsActive),
            PatientCount = await _context.Patients.CountAsync(p => branchIds.Contains(p.BranchId))
        };
    }

    // ==================== Branch Management ====================

    public async Task<List<Branch>> GetBranchesAsync(int companyId, bool includeInactive = false)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var query = _context.Branches
            .Include(b => b.Company)
            .Where(b => b.CompanyId == companyId);

        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<Branch> CreateBranchAsync(Branch branch)
    {
        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        if (!await _companyService.CanCreateBranchAsync(branch.CompanyId))
        {
            throw new InvalidOperationException("Company has reached the maximum number of branches allowed");
        }

        var user = await GetCurrentUserAsync();
        branch.CreatedAt = DateTime.UtcNow;
        branch.CreatedBy = user?.Id;

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return branch;
    }

    public async Task<Branch> UpdateBranchAsync(Branch branch)
    {
        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var user = await GetCurrentUserAsync();
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = user?.Id;

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();

        return branch;
    }

    public async Task<bool> DeactivateBranchAsync(int branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null) return false;

        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var user = await GetCurrentUserAsync();
        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = user?.Id;

        await _context.SaveChangesAsync();

        return true;
    }

    // ==================== User Management ====================

    public async Task<List<IApplicationUser>> GetTenantUsersAsync(int tenantId, bool includeInactive = false)
    {
        if (!await _tenantService.HasAccessToTenantAsync(tenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        var query = _context.Users
            .Include(u => u.Company)
            .Include(u => u.PrimaryBranch)
            .Where(u => u.TenantId == tenantId);

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        var users = await query.OrderBy(u => u.DisplayName ?? u.UserName).ToListAsync();
        return users.Cast<IApplicationUser>().ToList();
    }

    public async Task<List<IApplicationUser>> GetCompanyUsersAsync(int companyId, bool includeInactive = false)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var query = _context.Users
            .Include(u => u.PrimaryBranch)
            .Where(u => u.CompanyId == companyId);

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        var users = await query.OrderBy(u => u.DisplayName ?? u.UserName).ToListAsync();
        return users.Cast<IApplicationUser>().ToList();
    }

    public async Task<List<IApplicationUser>> GetBranchUsersAsync(int branchId, bool includeInactive = false)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            return new List<IApplicationUser>();
        }

        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this branch");
        }

        var userIds = await _context.UserBranches
            .Where(ub => ub.BranchId == branchId)
            .Select(ub => ub.UserId)
            .ToListAsync();

        var query = _context.Users
            .Where(u => userIds.Contains(u.Id));

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        var users = await query.OrderBy(u => u.DisplayName ?? u.UserName).ToListAsync();
        return users.Cast<IApplicationUser>().ToList();
    }

    public async Task<IApplicationUser> CreateUserAsync(IApplicationUser user, string password, List<string> roles, List<int> branchIds)
    {
        // Cast to ApplicationUser for UserManager operations
        var appUser = user as ApplicationUser ?? throw new ArgumentException("User must be of type ApplicationUser", nameof(user));

        // Verify tenant access
        if (appUser.TenantId.HasValue && !await _tenantService.HasAccessToTenantAsync(appUser.TenantId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        // Verify company access
        if (appUser.CompanyId.HasValue && !await _companyService.HasAccessToCompanyAsync(appUser.CompanyId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        // Check user limit
        if (appUser.TenantId.HasValue && !await _tenantService.CanCreateUserAsync(appUser.TenantId.Value))
        {
            throw new InvalidOperationException("Tenant has reached the maximum number of users allowed");
        }

        var currentUser = await GetCurrentUserAsync();
        appUser.CreatedAt = DateTime.UtcNow;
        appUser.CreatedBy = currentUser?.Id;
        appUser.IsActive = true;

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Assign roles
        if (roles.Any())
        {
            await _userManager.AddToRolesAsync(appUser, roles);
        }

        // Assign branches
        foreach (var branchId in branchIds)
        {
            _context.UserBranches.Add(new UserBranch
            {
                UserId = appUser.Id,
                BranchId = branchId
            });
        }

        await _context.SaveChangesAsync();

        return appUser;
    }

    public async Task<IApplicationUser> UpdateUserAssignmentsAsync(string userId, int? tenantId, int? companyId, int? primaryBranchId, List<int> branchIds)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify access to new assignments
        if (tenantId.HasValue && !await _tenantService.HasAccessToTenantAsync(tenantId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        if (companyId.HasValue && !await _companyService.HasAccessToCompanyAsync(companyId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var currentUser = await GetCurrentUserAsync();
        user.TenantId = tenantId;
        user.CompanyId = companyId;
        user.PrimaryBranchId = primaryBranchId;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUser?.Id;

        // Update branch assignments
        var existingBranches = await _context.UserBranches
            .Where(ub => ub.UserId == userId)
            .ToListAsync();

        _context.UserBranches.RemoveRange(existingBranches);

        foreach (var branchId in branchIds)
        {
            _context.UserBranches.Add(new UserBranch
            {
                UserId = userId,
                BranchId = branchId
            });
        }

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // Verify access to user's tenant
        if (user.TenantId.HasValue && !await _tenantService.HasAccessToTenantAsync(user.TenantId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this user");
        }

        var currentUser = await GetCurrentUserAsync();
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUser?.Id;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new List<string>();
        }

        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task UpdateUserRolesAsync(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify access to user's tenant
        if (user.TenantId.HasValue && !await _tenantService.HasAccessToTenantAsync(user.TenantId.Value))
        {
            throw new UnauthorizedAccessException("You don't have access to this user");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, roles);
    }

    // ==================== System Statistics ====================

    public async Task<SystemStatistics> GetSystemStatisticsAsync()
    {
        if (!await _tenantService.IsSuperAdminAsync())
        {
            throw new UnauthorizedAccessException("Only super admins can access system statistics");
        }

        return new SystemStatistics
        {
            TotalTenants = await _context.Tenants.CountAsync(),
            ActiveTenants = await _context.Tenants.CountAsync(t => t.IsActive),
            TotalCompanies = await _context.Companies.CountAsync(),
            TotalBranches = await _context.Branches.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
            TotalPatients = await _context.Patients.CountAsync()
        };
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }
}
