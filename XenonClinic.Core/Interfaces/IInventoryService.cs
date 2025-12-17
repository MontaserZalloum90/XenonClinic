using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Inventory management
/// </summary>
public interface IInventoryService
{
    // Inventory Item Management
    Task<InventoryItem?> GetInventoryItemByIdAsync(int id);
    Task<InventoryItem?> GetInventoryItemByCodeAsync(string itemCode, int branchId);
    Task<IEnumerable<InventoryItem>> GetInventoryItemsByBranchIdAsync(int branchId);
    Task<IEnumerable<InventoryItem>> GetActiveInventoryItemsAsync(int branchId);
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int branchId);
    Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item);
    Task UpdateInventoryItemAsync(InventoryItem item);
    Task DeleteInventoryItemAsync(int id);

    // Inventory Transaction Management
    Task<InventoryTransaction?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<InventoryTransaction>> GetTransactionsByItemIdAsync(int itemId);
    Task<IEnumerable<InventoryTransaction>> GetTransactionsByBranchIdAsync(int branchId);
    Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(int branchId, InventoryTransactionType transactionType);
    Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction);
    Task UpdateTransactionAsync(InventoryTransaction transaction);
    Task DeleteTransactionAsync(int id);

    // Stock Operations
    Task<InventoryTransaction> AddStockAsync(int itemId, int quantity, decimal unitCost, string userId, string? notes);
    Task<InventoryTransaction> RemoveStockAsync(int itemId, int quantity, string userId, string? notes);
    Task<InventoryTransaction> AdjustStockAsync(int itemId, int newQuantity, string userId, string? notes);
    Task<int> GetCurrentStockLevelAsync(int itemId);
    Task<decimal> GetStockValueAsync(int branchId);

    // Statistics & Reporting
    Task<int> GetTotalItemsCountAsync(int branchId);
    Task<int> GetLowStockItemsCountAsync(int branchId);
    Task<int> GetOutOfStockItemsCountAsync(int branchId);
    Task<decimal> GetTotalInventoryValueAsync(int branchId);
    Task<Dictionary<InventoryTransactionType, int>> GetTransactionTypeDistributionAsync(int branchId);
}
