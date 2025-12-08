namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for payment statuses (replaces PaymentStatus enum).
/// Examples: Pending, Partial, Paid, Refunded, Overdue, Cancelled
/// </summary>
public class PaymentStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether this status represents a fully paid status.
    /// </summary>
    public bool IsPaidStatus { get; set; } = false;

    /// <summary>
    /// Whether this status represents a pending status.
    /// </summary>
    public bool IsPendingStatus { get; set; } = false;

    /// <summary>
    /// Whether this status allows further payment attempts.
    /// </summary>
    public bool AllowsPayment { get; set; } = true;

    // Navigation properties (if needed in future)
}
