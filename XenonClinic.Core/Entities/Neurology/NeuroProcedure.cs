using XenonClinic.Core.Enums.Neurology;

namespace XenonClinic.Core.Entities.Neurology;

public class NeuroProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? NeuroVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public NeuroProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public string? Indication { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? AccessSite { get; set; }
    public string? Findings { get; set; }
    public string? CSFAnalysis { get; set; }
    public decimal? OpeningPressure { get; set; }
    public decimal? ClosingPressure { get; set; }
    public string? MedicationsAdministered { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? PostProcedureInstructions { get; set; }
    public string? PerformingPhysician { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public NeuroVisit? NeuroVisit { get; set; }
}
