using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class PodiatryProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PodiatryVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public PodiatryProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? CPTCode { get; set; }
    public FootSide Side { get; set; }
    public string? Location { get; set; }
    public string? Indication { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? Findings { get; set; }
    public string? ImplantsUsed { get; set; }
    public bool? SpecimenSent { get; set; }
    public string? PathologyResults { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public string? PostOpInstructions { get; set; }
    public string? WeightBearingStatus { get; set; }
    public string? DressingInstructions { get; set; }
    public string? FollowUpSchedule { get; set; }
    public string? PerformingPodiatrist { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PodiatryVisit? PodiatryVisit { get; set; }
}
