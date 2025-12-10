using XenonClinic.Core.Enums.Psychiatry;

namespace XenonClinic.Core.Entities.Psychiatry;

public class PsychAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? MentalHealthVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public AssessmentType AssessmentType { get; set; }
    public string? AssessmentName { get; set; }
    public string? ReferralReason { get; set; }
    public string? BackgroundHistory { get; set; }
    public string? DevelopmentalHistory { get; set; }
    public string? FamilyHistory { get; set; }
    public string? EducationalHistory { get; set; }
    public string? OccupationalHistory { get; set; }
    public string? SocialHistory { get; set; }
    public string? MedicalHistory { get; set; }
    public string? PsychiatricHistory { get; set; }
    public string? SubstanceUseHistory { get; set; }
    public string? TestsAdministered { get; set; }
    public string? CognitiveResults { get; set; }
    public int? IQScore { get; set; }
    public string? PersonalityResults { get; set; }
    public string? NeuropsychResults { get; set; }
    public string? ProjectiveResults { get; set; }
    public string? BehavioralObservations { get; set; }
    public string? ClinicalImpression { get; set; }
    public string? DiagnosticFormulation { get; set; }
    public string? DSM5Diagnoses { get; set; }
    public string? DifferentialDiagnoses { get; set; }
    public string? Recommendations { get; set; }
    public string? TreatmentRecommendations { get; set; }
    public string? Prognosis { get; set; }
    public string? Examiner { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public MentalHealthVisit? MentalHealthVisit { get; set; }
}
