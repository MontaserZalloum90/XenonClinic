using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a sales transaction/invoice
/// </summary>
public class Sale
{
    public int Id { get; set; }

    // Sale Information
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Draft;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // Customer Information
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    // Branch
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    // Financial Details
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default 5%
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }

    // Additional Information
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Related Quotation
    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    // Audit fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Computed Properties
    public decimal Balance => Total - PaidAmount;
    public bool IsFullyPaid => PaidAmount >= Total && Total > 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsFullyPaid;
}
