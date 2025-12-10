using XenonClinic.Core.Enums.Gastroenterology;

namespace XenonClinic.Core.Entities.Gastroenterology;

public class GastroProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? GastroVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public GastroProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public string? Indication { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? Findings { get; set; }
    public string? Intervention { get; set; }
    public bool? SpecimenSent { get; set; }
    public string? PathologyResults { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? PostProcedureInstructions { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? FollowUpPlan { get; set; }
    public string? PerformingPhysician { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public GastroVisit? GastroVisit { get; set; }
}
