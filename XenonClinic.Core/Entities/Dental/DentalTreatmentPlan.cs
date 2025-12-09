namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a comprehensive dental treatment plan for a patient
/// </summary>
public class DentalTreatmentPlan
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Overall status (Draft, Proposed, Accepted, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Priority level (Low, Medium, High, Urgent)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }

    public decimal? EstimatedTotalCost { get; set; }
    public decimal? ActualTotalCost { get; set; }

    /// <summary>
    /// Insurance pre-authorization reference
    /// </summary>
    public string? InsurancePreAuthRef { get; set; }

    public string? ProviderId { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<DentalTreatmentPlanItem> Items { get; set; } = new List<DentalTreatmentPlanItem>();
    public ICollection<DentalVisit> Visits { get; set; } = new List<DentalVisit>();
}
