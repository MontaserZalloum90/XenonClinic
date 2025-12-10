using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    [Required]
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    [Required]
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public decimal SubTotal { get; set; }

    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }

    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }

    public decimal? ShippingCost { get; set; }

    [Required]
    public decimal Total { get; set; }

    public decimal ReceivedAmount { get; set; }
    public decimal Balance => Total - ReceivedAmount;
    public bool IsFullyReceived => ReceivedAmount >= Total && Total > 0;

    [MaxLength(50)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime? SupplierInvoiceDate { get; set; }

    public string? Notes { get; set; }

    public string? Terms { get; set; }

    // Audit fields
    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
}
