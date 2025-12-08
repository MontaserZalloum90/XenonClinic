using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateQuotationViewModel
{
    [Required]
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(30);

    [Required]
    public int PatientId { get; set; }

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal? TaxPercentage { get; set; } = 5;

    public string? Notes { get; set; }

    public string? Terms { get; set; }

    [Range(1, 365)]
    public int ValidityDays { get; set; } = 30;

    // Items (will be added dynamically via JavaScript)
    public List<QuotationItemViewModel> Items { get; set; } = new();
}

public class QuotationItemViewModel
{
    public int? InventoryItemId { get; set; }

    [Required]
    public string ItemName { get; set; } = string.Empty;

    public string? ItemDescription { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

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
