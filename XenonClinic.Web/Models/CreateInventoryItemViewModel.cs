using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateInventoryItemViewModel
{
    [Required(ErrorMessage = "Item code is required")]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public InventoryCategory Category { get; set; }

    [Required(ErrorMessage = "Branch is required")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
    public int QuantityOnHand { get; set; }

    [Required(ErrorMessage = "Reorder level is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be a positive number")]
    public int ReorderLevel { get; set; }

    [Required(ErrorMessage = "Max stock level is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Max stock level must be a positive number")]
    public int MaxStockLevel { get; set; }

    [Required(ErrorMessage = "Cost price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than zero")]
    public decimal CostPrice { get; set; }

    [Required(ErrorMessage = "Selling price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than zero")]
    public decimal SellingPrice { get; set; }

    public int? SupplierId { get; set; }

    [StringLength(100)]
    public string? SupplierPartNumber { get; set; }

    [StringLength(100)]
    public string? Barcode { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
