using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class IVFCycle
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PartnerId { get; set; }
    public int CycleNumber { get; set; }
    public DateTime CycleStartDate { get; set; }
    public IVFProtocol Protocol { get; set; }
    public string? ProtocolDetails { get; set; }
    public DateTime? StimulationStartDate { get; set; }
    public int? StimulationDays { get; set; }
    public string? GonadotropinUsed { get; set; }
    public decimal? TotalGonadotropinDose { get; set; }
    public string? TriggerMedication { get; set; }
    public DateTime? TriggerDate { get; set; }
    public DateTime? EggRetrievalDate { get; set; }
    public int? OocytesRetrieved { get; set; }
    public int? MatureOocytes { get; set; }
    public string? FertilizationMethod { get; set; }
    public int? EmbryosFertilized { get; set; }
    public int? EmbryosDayThree { get; set; }
    public int? BlastocystsFormed { get; set; }
    public int? EmbryosTransferred { get; set; }
    public int? EmbryosFrozen { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? TransferType { get; set; }
    public string? EmbryoQuality { get; set; }
    public string? EndometrialThickness { get; set; }
    public string? TransferDifficulty { get; set; }
    public string? LutealSupport { get; set; }
    public DateTime? BetaHCGDate { get; set; }
    public decimal? BetaHCGLevel { get; set; }
    public IVFOutcome Outcome { get; set; }
    public string? PregnancyOutcome { get; set; }
    public string? Complications { get; set; }
    public bool? OHSS { get; set; }
    public string? OHSSGrade { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Patient? Partner { get; set; }
    public ICollection<EmbryoRecord> Embryos { get; set; } = new List<EmbryoRecord>();
}
