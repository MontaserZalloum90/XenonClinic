using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class CastRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? OrthoVisitId { get; set; }
    public DateTime ApplicationDate { get; set; }
    public CastType CastType { get; set; }
    public string? CastMaterial { get; set; }
    public BodyRegion BodyRegion { get; set; }
    public string? SpecificLocation { get; set; }
    public Laterality Laterality { get; set; }
    public string? Indication { get; set; }
    public string? Position { get; set; }
    public int? ExpectedDurationWeeks { get; set; }
    public DateTime? ScheduledRemovalDate { get; set; }
    public DateTime? ActualRemovalDate { get; set; }
    public string? RemovalReason { get; set; }
    public bool? SkinIntact { get; set; }
    public string? SkinCondition { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? CareInstructions { get; set; }
    public string? AppliedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public OrthoVisit? OrthoVisit { get; set; }
}
