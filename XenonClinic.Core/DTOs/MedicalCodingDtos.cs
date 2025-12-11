namespace XenonClinic.Core.DTOs;

#region ICD-10 Code DTOs

/// <summary>
/// DTO for ICD-10 code
/// </summary>
public class ICD10CodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public int ChapterNumber { get; set; }
    public string? ChapterTitle { get; set; }
    public string? BlockRange { get; set; }
    public string? BlockDescription { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public bool IsActive { get; set; }
    public int VersionYear { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? ParentCode { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
}

/// <summary>
/// DTO for ICD-10 code search request
/// </summary>
public class ICD10SearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? CategoryCode { get; set; }
    public int? ChapterNumber { get; set; }
    public bool? IsBillable { get; set; }
    public bool? IsCommon { get; set; }
    public string? Specialty { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for creating/updating ICD-10 code
/// </summary>
public class CreateICD10CodeDto
{
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public int ChapterNumber { get; set; }
    public string? ChapterTitle { get; set; }
    public string? BlockRange { get; set; }
    public string? BlockDescription { get; set; }
    public string CodeType { get; set; } = "CM";
    public bool IsBillable { get; set; } = true;
    public int VersionYear { get; set; } = 2024;
    public DateTime? EffectiveDate { get; set; }
    public string? ParentCode { get; set; }
    public string? IncludesNotes { get; set; }
    public string? Excludes1Notes { get; set; }
    public string? Excludes2Notes { get; set; }
    public string? CodeFirstNotes { get; set; }
    public string? UseAdditionalCodeNotes { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
    public string? SearchKeywords { get; set; }
}

#endregion

#region CPT Code DTOs

/// <summary>
/// DTO for CPT code
/// </summary>
public class CPTCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? MediumDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string? Section { get; set; }
    public string? CodeRange { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int VersionYear { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public int? GlobalPeriod { get; set; }
    public decimal? WorkRvu { get; set; }
    public decimal? TotalFacilityRvu { get; set; }
    public decimal? TotalNonFacilityRvu { get; set; }
    public decimal? StandardFee { get; set; }
    public decimal? MedicareFee { get; set; }
    public decimal? ProfessionalFee { get; set; }
    public decimal? TechnicalFee { get; set; }
    public int? TypicalTime { get; set; }
    public List<string>? CommonModifiers { get; set; }
    public bool RequiresModifier { get; set; }
    public bool IsAddOnCode { get; set; }
    public string? PrimaryCode { get; set; }
    public List<string>? RelatedCodes { get; set; }
    public List<string>? CommonDiagnosisCodes { get; set; }
    public string? Notes { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
}

/// <summary>
/// DTO for CPT code search request
/// </summary>
public class CPTSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? CodeType { get; set; }
    public bool? IsCommon { get; set; }
    public bool? IsAddOnCode { get; set; }
    public string? Specialty { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for creating/updating CPT code
/// </summary>
public class CreateCPTCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? MediumDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string? Section { get; set; }
    public string? CodeRange { get; set; }
    public string CodeType { get; set; } = "CPT-I";
    public int VersionYear { get; set; } = 2024;
    public DateTime? EffectiveDate { get; set; }
    public int? GlobalPeriod { get; set; }
    public decimal? WorkRvu { get; set; }
    public decimal? FacilityPeRvu { get; set; }
    public decimal? NonFacilityPeRvu { get; set; }
    public decimal? MalpracticeRvu { get; set; }
    public decimal? StandardFee { get; set; }
    public decimal? MedicareFee { get; set; }
    public decimal? ProfessionalFee { get; set; }
    public decimal? TechnicalFee { get; set; }
    public int? TypicalTime { get; set; }
    public int? PreServiceTime { get; set; }
    public int? IntraServiceTime { get; set; }
    public int? PostServiceTime { get; set; }
    public List<string>? CommonModifiers { get; set; }
    public bool RequiresModifier { get; set; }
    public bool IsAddOnCode { get; set; }
    public string? PrimaryCode { get; set; }
    public List<string>? RelatedCodes { get; set; }
    public List<string>? ExcludedCodes { get; set; }
    public List<string>? BundledCodes { get; set; }
    public List<string>? CommonDiagnosisCodes { get; set; }
    public string? Notes { get; set; }
    public string? ClinicalExamples { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
    public string? SearchKeywords { get; set; }
}

#endregion

#region HCPCS Code DTOs

/// <summary>
/// DTO for HCPCS code
/// </summary>
public class HCPCSCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int VersionYear { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? CoverageNotes { get; set; }
    public string? BillingNotes { get; set; }
    public decimal? StandardFee { get; set; }
    public decimal? MedicareFee { get; set; }
    public string? NdcCode { get; set; }
    public string? DrugName { get; set; }
    public string? DrugUnit { get; set; }
    public string? RouteOfAdministration { get; set; }
    public List<string>? CommonModifiers { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
}

/// <summary>
/// DTO for HCPCS code search request
/// </summary>
public class HCPCSSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? CodeType { get; set; }
    public bool? IsCommon { get; set; }
    public string? DrugName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region Modifier DTOs

/// <summary>
/// DTO for medical code modifier
/// </summary>
public class MedicalCodeModifierDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string ModifierType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public string PriceAdjustmentType { get; set; } = string.Empty;
    public decimal? PriceAdjustmentValue { get; set; }
    public bool AppliesToProfessional { get; set; }
    public bool AppliesToTechnical { get; set; }
    public bool IsGlobalModifier { get; set; }
    public bool IsInformational { get; set; }
    public string? UsageNotes { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// DTO for modifier search request
/// </summary>
public class ModifierSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? ModifierType { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

#endregion

#region Code Validation DTOs

/// <summary>
/// DTO for code validation request
/// </summary>
public class CodeValidationRequestDto
{
    public string? DiagnosisCode { get; set; }
    public List<string>? SecondaryDiagnosisCodes { get; set; }
    public string? ProcedureCode { get; set; }
    public List<string>? Modifiers { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? PlaceOfService { get; set; }
}

/// <summary>
/// DTO for code validation response
/// </summary>
public class CodeValidationResponseDto
{
    public bool IsValid { get; set; }
    public List<CodeValidationIssueDto> Issues { get; set; } = new();
    public List<CodeSuggestionDto> Suggestions { get; set; } = new();
}

/// <summary>
/// DTO for code validation issue
/// </summary>
public class CodeValidationIssueDto
{
    public string Severity { get; set; } = string.Empty; // Error, Warning, Info
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RuleId { get; set; }
}

/// <summary>
/// DTO for code suggestion
/// </summary>
public class CodeSuggestionDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuggestionType { get; set; } = string.Empty; // Alternative, MoreSpecific, Related
    public string? Reason { get; set; }
}

#endregion

#region Fee Schedule DTOs

/// <summary>
/// DTO for fee schedule entry
/// </summary>
public class FeeScheduleEntryDto
{
    public int Id { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string? ProcedureDescription { get; set; }
    public string FeeScheduleName { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal StandardFee { get; set; }
    public decimal? FacilityFee { get; set; }
    public decimal? NonFacilityFee { get; set; }
    public decimal? ProfessionalFee { get; set; }
    public decimal? TechnicalFee { get; set; }
    public string? Locality { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for fee lookup request
/// </summary>
public class FeeLookupRequestDto
{
    public string ProcedureCode { get; set; } = string.Empty;
    public List<string>? Modifiers { get; set; }
    public string? FeeSchedule { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? PlaceOfService { get; set; }
    public string? Locality { get; set; }
}

/// <summary>
/// DTO for fee lookup response
/// </summary>
public class FeeLookupResponseDto
{
    public string ProcedureCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal StandardFee { get; set; }
    public decimal? AdjustedFee { get; set; }
    public decimal? FacilityFee { get; set; }
    public decimal? NonFacilityFee { get; set; }
    public decimal? ProfessionalFee { get; set; }
    public decimal? TechnicalFee { get; set; }
    public List<string>? AppliedModifiers { get; set; }
    public List<FeeAdjustmentDto>? Adjustments { get; set; }
    public string? FeeScheduleUsed { get; set; }
    public DateTime FeeEffectiveDate { get; set; }
}

/// <summary>
/// DTO for fee adjustment
/// </summary>
public class FeeAdjustmentDto
{
    public string AdjustmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AdjustmentPercent { get; set; }
    public decimal AdjustmentAmount { get; set; }
}

#endregion

#region Medical Coding Statistics

/// <summary>
/// Medical coding statistics
/// </summary>
public class MedicalCodingStatisticsDto
{
    public int TotalICD10Codes { get; set; }
    public int ActiveICD10Codes { get; set; }
    public int BillableICD10Codes { get; set; }
    public int TotalCPTCodes { get; set; }
    public int ActiveCPTCodes { get; set; }
    public int TotalHCPCSCodes { get; set; }
    public int ActiveHCPCSCodes { get; set; }
    public int TotalModifiers { get; set; }
    public int ActiveModifiers { get; set; }
    public Dictionary<string, int> ICD10ByChapter { get; set; } = new();
    public Dictionary<string, int> CPTByCategory { get; set; } = new();
    public List<string> MostUsedDiagnosisCodes { get; set; } = new();
    public List<string> MostUsedProcedureCodes { get; set; } = new();
}

#endregion
