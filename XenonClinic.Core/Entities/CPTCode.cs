using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// CPT (Current Procedural Terminology) procedure code reference
/// </summary>
public class CPTCode : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// CPT code (5 characters, e.g., 99213)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Short description
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Medium description
    /// </summary>
    public string? MediumDescription { get; set; }

    /// <summary>
    /// Long/Full description
    /// </summary>
    public string? LongDescription { get; set; }

    /// <summary>
    /// Description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Code category (E/M, Surgery, Radiology, Pathology, Medicine)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory
    /// </summary>
    public string? Subcategory { get; set; }

    /// <summary>
    /// Section
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// Code range (e.g., 99201-99215)
    /// </summary>
    public string? CodeRange { get; set; }

    /// <summary>
    /// Code type (CPT-I, CPT-II, CPT-III, HCPCS)
    /// </summary>
    public string CodeType { get; set; } = "CPT-I";

    /// <summary>
    /// Whether code is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// CPT version year
    /// </summary>
    public int VersionYear { get; set; } = 2024;

    /// <summary>
    /// Effective date
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Termination date (if retired)
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Global period (days)
    /// </summary>
    public int? GlobalPeriod { get; set; }

    /// <summary>
    /// Work RVU (Relative Value Unit)
    /// </summary>
    public decimal? WorkRvu { get; set; }

    /// <summary>
    /// Facility Practice Expense RVU
    /// </summary>
    public decimal? FacilityPeRvu { get; set; }

    /// <summary>
    /// Non-Facility Practice Expense RVU
    /// </summary>
    public decimal? NonFacilityPeRvu { get; set; }

    /// <summary>
    /// Malpractice RVU
    /// </summary>
    public decimal? MalpracticeRvu { get; set; }

    /// <summary>
    /// Total Facility RVU
    /// </summary>
    public decimal? TotalFacilityRvu { get; set; }

    /// <summary>
    /// Total Non-Facility RVU
    /// </summary>
    public decimal? TotalNonFacilityRvu { get; set; }

    /// <summary>
    /// Standard fee/price
    /// </summary>
    public decimal? StandardFee { get; set; }

    /// <summary>
    /// Medicare fee
    /// </summary>
    public decimal? MedicareFee { get; set; }

    /// <summary>
    /// Professional component fee
    /// </summary>
    public decimal? ProfessionalFee { get; set; }

    /// <summary>
    /// Technical component fee
    /// </summary>
    public decimal? TechnicalFee { get; set; }

    /// <summary>
    /// Typical procedure time (minutes)
    /// </summary>
    public int? TypicalTime { get; set; }

    /// <summary>
    /// Pre-service time (minutes)
    /// </summary>
    public int? PreServiceTime { get; set; }

    /// <summary>
    /// Intra-service time (minutes)
    /// </summary>
    public int? IntraServiceTime { get; set; }

    /// <summary>
    /// Post-service time (minutes)
    /// </summary>
    public int? PostServiceTime { get; set; }

    /// <summary>
    /// Common modifiers (JSON array)
    /// </summary>
    public string? CommonModifiers { get; set; }

    /// <summary>
    /// Requires modifier flag
    /// </summary>
    public bool RequiresModifier { get; set; }

    /// <summary>
    /// Add-on code flag
    /// </summary>
    public bool IsAddOnCode { get; set; }

    /// <summary>
    /// Primary code (if this is an add-on)
    /// </summary>
    public string? PrimaryCode { get; set; }

    /// <summary>
    /// Related codes (JSON array)
    /// </summary>
    public string? RelatedCodes { get; set; }

    /// <summary>
    /// Excluded codes (cannot bill together)
    /// </summary>
    public string? ExcludedCodes { get; set; }

    /// <summary>
    /// Bundled codes (included in this code)
    /// </summary>
    public string? BundledCodes { get; set; }

    /// <summary>
    /// Common diagnosis codes (JSON array)
    /// </summary>
    public string? CommonDiagnosisCodes { get; set; }

    /// <summary>
    /// Notes and guidelines
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Clinical examples
    /// </summary>
    public string? ClinicalExamples { get; set; }

    /// <summary>
    /// Specialty tags (JSON array)
    /// </summary>
    public string? SpecialtyTags { get; set; }

    /// <summary>
    /// Search keywords
    /// </summary>
    public string? SearchKeywords { get; set; }

    /// <summary>
    /// Common/frequently used flag
    /// </summary>
    public bool IsCommon { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
