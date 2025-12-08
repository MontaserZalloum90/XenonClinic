using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class GoodsReceipt
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    [Required]
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    [Required]
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    [MaxLength(50)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime? SupplierInvoiceDate { get; set; }

    [MaxLength(50)]
    public string? DeliveryNoteNumber { get; set; }

    [MaxLength(100)]
    public string? ReceivedBy { get; set; }

    public string? Notes { get; set; }

    public bool UpdateInventory { get; set; } = true;

    // Audit fields
    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}
