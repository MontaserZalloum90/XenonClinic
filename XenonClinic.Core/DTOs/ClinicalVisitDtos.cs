namespace XenonClinic.Core.DTOs;

/// <summary>
/// Base DTO for all clinical visit types. Provides common properties shared across specialties.
/// </summary>
public abstract class ClinicalVisitBaseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    // SECURITY FIX: Removed PatientEmiratesId - PII should not be exposed in general DTOs
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base DTO for creating clinical visits.
/// </summary>
public abstract class CreateClinicalVisitBaseDto
{
    public int PatientId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Base DTO for updating clinical visits.
/// </summary>
public abstract class UpdateClinicalVisitBaseDto
{
    public int Id { get; set; }
    public int? ProviderId { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
}

#region Audiology DTOs

/// <summary>
/// DTO for audiology visit data.
/// </summary>
public class AudiologyVisitDto : ClinicalVisitBaseDto
{
    public string? HearingLossType { get; set; }
    public string? AudiogramResults { get; set; }
    public string? Recommendations { get; set; }
    public bool? HearingAidRecommended { get; set; }
    public string? HearingAidType { get; set; }
    public string? FollowUpPlan { get; set; }
}

/// <summary>
/// DTO for creating an audiology visit.
/// </summary>
public class CreateAudiologyVisitDto : CreateClinicalVisitBaseDto
{
    public string? HearingLossType { get; set; }
    public string? AudiogramResults { get; set; }
    public string? Recommendations { get; set; }
    public bool? HearingAidRecommended { get; set; }
    public string? HearingAidType { get; set; }
}

/// <summary>
/// DTO for updating an audiology visit.
/// </summary>
public class UpdateAudiologyVisitDto : UpdateClinicalVisitBaseDto
{
    public string? HearingLossType { get; set; }
    public string? AudiogramResults { get; set; }
    public string? Recommendations { get; set; }
    public bool? HearingAidRecommended { get; set; }
    public string? HearingAidType { get; set; }
}

/// <summary>
/// DTO for audiogram data.
/// </summary>
public class AudiogramDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime TestDate { get; set; }
    public string? TestType { get; set; }

    // Right ear thresholds (dB)
    public int? Right250Hz { get; set; }
    public int? Right500Hz { get; set; }
    public int? Right1000Hz { get; set; }
    public int? Right2000Hz { get; set; }
    public int? Right4000Hz { get; set; }
    public int? Right8000Hz { get; set; }

    // Left ear thresholds (dB)
    public int? Left250Hz { get; set; }
    public int? Left500Hz { get; set; }
    public int? Left1000Hz { get; set; }
    public int? Left2000Hz { get; set; }
    public int? Left4000Hz { get; set; }
    public int? Left8000Hz { get; set; }

    // Speech recognition
    public int? RightSpeechRecognition { get; set; }
    public int? LeftSpeechRecognition { get; set; }

    public string? Interpretation { get; set; }
    public string? TestedBy { get; set; }
}

#endregion

#region Dental DTOs

/// <summary>
/// DTO for dental visit data.
/// </summary>
public class DentalVisitDto : ClinicalVisitBaseDto
{
    public string? ExaminationFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentProvided { get; set; }
    public string? ProceduresConducted { get; set; }
    public string? Prescriptions { get; set; }
    public DateTime? NextAppointmentRecommended { get; set; }
    public decimal? TotalCost { get; set; }
}

/// <summary>
/// DTO for creating a dental visit.
/// </summary>
public class CreateDentalVisitDto : CreateClinicalVisitBaseDto
{
    public string? ExaminationFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentProvided { get; set; }
    public List<int>? ProcedureIds { get; set; }
}

/// <summary>
/// DTO for dental procedure data.
/// </summary>
public class DentalProcedureDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ToothNumber { get; set; }
    public string? Surface { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO for tooth chart data.
/// </summary>
public class ToothChartDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<ToothRecordDto> Teeth { get; set; } = new();
}

/// <summary>
/// DTO for individual tooth record.
/// </summary>
public class ToothRecordDto
{
    public int Id { get; set; }
    public string ToothNumber { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
    public List<string>? Treatments { get; set; }
}

#endregion

#region Cardiology DTOs

/// <summary>
/// DTO for cardiology visit data.
/// </summary>
public class CardioVisitDto : ClinicalVisitBaseDto
{
    public string? HeartSoundFindings { get; set; }
    public string? RhythmAssessment { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public string? ECGFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public List<string>? Medications { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

/// <summary>
/// DTO for creating a cardiology visit.
/// </summary>
public class CreateCardioVisitDto : CreateClinicalVisitBaseDto
{
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public string? HeartSoundFindings { get; set; }
    public string? RhythmAssessment { get; set; }
    public string? ECGFindings { get; set; }
    public string? Diagnosis { get; set; }
}

/// <summary>
/// DTO for ECG record data.
/// </summary>
public class ECGRecordDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime RecordDate { get; set; }
    public string? Rhythm { get; set; }
    public int? HeartRate { get; set; }
    public string? PRInterval { get; set; }
    public string? QRSDuration { get; set; }
    public string? QTInterval { get; set; }
    public string? STSegment { get; set; }
    public string? Interpretation { get; set; }
    public string? Abnormalities { get; set; }
    public string? PerformedBy { get; set; }
    public string? FilePath { get; set; }
}

/// <summary>
/// DTO for echocardiogram results.
/// </summary>
public class EchoResultDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime StudyDate { get; set; }
    public decimal? EjectionFraction { get; set; }
    public string? LeftVentricle { get; set; }
    public string? RightVentricle { get; set; }
    public string? LeftAtrium { get; set; }
    public string? RightAtrium { get; set; }
    public string? ValveFindings { get; set; }
    public string? WallMotion { get; set; }
    public string? Conclusion { get; set; }
    public string? PerformedBy { get; set; }
}

#endregion

#region Ophthalmology DTOs

/// <summary>
/// DTO for ophthalmology visit data.
/// </summary>
public class OphthalmologyVisitDto : ClinicalVisitBaseDto
{
    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public string? IntraocularPressureRight { get; set; }
    public string? IntraocularPressureLeft { get; set; }
    public string? AnteriorSegmentFindings { get; set; }
    public string? PosteriorSegmentFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public bool? GlassesRecommended { get; set; }
}

/// <summary>
/// DTO for creating an ophthalmology visit.
/// </summary>
public class CreateOphthalmologyVisitDto : CreateClinicalVisitBaseDto
{
    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public string? IntraocularPressureRight { get; set; }
    public string? IntraocularPressureLeft { get; set; }
    public string? Diagnosis { get; set; }
}

/// <summary>
/// DTO for eye prescription data.
/// </summary>
public class EyePrescriptionDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime PrescriptionDate { get; set; }

    // Right eye
    public decimal? RightSphere { get; set; }
    public decimal? RightCylinder { get; set; }
    public int? RightAxis { get; set; }
    public decimal? RightAdd { get; set; }

    // Left eye
    public decimal? LeftSphere { get; set; }
    public decimal? LeftCylinder { get; set; }
    public int? LeftAxis { get; set; }
    public decimal? LeftAdd { get; set; }

    public string? PupillaryDistance { get; set; }
    public string? LensType { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

#endregion

#region Physiotherapy DTOs

/// <summary>
/// DTO for physiotherapy session data.
/// </summary>
public class PhysioSessionDto : ClinicalVisitBaseDto
{
    public int? AssessmentId { get; set; }
    public string? TreatmentType { get; set; }
    public string? TechniquesUsed { get; set; }
    public int? DurationMinutes { get; set; }
    public int? PainLevelBefore { get; set; }
    public int? PainLevelAfter { get; set; }
    public string? PatientResponse { get; set; }
    public string? HomeExercises { get; set; }
    public int SessionNumber { get; set; }
    public int TotalPlannedSessions { get; set; }
}

/// <summary>
/// DTO for creating a physiotherapy session.
/// </summary>
public class CreatePhysioSessionDto : CreateClinicalVisitBaseDto
{
    public int? AssessmentId { get; set; }
    public string? TreatmentType { get; set; }
    public string? TechniquesUsed { get; set; }
    public int? DurationMinutes { get; set; }
    public int? PainLevelBefore { get; set; }
}

/// <summary>
/// DTO for physiotherapy assessment.
/// </summary>
public class PhysioAssessmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string? Diagnosis { get; set; }
    public string? PresentingComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? PastMedicalHistory { get; set; }
    public string? ObjectiveFindings { get; set; }
    public string? RangeOfMotion { get; set; }
    public string? StrengthAssessment { get; set; }
    public string? FunctionalLimitations { get; set; }
    public string? TreatmentGoals { get; set; }
    public string? TreatmentPlan { get; set; }
    public int? PlannedSessions { get; set; }
    public string? AssessedBy { get; set; }
}

#endregion

#region Clinical Visit List DTOs

/// <summary>
/// Request DTO for clinical visit list with filtering.
/// </summary>
public class ClinicalVisitListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "VisitDate";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Summary DTO for clinical visit list (lightweight).
/// </summary>
public class ClinicalVisitSummaryDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public string? ProviderName { get; set; }
    public string? ChiefComplaint { get; set; }
    public string SpecialtyType { get; set; } = string.Empty;
}

/// <summary>
/// Statistics DTO for clinical visits.
/// </summary>
public class ClinicalVisitStatisticsDto
{
    public int TotalVisits { get; set; }
    public int VisitsToday { get; set; }
    public int VisitsThisWeek { get; set; }
    public int VisitsThisMonth { get; set; }
    public Dictionary<string, int> VisitsBySpecialty { get; set; } = new();
    public Dictionary<string, int> VisitsByProvider { get; set; } = new();
}

#endregion

/// <summary>
/// Validation messages for clinical visits.
/// </summary>
public static class ClinicalVisitValidationMessages
{
    public const string PatientIdRequired = "Patient ID is required";
    public const string PatientNotFound = "Patient not found";
    public const string VisitDateRequired = "Visit date is required";
    public const string VisitDateInFuture = "Visit date cannot be in the future";
    public const string VisitNotFound = "Visit not found";
    public const string ProviderNotFound = "Provider not found";
    public const string BranchAccessDenied = "You do not have access to this branch";
    public const string ChiefComplaintRequired = "Chief complaint is required";
    public const string InvalidSpecialty = "Invalid specialty type";
}
