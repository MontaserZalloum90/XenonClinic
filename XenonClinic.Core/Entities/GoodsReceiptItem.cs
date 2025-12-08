using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class GoodsReceiptItem
{
    public int Id { get; set; }

    [Required]
    public int GoodsReceiptId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }

    public int? PurchaseOrderItemId { get; set; }
    public PurchaseOrderItem? PurchaseOrderItem { get; set; }

    public int? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ItemDescription { get; set; }

    [MaxLength(50)]
    public string? ItemCode { get; set; }

    [Required]
    public int ReceivedQuantity { get; set; }

    public int? AcceptedQuantity { get; set; }

    public int? RejectedQuantity { get; set; }

    public decimal? UnitPrice { get; set; }

    [MaxLength(50)]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Notes { get; set; }
}
