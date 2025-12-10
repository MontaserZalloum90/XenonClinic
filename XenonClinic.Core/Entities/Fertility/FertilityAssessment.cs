using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class FertilityAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PartnerId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public InfertilityType InfertilityType { get; set; }
    public string? InfertilityDiagnosis { get; set; }
    public string? FemaleFactor { get; set; }
    public string? MaleFactor { get; set; }
    public bool? UnexplainedInfertility { get; set; }
    public string? OvarianReserveStatus { get; set; }
    public decimal? AMH { get; set; }
    public int? AFCRight { get; set; }
    public int? AFCLeft { get; set; }
    public decimal? FSH { get; set; }
    public decimal? LH { get; set; }
    public decimal? Estradiol { get; set; }
    public string? HysterosalpingogramResults { get; set; }
    public bool? TubesPatent { get; set; }
    public string? UterineCavityAssessment { get; set; }
    public string? EndometriosisStage { get; set; }
    public string? PCOSStatus { get; set; }
    public string? ThyroidStatus { get; set; }
    public string? ProlactinLevel { get; set; }
    public string? SpermAnalysisSummary { get; set; }
    public string? GeneticTestingResults { get; set; }
    public string? RecommendedTreatment { get; set; }
    public string? Prognosis { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Patient? Partner { get; set; }
}
