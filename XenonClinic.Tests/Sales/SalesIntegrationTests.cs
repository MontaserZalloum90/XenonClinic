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
/// Integration tests for end-to-end sales workflows
/// Tests complete business scenarios involving multiple operations
/// </summary>
public class SalesIntegrationTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;
    private readonly int _testPatientId2 = 2;
    private int _sequenceCounter = 0;

    public SalesIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"IntegrationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"SEQ-{++_sequenceCounter:D6}");

        _service = new SalesService(_context, _sequenceGeneratorMock.Object);

        SetupTestData();
    }

    private void SetupTestData()
    {
        var branch = new Branch
        {
            Id = _testBranchId,
            Name = "Main Clinic Branch",
            Code = "MAIN-BR",
            IsActive = true
        };
        _context.Branches.Add(branch);

        var patient1 = new Patient
        {
            Id = _testPatientId,
            FirstName = "Ahmed",
            LastName = "Al-Maktoum",
            BranchId = _testBranchId,
            DateOfBirth = DateTime.UtcNow.AddYears(-35),
            Gender = "Male"
        };

        var patient2 = new Patient
        {
            Id = _testPatientId2,
            FirstName = "Sara",
            LastName = "Mohammed",
            BranchId = _testBranchId,
            DateOfBirth = DateTime.UtcNow.AddYears(-28),
            Gender = "Female"
        };

        _context.Patients.AddRange(patient1, patient2);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Complete Sale Workflow Tests

    [Fact]
    public async Task CompleteSaleWorkflow_FromDraftToCompletion()
    {
        // Step 1: Create draft sale
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            TaxPercentage = 5,
            CreatedBy = "reception-user"
        });

        sale.Status.Should().Be(SaleStatus.Draft);
        sale.InvoiceNumber.Should().NotBeNullOrEmpty();

        // Step 2: Add multiple items
        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "General Consultation",
            Description = "Doctor consultation - 30 minutes",
            Quantity = 1,
            UnitPrice = 200,
            Subtotal = 200,
            Total = 200
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Blood Test Panel",
            Description = "Complete blood count",
            Quantity = 1,
            UnitPrice = 150,
            Subtotal = 150,
            Total = 150
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "X-Ray Chest",
            Description = "Digital chest X-ray",
            Quantity = 1,
            UnitPrice = 250,
            Subtotal = 250,
            Total = 250
        });

        // Recalculate totals
        await _service.RecalculateSaleTotalsAsync(sale.Id);

        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        updatedSale!.SubTotal.Should().Be(600);
        updatedSale.TaxAmount.Should().Be(30); // 5% of 600
        updatedSale.Total.Should().Be(630);

        // Step 3: Confirm the sale
        var confirmedSale = await _service.ConfirmSaleAsync(sale.Id);
        confirmedSale.Status.Should().Be(SaleStatus.Confirmed);

        // Step 4: Process payment in two parts
        var payment1 = await _service.RecordPaymentAsync(sale.Id, 400, PaymentMethod.Card, "VISA-1234");
        payment1.Amount.Should().Be(400);
        payment1.PaymentMethod.Should().Be(PaymentMethod.Card);

        var saleAfterPartialPayment = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterPartialPayment!.PaymentStatus.Should().Be(PaymentStatus.Partial);
        saleAfterPartialPayment.PaidAmount.Should().Be(400);
        saleAfterPartialPayment.Balance.Should().Be(230);

        var payment2 = await _service.RecordPaymentAsync(sale.Id, 230, PaymentMethod.Cash);
        payment2.PaymentMethod.Should().Be(PaymentMethod.Cash);

        var saleAfterFullPayment = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterFullPayment!.PaymentStatus.Should().Be(PaymentStatus.Paid);
        saleAfterFullPayment.IsFullyPaid.Should().BeTrue();

        // Step 5: Complete the sale
        var completedSale = await _service.CompleteSaleAsync(sale.Id);
        completedSale.Status.Should().Be(SaleStatus.Completed);

        // Verify all payments exist
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        payments.Should().HaveCount(2);
        payments.Sum(p => p.Amount).Should().Be(630);
    }

    [Fact]
    public async Task SaleCancellationWorkflow_DraftSale()
    {
        // Create draft sale
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user"
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Consultation",
            Quantity = 1,
            UnitPrice = 200,
            Subtotal = 200,
            Total = 200
        });

        // Cancel without any payment
        var cancelledSale = await _service.CancelSaleAsync(sale.Id, "Patient cancelled appointment");

        cancelledSale.Status.Should().Be(SaleStatus.Cancelled);
        cancelledSale.PaymentStatus.Should().Be(PaymentStatus.Cancelled);
        cancelledSale.Notes.Should().Contain("Patient cancelled appointment");
    }

    #endregion

    #region Quotation to Sale Conversion Workflow

    [Fact]
    public async Task QuotationToSaleConversion_CompleteWorkflow()
    {
        // Step 1: Create quotation
        var quotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 14,
            TaxPercentage = 5,
            CreatedBy = "sales-rep"
        });

        quotation.Status.Should().Be(QuotationStatus.Draft);
        quotation.QuotationNumber.Should().NotBeNullOrEmpty();

        // Step 2: Add quotation items
        await _service.AddQuotationItemAsync(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Dental Package - Basic",
            Description = "Includes cleaning and checkup",
            Quantity = 1,
            UnitPrice = 500,
            Subtotal = 500,
            Total = 500
        });

        await _service.AddQuotationItemAsync(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Dental Package - Whitening",
            Description = "Professional teeth whitening",
            Quantity = 1,
            UnitPrice = 800,
            Subtotal = 800,
            Total = 800
        });

        await _service.RecalculateQuotationTotalsAsync(quotation.Id);

        var updatedQuotation = await _service.GetQuotationByIdAsync(quotation.Id);
        updatedQuotation!.SubTotal.Should().Be(1300);
        updatedQuotation.Total.Should().BeGreaterThan(1300); // Including tax

        // Step 3: Send quotation to patient
        var sentQuotation = await _service.SendQuotationAsync(quotation.Id);
        sentQuotation.Status.Should().Be(QuotationStatus.Sent);
        sentQuotation.SentDate.Should().NotBeNull();
        sentQuotation.IsActive.Should().BeTrue();

        // Step 4: Patient accepts quotation
        var acceptedQuotation = await _service.AcceptQuotationAsync(quotation.Id);
        acceptedQuotation.Status.Should().Be(QuotationStatus.Accepted);
        acceptedQuotation.AcceptedDate.Should().NotBeNull();
        acceptedQuotation.CanConvertToSale.Should().BeTrue();

        // Step 5: Convert quotation to sale
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);
        sale.Should().NotBeNull();
        sale.PatientId.Should().Be(_testPatientId);
        sale.BranchId.Should().Be(_testBranchId);
        sale.Status.Should().Be(SaleStatus.Draft);
        sale.Items.Should().HaveCount(2);
        sale.QuotationId.Should().Be(quotation.Id);

        // Verify quotation status updated
        var convertedQuotation = await _service.GetQuotationByIdAsync(quotation.Id);
        convertedQuotation!.Status.Should().Be(QuotationStatus.Converted);
        convertedQuotation.ConvertedToSaleId.Should().Be(sale.Id);

        // Step 6: Complete the sale workflow
        await _service.ConfirmSaleAsync(sale.Id);
        await _service.RecordPaymentAsync(sale.Id, sale.Total, PaymentMethod.Cash);
        var completedSale = await _service.CompleteSaleAsync(sale.Id);
        completedSale.Status.Should().Be(SaleStatus.Completed);
    }

    [Fact]
    public async Task QuotationRejection_Workflow()
    {
        // Create and send quotation
        var quotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 7,
            CreatedBy = "sales-rep",
            Items = new List<QuotationItem>
            {
                new QuotationItem
                {
                    ItemName = "Premium Package",
                    Quantity = 1,
                    UnitPrice = 5000,
                    Subtotal = 5000,
                    Total = 5000
                }
            }
        });

        await _service.SendQuotationAsync(quotation.Id);

        // Patient rejects the quotation
        var rejectedQuotation = await _service.RejectQuotationAsync(quotation.Id, "Price too high");

        rejectedQuotation.Status.Should().Be(QuotationStatus.Rejected);
        rejectedQuotation.RejectedDate.Should().NotBeNull();
        rejectedQuotation.RejectionReason.Should().Be("Price too high");
        rejectedQuotation.CanConvertToSale.Should().BeFalse();
    }

    [Fact]
    public async Task QuotationExpiry_Workflow()
    {
        // Create quotation with short validity
        var quotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 1,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Already expired
            CreatedBy = "sales-rep",
            Items = new List<QuotationItem>
            {
                new QuotationItem
                {
                    ItemName = "Service",
                    Quantity = 1,
                    UnitPrice = 100,
                    Subtotal = 100,
                    Total = 100
                }
            }
        });

        await _service.SendQuotationAsync(quotation.Id);

        // Verify quotation is expired
        var expiredQuotation = await _service.GetQuotationByIdAsync(quotation.Id);
        expiredQuotation!.IsExpired.Should().BeTrue();
        expiredQuotation.IsActive.Should().BeFalse();

        // Attempting to accept should fail
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptQuotationAsync(quotation.Id));
    }

    #endregion

    #region Refund Workflow Tests

    [Fact]
    public async Task PartialRefundWorkflow()
    {
        // Create and pay for a sale
        var sale = await CreateAndPaySale(1000);

        sale.PaymentStatus.Should().Be(PaymentStatus.Paid);
        sale.PaidAmount.Should().Be(1000);

        // Get the payment for partial refund
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        var originalPayment = payments.First();

        // Process partial refund
        await _service.RefundPaymentAsync(originalPayment.Id, 300, "Partial refund - service not provided");

        // Verify sale status after partial refund
        var refundedSale = await _service.GetSaleByIdAsync(sale.Id);
        refundedSale!.PaidAmount.Should().Be(700); // 1000 - 300
        refundedSale.PaymentStatus.Should().Be(PaymentStatus.Partial);

        // Verify refund payment exists
        var allPayments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        allPayments.Should().HaveCount(2);
        allPayments.Should().Contain(p => p.Amount == -300);
    }

    [Fact]
    public async Task FullRefundWorkflow()
    {
        // Create and complete a sale
        var sale = await CreateAndPaySale(500);

        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        var originalPayment = payments.First();

        // Process full refund
        await _service.RefundPaymentAsync(originalPayment.Id, 500, "Full refund - patient dissatisfied");

        // Verify sale status after full refund
        var refundedSale = await _service.GetSaleByIdAsync(sale.Id);
        refundedSale!.PaidAmount.Should().Be(0);
        refundedSale.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        refundedSale.Status.Should().Be(SaleStatus.Refunded);
    }

    #endregion

    #region Multi-Patient Concurrent Sales Tests

    [Fact]
    public async Task MultiplePatientsSales_IsolatedCorrectly()
    {
        // Create sales for different patients
        var sale1 = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Patient 1 Service", Quantity = 1, UnitPrice = 300, Subtotal = 300, Total = 300 }
            }
        });

        var sale2 = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId2,
            BranchId = _testBranchId,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Patient 2 Service", Quantity = 1, UnitPrice = 500, Subtotal = 500, Total = 500 }
            }
        });

        // Verify patient isolation
        var patient1Sales = await _service.GetSalesByPatientIdAsync(_testPatientId);
        var patient2Sales = await _service.GetSalesByPatientIdAsync(_testPatientId2);

        patient1Sales.Should().HaveCount(1);
        patient1Sales.First().Id.Should().Be(sale1.Id);

        patient2Sales.Should().HaveCount(1);
        patient2Sales.First().Id.Should().Be(sale2.Id);

        // Process payments independently
        await _service.RecordPaymentAsync(sale1.Id, 300, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale2.Id, 250, PaymentMethod.Card);

        var updatedSale1 = await _service.GetSaleByIdAsync(sale1.Id);
        var updatedSale2 = await _service.GetSaleByIdAsync(sale2.Id);

        updatedSale1!.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updatedSale2!.PaymentStatus.Should().Be(PaymentStatus.Partial);
    }

    #endregion

    #region Discount and Tax Combined Workflow Tests

    [Fact]
    public async Task ComplexDiscountAndTaxCalculation_Workflow()
    {
        // Create sale with items totaling 2000
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            DiscountPercentage = 20, // 20% discount
            TaxPercentage = 5, // UAE VAT
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Premium Consultation", Quantity = 2, UnitPrice = 500, Subtotal = 1000, Total = 1000 },
                new SaleItem { ItemName = "Lab Tests", Quantity = 4, UnitPrice = 250, Subtotal = 1000, Total = 1000 }
            }
        });

        // Verify calculations:
        // SubTotal: 2000
        // Discount: 2000 * 20% = 400
        // After discount: 1600
        // Tax: 1600 * 5% = 80
        // Total: 1680
        var retrievedSale = await _service.GetSaleByIdAsync(sale.Id);
        retrievedSale!.SubTotal.Should().Be(2000);
        retrievedSale.DiscountAmount.Should().Be(400);
        retrievedSale.TaxAmount.Should().Be(80);
        retrievedSale.Total.Should().Be(1680);

        // Pay in installments
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 500, PaymentMethod.Card);
        await _service.RecordPaymentAsync(sale.Id, 680, PaymentMethod.BankTransfer);

        var paidSale = await _service.GetSaleByIdAsync(sale.Id);
        paidSale!.IsFullyPaid.Should().BeTrue();
        paidSale.Balance.Should().Be(0);
    }

    [Fact]
    public async Task ApplyDiscountToExistingSale_RecalculatesTotals()
    {
        // Create sale without discount
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            TaxPercentage = 0,
            CreatedBy = "test-user",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 1000, Subtotal = 1000, Total = 1000 }
            }
        });

        sale.Total.Should().Be(1000);

        // Now apply a discount
        sale.DiscountPercentage = 15;
        await _service.UpdateSaleAsync(sale);
        await _service.RecalculateSaleTotalsAsync(sale.Id);

        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        updatedSale!.DiscountAmount.Should().Be(150);
        updatedSale.Total.Should().Be(850);
    }

    #endregion

    #region Payment Method Mix Tests

    [Fact]
    public async Task MixedPaymentMethods_TrackedCorrectly()
    {
        var sale = await CreateSaleWithTotal(1000);

        // Pay with multiple methods
        await _service.RecordPaymentAsync(sale.Id, 300, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(sale.Id, 300, PaymentMethod.Card, "VISA-5678");
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.BankTransfer, "TRF-123456");
        await _service.RecordPaymentAsync(sale.Id, 200, PaymentMethod.Insurance, "INS-POL-789");

        // Verify all payment methods recorded
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);

        payments.Should().HaveCount(4);
        payments.Should().Contain(p => p.PaymentMethod == PaymentMethod.Cash && p.Amount == 300);
        payments.Should().Contain(p => p.PaymentMethod == PaymentMethod.Card && p.Amount == 300);
        payments.Should().Contain(p => p.PaymentMethod == PaymentMethod.BankTransfer && p.Amount == 200);
        payments.Should().Contain(p => p.PaymentMethod == PaymentMethod.Insurance && p.Amount == 200);

        // Verify reference numbers
        payments.First(p => p.PaymentMethod == PaymentMethod.Card).ReferenceNumber.Should().Be("VISA-5678");
        payments.First(p => p.PaymentMethod == PaymentMethod.BankTransfer).ReferenceNumber.Should().Be("TRF-123456");

        // Verify sale fully paid
        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        updatedSale!.IsFullyPaid.Should().BeTrue();

        // Check statistics
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);
        stats.PaymentMethodDistribution.Should().ContainKey(PaymentMethod.Cash);
        stats.PaymentMethodDistribution.Should().ContainKey(PaymentMethod.Card);
        stats.PaymentMethodDistribution[PaymentMethod.Cash].Should().Be(300);
        stats.PaymentMethodDistribution[PaymentMethod.Card].Should().Be(300);
    }

    #endregion

    #region Overdue Sales Management Tests

    [Fact]
    public async Task OverdueSalesTracking_Workflow()
    {
        // Create current sale (not overdue)
        var currentSale = await CreateSaleWithTotal(500);

        // Create overdue sale
        var overdueSale = await CreateSaleWithTotal(300);
        overdueSale.DueDate = DateTime.UtcNow.AddDays(-10);
        await _context.SaveChangesAsync();

        // Create another overdue sale with partial payment
        var partiallyPaidOverdueSale = await CreateSaleWithTotal(400);
        partiallyPaidOverdueSale.DueDate = DateTime.UtcNow.AddDays(-5);
        await _context.SaveChangesAsync();
        await _service.RecordPaymentAsync(partiallyPaidOverdueSale.Id, 200, PaymentMethod.Cash);

        // Create paid sale (not overdue even though past due date)
        var paidSale = await CreateSaleWithTotal(600);
        paidSale.DueDate = DateTime.UtcNow.AddDays(-3);
        await _context.SaveChangesAsync();
        await _service.RecordPaymentAsync(paidSale.Id, 600, PaymentMethod.Cash);

        // Get overdue sales
        var overdueSales = await _service.GetOverdueSalesAsync(_testBranchId);

        // Should have 2 overdue sales (not the paid one or current one)
        overdueSales.Should().HaveCount(2);
        overdueSales.Should().Contain(s => s.Id == overdueSale.Id);
        overdueSales.Should().Contain(s => s.Id == partiallyPaidOverdueSale.Id);
        overdueSales.Should().NotContain(s => s.Id == paidSale.Id);
        overdueSales.Should().NotContain(s => s.Id == currentSale.Id);

        // Verify statistics
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);
        stats.OverdueSalesCount.Should().Be(2);
    }

    #endregion

    #region Sale Items Management Tests

    [Fact]
    public async Task SaleItemsModification_RecalculatesTotals()
    {
        // Create sale with initial item
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            TaxPercentage = 5,
            CreatedBy = "test-user"
        });

        // Add first item
        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Service A",
            Quantity = 1,
            UnitPrice = 200,
            Subtotal = 200,
            Total = 200
        });

        await _service.RecalculateSaleTotalsAsync(sale.Id);
        var saleAfterFirst = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterFirst!.SubTotal.Should().Be(200);
        saleAfterFirst.Total.Should().Be(210); // +5% tax

        // Add second item
        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Service B",
            Quantity = 2,
            UnitPrice = 150,
            Subtotal = 300,
            Total = 300
        });

        await _service.RecalculateSaleTotalsAsync(sale.Id);
        var saleAfterSecond = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterSecond!.SubTotal.Should().Be(500);
        saleAfterSecond.Total.Should().Be(525); // +5% tax

        // Update an item
        var items = await _context.SaleItems.Where(i => i.SaleId == sale.Id).ToListAsync();
        var itemToUpdate = items.First();
        itemToUpdate.Quantity = 3;
        itemToUpdate.Subtotal = itemToUpdate.Quantity * itemToUpdate.UnitPrice;
        itemToUpdate.Total = itemToUpdate.Subtotal;
        await _service.UpdateSaleItemAsync(itemToUpdate);

        await _service.RecalculateSaleTotalsAsync(sale.Id);
        var saleAfterUpdate = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterUpdate!.SubTotal.Should().Be(900); // 3*200 + 300
        saleAfterUpdate.Total.Should().Be(945); // +5% tax

        // Remove an item
        await _service.DeleteSaleItemAsync(items.Last().Id);
        await _service.RecalculateSaleTotalsAsync(sale.Id);

        var saleAfterDelete = await _service.GetSaleByIdAsync(sale.Id);
        saleAfterDelete!.SubTotal.Should().Be(600); // Only 3*200 remains
        saleAfterDelete.Total.Should().Be(630); // +5% tax
    }

    #endregion

    #region Statistics and Reporting Tests

    [Fact]
    public async Task SalesStatistics_ComprehensiveReporting()
    {
        // Create diverse sale data
        var sale1 = await CreateAndPaySale(1000);
        await _service.CompleteSaleAsync(sale1.Id);

        var sale2 = await CreateAndPaySale(500);
        await _service.CompleteSaleAsync(sale2.Id);

        var sale3 = await CreateSaleWithTotal(300);
        await _service.RecordPaymentAsync(sale3.Id, 150, PaymentMethod.Cash);
        // Partial payment

        var sale4 = await CreateSaleWithTotal(200);
        // No payment

        var cancelledSale = await CreateSaleWithTotal(400);
        await _service.CancelSaleAsync(cancelledSale.Id, "Test cancellation");

        // Get comprehensive statistics
        var stats = await _service.GetSalesStatisticsAsync(_testBranchId);

        stats.SalesCount.Should().BeGreaterThanOrEqualTo(4); // At least 4 non-cancelled
        stats.CompletedSalesCount.Should().Be(2);
        stats.TotalSales.Should().Be(2000); // 1000 + 500 + 300 + 200 (excluding cancelled)
        stats.TotalPaid.Should().Be(1650); // 1000 + 500 + 150
        stats.TotalOutstanding.Should().Be(350); // 150 + 200
    }

    [Fact]
    public async Task DateRangeFiltering_WorksCorrectly()
    {
        // Create sales at different dates
        var todaySale = await CreateSaleWithTotal(100);

        var yesterdaySale = await CreateSaleWithTotal(200);
        yesterdaySale.SaleDate = DateTime.UtcNow.AddDays(-1);
        yesterdaySale.CreatedAt = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync();

        var lastWeekSale = await CreateSaleWithTotal(300);
        lastWeekSale.SaleDate = DateTime.UtcNow.AddDays(-7);
        lastWeekSale.CreatedAt = DateTime.UtcNow.AddDays(-7);
        await _context.SaveChangesAsync();

        // Query only today
        var todaySales = await _service.GetSalesByDateRangeAsync(
            _testBranchId,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(1));

        todaySales.Should().ContainSingle(s => s.Id == todaySale.Id);

        // Query last 3 days
        var recentSales = await _service.GetSalesByDateRangeAsync(
            _testBranchId,
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(1));

        recentSales.Should().HaveCount(2);
        recentSales.Should().Contain(s => s.Id == todaySale.Id);
        recentSales.Should().Contain(s => s.Id == yesterdaySale.Id);
    }

    #endregion

    #region Helper Methods

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

    private async Task<Sale> CreateAndPaySale(decimal amount)
    {
        var sale = await CreateSaleWithTotal(amount);
        await _service.ConfirmSaleAsync(sale.Id);
        await _service.RecordPaymentAsync(sale.Id, amount, PaymentMethod.Cash);
        return await _service.GetSaleByIdAsync(sale.Id) ?? sale;
    }

    #endregion
}
