using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models.Admin;
using XenonClinic.Web.Models.Admin.Auth;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly ICompanyService _companyService;
    private readonly IAdminService _adminService;
    private readonly ICompanyAuthConfigService _authConfigService;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly IConfigurationResolverService _configResolver;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ClinicDbContext _context;

    public AdminController(
        ITenantService tenantService,
        ICompanyService companyService,
        IAdminService adminService,
        ICompanyAuthConfigService authConfigService,
        ISecretEncryptionService encryptionService,
        IConfigurationResolverService configResolver,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ClinicDbContext context)
    {
        _tenantService = tenantService;
        _companyService = companyService;
        _adminService = adminService;
        _authConfigService = authConfigService;
        _encryptionService = encryptionService;
        _configResolver = configResolver;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // ==================== Dashboard ====================

    public async Task<IActionResult> Index()
    {
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var model = new AdminDashboardViewModel
        {
            IsSuperAdmin = isSuperAdmin
        };

        if (isSuperAdmin)
        {
            model.Statistics = await _adminService.GetSystemStatisticsAsync();
            var tenants = await _adminService.GetTenantsAsync();
            model.RecentTenants = tenants.Take(5).Select(t => new TenantSummaryViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                CompanyCount = t.Companies.Count,
                IsActive = t.IsActive,
                SubscriptionEndDate = t.SubscriptionEndDate
            }).ToList();
        }
        else
        {
            var tenantId = await _tenantService.GetCurrentTenantIdAsync();
            if (tenantId.HasValue)
            {
                model.CurrentTenantStats = await _adminService.GetTenantStatisticsAsync(tenantId.Value);
            }
        }

        return View(model);
    }

    // ==================== Tenant Management ====================

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Tenants(bool includeInactive = false)
    {
        var tenants = await _adminService.GetTenantsAsync(includeInactive);

        var model = new TenantListViewModel
        {
            IncludeInactive = includeInactive,
            Tenants = tenants.Select(t => new TenantItemViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                ContactEmail = t.ContactEmail,
                CompanyCount = t.Companies.Count,
                BranchCount = t.Companies.SelectMany(c => c.Branches).Count(),
                IsActive = t.IsActive,
                SubscriptionPlan = t.SubscriptionPlan,
                SubscriptionEndDate = t.SubscriptionEndDate
            }).ToList()
        };

        // Get user counts
        foreach (var tenant in model.Tenants)
        {
            tenant.UserCount = await _context.Users.CountAsync(u => u.TenantId == tenant.Id);
        }

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult CreateTenant()
    {
        return View("TenantForm", new TenantFormViewModel());
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTenant(TenantFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("TenantForm", model);
        }

        var tenant = new Tenant
        {
            Name = model.Name,
            Code = model.Code,
            Description = model.Description,
            ContactEmail = model.ContactEmail,
            ContactPhone = model.ContactPhone,
            Address = model.Address,
            LogoPath = model.LogoPath,
            PrimaryColor = model.PrimaryColor,
            SecondaryColor = model.SecondaryColor,
            IsActive = model.IsActive,
            SubscriptionStartDate = model.SubscriptionStartDate,
            SubscriptionEndDate = model.SubscriptionEndDate,
            SubscriptionPlan = model.SubscriptionPlan,
            MaxCompanies = model.MaxCompanies,
            MaxBranchesPerCompany = model.MaxBranchesPerCompany,
            MaxUsersPerTenant = model.MaxUsersPerTenant
        };

        await _tenantService.CreateTenantAsync(tenant);
        TempData["Success"] = "Tenant created successfully.";

        return RedirectToAction(nameof(Tenants));
    }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> EditTenant(int id)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        var model = new TenantFormViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            Description = tenant.Description,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            Address = tenant.Address,
            LogoPath = tenant.LogoPath,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive,
            SubscriptionStartDate = tenant.SubscriptionStartDate,
            SubscriptionEndDate = tenant.SubscriptionEndDate,
            SubscriptionPlan = tenant.SubscriptionPlan,
            MaxCompanies = tenant.MaxCompanies,
            MaxBranchesPerCompany = tenant.MaxBranchesPerCompany,
            MaxUsersPerTenant = tenant.MaxUsersPerTenant
        };

        return View("TenantForm", model);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTenant(TenantFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("TenantForm", model);
        }

        var tenant = await _context.Tenants.FindAsync(model.Id);
        if (tenant == null)
        {
            return NotFound();
        }

        tenant.Name = model.Name;
        tenant.Code = model.Code;
        tenant.Description = model.Description;
        tenant.ContactEmail = model.ContactEmail;
        tenant.ContactPhone = model.ContactPhone;
        tenant.Address = model.Address;
        tenant.LogoPath = model.LogoPath;
        tenant.PrimaryColor = model.PrimaryColor;
        tenant.SecondaryColor = model.SecondaryColor;
        tenant.IsActive = model.IsActive;
        tenant.SubscriptionStartDate = model.SubscriptionStartDate;
        tenant.SubscriptionEndDate = model.SubscriptionEndDate;
        tenant.SubscriptionPlan = model.SubscriptionPlan;
        tenant.MaxCompanies = model.MaxCompanies;
        tenant.MaxBranchesPerCompany = model.MaxBranchesPerCompany;
        tenant.MaxUsersPerTenant = model.MaxUsersPerTenant;

        await _tenantService.UpdateTenantAsync(tenant);
        TempData["Success"] = "Tenant updated successfully.";

        return RedirectToAction(nameof(Tenants));
    }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> TenantDetails(int id)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        var stats = await _adminService.GetTenantStatisticsAsync(id);
        var companies = await _adminService.GetCompaniesAsync(id);

        var model = new TenantDetailsViewModel
        {
            Tenant = tenant,
            Statistics = stats,
            Companies = companies.Select(c => new CompanyItemViewModel
            {
                Id = c.Id,
                TenantId = c.TenantId,
                TenantName = tenant.Name,
                Name = c.Name,
                Code = c.Code,
                City = c.City,
                BranchCount = c.Branches.Count,
                IsActive = c.IsActive
            }).ToList()
        };

        // Get user counts for companies
        foreach (var company in model.Companies)
        {
            company.UserCount = await _context.Users.CountAsync(u => u.CompanyId == company.Id);
        }

        return View(model);
    }

    // ==================== Company Management ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> Companies(int? tenantId, bool includeInactive = false)
    {
        List<Company> companies;

        if (await _tenantService.IsSuperAdminAsync())
        {
            if (tenantId.HasValue)
            {
                companies = await _adminService.GetCompaniesAsync(tenantId.Value, includeInactive);
            }
            else
            {
                companies = await _context.Companies
                    .Include(c => c.Tenant)
                    .Include(c => c.Branches)
                    .Where(c => includeInactive || c.IsActive)
                    .ToListAsync();
            }
        }
        else
        {
            var currentTenantId = await _tenantService.GetCurrentTenantIdAsync();
            if (!currentTenantId.HasValue)
            {
                return Forbid();
            }
            companies = await _adminService.GetCompaniesAsync(currentTenantId.Value, includeInactive);
            tenantId = currentTenantId;
        }

        var model = new CompanyListViewModel
        {
            TenantId = tenantId,
            IncludeInactive = includeInactive,
            Companies = companies.Select(c => new CompanyItemViewModel
            {
                Id = c.Id,
                TenantId = c.TenantId,
                TenantName = c.Tenant?.Name ?? "",
                Name = c.Name,
                Code = c.Code,
                City = c.City,
                BranchCount = c.Branches.Count,
                IsActive = c.IsActive
            }).ToList()
        };

        // Get user counts
        foreach (var company in model.Companies)
        {
            company.UserCount = await _context.Users.CountAsync(u => u.CompanyId == company.Id);
        }

        if (tenantId.HasValue)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId.Value);
            model.TenantName = tenant?.Name;
        }

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> CreateCompany(int? tenantId)
    {
        var model = new CompanyFormViewModel
        {
            TenantId = tenantId ?? 0,
            AvailableTenants = await GetAvailableTenantsAsync()
        };

        if (!await _tenantService.IsSuperAdminAsync())
        {
            model.TenantId = (await _tenantService.GetCurrentTenantIdAsync()) ?? 0;
        }

        return View("CompanyForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CompanyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTenants = await GetAvailableTenantsAsync();
            return View("CompanyForm", model);
        }

        var company = new Company
        {
            TenantId = model.TenantId,
            Name = model.Name,
            Code = model.Code,
            TradeLicenseNumber = model.TradeLicenseNumber,
            TaxRegistrationNumber = model.TaxRegistrationNumber,
            Description = model.Description,
            ContactEmail = model.ContactEmail,
            ContactPhone = model.ContactPhone,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            PostalCode = model.PostalCode,
            LogoPath = model.LogoPath,
            Website = model.Website,
            PrimaryColor = model.PrimaryColor,
            SecondaryColor = model.SecondaryColor,
            Currency = model.Currency,
            Timezone = model.Timezone,
            IsActive = model.IsActive
        };

        await _companyService.CreateCompanyAsync(company);
        TempData["Success"] = "Company created successfully.";

        return RedirectToAction(nameof(Companies), new { tenantId = model.TenantId });
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> EditCompany(int id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            return NotFound();
        }

        var model = new CompanyFormViewModel
        {
            Id = company.Id,
            TenantId = company.TenantId,
            Name = company.Name,
            Code = company.Code,
            TradeLicenseNumber = company.TradeLicenseNumber,
            TaxRegistrationNumber = company.TaxRegistrationNumber,
            Description = company.Description,
            ContactEmail = company.ContactEmail,
            ContactPhone = company.ContactPhone,
            Address = company.Address,
            City = company.City,
            Country = company.Country,
            PostalCode = company.PostalCode,
            LogoPath = company.LogoPath,
            Website = company.Website,
            PrimaryColor = company.PrimaryColor,
            SecondaryColor = company.SecondaryColor,
            Currency = company.Currency,
            Timezone = company.Timezone,
            IsActive = company.IsActive,
            AvailableTenants = await GetAvailableTenantsAsync()
        };

        return View("CompanyForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCompany(CompanyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTenants = await GetAvailableTenantsAsync();
            return View("CompanyForm", model);
        }

        var company = await _context.Companies.FindAsync(model.Id);
        if (company == null)
        {
            return NotFound();
        }

        company.Name = model.Name;
        company.Code = model.Code;
        company.TradeLicenseNumber = model.TradeLicenseNumber;
        company.TaxRegistrationNumber = model.TaxRegistrationNumber;
        company.Description = model.Description;
        company.ContactEmail = model.ContactEmail;
        company.ContactPhone = model.ContactPhone;
        company.Address = model.Address;
        company.City = model.City;
        company.Country = model.Country;
        company.PostalCode = model.PostalCode;
        company.LogoPath = model.LogoPath;
        company.Website = model.Website;
        company.PrimaryColor = model.PrimaryColor;
        company.SecondaryColor = model.SecondaryColor;
        company.Currency = model.Currency;
        company.Timezone = model.Timezone;
        company.IsActive = model.IsActive;

        await _companyService.UpdateCompanyAsync(company);
        TempData["Success"] = "Company updated successfully.";

        return RedirectToAction(nameof(Companies), new { tenantId = company.TenantId });
    }

    // ==================== Branch Management ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> Branches(int? companyId, bool includeInactive = false)
    {
        List<Branch> branches;

        if (companyId.HasValue)
        {
            branches = await _adminService.GetBranchesAsync(companyId.Value, includeInactive);
        }
        else
        {
            var companies = await _companyService.GetUserCompaniesAsync();
            var companyIds = companies.Select(c => c.Id).ToList();
            branches = await _context.Branches
                .Include(b => b.Company)
                .ThenInclude(c => c.Tenant)
                .Where(b => companyIds.Contains(b.CompanyId) && (includeInactive || b.IsActive))
                .ToListAsync();
        }

        var model = new BranchListViewModel
        {
            CompanyId = companyId,
            IncludeInactive = includeInactive,
            Branches = branches.Select(b => new BranchItemViewModel
            {
                Id = b.Id,
                CompanyId = b.CompanyId,
                CompanyName = b.Company?.Name ?? "",
                TenantName = b.Company?.Tenant?.Name ?? "",
                Name = b.Name,
                Code = b.Code,
                City = b.City,
                Phone = b.Phone,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive
            }).ToList()
        };

        // Get user counts
        foreach (var branch in model.Branches)
        {
            branch.UserCount = await _context.UserBranches.CountAsync(ub => ub.BranchId == branch.Id);
        }

        if (companyId.HasValue)
        {
            var company = await _context.Companies.FindAsync(companyId.Value);
            model.CompanyName = company?.Name;
        }

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> CreateBranch(int? companyId)
    {
        var model = new BranchFormViewModel
        {
            CompanyId = companyId ?? 0,
            AvailableCompanies = await GetAvailableCompaniesAsync()
        };

        return View("BranchForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBranch(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync();
            return View("BranchForm", model);
        }

        var branch = new Branch
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            Address = model.Address,
            City = model.City,
            Phone = model.Phone,
            Email = model.Email,
            LogoPath = model.LogoPath,
            PrimaryColor = model.PrimaryColor,
            SecondaryColor = model.SecondaryColor,
            Timezone = model.Timezone,
            Currency = model.Currency,
            IsActive = model.IsActive,
            IsMainBranch = model.IsMainBranch,
            OpeningTime = model.OpeningTime,
            ClosingTime = model.ClosingTime,
            WorkingDays = model.WorkingDays
        };

        await _adminService.CreateBranchAsync(branch);
        TempData["Success"] = "Branch created successfully.";

        return RedirectToAction(nameof(Branches), new { companyId = model.CompanyId });
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> EditBranch(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (branch == null)
        {
            return NotFound();
        }

        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            return Forbid();
        }

        var model = new BranchFormViewModel
        {
            Id = branch.Id,
            CompanyId = branch.CompanyId,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            City = branch.City,
            Phone = branch.Phone,
            Email = branch.Email,
            LogoPath = branch.LogoPath,
            PrimaryColor = branch.PrimaryColor,
            SecondaryColor = branch.SecondaryColor,
            Timezone = branch.Timezone,
            Currency = branch.Currency,
            IsActive = branch.IsActive,
            IsMainBranch = branch.IsMainBranch,
            OpeningTime = branch.OpeningTime,
            ClosingTime = branch.ClosingTime,
            WorkingDays = branch.WorkingDays,
            AvailableCompanies = await GetAvailableCompaniesAsync()
        };

        return View("BranchForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBranch(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync();
            return View("BranchForm", model);
        }

        var branch = await _context.Branches.FindAsync(model.Id);
        if (branch == null)
        {
            return NotFound();
        }

        if (!await _companyService.HasAccessToCompanyAsync(branch.CompanyId))
        {
            return Forbid();
        }

        branch.Name = model.Name;
        branch.Code = model.Code;
        branch.Address = model.Address;
        branch.City = model.City;
        branch.Phone = model.Phone;
        branch.Email = model.Email;
        branch.LogoPath = model.LogoPath;
        branch.PrimaryColor = model.PrimaryColor;
        branch.SecondaryColor = model.SecondaryColor;
        branch.Timezone = model.Timezone;
        branch.Currency = model.Currency;
        branch.IsActive = model.IsActive;
        branch.IsMainBranch = model.IsMainBranch;
        branch.OpeningTime = model.OpeningTime;
        branch.ClosingTime = model.ClosingTime;
        branch.WorkingDays = model.WorkingDays;

        await _adminService.UpdateBranchAsync(branch);
        TempData["Success"] = "Branch updated successfully.";

        return RedirectToAction(nameof(Branches), new { companyId = branch.CompanyId });
    }

    // ==================== User Management ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin,BranchAdmin")]
    public async Task<IActionResult> Users(int? tenantId, int? companyId, int? branchId, bool includeInactive = false)
    {
        List<ApplicationUser> users;

        if (branchId.HasValue)
        {
            users = await _adminService.GetBranchUsersAsync(branchId.Value, includeInactive);
        }
        else if (companyId.HasValue)
        {
            users = await _adminService.GetCompanyUsersAsync(companyId.Value, includeInactive);
        }
        else if (tenantId.HasValue)
        {
            users = await _adminService.GetTenantUsersAsync(tenantId.Value, includeInactive);
        }
        else if (await _tenantService.IsSuperAdminAsync())
        {
            users = await _context.Users
                .Include(u => u.Tenant)
                .Include(u => u.Company)
                .Include(u => u.PrimaryBranch)
                .Where(u => includeInactive || u.IsActive)
                .ToListAsync();
        }
        else
        {
            var currentTenantId = await _tenantService.GetCurrentTenantIdAsync();
            if (!currentTenantId.HasValue)
            {
                return Forbid();
            }
            users = await _adminService.GetTenantUsersAsync(currentTenantId.Value, includeInactive);
        }

        var model = new UserListViewModel
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            IncludeInactive = includeInactive,
            Users = new List<UserItemViewModel>()
        };

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Users.Add(new UserItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName ?? $"{user.FirstName} {user.LastName}".Trim(),
                TenantName = user.Tenant?.Name,
                CompanyName = user.Company?.Name,
                PrimaryBranchName = user.PrimaryBranch?.Name,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                LastLoginAt = user.LastLoginAt
            });
        }

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> CreateUser()
    {
        var model = new UserFormViewModel
        {
            AvailableTenants = await GetAvailableTenantsAsync(),
            AvailableCompanies = await GetAvailableCompaniesAsync(),
            AvailableBranches = await GetAvailableBranchesAsync(),
            AvailableRoles = await GetAvailableRolesAsync()
        };

        return View("UserForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserFormViewModel model)
    {
        if (string.IsNullOrEmpty(model.Password))
        {
            ModelState.AddModelError("Password", "Password is required for new users.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableTenants = await GetAvailableTenantsAsync();
            model.AvailableCompanies = await GetAvailableCompaniesAsync();
            model.AvailableBranches = await GetAvailableBranchesAsync();
            model.AvailableRoles = await GetAvailableRolesAsync();
            return View("UserForm", model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName,
            LastName = model.LastName,
            DisplayName = model.DisplayName ?? $"{model.FirstName} {model.LastName}".Trim(),
            PhoneNumber = model.PhoneNumber,
            TenantId = model.TenantId,
            CompanyId = model.CompanyId,
            PrimaryBranchId = model.PrimaryBranchId,
            IsActive = model.IsActive,
            IsSuperAdmin = model.IsSuperAdmin && await _tenantService.IsSuperAdminAsync()
        };

        await _adminService.CreateUserAsync(user, model.Password!, model.SelectedRoles, model.SelectedBranchIds);
        TempData["Success"] = "User created successfully.";

        return RedirectToAction(nameof(Users));
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _context.Users
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserFormViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            CompanyId = user.CompanyId,
            PrimaryBranchId = user.PrimaryBranchId,
            SelectedRoles = roles.ToList(),
            SelectedBranchIds = user.UserBranches.Select(ub => ub.BranchId).ToList(),
            IsActive = user.IsActive,
            IsSuperAdmin = user.IsSuperAdmin,
            AvailableTenants = await GetAvailableTenantsAsync(),
            AvailableCompanies = await GetAvailableCompaniesAsync(),
            AvailableBranches = await GetAvailableBranchesAsync(),
            AvailableRoles = await GetAvailableRolesAsync()
        };

        return View("UserForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserFormViewModel model)
    {
        // Password is optional for edit
        ModelState.Remove("Password");
        ModelState.Remove("ConfirmPassword");

        if (!ModelState.IsValid)
        {
            model.AvailableTenants = await GetAvailableTenantsAsync();
            model.AvailableCompanies = await GetAvailableCompaniesAsync();
            model.AvailableBranches = await GetAvailableBranchesAsync();
            model.AvailableRoles = await GetAvailableRolesAsync();
            return View("UserForm", model);
        }

        var user = await _userManager.FindByIdAsync(model.Id!);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.DisplayName = model.DisplayName ?? $"{model.FirstName} {model.LastName}".Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;

        if (await _tenantService.IsSuperAdminAsync())
        {
            user.IsSuperAdmin = model.IsSuperAdmin;
        }

        await _userManager.UpdateAsync(user);
        await _adminService.UpdateUserAssignmentsAsync(user.Id, model.TenantId, model.CompanyId, model.PrimaryBranchId, model.SelectedBranchIds);
        await _adminService.UpdateUserRolesAsync(user.Id, model.SelectedRoles);

        // Update password if provided
        if (!string.IsNullOrEmpty(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, model.Password);
        }

        TempData["Success"] = "User updated successfully.";

        return RedirectToAction(nameof(Users));
    }

    // ==================== Settings ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> Settings(int? tenantId)
    {
        int effectiveTenantId;

        if (await _tenantService.IsSuperAdminAsync() && tenantId.HasValue)
        {
            effectiveTenantId = tenantId.Value;
        }
        else
        {
            var currentTenantId = await _tenantService.GetCurrentTenantIdAsync();
            if (!currentTenantId.HasValue)
            {
                return Forbid();
            }
            effectiveTenantId = currentTenantId.Value;
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId);
        if (tenant == null)
        {
            return NotFound();
        }

        var settings = tenant.Settings ?? new TenantSettings { TenantId = effectiveTenantId };

        var model = new TenantSettingsFormViewModel
        {
            TenantId = effectiveTenantId,
            TenantName = tenant.Name,
            DefaultLanguage = settings.DefaultLanguage,
            DefaultCurrency = settings.DefaultCurrency,
            DefaultTimezone = settings.DefaultTimezone,
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            EnableMultiCurrency = settings.EnableMultiCurrency,
            EnableMultiLanguage = settings.EnableMultiLanguage,
            RequireApprovalForExpenses = settings.RequireApprovalForExpenses,
            ExpenseApprovalThreshold = settings.ExpenseApprovalThreshold,
            DefaultAppointmentDurationMinutes = settings.DefaultAppointmentDurationMinutes,
            EnableOnlineBooking = settings.EnableOnlineBooking,
            SendAppointmentReminders = settings.SendAppointmentReminders,
            ReminderHoursBeforeAppointment = settings.ReminderHoursBeforeAppointment,
            InvoicePrefix = settings.InvoicePrefix,
            QuotationPrefix = settings.QuotationPrefix,
            PurchaseOrderPrefix = settings.PurchaseOrderPrefix,
            SalePrefix = settings.SalePrefix,
            InvoiceStartNumber = settings.InvoiceStartNumber,
            DefaultTaxRate = settings.DefaultTaxRate,
            ShowTaxOnInvoice = settings.ShowTaxOnInvoice,
            EnableLabModule = settings.EnableLabModule,
            EnableInventoryModule = settings.EnableInventoryModule,
            EnableHRModule = settings.EnableHRModule,
            EnableFinanceModule = settings.EnableFinanceModule,
            EnableProcurementModule = settings.EnableProcurementModule,
            EnableSalesModule = settings.EnableSalesModule,
            EnableAnalyticsModule = settings.EnableAnalyticsModule,
            EnablePatientPortal = settings.EnablePatientPortal,
            EnableAuditLogging = settings.EnableAuditLogging,
            AuditLogRetentionDays = settings.AuditLogRetentionDays
        };

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(TenantSettingsFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingSettings = await _context.TenantSettings
            .FirstOrDefaultAsync(ts => ts.TenantId == model.TenantId);

        if (existingSettings == null)
        {
            existingSettings = new TenantSettings { TenantId = model.TenantId };
            _context.TenantSettings.Add(existingSettings);
        }

        existingSettings.DefaultLanguage = model.DefaultLanguage;
        existingSettings.DefaultCurrency = model.DefaultCurrency;
        existingSettings.DefaultTimezone = model.DefaultTimezone;
        existingSettings.DateFormat = model.DateFormat;
        existingSettings.TimeFormat = model.TimeFormat;
        existingSettings.EnableMultiCurrency = model.EnableMultiCurrency;
        existingSettings.EnableMultiLanguage = model.EnableMultiLanguage;
        existingSettings.RequireApprovalForExpenses = model.RequireApprovalForExpenses;
        existingSettings.ExpenseApprovalThreshold = model.ExpenseApprovalThreshold;
        existingSettings.DefaultAppointmentDurationMinutes = model.DefaultAppointmentDurationMinutes;
        existingSettings.EnableOnlineBooking = model.EnableOnlineBooking;
        existingSettings.SendAppointmentReminders = model.SendAppointmentReminders;
        existingSettings.ReminderHoursBeforeAppointment = model.ReminderHoursBeforeAppointment;
        existingSettings.InvoicePrefix = model.InvoicePrefix;
        existingSettings.QuotationPrefix = model.QuotationPrefix;
        existingSettings.PurchaseOrderPrefix = model.PurchaseOrderPrefix;
        existingSettings.SalePrefix = model.SalePrefix;
        existingSettings.InvoiceStartNumber = model.InvoiceStartNumber;
        existingSettings.DefaultTaxRate = model.DefaultTaxRate;
        existingSettings.ShowTaxOnInvoice = model.ShowTaxOnInvoice;
        existingSettings.EnableLabModule = model.EnableLabModule;
        existingSettings.EnableInventoryModule = model.EnableInventoryModule;
        existingSettings.EnableHRModule = model.EnableHRModule;
        existingSettings.EnableFinanceModule = model.EnableFinanceModule;
        existingSettings.EnableProcurementModule = model.EnableProcurementModule;
        existingSettings.EnableSalesModule = model.EnableSalesModule;
        existingSettings.EnableAnalyticsModule = model.EnableAnalyticsModule;
        existingSettings.EnablePatientPortal = model.EnablePatientPortal;
        existingSettings.EnableAuditLogging = model.EnableAuditLogging;
        existingSettings.AuditLogRetentionDays = model.AuditLogRetentionDays;

        await _tenantService.UpdateTenantSettingsAsync(existingSettings);
        TempData["Success"] = "Settings saved successfully.";

        return RedirectToAction(nameof(Settings), new { tenantId = model.TenantId });
    }

    // ==================== Communication Settings (WhatsApp & Email) ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> CommunicationSettings(int? tenantId)
    {
        int effectiveTenantId;

        if (await _tenantService.IsSuperAdminAsync() && tenantId.HasValue)
        {
            effectiveTenantId = tenantId.Value;
        }
        else
        {
            var currentTenantId = await _tenantService.GetCurrentTenantIdAsync();
            if (!currentTenantId.HasValue)
            {
                return Forbid();
            }
            effectiveTenantId = currentTenantId.Value;
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId);
        if (tenant == null)
        {
            return NotFound();
        }

        var settings = tenant.Settings ?? new TenantSettings { TenantId = effectiveTenantId };

        var model = new CommunicationSettingsViewModel
        {
            TenantId = effectiveTenantId,
            ConfigurationLevel = "Tenant",

            // Email settings
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SmtpUsername = settings.SmtpUsername,
            SmtpPassword = null, // Don't expose password
            SmtpUseSsl = settings.SmtpUseSsl,
            DefaultSenderEmail = settings.DefaultSenderEmail,
            DefaultSenderName = settings.DefaultSenderName,

            // WhatsApp settings
            EnableWhatsApp = settings.EnableWhatsApp,
            WhatsAppProvider = settings.WhatsAppProvider,
            WhatsAppAccountSid = settings.WhatsAppAccountSid,
            WhatsAppAuthToken = null, // Don't expose token
            WhatsAppPhoneNumber = settings.WhatsAppPhoneNumber,
            WhatsAppBusinessApiToken = null, // Don't expose token
            WhatsAppBusinessPhoneNumberId = settings.WhatsAppBusinessPhoneNumberId,

            // Reminder settings
            SendAppointmentReminders = settings.SendAppointmentReminders,
            ReminderHoursBeforeAppointment = settings.ReminderHoursBeforeAppointment
        };

        ViewBag.TenantName = tenant.Name;
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CommunicationSettings(CommunicationSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(model.TenantId!.Value);
            ViewBag.TenantName = tenant?.Name;
            return View(model);
        }

        var existingSettings = await _context.TenantSettings
            .FirstOrDefaultAsync(ts => ts.TenantId == model.TenantId);

        if (existingSettings == null)
        {
            existingSettings = new TenantSettings { TenantId = model.TenantId!.Value };
            _context.TenantSettings.Add(existingSettings);
        }

        // Update email settings
        if (!string.IsNullOrEmpty(model.SmtpHost))
        {
            existingSettings.SmtpHost = model.SmtpHost;
            existingSettings.SmtpPort = model.SmtpPort ?? 587;
            existingSettings.SmtpUsername = model.SmtpUsername;
            existingSettings.SmtpUseSsl = model.SmtpUseSsl ?? true;
            existingSettings.DefaultSenderEmail = model.DefaultSenderEmail;
            existingSettings.DefaultSenderName = model.DefaultSenderName;

            // Only update password if provided
            if (!string.IsNullOrEmpty(model.SmtpPassword))
            {
                existingSettings.SmtpPassword = model.SmtpPassword;
            }
        }

        // Update WhatsApp settings
        existingSettings.EnableWhatsApp = model.EnableWhatsApp ?? false;
        existingSettings.WhatsAppProvider = model.WhatsAppProvider;
        existingSettings.WhatsAppAccountSid = model.WhatsAppAccountSid;
        existingSettings.WhatsAppPhoneNumber = model.WhatsAppPhoneNumber;
        existingSettings.WhatsAppBusinessPhoneNumberId = model.WhatsAppBusinessPhoneNumberId;

        // Only update tokens if provided
        if (!string.IsNullOrEmpty(model.WhatsAppAuthToken))
        {
            existingSettings.WhatsAppAuthToken = model.WhatsAppAuthToken;
        }
        if (!string.IsNullOrEmpty(model.WhatsAppBusinessApiToken))
        {
            existingSettings.WhatsAppBusinessApiToken = model.WhatsAppBusinessApiToken;
        }

        // Update reminder settings
        existingSettings.SendAppointmentReminders = model.SendAppointmentReminders ?? false;
        existingSettings.ReminderHoursBeforeAppointment = model.ReminderHoursBeforeAppointment ?? 24;

        await _tenantService.UpdateTenantSettingsAsync(existingSettings);
        TempData["Success"] = "Communication settings saved successfully.";

        return RedirectToAction(nameof(CommunicationSettings), new { tenantId = model.TenantId });
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> CompanyCommunicationSettings(int companyId)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            return Forbid();
        }

        var company = await _context.Companies
            .Include(c => c.Settings)
            .Include(c => c.Tenant)
                .ThenInclude(t => t.Settings)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
        {
            return NotFound();
        }

        var companySettings = company.Settings;
        var tenantSettings = company.Tenant?.Settings;

        var model = new CommunicationSettingsViewModel
        {
            CompanyId = companyId,
            ConfigurationLevel = "Company",

            // Email settings (show company override or tenant default)
            SmtpHost = companySettings?.SmtpHost ?? tenantSettings?.SmtpHost,
            SmtpPort = companySettings?.SmtpPort ?? tenantSettings?.SmtpPort,
            SmtpUsername = companySettings?.SmtpUsername ?? tenantSettings?.SmtpUsername,
            SmtpPassword = null, // Don't expose password
            SmtpUseSsl = companySettings?.SmtpUseSsl ?? tenantSettings?.SmtpUseSsl,
            DefaultSenderEmail = companySettings?.DefaultSenderEmail ?? tenantSettings?.DefaultSenderEmail,
            DefaultSenderName = companySettings?.DefaultSenderName ?? tenantSettings?.DefaultSenderName,

            // WhatsApp settings (show company override or tenant default)
            EnableWhatsApp = companySettings?.EnableWhatsApp ?? tenantSettings?.EnableWhatsApp,
            WhatsAppProvider = companySettings?.WhatsAppProvider ?? tenantSettings?.WhatsAppProvider,
            WhatsAppAccountSid = companySettings?.WhatsAppAccountSid ?? tenantSettings?.WhatsAppAccountSid,
            WhatsAppAuthToken = null, // Don't expose token
            WhatsAppPhoneNumber = companySettings?.WhatsAppPhoneNumber ?? tenantSettings?.WhatsAppPhoneNumber,
            WhatsAppBusinessApiToken = null, // Don't expose token
            WhatsAppBusinessPhoneNumberId = companySettings?.WhatsAppBusinessPhoneNumberId ?? tenantSettings?.WhatsAppBusinessPhoneNumberId,

            // Reminder settings
            SendAppointmentReminders = companySettings?.SendAppointmentReminders ?? tenantSettings?.SendAppointmentReminders,
            ReminderHoursBeforeAppointment = companySettings?.ReminderHoursBeforeAppointment ?? tenantSettings?.ReminderHoursBeforeAppointment
        };

        ViewBag.CompanyName = company.Name;
        ViewBag.TenantName = company.Tenant?.Name;
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanyCommunicationSettings(CommunicationSettingsViewModel model)
    {
        if (!await _companyService.HasAccessToCompanyAsync(model.CompanyId!.Value))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var company = await _companyService.GetCompanyByIdAsync(model.CompanyId.Value);
            ViewBag.CompanyName = company?.Name;
            return View(model);
        }

        var existingSettings = await _context.CompanySettings
            .FirstOrDefaultAsync(cs => cs.CompanyId == model.CompanyId);

        if (existingSettings == null)
        {
            existingSettings = new CompanySettings { CompanyId = model.CompanyId.Value };
            _context.CompanySettings.Add(existingSettings);
        }

        // Update email settings (null means use tenant default)
        existingSettings.SmtpHost = model.SmtpHost;
        existingSettings.SmtpPort = model.SmtpPort;
        existingSettings.SmtpUsername = model.SmtpUsername;
        existingSettings.SmtpUseSsl = model.SmtpUseSsl;
        existingSettings.DefaultSenderEmail = model.DefaultSenderEmail;
        existingSettings.DefaultSenderName = model.DefaultSenderName;

        // Only update password if provided
        if (!string.IsNullOrEmpty(model.SmtpPassword))
        {
            existingSettings.SmtpPassword = model.SmtpPassword;
        }

        // Update WhatsApp settings
        existingSettings.EnableWhatsApp = model.EnableWhatsApp;
        existingSettings.WhatsAppProvider = model.WhatsAppProvider;
        existingSettings.WhatsAppAccountSid = model.WhatsAppAccountSid;
        existingSettings.WhatsAppPhoneNumber = model.WhatsAppPhoneNumber;
        existingSettings.WhatsAppBusinessPhoneNumberId = model.WhatsAppBusinessPhoneNumberId;

        // Only update tokens if provided
        if (!string.IsNullOrEmpty(model.WhatsAppAuthToken))
        {
            existingSettings.WhatsAppAuthToken = model.WhatsAppAuthToken;
        }
        if (!string.IsNullOrEmpty(model.WhatsAppBusinessApiToken))
        {
            existingSettings.WhatsAppBusinessApiToken = model.WhatsAppBusinessApiToken;
        }

        // Update reminder settings
        existingSettings.SendAppointmentReminders = model.SendAppointmentReminders;
        existingSettings.ReminderHoursBeforeAppointment = model.ReminderHoursBeforeAppointment;

        existingSettings.LastModifiedAt = DateTime.UtcNow;
        existingSettings.LastModifiedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Company communication settings saved successfully.";

        return RedirectToAction(nameof(CompanyCommunicationSettings), new { companyId = model.CompanyId });
    }

    // ==================== Company Authentication Settings ====================

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> AuthSettings(int companyId)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            return Forbid();
        }

        var company = await _companyService.GetCompanyByIdAsync(companyId);
        if (company == null)
        {
            return NotFound();
        }

        var authSettings = await _authConfigService.GetAuthSettingsAsync(companyId);
        var providers = await _authConfigService.GetIdentityProvidersAsync(companyId);

        var model = new CompanyAuthSettingsViewModel
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            CompanyCode = company.Code,

            // Auth settings
            AuthMode = authSettings?.AuthMode ?? AuthMode.LocalOnly,
            AllowLocalLogin = authSettings?.AllowLocalLogin ?? true,
            AllowExternalLogin = authSettings?.AllowExternalLogin ?? false,
            AutoProvisionUsers = authSettings?.AutoProvisionUsers ?? false,
            DefaultRoleOnAutoProvision = authSettings?.DefaultRoleOnAutoProvision ?? "User",
            DefaultExternalProviderName = authSettings?.DefaultExternalProviderName,
            IsEnabled = authSettings?.IsEnabled ?? false,
            LoginPageMessage = authSettings?.LoginPageMessage,
            LoginPageMessageAr = authSettings?.LoginPageMessageAr,
            PostLoginRedirectUrl = authSettings?.PostLoginRedirectUrl,
            PostLogoutRedirectUrl = authSettings?.PostLogoutRedirectUrl,

            // Identity providers
            IdentityProviders = providers.Select(p => new IdentityProviderItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Type = p.Type,
                IsDefault = p.IsDefault,
                IsEnabled = p.IsEnabled,
                DisplayOrder = p.DisplayOrder
            }).ToList(),

            // Available roles for auto-provision
            AvailableRoles = await GetAvailableRolesAsync()
        };

        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AuthSettings(CompanyAuthSettingsViewModel model)
    {
        if (!await _companyService.HasAccessToCompanyAsync(model.CompanyId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await GetAvailableRolesAsync();
            model.IdentityProviders = (await _authConfigService.GetIdentityProvidersAsync(model.CompanyId))
                .Select(p => new IdentityProviderItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Type = p.Type,
                    IsDefault = p.IsDefault,
                    IsEnabled = p.IsEnabled
                }).ToList();
            return View(model);
        }

        var settings = new CompanyAuthSettings
        {
            CompanyId = model.CompanyId,
            AuthMode = model.AuthMode,
            AllowLocalLogin = model.AllowLocalLogin,
            AllowExternalLogin = model.AllowExternalLogin,
            AutoProvisionUsers = model.AutoProvisionUsers,
            DefaultRoleOnAutoProvision = model.DefaultRoleOnAutoProvision,
            DefaultExternalProviderName = model.DefaultExternalProviderName,
            IsEnabled = model.IsEnabled,
            LoginPageMessage = model.LoginPageMessage,
            LoginPageMessageAr = model.LoginPageMessageAr,
            PostLoginRedirectUrl = model.PostLoginRedirectUrl,
            PostLogoutRedirectUrl = model.PostLogoutRedirectUrl,
            UpdatedBy = User.Identity?.Name
        };

        await _authConfigService.SaveAuthSettingsAsync(settings);
        TempData["Success"] = "Authentication settings saved successfully.";

        return RedirectToAction(nameof(AuthSettings), new { companyId = model.CompanyId });
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> CreateIdentityProvider(int companyId)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            return Forbid();
        }

        var company = await _companyService.GetCompanyByIdAsync(companyId);
        if (company == null)
        {
            return NotFound();
        }

        var model = new IdentityProviderFormViewModel
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            Type = IdentityProviderType.OIDC,
            IsEnabled = true,
            OidcUsePkce = true,
            OidcRequireHttpsMetadata = true,
            OidcGetClaimsFromUserInfoEndpoint = true,
            OidcScopes = "openid,profile,email",
            OidcResponseType = "code",
            SamlWantAssertionsSigned = true,
            SamlAllowedClockSkewMinutes = 5,
            WsFedRequireHttps = true,
            ButtonClass = "btn-outline-primary",
            IconClass = "bi bi-box-arrow-in-right"
        };

        return View("IdentityProviderForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> EditIdentityProvider(int id)
    {
        var provider = await _authConfigService.GetIdentityProviderByIdAsync(id);
        if (provider == null)
        {
            return NotFound();
        }

        if (!await _companyService.HasAccessToCompanyAsync(provider.CompanyId))
        {
            return Forbid();
        }

        var company = await _companyService.GetCompanyByIdAsync(provider.CompanyId);

        var model = new IdentityProviderFormViewModel
        {
            Id = provider.Id,
            CompanyId = provider.CompanyId,
            CompanyName = company?.Name ?? "",
            Name = provider.Name,
            DisplayName = provider.DisplayName,
            Type = provider.Type,
            IsDefault = provider.IsDefault,
            IsEnabled = provider.IsEnabled,
            DisplayOrder = provider.DisplayOrder,
            IconClass = provider.IconClass,
            ButtonClass = provider.ButtonClass,

            // OIDC
            OidcAuthority = provider.OidcAuthority,
            OidcClientId = provider.OidcClientId,
            OidcCallbackPath = provider.OidcCallbackPath,
            OidcSignedOutCallbackPath = provider.OidcSignedOutCallbackPath,
            OidcResponseType = provider.OidcResponseType,
            OidcUsePkce = provider.OidcUsePkce,
            OidcScopes = provider.OidcScopes,
            OidcRequireHttpsMetadata = provider.OidcRequireHttpsMetadata,
            OidcGetClaimsFromUserInfoEndpoint = provider.OidcGetClaimsFromUserInfoEndpoint,

            // SAML
            SamlEntityId = provider.SamlEntityId,
            SamlAcsPath = provider.SamlAcsPath,
            SamlSloPath = provider.SamlSloPath,
            SamlMetadataUrl = provider.SamlMetadataUrl,
            SamlIdpEntityId = provider.SamlIdpEntityId,
            SamlSignAuthnRequests = provider.SamlSignAuthnRequests,
            SamlWantAssertionsSigned = provider.SamlWantAssertionsSigned,
            SamlAllowedClockSkewMinutes = provider.SamlAllowedClockSkewMinutes,

            // WS-Fed
            WsFedMetadataAddress = provider.WsFedMetadataAddress,
            WsFedWtrealm = provider.WsFedWtrealm,
            WsFedReplyUrl = provider.WsFedReplyUrl,
            WsFedRequireHttps = provider.WsFedRequireHttps,

            // Claim mappings
            ClaimMappingEmail = provider.ClaimMappingEmail,
            ClaimMappingUpn = provider.ClaimMappingUpn,
            ClaimMappingName = provider.ClaimMappingName,
            ClaimMappingFirstName = provider.ClaimMappingFirstName,
            ClaimMappingLastName = provider.ClaimMappingLastName,
            ClaimMappingGroups = provider.ClaimMappingGroups,
            ExtraConfigJson = provider.ExtraConfigJson
        };

        return View("IdentityProviderForm", model);
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveIdentityProvider(IdentityProviderFormViewModel model)
    {
        if (!await _companyService.HasAccessToCompanyAsync(model.CompanyId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("IdentityProviderForm", model);
        }

        var provider = new CompanyIdentityProvider
        {
            Id = model.Id ?? 0,
            CompanyId = model.CompanyId,
            Name = model.Name,
            DisplayName = model.DisplayName,
            Type = model.Type,
            IsDefault = model.IsDefault,
            IsEnabled = model.IsEnabled,
            DisplayOrder = model.DisplayOrder,
            IconClass = model.IconClass,
            ButtonClass = model.ButtonClass,

            // OIDC
            OidcAuthority = model.OidcAuthority,
            OidcClientId = model.OidcClientId,
            OidcCallbackPath = model.OidcCallbackPath,
            OidcSignedOutCallbackPath = model.OidcSignedOutCallbackPath,
            OidcResponseType = model.OidcResponseType,
            OidcUsePkce = model.OidcUsePkce,
            OidcScopes = model.OidcScopes,
            OidcRequireHttpsMetadata = model.OidcRequireHttpsMetadata,
            OidcGetClaimsFromUserInfoEndpoint = model.OidcGetClaimsFromUserInfoEndpoint,

            // SAML
            SamlEntityId = model.SamlEntityId,
            SamlAcsPath = model.SamlAcsPath,
            SamlSloPath = model.SamlSloPath,
            SamlMetadataUrl = model.SamlMetadataUrl,
            SamlIdpEntityId = model.SamlIdpEntityId,
            SamlSignAuthnRequests = model.SamlSignAuthnRequests,
            SamlWantAssertionsSigned = model.SamlWantAssertionsSigned,
            SamlAllowedClockSkewMinutes = model.SamlAllowedClockSkewMinutes,

            // WS-Fed
            WsFedMetadataAddress = model.WsFedMetadataAddress,
            WsFedWtrealm = model.WsFedWtrealm,
            WsFedReplyUrl = model.WsFedReplyUrl,
            WsFedRequireHttps = model.WsFedRequireHttps,

            // Claim mappings
            ClaimMappingEmail = model.ClaimMappingEmail,
            ClaimMappingUpn = model.ClaimMappingUpn,
            ClaimMappingName = model.ClaimMappingName,
            ClaimMappingFirstName = model.ClaimMappingFirstName,
            ClaimMappingLastName = model.ClaimMappingLastName,
            ClaimMappingGroups = model.ClaimMappingGroups,
            ExtraConfigJson = model.ExtraConfigJson,

            UpdatedBy = User.Identity?.Name
        };

        // Encrypt client secret if provided
        if (!string.IsNullOrEmpty(model.OidcClientSecret))
        {
            provider.OidcClientSecretEncrypted = _encryptionService.Encrypt(model.OidcClientSecret);
        }
        else if (model.Id.HasValue)
        {
            // Preserve existing secret if not changed
            var existing = await _authConfigService.GetIdentityProviderByIdAsync(model.Id.Value);
            provider.OidcClientSecretEncrypted = existing?.OidcClientSecretEncrypted;
        }

        // Encrypt SAML certificate password if provided
        if (!string.IsNullOrEmpty(model.SamlSpCertificatePassword))
        {
            provider.SamlSpCertificatePasswordEncrypted = _encryptionService.Encrypt(model.SamlSpCertificatePassword);
        }
        else if (model.Id.HasValue)
        {
            var existing = await _authConfigService.GetIdentityProviderByIdAsync(model.Id.Value);
            provider.SamlSpCertificatePasswordEncrypted = existing?.SamlSpCertificatePasswordEncrypted;
        }

        provider.SamlSpCertificate = model.SamlSpCertificate;

        await _authConfigService.SaveIdentityProviderAsync(provider);
        TempData["Success"] = "Identity provider saved successfully.";

        return RedirectToAction(nameof(AuthSettings), new { companyId = model.CompanyId });
    }

    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIdentityProvider(int id, int companyId)
    {
        var provider = await _authConfigService.GetIdentityProviderByIdAsync(id);
        if (provider == null)
        {
            return NotFound();
        }

        if (!await _companyService.HasAccessToCompanyAsync(provider.CompanyId))
        {
            return Forbid();
        }

        await _authConfigService.DeleteIdentityProviderAsync(id);
        TempData["Success"] = "Identity provider deleted successfully.";

        return RedirectToAction(nameof(AuthSettings), new { companyId });
    }

    // ==================== Helper Methods ====================

    private async Task<List<TenantSelectItem>> GetAvailableTenantsAsync()
    {
        if (await _tenantService.IsSuperAdminAsync())
        {
            return await _context.Tenants
                .Where(t => t.IsActive)
                .Select(t => new TenantSelectItem { Id = t.Id, Name = t.Name })
                .ToListAsync();
        }

        var tenantId = await _tenantService.GetCurrentTenantIdAsync();
        if (tenantId.HasValue)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId.Value);
            if (tenant != null)
            {
                return new List<TenantSelectItem> { new() { Id = tenant.Id, Name = tenant.Name } };
            }
        }

        return new List<TenantSelectItem>();
    }

    private async Task<List<CompanySelectItem>> GetAvailableCompaniesAsync()
    {
        var companies = await _companyService.GetUserCompaniesAsync();
        return companies.Select(c => new CompanySelectItem
        {
            Id = c.Id,
            Name = c.Name,
            TenantId = c.TenantId,
            TenantName = c.Tenant?.Name ?? ""
        }).ToList();
    }

    private async Task<List<BranchSelectItem>> GetAvailableBranchesAsync()
    {
        var companies = await _companyService.GetUserCompaniesAsync();
        var branches = new List<BranchSelectItem>();

        foreach (var company in companies)
        {
            var companyBranches = await _companyService.GetCompanyBranchesAsync(company.Id);
            branches.AddRange(companyBranches.Select(b => new BranchSelectItem
            {
                Id = b.Id,
                Name = b.Name,
                CompanyId = company.Id,
                CompanyName = company.Name
            }));
        }

        return branches;
    }

    private async Task<List<string>> GetAvailableRolesAsync()
    {
        var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

        // Non-super admins cannot assign SuperAdmin role
        if (!await _tenantService.IsSuperAdminAsync())
        {
            allRoles.Remove("SuperAdmin");
        }

        return allRoles;
    }

    // ==================== API Endpoints for AJAX ====================

    [HttpGet]
    public async Task<IActionResult> GetCompaniesForTenant(int tenantId)
    {
        if (!await _tenantService.HasAccessToTenantAsync(tenantId))
        {
            return Forbid();
        }

        var companies = await _context.Companies
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Json(companies);
    }

    [HttpGet]
    public async Task<IActionResult> GetBranchesForCompany(int companyId)
    {
        if (!await _companyService.HasAccessToCompanyAsync(companyId))
        {
            return Forbid();
        }

        var branches = await _context.Branches
            .Where(b => b.CompanyId == companyId && b.IsActive)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync();

        return Json(branches);
    }
}
