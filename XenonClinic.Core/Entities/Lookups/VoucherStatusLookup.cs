namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for voucher statuses (replaces VoucherStatus enum).
/// Examples: Draft, Approved, Posted, Cancelled
/// </summary>
public class VoucherStatusLookup : SystemLookup
{
    public bool IsPostedStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
