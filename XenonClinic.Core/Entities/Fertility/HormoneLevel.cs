using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class HormoneLevel
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? IVFCycleId { get; set; }
    public DateTime TestDate { get; set; }
    public int? CycleDay { get; set; }
    public int? StimulationDay { get; set; }
    public decimal? FSH { get; set; }
    public decimal? LH { get; set; }
    public decimal? Estradiol { get; set; }
    public decimal? Progesterone { get; set; }
    public decimal? AMH { get; set; }
    public decimal? Prolactin { get; set; }
    public decimal? TSH { get; set; }
    public decimal? FreeT4 { get; set; }
    public decimal? FreeT3 { get; set; }
    public decimal? Testosterone { get; set; }
    public decimal? DHEAS { get; set; }
    public decimal? Androstenedione { get; set; }
    public decimal? SHBG { get; set; }
    public decimal? Insulin { get; set; }
    public decimal? FastingGlucose { get; set; }
    public decimal? BetaHCG { get; set; }
    public string? EndometrialThickness { get; set; }
    public int? FollicleCountRight { get; set; }
    public int? FollicleCountLeft { get; set; }
    public string? FollicleSizesRight { get; set; }
    public string? FollicleSizesLeft { get; set; }
    public string? LeadFollicleSize { get; set; }
    public string? Interpretation { get; set; }
    public string? ActionPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public IVFCycle? IVFCycle { get; set; }
}
