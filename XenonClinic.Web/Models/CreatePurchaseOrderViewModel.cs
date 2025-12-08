using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreatePurchaseOrderViewModel
{
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal? TaxPercentage { get; set; } = 5;

    [Range(0, double.MaxValue)]
    public decimal? ShippingCost { get; set; }

    public string? Notes { get; set; }

    public string? Terms { get; set; }

    // Items (will be added dynamically via JavaScript)
    public List<PurchaseOrderItemViewModel> Items { get; set; } = new();
}

public class PurchaseOrderItemViewModel
{
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
    public int OrderedQuantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal? TaxPercentage { get; set; }

    public string? Notes { get; set; }
}
