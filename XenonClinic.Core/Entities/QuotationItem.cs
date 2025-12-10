namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a line item in a quotation
/// </summary>
public class QuotationItem
{
    public int Id { get; set; }

    // Quotation Reference
    public int QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    // Item Information
    public int? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    // Item Details
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

    // Notes
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
