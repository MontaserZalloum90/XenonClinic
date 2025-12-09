namespace XenonClinic.Core.Entities;

/// <summary>
/// Master feature registry - defines all available features/modules
/// </summary>
public class Feature
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "core"; // core, clinical, trading, admin
    public string? IconName { get; set; }
    public string? DefaultRoute { get; set; }
    public int SortOrder { get; set; } = 0;

    // Navigation
    public ICollection<TenantFeature> TenantFeatures { get; set; } = new List<TenantFeature>();
}
