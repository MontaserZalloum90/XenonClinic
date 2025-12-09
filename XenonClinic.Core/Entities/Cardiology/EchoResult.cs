namespace XenonClinic.Core.Entities.Cardiology;

public class EchoResult
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? CardioVisitId { get; set; }
    public DateTime StudyDate { get; set; }
    public string? StudyType { get; set; }
    public decimal? LeftVentricleEF { get; set; }
    public decimal? LeftVentricleEDD { get; set; }
    public decimal? LeftVentricleESD { get; set; }
    public decimal? InterventricularSeptum { get; set; }
    public decimal? PosteriorWall { get; set; }
    public decimal? LeftAtriumSize { get; set; }
    public decimal? RightVentricleSize { get; set; }
    public decimal? RightAtriumSize { get; set; }
    public decimal? AorticRootSize { get; set; }
    public string? AorticValve { get; set; }
    public string? MitralValve { get; set; }
    public string? TricuspidValve { get; set; }
    public string? PulmonicValve { get; set; }
    public decimal? TRVelocity { get; set; }
    public decimal? EstimatedRVSP { get; set; }
    public bool? PericardialEffusion { get; set; }
    public string? PericardialEffusionSize { get; set; }
    public bool? WallMotionAbnormality { get; set; }
    public string? WallMotionDetails { get; set; }
    public bool? Diastolic​Dysfunction { get; set; }
    public string? DiastolicGrade { get; set; }
    public string? Conclusion { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public CardioVisit? CardioVisit { get; set; }
}
