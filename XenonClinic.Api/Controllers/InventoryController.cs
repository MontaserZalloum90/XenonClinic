using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Controller for inventory management operations.
/// Handles inventory items, stock operations, and transactions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        ICurrentUserService currentUserService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Inventory Items

    /// <summary>
    /// Get all inventory items for the current branch.
    /// </summary>
    [HttpGet("items")]
    public async Task<IActionResult>> GetItems(
        [FromQuery] InventoryItemListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var items = await _inventoryService.GetInventoryItemsByBranchIdAsync(branchId.Value);

            // Apply filters
            var filteredItems = items.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm;
                filteredItems = filteredItems.Where(i =>
                    i.ItemCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (i.Description != null && i.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (request.Category.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.Category == request.Category.Value);
            }

            if (request.SupplierId.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.SupplierId == request.SupplierId.Value);
            }

            if (request.IsActive.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.IsActive == request.IsActive.Value);
            }

            if (request.LowStockOnly == true)
            {
                filteredItems = filteredItems.Where(i => i.IsLowStock);
            }

            if (request.OutOfStockOnly == true)
            {
                filteredItems = filteredItems.Where(i => i.IsOutOfStock);
            }

            if (request.ExpiringSoon == true)
            {
                var expiryThreshold = DateTime.UtcNow.Date.AddDays(30);
                filteredItems = filteredItems.Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value <= expiryThreshold);
            }

            var totalCount = filteredItems.Count();

            // Apply sorting
            filteredItems = request.SortBy?.ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? filteredItems.OrderByDescending(i => i.Name) : filteredItems.OrderBy(i => i.Name),
                "itemcode" => request.SortDescending ? filteredItems.OrderByDescending(i => i.ItemCode) : filteredItems.OrderBy(i => i.ItemCode),
                "quantity" => request.SortDescending ? filteredItems.OrderByDescending(i => i.QuantityOnHand) : filteredItems.OrderBy(i => i.QuantityOnHand),
                "category" => request.SortDescending ? filteredItems.OrderByDescending(i => i.Category) : filteredItems.OrderBy(i => i.Category),
                "expirydate" => request.SortDescending ? filteredItems.OrderByDescending(i => i.ExpiryDate) : filteredItems.OrderBy(i => i.ExpiryDate),
                _ => request.SortDescending ? filteredItems.OrderByDescending(i => i.CreatedAt) : filteredItems.OrderBy(i => i.Name)
            };

            // Apply pagination
            var pagedItems = filteredItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(MapToInventoryItemDto)
                .ToList();

            return ApiPaginated(pagedItems, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory items");
            return ApiServerError("Failed to retrieve inventory items");
        }
    }

    /// <summary>
    /// Get inventory item by ID.
    /// </summary>
    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        try
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            return ApiOk(MapToInventoryItemDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory item {Id}", id);
            return ApiServerError("Failed to retrieve inventory item");
        }
    }

    /// <summary>
    /// Get inventory item by item code.
    /// </summary>
    [HttpGet("items/by-code/{itemCode}")]
    public async Task<IActionResult> GetItemByCode(string itemCode)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var item = await _inventoryService.GetInventoryItemByCodeAsync(itemCode, branchId.Value);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            return ApiOk(MapToInventoryItemDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory item by code {ItemCode}", itemCode);
            return ApiServerError("Failed to retrieve inventory item");
        }
    }

    /// <summary>
    /// Get active inventory items.
    /// </summary>
    [HttpGet("items/active")]
    public async Task<IActionResult>> GetActiveItems()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var items = await _inventoryService.GetActiveInventoryItemsAsync(branchId.Value);
            return ApiOk(items.Select(MapToInventoryItemDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active inventory items");
            return ApiServerError("Failed to retrieve active inventory items");
        }
    }

    /// <summary>
    /// Get low stock items.
    /// </summary>
    [HttpGet("items/low-stock")]
    public async Task<IActionResult>> GetLowStockItems()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var items = await _inventoryService.GetLowStockItemsAsync(branchId.Value);
            return ApiOk(items.Select(MapToInventoryItemDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items");
            return ApiServerError("Failed to retrieve low stock items");
        }
    }

    /// <summary>
    /// Create a new inventory item.
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            // Check if item code already exists
            var existingItem = await _inventoryService.GetInventoryItemByCodeAsync(dto.ItemCode, branchId.Value);
            if (existingItem != null)
            {
                return ApiConflict($"Item with code '{dto.ItemCode}' already exists");
            }

            var item = new InventoryItem
            {
                ItemCode = dto.ItemCode,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Category = dto.Category,
                BranchId = branchId.Value,
                QuantityOnHand = dto.InitialQuantity,
                ReorderLevel = dto.ReorderLevel,
                MaxStockLevel = dto.MaxStockLevel,
                CostPrice = dto.CostPrice,
                SellingPrice = dto.SellingPrice,
                SupplierId = dto.SupplierId,
                SupplierPartNumber = dto.SupplierPartNumber,
                Barcode = dto.Barcode,
                Location = dto.Location,
                ExpiryDate = dto.ExpiryDate,
                Notes = dto.Notes,
                IsActive = true,
                CreatedBy = _currentUserService.UserId
            };

            var createdItem = await _inventoryService.CreateInventoryItemAsync(item);

            _logger.LogInformation("Inventory item created: {ItemCode} by {UserId}",
                item.ItemCode, _currentUserService.UserId);

            return ApiCreated(MapToInventoryItemDto(createdItem), message: "Inventory item created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item");
            return ApiServerError("Failed to create inventory item");
        }
    }

    /// <summary>
    /// Update an existing inventory item.
    /// </summary>
    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateInventoryItemDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            // Check if item code is being changed and already exists
            if (item.ItemCode != dto.ItemCode)
            {
                var existingItem = await _inventoryService.GetInventoryItemByCodeAsync(dto.ItemCode, item.BranchId);
                if (existingItem != null && existingItem.Id != id)
                {
                    return ApiConflict($"Item with code '{dto.ItemCode}' already exists");
                }
            }

            item.ItemCode = dto.ItemCode;
            item.Name = dto.Name;
            item.Description = dto.Description ?? string.Empty;
            item.Category = dto.Category;
            item.ReorderLevel = dto.ReorderLevel;
            item.MaxStockLevel = dto.MaxStockLevel;
            item.CostPrice = dto.CostPrice;
            item.SellingPrice = dto.SellingPrice;
            item.SupplierId = dto.SupplierId;
            item.SupplierPartNumber = dto.SupplierPartNumber;
            item.Barcode = dto.Barcode;
            item.Location = dto.Location;
            item.ExpiryDate = dto.ExpiryDate;
            item.Notes = dto.Notes;
            item.IsActive = dto.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = _currentUserService.UserId;

            await _inventoryService.UpdateInventoryItemAsync(item);

            _logger.LogInformation("Inventory item updated: {ItemCode} by {UserId}",
                item.ItemCode, _currentUserService.UserId);

            return ApiOk(MapToInventoryItemDto(item), "Inventory item updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item {Id}", id);
            return ApiServerError("Failed to update inventory item");
        }
    }

    /// <summary>
    /// Delete an inventory item.
    /// </summary>
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            await _inventoryService.DeleteInventoryItemAsync(id);

            _logger.LogInformation("Inventory item deleted: {ItemCode} by {UserId}",
                item.ItemCode, _currentUserService.UserId);

            return ApiOk("Inventory item deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item {Id}", id);
            return ApiServerError("Failed to delete inventory item");
        }
    }

    #endregion

    #region Stock Operations

    /// <summary>
    /// Add stock to an inventory item.
    /// </summary>
    [HttpPost("stock/add")]
    public async Task<IActionResult> AddStock([FromBody] AddStockDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            // BUG FIX: Validate quantity is positive
            if (dto.Quantity <= 0)
            {
                return ApiBadRequest("Quantity must be greater than zero");
            }

            // BUG FIX: Validate unit cost is not negative
            if (dto.UnitCost < 0)
            {
                return ApiBadRequest("Unit cost cannot be negative");
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(dto.InventoryItemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            var transaction = await _inventoryService.AddStockAsync(
                dto.InventoryItemId,
                dto.Quantity,
                dto.UnitCost,
                _currentUserService.RequireUserId(),
                dto.Notes);

            _logger.LogInformation("Stock added: {Quantity} units to item {ItemId} by {UserId}",
                dto.Quantity, dto.InventoryItemId, _currentUserService.UserId);

            return ApiOk(MapToTransactionDto(transaction, item), "Stock added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stock to item {ItemId}", dto.InventoryItemId);
            return ApiServerError("Failed to add stock");
        }
    }

    /// <summary>
    /// Remove stock from an inventory item.
    /// </summary>
    [HttpPost("stock/remove")]
    public async Task<IActionResult> RemoveStock([FromBody] RemoveStockDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            // BUG FIX: Validate quantity is positive
            if (dto.Quantity <= 0)
            {
                return ApiBadRequest("Quantity must be greater than zero");
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(dto.InventoryItemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            // NOTE: TOCTOU race condition exists between this check and the actual removal.
            // The service layer's RemoveStockAsync should use database-level locking
            // (e.g., optimistic concurrency with row versioning or pessimistic locking)
            // to ensure atomic stock removal. The service should:
            // 1. Lock the inventory item row (SELECT FOR UPDATE)
            // 2. Re-check current stock level
            // 3. Perform the removal if sufficient stock
            // 4. Update the item's QuantityOnHand
            // All within a single transaction
            var currentStock = await _inventoryService.GetCurrentStockLevelAsync(dto.InventoryItemId);
            if (currentStock < dto.Quantity)
            {
                return ApiBadRequest($"Insufficient stock. Available: {currentStock}, Requested: {dto.Quantity}");
            }

            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            var transaction = await _inventoryService.RemoveStockAsync(
                dto.InventoryItemId,
                dto.Quantity,
                _currentUserService.RequireUserId(),
                dto.Notes);

            _logger.LogInformation("Stock removed: {Quantity} units from item {ItemId} by {UserId}",
                dto.Quantity, dto.InventoryItemId, _currentUserService.UserId);

            return ApiOk(MapToTransactionDto(transaction, item), "Stock removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing stock from item {ItemId}", dto.InventoryItemId);
            return ApiServerError("Failed to remove stock");
        }
    }

    /// <summary>
    /// Adjust stock level to a specific quantity.
    /// </summary>
    [HttpPost("stock/adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            // BUG FIX: Validate new quantity is not negative
            if (dto.NewQuantity < 0)
            {
                return ApiBadRequest("Stock quantity cannot be negative");
            }

            // BUG FIX: Require a reason for stock adjustment (audit trail requirement)
            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return ApiBadRequest("A reason is required for stock adjustment");
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(dto.InventoryItemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            var transaction = await _inventoryService.AdjustStockAsync(
                dto.InventoryItemId,
                dto.NewQuantity,
                _currentUserService.RequireUserId(),
                $"{dto.Reason}. {dto.Notes}".Trim());

            _logger.LogInformation("Stock adjusted: item {ItemId} from {OldQuantity} to {NewQuantity} by {UserId}, Reason: {Reason}",
                dto.InventoryItemId, item.QuantityOnHand, dto.NewQuantity, _currentUserService.UserId, dto.Reason);

            return ApiOk(MapToTransactionDto(transaction, item), "Stock adjusted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for item {ItemId}", dto.InventoryItemId);
            return ApiServerError("Failed to adjust stock");
        }
    }

    /// <summary>
    /// Get current stock level for an item.
    /// </summary>
    [HttpGet("stock/{itemId}/level")]
    public async Task<IActionResult> GetStockLevel(int itemId)
    {
        try
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(itemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            var stockLevel = await _inventoryService.GetCurrentStockLevelAsync(itemId);
            return ApiOk(stockLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock level for item {ItemId}", itemId);
            return ApiServerError("Failed to get stock level");
        }
    }

    /// <summary>
    /// Get total stock value for the branch.
    /// </summary>
    [HttpGet("stock/value")]
    public async Task<IActionResult> GetStockValue()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var value = await _inventoryService.GetStockValueAsync(branchId.Value);
            return ApiOk(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock value");
            return ApiServerError("Failed to get stock value");
        }
    }

    #endregion

    #region Transactions

    /// <summary>
    /// Get transactions for the current branch.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult>> GetTransactions(
        [FromQuery] InventoryTransactionListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var transactions = await _inventoryService.GetTransactionsByBranchIdAsync(branchId.Value);

            // Apply filters
            var filteredTransactions = transactions.AsQueryable();

            if (request.InventoryItemId.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.InventoryItemId == request.InventoryItemId.Value);
            }

            if (request.TransactionType.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.TransactionType == request.TransactionType.Value);
            }

            if (request.PatientId.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.PatientId == request.PatientId.Value);
            }

            if (request.DateFrom.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate <= request.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(request.ReferenceNumber))
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.ReferenceNumber != null && t.ReferenceNumber.Contains(request.ReferenceNumber));
            }

            var totalCount = filteredTransactions.Count();

            // Apply sorting
            filteredTransactions = request.SortBy?.ToLowerInvariant() switch
            {
                "amount" => request.SortDescending ? filteredTransactions.OrderByDescending(t => t.TotalAmount) : filteredTransactions.OrderBy(t => t.TotalAmount),
                "quantity" => request.SortDescending ? filteredTransactions.OrderByDescending(t => t.Quantity) : filteredTransactions.OrderBy(t => t.Quantity),
                _ => request.SortDescending ? filteredTransactions.OrderByDescending(t => t.TransactionDate) : filteredTransactions.OrderBy(t => t.TransactionDate)
            };

            // Apply pagination
            var pagedTransactions = filteredTransactions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => MapToTransactionDto(t, t.InventoryItem))
                .ToList();

            return ApiPaginated(pagedTransactions, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory transactions");
            return ApiServerError("Failed to retrieve transactions");
        }
    }

    /// <summary>
    /// Get transaction by ID.
    /// </summary>
    [HttpGet("transactions/{id}")]
    public async Task<IActionResult> GetTransaction(int id)
    {
        try
        {
            var transaction = await _inventoryService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return ApiNotFound("Transaction not found");
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(transaction.InventoryItemId);
            if (item == null || !HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this transaction");
            }

            return ApiOk(MapToTransactionDto(transaction, item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {Id}", id);
            return ApiServerError("Failed to retrieve transaction");
        }
    }

    /// <summary>
    /// Get transactions for a specific item.
    /// </summary>
    [HttpGet("items/{itemId}/transactions")]
    public async Task<IActionResult>> GetItemTransactions(int itemId)
    {
        try
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(itemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            var transactions = await _inventoryService.GetTransactionsByItemIdAsync(itemId);
            return ApiOk(transactions.Select(t => MapToTransactionDto(t, item)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for item {ItemId}", itemId);
            return ApiServerError("Failed to retrieve transactions");
        }
    }

    /// <summary>
    /// Create a custom inventory transaction.
    /// </summary>
    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateInventoryTransactionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var item = await _inventoryService.GetInventoryItemByIdAsync(dto.InventoryItemId);
            if (item == null)
            {
                return ApiNotFound("Inventory item not found");
            }

            if (!HasBranchAccess(item.BranchId))
            {
                return ApiForbidden("Access denied to this branch's inventory");
            }

            var transaction = new InventoryTransaction
            {
                InventoryItemId = dto.InventoryItemId,
                TransactionType = dto.TransactionType,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalAmount = dto.Quantity * dto.UnitPrice,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = dto.ReferenceNumber,
                PatientId = dto.PatientId,
                TransferToBranchId = dto.TransferToBranchId,
                PerformedBy = _currentUserService.UserId,
                Notes = dto.Notes,
                QuantityAfterTransaction = item.QuantityOnHand + dto.Quantity,
                CreatedBy = _currentUserService.UserId
            };

            var createdTransaction = await _inventoryService.CreateTransactionAsync(transaction);

            _logger.LogInformation("Transaction created: {Type} for item {ItemId} by {UserId}",
                dto.TransactionType, dto.InventoryItemId, _currentUserService.UserId);

            return ApiCreated(MapToTransactionDto(createdTransaction, item), message: "Transaction created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for item {ItemId}", dto.InventoryItemId);
            return ApiServerError("Failed to create transaction");
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get inventory statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var items = await _inventoryService.GetInventoryItemsByBranchIdAsync(branchId.Value);
            var itemsList = items.ToList();

            var totalItems = await _inventoryService.GetTotalItemsCountAsync(branchId.Value);
            var lowStockCount = await _inventoryService.GetLowStockItemsCountAsync(branchId.Value);
            var outOfStockCount = await _inventoryService.GetOutOfStockItemsCountAsync(branchId.Value);
            var totalValue = await _inventoryService.GetTotalInventoryValueAsync(branchId.Value);
            var transactionDistribution = await _inventoryService.GetTransactionTypeDistributionAsync(branchId.Value);

            var expiryThreshold = DateTime.UtcNow.Date.AddDays(30);

            var statistics = new InventoryStatisticsDto
            {
                TotalItems = totalItems,
                ActiveItems = itemsList.Count(i => i.IsActive),
                InactiveItems = itemsList.Count(i => !i.IsActive),
                LowStockItems = lowStockCount,
                OutOfStockItems = outOfStockCount,
                ExpiringSoonItems = itemsList.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value <= expiryThreshold && i.ExpiryDate.Value > DateTime.UtcNow.Date),
                ExpiredItems = itemsList.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < DateTime.UtcNow.Date),
                TotalInventoryValue = totalValue,
                TotalPotentialRevenue = itemsList.Sum(i => i.QuantityOnHand * i.SellingPrice),
                ItemsByCategory = itemsList
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TransactionsByType = transactionDistribution
                    .ToDictionary(kvp => (InventoryTransactionType)Enum.Parse(typeof(InventoryTransactionType), kvp.Key.ToString()), kvp => kvp.Value),
                TopLowStockItems = itemsList
                    .Where(i => i.IsLowStock)
                    .OrderBy(i => i.QuantityOnHand)
                    .Take(10)
                    .Select(MapToInventoryItemDto)
                    .ToList(),
                TopExpiringItems = itemsList
                    .Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value <= expiryThreshold)
                    .OrderBy(i => i.ExpiryDate)
                    .Take(10)
                    .Select(MapToInventoryItemDto)
                    .ToList()
            };

            return ApiOk(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory statistics");
            return ApiServerError("Failed to retrieve inventory statistics");
        }
    }

    /// <summary>
    /// Get stock level summary for all items.
    /// </summary>
    [HttpGet("stock/summary")]
    public async Task<IActionResult>> GetStockSummary()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var items = await _inventoryService.GetActiveInventoryItemsAsync(branchId.Value);

            var summary = items.Select(i => new StockLevelSummaryDto
            {
                InventoryItemId = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.Name,
                CurrentQuantity = i.QuantityOnHand,
                ReorderLevel = i.ReorderLevel,
                MaxStockLevel = i.MaxStockLevel,
                CurrentValue = i.QuantityOnHand * i.CostPrice,
                Status = i.IsOutOfStock ? "Out of Stock" : i.IsLowStock ? "Low Stock" : "In Stock"
            });

            return ApiOk(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock summary");
            return ApiServerError("Failed to retrieve stock summary");
        }
    }

    #endregion

    #region Helper Methods

    private bool HasBranchAccess(int branchId)
    {
        return _currentUserService.BranchId == branchId ||
               _currentUserService.HasRole("SuperAdmin");
    }

    private static InventoryItemDto MapToInventoryItemDto(InventoryItem item)
    {
        return new InventoryItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            Name = item.Name,
            Description = item.Description,
            Category = item.Category,
            BranchId = item.BranchId,
            BranchName = item.Branch?.Name,
            QuantityOnHand = item.QuantityOnHand,
            ReorderLevel = item.ReorderLevel,
            MaxStockLevel = item.MaxStockLevel,
            CostPrice = item.CostPrice,
            SellingPrice = item.SellingPrice,
            SupplierId = item.SupplierId,
            SupplierName = item.Supplier?.Name,
            SupplierPartNumber = item.SupplierPartNumber,
            Barcode = item.Barcode,
            Location = item.Location,
            ExpiryDate = item.ExpiryDate,
            Notes = item.Notes,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            UpdatedAt = item.UpdatedAt,
            UpdatedBy = item.UpdatedBy
        };
    }

    private static InventoryTransactionDto MapToTransactionDto(InventoryTransaction transaction, InventoryItem? item)
    {
        return new InventoryTransactionDto
        {
            Id = transaction.Id,
            InventoryItemId = transaction.InventoryItemId,
            ItemCode = item?.ItemCode ?? string.Empty,
            ItemName = item?.Name ?? string.Empty,
            TransactionType = transaction.TransactionType,
            Quantity = transaction.Quantity,
            UnitPrice = transaction.UnitPrice,
            TotalAmount = transaction.TotalAmount,
            TransactionDate = transaction.TransactionDate,
            ReferenceNumber = transaction.ReferenceNumber,
            PatientId = transaction.PatientId,
            PatientName = transaction.Patient?.FullNameEn,
            TransferToBranchId = transaction.TransferToBranchId,
            TransferToBranchName = transaction.TransferToBranch?.Name,
            PerformedBy = transaction.PerformedBy,
            Notes = transaction.Notes,
            QuantityAfterTransaction = transaction.QuantityAfterTransaction,
            CreatedAt = transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy
        };
    }

    #endregion
}
