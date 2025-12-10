using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class SpermAnalysis
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? IVFCycleId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public int? AbstinenceDays { get; set; }
    public string? CollectionMethod { get; set; }
    public string? CollectionTime { get; set; }
    public string? AnalysisTime { get; set; }
    public decimal? Volume { get; set; }
    public string? Liquefaction { get; set; }
    public string? Viscosity { get; set; }
    public decimal? pH { get; set; }
    public string? Appearance { get; set; }
    public decimal? Concentration { get; set; }
    public decimal? TotalSpermCount { get; set; }
    public decimal? ProgressiveMotility { get; set; }
    public decimal? NonProgressiveMotility { get; set; }
    public decimal? Immotile { get; set; }
    public decimal? TotalMotility { get; set; }
    public decimal? NormalMorphology { get; set; }
    public string? HeadDefects { get; set; }
    public string? NeckDefects { get; set; }
    public string? TailDefects { get; set; }
    public decimal? Vitality { get; set; }
    public decimal? WBCCount { get; set; }
    public string? Agglutination { get; set; }
    public string? AntispermAntibodies { get; set; }
    public SpermQuality OverallAssessment { get; set; }
    public string? WHOClassification { get; set; }
    public decimal? DNAFragmentationIndex { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? LabTechnician { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public IVFCycle? IVFCycle { get; set; }
}
