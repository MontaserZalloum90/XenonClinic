using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

public class ThemeService : IThemeService
{
    private readonly ClinicDbContext _context;

    // Default colors as fallback
    private const string DefaultPrimaryColor = "#1F6FEB";
    private const string DefaultSecondaryColor = "#6B7280";

    public ThemeService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ThemeColors> GetCompanyThemeColorsAsync(int companyId)
    {
        var company = await _context.Companies
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
        {
            return new ThemeColors
            {
                PrimaryColor = DefaultPrimaryColor,
                SecondaryColor = DefaultSecondaryColor,
                Source = "Default"
            };
        }

        // If company has custom colors, use them
        if (!string.IsNullOrEmpty(company.PrimaryColor) && !string.IsNullOrEmpty(company.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = company.PrimaryColor,
                SecondaryColor = company.SecondaryColor,
                Source = "Company"
            };
        }

        // Otherwise, fall back to tenant colors
        if (company.Tenant != null &&
            !string.IsNullOrEmpty(company.Tenant.PrimaryColor) &&
            !string.IsNullOrEmpty(company.Tenant.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = company.Tenant.PrimaryColor,
                SecondaryColor = company.Tenant.SecondaryColor,
                Source = "Tenant"
            };
        }

        // Final fallback to defaults
        return new ThemeColors
        {
            PrimaryColor = DefaultPrimaryColor,
            SecondaryColor = DefaultSecondaryColor,
            Source = "Default"
        };
    }

    public async Task<ThemeColors> GetBranchThemeColorsAsync(int branchId)
    {
        var branch = await _context.Branches
            .Include(b => b.Company)
                .ThenInclude(c => c.Tenant)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
        {
            return new ThemeColors
            {
                PrimaryColor = DefaultPrimaryColor,
                SecondaryColor = DefaultSecondaryColor,
                Source = "Default"
            };
        }

        // If branch has custom colors, use them
        if (!string.IsNullOrEmpty(branch.PrimaryColor) && !string.IsNullOrEmpty(branch.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = branch.PrimaryColor,
                SecondaryColor = branch.SecondaryColor,
                Source = "Branch"
            };
        }

        // Otherwise, fall back to company colors
        if (branch.Company != null &&
            !string.IsNullOrEmpty(branch.Company.PrimaryColor) &&
            !string.IsNullOrEmpty(branch.Company.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = branch.Company.PrimaryColor,
                SecondaryColor = branch.Company.SecondaryColor,
                Source = "Company"
            };
        }

        // Otherwise, fall back to tenant colors
        if (branch.Company?.Tenant != null &&
            !string.IsNullOrEmpty(branch.Company.Tenant.PrimaryColor) &&
            !string.IsNullOrEmpty(branch.Company.Tenant.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = branch.Company.Tenant.PrimaryColor,
                SecondaryColor = branch.Company.Tenant.SecondaryColor,
                Source = "Tenant"
            };
        }

        // Final fallback to defaults
        return new ThemeColors
        {
            PrimaryColor = DefaultPrimaryColor,
            SecondaryColor = DefaultSecondaryColor,
            Source = "Default"
        };
    }

    public async Task<ThemeColors> GetTenantThemeColorsAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant != null &&
            !string.IsNullOrEmpty(tenant.PrimaryColor) &&
            !string.IsNullOrEmpty(tenant.SecondaryColor))
        {
            return new ThemeColors
            {
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor,
                Source = "Tenant"
            };
        }

        return new ThemeColors
        {
            PrimaryColor = DefaultPrimaryColor,
            SecondaryColor = DefaultSecondaryColor,
            Source = "Default"
        };
    }

    public async Task UpdateTenantThemeColorsAsync(int tenantId, string primaryColor, string secondaryColor)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
        }

        tenant.PrimaryColor = primaryColor;
        tenant.SecondaryColor = secondaryColor;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCompanyThemeColorsAsync(int companyId, string? primaryColor, string? secondaryColor)
    {
        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
        {
            throw new InvalidOperationException($"Company with ID {companyId} not found.");
        }

        // Null values mean "use tenant default"
        company.PrimaryColor = primaryColor ?? string.Empty;
        company.SecondaryColor = secondaryColor ?? string.Empty;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateBranchThemeColorsAsync(int branchId, string? primaryColor, string? secondaryColor)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new InvalidOperationException($"Branch with ID {branchId} not found.");
        }

        // Null values mean "use company/tenant default"
        branch.PrimaryColor = primaryColor ?? string.Empty;
        branch.SecondaryColor = secondaryColor ?? string.Empty;

        await _context.SaveChangesAsync();
    }

    public async Task ResetCompanyThemeColorsAsync(int companyId)
    {
        await UpdateCompanyThemeColorsAsync(companyId, null, null);
    }

    public async Task ResetBranchThemeColorsAsync(int branchId)
    {
        await UpdateBranchThemeColorsAsync(branchId, null, null);
    }
}
