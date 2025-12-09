namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a single tooth record in a patient's dental chart
/// </summary>
public class ToothRecord
{
    public int Id { get; set; }
    public int ToothChartId { get; set; }

    /// <summary>
    /// Universal numbering system (1-32 for adults, A-T for primary teeth)
    /// </summary>
    public string ToothNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current condition of the tooth (e.g., Healthy, Decayed, Missing, Filled, Crown, etc.)
    /// </summary>
    public string Condition { get; set; } = "Healthy";

    /// <summary>
    /// Affected surfaces (e.g., Mesial, Distal, Occlusal, Buccal, Lingual)
    /// </summary>
    public string? AffectedSurfaces { get; set; }

    /// <summary>
    /// Whether this is a primary (baby) tooth
    /// </summary>
    public bool IsPrimaryTooth { get; set; }

    /// <summary>
    /// Whether the tooth is missing
    /// </summary>
    public bool IsMissing { get; set; }

    /// <summary>
    /// Restoration type if any (e.g., Amalgam, Composite, Crown, Bridge, Implant)
    /// </summary>
    public string? RestorationType { get; set; }

    public string? Notes { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ToothChart? ToothChart { get; set; }
}
