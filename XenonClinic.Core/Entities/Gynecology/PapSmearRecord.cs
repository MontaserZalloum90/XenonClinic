using XenonClinic.Core.Enums.Gynecology;

namespace XenonClinic.Core.Entities.Gynecology;

public class PapSmearRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? GynVisitId { get; set; }
    public DateTime CollectionDate { get; set; }
    public string? SpecimenAdequacy { get; set; }
    public PapSmearResult Result { get; set; }
    public string? BethesdaCategory { get; set; }
    public bool? HPVTesting { get; set; }
    public bool? HPVPositive { get; set; }
    public string? HPVGenotypes { get; set; }
    public string? CytologyFindings { get; set; }
    public string? EndocervicalCells { get; set; }
    public string? TransformationZone { get; set; }
    public string? Organisms { get; set; }
    public string? OtherFindings { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? FollowUpPlan { get; set; }
    public bool? ColposcopyRecommended { get; set; }
    public string? PathologyLab { get; set; }
    public string? PathologistName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public GynVisit? GynVisit { get; set; }
}
