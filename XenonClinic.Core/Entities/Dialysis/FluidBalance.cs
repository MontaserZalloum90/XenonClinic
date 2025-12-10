namespace XenonClinic.Core.Entities.Dialysis;

public class FluidBalance
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? DialysisSessionId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? PreDialysisWeight { get; set; }
    public decimal? PostDialysisWeight { get; set; }
    public decimal? InterDialyticWeightGain { get; set; }
    public decimal? DryWeight { get; set; }
    public decimal? FluidRemoved { get; set; }
    public decimal? FluidIntakeOral { get; set; }
    public decimal? FluidIntakeIV { get; set; }
    public decimal? UrineOutput { get; set; }
    public bool? EdemaPresent { get; set; }
    public string? EdemaLocation { get; set; }
    public int? EdemaGrade { get; set; }
    public bool? ShortnessOfBreath { get; set; }
    public bool? Orthopnea { get; set; }
    public int? JVPLevel { get; set; }
    public string? LungSounds { get; set; }
    public string? BloodPressureTrend { get; set; }
    public string? FluidRestrictionCompliance { get; set; }
    public string? SodiumRestrictionCompliance { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public DialysisSession? DialysisSession { get; set; }
}
