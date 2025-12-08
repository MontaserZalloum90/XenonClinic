using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Interfaces;
using XenonClinic.Web.Models.Admin;

namespace XenonClinic.Web.Controllers;

[Authorize(Roles = RoleConstants.Combined.SuperAndTenantAdmin)]
public class ModulesController : Controller
{
    private readonly IModuleManager _moduleManager;
    private readonly IConfiguration _configuration;

    public ModulesController(
        IModuleManager moduleManager,
        IConfiguration configuration)
    {
        _moduleManager = moduleManager;
        _configuration = configuration;
    }

    // GET: /Modules
    public IActionResult Index()
    {
        var allModules = _moduleManager.GetAllModules();
        var enabledModules = _moduleManager.GetEnabledModules().ToList();

        var model = new ModuleListViewModel
        {
            TotalModules = allModules.Count(),
            EnabledModules = enabledModules.Count,
            DisabledModules = allModules.Count() - enabledModules.Count,
            Modules = allModules.OrderBy(m => m.DisplayOrder).Select(m =>
            {
                var licenseConfig = _configuration.GetSection($"Modules:Licenses:{m.Name}");
                var licenseKey = licenseConfig.GetValue<string>("LicenseKey");
                var expiryDateStr = licenseConfig.GetValue<string>("ExpiryDate");
                DateTime? expiryDate = null;

                if (!string.IsNullOrEmpty(expiryDateStr) && DateTime.TryParse(expiryDateStr, out var parsedDate))
                {
                    expiryDate = parsedDate;
                }

                return new ModuleItemViewModel
                {
                    Name = m.Name,
                    DisplayName = m.DisplayName,
                    Version = m.Version,
                    Description = m.Description,
                    Category = m.Category,
                    IconClass = m.IconClass,
                    DisplayOrder = m.DisplayOrder,
                    IsEnabled = _moduleManager.IsModuleEnabled(m.Name),
                    IsRequired = m.IsRequired,
                    Dependencies = m.Dependencies,
                    LicenseKey = licenseKey,
                    LicenseExpiryDate = expiryDate,
                    MaxUsers = licenseConfig.GetValue<int?>("MaxUsers")
                };
            }).ToList()
        };

        return View(model);
    }

    // GET: /Modules/Details/{moduleName}
    public IActionResult Details(string moduleName)
    {
        var module = _moduleManager.GetModule(moduleName);
        if (module == null)
        {
            return NotFound();
        }

        var licenseConfig = _configuration.GetSection($"Modules:Licenses:{moduleName}");
        var licenseKey = licenseConfig.GetValue<string>("LicenseKey");
        var expiryDateStr = licenseConfig.GetValue<string>("ExpiryDate");
        DateTime? expiryDate = null;

        if (!string.IsNullOrEmpty(expiryDateStr) && DateTime.TryParse(expiryDateStr, out var parsedDate))
        {
            expiryDate = parsedDate;
        }

        var isExpired = expiryDate.HasValue && expiryDate.Value < DateTime.UtcNow;
        var daysUntilExpiry = expiryDate.HasValue
            ? (int?)(expiryDate.Value - DateTime.UtcNow).TotalDays
            : null;

        var model = new ModuleDetailsViewModel
        {
            Name = module.Name,
            DisplayName = module.DisplayName,
            Version = module.Version,
            Description = module.Description,
            Category = module.Category,
            IconClass = module.IconClass,
            DisplayOrder = module.DisplayOrder,
            IsEnabled = _moduleManager.IsModuleEnabled(module.Name),
            IsRequired = module.IsRequired,
            Dependencies = module.Dependencies,
            LicenseKey = licenseKey,
            LicenseExpiryDate = expiryDate,
            MaxUsers = licenseConfig.GetValue<int?>("MaxUsers"),
            IsLicenseValid = !string.IsNullOrEmpty(licenseKey),
            IsLicenseExpired = isExpired,
            DaysUntilExpiry = daysUntilExpiry
        };

        return View(model);
    }
}
