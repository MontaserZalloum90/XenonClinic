using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class InventoryItem
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventoryCategory Category { get; set; }
    public int BranchId { get; set; }

    // Stock Information
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }

    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }

    // Supplier Information
    public int? SupplierId { get; set; }
    public string? SupplierPartNumber { get; set; }

    // Additional Information
    public string? Barcode { get; set; }
    public string? Location { get; set; } // Storage location in branch
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();

    // Computed property
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;
    public bool IsOutOfStock => QuantityOnHand == 0;
}
