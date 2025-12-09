namespace XenonClinic.Core.Entities.Chiropractic;

public class ChiroXRayFinding
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ChiroVisitId { get; set; }
    public DateTime XRayDate { get; set; }
    public string? ViewsTaken { get; set; }
    public string? CervicalFindings { get; set; }
    public decimal? CervicalLordosis { get; set; }
    public string? AtlasAlignment { get; set; }
    public string? AxisAlignment { get; set; }
    public string? ThoracicFindings { get; set; }
    public decimal? ThoracicKyphosis { get; set; }
    public string? LumbarFindings { get; set; }
    public decimal? LumbarLordosis { get; set; }
    public string? SacralFindings { get; set; }
    public decimal? SacralBaseAngle { get; set; }
    public string? DegenerativeChanges { get; set; }
    public string? DiscSpaceNarrowing { get; set; }
    public string? Osteophytes { get; set; }
    public string? Subluxations { get; set; }
    public string? Scoliosis { get; set; }
    public string? ScoliosisMeasurements { get; set; }
    public bool? FracturesPresent { get; set; }
    public string? FractureDetails { get; set; }
    public bool? Anomalies { get; set; }
    public string? AnomalyDetails { get; set; }
    public string? SoftTissueFindings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? InterpretingDoctor { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public ChiroVisit? ChiroVisit { get; set; }
}
