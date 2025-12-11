using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Drug Database operations
/// Integrates with RxNorm, FDA, and other drug information sources
/// </summary>
public interface IDrugDatabaseService
{
    #region Drug Search

    /// <summary>
    /// Search for drugs by name, NDC, or RxCUI
    /// </summary>
    Task<DrugSearchResultDto> SearchDrugsAsync(DrugSearchRequestDto request);

    /// <summary>
    /// Get drug autocomplete suggestions
    /// </summary>
    Task<IEnumerable<DrugSummaryDto>> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10);

    /// <summary>
    /// Get drug by RxCUI
    /// </summary>
    Task<DrugDetailDto?> GetDrugByRxCUIAsync(string rxCUI);

    /// <summary>
    /// Get drug by NDC
    /// </summary>
    Task<DrugDetailDto?> GetDrugByNDCAsync(string ndc);

    /// <summary>
    /// Get drug alternatives (generics, therapeutic equivalents)
    /// </summary>
    Task<IEnumerable<DrugAlternativeDto>> GetDrugAlternativesAsync(string rxCUI);

    /// <summary>
    /// Get brand names for generic drug
    /// </summary>
    Task<IEnumerable<DrugSummaryDto>> GetBrandNamesAsync(string genericRxCUI);

    /// <summary>
    /// Get generic equivalents for brand drug
    /// </summary>
    Task<IEnumerable<DrugSummaryDto>> GetGenericEquivalentsAsync(string brandRxCUI);

    #endregion

    #region Drug Interactions

    /// <summary>
    /// Check for drug-drug interactions
    /// </summary>
    Task<DrugInteractionCheckResponseDto> CheckInteractionsAsync(DrugInteractionCheckRequestDto request);

    /// <summary>
    /// Check for drug-drug interactions by RxCUI list
    /// </summary>
    Task<DrugInteractionCheckResponseDto> CheckInteractionsByRxCUIsAsync(IEnumerable<string> rxCUIs);

    /// <summary>
    /// Check for drug-allergy interactions for a patient
    /// </summary>
    Task<IEnumerable<DrugAllergyAlertDto>> CheckAllergyInteractionsAsync(int patientId, string rxCUI);

    /// <summary>
    /// Get known interactions for a specific drug
    /// </summary>
    Task<IEnumerable<DrugInteractionDto>> GetKnownInteractionsAsync(string rxCUI);

    #endregion

    #region Drug Classes

    /// <summary>
    /// Get drug classes (ATC, EPC, etc.)
    /// </summary>
    Task<IEnumerable<DrugClassDto>> GetDrugClassesAsync(string classType = "ATC");

    /// <summary>
    /// Get drugs by class
    /// </summary>
    Task<IEnumerable<DrugSummaryDto>> GetDrugsByClassAsync(DrugsByClassRequestDto request);

    /// <summary>
    /// Get drug class hierarchy for a drug
    /// </summary>
    Task<IEnumerable<DrugClassDto>> GetDrugClassHierarchyAsync(string rxCUI);

    #endregion

    #region Formulary

    /// <summary>
    /// Check if drug is on formulary
    /// </summary>
    Task<FormularyCheckResponseDto> CheckFormularyAsync(int branchId, string rxCUI);

    /// <summary>
    /// Get formulary drugs
    /// </summary>
    Task<IEnumerable<FormularyDrugDto>> GetFormularyDrugsAsync(int branchId, FormularyTierDto? tier = null);

    /// <summary>
    /// Add drug to formulary
    /// </summary>
    Task<FormularyDrugDto> AddToFormularyAsync(int branchId, AddToFormularyRequestDto request);

    /// <summary>
    /// Update formulary drug
    /// </summary>
    Task<FormularyDrugDto> UpdateFormularyDrugAsync(int formularyDrugId, AddToFormularyRequestDto request);

    /// <summary>
    /// Remove drug from formulary
    /// </summary>
    Task<bool> RemoveFromFormularyAsync(int formularyDrugId);

    /// <summary>
    /// Get formulary alternatives for non-formulary drug
    /// </summary>
    Task<IEnumerable<FormularyDrugDto>> GetFormularyAlternativesAsync(int branchId, string rxCUI);

    #endregion

    #region Drug Monograph

    /// <summary>
    /// Get drug monograph
    /// </summary>
    Task<DrugMonographDto?> GetMonographAsync(string rxCUI);

    /// <summary>
    /// Get drug monograph section
    /// </summary>
    Task<MonographSectionDto?> GetMonographSectionAsync(string rxCUI, string sectionName);

    #endregion

    #region Pricing

    /// <summary>
    /// Get drug pricing information
    /// </summary>
    Task<DrugPricingDto?> GetDrugPricingAsync(string rxCUI);

    /// <summary>
    /// Get drug pricing by NDC
    /// </summary>
    Task<DrugPricingDto?> GetDrugPricingByNDCAsync(string ndc);

    /// <summary>
    /// Compare drug prices
    /// </summary>
    Task<IEnumerable<(DrugSummaryDto Drug, DrugPricingDto? Pricing)>> ComparePricesAsync(IEnumerable<string> rxCUIs);

    #endregion

    #region Patient Assistance

    /// <summary>
    /// Find patient assistance programs for a drug
    /// </summary>
    Task<IEnumerable<PatientAssistanceProgramDto>> FindAssistanceProgramsAsync(FindAssistanceProgramsRequestDto request);

    /// <summary>
    /// Get copay card information
    /// </summary>
    Task<IEnumerable<PatientAssistanceProgramDto>> GetCopayCardsAsync(string rxCUI);

    #endregion

    #region Controlled Substances

    /// <summary>
    /// Get controlled substance information
    /// </summary>
    Task<ControlledSubstanceInfoDto?> GetControlledSubstanceInfoAsync(string rxCUI);

    /// <summary>
    /// Check if drug is controlled substance
    /// </summary>
    Task<bool> IsControlledSubstanceAsync(string rxCUI);

    /// <summary>
    /// Get DEA schedule for drug
    /// </summary>
    Task<string?> GetDEAScheduleAsync(string rxCUI);

    #endregion

    #region FDA Information

    /// <summary>
    /// Get FDA drug information
    /// </summary>
    Task<DrugFDAInfoDto?> GetFDAInfoAsync(string rxCUI);

    /// <summary>
    /// Get FDA drug recalls
    /// </summary>
    Task<IEnumerable<object>> GetDrugRecallsAsync(string? rxCUI = null, DateTime? fromDate = null);

    /// <summary>
    /// Report adverse event
    /// </summary>
    Task<AdverseEventResponseDto> ReportAdverseEventAsync(int branchId, ReportAdverseEventDto report);

    #endregion

    #region NDC Operations

    /// <summary>
    /// Convert NDC formats
    /// </summary>
    Task<DrugNDCDto?> ConvertNDCAsync(string ndc);

    /// <summary>
    /// Get all NDCs for a drug
    /// </summary>
    Task<IEnumerable<DrugNDCDto>> GetNDCsForDrugAsync(string rxCUI);

    /// <summary>
    /// Validate NDC
    /// </summary>
    Task<(bool IsValid, string? Message)> ValidateNDCAsync(string ndc);

    #endregion

    #region Caching & Updates

    /// <summary>
    /// Refresh drug cache
    /// </summary>
    Task RefreshDrugCacheAsync();

    /// <summary>
    /// Get last database update time
    /// </summary>
    Task<DateTime?> GetLastUpdateTimeAsync();

    /// <summary>
    /// Sync with RxNorm updates
    /// </summary>
    Task<int> SyncRxNormUpdatesAsync();

    #endregion
}
