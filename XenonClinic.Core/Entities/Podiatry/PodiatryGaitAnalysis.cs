using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class PodiatryGaitAnalysis
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PodiatryVisitId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public GaitAnalysisType AnalysisType { get; set; }
    public string? WalkingSpeed { get; set; }
    public string? StepLength { get; set; }
    public string? StancePhaseRight { get; set; }
    public string? StancePhaseLeft { get; set; }
    public string? SwingPhaseRight { get; set; }
    public string? SwingPhaseLeft { get; set; }
    public string? HeelStrikeRight { get; set; }
    public string? HeelStrikeLeft { get; set; }
    public string? MidStanceRight { get; set; }
    public string? MidStanceLeft { get; set; }
    public string? PropulsionRight { get; set; }
    public string? PropulsionLeft { get; set; }
    public string? ToeOffRight { get; set; }
    public string? ToeOffLeft { get; set; }
    public string? PronationPatternRight { get; set; }
    public string? PronationPatternLeft { get; set; }
    public string? SupinationPatternRight { get; set; }
    public string? SupinationPatternLeft { get; set; }
    public string? FootProgressionAngleRight { get; set; }
    public string? FootProgressionAngleLeft { get; set; }
    public string? KneeAlignmentRight { get; set; }
    public string? KneeAlignmentLeft { get; set; }
    public string? HipMovement { get; set; }
    public string? PelvicMovement { get; set; }
    public string? ArmSwing { get; set; }
    public string? Abnormalities { get; set; }
    public string? VideoFilePath { get; set; }
    public string? PressureMappingFilePath { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public PodiatryVisit? PodiatryVisit { get; set; }
}
