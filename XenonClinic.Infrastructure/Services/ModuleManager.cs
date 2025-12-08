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

    public ModuleManager(IConfiguration configuration)
    {
        _configuration = configuration;
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

        return new ModuleDescriptor
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
    }
}
