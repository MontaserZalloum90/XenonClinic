namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for supplier payment statuses (replaces SupplierPaymentStatus enum).
/// Examples: Pending, Paid, Cancelled
/// </summary>
public class SupplierPaymentStatusLookup : SystemLookup
{
    public bool IsPaidStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<SupplierPayment> SupplierPayments { get; set; } = new List<SupplierPayment>();
}
