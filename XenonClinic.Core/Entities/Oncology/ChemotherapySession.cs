using XenonClinic.Core.Enums.Oncology;

namespace XenonClinic.Core.Entities.Oncology;

public class ChemotherapySession
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? OncologyVisitId { get; set; }
    public int? TreatmentPlanId { get; set; }
    public DateTime SessionDate { get; set; }
    public int CycleNumber { get; set; }
    public int DayInCycle { get; set; }
    public string? RegimenName { get; set; }
    public string? DrugsAdministeredJson { get; set; }
    public string? PremedicationsJson { get; set; }
    public decimal? BSA { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? PreTreatmentLabs { get; set; }
    public decimal? ANC { get; set; }
    public decimal? Platelets { get; set; }
    public decimal? Hemoglobin { get; set; }
    public decimal? Creatinine { get; set; }
    public decimal? Bilirubin { get; set; }
    public bool? DoseReductionRequired { get; set; }
    public int? DoseReductionPercent { get; set; }
    public string? DoseReductionReason { get; set; }
    public bool? TreatmentDelayed { get; set; }
    public string? DelayReason { get; set; }
    public string? InfusionReactions { get; set; }
    public string? ImmediateComplications { get; set; }
    public string? AntiemeticProtocol { get; set; }
    public string? HydrationProtocol { get; set; }
    public string? GrowthFactorSupport { get; set; }
    public string? DischargeInstructions { get; set; }
    public string? NursingNotes { get; set; }
    public string? PharmacistNotes { get; set; }
    public ChemoSessionStatus Status { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public OncologyVisit? OncologyVisit { get; set; }
    public OncologyTreatmentPlan? TreatmentPlan { get; set; }
}
