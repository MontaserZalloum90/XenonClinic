using XenonClinic.Core.Enums.PainManagement;

namespace XenonClinic.Core.Entities.PainManagement;

public class PainMedicationRegimen
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PainMedicationStatus Status { get; set; }
    public string? MedicationName { get; set; }
    public PainMedicationClass MedicationClass { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public string? Indication { get; set; }
    public bool? IsControlledSubstance { get; set; }
    public string? DEASchedule { get; set; }
    public decimal? MMEDaily { get; set; }
    public string? PrescribingAgreementSigned { get; set; }
    public DateTime? LastUrineDrugScreen { get; set; }
    public string? UrineDrugScreenResult { get; set; }
    public DateTime? LastPDMPCheck { get; set; }
    public string? PDMPFindings { get; set; }
    public string? SideEffects { get; set; }
    public string? Effectiveness { get; set; }
    public string? PrescribingPhysician { get; set; }
    public int? RefillsRemaining { get; set; }
    public DateTime? LastRefillDate { get; set; }
    public string? Pharmacy { get; set; }
    public string? DiscontinuationReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}
