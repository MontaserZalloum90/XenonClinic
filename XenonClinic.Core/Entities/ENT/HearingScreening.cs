using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class HearingScreening
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ENTVisitId { get; set; }
    public DateTime ScreeningDate { get; set; }
    public HearingScreeningType ScreeningType { get; set; }
    public int? RightEar250Hz { get; set; }
    public int? RightEar500Hz { get; set; }
    public int? RightEar1000Hz { get; set; }
    public int? RightEar2000Hz { get; set; }
    public int? RightEar4000Hz { get; set; }
    public int? RightEar8000Hz { get; set; }
    public int? LeftEar250Hz { get; set; }
    public int? LeftEar500Hz { get; set; }
    public int? LeftEar1000Hz { get; set; }
    public int? LeftEar2000Hz { get; set; }
    public int? LeftEar4000Hz { get; set; }
    public int? LeftEar8000Hz { get; set; }
    public HearingLossType? RightEarLossType { get; set; }
    public HearingLossType? LeftEarLossType { get; set; }
    public HearingLossSeverity? RightEarSeverity { get; set; }
    public HearingLossSeverity? LeftEarSeverity { get; set; }
    public string? TympanometryRight { get; set; }
    public string? TympanometryLeft { get; set; }
    public string? SpeechDiscriminationRight { get; set; }
    public string? SpeechDiscriminationLeft { get; set; }
    public string? OtoacousticEmissions { get; set; }
    public string? ABRResults { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ENTVisit? ENTVisit { get; set; }
}
