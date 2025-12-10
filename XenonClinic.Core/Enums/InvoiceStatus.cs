namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the status of an invoice
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// Invoice is in draft state, not yet finalized
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Invoice has been issued to the customer
    /// </summary>
    Issued = 1,

    /// <summary>
    /// Invoice has received partial payment
    /// </summary>
    PartiallyPaid = 2,

    /// <summary>
    /// Invoice has been fully paid
    /// </summary>
    Paid = 3,

    /// <summary>
    /// Invoice payment is overdue
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Invoice has been cancelled
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Invoice has been refunded
    /// </summary>
    Refunded = 6
}
