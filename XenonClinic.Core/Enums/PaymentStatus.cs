namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the payment status of an invoice or sale
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Partial payment has been received
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Payment has been completed in full
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment has been refunded
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment is overdue
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Invoice/Sale was cancelled
    /// </summary>
    Cancelled = 5
}
