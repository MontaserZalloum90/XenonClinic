using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Tests.Sales;

/// <summary>
/// Unit tests for SalesService covering all sale, quotation, and payment operations.
/// </summary>
public class SalesServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly Branch _testBranch;
    private readonly Patient _testPatient;

    public SalesServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();
        _service = new SalesService(_context, _sequenceGeneratorMock.Object);

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

        _testPatient = new Patient
        {
            Id = 1,
            FullNameEn = "Test Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            BranchId = 1
        };
        _context.Patients.Add(_testPatient);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Sale Management Tests

    [Fact]
    public async Task GetSaleByIdAsync_ExistingSale_ReturnsSaleWithIncludes()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        var result = await _service.GetSaleByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.Patient.Should().NotBeNull();
        result.Branch.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSaleByIdAsync_NonExistentSale_ReturnsNull()
    {
        // Act
        var result = await _service.GetSaleByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSalesByBranchIdAsync_ReturnsFilteredSales()
    {
        // Arrange
        await CreateTestSale();
        await CreateTestSale();

        // Act
        var result = await _service.GetSalesByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSalesByPatientIdAsync_ReturnsFilteredSales()
    {
        // Arrange
        await CreateTestSale();

        // Act
        var result = await _service.GetSalesByPatientIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSalesByStatusAsync_ReturnsFilteredSales()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSalesByStatusAsync(1, SaleStatus.Confirmed);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSalesByDateRangeAsync_ReturnsFilteredSales()
    {
        // Arrange
        await CreateTestSale();

        // Act
        var result = await _service.GetSalesByDateRangeAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOverdueSalesAsync_ReturnsOverdueSales()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.DueDate = DateTime.UtcNow.AddDays(-1);
        sale.Total = 100;
        sale.PaidAmount = 50;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOverdueSalesAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSaleAsync_ValidSale_CreatesSaleWithInvoiceNumber()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("SALE-20241210-0001");

        var sale = new Sale
        {
            BranchId = 1,
            PatientId = 1,
            SaleDate = DateTime.UtcNow,
            SubTotal = 100,
            Total = 100
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceNumber.Should().Be("SALE-20241210-0001");
        result.Status.Should().Be(SaleStatus.Draft);
    }

    [Fact]
    public async Task CreateSaleAsync_PatientNotFound_ThrowsException()
    {
        // Arrange
        var sale = new Sale
        {
            BranchId = 1,
            PatientId = 9999,
            SaleDate = DateTime.UtcNow
        };

        // Act & Assert
        var act = () => _service.CreateSaleAsync(sale);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Patient with ID 9999 not found*");
    }

    [Fact]
    public async Task UpdateSaleAsync_ExistingSale_UpdatesSale()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Notes = "Updated notes";

        // Act
        await _service.UpdateSaleAsync(sale);
        var result = await _service.GetSaleByIdAsync(sale.Id);

        // Assert
        result!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateSaleAsync_CompletedSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Completed;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.UpdateSaleAsync(sale);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot update a completed sale*");
    }

    [Fact]
    public async Task DeleteSaleAsync_DraftSale_DeletesSale()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        await _service.DeleteSaleAsync(sale.Id);
        var result = await _service.GetSaleByIdAsync(sale.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSaleAsync_SaleWithPayments_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = 50,
            PaymentMethod = PaymentMethod.Cash,
            PaymentNumber = "PAY-001"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.DeleteSaleAsync(sale.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot delete a sale with payments*");
    }

    [Fact]
    public async Task ConfirmSaleAsync_DraftSaleWithItems_ConfirmsSale()
    {
        // Arrange
        var sale = await CreateTestSale();
        _context.SaleItems.Add(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Test Item",
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
            Total = 100
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmSaleAsync(sale.Id);

        // Assert
        result.Status.Should().Be(SaleStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmSaleAsync_SaleWithoutItems_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act & Assert
        var act = () => _service.ConfirmSaleAsync(sale.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot confirm a sale without items*");
    }

    [Fact]
    public async Task CompleteSaleAsync_FullyPaidSale_CompletesSale()
    {
        // Arrange
        var sale = await CreateTestSale();
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
    public async Task CompleteSaleAsync_NotFullyPaidSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.PaidAmount = 50;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.CompleteSaleAsync(sale.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot complete a sale that is not fully paid*");
    }

    [Fact]
    public async Task CancelSaleAsync_DraftSale_CancelsSale()
    {
        // Arrange
        var sale = await CreateTestSale();

        // Act
        var result = await _service.CancelSaleAsync(sale.Id, "Test cancellation");

        // Assert
        result.Status.Should().Be(SaleStatus.Cancelled);
        result.Notes.Should().Contain("Test cancellation");
    }

    [Fact]
    public async Task CancelSaleAsync_SaleWithPayments_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.PaidAmount = 50;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.CancelSaleAsync(sale.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot cancel a sale with payments*");
    }

    #endregion

    #region Payment Tests

    [Fact]
    public async Task RecordPaymentAsync_ValidPayment_RecordsPayment()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("PAY-20241210-0001");

        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RecordPaymentAsync(sale.Id, 50, PaymentMethod.Cash);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(50);

        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(50);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Partial);
    }

    [Fact]
    public async Task RecordPaymentAsync_FullPayment_UpdatesStatusToPaid()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("PAY-20241210-0001");

        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act
        await _service.RecordPaymentAsync(sale.Id, 100, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        updatedSale!.PaidAmount.Should().Be(100);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task RecordPaymentAsync_ExceedsBalance_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.PaidAmount = 80;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.RecordPaymentAsync(sale.Id, 50, PaymentMethod.Cash);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds remaining balance*");
    }

    [Fact]
    public async Task RecordPaymentAsync_CancelledSale_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Status = SaleStatus.Cancelled;
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.RecordPaymentAsync(sale.Id, 50, PaymentMethod.Cash);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot record payment for a cancelled sale*");
    }

    [Fact]
    public async Task RefundPaymentAsync_ValidRefund_CreatesNegativePayment()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("PAY-20241210-0001");

        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.PaidAmount = 100;
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = 100,
            PaymentMethod = PaymentMethod.Cash,
            PaymentNumber = "PAY-001"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        await _service.RefundPaymentAsync(payment.Id, 50, "Test refund");

        // Assert
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        payments.Should().HaveCount(2);
        payments.Should().Contain(p => p.Amount == -50);
    }

    [Fact]
    public async Task RefundPaymentAsync_ExceedsOriginalAmount_ThrowsException()
    {
        // Arrange
        var sale = await CreateTestSale();
        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = 50,
            PaymentMethod = PaymentMethod.Cash,
            PaymentNumber = "PAY-001"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.RefundPaymentAsync(payment.Id, 100, "Test refund");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Refund amount cannot exceed original payment amount*");
    }

    #endregion

    #region Quotation Tests

    [Fact]
    public async Task CreateQuotationAsync_ValidQuotation_CreatesQuotationWithNumber()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("QT-20241210-0001");

        var quotation = new Quotation
        {
            BranchId = 1,
            PatientId = 1,
            QuotationDate = DateTime.UtcNow,
            ValidityDays = 30,
            SubTotal = 100,
            Total = 100
        };

        // Act
        var result = await _service.CreateQuotationAsync(quotation);

        // Assert
        result.Should().NotBeNull();
        result.QuotationNumber.Should().Be("QT-20241210-0001");
        result.Status.Should().Be(QuotationStatus.Draft);
        result.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SendQuotationAsync_DraftQuotationWithItems_SendsQuotation()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        _context.QuotationItems.Add(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Item",
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
            Total = 100
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SendQuotationAsync(quotation.Id);

        // Assert
        result.Status.Should().Be(QuotationStatus.Sent);
    }

    [Fact]
    public async Task AcceptQuotationAsync_SentQuotation_AcceptsQuotation()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        quotation.Status = QuotationStatus.Sent;
        quotation.ExpiryDate = DateTime.UtcNow.AddDays(30);
        _context.QuotationItems.Add(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Item",
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
            Total = 100
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptQuotationAsync(quotation.Id);

        // Assert
        result.Status.Should().Be(QuotationStatus.Accepted);
        result.AcceptedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AcceptQuotationAsync_ExpiredQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        quotation.Status = QuotationStatus.Sent;
        quotation.ExpiryDate = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = () => _service.AcceptQuotationAsync(quotation.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot accept an expired quotation*");
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_AcceptedQuotation_CreatesSale()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("SALE-20241210-0001");

        var quotation = await CreateTestQuotation();
        quotation.Status = QuotationStatus.Accepted;
        _context.QuotationItems.Add(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Item",
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
            Total = 100
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Assert
        result.Should().NotBeNull();
        result.QuotationId.Should().Be(quotation.Id);
        result.PatientId.Should().Be(quotation.PatientId);
        result.Status.Should().Be(SaleStatus.Draft);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_NotAccepted_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act & Assert
        var act = () => _service.ConvertQuotationToSaleAsync(quotation.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only accepted quotations can be converted to sales*");
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalSalesAsync_ReturnsTotalForDateRange()
    {
        // Arrange
        var sale1 = await CreateTestSale();
        sale1.Total = 100;
        var sale2 = await CreateTestSale();
        sale2.Total = 200;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalSalesAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().Be(300);
    }

    [Fact]
    public async Task GetTotalOutstandingAsync_ReturnsUnpaidBalance()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.PaidAmount = 30;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalOutstandingAsync(1);

        // Assert
        result.Should().Be(70);
    }

    [Fact]
    public async Task GetSalesStatisticsAsync_ReturnsComprehensiveStatistics()
    {
        // Arrange
        var sale = await CreateTestSale();
        sale.Total = 100;
        sale.PaidAmount = 50;
        sale.Status = SaleStatus.Confirmed;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSalesStatisticsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TotalSales.Should().Be(100);
        result.TotalPaid.Should().Be(50);
        result.TotalOutstanding.Should().Be(50);
        result.SalesCount.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    private async Task<Sale> CreateTestSale()
    {
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync($"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}");

        var sale = new Sale
        {
            BranchId = 1,
            PatientId = 1,
            SaleDate = DateTime.UtcNow,
            SubTotal = 0,
            Total = 0
        };

        return await _service.CreateSaleAsync(sale);
    }

    private async Task<Quotation> CreateTestQuotation()
    {
        _sequenceGeneratorMock
            .Setup(x => x.GenerateSequenceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync($"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}");

        var quotation = new Quotation
        {
            BranchId = 1,
            PatientId = 1,
            QuotationDate = DateTime.UtcNow,
            ValidityDays = 30,
            SubTotal = 100,
            Total = 100
        };

        return await _service.CreateQuotationAsync(quotation);
    }

    #endregion
}
