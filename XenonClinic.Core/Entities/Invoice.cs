using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an invoice for billing purposes
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    // Invoice Identification
    public string InvoiceNumber { get; set; } = string.Empty;

    // Customer Information
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Branch
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Dates
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // Financial Details
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default 5%
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    // Payment Information
    public PaymentMethod? PaymentMethod { get; set; }

    // Additional Information
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Reference to related entities
    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Computed Properties
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public bool IsFullyPaid => PaidAmount >= TotalAmount && TotalAmount > 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsFullyPaid;
}
