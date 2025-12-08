using XenonClinic.Core.Abstractions;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for managing module lifecycle and discovery
/// </summary>
public interface IModuleManager
{
    /// <summary>
    /// Get all registered modules
    /// </summary>
    IEnumerable<IModule> GetAllModules();

    /// <summary>
    /// Get a module by name
    /// </summary>
    IModule? GetModule(string moduleName);

    /// <summary>
    /// Check if a module is enabled
    /// </summary>
    bool IsModuleEnabled(string moduleName);

    /// <summary>
    /// Get all enabled modules
    /// </summary>
    IEnumerable<IModule> GetEnabledModules();

    /// <summary>
    /// Register a module
    /// </summary>
    void RegisterModule(IModule module);

    /// <summary>
    /// Get module metadata
    /// </summary>
    ModuleDescriptor? GetModuleDescriptor(string moduleName);

    /// <summary>
    /// Get all module descriptors
    /// </summary>
    IEnumerable<ModuleDescriptor> GetAllModuleDescriptors();
}

/// <summary>
/// Metadata describing a module
/// </summary>
public class ModuleDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsInstalled { get; set; }
    public string[] Dependencies { get; set; } = Array.Empty<string>();
    public DateTime? InstalledDate { get; set; }
    public string? LicenseKey { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool IsLicensed { get; set; }
    public bool IsLicenseExpired { get; set; }
    public bool IsLicenseValid { get; set; }
    public int? DaysUntilLicenseExpiry { get; set; }
    public int? MaxUsers { get; set; }
}
