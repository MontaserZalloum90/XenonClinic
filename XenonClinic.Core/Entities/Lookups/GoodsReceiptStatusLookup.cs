namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for goods receipt statuses (replaces GoodsReceiptStatus enum).
/// Examples: Draft, Completed, Cancelled
/// </summary>
public class GoodsReceiptStatusLookup : SystemLookup
{
    public bool IsCompletedStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
