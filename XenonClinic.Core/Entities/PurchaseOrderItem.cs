using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class PurchaseOrderItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

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
    public int OrderedQuantity { get; set; }

    public int ReceivedQuantity { get; set; }
    public int RemainingQuantity => OrderedQuantity - ReceivedQuantity;

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }

    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }

    [Required]
    public decimal Total { get; set; }

    public string? Notes { get; set; }
}
