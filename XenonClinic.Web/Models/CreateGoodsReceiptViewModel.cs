using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateGoodsReceiptViewModel
{
    [Required]
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    public int? PurchaseOrderId { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [MaxLength(50)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime? SupplierInvoiceDate { get; set; }

    [MaxLength(50)]
    public string? DeliveryNoteNumber { get; set; }

    [MaxLength(100)]
    public string? ReceivedBy { get; set; }

    public string? Notes { get; set; }

    public bool UpdateInventory { get; set; } = true;

    // Items (will be added dynamically via JavaScript)
    public List<GoodsReceiptItemViewModel> Items { get; set; } = new();
}

public class GoodsReceiptItemViewModel
{
    public int? PurchaseOrderItemId { get; set; }

    public int? InventoryItemId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ItemDescription { get; set; }

    [MaxLength(50)]
    public string? ItemCode { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ReceivedQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int? AcceptedQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int? RejectedQuantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [MaxLength(50)]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Notes { get; set; }
}
