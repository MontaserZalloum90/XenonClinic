using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// ICD-10 diagnosis code reference
/// </summary>
public class ICD10Code : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// ICD-10 code (e.g., A00.0, J18.9)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Short description
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Long/Full description
    /// </summary>
    public string? LongDescription { get; set; }

    /// <summary>
    /// Description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Category code (first 3 characters)
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? CategoryDescription { get; set; }

    /// <summary>
    /// Chapter number (1-22)
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Chapter title
    /// </summary>
    public string? ChapterTitle { get; set; }

    /// <summary>
    /// Block/Range (e.g., A00-A09)
    /// </summary>
    public string? BlockRange { get; set; }

    /// <summary>
    /// Block description
    /// </summary>
    public string? BlockDescription { get; set; }

    /// <summary>
    /// Code type (CM = Clinical Modification, PCS = Procedure Coding System)
    /// </summary>
    public string CodeType { get; set; } = "CM";

    /// <summary>
    /// Whether code is billable/valid for claims
    /// </summary>
    public bool IsBillable { get; set; } = true;

    /// <summary>
    /// Whether code is active in current version
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ICD-10 version year
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
    /// Parent code for hierarchy
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// Includes notes
    /// </summary>
    public string? IncludesNotes { get; set; }

    /// <summary>
    /// Excludes1 notes (conditions not coded here)
    /// </summary>
    public string? Excludes1Notes { get; set; }

    /// <summary>
    /// Excludes2 notes (conditions coded elsewhere)
    /// </summary>
    public string? Excludes2Notes { get; set; }

    /// <summary>
    /// Code first instructions
    /// </summary>
    public string? CodeFirstNotes { get; set; }

    /// <summary>
    /// Use additional code instructions
    /// </summary>
    public string? UseAdditionalCodeNotes { get; set; }

    /// <summary>
    /// Common/frequently used flag
    /// </summary>
    public bool IsCommon { get; set; }

    /// <summary>
    /// Specialty tags (JSON array)
    /// </summary>
    public string? SpecialtyTags { get; set; }

    /// <summary>
    /// Search keywords
    /// </summary>
    public string? SearchKeywords { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
