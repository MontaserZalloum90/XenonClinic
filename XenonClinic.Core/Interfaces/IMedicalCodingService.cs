using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Medical Coding (ICD-10, CPT, HCPCS) operations
/// </summary>
public interface IMedicalCodingService
{
    #region ICD-10 Codes

    /// <summary>
    /// Search ICD-10 codes
    /// </summary>
    Task<IEnumerable<ICD10CodeDto>> SearchICD10CodesAsync(int branchId, ICD10SearchRequestDto request);

    /// <summary>
    /// Get an ICD-10 code by ID
    /// </summary>
    Task<ICD10CodeDto?> GetICD10CodeByIdAsync(int id);

    /// <summary>
    /// Get an ICD-10 code by code
    /// </summary>
    Task<ICD10CodeDto?> GetICD10CodeByCodeAsync(int branchId, string code);

    /// <summary>
    /// Get commonly used ICD-10 codes for a specialty
    /// </summary>
    Task<IEnumerable<ICD10CodeDto>> GetCommonICD10CodesAsync(int branchId, string? specialty = null, int limit = 50);

    /// <summary>
    /// Create an ICD-10 code
    /// </summary>
    Task<ICD10CodeDto> CreateICD10CodeAsync(int branchId, CreateICD10CodeDto dto);

    /// <summary>
    /// Update an ICD-10 code
    /// </summary>
    Task<ICD10CodeDto> UpdateICD10CodeAsync(int id, CreateICD10CodeDto dto);

    /// <summary>
    /// Delete an ICD-10 code
    /// </summary>
    Task DeleteICD10CodeAsync(int id);

    /// <summary>
    /// Import ICD-10 codes from file
    /// </summary>
    Task<int> ImportICD10CodesAsync(int branchId, Stream fileStream, string fileType);

    /// <summary>
    /// Get ICD-10 chapters list
    /// </summary>
    Task<IEnumerable<ICD10ChapterDto>> GetICD10ChaptersAsync();

    #endregion

    #region CPT Codes

    /// <summary>
    /// Search CPT codes
    /// </summary>
    Task<IEnumerable<CPTCodeDto>> SearchCPTCodesAsync(int branchId, CPTSearchRequestDto request);

    /// <summary>
    /// Get a CPT code by ID
    /// </summary>
    Task<CPTCodeDto?> GetCPTCodeByIdAsync(int id);

    /// <summary>
    /// Get a CPT code by code
    /// </summary>
    Task<CPTCodeDto?> GetCPTCodeByCodeAsync(int branchId, string code);

    /// <summary>
    /// Get commonly used CPT codes for a specialty
    /// </summary>
    Task<IEnumerable<CPTCodeDto>> GetCommonCPTCodesAsync(int branchId, string? specialty = null, int limit = 50);

    /// <summary>
    /// Create a CPT code
    /// </summary>
    Task<CPTCodeDto> CreateCPTCodeAsync(int branchId, CreateCPTCodeDto dto);

    /// <summary>
    /// Update a CPT code
    /// </summary>
    Task<CPTCodeDto> UpdateCPTCodeAsync(int id, CreateCPTCodeDto dto);

    /// <summary>
    /// Delete a CPT code
    /// </summary>
    Task DeleteCPTCodeAsync(int id);

    /// <summary>
    /// Import CPT codes from file
    /// </summary>
    Task<int> ImportCPTCodesAsync(int branchId, Stream fileStream, string fileType);

    /// <summary>
    /// Get CPT categories list
    /// </summary>
    Task<IEnumerable<CPTCategoryDto>> GetCPTCategoriesAsync();

    /// <summary>
    /// Get add-on codes for a primary CPT code
    /// </summary>
    Task<IEnumerable<CPTCodeDto>> GetAddOnCodesAsync(int branchId, string primaryCode);

    #endregion

    #region HCPCS Codes

    /// <summary>
    /// Search HCPCS codes
    /// </summary>
    Task<IEnumerable<HCPCSCodeDto>> SearchHCPCSCodesAsync(int branchId, HCPCSSearchRequestDto request);

    /// <summary>
    /// Get an HCPCS code by ID
    /// </summary>
    Task<HCPCSCodeDto?> GetHCPCSCodeByIdAsync(int id);

    /// <summary>
    /// Get an HCPCS code by code
    /// </summary>
    Task<HCPCSCodeDto?> GetHCPCSCodeByCodeAsync(int branchId, string code);

    /// <summary>
    /// Create an HCPCS code
    /// </summary>
    Task<HCPCSCodeDto> CreateHCPCSCodeAsync(int branchId, CreateHCPCSCodeDto dto);

    /// <summary>
    /// Update an HCPCS code
    /// </summary>
    Task<HCPCSCodeDto> UpdateHCPCSCodeAsync(int id, CreateHCPCSCodeDto dto);

    /// <summary>
    /// Delete an HCPCS code
    /// </summary>
    Task DeleteHCPCSCodeAsync(int id);

    #endregion

    #region Modifiers

    /// <summary>
    /// Search modifiers
    /// </summary>
    Task<IEnumerable<MedicalCodeModifierDto>> SearchModifiersAsync(int branchId, ModifierSearchRequestDto request);

    /// <summary>
    /// Get a modifier by ID
    /// </summary>
    Task<MedicalCodeModifierDto?> GetModifierByIdAsync(int id);

    /// <summary>
    /// Get a modifier by code
    /// </summary>
    Task<MedicalCodeModifierDto?> GetModifierByCodeAsync(int branchId, string code);

    /// <summary>
    /// Get commonly used modifiers
    /// </summary>
    Task<IEnumerable<MedicalCodeModifierDto>> GetCommonModifiersAsync(int branchId, string? procedureCode = null);

    /// <summary>
    /// Create a modifier
    /// </summary>
    Task<MedicalCodeModifierDto> CreateModifierAsync(int branchId, MedicalCodeModifierDto dto);

    /// <summary>
    /// Update a modifier
    /// </summary>
    Task<MedicalCodeModifierDto> UpdateModifierAsync(int id, MedicalCodeModifierDto dto);

    /// <summary>
    /// Delete a modifier
    /// </summary>
    Task DeleteModifierAsync(int id);

    #endregion

    #region Validation

    /// <summary>
    /// Validate diagnosis and procedure codes
    /// </summary>
    Task<CodeValidationResponseDto> ValidateCodesAsync(CodeValidationRequestDto request);

    /// <summary>
    /// Check if a diagnosis code is billable
    /// </summary>
    Task<bool> IsBillableCodeAsync(string code);

    /// <summary>
    /// Get related diagnosis codes for a procedure
    /// </summary>
    Task<IEnumerable<ICD10CodeDto>> GetRelatedDiagnosisCodesAsync(string procedureCode);

    /// <summary>
    /// Get bundled codes that cannot be billed together
    /// </summary>
    Task<IEnumerable<string>> GetBundledCodesAsync(string procedureCode);

    #endregion

    #region Fee Lookup

    /// <summary>
    /// Look up fee for a procedure
    /// </summary>
    Task<FeeLookupResponseDto> LookupFeeAsync(int branchId, FeeLookupRequestDto request);

    /// <summary>
    /// Get fee schedule entries
    /// </summary>
    Task<IEnumerable<FeeScheduleEntryDto>> GetFeeScheduleAsync(int branchId, string? feeScheduleName = null);

    /// <summary>
    /// Update fee schedule entry
    /// </summary>
    Task<FeeScheduleEntryDto> UpdateFeeScheduleEntryAsync(int branchId, FeeScheduleEntryDto dto);

    #endregion

    #region Statistics

    /// <summary>
    /// Get medical coding statistics
    /// </summary>
    Task<MedicalCodingStatisticsDto> GetStatisticsAsync(int branchId);

    /// <summary>
    /// Get most used diagnosis codes
    /// </summary>
    Task<IEnumerable<ICD10CodeDto>> GetMostUsedDiagnosisCodesAsync(int branchId, int limit = 20);

    /// <summary>
    /// Get most used procedure codes
    /// </summary>
    Task<IEnumerable<CPTCodeDto>> GetMostUsedProcedureCodesAsync(int branchId, int limit = 20);

    #endregion
}

/// <summary>
/// DTO for ICD-10 chapter
/// </summary>
public class ICD10ChapterDto
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CodeRange { get; set; } = string.Empty;
    public int CodeCount { get; set; }
}

/// <summary>
/// DTO for CPT category
/// </summary>
public class CPTCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CodeRange { get; set; } = string.Empty;
    public int CodeCount { get; set; }
    public List<CPTSubcategoryDto>? Subcategories { get; set; }
}

/// <summary>
/// DTO for CPT subcategory
/// </summary>
public class CPTSubcategoryDto
{
    public string SubcategoryName { get; set; } = string.Empty;
    public string CodeRange { get; set; } = string.Empty;
    public int CodeCount { get; set; }
}

/// <summary>
/// DTO for creating HCPCS code
/// </summary>
public class CreateHCPCSCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? DescriptionAr { get; set; }
    public string Level { get; set; } = "II";
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public int VersionYear { get; set; } = 2024;
    public DateTime? EffectiveDate { get; set; }
    public string? CoverageNotes { get; set; }
    public string? BillingNotes { get; set; }
    public string? PricingIndicator { get; set; }
    public decimal? StandardFee { get; set; }
    public decimal? MedicareFee { get; set; }
    public string? NdcCode { get; set; }
    public string? DrugName { get; set; }
    public string? DrugUnit { get; set; }
    public string? RouteOfAdministration { get; set; }
    public List<string>? CommonModifiers { get; set; }
    public bool IsCommon { get; set; }
    public List<string>? SpecialtyTags { get; set; }
    public string? SearchKeywords { get; set; }
}
