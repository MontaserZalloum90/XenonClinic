using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;
using PatientEntity = XenonClinic.Core.Entities.Patient;

namespace XenonClinic.Tests.Sales;

/// <summary>
/// Unit tests for SalesService - Payment processing operations
/// </summary>
public class SalesServicePaymentTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;

    public SalesServicePaymentTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"PaymentTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), "SALE", SequenceType.Sale, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}");

        _service = new SalesService(_context, _sequenceGeneratorMock.Object);

        SetupTestData();
    }

    private void SetupTestData()
    {
        var branch = new Branch
        {
            Id = _testBranchId,
            Name = "Test Branch",
            Code = "TEST-BR",
            IsActive = true
        };
        _context.Branches.Add(branch);

        var patient = new PatientEntity
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

    #region Record Payment Tests

    [Fact]
    public async Task RecordPaymentAsync_WithValidData_CreatesPayment()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = 200,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedBy = "test-user"
        };

        // Act
        var result = await _service.RecordPaymentAsync(payment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.PaymentNumber.Should().StartWith("PAY-");
        result.Amount.Should().Be(200);
    }

    [Fact]
    public async Task RecordPaymentAsync_UpdatesSalePaidAmount()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(200);
    }

    [Fact]
    public async Task RecordPaymentAsync_UpdatesPaymentStatus_ToPartial()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaymentStatus.Should().Be(PaymentStatus.Partial);
    }

    [Fact]
    public async Task RecordPaymentAsync_UpdatesPaymentStatus_ToPaid()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task RecordPaymentAsync_WithMultiplePayments_CalculatesTotalCorrectly()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 150, PaymentMethod.Card);
        await _service.RecordPaymentAsync(sale.Id, 150, PaymentMethod.BankTransfer);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(500);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task RecordPaymentAsync_WithCashMethod_SetsCorrectMethod()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Assert
        payment.PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public async Task RecordPaymentAsync_WithCardMethod_SetsCorrectMethod()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Card);

        // Assert
        payment.PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public async Task RecordPaymentAsync_WithReferenceNumber_StoresReference()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.BankTransfer, "TXN-123456");

        // Assert
        payment.ReferenceNumber.Should().Be("TXN-123456");
    }

    #endregion

    #region Payment Validation Tests

    [Fact]
    public async Task RecordPaymentAsync_WithZeroAmount_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = 0,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedBy = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordPaymentAsync(payment));
    }

    [Fact]
    public async Task RecordPaymentAsync_WithNegativeAmount_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = -100,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedBy = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordPaymentAsync(payment));
    }

    [Fact]
    public async Task RecordPaymentAsync_ExceedingBalance_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordPaymentAsync(sale.Id, 600, PaymentMethod.Cash));
    }

    [Fact]
    public async Task RecordPaymentAsync_ForCancelledSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        sale.Status = SaleStatus.Cancelled;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash));
    }

    [Fact]
    public async Task RecordPaymentAsync_ForNonExistentSale_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RecordPaymentAsync(9999, 200, PaymentMethod.Cash));
    }

    #endregion

    #region Get Payment Tests

    [Fact]
    public async Task GetPaymentByIdAsync_ReturnsPayment()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        var result = await _service.GetPaymentByIdAsync(payment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task GetPaymentByNumberAsync_ReturnsPayment()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        var result = await _service.GetPaymentByNumberAsync(payment.PaymentNumber);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentNumber.Should().Be(payment.PaymentNumber);
    }

    [Fact]
    public async Task GetPaymentsBySaleIdAsync_ReturnsAllPayments()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale.Id, 100, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 150, PaymentMethod.Card);

        // Act
        var result = await _service.GetPaymentsBySaleIdAsync(sale.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_FiltersCorrectly()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _service.GetPaymentsByDateRangeAsync(_testBranchId, startDate, endDate);

        // Assert
        result.Should().Contain(p => p.Id == payment.Id);
    }

    #endregion

    #region Update Payment Tests

    [Fact]
    public async Task UpdatePaymentAsync_WithNewAmount_RecalculatesSalePaidAmount()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Change payment amount
        payment.Amount = 300;

        // Act
        await _service.UpdatePaymentAsync(payment);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(300);
    }

    #endregion

    #region Delete Payment Tests

    [Fact]
    public async Task DeletePaymentAsync_UpdatesSalePaidAmount()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        await _service.DeletePaymentAsync(payment.Id);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(0);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task DeletePaymentAsync_RemovesPayment()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        await _service.DeletePaymentAsync(payment.Id);

        // Assert
        var deletedPayment = await _context.Payments.FindAsync(payment.Id);
        deletedPayment.Should().BeNull();
    }

    #endregion

    #region Refund Payment Tests

    [Fact]
    public async Task RefundPaymentAsync_CreatesNegativePayment()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        await _service.RefundPaymentAsync(payment.Id, 100, "Customer request");

        // Assert
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        payments.Should().Contain(p => p.Amount == -100);
    }

    [Fact]
    public async Task RefundPaymentAsync_UpdatesSalePaidAmount()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        var payment = (await _service.GetPaymentsBySaleIdAsync(sale.Id)).First();

        // Act
        await _service.RefundPaymentAsync(payment.Id, 100, "Customer request");

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(100); // 200 - 100 refund
    }

    [Fact]
    public async Task RefundPaymentAsync_ExceedingOriginalAmount_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RefundPaymentAsync(payment.Id, 300, "Too much"));
    }

    [Fact]
    public async Task RefundPaymentAsync_FullRefund_UpdatesSaleStatusToRefunded()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Cash);

        // Act
        await _service.RefundPaymentAsync(payment.Id, 500, "Full refund");

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        updatedSale.Status.Should().Be(SaleStatus.Refunded);
    }

    [Fact]
    public async Task RefundPaymentAsync_IncludesReasonInNotes()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        var payment = await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);

        // Act
        await _service.RefundPaymentAsync(payment.Id, 100, "Customer request");

        // Assert
        var refundPayment = (await _service.GetPaymentsBySaleIdAsync(sale.Id))
            .FirstOrDefault(p => p.Amount < 0);

        refundPayment!.Notes.Should().Contain("Customer request");
        refundPayment.Notes.Should().Contain("Refund for payment");
    }

    #endregion

    #region Payment Method Distribution Tests

    [Fact]
    public async Task GetSalesStatisticsAsync_CalculatesPaymentMethodDistribution()
    {
        // Arrange
        var sale = await CreateTestSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 150, PaymentMethod.Card);
        await _service.RecordPaymentAsync(sale.Id, 150, PaymentMethod.BankTransfer);

        // Act
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);

        // Assert
        stats.PaymentMethodDistribution.Should().ContainKey(PaymentMethod.Cash);
        stats.PaymentMethodDistribution.Should().ContainKey(PaymentMethod.Card);
        stats.PaymentMethodDistribution.Should().ContainKey(PaymentMethod.BankTransfer);
        stats.PaymentMethodDistribution[PaymentMethod.Cash].Should().Be(200);
        stats.PaymentMethodDistribution[PaymentMethod.Card].Should().Be(150);
        stats.PaymentMethodDistribution[PaymentMethod.BankTransfer].Should().Be(150);
    }

    #endregion

    #region Helper Methods

    private async Task<Sale> CreateTestSaleWithTotal(decimal total)
    {
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            SubTotal = total,
            Total = total,
            PaidAmount = 0,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem
                {
                    ItemName = "Test Service",
                    Quantity = 1,
                    UnitPrice = total,
                    Subtotal = total,
                    Total = total,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        return await _service.CreateSaleAsync(sale);
    }

    #endregion
}
