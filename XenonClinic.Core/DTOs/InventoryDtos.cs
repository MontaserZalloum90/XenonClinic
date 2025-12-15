using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Inventory Item DTOs

/// <summary>
/// DTO for displaying inventory item information.
/// </summary>
public class InventoryItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventoryCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public int BranchId { get; set; }
    public string? BranchName { get; set; }

    // Stock Information
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }

    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal StockValue => QuantityOnHand * CostPrice;
    public decimal PotentialRevenue => QuantityOnHand * SellingPrice;
    public decimal ProfitMargin => SellingPrice > 0 ? ((SellingPrice - CostPrice) / SellingPrice) * 100 : 0;

    // Supplier Information
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierPartNumber { get; set; }

    // Additional Information
    public string? Barcode { get; set; }
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    // Status
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;
    public bool IsOutOfStock => QuantityOnHand == 0;
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow.Date;
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.UtcNow.Date.AddDays(30);
    public string StockStatus => IsOutOfStock ? "Out of Stock" : IsLowStock ? "Low Stock" : "In Stock";

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating a new inventory item.
/// NOTE: BranchId may be set by service layer from authenticated user context if not provided.
/// </summary>
public class CreateInventoryItemDto
{
    /// <summary>
    /// FIX: Branch ID. If not provided, will be set from authenticated user's context.
    /// </summary>
    public int? BranchId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// FIX: Changed to non-null to match Entity definition.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public InventoryCategory Category { get; set; }

    // Stock Information
    public int InitialQuantity { get; set; }
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
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing inventory item.
/// </summary>
public class UpdateInventoryItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InventoryCategory Category { get; set; }

    // Stock Thresholds
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
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for inventory item list request with filtering and pagination.
/// </summary>
public class InventoryItemListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public InventoryCategory? Category { get; set; }
    public int? SupplierId { get; set; }
    public bool? IsActive { get; set; }
    public bool? LowStockOnly { get; set; }
    public bool? OutOfStockOnly { get; set; }
    public bool? ExpiringSoon { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

#endregion

#region Inventory Transaction DTOs

/// <summary>
/// DTO for displaying inventory transaction information.
/// </summary>
public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType.ToString();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }

    // Reference Information
    public string? ReferenceNumber { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public int? TransferToBranchId { get; set; }
    public string? TransferToBranchName { get; set; }

    // Tracking
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }
    public int QuantityAfterTransaction { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for adding stock to inventory.
/// </summary>
public class AddStockDto
{
    public int InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for removing stock from inventory.
/// </summary>
public class RemoveStockDto
{
    public int InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public int? PatientId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for adjusting stock level (setting to specific quantity).
/// </summary>
public class AdjustStockDto
{
    public int InventoryItemId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for transferring stock between branches.
/// </summary>
public class TransferStockDto
{
    public int InventoryItemId { get; set; }
    public int TargetBranchId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a custom inventory transaction.
/// </summary>
public class CreateInventoryTransactionDto
{
    public int InventoryItemId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ReferenceNumber { get; set; }
    public int? PatientId { get; set; }
    public int? TransferToBranchId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for inventory transaction list request with filtering and pagination.
/// </summary>
public class InventoryTransactionListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? InventoryItemId { get; set; }
    public InventoryTransactionType? TransactionType { get; set; }
    public int? PatientId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Statistics DTOs

/// <summary>
/// DTO for inventory statistics.
/// </summary>
public class InventoryStatisticsDto
{
    public int TotalItems { get; set; }
    public int ActiveItems { get; set; }
    public int InactiveItems { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int ExpiringSoonItems { get; set; }
    public int ExpiredItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public decimal TotalPotentialRevenue { get; set; }
    public Dictionary<InventoryCategory, int> ItemsByCategory { get; set; } = new();
    public Dictionary<InventoryTransactionType, int> TransactionsByType { get; set; } = new();
    public List<InventoryItemDto> TopLowStockItems { get; set; } = new();
    public List<InventoryItemDto> TopExpiringItems { get; set; } = new();
}

/// <summary>
/// DTO for stock level summary.
/// </summary>
public class StockLevelSummaryDto
{
    public int InventoryItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public decimal CurrentValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ReorderQuantity => Math.Max(0, MaxStockLevel - CurrentQuantity);
}

#endregion

#region Validation Messages

/// <summary>
/// Validation messages for inventory operations.
/// </summary>
public static class InventoryValidationMessages
{
    // Item Code
    public const string ItemCodeRequired = "Item code is required";
    public const string ItemCodeTooLong = "Item code cannot exceed 50 characters";
    public const string ItemCodeInvalid = "Item code format is invalid";

    // Item Name
    public const string ItemNameRequired = "Item name is required";
    public const string ItemNameTooLong = "Item name cannot exceed 200 characters";

    // Category
    public const string CategoryInvalid = "Invalid inventory category";

    // Quantity
    public const string QuantityRequired = "Quantity is required";
    public const string QuantityInvalid = "Quantity must be greater than 0";
    public const string NewQuantityInvalid = "New quantity cannot be negative";
    public const string InsufficientStock = "Insufficient stock for this operation";

    // Stock Levels
    public const string ReorderLevelInvalid = "Reorder level cannot be negative";
    public const string MaxStockLevelInvalid = "Max stock level must be greater than reorder level";

    // Pricing
    public const string CostPriceInvalid = "Cost price cannot be negative";
    public const string SellingPriceInvalid = "Selling price cannot be negative";
    public const string SellingPriceBelowCost = "Selling price should not be less than cost price";

    // Transaction
    public const string TransactionTypeInvalid = "Invalid transaction type";
    public const string InventoryItemRequired = "Inventory item is required";
    public const string UnitPriceInvalid = "Unit price cannot be negative";

    // Transfer
    public const string TargetBranchRequired = "Target branch is required for transfer";
    public const string TargetBranchSameAsSource = "Cannot transfer to the same branch";

    // Adjustment
    public const string AdjustmentReasonRequired = "Adjustment reason is required";

    // Expiry
    public const string ExpiryDateInvalid = "Expiry date must be in the future";

    // Supplier
    public const string SupplierInvalid = "Invalid supplier ID";

    // Barcode
    public const string BarcodeTooLong = "Barcode cannot exceed 100 characters";

    // Location
    public const string LocationTooLong = "Location cannot exceed 100 characters";

    // Pagination
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";

    // Date Range
    public const string DateRangeInvalid = "End date must be greater than or equal to start date";
}

#endregion
