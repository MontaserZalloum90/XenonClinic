using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class InventoryItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventoryCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public DateTime CreatedDate { get; set; }
}
