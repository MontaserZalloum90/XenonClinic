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
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetActiveInventoryItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId && i.IsActive)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.BranchId == branchId &&
                   i.IsActive &&
                   i.CurrentStock <= i.ReorderLevel)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item)
    {
        // Check for duplicate item code within the same branch
        var existingItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ItemCode == item.ItemCode && i.BranchId == item.BranchId);

        if (existingItem != null)
        {
            throw new InvalidOperationException(
                $"An inventory item with code '{item.ItemCode}' already exists in this branch");
        }

        // Validate non-negative stock
        if (item.CurrentStock < 0)
        {
            throw new InvalidOperationException("Initial stock cannot be negative");
        }

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
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.Patient)
            .Include(t => t.TransferToBranch)
            .Where(t => t.InventoryItemId == itemId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByBranchIdAsync(int branchId)
    {
        return await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.Patient)
            .Include(t => t.TransferToBranch)
            .Where(t => t.InventoryItem!.BranchId == branchId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(int branchId, TransactionType transactionType)
    {
        return await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Where(t => t.InventoryItem!.BranchId == branchId &&
                   t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction)
    {
        // Use single SaveChanges for both transaction and stock update
        _context.InventoryTransactions.Add(transaction);

        // Update item stock level in same save
        var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
        if (item != null)
        {
            var stockChange = transaction.TransactionType == TransactionType.Credit
                ? transaction.Quantity
                : -transaction.Quantity;
            item.CurrentStock += stockChange;
            item.LastRestockDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateTransactionAsync(InventoryTransaction transaction)
    {
        // Get the old transaction to calculate difference
        var oldTransaction = await _context.InventoryTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        _context.InventoryTransactions.Update(transaction);

        // Recalculate item stock level in same save
        var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
        if (item != null && oldTransaction != null)
        {
            // Reverse old transaction effect
            var oldStockChange = oldTransaction.TransactionType == TransactionType.Credit
                ? oldTransaction.Quantity
                : -oldTransaction.Quantity;
            // Apply new transaction effect
            var newStockChange = transaction.TransactionType == TransactionType.Credit
                ? transaction.Quantity
                : -transaction.Quantity;

            item.CurrentStock = item.CurrentStock - oldStockChange + newStockChange;
            item.LastRestockDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.InventoryTransactions.FindAsync(id);
        if (transaction != null)
        {
            // Update item stock in same save
            var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
            if (item != null)
            {
                var stockChange = transaction.TransactionType == TransactionType.Credit
                    ? -transaction.Quantity  // Reverse the credit
                    : transaction.Quantity;  // Reverse the debit
                item.CurrentStock += stockChange;
                item.LastRestockDate = DateTime.UtcNow;
            }

            _context.InventoryTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Stock Operations

    public async Task<InventoryTransaction> AddStockAsync(int itemId, int quantity, decimal unitCost, string userId, string? notes)
    {
        // Validate quantity
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero");
        }

        if (unitCost < 0)
        {
            throw new InvalidOperationException("Unit cost cannot be negative");
        }

        // Use transaction to prevent race conditions
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
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

            // Update item stock level and unit cost in single save
            item.CurrentStock += quantity;
            item.UnitCost = unitCost;
            item.LastRestockDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<InventoryTransaction> RemoveStockAsync(int itemId, int quantity, string userId, string? notes)
    {
        // Validate quantity
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero");
        }

        // Use transaction to prevent race conditions
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
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

            // Update item stock level in single save
            item.CurrentStock -= quantity;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<InventoryTransaction> AdjustStockAsync(int itemId, int newQuantity, string userId, string? notes)
    {
        // Validate non-negative stock
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("Stock quantity cannot be negative");
        }

        // Use transaction to prevent race conditions
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Inventory item with ID {itemId} not found");

            var difference = newQuantity - item.CurrentStock;

            // No transaction needed if no change
            if (difference == 0)
            {
                await dbTransaction.CommitAsync();
                return new InventoryTransaction
                {
                    InventoryItemId = itemId,
                    TransactionType = TransactionType.Credit,
                    Quantity = 0,
                    Notes = "No adjustment needed - stock already at target level"
                };
            }

            var transactionType = difference > 0 ? TransactionType.Credit : TransactionType.Debit;
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

            // Update item stock level in single save
            item.CurrentStock = newQuantity;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
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
