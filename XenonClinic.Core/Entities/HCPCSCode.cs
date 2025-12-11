using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// HCPCS (Healthcare Common Procedure Coding System) code reference
/// </summary>
public class HCPCSCode : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// HCPCS code (5 characters, starts with letter)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Short description
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Long description
    /// </summary>
    public string? LongDescription { get; set; }

    /// <summary>
    /// Description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// HCPCS level (Level II)
    /// </summary>
    public string Level { get; set; } = "II";

    /// <summary>
    /// Category (DME, Drugs, Services, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory
    /// </summary>
    public string? Subcategory { get; set; }

    /// <summary>
    /// Code type (J = Drugs, E = DME, etc.)
    /// </summary>
    public string CodeType { get; set; } = string.Empty;

    /// <summary>
    /// Whether code is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Version year
    /// </summary>
    public int VersionYear { get; set; } = 2024;

    /// <summary>
    /// Effective date
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Termination date
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Coverage notes
    /// </summary>
    public string? CoverageNotes { get; set; }

    /// <summary>
    /// Billing notes
    /// </summary>
    public string? BillingNotes { get; set; }

    /// <summary>
    /// Pricing indicator
    /// </summary>
    public string? PricingIndicator { get; set; }

    /// <summary>
    /// Standard fee
    /// </summary>
    public decimal? StandardFee { get; set; }

    /// <summary>
    /// Medicare fee
    /// </summary>
    public decimal? MedicareFee { get; set; }

    /// <summary>
    /// Status indicator
    /// </summary>
    public string? StatusIndicator { get; set; }

    /// <summary>
    /// Action code
    /// </summary>
    public string? ActionCode { get; set; }

    /// <summary>
    /// NDC (National Drug Code) if applicable
    /// </summary>
    public string? NdcCode { get; set; }

    /// <summary>
    /// Drug name if applicable
    /// </summary>
    public string? DrugName { get; set; }

    /// <summary>
    /// Drug unit (per 1mg, per 10 units, etc.)
    /// </summary>
    public string? DrugUnit { get; set; }

    /// <summary>
    /// Route of administration
    /// </summary>
    public string? RouteOfAdministration { get; set; }

    /// <summary>
    /// ASC payment indicator
    /// </summary>
    public string? AscPaymentIndicator { get; set; }

    /// <summary>
    /// Common modifiers
    /// </summary>
    public string? CommonModifiers { get; set; }

    /// <summary>
    /// Related codes
    /// </summary>
    public string? RelatedCodes { get; set; }

    /// <summary>
    /// Specialty tags
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
