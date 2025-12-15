using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.DTOs;

#region Drug Interactions

/// <summary>
/// Drug interaction check request
/// </summary>
public class DrugInteractionCheckDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive integer")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "At least one medication code is required")]
    [MinLength(1, ErrorMessage = "At least one medication code is required")]
    public List<string> MedicationCodes { get; set; } = new();

    public List<string>? NewMedicationCodes { get; set; }
    public bool IncludeOtcMedications { get; set; } = true;
    public bool IncludeSupplements { get; set; } = true;
}

/// <summary>
/// Drug interaction alert
/// </summary>
public class DrugInteractionAlertDto
{
    public int AlertId { get; set; }
    public string Drug1Name { get; set; } = string.Empty;
    public string Drug1Code { get; set; } = string.Empty;
    public string Drug2Name { get; set; } = string.Empty;
    public string Drug2Code { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Contraindicated, Major, Moderate, Minor
    public string SeverityLevel { get; set; } = string.Empty; // 1-5
    public string InteractionType { get; set; } = string.Empty; // Pharmacokinetic, Pharmacodynamic
    public string Description { get; set; } = string.Empty;
    public string ClinicalSignificance { get; set; } = string.Empty;
    public string ManagementGuidelines { get; set; } = string.Empty;
    public string? OnsetTiming { get; set; } // Rapid, Delayed
    public string? DocumentationLevel { get; set; } // Established, Probable, Suspected
    public List<string>? AffectedParameters { get; set; }
    public List<string>? MonitoringRecommendations { get; set; }
    public string? EvidenceSource { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Drug interaction check result
/// </summary>
public class DrugInteractionResultDto
{
    public int PatientId { get; set; }
    public DateTime CheckedAt { get; set; }
    public int TotalMedicationsChecked { get; set; }
    public int TotalInteractionsFound { get; set; }
    public int ContraindicatedCount { get; set; }
    public int MajorCount { get; set; }
    public int ModerateCount { get; set; }
    public int MinorCount { get; set; }
    public List<DrugInteractionAlertDto> Interactions { get; set; } = new();
    public bool HasContraindications { get; set; }
    public bool RequiresReview { get; set; }
    public string OverallRiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
}

#endregion

#region Allergy Contraindications

/// <summary>
/// Allergy contraindication check request
/// </summary>
public class AllergyCheckRequestDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive integer")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Medication code is required")]
    [StringLength(50, ErrorMessage = "Medication code cannot exceed 50 characters")]
    public string MedicationCode { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Medication name cannot exceed 200 characters")]
    public string? MedicationName { get; set; }

    public List<string>? ActiveIngredients { get; set; }
}

/// <summary>
/// Allergy alert
/// </summary>
public class AllergyAlertDto
{
    public int AlertId { get; set; }
    public string AllergenName { get; set; } = string.Empty;
    public string AllergenType { get; set; } = string.Empty; // Drug, DrugClass, Ingredient, CrossReactive
    public string MedicationName { get; set; } = string.Empty;
    public string MedicationCode { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Contraindicated, Warning, Caution
    public string PreviousReaction { get; set; } = string.Empty;
    public string PreviousReactionSeverity { get; set; } = string.Empty;
    public DateTime? ReactionDate { get; set; }
    public string MatchType { get; set; } = string.Empty; // Exact, Class, CrossReactive, Ingredient
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public List<string>? AlternativeMedications { get; set; }
    public bool IsCrossReactivity { get; set; }
    public string? CrossReactivityExplanation { get; set; }
}

/// <summary>
/// Allergy check result
/// </summary>
public class AllergyCheckResultDto
{
    public int PatientId { get; set; }
    public string MedicationCode { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public bool HasContraindication { get; set; }
    public bool HasWarnings { get; set; }
    public List<AllergyAlertDto> Alerts { get; set; } = new();
    public List<string> SafeAlternatives { get; set; } = new();
    public string OverrideReason { get; set; } = string.Empty;
    public bool CanPrescribeWithOverride { get; set; }
}

#endregion

#region Clinical Reminders & Care Gaps

/// <summary>
/// Clinical reminder / care gap
/// </summary>
public class ClinicalReminderDto
{
    public int ReminderId { get; set; }
    public int PatientId { get; set; }
    public string ReminderType { get; set; } = string.Empty; // Screening, Vaccination, LabTest, Assessment, Follow-up
    public string Category { get; set; } = string.Empty; // Preventive, ChronicCare, Medication, SafetyAlert
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    public string Status { get; set; } = string.Empty; // Due, Overdue, Upcoming, Completed, Dismissed
    public DateTime? DueDate { get; set; }
    public DateTime? LastCompletedDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? FrequencyCode { get; set; }
    public string? FrequencyDescription { get; set; }
    public string? EvidenceGrade { get; set; } // A, B, C
    public string? GuidelineSource { get; set; }
    public List<string>? ApplicableConditions { get; set; }
    public List<string>? ExclusionCriteria { get; set; }
    public ClinicalReminderActionDto? SuggestedAction { get; set; }
    public string? PatientEducationLink { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public string? DismissalReason { get; set; }
}

/// <summary>
/// Suggested action for clinical reminder
/// </summary>
public class ClinicalReminderActionDto
{
    public string ActionType { get; set; } = string.Empty; // OrderLab, OrderImaging, ScheduleVisit, Prescribe, Refer, Educate
    public string ActionDescription { get; set; } = string.Empty;
    public string? OrderCode { get; set; }
    public string? OrderName { get; set; }
    public string? AppointmentType { get; set; }
    public string? ReferralSpecialty { get; set; }
    public bool CanAutoOrder { get; set; }
}

/// <summary>
/// Care gap summary for a patient
/// </summary>
public class PatientCareGapSummaryDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public int TotalGaps { get; set; }
    public int HighPriorityGaps { get; set; }
    public int OverdueGaps { get; set; }
    public int CompletedThisYear { get; set; }
    public List<ClinicalReminderDto> CareGaps { get; set; } = new();
    public double ComplianceScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
}

#endregion

#region Diagnosis Suggestions

/// <summary>
/// Diagnosis suggestion request
/// </summary>
public class DiagnosisSuggestionRequestDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive integer")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "At least one symptom is required")]
    [MinLength(1, ErrorMessage = "At least one symptom is required")]
    public List<string> Symptoms { get; set; } = new();

    public List<string>? VitalSignAbnormalities { get; set; }
    public List<string>? LabAbnormalities { get; set; }

    [StringLength(500, ErrorMessage = "Chief complaint cannot exceed 500 characters")]
    public string? ChiefComplaint { get; set; }

    [Range(0, 150, ErrorMessage = "Patient age must be between 0 and 150")]
    public int? PatientAge { get; set; }

    [StringLength(20, ErrorMessage = "Patient gender cannot exceed 20 characters")]
    public string? PatientGender { get; set; }

    public List<string>? ExistingConditions { get; set; }
    public List<string>? CurrentMedications { get; set; }
    public bool IncludeDifferentials { get; set; } = true;

    [Range(1, 50, ErrorMessage = "Max suggestions must be between 1 and 50")]
    public int MaxSuggestions { get; set; } = 10;
}

/// <summary>
/// Diagnosis suggestion
/// </summary>
public class DiagnosisSuggestionDto
{
    public string IcdCode { get; set; } = string.Empty;
    public string DiagnosisName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double ConfidenceScore { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty; // High, Medium, Low
    public List<string> MatchingSymptoms { get; set; } = new();
    public List<string>? SupportingFindings { get; set; }
    public List<string>? ConflictingFindings { get; set; }
    public List<string>? AdditionalTestsRecommended { get; set; }
    public List<string>? DifferentialDiagnoses { get; set; }
    public string? ClinicalPearl { get; set; }
    public string? Urgency { get; set; } // Emergent, Urgent, Routine
    public bool RequiresSpecialistReferral { get; set; }
    public string? ReferralSpecialty { get; set; }
}

/// <summary>
/// Diagnosis suggestion result
/// </summary>
public class DiagnosisSuggestionResultDto
{
    public int PatientId { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public string InputSummary { get; set; } = string.Empty;
    public List<DiagnosisSuggestionDto> Suggestions { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();
    public bool HasEmergentCondition { get; set; }
    public string? EmergentConditionWarning { get; set; }
    public string Disclaimer { get; set; } = "These suggestions are for clinical decision support only and do not replace clinical judgment.";
}

#endregion

#region Dosage Checking

/// <summary>
/// Dosage check request
/// </summary>
public class DosageCheckRequestDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive integer")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Medication code is required")]
    [StringLength(50, ErrorMessage = "Medication code cannot exceed 50 characters")]
    public string MedicationCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Medication name is required")]
    [StringLength(200, ErrorMessage = "Medication name cannot exceed 200 characters")]
    public string MedicationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Proposed dose is required")]
    [Range(0.001, 100000, ErrorMessage = "Proposed dose must be between 0.001 and 100000")]
    public decimal ProposedDose { get; set; }

    [Required(ErrorMessage = "Dose unit is required")]
    [StringLength(20, ErrorMessage = "Dose unit cannot exceed 20 characters")]
    public string DoseUnit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Frequency is required")]
    [StringLength(50, ErrorMessage = "Frequency cannot exceed 50 characters")]
    public string Frequency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Route is required")]
    [StringLength(50, ErrorMessage = "Route cannot exceed 50 characters")]
    public string Route { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Indication cannot exceed 200 characters")]
    public string? Indication { get; set; }
}

/// <summary>
/// Dosage check result
/// </summary>
public class DosageCheckResultDto
{
    public int PatientId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public decimal ProposedDose { get; set; }
    public string DoseUnit { get; set; } = string.Empty;
    public bool IsDoseAppropriate { get; set; }
    public string DoseStatus { get; set; } = string.Empty; // Normal, Low, High, Excessive, SubTherapeutic
    public DosageRangeDto? RecommendedRange { get; set; }
    public DosageRangeDto? MaxDailyDose { get; set; }
    public List<DosageAlertDto> Alerts { get; set; } = new();
    public bool RequiresRenalAdjustment { get; set; }
    public bool RequiresHepaticAdjustment { get; set; }
    public bool RequiresAgeAdjustment { get; set; }
    public bool RequiresWeightAdjustment { get; set; }
    public DosageRecommendationDto? AdjustedDoseRecommendation { get; set; }
    public string? SpecialConsiderations { get; set; }
}

/// <summary>
/// Dosage range
/// </summary>
public class DosageRangeDto
{
    public decimal MinDose { get; set; }
    public decimal MaxDose { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public string? Indication { get; set; }
    public string? Population { get; set; } // Adult, Pediatric, Geriatric, Renal Impairment
}

/// <summary>
/// Dosage alert
/// </summary>
public class DosageAlertDto
{
    public string AlertType { get; set; } = string.Empty; // Overdose, Underdose, FrequencyIssue, DurationIssue
    public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
    public string Message { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Adjusted dose recommendation
/// </summary>
public class DosageRecommendationDto
{
    public decimal RecommendedDose { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public List<string> AdjustmentFactors { get; set; } = new();
}

#endregion

#region Diagnosis Support - Additional DTOs

/// <summary>
/// Diagnosis request DTO (alias for DiagnosisSuggestionRequestDto for backwards compatibility)
/// </summary>
public class DiagnosisRequestDto
{
    public List<string>? Symptoms { get; set; }
}

/// <summary>
/// Differential diagnosis DTO
/// </summary>
public class DifferentialDiagnosisDto
{
    public string IcdCode { get; set; } = string.Empty;
    public string DiagnosisName { get; set; } = string.Empty;
    public double Probability { get; set; }
    public List<string> DistinguishingFeatures { get; set; } = new();
}

/// <summary>
/// Red flag alert DTO
/// </summary>
public class RedFlagAlertDto
{
    public string Symptom { get; set; } = string.Empty;
    public string AlertLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public bool RequiresImmediateAttention { get; set; }
    public bool EscalationRequired { get; set; }
}

#endregion

#region Contraindication Checking

/// <summary>
/// Contraindication check request
/// </summary>
public class ContraindicationCheckRequestDto
{
    public int PatientId { get; set; }
    public string MedicationCode { get; set; } = string.Empty;
    public string? MedicationName { get; set; }
    public bool IncludeRelativeContraindications { get; set; } = true;
    public bool IncludePrecautions { get; set; } = true;
}

/// <summary>
/// Contraindication alert
/// </summary>
public class ContraindicationAlertDto
{
    public int AlertId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string MedicationCode { get; set; } = string.Empty;
    public string ContraindicationType { get; set; } = string.Empty; // Absolute, Relative, Precaution
    public string Reason { get; set; } = string.Empty;
    public string? ConditionName { get; set; }
    public string? IcdCode { get; set; }
    public string Severity { get; set; } = string.Empty; // Contraindicated, UseWithCaution, MonitorClosely
    public string Description { get; set; } = string.Empty;
    public string ClinicalRationale { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public List<string>? Alternatives { get; set; }
    public bool CanOverride { get; set; }
    public string? OverrideRequirement { get; set; }
}

/// <summary>
/// Contraindication check result
/// </summary>
public class ContraindicationCheckResultDto
{
    public int PatientId { get; set; }
    public string MedicationCode { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public bool HasAbsoluteContraindication { get; set; }
    public bool HasRelativeContraindication { get; set; }
    public bool HasPrecautions { get; set; }
    public List<ContraindicationAlertDto> Alerts { get; set; } = new();
    public bool CanPrescribe { get; set; }
    public string PrescribingDecision { get; set; } = string.Empty; // Safe, ProceedWithCaution, NotRecommended, Contraindicated
    public List<string>? RecommendedAlternatives { get; set; }
}

#endregion

#region Lab Value Interpretation

/// <summary>
/// Lab interpretation request
/// </summary>
public class LabInterpretationRequestDto
{
    public int PatientId { get; set; }
    public string LabCode { get; set; } = string.Empty;
    public string LabName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ResultDate { get; set; }
    public List<LabValueDto>? RelatedLabValues { get; set; }
}

/// <summary>
/// Lab value
/// </summary>
public class LabValueDto
{
    public string LabCode { get; set; } = string.Empty;
    public string LabName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ResultDate { get; set; }
}

/// <summary>
/// Lab interpretation result
/// </summary>
public class LabInterpretationResultDto
{
    public int PatientId { get; set; }
    public string LabCode { get; set; } = string.Empty;
    public string LabName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Normal, Low, High, Critical
    public string Flag { get; set; } = string.Empty; // L, H, LL, HH, N
    public LabReferenceRangeDto? ReferenceRange { get; set; }
    public string Interpretation { get; set; } = string.Empty;
    public List<string> PossibleCauses { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public List<string>? AssociatedConditions { get; set; }
    public bool IsCritical { get; set; }
    public string? CriticalAction { get; set; }
    public TrendAnalysisDto? TrendAnalysis { get; set; }
}

/// <summary>
/// Lab reference range
/// </summary>
public class LabReferenceRangeDto
{
    public decimal LowNormal { get; set; }
    public decimal HighNormal { get; set; }
    public decimal? CriticalLow { get; set; }
    public decimal? CriticalHigh { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Population { get; set; } // Male, Female, Pediatric, etc.
}

/// <summary>
/// Trend analysis for lab values
/// </summary>
public class TrendAnalysisDto
{
    public string TrendDirection { get; set; } = string.Empty; // Increasing, Decreasing, Stable, Fluctuating
    public double ChangePercent { get; set; }
    public string ChangeSignificance { get; set; } = string.Empty;
    public List<LabValueDto> HistoricalValues { get; set; } = new();
    public string TrendInterpretation { get; set; } = string.Empty;
}

#endregion

#region Clinical Guidelines

/// <summary>
/// Clinical guideline
/// </summary>
public class ClinicalGuidelineDto
{
    public int GuidelineId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ConditionCode { get; set; } = string.Empty;
    public string ConditionName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<GuidelineRecommendationDto> Recommendations { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string EvidenceLevel { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string? Url { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Guideline recommendation
/// </summary>
public class GuidelineRecommendationDto
{
    public string RecommendationType { get; set; } = string.Empty;
    public string RecommendationText { get; set; } = string.Empty;
    public string EvidenceGrade { get; set; } = string.Empty; // A, B, C, D
    public string Strength { get; set; } = string.Empty; // Strong, Moderate, Weak
    public List<string>? SupportingEvidence { get; set; }
}

#endregion

#region Order Sets & Protocols

/// <summary>
/// Clinical order set
/// </summary>
public class ClinicalOrderSetDto
{
    public int OrderSetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ConditionCode { get; set; }
    public string? ConditionName { get; set; }
    public List<OrderSetItemDto> Items { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Order set item
/// </summary>
public class OrderSetItemDto
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty; // Lab, Imaging, Medication, Referral, Instruction
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
    public bool IsRequired { get; set; }
    public bool IsPreSelected { get; set; }
    public int DisplayOrder { get; set; }
    public string? ConditionalLogic { get; set; }
}

#endregion

#region Risk Calculators

/// <summary>
/// Clinical risk calculator
/// </summary>
public class RiskCalculatorDto
{
    public string CalculatorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Cardiovascular, Diabetes, Cancer, etc.
    public List<RiskCalculatorInputDto> Inputs { get; set; } = new();
    public string Formula { get; set; } = string.Empty;
    public string EvidenceSource { get; set; } = string.Empty;
    public string Validation { get; set; } = string.Empty;
}

/// <summary>
/// Risk calculator input parameter
/// </summary>
public class RiskCalculatorInputDto
{
    public string ParameterName { get; set; } = string.Empty;
    public string ParameterCode { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // Numeric, Boolean, Categorical
    public string? Unit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public List<string>? Options { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Risk calculation request
/// </summary>
public class RiskCalculationRequestDto
{
    public int PatientId { get; set; }
    public string CalculatorId { get; set; } = string.Empty;
    public Dictionary<string, object> InputValues { get; set; } = new();
    public bool UsePatientData { get; set; } = true;
}

/// <summary>
/// Risk calculation result
/// </summary>
public class RiskCalculationResultDto
{
    public int PatientId { get; set; }
    public string CalculatorName { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Low, Moderate, High, Very High
    public string RiskPercentage { get; set; } = string.Empty;
    public string TimeFrame { get; set; } = string.Empty; // e.g., "10-year risk"
    public string Interpretation { get; set; } = string.Empty;
    public List<ClinicalRiskFactorDto> ContributingFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<RiskModificationDto> ModifiableFactors { get; set; } = new();
    public Dictionary<string, object> InputsUsed { get; set; } = new();
}

/// <summary>
/// Risk factor contribution for clinical risk calculations
/// </summary>
public class ClinicalRiskFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public object CurrentValue { get; set; } = new();
    public double ContributionPercent { get; set; }
    public bool IsModifiable { get; set; }
}

/// <summary>
/// Risk modification opportunity
/// </summary>
public class RiskModificationDto
{
    public string FactorName { get; set; } = string.Empty;
    public object CurrentValue { get; set; } = new();
    public object TargetValue { get; set; } = new();
    public double PotentialRiskReduction { get; set; }
    public string Intervention { get; set; } = string.Empty;
}

#endregion

#region Alert Management

/// <summary>
/// Clinical alert configuration
/// </summary>
public class ClinicalAlertConfigDto
{
    public int ConfigId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool RequiresAcknowledgment { get; set; }
    public bool AllowOverride { get; set; }
    public string? OverrideReasonRequired { get; set; }
    public List<string>? ExemptRoles { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
}

/// <summary>
/// Alert override record
/// </summary>
public class AlertOverrideDto
{
    public int OverrideId { get; set; }
    public int AlertId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string OverrideReason { get; set; } = string.Empty;
    public int OverriddenByUserId { get; set; }
    public string OverriddenByUserName { get; set; } = string.Empty;
    public DateTime OverriddenAt { get; set; }
    public int PatientId { get; set; }
    public string? AdditionalNotes { get; set; }
    public bool WasReviewed { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
}

#endregion

#region Additional DTOs for Service Compatibility

/// <summary>
/// Lab trend analysis DTO (alias for TrendAnalysisDto)
/// </summary>
public class LabTrendAnalysisDto
{
    public int PatientId { get; set; }
    public string LabCode { get; set; } = string.Empty;
    public int AnalysisPeriodMonths { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public List<LabTrendDataPointDto> DataPoints { get; set; } = new();
}

/// <summary>
/// Lab trend data point
/// </summary>
public class LabTrendDataPointDto
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Order set DTO (alias for ClinicalOrderSetDto)
/// </summary>
public class OrderSetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<OrderSetItemDto> Items { get; set; } = new();
}

/// <summary>
/// Create order set DTO
/// </summary>
public class CreateOrderSetDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<OrderSetItemDto> Items { get; set; } = new();
}

/// <summary>
/// Update order set DTO
/// </summary>
public class UpdateOrderSetDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<OrderSetItemDto> Items { get; set; } = new();
}

/// <summary>
/// Risk calculation history DTO
/// </summary>
public class RiskCalculationHistoryDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string CalculatorId { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public double RiskScore { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
}

/// <summary>
/// Alert configuration DTO (alias for ClinicalAlertConfigDto)
/// </summary>
public class AlertConfigurationDto
{
    public int UserId { get; set; }
    public Dictionary<string, bool> EnabledAlerts { get; set; } = new();
}

/// <summary>
/// Update alert configuration DTO
/// </summary>
public class UpdateAlertConfigurationDto
{
    public Dictionary<string, bool> EnabledAlerts { get; set; } = new();
}

/// <summary>
/// Alert override audit DTO (alias for AlertOverrideDto)
/// </summary>
public class AlertOverrideAuditDto
{
    public int OverrideId { get; set; }
    public int AlertId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string OverrideReason { get; set; } = string.Empty;
    public int OverriddenByUserId { get; set; }
    public string OverriddenByUserName { get; set; } = string.Empty;
    public DateTime OverriddenAt { get; set; }
    public int PatientId { get; set; }
}

#endregion
