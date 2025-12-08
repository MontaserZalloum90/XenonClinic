using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Inventory management
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly ClinicDbContext _context;

    public InventoryService(ClinicDbContext context)
    {
        _context = context;
    }

    #region Inventory Item Management

    public async Task<InventoryItem?> GetInventoryItemByIdAsync(int id)
    {
        return await _context.InventoryItems
            .Include(i => i.Branch)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<InventoryItem?> GetInventoryItemByCodeAsync(string itemCode, int branchId)
    {
        return await _context.InventoryItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.ItemCode == itemCode && i.BranchId == branchId);
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsByBranchIdAsync(int branchId)
    {
        return await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetActiveInventoryItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId && i.IsActive)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId &&
                   i.IsActive &&
                   i.CurrentStock <= i.ReorderLevel)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInventoryItemAsync(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Inventory Transaction Management

    public async Task<InventoryTransaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByItemIdAsync(int itemId)
    {
        return await _context.InventoryTransactions
            .Where(t => t.InventoryItemId == itemId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByBranchIdAsync(int branchId)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Where(t => t.InventoryItem!.BranchId == branchId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(int branchId, TransactionType transactionType)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Where(t => t.InventoryItem!.BranchId == branchId &&
                   t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction)
    {
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Update item stock level
        await UpdateItemStockLevelAsync(transaction.InventoryItemId);

        return transaction;
    }

    public async Task UpdateTransactionAsync(InventoryTransaction transaction)
    {
        _context.InventoryTransactions.Update(transaction);
        await _context.SaveChangesAsync();

        // Recalculate item stock level
        await UpdateItemStockLevelAsync(transaction.InventoryItemId);
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.InventoryTransactions.FindAsync(id);
        if (transaction != null)
        {
            var itemId = transaction.InventoryItemId;
            _context.InventoryTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            // Recalculate item stock level
            await UpdateItemStockLevelAsync(itemId);
        }
    }

    private async Task UpdateItemStockLevelAsync(int itemId)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item != null)
        {
            var stockLevel = await _context.InventoryTransactions
                .Where(t => t.InventoryItemId == itemId)
                .SumAsync(t => t.TransactionType == TransactionType.Credit ? t.Quantity : -t.Quantity);

            item.CurrentStock = stockLevel;
            item.LastRestockDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Stock Operations

    public async Task<InventoryTransaction> AddStockAsync(int itemId, int quantity, decimal unitCost, string userId, string? notes)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Inventory item with ID {itemId} not found");

        var transaction = new InventoryTransaction
        {
            InventoryItemId = itemId,
            TransactionType = TransactionType.Credit,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            TransactionDate = DateTime.UtcNow,
            PerformedBy = userId,
            Notes = notes ?? "Stock added"
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Update item stock level and unit cost
        item.CurrentStock += quantity;
        item.UnitCost = unitCost;
        item.LastRestockDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<InventoryTransaction> RemoveStockAsync(int itemId, int quantity, string userId, string? notes)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Inventory item with ID {itemId} not found");

        if (item.CurrentStock < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {item.CurrentStock}, Requested: {quantity}");

        var transaction = new InventoryTransaction
        {
            InventoryItemId = itemId,
            TransactionType = TransactionType.Debit,
            Quantity = quantity,
            UnitCost = item.UnitCost,
            TotalCost = quantity * item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            PerformedBy = userId,
            Notes = notes ?? "Stock removed"
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Update item stock level
        item.CurrentStock -= quantity;
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<InventoryTransaction> AdjustStockAsync(int itemId, int newQuantity, string userId, string? notes)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Inventory item with ID {itemId} not found");

        var difference = newQuantity - item.CurrentStock;
        var transactionType = difference >= 0 ? TransactionType.Credit : TransactionType.Debit;
        var absoluteDifference = Math.Abs(difference);

        var transaction = new InventoryTransaction
        {
            InventoryItemId = itemId,
            TransactionType = transactionType,
            Quantity = absoluteDifference,
            UnitCost = item.UnitCost,
            TotalCost = absoluteDifference * item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            PerformedBy = userId,
            Notes = notes ?? $"Stock adjusted from {item.CurrentStock} to {newQuantity}"
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Update item stock level
        item.CurrentStock = newQuantity;
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<int> GetCurrentStockLevelAsync(int itemId)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        return item?.CurrentStock ?? 0;
    }

    public async Task<decimal> GetStockValueAsync(int branchId)
    {
        return await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive)
            .SumAsync(i => i.CurrentStock * i.UnitCost);
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalItemsCountAsync(int branchId)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.BranchId == branchId && i.IsActive);
    }

    public async Task<int> GetLowStockItemsCountAsync(int branchId)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.BranchId == branchId &&
                        i.IsActive &&
                        i.CurrentStock <= i.ReorderLevel);
    }

    public async Task<int> GetOutOfStockItemsCountAsync(int branchId)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.BranchId == branchId &&
                        i.IsActive &&
                        i.CurrentStock == 0);
    }

    public async Task<decimal> GetTotalInventoryValueAsync(int branchId)
    {
        return await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive)
            .SumAsync(i => i.CurrentStock * i.UnitCost);
    }

    public async Task<Dictionary<TransactionType, int>> GetTransactionTypeDistributionAsync(int branchId)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Where(t => t.InventoryItem!.BranchId == branchId)
            .GroupBy(t => t.TransactionType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return transactions.ToDictionary(x => x.Type, x => x.Count);
    }

    #endregion
}
