using XenonClinic.Core.Enums.PainManagement;

namespace XenonClinic.Core.Entities.PainManagement;

public class PainProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PainVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public PainProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public string? Indication { get; set; }
    public string? TargetStructure { get; set; }
    public string? ApproachLevel { get; set; }
    public string? GuidanceType { get; set; }
    public string? MedicationsUsed { get; set; }
    public string? SteroidUsed { get; set; }
    public decimal? SteroidDose { get; set; }
    public string? LocalAnestheticUsed { get; set; }
    public decimal? LocalAnestheticVolume { get; set; }
    public int? PreProcedurePainLevel { get; set; }
    public int? PostProcedurePainLevel { get; set; }
    public int? PercentRelief { get; set; }
    public string? ReliefDuration { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? PostProcedureInstructions { get; set; }
    public string? PerformingPhysician { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public PainVisit? PainVisit { get; set; }
}
