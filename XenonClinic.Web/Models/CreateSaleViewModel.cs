using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateSaleViewModel
{
    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int? QuotationId { get; set; }

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal? TaxPercentage { get; set; } = 5;

    public string? Notes { get; set; }

    public string? Terms { get; set; }

    // Items (will be added dynamically via JavaScript)
    public List<SaleItemViewModel> Items { get; set; } = new();
}

public class SaleItemViewModel
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

    public string? SerialNumber { get; set; }

    public DateTime? WarrantyStartDate { get; set; }

    public DateTime? WarrantyEndDate { get; set; }

    public string? Notes { get; set; }
}
