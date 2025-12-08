namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the payment status of an invoice
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment has been completed
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Invoice was cancelled
    /// </summary>
    Cancelled = 2
}
