using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Sales;

/// <summary>
/// Unit tests for SalesService - Sale CRUD operations
/// </summary>
public class SalesServiceSaleTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;

    public SalesServiceSaleTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"SalesTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();

        // Setup sequence generator to return predictable values
        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), "SALE", SequenceType.Sale, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}");

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), "QT", SequenceType.Quotation, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}");

        _service = new SalesService(_context, _sequenceGeneratorMock.Object);

        SetupTestData();
    }

    private void SetupTestData()
    {
        // Create test branch
        var branch = new Branch
        {
            Id = _testBranchId,
            Name = "Test Branch",
            Code = "TEST-BR",
            IsActive = true
        };
        _context.Branches.Add(branch);

        // Create test patient
        var patient = new Patient
        {
            Id = _testPatientId,
            FirstName = "Test",
            LastName = "Patient",
            BranchId = _testBranchId,
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Gender = "Male"
        };
        _context.Patients.Add(patient);

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Create Sale Tests

    [Fact]
    public async Task CreateSaleAsync_WithValidData_CreatesSale()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            Notes = "Test sale",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.InvoiceNumber.Should().StartWith("SALE-");
        result.Status.Should().Be(SaleStatus.Draft);
        result.PaymentStatus.Should().Be(PaymentStatus.Pending);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateSaleAsync_WithInvalidPatient_ThrowsException()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = 9999, // Non-existent patient
            BranchId = _testBranchId,
            CreatedBy = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateSaleAsync(sale));
    }

    [Fact]
    public async Task CreateSaleAsync_GeneratesInvoiceNumber_WhenNotProvided()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.InvoiceNumber.Should().NotBeNullOrEmpty();
        _sequenceGeneratorMock.Verify(
            s => s.GenerateSequenceAsync(_testBranchId, "SALE", SequenceType.Sale, It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_CalculatesTotals_WithItems()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            TaxPercentage = 5,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem
                {
                    ItemName = "Service 1",
                    Quantity = 2,
                    UnitPrice = 100,
                    Subtotal = 200,
                    Total = 200
                },
                new SaleItem
                {
                    ItemName = "Service 2",
                    Quantity = 1,
                    UnitPrice = 50,
                    Subtotal = 50,
                    Total = 50
                }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.SubTotal.Should().Be(250);
        result.Total.Should().BeGreaterThan(0);
    }

    #endregion

    #region Read Sale Tests

    [Fact]
    public async Task GetSaleByIdAsync_WithExistingSale_ReturnsSale()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        var result = await _service.GetSaleByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSaleByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _service.GetSaleByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSaleByInvoiceNumberAsync_ReturnsCorrectSale()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        var result = await _service.GetSaleByInvoiceNumberAsync(sale.InvoiceNumber);

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(sale.InvoiceNumber);
    }

    [Fact]
    public async Task GetSalesByBranchIdAsync_ReturnsOnlyBranchSales()
    {
        // Arrange
        await CreateTestSale();
        await CreateTestSale();

        // Act
        var result = await _service.GetSalesByBranchIdAsync(_testBranchId);

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.BranchId == _testBranchId).Should().BeTrue();
    }

    [Fact]
    public async Task GetSalesByPatientIdAsync_ReturnsOnlyPatientSales()
    {
        // Arrange
        await CreateTestSale();
        await CreateTestSale();

        // Act
        var result = await _service.GetSalesByPatientIdAsync(_testPatientId);

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.PatientId == _testPatientId).Should().BeTrue();
    }

    [Fact]
    public async Task GetSalesByStatusAsync_FiltersCorrectly()
    {
        // Arrange
        var draftSale = await CreateTestSale();
        var confirmedSale = await CreateTestSale();
        confirmedSale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act
        var draftSales = await _service.GetSalesByStatusAsync(_testBranchId, SaleStatus.Draft);
        var confirmedSales = await _service.GetSalesByStatusAsync(_testBranchId, SaleStatus.Confirmed);

        // Assert
        draftSales.Should().HaveCount(1);
        confirmedSales.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSalesByDateRangeAsync_FiltersCorrectly()
    {
        // Arrange
        var sale1 = await CreateTestSale();
        sale1.SaleDate = DateTime.UtcNow.AddDays(-5);

        var sale2 = await CreateTestSale();
        sale2.SaleDate = DateTime.UtcNow.AddDays(-2);

        await _context.SaveChangesAsync();

        var startDate = DateTime.UtcNow.AddDays(-3);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _service.GetSalesByDateRangeAsync(_testBranchId, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOverdueSalesAsync_ReturnsOnlyOverdue()
    {
        // Arrange
        var overdueSale = await CreateTestSale();
        overdueSale.DueDate = DateTime.UtcNow.AddDays(-5);
        overdueSale.Total = 100;
        overdueSale.PaidAmount = 0;

        var currentSale = await CreateTestSale();
        currentSale.DueDate = DateTime.UtcNow.AddDays(5);
        currentSale.Total = 100;
        currentSale.PaidAmount = 0;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOverdueSalesAsync(_testBranchId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(overdueSale.Id);
    }

    #endregion

    #region Update Sale Tests

    [Fact]
    public async Task UpdateSaleAsync_WithDraftSale_UpdatesSuccessfully()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Notes = "Updated notes";

        // Act
        await _service.UpdateSaleAsync(sale);

        // Assert
        var updated = await _context.Sales.FindAsync(sale.Id);
        updated!.Notes.Should().Be("Updated notes");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSaleAsync_WithCompletedSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Completed;
        await _context.SaveChangesAsync();

        sale.Notes = "Try to update";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateSaleAsync(sale));
    }

    [Fact]
    public async Task UpdateSaleAsync_WithCancelledSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Cancelled;
        await _context.SaveChangesAsync();

        sale.Notes = "Try to update";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateSaleAsync(sale));
    }

    #endregion

    #region Delete Sale Tests

    [Fact]
    public async Task DeleteSaleAsync_WithDraftSale_DeletesSuccessfully()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        await _service.DeleteSaleAsync(sale.Id);

        // Assert
        var deleted = await _context.Sales.FindAsync(sale.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSaleAsync_WithConfirmedSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteSaleAsync(sale.Id));
    }

    [Fact]
    public async Task DeleteSaleAsync_WithPayments_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Payments.Add(new Payment
        {
            PaymentNumber = "PAY-001",
            Amount = 50,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedBy = "test-user",
            PaymentDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteSaleAsync(sale.Id));
    }

    #endregion

    #region Sale Status Management Tests

    [Fact]
    public async Task ConfirmSaleAsync_WithDraftSaleWithItems_ConfirmsSuccessfully()
    {
        // Arrange
        var sale = await CreateTestSaleWithItems();

        // Act
        var result = await _service.ConfirmSaleAsync(sale.Id);

        // Assert
        result.Status.Should().Be(SaleStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmSaleAsync_WithDraftSaleWithoutItems_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfirmSaleAsync(sale.Id));
    }

    [Fact]
    public async Task ConfirmSaleAsync_WithConfirmedSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithItems();
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfirmSaleAsync(sale.Id));
    }

    [Fact]
    public async Task CompleteSaleAsync_WithFullyPaidSale_CompletesSuccessfully()
    {
        // Arrange
        var sale = await CreateTestSaleWithItems();
        sale.Status = SaleStatus.Confirmed;
        sale.Total = 100;
        sale.PaidAmount = 100;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CompleteSaleAsync(sale.Id);

        // Assert
        result.Status.Should().Be(SaleStatus.Completed);
        result.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task CompleteSaleAsync_WithUnpaidSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithItems();
        sale.Status = SaleStatus.Confirmed;
        sale.Total = 100;
        sale.PaidAmount = 50;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CompleteSaleAsync(sale.Id));
    }

    [Fact]
    public async Task CancelSaleAsync_WithDraftSale_CancelsSuccessfully()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        var result = await _service.CancelSaleAsync(sale.Id, "Test cancellation");

        // Assert
        result.Status.Should().Be(SaleStatus.Cancelled);
        result.PaymentStatus.Should().Be(PaymentStatus.Cancelled);
        result.Notes.Should().Contain("Test cancellation");
    }

    [Fact]
    public async Task CancelSaleAsync_WithPaidSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.PaidAmount = 50;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CancelSaleAsync(sale.Id));
    }

    [Fact]
    public async Task CancelSaleAsync_WithCompletedSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Completed;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CancelSaleAsync(sale.Id));
    }

    #endregion

    #region Helper Methods

    private async Task<Sale> CreateTestSale()
    {
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user"
        };
        return await _service.CreateSaleAsync(sale);
    }

    private async Task<Sale> CreateTestSaleWithItems()
    {
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem
                {
                    ItemName = "Test Service",
                    Quantity = 1,
                    UnitPrice = 100,
                    Subtotal = 100,
                    Total = 100,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        return await _service.CreateSaleAsync(sale);
    }

    #endregion
}
