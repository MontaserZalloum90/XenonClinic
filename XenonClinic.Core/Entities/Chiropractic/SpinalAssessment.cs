using XenonClinic.Core.Enums.Chiropractic;

namespace XenonClinic.Core.Entities.Chiropractic;

public class SpinalAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ChiroVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string? CervicalFindings { get; set; }
    public string? ThoracicFindings { get; set; }
    public string? LumbarFindings { get; set; }
    public string? SacralFindings { get; set; }
    public string? PelvicFindings { get; set; }
    public bool? C1Subluxation { get; set; }
    public bool? C2Subluxation { get; set; }
    public bool? CervicalRestriction { get; set; }
    public string? CervicalListingPattern { get; set; }
    public bool? ThoracicRestriction { get; set; }
    public string? ThoracicListingPattern { get; set; }
    public bool? LumbarRestriction { get; set; }
    public string? LumbarListingPattern { get; set; }
    public bool? SacroiliacDysfunction { get; set; }
    public string? SIDysfunctionSide { get; set; }
    public decimal? LegLengthDiscrepancy { get; set; }
    public string? LegLengthShortSide { get; set; }
    public string? SpinalCurvatures { get; set; }
    public string? MuscleSpasm { get; set; }
    public string? TriggerPointsFound { get; set; }
    public string? JointHypermobility { get; set; }
    public string? JointHypomobility { get; set; }
    public string? OverallFindings { get; set; }
    public string? TreatmentRecommendations { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ChiroVisit? ChiroVisit { get; set; }
}
