using XenonClinic.Core.Enums.Chiropractic;

namespace XenonClinic.Core.Entities.Chiropractic;

public class ChiroAdjustment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ChiroVisitId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string? SegmentAdjusted { get; set; }
    public SpinalRegion SpinalRegion { get; set; }
    public AdjustmentTechnique Technique { get; set; }
    public string? TechniqueDetails { get; set; }
    public string? ListingCorrected { get; set; }
    public string? DirectionOfForce { get; set; }
    public string? PatientPosition { get; set; }
    public bool? CavitationAchieved { get; set; }
    public string? PatientResponse { get; set; }
    public bool? AdverseReaction { get; set; }
    public string? AdverseReactionDetails { get; set; }
    public int? PreAdjustmentPain { get; set; }
    public int? PostAdjustmentPain { get; set; }
    public string? AdjunctTherapies { get; set; }
    public string? PerformingChiropractor { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public ChiroVisit? ChiroVisit { get; set; }
}
