using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Clinical Decision Support (CDS) Service
/// Provides evidence-based clinical recommendations, alerts, and decision support tools
/// </summary>
public interface IClinicalDecisionSupportService
{
    #region Drug Interaction Checking

    /// <summary>
    /// Check for drug-drug interactions
    /// </summary>
    Task<DrugInteractionResultDto> CheckDrugInteractionsAsync(DrugInteractionCheckDto request);

    /// <summary>
    /// Check interactions for a new medication against patient's current medications
    /// </summary>
    Task<DrugInteractionResultDto> CheckNewMedicationInteractionsAsync(int patientId, string newMedicationCode);

    /// <summary>
    /// Get all current interactions for a patient
    /// </summary>
    Task<DrugInteractionResultDto> GetCurrentInteractionsAsync(int patientId);

    #endregion

    #region Allergy Checking

    /// <summary>
    /// Check for allergy contraindications
    /// </summary>
    Task<AllergyCheckResultDto> CheckAllergyContraindicationsAsync(AllergyCheckRequestDto request);

    /// <summary>
    /// Check if medication is safe for patient based on allergies
    /// </summary>
    Task<AllergyCheckResultDto> IsMedicationSafeAsync(int patientId, string medicationCode);

    /// <summary>
    /// Get alternative medications for an allergic patient
    /// </summary>
    Task<List<string>> GetAllergyAlternativesAsync(int patientId, string medicationCode, string drugClass);

    #endregion

    #region Care Gaps & Clinical Reminders

    /// <summary>
    /// Get care gaps for a patient
    /// </summary>
    Task<PatientCareGapSummaryDto> GetPatientCareGapsAsync(int patientId);

    /// <summary>
    /// Get clinical reminders for a patient
    /// </summary>
    Task<List<ClinicalReminderDto>> GetClinicalRemindersAsync(int patientId, string? category = null);

    /// <summary>
    /// Get overdue care gaps for a branch
    /// </summary>
    Task<List<PatientCareGapSummaryDto>> GetOverdueCareGapsAsync(int branchId, int limit = 100);

    /// <summary>
    /// Acknowledge a clinical reminder
    /// </summary>
    Task<bool> AcknowledgeReminderAsync(int reminderId, int userId);

    /// <summary>
    /// Dismiss a clinical reminder with reason
    /// </summary>
    Task<bool> DismissReminderAsync(int reminderId, int userId, string reason);

    /// <summary>
    /// Complete a care gap
    /// </summary>
    Task<bool> CompleteCareGapAsync(int reminderId, int userId, string? notes = null);

    /// <summary>
    /// Recalculate care gaps for a patient
    /// </summary>
    Task RecalculateCareGapsAsync(int patientId);

    #endregion

    #region Diagnosis Suggestions

    /// <summary>
    /// Get diagnosis suggestions based on symptoms and findings
    /// </summary>
    Task<DiagnosisSuggestionResultDto> GetDiagnosisSuggestionsAsync(DiagnosisSuggestionRequestDto request);

    /// <summary>
    /// Get differential diagnoses for a symptom set
    /// </summary>
    Task<List<DiagnosisSuggestionDto>> GetDifferentialDiagnosesAsync(List<string> symptoms, int? patientId = null);

    /// <summary>
    /// Get red flag symptoms that require immediate attention
    /// </summary>
    Task<List<string>> GetRedFlagsAsync(List<string> symptoms, string? chiefComplaint = null);

    #endregion

    #region Dosage Checking

    /// <summary>
    /// Check if dosage is appropriate for patient
    /// </summary>
    Task<DosageCheckResultDto> CheckDosageAsync(DosageCheckRequestDto request);

    /// <summary>
    /// Get recommended dosage for patient based on factors
    /// </summary>
    Task<DosageRecommendationDto> GetRecommendedDosageAsync(int patientId, string medicationCode, string? indication = null);

    /// <summary>
    /// Get maximum daily dose for a medication
    /// </summary>
    Task<DosageRangeDto> GetMaxDailyDoseAsync(string medicationCode, int? patientAge = null, decimal? patientWeight = null);

    #endregion

    #region Contraindication Checking

    /// <summary>
    /// Check for medication contraindications based on patient conditions
    /// </summary>
    Task<ContraindicationCheckResultDto> CheckContraindicationsAsync(ContraindicationCheckRequestDto request);

    /// <summary>
    /// Get all medications contraindicated for a patient
    /// </summary>
    Task<List<ContraindicationAlertDto>> GetPatientContraindicationsAsync(int patientId);

    /// <summary>
    /// Check pregnancy/lactation contraindications
    /// </summary>
    Task<ContraindicationCheckResultDto> CheckPregnancyContraindicationsAsync(int patientId, string medicationCode);

    #endregion

    #region Lab Interpretation

    /// <summary>
    /// Interpret a lab result
    /// </summary>
    Task<LabInterpretationResultDto> InterpretLabResultAsync(LabInterpretationRequestDto request);

    /// <summary>
    /// Get lab value trends and analysis
    /// </summary>
    Task<TrendAnalysisDto> GetLabTrendAnalysisAsync(int patientId, string labCode, int? monthsBack = 12);

    /// <summary>
    /// Get critical lab values requiring immediate attention
    /// </summary>
    Task<List<LabInterpretationResultDto>> GetCriticalLabsAsync(int patientId);

    /// <summary>
    /// Get suggested follow-up labs based on current results
    /// </summary>
    Task<List<string>> GetSuggestedFollowUpLabsAsync(int patientId, string labCode, decimal value);

    #endregion

    #region Clinical Guidelines

    /// <summary>
    /// Get clinical guidelines for a condition
    /// </summary>
    Task<List<ClinicalGuidelineDto>> GetGuidelinesAsync(string conditionCode);

    /// <summary>
    /// Get applicable guidelines for a patient
    /// </summary>
    Task<List<ClinicalGuidelineDto>> GetPatientGuidelinesAsync(int patientId);

    /// <summary>
    /// Search clinical guidelines
    /// </summary>
    Task<List<ClinicalGuidelineDto>> SearchGuidelinesAsync(string searchTerm, string? category = null);

    #endregion

    #region Order Sets

    /// <summary>
    /// Get available order sets
    /// </summary>
    Task<List<ClinicalOrderSetDto>> GetOrderSetsAsync(string? category = null, string? conditionCode = null);

    /// <summary>
    /// Get order set by ID
    /// </summary>
    Task<ClinicalOrderSetDto?> GetOrderSetByIdAsync(int orderSetId);

    /// <summary>
    /// Get recommended order sets for patient's conditions
    /// </summary>
    Task<List<ClinicalOrderSetDto>> GetRecommendedOrderSetsAsync(int patientId);

    /// <summary>
    /// Create a new order set
    /// </summary>
    Task<ClinicalOrderSetDto> CreateOrderSetAsync(ClinicalOrderSetDto orderSet, int createdByUserId);

    /// <summary>
    /// Update an order set
    /// </summary>
    Task<ClinicalOrderSetDto?> UpdateOrderSetAsync(int orderSetId, ClinicalOrderSetDto orderSet, int updatedByUserId);

    /// <summary>
    /// Delete an order set
    /// </summary>
    Task<bool> DeleteOrderSetAsync(int orderSetId, int deletedByUserId);

    #endregion

    #region Risk Calculators

    /// <summary>
    /// Get available risk calculators
    /// </summary>
    Task<List<RiskCalculatorDto>> GetRiskCalculatorsAsync(string? category = null);

    /// <summary>
    /// Calculate risk score
    /// </summary>
    Task<RiskCalculationResultDto> CalculateRiskAsync(RiskCalculationRequestDto request);

    /// <summary>
    /// Get patient's risk scores history
    /// </summary>
    Task<List<RiskCalculationResultDto>> GetPatientRiskHistoryAsync(int patientId, string calculatorId);

    /// <summary>
    /// Calculate cardiovascular risk (ASCVD)
    /// </summary>
    Task<RiskCalculationResultDto> CalculateAscvdRiskAsync(int patientId);

    /// <summary>
    /// Calculate diabetes risk
    /// </summary>
    Task<RiskCalculationResultDto> CalculateDiabetesRiskAsync(int patientId);

    /// <summary>
    /// Calculate fall risk
    /// </summary>
    Task<RiskCalculationResultDto> CalculateFallRiskAsync(int patientId);

    #endregion

    #region Alert Management

    /// <summary>
    /// Get alert configuration
    /// </summary>
    Task<List<ClinicalAlertConfigDto>> GetAlertConfigurationsAsync();

    /// <summary>
    /// Update alert configuration
    /// </summary>
    Task<ClinicalAlertConfigDto> UpdateAlertConfigurationAsync(ClinicalAlertConfigDto config, int updatedByUserId);

    /// <summary>
    /// Override a clinical alert
    /// </summary>
    Task<AlertOverrideDto> OverrideAlertAsync(int alertId, string alertType, string reason, int userId, int patientId);

    /// <summary>
    /// Get alert overrides for audit
    /// </summary>
    Task<List<AlertOverrideDto>> GetAlertOverridesAsync(int? patientId = null, int? userId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Review an alert override
    /// </summary>
    Task<bool> ReviewAlertOverrideAsync(int overrideId, int reviewedByUserId, string? notes = null);

    #endregion

    #region Comprehensive Safety Check

    /// <summary>
    /// Perform comprehensive medication safety check
    /// </summary>
    Task<MedicationSafetyCheckResultDto> PerformMedicationSafetyCheckAsync(int patientId, string medicationCode, DosageCheckRequestDto? dosageInfo = null);

    #endregion
}
