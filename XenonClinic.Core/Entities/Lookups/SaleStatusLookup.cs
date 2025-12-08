namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for sale statuses (replaces SaleStatus enum).
/// Examples: Draft, Confirmed, Completed, Cancelled, Refunded
/// </summary>
public class SaleStatusLookup : SystemLookup
{
    public bool IsCompletedStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
