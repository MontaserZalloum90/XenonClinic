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
/// Tests for sales business logic - calculations, validations, and complex scenarios
/// </summary>
public class SalesBusinessLogicTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;

    public SalesBusinessLogicTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"BusinessLogicTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"NUM-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");

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

    #region Discount Calculation Tests

    [Theory]
    [InlineData(1000, 10, 900)] // 10% discount
    [InlineData(1000, 25, 750)] // 25% discount
    [InlineData(1000, 0, 1000)] // No discount
    [InlineData(500, 50, 250)]  // 50% discount
    public async Task CalculateSaleTotals_AppliesPercentageDiscount_Correctly(
        decimal subtotal, decimal discountPercentage, decimal expectedAfterDiscount)
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = subtotal,
            DiscountPercentage = discountPercentage,
            TaxPercentage = 0, // No tax for this test
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = subtotal, Subtotal = subtotal, Total = subtotal }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Total.Should().Be(expectedAfterDiscount);
    }

    [Theory]
    [InlineData(1000, 100, 900)]  // Fixed discount
    [InlineData(1000, 250, 750)]  // Larger discount
    [InlineData(500, 50, 450)]    // Small discount
    public async Task CalculateSaleTotals_AppliesFixedDiscount_Correctly(
        decimal subtotal, decimal fixedDiscount, decimal expectedAfterDiscount)
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = subtotal,
            DiscountAmount = fixedDiscount,
            DiscountPercentage = null, // Fixed amount takes precedence when percentage is null
            TaxPercentage = 0,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = subtotal, Subtotal = subtotal, Total = subtotal }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Total.Should().Be(expectedAfterDiscount);
    }

    #endregion

    #region Tax Calculation Tests

    [Theory]
    [InlineData(1000, 5, 1050)]   // 5% VAT
    [InlineData(1000, 10, 1100)]  // 10% tax
    [InlineData(1000, 0, 1000)]   // No tax
    [InlineData(500, 15, 575)]    // 15% tax
    public async Task CalculateSaleTotals_AppliesTax_Correctly(
        decimal subtotal, decimal taxPercentage, decimal expectedTotal)
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = subtotal,
            TaxPercentage = taxPercentage,
            DiscountPercentage = 0,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = subtotal, Subtotal = subtotal, Total = subtotal }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Total.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task CalculateSaleTotals_AppliesTaxAfterDiscount()
    {
        // Arrange: 1000 - 10% discount = 900, then + 5% tax = 945
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = 1000,
            DiscountPercentage = 10,
            TaxPercentage = 5,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 1000, Subtotal = 1000, Total = 1000 }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.DiscountAmount.Should().Be(100);
        result.TaxAmount.Should().Be(45); // 5% of 900
        result.Total.Should().Be(945);
    }

    #endregion

    #region UAE VAT (5%) Tests

    [Fact]
    public async Task CreateSale_AppliesDefaultUAEVAT()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = 1000,
            // TaxPercentage defaults to 5% in entity
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 1000, Subtotal = 1000, Total = 1000 }
            }
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.TaxPercentage.Should().Be(5);
        result.TaxAmount.Should().Be(50);
        result.Total.Should().Be(1050);
    }

    #endregion

    #region Sale Item Calculations Tests

    [Theory]
    [InlineData(5, 100, 500)]     // 5 items at 100 each
    [InlineData(3, 250, 750)]     // 3 items at 250 each
    [InlineData(1, 1000, 1000)]   // Single item
    [InlineData(10, 50, 500)]     // 10 items at 50 each
    public async Task CalculateSaleItemTotals_CalculatesLineTotalCorrectly(
        int quantity, decimal unitPrice, decimal expectedLineTotal)
    {
        // Arrange
        var sale = await CreateBasicSale();
        var item = new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Test Item",
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        // Act
        var result = await _service.AddSaleItemAsync(item);

        // Assert
        result.Subtotal.Should().Be(expectedLineTotal);
    }

    [Fact]
    public async Task RecalculateSaleTotals_SumsAllItemSubtotals()
    {
        // Arrange
        var sale = await CreateBasicSale();

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Item 1",
            Quantity = 2,
            UnitPrice = 100,
            Subtotal = 200,
            Total = 200
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Item 2",
            Quantity = 1,
            UnitPrice = 300,
            Subtotal = 300,
            Total = 300
        });

        // Act
        await _service.RecalculateSaleTotalsAsync(sale.Id);

        // Assert
        var updatedSale = await _context.Sales
            .Include(s => s.Items)
            .FirstAsync(s => s.Id == sale.Id);

        updatedSale.SubTotal.Should().Be(500); // 200 + 300
    }

    #endregion

    #region Payment Status Transitions Tests

    [Fact]
    public async Task PaymentStatus_TransitionsFromPendingToPartial()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(1000);
        sale.PaymentStatus.Should().Be(PaymentStatus.Pending);

        // Act - Pay 50%
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaymentStatus.Should().Be(PaymentStatus.Partial);
    }

    [Fact]
    public async Task PaymentStatus_TransitionsFromPartialToPaid()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(1000);

        // Act - Pay in two installments
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Card);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task PaymentStatus_BecomesOverdue_WhenPastDueDate()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(1000);
        sale.DueDate = DateTime.UtcNow.AddDays(-1); // Past due
        await _context.SaveChangesAsync();

        // Act - Check computed property
        var updatedSale = await _context.Sales.FindAsync(sale.Id);

        // Assert
        updatedSale!.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task PaymentStatus_NotOverdue_WhenFullyPaid()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(1000);
        sale.DueDate = DateTime.UtcNow.AddDays(-1); // Past due
        await _context.SaveChangesAsync();

        // Act - Pay in full
        await _service.RecordPaymentAsync(sale.Id, 1000, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.IsOverdue.Should().BeFalse();
        updatedSale.IsFullyPaid.Should().BeTrue();
    }

    #endregion

    #region Balance Calculation Tests

    [Theory]
    [InlineData(1000, 0, 1000)]     // No payment
    [InlineData(1000, 300, 700)]    // Partial payment
    [InlineData(1000, 1000, 0)]     // Full payment
    [InlineData(500, 200, 300)]     // Another partial
    public async Task Balance_CalculatesCorrectly(decimal total, decimal paid, decimal expectedBalance)
    {
        // Arrange
        var sale = await CreateSaleWithTotal(total);

        if (paid > 0)
        {
            await _service.RecordPaymentAsync(sale.Id, paid, PaymentMethod.Cash);
        }

        // Act
        var updatedSale = await _context.Sales.FindAsync(sale.Id);

        // Assert
        updatedSale!.Balance.Should().Be(expectedBalance);
    }

    #endregion

    #region Quotation Expiry Tests

    [Fact]
    public async Task Quotation_IsExpired_WhenPastExpiryDate_AndDraft()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
            Status = QuotationStatus.Draft,
            CreatedBy = "test-user"
        };

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();

        // Act & Assert
        quotation.IsExpired.Should().BeTrue();
    }

    [Fact]
    public async Task Quotation_IsExpired_WhenPastExpiryDate_AndSent()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
            Status = QuotationStatus.Sent,
            CreatedBy = "test-user"
        };

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();

        // Act & Assert
        quotation.IsExpired.Should().BeTrue();
    }

    [Fact]
    public async Task Quotation_NotExpired_WhenAccepted_EvenIfPastExpiryDate()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Past expiry
            Status = QuotationStatus.Accepted, // But already accepted
            AcceptedDate = DateTime.UtcNow.AddDays(-5),
            CreatedBy = "test-user"
        };

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();

        // Act & Assert
        quotation.IsExpired.Should().BeFalse();
    }

    [Fact]
    public async Task Quotation_IsActive_WhenSentAndNotExpired()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(10), // Not expired
            Status = QuotationStatus.Sent,
            CreatedBy = "test-user"
        };

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();

        // Act & Assert
        quotation.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Quotation_CanConvertToSale_OnlyWhenAccepted()
    {
        // Arrange
        var acceptedQuotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            Status = QuotationStatus.Accepted,
            CreatedBy = "test-user"
        };

        var sentQuotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            Status = QuotationStatus.Sent,
            CreatedBy = "test-user"
        };

        _context.Quotations.AddRange(acceptedQuotation, sentQuotation);
        await _context.SaveChangesAsync();

        // Act & Assert
        acceptedQuotation.CanConvertToSale.Should().BeTrue();
        sentQuotation.CanConvertToSale.Should().BeFalse();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalSalesAsync_SumsCorrectly()
    {
        // Arrange
        await CreateSaleWithTotal(500);
        await CreateSaleWithTotal(300);
        await CreateSaleWithTotal(200);

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var total = await _service.GetTotalSalesAsync(_testBranchId, startDate, endDate);

        // Assert
        total.Should().Be(1000);
    }

    [Fact]
    public async Task GetTotalPaidAsync_SumsOnlyPaidAmounts()
    {
        // Arrange
        var sale1 = await CreateSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale1.Id, 300, PaymentMethod.Cash);

        var sale2 = await CreateSaleWithTotal(300);
        await _service.RecordPaymentAsync(sale2.Id, 300, PaymentMethod.Card);

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var totalPaid = await _service.GetTotalPaidAsync(_testBranchId, startDate, endDate);

        // Assert
        totalPaid.Should().Be(600); // 300 + 300
    }

    [Fact]
    public async Task GetTotalOutstandingAsync_CalculatesUnpaidBalance()
    {
        // Arrange
        var sale1 = await CreateSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale1.Id, 300, PaymentMethod.Cash);

        var sale2 = await CreateSaleWithTotal(300);
        // No payment on sale2

        // Act
        var outstanding = await _service.GetTotalOutstandingAsync(_testBranchId);

        // Assert
        outstanding.Should().Be(500); // (500-300) + 300
    }

    [Fact]
    public async Task GetSalesStatisticsAsync_ReturnsComprehensiveStats()
    {
        // Arrange
        var sale1 = await CreateSaleWithTotal(500);
        await _service.RecordPaymentAsync(sale1.Id, 500, PaymentMethod.Cash);
        await _service.CompleteSaleAsync(sale1.Id);

        var sale2 = await CreateSaleWithTotal(300);
        sale2.DueDate = DateTime.UtcNow.AddDays(-5);
        await _context.SaveChangesAsync();

        // Act
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);

        // Assert
        stats.TotalSales.Should().Be(800);
        stats.TotalPaid.Should().Be(500);
        stats.TotalOutstanding.Should().Be(300);
        stats.SalesCount.Should().Be(2);
        stats.CompletedSalesCount.Should().Be(1);
        stats.OverdueSalesCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSalesStatisticsAsync_CalculatesAverageTransactionValue()
    {
        // Arrange
        await CreateSaleWithTotal(600);
        await CreateSaleWithTotal(400);

        // Act
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);

        // Assert
        stats.AverageTransactionValue.Should().Be(500); // (600 + 400) / 2
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task CreateSale_WithZeroSubtotal_CreatesWithZeroTotal()
    {
        // Arrange
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SubTotal = 0,
            TaxPercentage = 5,
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateSaleAsync(sale);

        // Assert
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task RecordPayment_ExactAmount_SetsFullyPaid()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(99.99m); // Decimal precision test

        // Act
        await _service.RecordPaymentAsync(sale.Id, 99.99m, PaymentMethod.Cash);

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.IsFullyPaid.Should().BeTrue();
        updatedSale.Balance.Should().Be(0);
    }

    [Fact]
    public async Task MultipleSmallPayments_EventuallyFullyPays()
    {
        // Arrange
        var sale = await CreateSaleWithTotal(100);

        // Act - Pay in 10 installments of 10 each
        for (int i = 0; i < 10; i++)
        {
            await _service.RecordPaymentAsync(sale.Id, 10, PaymentMethod.Cash);
        }

        // Assert
        var updatedSale = await _context.Sales.FindAsync(sale.Id);
        updatedSale!.IsFullyPaid.Should().BeTrue();
        updatedSale.PaidAmount.Should().Be(100);
    }

    #endregion

    #region Helper Methods

    private async Task<Sale> CreateBasicSale()
    {
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            TaxPercentage = 0,
            CreatedBy = "test-user"
        };
        return await _service.CreateSaleAsync(sale);
    }

    private async Task<Sale> CreateSaleWithTotal(decimal total)
    {
        var sale = new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            SubTotal = total,
            Total = total,
            TaxPercentage = 0,
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
