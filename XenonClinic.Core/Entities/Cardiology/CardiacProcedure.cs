using XenonClinic.Core.Enums.Cardiology;

namespace XenonClinic.Core.Entities.Cardiology;

public class CardiacProcedure
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? CardioVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public CardiacProcedureType ProcedureType { get; set; }
    public string? ProcedureName { get; set; }
    public string? Indication { get; set; }
    public string? AccessSite { get; set; }
    public string? VesselsExamined { get; set; }
    public string? Findings { get; set; }
    public string? Intervention { get; set; }
    public int? NumberOfStents { get; set; }
    public string? StentDetails { get; set; }
    public bool? Complications { get; set; }
    public string? ComplicationDetails { get; set; }
    public decimal? ContrastUsed { get; set; }
    public decimal? FluoroscopyTime { get; set; }
    public decimal? RadiationDose { get; set; }
    public string? PostProcedureCare { get; set; }
    public string? Recommendations { get; set; }
    public string? PerformingPhysician { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public CardioVisit? CardioVisit { get; set; }
}
