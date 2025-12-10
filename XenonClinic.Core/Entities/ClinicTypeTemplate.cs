namespace XenonClinic.Core.Entities;

/// <summary>
/// Override template for a clinic type (AUDIOLOGY, DENTAL, VET, etc.)
/// Applied on top of the base CLINIC template
/// </summary>
public class ClinicTypeTemplate
{
    public int Id { get; set; }
    public string ClinicTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of additional feature codes to enable
    /// </summary>
    public string? FeaturesJson { get; set; }

    /// <summary>
    /// JSON object of terminology overrides
    /// </summary>
    public string? TerminologyJson { get; set; }

    /// <summary>
    /// JSON array of additional/override navigation items
    /// </summary>
    public string? NavigationJson { get; set; }

    /// <summary>
    /// JSON object of UI schema overrides by entity name
    /// </summary>
    public string? UISchemasJson { get; set; }

    /// <summary>
    /// JSON object of form layout overrides by entity name
    /// </summary>
    public string? FormLayoutsJson { get; set; }

    /// <summary>
    /// JSON object of list layout overrides by entity name
    /// </summary>
    public string? ListLayoutsJson { get; set; }

    public bool IsDefault { get; set; } = false;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ClinicType ClinicType { get; set; } = null!;
}
