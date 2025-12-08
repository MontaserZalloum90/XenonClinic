namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for purchase order statuses (replaces PurchaseOrderStatus enum).
/// Examples: Draft, Submitted, Approved, Ordered, Partially Received, Received, Cancelled
/// </summary>
public class PurchaseOrderStatusLookup : SystemLookup
{
    public bool IsFinalStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public bool AllowsReceiving { get; set; } = false;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
