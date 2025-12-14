using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Clinical Decision Support Service implementation
/// Provides evidence-based clinical recommendations, alerts, and decision support tools
/// </summary>
public class ClinicalDecisionSupportService : IClinicalDecisionSupportService
{
    private readonly ILogger<ClinicalDecisionSupportService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ClinicalDecisionSupportService(
        ILogger<ClinicalDecisionSupportService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Helper Methods

    /// <summary>
    /// Sanitize input for logging to prevent log injection attacks
    /// </summary>
    private static string SanitizeLogInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";

        // Remove newlines and control characters that could inject into logs
        return input
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("\t", " ")
            .Trim();
    }

    /// <summary>
    /// Validate patient ID
    /// </summary>
    private static void ValidatePatientId(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient ID", nameof(patientId));
    }

    /// <summary>
    /// Validate medication code
    /// </summary>
    private static void ValidateMedicationCode(string? medicationCode)
    {
        if (string.IsNullOrWhiteSpace(medicationCode))
            throw new ArgumentException("Medication code is required", nameof(medicationCode));
    }

    #endregion

    #region Drug Interaction Checking

    public async Task<DrugInteractionResultDto> CheckDrugInteractionsAsync(DrugInteractionCheckDto request)
    {
        _logger.LogInformation("Checking drug interactions for patient: {PatientId}", request.PatientId);

        // TODO: Integrate with drug interaction database (e.g., DrugBank, Medi-Span)
        // This is a placeholder implementation
        return await Task.FromResult(new DrugInteractionResultDto
        {
            PatientId = request.PatientId,
            CheckedAt = DateTime.UtcNow,
            TotalMedicationsChecked = request.MedicationCodes.Count,
            TotalInteractionsFound = 0,
            Interactions = new List<DrugInteractionAlertDto>(),
            HasContraindications = false,
            RequiresReview = false,
            OverallRiskLevel = "Low"
        });
    }

    public async Task<DrugInteractionResultDto> CheckNewMedicationInteractionsAsync(int patientId, string newMedicationCode)
    {
        _logger.LogInformation("Checking new medication {MedicationCode} interactions for patient: {PatientId}",
            newMedicationCode, patientId);

        // TODO: Get patient's current medications and check interactions
        return await CheckDrugInteractionsAsync(new DrugInteractionCheckDto
        {
            PatientId = patientId,
            NewMedicationCodes = new List<string> { newMedicationCode }
        });
    }

    public async Task<DrugInteractionResultDto> GetCurrentInteractionsAsync(int patientId)
    {
        _logger.LogInformation("Getting current interactions for patient: {PatientId}", patientId);

        // TODO: Get all current medication interactions
        return await Task.FromResult(new DrugInteractionResultDto
        {
            PatientId = patientId,
            CheckedAt = DateTime.UtcNow,
            TotalMedicationsChecked = 0,
            TotalInteractionsFound = 0,
            OverallRiskLevel = "Low"
        });
    }

    #endregion

    #region Allergy Checking

    public async Task<AllergyCheckResultDto> CheckAllergyContraindicationsAsync(AllergyCheckRequestDto request)
    {
        _logger.LogInformation("Checking allergy contraindications for patient: {PatientId}, medication: {MedicationCode}",
            request.PatientId, request.MedicationCode);

        // TODO: Check patient allergies against medication ingredients
        return await Task.FromResult(new AllergyCheckResultDto
        {
            PatientId = request.PatientId,
            MedicationCode = request.MedicationCode,
            MedicationName = request.MedicationName ?? "",
            CheckedAt = DateTime.UtcNow,
            HasContraindication = false,
            HasWarnings = false,
            Alerts = new List<AllergyAlertDto>(),
            SafeAlternatives = new List<string>(),
            CanPrescribeWithOverride = true
        });
    }

    public async Task<AllergyCheckResultDto> IsMedicationSafeAsync(int patientId, string medicationCode)
    {
        return await CheckAllergyContraindicationsAsync(new AllergyCheckRequestDto
        {
            PatientId = patientId,
            MedicationCode = medicationCode
        });
    }

    public async Task<List<string>> GetAllergyAlternativesAsync(int patientId, string medicationCode, string drugClass)
    {
        _logger.LogInformation("Getting allergy alternatives for patient: {PatientId}, drug class: {DrugClass}",
            patientId, drugClass);

        // TODO: Lookup alternative medications in the same drug class
        return await Task.FromResult(new List<string>());
    }

    #endregion

    #region Care Gaps & Clinical Reminders

    public async Task<PatientCareGapSummaryDto> GetPatientCareGapsAsync(int patientId)
    {
        _logger.LogInformation("Getting care gaps for patient: {PatientId}", patientId);

        // TODO: Calculate care gaps based on patient conditions, age, gender
        return await Task.FromResult(new PatientCareGapSummaryDto
        {
            PatientId = patientId,
            AsOfDate = DateTime.UtcNow,
            TotalGaps = 0,
            HighPriorityGaps = 0,
            OverdueGaps = 0,
            CareGaps = new List<ClinicalReminderDto>(),
            ComplianceScore = 100.0,
            RiskLevel = "Low"
        });
    }

    public async Task<List<ClinicalReminderDto>> GetClinicalRemindersAsync(int patientId, string? category = null)
    {
        _logger.LogInformation("Getting clinical reminders for patient: {PatientId}, category: {Category}",
            patientId, category);

        // TODO: Get applicable clinical reminders based on patient profile
        return await Task.FromResult(new List<ClinicalReminderDto>());
    }

    public async Task<List<PatientCareGapSummaryDto>> GetOverdueCareGapsAsync(int branchId, int limit = 100)
    {
        _logger.LogInformation("Getting overdue care gaps for branch: {BranchId}", branchId);

        // TODO: Query patients with overdue care gaps
        return await Task.FromResult(new List<PatientCareGapSummaryDto>());
    }

    public async Task<bool> AcknowledgeReminderAsync(int reminderId, int userId)
    {
        _logger.LogInformation("Acknowledging reminder: {ReminderId} by user: {UserId}", reminderId, userId);
        // TODO: Update reminder acknowledgment status
        return await Task.FromResult(true);
    }

    public async Task<bool> DismissReminderAsync(int reminderId, int userId, string reason)
    {
        _logger.LogInformation("Dismissing reminder: {ReminderId} by user: {UserId}, reason: {Reason}",
            reminderId, userId, reason);
        // TODO: Update reminder dismissal with reason
        return await Task.FromResult(true);
    }

    public async Task<bool> CompleteCareGapAsync(int reminderId, int userId, string? notes = null)
    {
        _logger.LogInformation("Completing care gap: {ReminderId} by user: {UserId}", reminderId, userId);
        // TODO: Mark care gap as completed
        return await Task.FromResult(true);
    }

    public async Task RecalculateCareGapsAsync(int patientId)
    {
        _logger.LogInformation("Recalculating care gaps for patient: {PatientId}", patientId);
        // TODO: Recalculate care gaps based on updated patient data
        await Task.CompletedTask;
    }

    #endregion

    #region Diagnosis Suggestions

    public async Task<DiagnosisSuggestionResultDto> GetDiagnosisSuggestionsAsync(DiagnosisSuggestionRequestDto request)
    {
        _logger.LogInformation("Getting diagnosis suggestions for patient: {PatientId}, symptoms: {SymptomCount}",
            request.PatientId, request.Symptoms.Count);

        // TODO: Implement AI-based diagnosis suggestion using symptoms, patient history
        return await Task.FromResult(new DiagnosisSuggestionResultDto
        {
            PatientId = request.PatientId,
            AnalyzedAt = DateTime.UtcNow,
            InputSummary = string.Join(", ", request.Symptoms),
            Suggestions = new List<DiagnosisSuggestionDto>(),
            RedFlags = new List<string>(),
            HasEmergentCondition = false,
            Disclaimer = "These suggestions are for clinical decision support only and do not replace clinical judgment."
        });
    }

    public async Task<List<DiagnosisSuggestionDto>> GetDifferentialDiagnosesAsync(List<string> symptoms, int? patientId = null)
    {
        _logger.LogInformation("Getting differential diagnoses for symptoms: {SymptomCount}", symptoms.Count);

        // TODO: Lookup differential diagnoses based on symptom combination
        return await Task.FromResult(new List<DiagnosisSuggestionDto>());
    }

    public async Task<List<string>> GetRedFlagsAsync(List<string> symptoms, string? chiefComplaint = null)
    {
        _logger.LogInformation("Checking red flags for symptoms");

        // TODO: Check for red flag symptoms that require immediate attention
        var redFlags = new List<string>();

        // Example red flag checks
        var emergentSymptoms = new[] { "chest pain", "shortness of breath", "severe headache", "loss of consciousness" };
        foreach (var symptom in symptoms)
        {
            if (emergentSymptoms.Any(e => symptom.Contains(e, StringComparison.OrdinalIgnoreCase)))
            {
                redFlags.Add($"ALERT: {symptom} may indicate an emergent condition requiring immediate evaluation.");
            }
        }

        return await Task.FromResult(redFlags);
    }

    #endregion

    #region Dosage Checking

    public async Task<DosageCheckResultDto> CheckDosageAsync(DosageCheckRequestDto request)
    {
        _logger.LogInformation("Checking dosage for patient: {PatientId}, medication: {MedicationCode}",
            request.PatientId, request.MedicationCode);

        // TODO: Check dosage against patient factors (age, weight, renal function, etc.)
        return await Task.FromResult(new DosageCheckResultDto
        {
            PatientId = request.PatientId,
            MedicationName = request.MedicationName,
            ProposedDose = request.ProposedDose,
            DoseUnit = request.DoseUnit,
            IsDoseAppropriate = true,
            DoseStatus = "Normal",
            Alerts = new List<DosageAlertDto>(),
            RequiresRenalAdjustment = false,
            RequiresHepaticAdjustment = false,
            RequiresAgeAdjustment = false,
            RequiresWeightAdjustment = false
        });
    }

    public async Task<DosageRecommendationDto> GetRecommendedDosageAsync(int patientId, string medicationCode, string? indication = null)
    {
        _logger.LogInformation("Getting recommended dosage for patient: {PatientId}, medication: {MedicationCode}",
            patientId, medicationCode);

        // TODO: Calculate recommended dosage based on patient factors
        return await Task.FromResult(new DosageRecommendationDto
        {
            RecommendedDose = 0,
            Unit = "mg",
            Frequency = "Once daily",
            Rationale = "Standard adult dosing",
            AdjustmentFactors = new List<string>()
        });
    }

    public async Task<DosageRangeDto> GetMaxDailyDoseAsync(string medicationCode, int? patientAge = null, decimal? patientWeight = null)
    {
        _logger.LogInformation("Getting max daily dose for medication: {MedicationCode}", medicationCode);

        // TODO: Lookup max daily dose from drug database
        return await Task.FromResult(new DosageRangeDto
        {
            MinDose = 0,
            MaxDose = 0,
            Unit = "mg"
        });
    }

    #endregion

    #region Contraindication Checking

    public async Task<ContraindicationCheckResultDto> CheckContraindicationsAsync(ContraindicationCheckRequestDto request)
    {
        _logger.LogInformation("Checking contraindications for patient: {PatientId}, medication: {MedicationCode}",
            request.PatientId, request.MedicationCode);

        // TODO: Check patient conditions against medication contraindications
        return await Task.FromResult(new ContraindicationCheckResultDto
        {
            PatientId = request.PatientId,
            MedicationCode = request.MedicationCode,
            MedicationName = request.MedicationName ?? "",
            CheckedAt = DateTime.UtcNow,
            HasAbsoluteContraindication = false,
            HasRelativeContraindication = false,
            HasPrecautions = false,
            Alerts = new List<ContraindicationAlertDto>(),
            CanPrescribe = true,
            PrescribingDecision = "Safe"
        });
    }

    public async Task<List<ContraindicationAlertDto>> GetPatientContraindicationsAsync(int patientId)
    {
        _logger.LogInformation("Getting all contraindications for patient: {PatientId}", patientId);

        // TODO: Get all medications contraindicated for patient's conditions
        return await Task.FromResult(new List<ContraindicationAlertDto>());
    }

    public async Task<ContraindicationCheckResultDto> CheckPregnancyContraindicationsAsync(int patientId, string medicationCode)
    {
        _logger.LogInformation("Checking pregnancy contraindications for patient: {PatientId}, medication: {MedicationCode}",
            patientId, medicationCode);

        // TODO: Check FDA pregnancy category and lactation safety
        return await CheckContraindicationsAsync(new ContraindicationCheckRequestDto
        {
            PatientId = patientId,
            MedicationCode = medicationCode
        });
    }

    #endregion

    #region Lab Interpretation

    public async Task<LabInterpretationResultDto> InterpretLabResultAsync(LabInterpretationRequestDto request)
    {
        _logger.LogInformation("Interpreting lab result for patient: {PatientId}, lab: {LabCode}",
            request.PatientId, request.LabCode);

        // TODO: Interpret lab values against reference ranges and patient context
        return await Task.FromResult(new LabInterpretationResultDto
        {
            PatientId = request.PatientId,
            LabCode = request.LabCode,
            LabName = request.LabName,
            Value = request.Value,
            Unit = request.Unit,
            Status = "Normal",
            Flag = "N",
            Interpretation = "Within normal limits",
            PossibleCauses = new List<string>(),
            RecommendedActions = new List<string>(),
            IsCritical = false
        });
    }

    public async Task<TrendAnalysisDto> GetLabTrendAnalysisAsync(int patientId, string labCode, int? monthsBack = 12)
    {
        _logger.LogInformation("Getting lab trend analysis for patient: {PatientId}, lab: {LabCode}",
            patientId, labCode);

        // TODO: Analyze historical lab values for trends
        return await Task.FromResult(new TrendAnalysisDto
        {
            TrendDirection = "Stable",
            ChangePercent = 0,
            ChangeSignificance = "Not significant",
            HistoricalValues = new List<LabValueDto>(),
            TrendInterpretation = "Values have remained stable"
        });
    }

    public async Task<List<LabInterpretationResultDto>> GetCriticalLabsAsync(int patientId)
    {
        _logger.LogInformation("Getting critical labs for patient: {PatientId}", patientId);

        // TODO: Get recent labs with critical values
        return await Task.FromResult(new List<LabInterpretationResultDto>());
    }

    public async Task<List<string>> GetSuggestedFollowUpLabsAsync(int patientId, string labCode, decimal value)
    {
        _logger.LogInformation("Getting suggested follow-up labs for patient: {PatientId}, lab: {LabCode}",
            patientId, labCode);

        // TODO: Suggest follow-up labs based on abnormal results
        return await Task.FromResult(new List<string>());
    }

    #endregion

    #region Clinical Guidelines

    public async Task<List<ClinicalGuidelineDto>> GetGuidelinesAsync(string conditionCode)
    {
        _logger.LogInformation("Getting clinical guidelines for condition: {ConditionCode}", conditionCode);

        // TODO: Lookup clinical guidelines from guideline database
        return await Task.FromResult(new List<ClinicalGuidelineDto>());
    }

    public async Task<List<ClinicalGuidelineDto>> GetPatientGuidelinesAsync(int patientId)
    {
        _logger.LogInformation("Getting applicable guidelines for patient: {PatientId}", patientId);

        // TODO: Get guidelines applicable to patient's conditions
        return await Task.FromResult(new List<ClinicalGuidelineDto>());
    }

    public async Task<List<ClinicalGuidelineDto>> SearchGuidelinesAsync(string searchTerm, string? category = null)
    {
        _logger.LogInformation("Searching guidelines for: {SearchTerm}", searchTerm);

        // TODO: Search guideline database
        return await Task.FromResult(new List<ClinicalGuidelineDto>());
    }

    #endregion

    #region Order Sets

    public async Task<List<ClinicalOrderSetDto>> GetOrderSetsAsync(string? category = null, string? conditionCode = null)
    {
        _logger.LogInformation("Getting order sets, category: {Category}, condition: {ConditionCode}",
            category, conditionCode);

        // TODO: Get order sets from database
        return await Task.FromResult(new List<ClinicalOrderSetDto>());
    }

    public async Task<ClinicalOrderSetDto?> GetOrderSetByIdAsync(int orderSetId)
    {
        _logger.LogInformation("Getting order set: {OrderSetId}", orderSetId);

        // TODO: Get order set by ID
        return await Task.FromResult<ClinicalOrderSetDto?>(null);
    }

    public async Task<List<ClinicalOrderSetDto>> GetRecommendedOrderSetsAsync(int patientId)
    {
        _logger.LogInformation("Getting recommended order sets for patient: {PatientId}", patientId);

        // TODO: Get order sets recommended for patient's conditions
        return await Task.FromResult(new List<ClinicalOrderSetDto>());
    }

    public async Task<ClinicalOrderSetDto> CreateOrderSetAsync(ClinicalOrderSetDto orderSet, int createdByUserId)
    {
        _logger.LogInformation("Creating order set: {Name} by user: {UserId}", orderSet.Name, createdByUserId);

        // TODO: Create new order set
        orderSet.CreatedAt = DateTime.UtcNow;
        return await Task.FromResult(orderSet);
    }

    public async Task<ClinicalOrderSetDto?> UpdateOrderSetAsync(int orderSetId, ClinicalOrderSetDto orderSet, int updatedByUserId)
    {
        _logger.LogInformation("Updating order set: {OrderSetId} by user: {UserId}", orderSetId, updatedByUserId);

        // TODO: Update order set
        return await Task.FromResult<ClinicalOrderSetDto?>(orderSet);
    }

    public async Task<bool> DeleteOrderSetAsync(int orderSetId, int deletedByUserId)
    {
        _logger.LogInformation("Deleting order set: {OrderSetId} by user: {UserId}", orderSetId, deletedByUserId);

        // TODO: Delete order set
        return await Task.FromResult(true);
    }

    #endregion

    #region Risk Calculators

    public async Task<List<RiskCalculatorDto>> GetRiskCalculatorsAsync(string? category = null)
    {
        _logger.LogInformation("Getting risk calculators, category: {Category}", category);

        // TODO: Get available risk calculators
        return await Task.FromResult(new List<RiskCalculatorDto>
        {
            new RiskCalculatorDto
            {
                CalculatorId = "ascvd",
                Name = "ASCVD Risk Calculator",
                Description = "10-year cardiovascular risk",
                Category = "Cardiovascular"
            },
            new RiskCalculatorDto
            {
                CalculatorId = "diabetes-risk",
                Name = "Diabetes Risk Score",
                Description = "Type 2 diabetes risk assessment",
                Category = "Metabolic"
            },
            new RiskCalculatorDto
            {
                CalculatorId = "fall-risk",
                Name = "Fall Risk Assessment",
                Description = "Risk of falls in elderly patients",
                Category = "Geriatric"
            }
        });
    }

    public async Task<RiskCalculationResultDto> CalculateRiskAsync(RiskCalculationRequestDto request)
    {
        _logger.LogInformation("Calculating risk for patient: {PatientId}, calculator: {CalculatorId}",
            request.PatientId, request.CalculatorId);

        // TODO: Implement risk calculation algorithms
        return await Task.FromResult(new RiskCalculationResultDto
        {
            PatientId = request.PatientId,
            CalculatorName = request.CalculatorId,
            CalculatedAt = DateTime.UtcNow,
            RiskScore = 0,
            RiskLevel = "Low",
            RiskPercentage = "0%",
            TimeFrame = "10-year",
            Interpretation = "Risk calculation placeholder",
            ContributingFactors = new List<RiskFactorDto>(),
            Recommendations = new List<string>(),
            ModifiableFactors = new List<RiskModificationDto>(),
            InputsUsed = request.InputValues
        });
    }

    public async Task<List<RiskCalculationResultDto>> GetPatientRiskHistoryAsync(int patientId, string calculatorId)
    {
        _logger.LogInformation("Getting risk history for patient: {PatientId}, calculator: {CalculatorId}",
            patientId, calculatorId);

        // TODO: Get historical risk calculations
        return await Task.FromResult(new List<RiskCalculationResultDto>());
    }

    public async Task<RiskCalculationResultDto> CalculateAscvdRiskAsync(int patientId)
    {
        return await CalculateRiskAsync(new RiskCalculationRequestDto
        {
            PatientId = patientId,
            CalculatorId = "ascvd",
            UsePatientData = true
        });
    }

    public async Task<RiskCalculationResultDto> CalculateDiabetesRiskAsync(int patientId)
    {
        return await CalculateRiskAsync(new RiskCalculationRequestDto
        {
            PatientId = patientId,
            CalculatorId = "diabetes-risk",
            UsePatientData = true
        });
    }

    public async Task<RiskCalculationResultDto> CalculateFallRiskAsync(int patientId)
    {
        return await CalculateRiskAsync(new RiskCalculationRequestDto
        {
            PatientId = patientId,
            CalculatorId = "fall-risk",
            UsePatientData = true
        });
    }

    #endregion

    #region Alert Management

    public async Task<List<ClinicalAlertConfigDto>> GetAlertConfigurationsAsync()
    {
        _logger.LogInformation("Getting alert configurations");

        // TODO: Get alert configurations from database
        return await Task.FromResult(new List<ClinicalAlertConfigDto>
        {
            new ClinicalAlertConfigDto
            {
                ConfigId = 1,
                AlertType = "DrugInteraction",
                IsEnabled = true,
                Severity = "High",
                RequiresAcknowledgment = true,
                AllowOverride = true
            },
            new ClinicalAlertConfigDto
            {
                ConfigId = 2,
                AlertType = "AllergyContraindication",
                IsEnabled = true,
                Severity = "Critical",
                RequiresAcknowledgment = true,
                AllowOverride = false
            }
        });
    }

    public async Task<ClinicalAlertConfigDto> UpdateAlertConfigurationAsync(ClinicalAlertConfigDto config, int updatedByUserId)
    {
        _logger.LogInformation("Updating alert configuration: {ConfigId} by user: {UserId}",
            config.ConfigId, updatedByUserId);

        // TODO: Update alert configuration
        return await Task.FromResult(config);
    }

    public async Task<AlertOverrideDto> OverrideAlertAsync(int alertId, string alertType, string reason, int userId, int patientId)
    {
        _logger.LogInformation("Overriding alert: {AlertId} type: {AlertType} by user: {UserId}, reason: {Reason}",
            alertId, alertType, userId, reason);

        // TODO: Record alert override for audit
        return await Task.FromResult(new AlertOverrideDto
        {
            OverrideId = 1,
            AlertId = alertId,
            AlertType = alertType,
            OverrideReason = reason,
            OverriddenByUserId = userId,
            OverriddenAt = DateTime.UtcNow,
            PatientId = patientId
        });
    }

    public async Task<List<AlertOverrideDto>> GetAlertOverridesAsync(int? patientId = null, int? userId = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        _logger.LogInformation("Getting alert overrides");

        // TODO: Query alert overrides for audit
        return await Task.FromResult(new List<AlertOverrideDto>());
    }

    public async Task<bool> ReviewAlertOverrideAsync(int overrideId, int reviewedByUserId, string? notes = null)
    {
        _logger.LogInformation("Reviewing alert override: {OverrideId} by user: {UserId}",
            overrideId, reviewedByUserId);

        // TODO: Mark override as reviewed
        return await Task.FromResult(true);
    }

    #endregion

    #region Comprehensive Safety Check

    public async Task<MedicationSafetyCheckResultDto> PerformMedicationSafetyCheckAsync(
        int patientId, string medicationCode, DosageCheckRequestDto? dosageInfo = null)
    {
        // Input validation
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient ID", nameof(patientId));

        if (string.IsNullOrWhiteSpace(medicationCode))
            throw new ArgumentException("Medication code is required", nameof(medicationCode));

        // Sanitize medication code for logging (prevent log injection)
        var sanitizedMedCode = SanitizeLogInput(medicationCode);
        _logger.LogInformation("Performing comprehensive safety check for patient: {PatientId}, medication: {MedicationCode}",
            patientId, sanitizedMedCode);

        // Run all safety checks in parallel for better performance
        var allergyCheckTask = CheckAllergyContraindicationsAsync(new AllergyCheckRequestDto
        {
            PatientId = patientId,
            MedicationCode = medicationCode
        });

        var interactionCheckTask = CheckNewMedicationInteractionsAsync(patientId, medicationCode);

        var contraindicationCheckTask = CheckContraindicationsAsync(new ContraindicationCheckRequestDto
        {
            PatientId = patientId,
            MedicationCode = medicationCode
        });

        // Wait for all tasks to complete
        await Task.WhenAll(allergyCheckTask, interactionCheckTask, contraindicationCheckTask);

        var allergyCheck = await allergyCheckTask;
        var interactionCheck = await interactionCheckTask;
        var contraindicationCheck = await contraindicationCheckTask;

        DosageCheckResultDto? dosageCheck = null;
        if (dosageInfo != null)
        {
            dosageCheck = await CheckDosageAsync(dosageInfo);
        }

        // Determine overall safety
        var isSafe = !allergyCheck.HasContraindication &&
                     !interactionCheck.HasContraindications &&
                     !contraindicationCheck.HasAbsoluteContraindication;

        var safetyLevel = "Safe";
        if (!isSafe)
        {
            safetyLevel = "Contraindicated";
        }
        else if (allergyCheck.HasWarnings || interactionCheck.RequiresReview || contraindicationCheck.HasPrecautions)
        {
            safetyLevel = "ProceedWithCaution";
        }

        var warnings = new List<string>();
        var recommendations = new List<string>();

        if (allergyCheck.Alerts.Any())
        {
            warnings.AddRange(allergyCheck.Alerts.Select(a => a.Description));
        }

        if (interactionCheck.Interactions.Any())
        {
            warnings.AddRange(interactionCheck.Interactions.Select(i => i.Description));
        }

        if (contraindicationCheck.Alerts.Any())
        {
            warnings.AddRange(contraindicationCheck.Alerts.Select(c => c.Description));
            recommendations.AddRange(contraindicationCheck.Alerts.Select(c => c.Recommendation));
        }

        return new MedicationSafetyCheckResultDto
        {
            PatientId = patientId,
            MedicationCode = medicationCode,
            CheckedAt = DateTime.UtcNow,
            IsSafe = isSafe,
            SafetyLevel = safetyLevel,
            AllergyCheck = allergyCheck,
            InteractionCheck = interactionCheck,
            ContraindicationCheck = contraindicationCheck,
            DosageCheck = dosageCheck,
            Warnings = warnings,
            Recommendations = recommendations,
            RequiresOverride = !isSafe
        };
    }

    #endregion
}
