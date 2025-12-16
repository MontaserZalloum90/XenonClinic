using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an invoice for billing purposes
/// </summary>
public class Invoice : IBranchEntity
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
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

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
    public string? ServiceDescription { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Reference to related entities
    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    // Navigation Properties
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<InvoiceItem> LineItems => Items;
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();

    // Aliases for compatibility
    public decimal Tax => TaxAmount ?? 0;
    public decimal Discount => DiscountAmount ?? 0;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// BUG FIX: Row version for optimistic concurrency control.
    /// Prevents race conditions in concurrent payment processing.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Computed Properties
    // BUG FIX: Use proper rounding to ensure consistent 2 decimal places for currency
    public decimal RemainingAmount => Math.Round(TotalAmount - PaidAmount, 2, MidpointRounding.AwayFromZero);
    public bool IsFullyPaid => PaidAmount >= TotalAmount && TotalAmount > 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsFullyPaid;
}

/// <summary>
/// Represents a line item in an invoice
/// </summary>
public class InvoiceItem
{
    public int Id { get; set; }

    // Invoice Reference
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    // Item Details
    public string? ServiceCode { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalPrice => Amount;
    public DateTime? ServiceDate { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a payment for an invoice
/// </summary>
public class InvoicePayment
{
    public int Id { get; set; }

    // Invoice Reference
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    // Patient Reference
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Payment Details
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? Status { get; set; } = "Completed";
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
