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
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<InventoryItem?> GetInventoryItemByCodeAsync(string itemCode, int branchId)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ItemCode == itemCode && i.BranchId == branchId);
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsByBranchIdAsync(int branchId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.BranchId == branchId)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetActiveInventoryItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.BranchId == branchId && i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int branchId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.BranchId == branchId &&
                   i.IsActive &&
                   i.QuantityOnHand <= i.ReorderLevel)
            .OrderBy(i => i.Name)
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
        if (item.QuantityOnHand < 0)
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
            // BUG FIX: Add null check instead of null-forgiving operator to prevent NullReferenceException
            .Where(t => t.InventoryItem != null && t.InventoryItem.BranchId == branchId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(int branchId, TransactionType transactionType)
    {
        return await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            // BUG FIX: Add null check instead of null-forgiving operator to prevent NullReferenceException
            .Where(t => t.InventoryItem != null && t.InventoryItem.BranchId == branchId &&
                   t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction)
    {
        // BUG FIX: Add null check for transaction parameter
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        // BUG FIX: Validate quantity is positive
        if (transaction.Quantity <= 0)
        {
            throw new InvalidOperationException("Transaction quantity must be greater than zero");
        }

        // BUG FIX: Validate item exists before transaction
        var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Inventory item with ID {transaction.InventoryItemId} not found");
        }

        // BUG FIX: Use database transaction to ensure atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var stockChange = transaction.TransactionType == TransactionType.Credit
                ? transaction.Quantity
                : -transaction.Quantity;

            // BUG FIX: Validate stock won't go negative for debits
            if (transaction.TransactionType == TransactionType.Debit && item.QuantityOnHand < transaction.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {item.QuantityOnHand}, Requested: {transaction.Quantity}");
            }

            _context.InventoryTransactions.Add(transaction);

            // Update item stock level
            item.QuantityOnHand += stockChange;
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

    public async Task UpdateTransactionAsync(InventoryTransaction transaction)
    {
        // BUG FIX: Add null check for transaction parameter
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        // BUG FIX: Validate quantity is positive
        if (transaction.Quantity <= 0)
        {
            throw new InvalidOperationException("Transaction quantity must be greater than zero");
        }

        // Get the old transaction to calculate difference
        var oldTransaction = await _context.InventoryTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        // BUG FIX: Throw exception if old transaction not found
        if (oldTransaction == null)
        {
            throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found");
        }

        // BUG FIX: Use database transaction to ensure atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
            if (item == null)
            {
                throw new KeyNotFoundException($"Inventory item with ID {transaction.InventoryItemId} not found");
            }

            // Reverse old transaction effect
            var oldStockChange = oldTransaction.TransactionType == TransactionType.Credit
                ? oldTransaction.Quantity
                : -oldTransaction.Quantity;
            // Apply new transaction effect
            var newStockChange = transaction.TransactionType == TransactionType.Credit
                ? transaction.Quantity
                : -transaction.Quantity;

            var newStock = item.QuantityOnHand - oldStockChange + newStockChange;

            // BUG FIX: Validate stock won't go negative
            if (newStock < 0)
            {
                throw new InvalidOperationException($"Cannot update transaction - would result in negative stock ({newStock})");
            }

            _context.InventoryTransactions.Update(transaction);
            item.QuantityOnHand = newStock;
            item.LastRestockDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteTransactionAsync(int id)
    {
        // BUG FIX: Validate id is positive
        if (id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero", nameof(id));
        }

        var transaction = await _context.InventoryTransactions.FindAsync(id);
        // BUG FIX: Throw exception instead of silent failure
        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction with ID {id} not found");
        }

        // BUG FIX: Use database transaction to ensure atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
            if (item != null)
            {
                var stockChange = transaction.TransactionType == TransactionType.Credit
                    ? -transaction.Quantity  // Reverse the credit
                    : transaction.Quantity;  // Reverse the debit

                var newStock = item.QuantityOnHand + stockChange;

                // BUG FIX: Validate stock won't go negative after reversal
                if (newStock < 0)
                {
                    throw new InvalidOperationException($"Cannot delete transaction - would result in negative stock ({newStock})");
                }

                item.QuantityOnHand = newStock;
                item.LastRestockDate = DateTime.UtcNow;
            }

            _context.InventoryTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
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

            // Validate against MaxStockLevel if set (0 means no limit)
            if (item.MaxStockLevel > 0 && (item.QuantityOnHand + quantity) > item.MaxStockLevel)
            {
                throw new InvalidOperationException(
                    $"Adding {quantity} units would exceed maximum stock level of {item.MaxStockLevel}. " +
                    $"Current stock: {item.QuantityOnHand}");
            }

            var newStockLevel = item.QuantityOnHand + quantity;
            var transaction = new InventoryTransaction
            {
                InventoryItemId = itemId,
                TransactionType = TransactionType.Credit,
                Quantity = quantity,
                UnitPrice = unitCost,
                TotalAmount = quantity * unitCost,
                TransactionDate = DateTime.UtcNow,
                PerformedBy = userId,
                Notes = notes ?? "Stock added",
                QuantityAfterTransaction = newStockLevel
            };

            _context.InventoryTransactions.Add(transaction);

            // Update item stock level and unit cost in single save
            item.QuantityOnHand = newStockLevel;
            item.CostPrice = unitCost;
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

            if (item.QuantityOnHand < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {item.QuantityOnHand}, Requested: {quantity}");

            // Check for expired items - prevent dispensing expired stock
            if (item.ExpiryDate.HasValue && item.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException(
                    $"Cannot remove stock from expired item. Expiry date: {item.ExpiryDate.Value:yyyy-MM-dd}. " +
                    "Please dispose of expired stock and adjust inventory.");
            }

            // Warn if stock will fall below reorder level
            var remainingStock = item.QuantityOnHand - quantity;
            var stockWarning = remainingStock <= item.ReorderLevel
                ? $" Warning: Stock level ({remainingStock}) is at or below reorder level ({item.ReorderLevel})."
                : "";

            var transaction = new InventoryTransaction
            {
                InventoryItemId = itemId,
                TransactionType = TransactionType.Debit,
                Quantity = quantity,
                UnitPrice = item.CostPrice,
                TotalAmount = quantity * item.CostPrice,
                TransactionDate = DateTime.UtcNow,
                PerformedBy = userId,
                Notes = (notes ?? "Stock removed") + stockWarning,
                QuantityAfterTransaction = remainingStock
            };

            _context.InventoryTransactions.Add(transaction);

            // Update item stock level in single save
            item.QuantityOnHand -= quantity;

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

            var difference = newQuantity - item.QuantityOnHand;

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
                UnitPrice = item.CostPrice,
                TotalAmount = absoluteDifference * item.CostPrice,
                TransactionDate = DateTime.UtcNow,
                PerformedBy = userId,
                Notes = notes ?? $"Stock adjusted from {item.QuantityOnHand} to {newQuantity}"
            };

            _context.InventoryTransactions.Add(transaction);

            // Update item stock level in single save
            item.QuantityOnHand = newQuantity;

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
        return item?.QuantityOnHand ?? 0;
    }

    public async Task<decimal> GetStockValueAsync(int branchId)
    {
        return await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive)
            .SumAsync(i => i.QuantityOnHand * i.CostPrice);
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
                        i.QuantityOnHand <= i.ReorderLevel);
    }

    public async Task<int> GetOutOfStockItemsCountAsync(int branchId)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.BranchId == branchId &&
                        i.IsActive &&
                        i.QuantityOnHand == 0);
    }

    public async Task<decimal> GetTotalInventoryValueAsync(int branchId)
    {
        return await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive)
            .SumAsync(i => i.QuantityOnHand * i.CostPrice);
    }

    public async Task<Dictionary<TransactionType, int>> GetTransactionTypeDistributionAsync(int branchId)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            // BUG FIX: Add null check instead of null-forgiving operator to prevent NullReferenceException
            .Where(t => t.InventoryItem != null && t.InventoryItem.BranchId == branchId)
            .GroupBy(t => t.TransactionType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return transactions.ToDictionary(x => x.Type, x => x.Count);
    }

    #endregion
}
