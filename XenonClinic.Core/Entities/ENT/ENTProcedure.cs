using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class ENTProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ENTVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public ENTProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public string? Indication { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? Findings { get; set; }
    public string? ImplantsUsed { get; set; }
    public decimal? OperativeTime { get; set; }
    public decimal? EstimatedBloodLoss { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? SpecimenSent { get; set; }
    public string? PathologyResults { get; set; }
    public string? PostOpInstructions { get; set; }
    public string? VoiceRest { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? PerformingSurgeon { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ENTVisit? ENTVisit { get; set; }
}
