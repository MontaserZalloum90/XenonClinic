using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Tests.Inventory;

/// <summary>
/// Unit tests for InventoryService covering all inventory operations.
/// </summary>
public class InventoryServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly InventoryService _service;
    private readonly Branch _testBranch;
    private readonly InventoryCategory _testCategory;

    public InventoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _service = new InventoryService(_context);

        // Seed test data
        _testBranch = new Branch
        {
            Id = 1,
            Name = "Test Branch",
            Code = "TB01",
            CompanyId = 1,
            IsActive = true
        };
        _context.Branches.Add(_testBranch);

        _testCategory = new InventoryCategory
        {
            Id = 1,
            Name = "Test Category",
            BranchId = 1,
            IsActive = true
        };
        _context.InventoryCategories.Add(_testCategory);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Inventory Item Tests

    [Fact]
    public async Task GetInventoryItemByIdAsync_ExistingItem_ReturnsItemWithIncludes()
    {
        // Arrange
        var item = await CreateTestInventoryItem();

        // Act
        var result = await _service.GetInventoryItemByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Branch.Should().NotBeNull();
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryItemByIdAsync_NonExistentItem_ReturnsNull()
    {
        // Act
        var result = await _service.GetInventoryItemByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInventoryItemByCodeAsync_ExistingCode_ReturnsItem()
    {
        // Arrange
        var item = await CreateTestInventoryItem("ITEM-001");

        // Act
        var result = await _service.GetInventoryItemByCodeAsync("ITEM-001", 1);

        // Assert
        result.Should().NotBeNull();
        result!.ItemCode.Should().Be("ITEM-001");
    }

    [Fact]
    public async Task GetInventoryItemByCodeAsync_NonExistentCode_ReturnsNull()
    {
        // Act
        var result = await _service.GetInventoryItemByCodeAsync("NONEXISTENT", 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInventoryItemsByBranchIdAsync_ReturnsFilteredItems()
    {
        // Arrange
        await CreateTestInventoryItem("ITEM-001");
        await CreateTestInventoryItem("ITEM-002");

        // Act
        var result = await _service.GetInventoryItemsByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveInventoryItemsAsync_ReturnsOnlyActiveItems()
    {
        // Arrange
        var activeItem = await CreateTestInventoryItem("ACTIVE-001");
        var inactiveItem = await CreateTestInventoryItem("INACTIVE-001");
        inactiveItem.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetActiveInventoryItemsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().ItemCode.Should().Be("ACTIVE-001");
    }

    [Fact]
    public async Task GetLowStockItemsAsync_ReturnsLowStockItems()
    {
        // Arrange
        var lowStockItem = await CreateTestInventoryItem("LOW-001");
        lowStockItem.CurrentStock = 5;
        lowStockItem.ReorderLevel = 10;

        var normalStockItem = await CreateTestInventoryItem("NORMAL-001");
        normalStockItem.CurrentStock = 100;
        normalStockItem.ReorderLevel = 10;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLowStockItemsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().ItemCode.Should().Be("LOW-001");
    }

    [Fact]
    public async Task CreateInventoryItemAsync_ValidItem_CreatesItem()
    {
        // Arrange
        var item = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "NEW-001",
            ItemName = "New Item",
            CurrentStock = 100,
            UnitPrice = 10,
            IsActive = true
        };

        // Act
        var result = await _service.CreateInventoryItemAsync(item);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateInventoryItemAsync_DuplicateCode_ThrowsException()
    {
        // Arrange
        await CreateTestInventoryItem("DUPLICATE-001");

        var duplicateItem = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "DUPLICATE-001",
            ItemName = "Duplicate Item",
            CurrentStock = 100,
            UnitPrice = 10,
            IsActive = true
        };

        // Act & Assert
        var act = () => _service.CreateInventoryItemAsync(duplicateItem);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateInventoryItemAsync_NegativeStock_ThrowsException()
    {
        // Arrange
        var item = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = "NEG-001",
            ItemName = "Negative Stock Item",
            CurrentStock = -10,
            UnitPrice = 10,
            IsActive = true
        };

        // Act & Assert
        var act = () => _service.CreateInventoryItemAsync(item);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Initial stock cannot be negative*");
    }

    [Fact]
    public async Task DeleteInventoryItemAsync_ExistingItem_DeletesItem()
    {
        // Arrange
        var item = await CreateTestInventoryItem();

        // Act
        await _service.DeleteInventoryItemAsync(item.Id);
        var result = await _service.GetInventoryItemByIdAsync(item.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Stock Adjustment Tests

    [Fact]
    public async Task AdjustStockAsync_AddStock_IncreasesStock()
    {
        // Arrange
        var item = await CreateTestInventoryItem();
        item.CurrentStock = 100;
        await _context.SaveChangesAsync();

        // Act
        await _service.AdjustStockAsync(
            item.Id,
            50,
            StockAdjustmentType.Add,
            "Test addition",
            "TestUser");

        // Assert
        var updatedItem = await _service.GetInventoryItemByIdAsync(item.Id);
        updatedItem!.CurrentStock.Should().Be(150);
    }

    [Fact]
    public async Task AdjustStockAsync_RemoveStock_DecreasesStock()
    {
        // Arrange
        var item = await CreateTestInventoryItem();
        item.CurrentStock = 100;
        await _context.SaveChangesAsync();

        // Act
        await _service.AdjustStockAsync(
            item.Id,
            30,
            StockAdjustmentType.Remove,
            "Test removal",
            "TestUser");

        // Assert
        var updatedItem = await _service.GetInventoryItemByIdAsync(item.Id);
        updatedItem!.CurrentStock.Should().Be(70);
    }

    [Fact]
    public async Task AdjustStockAsync_RemoveMoreThanAvailable_ThrowsException()
    {
        // Arrange
        var item = await CreateTestInventoryItem();
        item.CurrentStock = 50;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.AdjustStockAsync(
            item.Id,
            100,
            StockAdjustmentType.Remove,
            "Test removal",
            "TestUser");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task AdjustStockAsync_SetStock_SetsExactQuantity()
    {
        // Arrange
        var item = await CreateTestInventoryItem();
        item.CurrentStock = 100;
        await _context.SaveChangesAsync();

        // Act
        await _service.AdjustStockAsync(
            item.Id,
            75,
            StockAdjustmentType.Set,
            "Test set",
            "TestUser");

        // Assert
        var updatedItem = await _service.GetInventoryItemByIdAsync(item.Id);
        updatedItem!.CurrentStock.Should().Be(75);
    }

    [Fact]
    public async Task AdjustStockAsync_CreatesTransactionLog()
    {
        // Arrange
        var item = await CreateTestInventoryItem();
        item.CurrentStock = 100;
        await _context.SaveChangesAsync();

        // Act
        await _service.AdjustStockAsync(
            item.Id,
            50,
            StockAdjustmentType.Add,
            "Test addition",
            "TestUser");

        // Assert
        var transactions = await _service.GetInventoryTransactionsByItemIdAsync(item.Id);
        transactions.Should().HaveCount(1);
        transactions.First().Quantity.Should().Be(50);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalInventoryValueAsync_ReturnsCorrectValue()
    {
        // Arrange
        var item1 = await CreateTestInventoryItem("ITEM-001");
        item1.CurrentStock = 10;
        item1.UnitPrice = 100;

        var item2 = await CreateTestInventoryItem("ITEM-002");
        item2.CurrentStock = 20;
        item2.UnitPrice = 50;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalInventoryValueAsync(1);

        // Assert
        result.Should().Be(2000); // (10 * 100) + (20 * 50)
    }

    [Fact]
    public async Task GetLowStockItemsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var lowStock1 = await CreateTestInventoryItem("LOW-001");
        lowStock1.CurrentStock = 5;
        lowStock1.ReorderLevel = 10;

        var lowStock2 = await CreateTestInventoryItem("LOW-002");
        lowStock2.CurrentStock = 3;
        lowStock2.ReorderLevel = 10;

        var normalStock = await CreateTestInventoryItem("NORMAL-001");
        normalStock.CurrentStock = 100;
        normalStock.ReorderLevel = 10;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLowStockItemsCountAsync(1);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetExpiringItemsAsync_ReturnsItemsExpiringSoon()
    {
        // Arrange
        var expiringItem = await CreateTestInventoryItem("EXPIRING-001");
        expiringItem.ExpirationDate = DateTime.UtcNow.AddDays(10);

        var notExpiringItem = await CreateTestInventoryItem("NOTEXPIRING-001");
        notExpiringItem.ExpirationDate = DateTime.UtcNow.AddDays(100);

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetExpiringItemsAsync(1, 30);

        // Assert
        result.Should().HaveCount(1);
        result.First().ItemCode.Should().Be("EXPIRING-001");
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetCategoriesByBranchIdAsync_ReturnsCategories()
    {
        // Act
        var result = await _service.GetCategoriesByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateCategoryAsync_ValidCategory_CreatesCategory()
    {
        // Arrange
        var category = new InventoryCategory
        {
            BranchId = 1,
            Name = "New Category",
            IsActive = true
        };

        // Act
        var result = await _service.CreateCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    private async Task<InventoryItem> CreateTestInventoryItem(string? itemCode = null)
    {
        var item = new InventoryItem
        {
            BranchId = 1,
            CategoryId = 1,
            ItemCode = itemCode ?? $"ITEM-{Guid.NewGuid().ToString()[..4]}",
            ItemName = "Test Item",
            CurrentStock = 100,
            UnitPrice = 10,
            ReorderLevel = 10,
            IsActive = true
        };

        return await _service.CreateInventoryItemAsync(item);
    }

    #endregion
}
