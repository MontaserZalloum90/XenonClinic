namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for lab order statuses (replaces LabOrderStatus enum).
/// Examples: Pending, Collected, In Progress, Completed, Cancelled
/// </summary>
public class LabOrderStatusLookup : SystemLookup
{
    public bool IsCompletedStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;
    public bool AllowsResultEntry { get; set; } = false;
    public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
}
