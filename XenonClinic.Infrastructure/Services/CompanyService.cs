using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public CompanyService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context,
        ITenantService tenantService,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _context = context;
        _tenantService = tenantService;
        _cache = cache;
    }

    public async Task<int?> GetCurrentCompanyIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.CompanyId;
    }

    public async Task<Company?> GetCurrentCompanyAsync()
    {
        var companyId = await GetCurrentCompanyIdAsync();
        if (companyId == null) return null;

        return await GetCompanyByIdAsync(companyId.Value);
    }

    public async Task<bool> HasAccessToCompanyAsync(int companyId)
    {
        if (await _tenantService.IsSuperAdminAsync())
        {
            return true;
        }

        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        // Tenant admins have access to all companies in their tenant
        if (await _userManager.IsInRoleAsync(user, "TenantAdmin"))
        {
            var company = await _context.Companies.FindAsync(companyId);
            return company?.TenantId == user.TenantId;
        }

        // Regular users only have access to their assigned company
        return user.CompanyId == companyId;
    }

    public async Task<List<Company>> GetTenantCompaniesAsync()
    {
        var tenantId = await _tenantService.GetCurrentTenantIdAsync();
        if (tenantId == null) return new List<Company>();

        return await _context.Companies
            .Include(c => c.Branches)
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Company>> GetUserCompaniesAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return new List<Company>();

        // Super admins can see all companies
        if (await _tenantService.IsSuperAdminAsync())
        {
            return await _context.Companies
                .Include(c => c.Branches)
                .Include(c => c.Tenant)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Tenant.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        // Tenant admins can see all companies in their tenant
        if (await _userManager.IsInRoleAsync(user, "TenantAdmin"))
        {
            return await _context.Companies
                .Include(c => c.Branches)
                .Where(c => c.TenantId == user.TenantId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // Regular users can only see their assigned company
        if (user.CompanyId == null) return new List<Company>();

        var company = await GetCompanyByIdAsync(user.CompanyId.Value);
        return company != null ? new List<Company> { company } : new List<Company>();
    }

    public async Task<Company?> GetCompanyByIdAsync(int companyId)
    {
        if (!await HasAccessToCompanyAsync(companyId))
        {
            return null;
        }

        var cacheKey = $"company_{companyId}";
        if (_cache.TryGetValue(cacheKey, out Company? company))
        {
            return company;
        }

        company = await _context.Companies
            .Include(c => c.Tenant)
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company != null)
        {
            _cache.Set(cacheKey, company, CacheDuration);
        }

        return company;
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    {
        // Verify tenant access
        if (!await _tenantService.HasAccessToTenantAsync(company.TenantId))
        {
            throw new UnauthorizedAccessException("You don't have access to this tenant");
        }

        // Check if tenant can create more companies
        if (!await _tenantService.CanCreateCompanyAsync(company.TenantId))
        {
            throw new InvalidOperationException("Tenant has reached the maximum number of companies allowed");
        }

        var user = await GetCurrentUserAsync();
        company.CreatedAt = DateTime.UtcNow;
        company.CreatedBy = user?.Id;

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return company;
    }

    public async Task<Company> UpdateCompanyAsync(Company company)
    {
        if (!await HasAccessToCompanyAsync(company.Id))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var user = await GetCurrentUserAsync();
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = user?.Id;

        _context.Companies.Update(company);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"company_{company.Id}");

        return company;
    }

    public async Task<bool> DeactivateCompanyAsync(int companyId)
    {
        if (!await HasAccessToCompanyAsync(companyId))
        {
            throw new UnauthorizedAccessException("You don't have access to this company");
        }

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null) return false;

        var user = await GetCurrentUserAsync();
        company.IsActive = false;
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = user?.Id;

        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove($"company_{companyId}");

        return true;
    }

    public async Task<bool> CanCreateBranchAsync(int companyId)
    {
        var company = await GetCompanyByIdAsync(companyId);
        if (company == null) return false;

        var tenant = await _tenantService.GetTenantByIdAsync(company.TenantId);
        if (tenant == null) return false;

        var branchCount = await _context.Branches
            .CountAsync(b => b.CompanyId == companyId);

        return branchCount < tenant.MaxBranchesPerCompany;
    }

    public async Task<List<Branch>> GetCompanyBranchesAsync(int companyId)
    {
        if (!await HasAccessToCompanyAsync(companyId))
        {
            return new List<Branch>();
        }

        return await _context.Branches
            .Where(b => b.CompanyId == companyId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null) return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }
}
