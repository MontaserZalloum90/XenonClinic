using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Medical code modifiers for CPT/HCPCS codes
/// </summary>
public class MedicalCodeModifier : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Modifier code (2 characters, e.g., 25, 59, TC)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Modifier description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Modifier type (CPT, HCPCS, Anesthesia, Physical Status)
    /// </summary>
    public string ModifierType { get; set; } = "CPT";

    /// <summary>
    /// Category (E&M, Surgery, Radiology, etc.)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether modifier is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Price adjustment type (Percentage, Fixed, None)
    /// </summary>
    public string PriceAdjustmentType { get; set; } = "None";

    /// <summary>
    /// Price adjustment value
    /// </summary>
    public decimal? PriceAdjustmentValue { get; set; }

    /// <summary>
    /// Applies to professional component
    /// </summary>
    public bool AppliesToProfessional { get; set; } = true;

    /// <summary>
    /// Applies to technical component
    /// </summary>
    public bool AppliesToTechnical { get; set; } = true;

    /// <summary>
    /// Global modifier flag
    /// </summary>
    public bool IsGlobalModifier { get; set; }

    /// <summary>
    /// Informational only (no pricing impact)
    /// </summary>
    public bool IsInformational { get; set; }

    /// <summary>
    /// Usage notes
    /// </summary>
    public string? UsageNotes { get; set; }

    /// <summary>
    /// Common codes this modifier applies to
    /// </summary>
    public string? ApplicableCodes { get; set; }

    /// <summary>
    /// Incompatible modifiers
    /// </summary>
    public string? IncompatibleModifiers { get; set; }

    /// <summary>
    /// Sort order/priority when multiple modifiers
    /// </summary>
    public int SortOrder { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
