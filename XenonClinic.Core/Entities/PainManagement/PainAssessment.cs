using XenonClinic.Core.Enums.PainManagement;

namespace XenonClinic.Core.Entities.PainManagement;

public class PainAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PainVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public PainType PainType { get; set; }
    public ChronicityType Chronicity { get; set; }
    public string? PainDiagnosis { get; set; }
    public string? ICD10Code { get; set; }
    public string? PrimaryPainGenerator { get; set; }
    public string? SecondaryFactors { get; set; }
    public int? NRSScore { get; set; }
    public int? VASScore { get; set; }
    public decimal? OswestryDisabilityIndex { get; set; }
    public decimal? NeckDisabilityIndex { get; set; }
    public int? BriefPainInventory { get; set; }
    public int? PHQ9Score { get; set; }
    public int? GAD7Score { get; set; }
    public string? FunctionalCapacity { get; set; }
    public string? ImagingFindings { get; set; }
    public string? EMGFindings { get; set; }
    public string? DiagnosticBlockResults { get; set; }
    public string? OpioidRiskScore { get; set; }
    public string? TreatmentGoals { get; set; }
    public string? RecommendedTreatment { get; set; }
    public string? Prognosis { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PainVisit? PainVisit { get; set; }
}
