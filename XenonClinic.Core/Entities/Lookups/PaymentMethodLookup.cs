namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for payment methods (replaces PaymentMethod enum).
/// Examples: Cash, Card, Bank Transfer, Insurance, Installment, Cheque
/// </summary>
public class PaymentMethodLookup : SystemLookup
{
    /// <summary>
    /// Whether this payment method requires additional verification.
    /// </summary>
    public bool RequiresVerification { get; set; } = false;

    /// <summary>
    /// Whether this payment method supports installments.
    /// </summary>
    public bool SupportsInstallments { get; set; } = false;

    /// <summary>
    /// Optional processing fee percentage.
    /// </summary>
    public decimal? ProcessingFeePercentage { get; set; }

    // Navigation properties (if needed in future)
}
