using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Clinical Decision Support (CDS) API
/// Evidence-based clinical recommendations, alerts, and decision support tools
/// </summary>
[Route("api/cds")]
[Authorize]
public class ClinicalDecisionSupportController : BaseApiController
{
    private readonly IClinicalDecisionSupportService _cdsService;
    private readonly ILogger<ClinicalDecisionSupportController> _logger;

    public ClinicalDecisionSupportController(
        IClinicalDecisionSupportService cdsService,
        ILogger<ClinicalDecisionSupportController> logger)
    {
        _cdsService = cdsService;
        _logger = logger;
    }

    #region Drug Interactions

    /// <summary>
    /// Check for drug-drug interactions
    /// </summary>
    [HttpPost("drug-interactions/check")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<DrugInteractionResultDto>), 200)]
    public async Task<IActionResult> CheckDrugInteractions(
        [FromBody] DrugInteractionCheckDto request)
    {
        var result = await _cdsService.CheckDrugInteractionsAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Check interactions for a new medication against patient's current medications
    /// </summary>
    [HttpGet("drug-interactions/patient/{patientId}/check/{medicationCode}")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<DrugInteractionResultDto>), 200)]
    public async Task<IActionResult> CheckNewMedicationInteractions(
        int patientId,
        string medicationCode)
    {
        var result = await _cdsService.CheckNewMedicationInteractionsAsync(patientId, medicationCode);
        return ApiOk(result);
    }

    /// <summary>
    /// Get all current interactions for a patient
    /// </summary>
    [HttpGet("drug-interactions/patient/{patientId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<DrugInteractionResultDto>), 200)]
    public async Task<IActionResult> GetCurrentInteractions(int patientId)
    {
        var result = await _cdsService.GetCurrentInteractionsAsync(patientId);
        return ApiOk(result);
    }

    #endregion

    #region Allergy Checking

    /// <summary>
    /// Check for allergy contraindications
    /// </summary>
    [HttpPost("allergies/check")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<AllergyCheckResultDto>), 200)]
    public async Task<IActionResult> CheckAllergyContraindications(
        [FromBody] AllergyCheckRequestDto request)
    {
        var result = await _cdsService.CheckAllergyContraindicationsAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Check if medication is safe for patient based on allergies
    /// </summary>
    [HttpGet("allergies/patient/{patientId}/safe/{medicationCode}")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<AllergyCheckResultDto>), 200)]
    public async Task<IActionResult> IsMedicationSafe(
        int patientId,
        string medicationCode)
    {
        var result = await _cdsService.IsMedicationSafeAsync(patientId, medicationCode);
        return ApiOk(result);
    }

    /// <summary>
    /// Get alternative medications for an allergic patient
    /// </summary>
    [HttpGet("allergies/patient/{patientId}/alternatives")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
    public async Task<IActionResult> GetAllergyAlternatives(
        int patientId,
        [FromQuery] string medicationCode,
        [FromQuery] string drugClass)
    {
        var alternatives = await _cdsService.GetAllergyAlternativesAsync(patientId, medicationCode, drugClass);
        return ApiOk(alternatives);
    }

    #endregion

    #region Care Gaps & Clinical Reminders

    /// <summary>
    /// Get care gaps for a patient
    /// </summary>
    [HttpGet("care-gaps/patient/{patientId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<PatientCareGapSummaryDto>), 200)]
    public async Task<IActionResult> GetPatientCareGaps(int patientId)
    {
        var result = await _cdsService.GetPatientCareGapsAsync(patientId);
        return ApiOk(result);
    }

    /// <summary>
    /// Get clinical reminders for a patient
    /// </summary>
    [HttpGet("reminders/patient/{patientId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalReminderDto>>), 200)]
    public async Task<IActionResult> GetClinicalReminders(
        int patientId,
        [FromQuery] string? category = null)
    {
        var reminders = await _cdsService.GetClinicalRemindersAsync(patientId, category);
        return ApiOk(reminders);
    }

    /// <summary>
    /// Get overdue care gaps for a branch
    /// </summary>
    [HttpGet("care-gaps/branch/{branchId}/overdue")]
    [Authorize(Policy = "ClinicalReportView")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientCareGapSummaryDto>>), 200)]
    public async Task<IActionResult> GetOverdueCareGaps(
        int branchId,
        [FromQuery] int limit = 100)
    {
        var results = await _cdsService.GetOverdueCareGapsAsync(branchId, limit);
        return ApiOk(results);
    }

    /// <summary>
    /// Acknowledge a clinical reminder
    /// </summary>
    [HttpPost("reminders/{reminderId}/acknowledge")]
    [Authorize(Policy = "PatientEdit")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> AcknowledgeReminder(
        int reminderId,
        [FromQuery] int userId)
    {
        var success = await _cdsService.AcknowledgeReminderAsync(reminderId, userId);

        if (!success)
            return ApiBadRequest("Unable to acknowledge reminder");

        return ApiOk("Reminder acknowledged");
    }

    /// <summary>
    /// Dismiss a clinical reminder with reason
    /// </summary>
    [HttpPost("reminders/{reminderId}/dismiss")]
    [Authorize(Policy = "PatientEdit")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> DismissReminder(
        int reminderId,
        [FromQuery] int userId,
        [FromBody] DismissReminderRequest request)
    {
        var success = await _cdsService.DismissReminderAsync(reminderId, userId, request.Reason);

        if (!success)
            return ApiBadRequest("Unable to dismiss reminder");

        return ApiOk("Reminder dismissed");
    }

    /// <summary>
    /// Complete a care gap
    /// </summary>
    [HttpPost("care-gaps/{reminderId}/complete")]
    [Authorize(Policy = "PatientEdit")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> CompleteCareGap(
        int reminderId,
        [FromQuery] int userId,
        [FromBody] CompleteCareGapRequest? request = null)
    {
        var success = await _cdsService.CompleteCareGapAsync(reminderId, userId, request?.Notes);

        if (!success)
            return ApiBadRequest("Unable to complete care gap");

        return ApiOk("Care gap marked as complete");
    }

    /// <summary>
    /// Recalculate care gaps for a patient
    /// </summary>
    [HttpPost("care-gaps/patient/{patientId}/recalculate")]
    [Authorize(Policy = "PatientEdit")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> RecalculateCareGaps(int patientId)
    {
        await _cdsService.RecalculateCareGapsAsync(patientId);
        return ApiOk("Care gaps recalculated");
    }

    #endregion

    #region Diagnosis Suggestions

    /// <summary>
    /// Get diagnosis suggestions based on symptoms and findings
    /// </summary>
    [HttpPost("diagnosis/suggest")]
    [Authorize(Policy = "MedicalRecordCreate")]
    [ProducesResponseType(typeof(ApiResponse<DiagnosisSuggestionResultDto>), 200)]
    public async Task<IActionResult> GetDiagnosisSuggestions(
        [FromBody] DiagnosisSuggestionRequestDto request)
    {
        var result = await _cdsService.GetDiagnosisSuggestionsAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get differential diagnoses for a symptom set
    /// </summary>
    [HttpPost("diagnosis/differential")]
    [Authorize(Policy = "MedicalRecordCreate")]
    [ProducesResponseType(typeof(ApiResponse<List<DiagnosisSuggestionDto>>), 200)]
    public async Task<IActionResult> GetDifferentialDiagnoses(
        [FromBody] DifferentialDiagnosisRequest request)
    {
        var suggestions = await _cdsService.GetDifferentialDiagnosesAsync(request.Symptoms, request.PatientId);
        return ApiOk(suggestions);
    }

    /// <summary>
    /// Get red flag symptoms that require immediate attention
    /// </summary>
    [HttpPost("diagnosis/red-flags")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
    public async Task<IActionResult> GetRedFlags([FromBody] RedFlagsRequest request)
    {
        var redFlags = await _cdsService.GetRedFlagsAsync(request.Symptoms, request.ChiefComplaint);
        return ApiOk(redFlags);
    }

    #endregion

    #region Dosage Checking

    /// <summary>
    /// Check if dosage is appropriate for patient
    /// </summary>
    [HttpPost("dosage/check")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<DosageCheckResultDto>), 200)]
    public async Task<IActionResult> CheckDosage(
        [FromBody] DosageCheckRequestDto request)
    {
        var result = await _cdsService.CheckDosageAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get recommended dosage for patient based on factors
    /// </summary>
    [HttpGet("dosage/patient/{patientId}/recommend/{medicationCode}")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<DosageRecommendationDto>), 200)]
    public async Task<IActionResult> GetRecommendedDosage(
        int patientId,
        string medicationCode,
        [FromQuery] string? indication = null)
    {
        var recommendation = await _cdsService.GetRecommendedDosageAsync(patientId, medicationCode, indication);
        return ApiOk(recommendation);
    }

    /// <summary>
    /// Get maximum daily dose for a medication
    /// </summary>
    [HttpGet("dosage/max-daily/{medicationCode}")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<DosageRangeDto>), 200)]
    public async Task<IActionResult> GetMaxDailyDose(
        string medicationCode,
        [FromQuery] int? patientAge = null,
        [FromQuery] decimal? patientWeight = null)
    {
        var range = await _cdsService.GetMaxDailyDoseAsync(medicationCode, patientAge, patientWeight);
        return ApiOk(range);
    }

    #endregion

    #region Contraindication Checking

    /// <summary>
    /// Check for medication contraindications based on patient conditions
    /// </summary>
    [HttpPost("contraindications/check")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<ContraindicationCheckResultDto>), 200)]
    public async Task<IActionResult> CheckContraindications(
        [FromBody] ContraindicationCheckRequestDto request)
    {
        var result = await _cdsService.CheckContraindicationsAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get all medications contraindicated for a patient
    /// </summary>
    [HttpGet("contraindications/patient/{patientId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ContraindicationAlertDto>>), 200)]
    public async Task<IActionResult> GetPatientContraindications(
        int patientId)
    {
        var contraindications = await _cdsService.GetPatientContraindicationsAsync(patientId);
        return ApiOk(contraindications);
    }

    /// <summary>
    /// Check pregnancy/lactation contraindications
    /// </summary>
    [HttpGet("contraindications/pregnancy/patient/{patientId}/{medicationCode}")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<ContraindicationCheckResultDto>), 200)]
    public async Task<IActionResult> CheckPregnancyContraindications(
        int patientId,
        string medicationCode)
    {
        var result = await _cdsService.CheckPregnancyContraindicationsAsync(patientId, medicationCode);
        return ApiOk(result);
    }

    #endregion

    #region Lab Interpretation

    /// <summary>
    /// Interpret a lab result
    /// </summary>
    [HttpPost("labs/interpret")]
    [Authorize(Policy = "LabResultView")]
    [ProducesResponseType(typeof(ApiResponse<LabInterpretationResultDto>), 200)]
    public async Task<IActionResult> InterpretLabResult(
        [FromBody] LabInterpretationRequestDto request)
    {
        var result = await _cdsService.InterpretLabResultAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get lab value trends and analysis
    /// </summary>
    [HttpGet("labs/patient/{patientId}/trends/{labCode}")]
    [Authorize(Policy = "LabResultView")]
    [ProducesResponseType(typeof(ApiResponse<TrendAnalysisDto>), 200)]
    public async Task<IActionResult> GetLabTrendAnalysis(
        int patientId,
        string labCode,
        [FromQuery] int? monthsBack = 12)
    {
        var trends = await _cdsService.GetLabTrendAnalysisAsync(patientId, labCode, monthsBack);
        return ApiOk(trends);
    }

    /// <summary>
    /// Get critical lab values requiring immediate attention
    /// </summary>
    [HttpGet("labs/patient/{patientId}/critical")]
    [Authorize(Policy = "LabResultView")]
    [ProducesResponseType(typeof(ApiResponse<List<LabInterpretationResultDto>>), 200)]
    public async Task<IActionResult> GetCriticalLabs(int patientId)
    {
        var criticalLabs = await _cdsService.GetCriticalLabsAsync(patientId);
        return ApiOk(criticalLabs);
    }

    /// <summary>
    /// Get suggested follow-up labs based on current results
    /// </summary>
    [HttpGet("labs/follow-up-suggestions")]
    [Authorize(Policy = "LabResultView")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
    public async Task<IActionResult> GetSuggestedFollowUpLabs(
        [FromQuery] int patientId,
        [FromQuery] string labCode,
        [FromQuery] decimal value)
    {
        var suggestions = await _cdsService.GetSuggestedFollowUpLabsAsync(patientId, labCode, value);
        return ApiOk(suggestions);
    }

    #endregion

    #region Clinical Guidelines

    /// <summary>
    /// Get clinical guidelines for a condition
    /// </summary>
    [HttpGet("guidelines/condition/{conditionCode}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalGuidelineDto>>), 200)]
    public async Task<IActionResult> GetGuidelines(string conditionCode)
    {
        var guidelines = await _cdsService.GetGuidelinesAsync(conditionCode);
        return ApiOk(guidelines);
    }

    /// <summary>
    /// Get applicable guidelines for a patient
    /// </summary>
    [HttpGet("guidelines/patient/{patientId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalGuidelineDto>>), 200)]
    public async Task<IActionResult> GetPatientGuidelines(int patientId)
    {
        var guidelines = await _cdsService.GetPatientGuidelinesAsync(patientId);
        return ApiOk(guidelines);
    }

    /// <summary>
    /// Search clinical guidelines
    /// </summary>
    [HttpGet("guidelines/search")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalGuidelineDto>>), 200)]
    public async Task<IActionResult> SearchGuidelines(
        [FromQuery] string searchTerm,
        [FromQuery] string? category = null)
    {
        var guidelines = await _cdsService.SearchGuidelinesAsync(searchTerm, category);
        return ApiOk(guidelines);
    }

    #endregion

    #region Order Sets

    /// <summary>
    /// Get available order sets
    /// </summary>
    [HttpGet("order-sets")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalOrderSetDto>>), 200)]
    public async Task<IActionResult> GetOrderSets(
        [FromQuery] string? category = null,
        [FromQuery] string? conditionCode = null)
    {
        var orderSets = await _cdsService.GetOrderSetsAsync(category, conditionCode);
        return ApiOk(orderSets);
    }

    /// <summary>
    /// Get order set by ID
    /// </summary>
    [HttpGet("order-sets/{orderSetId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalOrderSetDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetOrderSetById(int orderSetId)
    {
        var orderSet = await _cdsService.GetOrderSetByIdAsync(orderSetId);

        if (orderSet == null)
            return ApiNotFound("Order set not found");

        return ApiOk(orderSet);
    }

    /// <summary>
    /// Get recommended order sets for patient's conditions
    /// </summary>
    [HttpGet("order-sets/patient/{patientId}/recommended")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalOrderSetDto>>), 200)]
    public async Task<IActionResult> GetRecommendedOrderSets(int patientId)
    {
        var orderSets = await _cdsService.GetRecommendedOrderSetsAsync(patientId);
        return ApiOk(orderSets);
    }

    /// <summary>
    /// Create a new order set
    /// </summary>
    [HttpPost("order-sets")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalOrderSetDto>), 201)]
    public async Task<IActionResult> CreateOrderSet(
        [FromBody] ClinicalOrderSetDto orderSet,
        [FromQuery] int userId)
    {
        var created = await _cdsService.CreateOrderSetAsync(orderSet, userId);
        return ApiCreated(created, message: "Order set created successfully");
    }

    /// <summary>
    /// Update an order set
    /// </summary>
    [HttpPut("order-sets/{orderSetId}")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalOrderSetDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateOrderSet(
        int orderSetId,
        [FromBody] ClinicalOrderSetDto orderSet,
        [FromQuery] int userId)
    {
        var updated = await _cdsService.UpdateOrderSetAsync(orderSetId, orderSet, userId);

        if (updated == null)
            return ApiNotFound("Order set not found");

        return ApiOk(updated, "Order set updated successfully");
    }

    /// <summary>
    /// Delete an order set
    /// </summary>
    [HttpDelete("order-sets/{orderSetId}")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteOrderSet(int orderSetId, [FromQuery] int userId)
    {
        var success = await _cdsService.DeleteOrderSetAsync(orderSetId, userId);

        if (!success)
            return ApiNotFound("Order set not found");

        return ApiOk("Order set deleted successfully");
    }

    #endregion

    #region Risk Calculators

    /// <summary>
    /// Get available risk calculators
    /// </summary>
    [HttpGet("risk-calculators")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<RiskCalculatorDto>>), 200)]
    public async Task<IActionResult> GetRiskCalculators(
        [FromQuery] string? category = null)
    {
        var calculators = await _cdsService.GetRiskCalculatorsAsync(category);
        return ApiOk(calculators);
    }

    /// <summary>
    /// Calculate risk score
    /// </summary>
    [HttpPost("risk-calculators/calculate")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<RiskCalculationResultDto>), 200)]
    public async Task<IActionResult> CalculateRisk(
        [FromBody] RiskCalculationRequestDto request)
    {
        var result = await _cdsService.CalculateRiskAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get patient's risk scores history
    /// </summary>
    [HttpGet("risk-calculators/patient/{patientId}/history/{calculatorId}")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<List<RiskCalculationResultDto>>), 200)]
    public async Task<IActionResult> GetPatientRiskHistory(
        int patientId,
        string calculatorId)
    {
        var history = await _cdsService.GetPatientRiskHistoryAsync(patientId, calculatorId);
        return ApiOk(history);
    }

    /// <summary>
    /// Calculate cardiovascular risk (ASCVD)
    /// </summary>
    [HttpGet("risk-calculators/patient/{patientId}/ascvd")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<RiskCalculationResultDto>), 200)]
    public async Task<IActionResult> CalculateAscvdRisk(int patientId)
    {
        var result = await _cdsService.CalculateAscvdRiskAsync(patientId);
        return ApiOk(result);
    }

    /// <summary>
    /// Calculate diabetes risk
    /// </summary>
    [HttpGet("risk-calculators/patient/{patientId}/diabetes")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<RiskCalculationResultDto>), 200)]
    public async Task<IActionResult> CalculateDiabetesRisk(int patientId)
    {
        var result = await _cdsService.CalculateDiabetesRiskAsync(patientId);
        return ApiOk(result);
    }

    /// <summary>
    /// Calculate fall risk
    /// </summary>
    [HttpGet("risk-calculators/patient/{patientId}/fall")]
    [Authorize(Policy = "PatientView")]
    [ProducesResponseType(typeof(ApiResponse<RiskCalculationResultDto>), 200)]
    public async Task<IActionResult> CalculateFallRisk(int patientId)
    {
        var result = await _cdsService.CalculateFallRiskAsync(patientId);
        return ApiOk(result);
    }

    #endregion

    #region Alert Management

    /// <summary>
    /// Get alert configuration
    /// </summary>
    [HttpGet("alerts/config")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicalAlertConfigDto>>), 200)]
    public async Task<IActionResult> GetAlertConfigurations()
    {
        var configs = await _cdsService.GetAlertConfigurationsAsync();
        return ApiOk(configs);
    }

    /// <summary>
    /// Update alert configuration
    /// </summary>
    [HttpPut("alerts/config")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalAlertConfigDto>), 200)]
    public async Task<IActionResult> UpdateAlertConfiguration(
        [FromBody] ClinicalAlertConfigDto config,
        [FromQuery] int userId)
    {
        var updated = await _cdsService.UpdateAlertConfigurationAsync(config, userId);
        return ApiOk(updated, "Alert configuration updated");
    }

    /// <summary>
    /// Override a clinical alert
    /// </summary>
    [HttpPost("alerts/{alertId}/override")]
    [Authorize(Policy = "PatientEdit")]
    [ProducesResponseType(typeof(ApiResponse<AlertOverrideDto>), 200)]
    public async Task<IActionResult> OverrideAlert(
        int alertId,
        [FromBody] OverrideAlertRequest request,
        [FromQuery] int userId)
    {
        var override_ = await _cdsService.OverrideAlertAsync(
            alertId,
            request.AlertType,
            request.Reason,
            userId,
            request.PatientId);

        return ApiOk(override_, "Alert overridden");
    }

    /// <summary>
    /// Get alert overrides for audit
    /// </summary>
    [HttpGet("alerts/overrides")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<List<AlertOverrideDto>>), 200)]
    public async Task<IActionResult> GetAlertOverrides(
        [FromQuery] int? patientId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var overrides = await _cdsService.GetAlertOverridesAsync(patientId, userId, fromDate, toDate);
        return ApiOk(overrides);
    }

    /// <summary>
    /// Review an alert override
    /// </summary>
    [HttpPost("alerts/overrides/{overrideId}/review")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> ReviewAlertOverride(
        int overrideId,
        [FromQuery] int reviewedByUserId,
        [FromBody] ReviewOverrideRequest? request = null)
    {
        var success = await _cdsService.ReviewAlertOverrideAsync(overrideId, reviewedByUserId, request?.Notes);

        if (!success)
            return ApiBadRequest("Unable to review override");

        return ApiOk("Override reviewed");
    }

    #endregion

    #region Comprehensive Safety Check

    /// <summary>
    /// Perform comprehensive medication safety check
    /// </summary>
    [HttpPost("safety-check")]
    [Authorize(Policy = "PrescriptionCreate")]
    [ProducesResponseType(typeof(ApiResponse<MedicationSafetyCheckResultDto>), 200)]
    public async Task<IActionResult> PerformMedicationSafetyCheck(
        [FromQuery] int patientId,
        [FromQuery] string medicationCode,
        [FromBody] DosageCheckRequestDto? dosageInfo = null)
    {
        var result = await _cdsService.PerformMedicationSafetyCheckAsync(patientId, medicationCode, dosageInfo);
        return ApiOk(result);
    }

    #endregion
}

#region Request Models

public class DismissReminderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CompleteCareGapRequest
{
    public string? Notes { get; set; }
}

public class DifferentialDiagnosisRequest
{
    public List<string> Symptoms { get; set; } = new();
    public int? PatientId { get; set; }
}

public class RedFlagsRequest
{
    public List<string> Symptoms { get; set; } = new();
    public string? ChiefComplaint { get; set; }
}

public class OverrideAlertRequest
{
    public string AlertType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int PatientId { get; set; }
}

public class ReviewOverrideRequest
{
    public string? Notes { get; set; }
}

#endregion
