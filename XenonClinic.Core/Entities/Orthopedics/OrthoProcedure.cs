using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class OrthoProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? OrthoVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public OrthoProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public BodyRegion BodyRegion { get; set; }
    public string? SpecificLocation { get; set; }
    public Laterality Laterality { get; set; }
    public string? Indication { get; set; }
    public string? Technique { get; set; }
    public string? ImplantsUsed { get; set; }
    public string? GraftType { get; set; }
    public string? Findings { get; set; }
    public string? AnesthesiaType { get; set; }
    public decimal? OperativeTime { get; set; }
    public decimal? EstimatedBloodLoss { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? PostOpInstructions { get; set; }
    public string? WeightBearingStatus { get; set; }
    public bool? PhysicalTherapyRequired { get; set; }
    public string? PerformingSurgeon { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public OrthoVisit? OrthoVisit { get; set; }
}
