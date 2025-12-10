namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents an individual item/procedure within a dental treatment plan
/// </summary>
public class DentalTreatmentPlanItem
{
    public int Id { get; set; }
    public int DentalTreatmentPlanId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// Sequence/order in the treatment plan
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Phase of treatment (e.g., Phase 1: Emergency, Phase 2: Disease Control, Phase 3: Restorative, Phase 4: Maintenance)
    /// </summary>
    public string? Phase { get; set; }

    /// <summary>
    /// CDT procedure code
    /// </summary>
    public string? ProcedureCode { get; set; }

    public string ProcedureName { get; set; } = string.Empty;
    public string? ToothNumbers { get; set; }
    public string? Surfaces { get; set; }

    /// <summary>
    /// Status of this item (Pending, Scheduled, InProgress, Completed, Cancelled, Declined)
    /// </summary>
    public string Status { get; set; } = "Pending";

    public decimal? EstimatedFee { get; set; }
    public decimal? ActualFee { get; set; }
    public decimal? InsuranceCoverage { get; set; }
    public decimal? PatientResponsibility { get; set; }

    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public DentalTreatmentPlan? TreatmentPlan { get; set; }
    public Branch? Branch { get; set; }
}
