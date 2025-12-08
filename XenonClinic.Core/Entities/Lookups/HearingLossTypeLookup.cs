namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for hearing loss types (replaces HearingLossType enum).
/// Examples: Normal, Sensorineural, Conductive, Mixed
/// </summary>
public class HearingLossTypeLookup : SystemLookup
{
    /// <summary>
    /// Clinical description for this hearing loss type.
    /// </summary>
    public string? ClinicalDescription { get; set; }

    /// <summary>
    /// Common treatment approaches for this type.
    /// </summary>
    public string? TreatmentApproaches { get; set; }

    // Navigation properties can be added as needed
}
