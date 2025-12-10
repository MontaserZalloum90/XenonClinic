namespace XenonClinic.Core.Entities;

/// <summary>
/// Base template for a company type (CLINIC, TRADING)
/// Contains default features, terminology, navigation, and UI schemas
/// </summary>
public class CompanyTypeTemplate
{
    public int Id { get; set; }
    public string CompanyTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of feature codes enabled by default
    /// </summary>
    public string FeaturesJson { get; set; } = "[]";

    /// <summary>
    /// JSON object of terminology key-value pairs
    /// </summary>
    public string TerminologyJson { get; set; } = "{}";

    /// <summary>
    /// JSON array of navigation items
    /// </summary>
    public string NavigationJson { get; set; } = "[]";

    /// <summary>
    /// JSON object of UI schemas by entity name
    /// </summary>
    public string? UISchemasJson { get; set; }

    /// <summary>
    /// JSON object of form layouts by entity name
    /// </summary>
    public string? FormLayoutsJson { get; set; }

    /// <summary>
    /// JSON object of list layouts by entity name
    /// </summary>
    public string? ListLayoutsJson { get; set; }

    public bool IsDefault { get; set; } = false;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public CompanyType CompanyType { get; set; } = null!;
}
