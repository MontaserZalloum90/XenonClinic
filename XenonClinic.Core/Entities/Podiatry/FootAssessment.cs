using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class FootAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PodiatryVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public FootType? FootTypeRight { get; set; }
    public FootType? FootTypeLeft { get; set; }
    public string? ArchHeightRight { get; set; }
    public string? ArchHeightLeft { get; set; }
    public string? HindFootAlignmentRight { get; set; }
    public string? HindFootAlignmentLeft { get; set; }
    public string? ForefootAlignmentRight { get; set; }
    public string? ForefootAlignmentLeft { get; set; }
    public decimal? FirstMTPROMRight { get; set; }
    public decimal? FirstMTPROMLeft { get; set; }
    public decimal? AnkleDorsiflexionRight { get; set; }
    public decimal? AnkleDorsiflexionLeft { get; set; }
    public decimal? AnklePlantarflexionRight { get; set; }
    public decimal? AnklePlantarflexionLeft { get; set; }
    public decimal? SubtalarInversionRight { get; set; }
    public decimal? SubtalarInversionLeft { get; set; }
    public decimal? SubtalarEversionRight { get; set; }
    public decimal? SubtalarEversionLeft { get; set; }
    public string? MuscleStrengthRight { get; set; }
    public string? MuscleStrengthLeft { get; set; }
    public string? JointStabilityRight { get; set; }
    public string? JointStabilityLeft { get; set; }
    public string? SkinConditionRight { get; set; }
    public string? SkinConditionLeft { get; set; }
    public string? CallusPattern { get; set; }
    public string? DeformitiesRight { get; set; }
    public string? DeformitiesLeft { get; set; }
    public string? OverallFindings { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PodiatryVisit? PodiatryVisit { get; set; }
}
