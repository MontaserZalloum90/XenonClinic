namespace XenonClinic.Core.Entities.Chiropractic;

public class PostureAnalysis
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ChiroVisitId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public string? AnteriorView { get; set; }
    public string? PosteriorView { get; set; }
    public string? LeftLateralView { get; set; }
    public string? RightLateralView { get; set; }
    public decimal? HeadTilt { get; set; }
    public string? HeadTiltDirection { get; set; }
    public decimal? ShoulderLevelDifference { get; set; }
    public string? ShoulderHighSide { get; set; }
    public decimal? HipLevelDifference { get; set; }
    public string? HipHighSide { get; set; }
    public bool? ForwardHeadPosture { get; set; }
    public decimal? ForwardHeadDistance { get; set; }
    public bool? RoundedShoulders { get; set; }
    public bool? ThoracicKyphosis { get; set; }
    public bool? LumbarLordosis { get; set; }
    public bool? SwaybackPosture { get; set; }
    public bool? FlatbackPosture { get; set; }
    public bool? Scoliosis { get; set; }
    public string? ScoliosisCurvePattern { get; set; }
    public decimal? CobbAngle { get; set; }
    public string? PlumbLineDeviation { get; set; }
    public string? OverallPostureGrade { get; set; }
    public string? ImageFilePath { get; set; }
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ChiroVisit? ChiroVisit { get; set; }
}
