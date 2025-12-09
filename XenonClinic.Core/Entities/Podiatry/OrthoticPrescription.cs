using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class OrthoticPrescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PodiatryVisitId { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public OrthoticType OrthoticType { get; set; }
    public string? OrthoticName { get; set; }
    public FootSide Side { get; set; }
    public string? Indication { get; set; }
    public string? ShellMaterial { get; set; }
    public string? TopCoverMaterial { get; set; }
    public string? ArchHeightSpecification { get; set; }
    public string? PostingSpecification { get; set; }
    public string? Accommodations { get; set; }
    public string? Modifications { get; set; }
    public string? MoldingType { get; set; }
    public DateTime? MoldDate { get; set; }
    public string? Lab { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? FittingDate { get; set; }
    public string? FittingNotes { get; set; }
    public string? BreakInInstructions { get; set; }
    public string? FootwearRecommendations { get; set; }
    public OrthoticStatus Status { get; set; }
    public string? AdjustmentsNeeded { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public PodiatryVisit? PodiatryVisit { get; set; }
}
