using XenonClinic.Core.Enums.Oncology;

namespace XenonClinic.Core.Entities.Oncology;

public class RadiationRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? OncologyVisitId { get; set; }
    public int? TreatmentPlanId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public RadiationType RadiationType { get; set; }
    public string? TreatmentSite { get; set; }
    public string? TreatmentIntent { get; set; }
    public decimal? DosePerFraction { get; set; }
    public decimal? TotalDosePlanned { get; set; }
    public decimal? TotalDoseDelivered { get; set; }
    public int FractionNumber { get; set; }
    public int TotalFractionsPlanned { get; set; }
    public string? TreatmentMachine { get; set; }
    public string? TreatmentTechnique { get; set; }
    public string? Energy { get; set; }
    public string? SimulationDate { get; set; }
    public string? ImagingVerification { get; set; }
    public string? AcuteSideEffects { get; set; }
    public int? SkinReactionGrade { get; set; }
    public int? MucositisGrade { get; set; }
    public int? FatigueGrade { get; set; }
    public string? OtherToxicities { get; set; }
    public bool? TreatmentBreak { get; set; }
    public string? BreakReason { get; set; }
    public int? BreakDays { get; set; }
    public string? PhysicistNotes { get; set; }
    public string? DosimetristNotes { get; set; }
    public string? RadOncoNotes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public OncologyVisit? OncologyVisit { get; set; }
    public OncologyTreatmentPlan? TreatmentPlan { get; set; }
}
