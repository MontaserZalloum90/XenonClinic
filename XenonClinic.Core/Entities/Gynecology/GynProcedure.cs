using XenonClinic.Core.Enums.Gynecology;

namespace XenonClinic.Core.Entities.Gynecology;

public class GynProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? GynVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public GynProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public string? Indication { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? Findings { get; set; }
    public string? SpecimenSent { get; set; }
    public string? PathologyResults { get; set; }
    public decimal? EstimatedBloodLoss { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? DeviceInserted { get; set; }
    public DateTime? DeviceExpirationDate { get; set; }
    public string? PostOpInstructions { get; set; }
    public string? FollowUpPlan { get; set; }
    public string? PerformingPhysician { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public GynVisit? GynVisit { get; set; }
}
