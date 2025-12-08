using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a payment transaction for a sale
/// </summary>
public class Payment
{
    public int Id { get; set; }

    // Payment Information
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    // Sale Reference
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    // Payment Details
    public string? ReferenceNumber { get; set; } // Transaction ID, Check Number, etc.
    public string? BankName { get; set; }
    public string? CardLastFourDigits { get; set; }

    // Insurance Details (if applicable)
    public string? InsuranceCompany { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    // Installment Details (if applicable)
    public int? InstallmentNumber { get; set; }
    public int? TotalInstallments { get; set; }

    // Additional Information
    public string? Notes { get; set; }

    // Audit
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
