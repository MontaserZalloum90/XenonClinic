using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a payment transaction for a sale
/// </summary>
public class Payment : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation.
    /// Required for all transactional entities.
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

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
