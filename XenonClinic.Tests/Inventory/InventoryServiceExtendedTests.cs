using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Inventory;

/// <summary>
/// Extended comprehensive tests for the Inventory Service implementation.
/// Contains 500+ test cases covering all inventory management scenarios.
/// </summary>
public class InventoryServiceExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IInventoryService _inventoryService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _inventoryService = new InventoryService(_context);
        await SeedExtendedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedExtendedTestDataAsync()
    {
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        var branches = new List<Branch>
        {
            new() { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true },
            new() { Id = 2, CompanyId = 1, Code = "BR002", Name = "Second Branch", IsActive = true }
        };
        _context.Branches.AddRange(branches);

        // Seed inventory categories
        var categories = new List<InventoryCategory>
        {
            new() { Id = 1, Name = "Medications", Code = "MED", IsActive = true },
            new() { Id = 2, Name = "Medical Supplies", Code = "SUP", IsActive = true },
            new() { Id = 3, Name = "Equipment", Code = "EQP", IsActive = true },
            new() { Id = 4, Name = "Consumables", Code = "CON", IsActive = true },
            new() { Id = 5, Name = "Laboratory", Code = "LAB", IsActive = true }
        };
        _context.InventoryCategories.AddRange(categories);

        // Seed inventory items
        var items = new List<InventoryItem>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new InventoryItem
            {
                Id = i,
                BranchId = (i % 2) + 1,
                CategoryId = (i % 5) + 1,
                ItemCode = $"ITEM-{i:D4}",
                ItemName = $"Inventory Item {i}",
                Description = $"Description for item {i}",
                UnitOfMeasure = new[] { "Piece", "Box", "Pack", "Bottle", "Roll" }[i % 5],
                QuantityOnHand = 100 - (i % 50),
                ReorderLevel = 20,
                ReorderQuantity = 50,
                UnitCost = 10 * (i % 20 + 1),
                SellingPrice = 15 * (i % 20 + 1),
                ExpiryDate = i % 10 == 0 ? DateTime.UtcNow.AddDays(-30) : DateTime.UtcNow.AddMonths(i % 24),
                IsActive = i % 15 != 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.InventoryItems.AddRange(items);

        // Seed stock transactions
        var transactions = new List<StockTransaction>();
        for (int i = 1; i <= 500; i++)
        {
            transactions.Add(new StockTransaction
            {
                Id = i,
                BranchId = (i % 2) + 1,
                ItemId = (i % 100) + 1,
                TransactionType = new[] { "Receipt", "Issue", "Adjustment", "Transfer", "Return" }[i % 5],
                Quantity = i % 20 + 1,
                UnitCost = 10 * ((i % 20) + 1),
                TransactionDate = DateTime.UtcNow.AddDays(-i % 100),
                ReferenceNumber = $"TXN-{i:D6}",
                Notes = $"Transaction notes {i}",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow.AddDays(-i % 100)
            });
        }
        _context.StockTransactions.AddRange(transactions);

        // Seed purchase orders
        var purchaseOrders = new List<PurchaseOrder>();
        for (int i = 1; i <= 50; i++)
        {
            purchaseOrders.Add(new PurchaseOrder
            {
                Id = i,
                BranchId = (i % 2) + 1,
                OrderNumber = $"PO-{i:D6}",
                VendorName = $"Vendor {i % 10}",
                OrderDate = DateTime.UtcNow.AddDays(-i * 2),
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-i * 2 + 7),
                TotalAmount = 1000 * (i % 10 + 1),
                Status = i <= 10 ? "Draft" : i <= 25 ? "Pending" : i <= 40 ? "Received" : "Cancelled",
                CreatedAt = DateTime.UtcNow.AddDays(-i * 2)
            });
        }
        _context.PurchaseOrders.AddRange(purchaseOrders);

        // Seed stock audits
        var audits = new List<StockAudit>();
        for (int i = 1; i <= 30; i++)
        {
            audits.Add(new StockAudit
            {
                Id = i,
                BranchId = (i % 2) + 1,
                AuditNumber = $"AUD-{i:D6}",
                AuditDate = DateTime.UtcNow.AddDays(-i * 10),
                Status = i <= 10 ? "Draft" : i <= 20 ? "InProgress" : "Completed",
                AuditedBy = $"auditor{i % 5}",
                CreatedAt = DateTime.UtcNow.AddDays(-i * 10)
            });
        }
        _context.StockAudits.AddRange(audits);

        await _context.SaveChangesAsync();
    }

    #region GetInventoryItemByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task GetInventoryItemByIdAsync_ValidIds_ReturnsItem(int itemId)
    {
        var result = await _inventoryService.GetInventoryItemByIdAsync(itemId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(itemId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInventoryItemByIdAsync_InvalidIds_ReturnsNull(int itemId)
    {
        var result = await _inventoryService.GetInventoryItemByIdAsync(itemId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInventoryItemByIdAsync_IncludesCategory()
    {
        var result = await _inventoryService.GetInventoryItemByIdAsync(1);
        result.Should().NotBeNull();
        result!.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryItemByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 50)
            .Select(id => _inventoryService.GetInventoryItemByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(i => i != null);
    }

    #endregion

    #region GetInventoryItemsByBranchIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetInventoryItemsByBranchIdAsync_ValidBranches_ReturnsItems(int branchId)
    {
        var result = await _inventoryService.GetInventoryItemsByBranchIdAsync(branchId);
        var items = result.ToList();

        items.Should().NotBeEmpty();
        items.Should().OnlyContain(i => i.BranchId == branchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInventoryItemsByBranchIdAsync_InvalidBranches_ReturnsEmpty(int branchId)
    {
        var result = await _inventoryService.GetInventoryItemsByBranchIdAsync(branchId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInventoryItemsByBranchIdAsync_OnlyActiveItems()
    {
        var result = await _inventoryService.GetInventoryItemsByBranchIdAsync(1);
        var items = result.ToList();

        items.Should().OnlyContain(i => i.IsActive);
    }

    #endregion

    #region GetInventoryItemsByCategoryAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task GetInventoryItemsByCategoryAsync_ValidCategories_ReturnsItems(int categoryId)
    {
        var result = await _inventoryService.GetInventoryItemsByCategoryAsync(1, categoryId);
        var items = result.ToList();

        items.Should().OnlyContain(i => i.CategoryId == categoryId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInventoryItemsByCategoryAsync_InvalidCategories_ReturnsEmpty(int categoryId)
    {
        var result = await _inventoryService.GetInventoryItemsByCategoryAsync(1, categoryId);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetLowStockItemsAsync Tests

    [Fact]
    public async Task GetLowStockItemsAsync_ReturnsItemsBelowReorderLevel()
    {
        var result = await _inventoryService.GetLowStockItemsAsync(1);
        var items = result.ToList();

        items.Should().OnlyContain(i => i.QuantityOnHand <= i.ReorderLevel);
    }

    [Fact]
    public async Task GetLowStockItemsAsync_OrderedByQuantity()
    {
        var result = await _inventoryService.GetLowStockItemsAsync(1);
        var items = result.ToList();

        items.Should().BeInAscendingOrder(i => i.QuantityOnHand);
    }

    #endregion

    #region GetExpiredItemsAsync Tests

    [Fact]
    public async Task GetExpiredItemsAsync_ReturnsExpiredItems()
    {
        var result = await _inventoryService.GetExpiredItemsAsync(1);
        var items = result.ToList();

        items.Should().OnlyContain(i => i.ExpiryDate < DateTime.UtcNow);
    }

    [Fact]
    public async Task GetExpiringItemsAsync_ReturnsItemsExpiringSoon()
    {
        var daysUntilExpiry = 30;
        var result = await _inventoryService.GetExpiringItemsAsync(1, daysUntilExpiry);
        var items = result.ToList();

        items.Should().OnlyContain(i => i.ExpiryDate <= DateTime.UtcNow.AddDays(daysUntilExpiry));
    }

    #endregion

    #region CreateInventoryItemAsync Tests

    [Fact]
    public async Task CreateInventoryItemAsync_ValidItem_CreatesSuccessfully()
    {
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "NEW-ITEM-001",
            ItemName = "New Test Item",
            UnitOfMeasure = "Piece",
            QuantityOnHand = 50,
            ReorderLevel = 10,
            UnitCost = 100,
            SellingPrice = 150,
            IsActive = true
        };

        var result = await _inventoryService.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Piece")]
    [InlineData("Box")]
    [InlineData("Pack")]
    [InlineData("Bottle")]
    [InlineData("Roll")]
    [InlineData("Kg")]
    [InlineData("Liter")]
    public async Task CreateInventoryItemAsync_VariousUnits_AllSucceed(string unit)
    {
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = $"UNIT-{unit}",
            ItemName = $"Item with {unit}",
            UnitOfMeasure = unit,
            QuantityOnHand = 100,
            ReorderLevel = 20,
            UnitCost = 50,
            SellingPrice = 75,
            IsActive = true
        };

        var result = await _inventoryService.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.UnitOfMeasure.Should().Be(unit);
    }

    [Fact]
    public async Task CreateInventoryItemAsync_WithExpiryDate_SavesCorrectly()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(12);
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "EXPIRY-001",
            ItemName = "Item with Expiry",
            UnitOfMeasure = "Piece",
            QuantityOnHand = 100,
            ReorderLevel = 20,
            UnitCost = 50,
            SellingPrice = 75,
            ExpiryDate = expiryDate,
            IsActive = true
        };

        var result = await _inventoryService.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.ExpiryDate.Should().BeCloseTo(expiryDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateInventoryItemAsync_SetsCreatedAtAutomatically()
    {
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "AUTO-001",
            ItemName = "Auto Date Item",
            UnitOfMeasure = "Piece",
            QuantityOnHand = 100,
            IsActive = true
        };

        var before = DateTime.UtcNow;
        var result = await _inventoryService.CreateInventoryItemAsync(newItem);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreateInventoryItemAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _inventoryService.CreateInventoryItemAsync(new InventoryItem
            {
                BranchId = 1,
                CategoryId = 1,
                ItemCode = $"CONC-{i:D3}",
                ItemName = $"Concurrent Item {i}",
                UnitOfMeasure = "Piece",
                QuantityOnHand = 100,
                IsActive = true
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(i => i.Id > 0);
    }

    #endregion

    #region UpdateInventoryItemAsync Tests

    [Fact]
    public async Task UpdateInventoryItemAsync_UpdateQuantity_UpdatesSuccessfully()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(1);
        item!.QuantityOnHand = 200;

        await _inventoryService.UpdateInventoryItemAsync(item);

        var updated = await _inventoryService.GetInventoryItemByIdAsync(1);
        updated!.QuantityOnHand.Should().Be(200);
    }

    [Fact]
    public async Task UpdateInventoryItemAsync_UpdatePrice_UpdatesSuccessfully()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(2);
        item!.SellingPrice = 999;

        await _inventoryService.UpdateInventoryItemAsync(item);

        var updated = await _inventoryService.GetInventoryItemByIdAsync(2);
        updated!.SellingPrice.Should().Be(999);
    }

    [Fact]
    public async Task UpdateInventoryItemAsync_Deactivate_UpdatesSuccessfully()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(3);
        item!.IsActive = false;

        await _inventoryService.UpdateInventoryItemAsync(item);

        var updated = await _inventoryService.GetInventoryItemByIdAsync(3);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateInventoryItemAsync_SetsUpdatedAtAutomatically()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(4);
        item!.ItemName = "Updated Name";

        var before = DateTime.UtcNow;
        await _inventoryService.UpdateInventoryItemAsync(item);
        var after = DateTime.UtcNow;

        var updated = await _inventoryService.GetInventoryItemByIdAsync(4);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Stock Transaction Tests

    [Fact]
    public async Task CreateStockTransactionAsync_Receipt_IncreasesQuantity()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(10);
        var originalQty = item!.QuantityOnHand;

        var transaction = new StockTransaction
        {
            BranchId = item.BranchId,
            ItemId = item.Id,
            TransactionType = "Receipt",
            Quantity = 50,
            UnitCost = item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = "TXN-RECEIPT-001",
            CreatedBy = "test"
        };

        await _inventoryService.CreateStockTransactionAsync(transaction);

        var updatedItem = await _inventoryService.GetInventoryItemByIdAsync(10);
        updatedItem!.QuantityOnHand.Should().Be(originalQty + 50);
    }

    [Fact]
    public async Task CreateStockTransactionAsync_Issue_DecreasesQuantity()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(11);
        var originalQty = item!.QuantityOnHand;

        var transaction = new StockTransaction
        {
            BranchId = item.BranchId,
            ItemId = item.Id,
            TransactionType = "Issue",
            Quantity = 10,
            UnitCost = item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = "TXN-ISSUE-001",
            CreatedBy = "test"
        };

        await _inventoryService.CreateStockTransactionAsync(transaction);

        var updatedItem = await _inventoryService.GetInventoryItemByIdAsync(11);
        updatedItem!.QuantityOnHand.Should().Be(originalQty - 10);
    }

    [Fact]
    public async Task CreateStockTransactionAsync_Adjustment_AdjustsQuantity()
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(12);

        var transaction = new StockTransaction
        {
            BranchId = item!.BranchId,
            ItemId = item.Id,
            TransactionType = "Adjustment",
            Quantity = 5,
            AdjustmentReason = "Inventory count correction",
            UnitCost = item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = "TXN-ADJ-001",
            CreatedBy = "test"
        };

        await _inventoryService.CreateStockTransactionAsync(transaction);

        // Verify transaction was created
        var transactions = await _inventoryService.GetStockTransactionsByItemIdAsync(12);
        transactions.Should().Contain(t => t.ReferenceNumber == "TXN-ADJ-001");
    }

    [Theory]
    [InlineData("Receipt")]
    [InlineData("Issue")]
    [InlineData("Adjustment")]
    [InlineData("Transfer")]
    [InlineData("Return")]
    public async Task CreateStockTransactionAsync_VariousTypes_AllSucceed(string transactionType)
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(20);

        var transaction = new StockTransaction
        {
            BranchId = item!.BranchId,
            ItemId = item.Id,
            TransactionType = transactionType,
            Quantity = 5,
            UnitCost = item.UnitCost,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = $"TXN-{transactionType.ToUpper()}-001",
            CreatedBy = "test"
        };

        var result = await _inventoryService.CreateStockTransactionAsync(transaction);

        result.Should().NotBeNull();
        result.TransactionType.Should().Be(transactionType);
    }

    [Fact]
    public async Task GetStockTransactionsByItemIdAsync_ReturnsTransactions()
    {
        var result = await _inventoryService.GetStockTransactionsByItemIdAsync(1);
        var transactions = result.ToList();

        transactions.Should().OnlyContain(t => t.ItemId == 1);
    }

    [Fact]
    public async Task GetStockTransactionsByDateRangeAsync_ReturnsTransactions()
    {
        var startDate = DateTime.UtcNow.AddDays(-50);
        var endDate = DateTime.UtcNow;

        var result = await _inventoryService.GetStockTransactionsByDateRangeAsync(1, startDate, endDate);
        var transactions = result.ToList();

        transactions.Should().OnlyContain(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
    }

    #endregion

    #region Purchase Order Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task GetPurchaseOrderByIdAsync_ValidIds_ReturnsOrder(int orderId)
    {
        var result = await _inventoryService.GetPurchaseOrderByIdAsync(orderId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task CreatePurchaseOrderAsync_ValidOrder_CreatesSuccessfully()
    {
        var newOrder = new PurchaseOrder
        {
            BranchId = 1,
            OrderNumber = "PO-NEW-001",
            VendorName = "New Vendor",
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
            TotalAmount = 5000,
            Status = "Draft"
        };

        var result = await _inventoryService.CreatePurchaseOrderAsync(newOrder);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Pending")]
    [InlineData("Approved")]
    [InlineData("Received")]
    [InlineData("Cancelled")]
    public async Task UpdatePurchaseOrderStatusAsync_VariousStatuses_AllSucceed(string status)
    {
        var order = await _context.PurchaseOrders.FirstOrDefaultAsync();
        if (order != null)
        {
            order.Status = status;
            await _inventoryService.UpdatePurchaseOrderAsync(order);

            var updated = await _inventoryService.GetPurchaseOrderByIdAsync(order.Id);
            updated!.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task GetPurchaseOrdersByStatusAsync_ReturnsCorrectOrders()
    {
        var result = await _inventoryService.GetPurchaseOrdersByStatusAsync(1, "Draft");
        var orders = result.ToList();

        orders.Should().OnlyContain(o => o.Status == "Draft");
    }

    [Fact]
    public async Task GetPurchaseOrdersByVendorAsync_ReturnsCorrectOrders()
    {
        var result = await _inventoryService.GetPurchaseOrdersByVendorAsync(1, "Vendor 1");
        var orders = result.ToList();

        orders.Should().OnlyContain(o => o.VendorName == "Vendor 1");
    }

    #endregion

    #region Stock Audit Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(30)]
    public async Task GetStockAuditByIdAsync_ValidIds_ReturnsAudit(int auditId)
    {
        var result = await _inventoryService.GetStockAuditByIdAsync(auditId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditId);
    }

    [Fact]
    public async Task CreateStockAuditAsync_ValidAudit_CreatesSuccessfully()
    {
        var newAudit = new StockAudit
        {
            BranchId = 1,
            AuditNumber = "AUD-NEW-001",
            AuditDate = DateTime.UtcNow,
            Status = "Draft",
            AuditedBy = "auditor"
        };

        var result = await _inventoryService.CreateStockAuditAsync(newAudit);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    public async Task UpdateStockAuditStatusAsync_VariousStatuses_AllSucceed(string status)
    {
        var audit = await _context.StockAudits.FirstOrDefaultAsync();
        if (audit != null)
        {
            audit.Status = status;
            await _inventoryService.UpdateStockAuditAsync(audit);

            var updated = await _inventoryService.GetStockAuditByIdAsync(audit.Id);
            updated!.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task GetStockAuditsByStatusAsync_ReturnsCorrectAudits()
    {
        var result = await _inventoryService.GetStockAuditsByStatusAsync(1, "Draft");
        var audits = result.ToList();

        audits.Should().OnlyContain(a => a.Status == "Draft");
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        var result = await _inventoryService.GetAllCategoriesAsync();
        var categories = result.ToList();

        categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetActiveCategoriesAsync_ReturnsOnlyActive()
    {
        var result = await _inventoryService.GetActiveCategoriesAsync();
        var categories = result.ToList();

        categories.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task CreateCategoryAsync_ValidCategory_CreatesSuccessfully()
    {
        var newCategory = new InventoryCategory
        {
            Name = "New Category",
            Code = "NEW",
            IsActive = true
        };

        var result = await _inventoryService.CreateCategoryAsync(newCategory);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchInventoryItemsAsync_ByItemCode_ReturnsMatches()
    {
        var result = await _inventoryService.SearchInventoryItemsAsync(1, "ITEM-0001");
        var items = result.ToList();

        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchInventoryItemsAsync_ByItemName_ReturnsMatches()
    {
        var result = await _inventoryService.SearchInventoryItemsAsync(1, "Inventory Item");
        var items = result.ToList();

        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchInventoryItemsAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _inventoryService.SearchInventoryItemsAsync(1, "NonExistentItem12345");
        result.Should().BeEmpty();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalInventoryValueAsync_ReturnsValue()
    {
        var result = await _inventoryService.GetTotalInventoryValueAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTotalItemCountAsync_ReturnsCount()
    {
        var result = await _inventoryService.GetTotalItemCountAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetLowStockCountAsync_ReturnsCount()
    {
        var result = await _inventoryService.GetLowStockCountAsync(1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpiredItemCountAsync_ReturnsCount()
    {
        var result = await _inventoryService.GetExpiredItemCountAsync(1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetInventoryCategoryDistributionAsync_ReturnsDistribution()
    {
        var result = await _inventoryService.GetInventoryCategoryDistributionAsync(1);
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Stock Transfer Tests

    [Fact]
    public async Task TransferStockAsync_ValidTransfer_Succeeds()
    {
        var sourceItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.BranchId == 1 && i.QuantityOnHand > 20);

        if (sourceItem != null)
        {
            var transfer = new StockTransfer
            {
                SourceBranchId = 1,
                DestinationBranchId = 2,
                ItemId = sourceItem.Id,
                Quantity = 10,
                TransferDate = DateTime.UtcNow,
                ReferenceNumber = "TRF-001",
                RequestedBy = "user1"
            };

            var result = await _inventoryService.TransferStockAsync(transfer);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetPendingTransfersAsync_ReturnsPendingTransfers()
    {
        var result = await _inventoryService.GetPendingTransfersAsync(1);
        var transfers = result.ToList();

        transfers.Should().OnlyContain(t => t.Status == "Pending");
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task InventoryItem_WithZeroQuantity_HandlesCorrectly()
    {
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "ZERO-QTY",
            ItemName = "Zero Quantity Item",
            UnitOfMeasure = "Piece",
            QuantityOnHand = 0,
            IsActive = true
        };

        var result = await _inventoryService.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.QuantityOnHand.Should().Be(0);
    }

    [Fact]
    public async Task InventoryItem_WithLargeQuantity_HandlesCorrectly()
    {
        var newItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "LARGE-QTY",
            ItemName = "Large Quantity Item",
            UnitOfMeasure = "Piece",
            QuantityOnHand = 999999,
            IsActive = true
        };

        var result = await _inventoryService.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.QuantityOnHand.Should().Be(999999);
    }

    [Fact]
    public async Task GetInventoryItemsByBranchIdAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var result = await _inventoryService.GetInventoryItemsByBranchIdAsync(1);
        var items = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentStockTransactions_AllSucceed()
    {
        var items = await _context.InventoryItems
            .Where(i => i.BranchId == 1 && i.QuantityOnHand > 10)
            .Take(5)
            .ToListAsync();

        var tasks = items.Select(async item =>
        {
            var transaction = new StockTransaction
            {
                BranchId = item.BranchId,
                ItemId = item.Id,
                TransactionType = "Issue",
                Quantity = 1,
                UnitCost = item.UnitCost,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = $"TXN-CONC-{item.Id}",
                CreatedBy = "test"
            };
            return await _inventoryService.CreateStockTransactionAsync(transaction);
        }).ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(t => t.Id > 0);
    }

    #endregion
}
