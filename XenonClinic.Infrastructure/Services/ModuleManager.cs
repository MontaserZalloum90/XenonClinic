using Microsoft.Extensions.Configuration;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Implementation of module management service
/// </summary>
public class ModuleManager : IModuleManager
{
    private readonly List<IModule> _modules = new();
    private readonly IConfiguration _configuration;
    private readonly HashSet<string> _enabledModules = new();
    private readonly ILicenseValidator? _licenseValidator;

    public ModuleManager(IConfiguration configuration, ILicenseValidator? licenseValidator = null)
    {
        _configuration = configuration;
        _licenseValidator = licenseValidator;
        LoadEnabledModules();
    }

    public IEnumerable<IModule> GetAllModules()
    {
        return _modules;
    }

    public IModule? GetModule(string moduleName)
    {
        return _modules.FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsModuleEnabled(string moduleName)
    {
        return _enabledModules.Contains(moduleName);
    }

    public IEnumerable<IModule> GetEnabledModules()
    {
        return _modules.Where(m => _enabledModules.Contains(m.Name));
    }

    public void RegisterModule(IModule module)
    {
        if (_modules.Any(m => m.Name.Equals(module.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Module '{module.Name}' is already registered.");
        }

        _modules.Add(module);
    }

    public ModuleDescriptor? GetModuleDescriptor(string moduleName)
    {
        var module = GetModule(moduleName);
        if (module == null)
            return null;

        return CreateDescriptor(module);
    }

    public IEnumerable<ModuleDescriptor> GetAllModuleDescriptors()
    {
        return _modules.Select(CreateDescriptor);
    }

    private void LoadEnabledModules()
    {
        var enabledModules = _configuration.GetSection("Modules:Enabled").Get<List<string>>();
        if (enabledModules != null)
        {
            foreach (var moduleName in enabledModules)
            {
                _enabledModules.Add(moduleName);
            }
        }
    }

    private ModuleDescriptor CreateDescriptor(IModule module)
    {
        var licenseSection = _configuration.GetSection($"Modules:Licenses:{module.Name}");

        // Get license validation information
        LicenseValidationResult? validationResult = null;
        if (_licenseValidator != null)
        {
            validationResult = _licenseValidator.ValidateModuleLicense(module.Name);
        }

        var descriptor = new ModuleDescriptor
        {
            Name = module.Name,
            DisplayName = module.DisplayName,
            Version = module.Version,
            Description = module.Description,
            Category = module.Category,
            IconClass = module.IconClass,
            DisplayOrder = module.DisplayOrder,
            IsRequired = module.IsRequired,
            IsEnabled = IsModuleEnabled(module.Name),
            IsInstalled = true,
            Dependencies = module.Dependencies,
            LicenseKey = licenseSection["LicenseKey"],
            LicenseExpiryDate = licenseSection["ExpiryDate"] != null
                ? DateTime.Parse(licenseSection["ExpiryDate"]!)
                : null
        };

        // Populate license validation fields
        if (validationResult != null)
        {
            descriptor.IsLicensed = validationResult.IsLicensed;
            descriptor.IsLicenseExpired = validationResult.IsExpired;
            descriptor.IsLicenseValid = validationResult.IsValid;
            descriptor.DaysUntilLicenseExpiry = validationResult.DaysUntilExpiry;
            descriptor.MaxUsers = validationResult.MaxUsers;
        }
        else
        {
            // Fallback if validator is not available
            descriptor.IsLicensed = !string.IsNullOrEmpty(descriptor.LicenseKey);
            descriptor.IsLicenseExpired = false;
            descriptor.IsLicenseValid = descriptor.IsLicensed;
            descriptor.DaysUntilLicenseExpiry = null;
            descriptor.MaxUsers = licenseSection.GetValue<int?>("MaxUsers");
        }

        return descriptor;
    }
}
