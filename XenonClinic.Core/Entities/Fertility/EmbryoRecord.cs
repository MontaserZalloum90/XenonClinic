using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class EmbryoRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int IVFCycleId { get; set; }
    public string? EmbryoIdentifier { get; set; }
    public DateTime CreationDate { get; set; }
    public EmbryoStatus Status { get; set; }
    public string? DayThreeGrade { get; set; }
    public int? DayThreeCellCount { get; set; }
    public string? DayThreeFragmentation { get; set; }
    public string? BlastocystGrade { get; set; }
    public string? BlastocystExpansion { get; set; }
    public string? ICMGrade { get; set; }
    public string? TEGrade { get; set; }
    public bool? PGTAPerformed { get; set; }
    public string? PGTAResult { get; set; }
    public string? Ploidy { get; set; }
    public string? Sex { get; set; }
    public bool? PGTMPerformed { get; set; }
    public string? PGTMResult { get; set; }
    public bool? BiopsyPerformed { get; set; }
    public DateTime? BiopsyDate { get; set; }
    public DateTime? FreezingDate { get; set; }
    public string? FreezingMethod { get; set; }
    public string? StorageLocation { get; set; }
    public string? StorageTank { get; set; }
    public string? StorageCanister { get; set; }
    public string? StorageCane { get; set; }
    public DateTime? ThawDate { get; set; }
    public string? ThawResult { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? TransferOutcome { get; set; }
    public DateTime? DiscardDate { get; set; }
    public string? DiscardReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public IVFCycle? IVFCycle { get; set; }
}
