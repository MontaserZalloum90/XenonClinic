namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a line item in a sale
/// </summary>
public class SaleItem
{
    public int Id { get; set; }

    // Sale Reference
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    // Item Information
    public int? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    // Item Details (stored for history even if inventory item is deleted)
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? ItemCode { get; set; }

    // Pricing
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }

    // Tax
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }

    // Warranty Information
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? SerialNumber { get; set; }

    // Notes
    public string? Notes { get; set; }
}
