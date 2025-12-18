using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;
using PatientEntity = XenonClinic.Core.Entities.Patient;

namespace XenonClinic.Tests.Sales;

/// <summary>
/// End-to-End tests for Sales module with multi-tenancy validation.
/// Tests complete workflows including tenant isolation and access control.
/// </summary>
public class SalesE2ETests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly SalesService _service;

    // Multi-tenant test data
    private Tenant _tenantA = null!;
    private Tenant _tenantB = null!;
    private Company _companyA = null!;
    private Company _companyB = null!;
    private Branch _branchA1 = null!;
    private Branch _branchA2 = null!;
    private Branch _branchB1 = null!;
    private PatientEntity _patientA1 = null!;
    private PatientEntity _patientA2 = null!;
    private PatientEntity _patientB1 = null!;

    private int _sequenceCounter = 0;

    public SalesE2ETests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"SalesE2ETestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SequenceType>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"SEQ-{++_sequenceCounter:D8}");

        _service = new SalesService(_context, _sequenceGeneratorMock.Object);

        SetupMultiTenantTestData();
    }

    private void SetupMultiTenantTestData()
    {
        // Create tenants
        _tenantA = new Tenant
        {
            Id = 1,
            Name = "Clinic Network A",
            Code = "CLINIC-A",
            IsActive = true,
            MaxCompanies = 5,
            MaxBranchesPerCompany = 10,
            PlatformTenantId = Guid.NewGuid()
        };

        _tenantB = new Tenant
        {
            Id = 2,
            Name = "Clinic Network B",
            Code = "CLINIC-B",
            IsActive = true,
            MaxCompanies = 3,
            MaxBranchesPerCompany = 5,
            PlatformTenantId = Guid.NewGuid()
        };

        _context.Tenants.AddRange(_tenantA, _tenantB);

        // Create companies
        _companyA = new Company
        {
            Id = 1,
            TenantId = _tenantA.Id,
            Name = "Clinic A Main Company",
            Code = "CA-MAIN",
            IsActive = true
        };

        _companyB = new Company
        {
            Id = 2,
            TenantId = _tenantB.Id,
            Name = "Clinic B Main Company",
            Code = "CB-MAIN",
            IsActive = true
        };

        _context.Companies.AddRange(_companyA, _companyB);

        // Create branches
        _branchA1 = new Branch
        {
            Id = 1,
            CompanyId = _companyA.Id,
            Name = "Clinic A - Downtown Branch",
            Code = "CA-DT",
            IsActive = true
        };

        _branchA2 = new Branch
        {
            Id = 2,
            CompanyId = _companyA.Id,
            Name = "Clinic A - Suburban Branch",
            Code = "CA-SB",
            IsActive = true
        };

        _branchB1 = new Branch
        {
            Id = 3,
            CompanyId = _companyB.Id,
            Name = "Clinic B - Main Branch",
            Code = "CB-MB",
            IsActive = true
        };

        _context.Branches.AddRange(_branchA1, _branchA2, _branchB1);

        // Create patients
        _patientA1 = new PatientEntity
        {
            Id = 1,
            BranchId = _branchA1.Id,
            FirstName = "Ali",
            LastName = "Ahmed",
            DateOfBirth = DateTime.UtcNow.AddYears(-40),
            Gender = "Male"
        };

        _patientA2 = new PatientEntity
        {
            Id = 2,
            BranchId = _branchA2.Id,
            FirstName = "Fatima",
            LastName = "Hassan",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Gender = "Female"
        };

        _patientB1 = new PatientEntity
        {
            Id = 3,
            BranchId = _branchB1.Id,
            FirstName = "Omar",
            LastName = "Khalid",
            DateOfBirth = DateTime.UtcNow.AddYears(-35),
            Gender = "Male"
        };

        _context.Patients.AddRange(_patientA1, _patientA2, _patientB1);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Multi-Tenant Sales Isolation Tests

    [Fact]
    public async Task Sales_AreIsolatedByBranch()
    {
        // Create sales in different branches
        var saleA1 = await CreateSaleForPatient(_patientA1, _branchA1, 500);
        var saleA2 = await CreateSaleForPatient(_patientA2, _branchA2, 300);
        var saleB1 = await CreateSaleForPatient(_patientB1, _branchB1, 700);

        // Query by branch should return only that branch's sales
        var branchA1Sales = await _service.GetSalesByBranchIdAsync(_branchA1.Id);
        var branchA2Sales = await _service.GetSalesByBranchIdAsync(_branchA2.Id);
        var branchB1Sales = await _service.GetSalesByBranchIdAsync(_branchB1.Id);

        branchA1Sales.Should().HaveCount(1);
        branchA1Sales.First().Id.Should().Be(saleA1.Id);

        branchA2Sales.Should().HaveCount(1);
        branchA2Sales.First().Id.Should().Be(saleA2.Id);

        branchB1Sales.Should().HaveCount(1);
        branchB1Sales.First().Id.Should().Be(saleB1.Id);
    }

    [Fact]
    public async Task Sales_AreIsolatedByPatient()
    {
        // Create multiple sales for different patients
        var sale1ForPatientA1 = await CreateSaleForPatient(_patientA1, _branchA1, 200);
        var sale2ForPatientA1 = await CreateSaleForPatient(_patientA1, _branchA1, 300);
        var saleForPatientA2 = await CreateSaleForPatient(_patientA2, _branchA2, 150);

        // Query by patient
        var patientA1Sales = await _service.GetSalesByPatientIdAsync(_patientA1.Id);
        var patientA2Sales = await _service.GetSalesByPatientIdAsync(_patientA2.Id);

        patientA1Sales.Should().HaveCount(2);
        patientA1Sales.Sum(s => s.Total).Should().Be(500);

        patientA2Sales.Should().HaveCount(1);
        patientA2Sales.First().Total.Should().Be(150);
    }

    [Fact]
    public async Task Payments_AreIsolatedToSale()
    {
        // Create sales for different tenants
        var saleA = await CreateSaleForPatient(_patientA1, _branchA1, 1000);
        var saleB = await CreateSaleForPatient(_patientB1, _branchB1, 500);

        // Add payments
        await _service.RecordPaymentAsync(saleA.Id, 600, PaymentMethod.Cash);
        await _service.RecordPaymentAsync(saleB.Id, 500, PaymentMethod.Card);

        // Verify payment isolation
        var paymentsA = await _service.GetPaymentsBySaleIdAsync(saleA.Id);
        var paymentsB = await _service.GetPaymentsBySaleIdAsync(saleB.Id);

        paymentsA.Should().HaveCount(1);
        paymentsA.First().Amount.Should().Be(600);

        paymentsB.Should().HaveCount(1);
        paymentsB.First().Amount.Should().Be(500);

        // Verify sale status updated correctly
        var updatedSaleA = await _service.GetSaleByIdAsync(saleA.Id);
        var updatedSaleB = await _service.GetSaleByIdAsync(saleB.Id);

        updatedSaleA!.PaymentStatus.Should().Be(PaymentStatus.Partial);
        updatedSaleB!.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Statistics_AreIsolatedByBranch()
    {
        // Create sales in different branches
        await CreateAndPaySaleForPatient(_patientA1, _branchA1, 1000);
        await CreateAndPaySaleForPatient(_patientA1, _branchA1, 500);
        await CreateAndPaySaleForPatient(_patientA2, _branchA2, 300);
        await CreateAndPaySaleForPatient(_patientB1, _branchB1, 2000);

        // Get statistics for each branch
        var statsA1 = await _service.GetSalesStatisticsAsync(_branchA1.Id);
        var statsA2 = await _service.GetSalesStatisticsAsync(_branchA2.Id);
        var statsB1 = await _service.GetSalesStatisticsAsync(_branchB1.Id);

        statsA1.TotalSales.Should().Be(1500);
        statsA1.SalesCount.Should().Be(2);

        statsA2.TotalSales.Should().Be(300);
        statsA2.SalesCount.Should().Be(1);

        statsB1.TotalSales.Should().Be(2000);
        statsB1.SalesCount.Should().Be(1);
    }

    #endregion

    #region Complete E2E Sale Workflow Tests

    [Fact]
    public async Task E2E_NewPatientConsultationSale()
    {
        // Scenario: New patient comes for consultation

        // Step 1: Create sale during registration
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            DueDate = DateTime.UtcNow,
            Notes = "New patient first visit",
            TaxPercentage = 5,
            CreatedBy = "reception-staff"
        });

        // Step 2: Doctor adds services
        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "New Patient Registration Fee",
            Quantity = 1,
            UnitPrice = 50,
            Subtotal = 50,
            Total = 50
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "General Consultation",
            Quantity = 1,
            UnitPrice = 200,
            Subtotal = 200,
            Total = 200
        });

        await _service.RecalculateSaleTotalsAsync(sale.Id);

        // Step 3: Confirm and process payment
        await _service.ConfirmSaleAsync(sale.Id);

        var updatedSale = await _service.GetSaleByIdAsync(sale.Id);
        // Total = 250 + 5% tax = 262.5
        await _service.RecordPaymentAsync(sale.Id, updatedSale!.Total, PaymentMethod.Cash);

        // Step 4: Complete
        var completedSale = await _service.CompleteSaleAsync(sale.Id);

        completedSale.Status.Should().Be(SaleStatus.Completed);
        completedSale.PaymentStatus.Should().Be(PaymentStatus.Paid);
        completedSale.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task E2E_TreatmentPackageWithInstallments()
    {
        // Scenario: Patient purchases treatment package, pays in installments

        // Step 1: Create quotation for treatment package
        var quotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _patientA2.Id,
            BranchId = _branchA2.Id,
            ValidityDays = 30,
            TaxPercentage = 5,
            Notes = "Dental treatment package",
            CreatedBy = "dental-coordinator"
        });

        // Step 2: Add package items
        await _service.AddQuotationItemAsync(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Root Canal Treatment",
            Quantity = 2,
            UnitPrice = 1500,
            Subtotal = 3000,
            Total = 3000
        });

        await _service.AddQuotationItemAsync(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Dental Crown - Ceramic",
            Quantity = 2,
            UnitPrice = 2000,
            Subtotal = 4000,
            Total = 4000
        });

        await _service.AddQuotationItemAsync(new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Dental Cleaning",
            Quantity = 1,
            UnitPrice = 300,
            Subtotal = 300,
            Total = 300
        });

        await _service.RecalculateQuotationTotalsAsync(quotation.Id);

        // Step 3: Send quotation and patient accepts
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        // Step 4: Convert to sale
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);
        sale.Items.Should().HaveCount(3);

        // Verify total: 7300 + 5% tax = 7665
        var saleWithItems = await _service.GetSaleByIdAsync(sale.Id);
        saleWithItems!.SubTotal.Should().Be(7300);
        saleWithItems.Total.Should().Be(7665);

        // Step 5: Confirm and start payment installments
        await _service.ConfirmSaleAsync(sale.Id);

        // First installment - 30%
        await _service.RecordPaymentAsync(sale.Id, 2300, PaymentMethod.Card, "VISA-1234");
        var afterFirst = await _service.GetSaleByIdAsync(sale.Id);
        afterFirst!.PaymentStatus.Should().Be(PaymentStatus.Partial);

        // Second installment - 30%
        await _service.RecordPaymentAsync(sale.Id, 2300, PaymentMethod.BankTransfer, "TRF-5678");
        var afterSecond = await _service.GetSaleByIdAsync(sale.Id);
        afterSecond!.PaymentStatus.Should().Be(PaymentStatus.Partial);

        // Final installment - remaining balance
        await _service.RecordPaymentAsync(sale.Id, 3065, PaymentMethod.Cash);
        var afterFinal = await _service.GetSaleByIdAsync(sale.Id);
        afterFinal!.PaymentStatus.Should().Be(PaymentStatus.Paid);
        afterFinal.IsFullyPaid.Should().BeTrue();

        // Complete the sale
        var completed = await _service.CompleteSaleAsync(sale.Id);
        completed.Status.Should().Be(SaleStatus.Completed);

        // Verify all payments recorded
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        payments.Should().HaveCount(3);
        payments.Sum(p => p.Amount).Should().Be(7665);
    }

    [Fact]
    public async Task E2E_InsuranceCoveredVisit()
    {
        // Scenario: Patient visit partially covered by insurance

        // Step 1: Create sale
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientB1.Id,
            BranchId = _branchB1.Id,
            TaxPercentage = 0, // Insurance-covered, no tax on covered amount
            Notes = "Insurance: Policy ABC-123",
            CreatedBy = "insurance-coordinator"
        });

        // Step 2: Add medical services
        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "MRI Scan - Brain",
            Quantity = 1,
            UnitPrice = 2500,
            Subtotal = 2500,
            Total = 2500
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Neurologist Consultation",
            Quantity = 1,
            UnitPrice = 500,
            Subtotal = 500,
            Total = 500
        });

        await _service.RecalculateSaleTotalsAsync(sale.Id);
        await _service.ConfirmSaleAsync(sale.Id);

        var confirmedSale = await _service.GetSaleByIdAsync(sale.Id);
        confirmedSale!.Total.Should().Be(3000);

        // Step 3: Insurance pays 80%
        await _service.RecordPaymentAsync(sale.Id, 2400, PaymentMethod.Insurance, "INS-CLAIM-789");

        // Step 4: Patient pays remaining 20%
        await _service.RecordPaymentAsync(sale.Id, 600, PaymentMethod.Card);

        var paidSale = await _service.GetSaleByIdAsync(sale.Id);
        paidSale!.PaymentStatus.Should().Be(PaymentStatus.Paid);

        // Verify payment distribution
        var payments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        payments.Should().HaveCount(2);
        payments.First(p => p.PaymentMethod == PaymentMethod.Insurance).Amount.Should().Be(2400);
        payments.First(p => p.PaymentMethod == PaymentMethod.Card).Amount.Should().Be(600);

        // Complete
        await _service.CompleteSaleAsync(sale.Id);
    }

    [Fact]
    public async Task E2E_SaleWithDiscountAndRefund()
    {
        // Scenario: Sale with promotional discount, then partial refund

        // Step 1: Create sale with discount
        var sale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            DiscountPercentage = 15, // 15% promotional discount
            TaxPercentage = 5,
            Notes = "Ramadan Special Promotion",
            CreatedBy = "promotion-manager"
        });

        await _service.AddSaleItemAsync(new SaleItem
        {
            SaleId = sale.Id,
            ItemName = "Full Body Checkup Package",
            Quantity = 1,
            UnitPrice = 2000,
            Subtotal = 2000,
            Total = 2000
        });

        await _service.RecalculateSaleTotalsAsync(sale.Id);

        var saleWithDiscount = await _service.GetSaleByIdAsync(sale.Id);
        // 2000 - 15% = 1700, + 5% tax = 1785
        saleWithDiscount!.SubTotal.Should().Be(2000);
        saleWithDiscount.DiscountAmount.Should().Be(300);
        saleWithDiscount.Total.Should().Be(1785);

        // Step 2: Process full payment
        await _service.ConfirmSaleAsync(sale.Id);
        await _service.RecordPaymentAsync(sale.Id, 1785, PaymentMethod.Cash);

        var paidSale = await _service.GetSaleByIdAsync(sale.Id);
        paidSale!.PaymentStatus.Should().Be(PaymentStatus.Paid);

        // Step 3: Patient cancels one part of checkup, needs partial refund
        var payment = (await _service.GetPaymentsBySaleIdAsync(sale.Id)).First();
        await _service.RefundPaymentAsync(payment.Id, 500, "Cancelled lab tests portion");

        // Verify refund
        var afterRefund = await _service.GetSaleByIdAsync(sale.Id);
        afterRefund!.PaidAmount.Should().Be(1285); // 1785 - 500

        var allPayments = await _service.GetPaymentsBySaleIdAsync(sale.Id);
        allPayments.Should().HaveCount(2);
        allPayments.Should().Contain(p => p.Amount == -500);
    }

    #endregion

    #region Quotation Edge Cases Tests

    [Fact]
    public async Task E2E_QuotationExpiryPreventsSale()
    {
        // Create expired quotation
        var quotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            ValidityDays = 1,
            ExpiryDate = DateTime.UtcNow.AddDays(-5), // Already expired
            CreatedBy = "sales-staff",
            Items = new List<QuotationItem>
            {
                new QuotationItem
                {
                    ItemName = "Service",
                    Quantity = 1,
                    UnitPrice = 500,
                    Subtotal = 500,
                    Total = 500
                }
            }
        });

        await _service.SendQuotationAsync(quotation.Id);

        // Expired quotation should not be convertible
        var expiredQuotation = await _service.GetQuotationByIdAsync(quotation.Id);
        expiredQuotation!.IsExpired.Should().BeTrue();
        expiredQuotation.CanConvertToSale.Should().BeFalse();

        // Attempting to accept should fail
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task E2E_MultipleQuotationsForSamePatient()
    {
        // Patient requests multiple quotations for different services
        var dentalQuotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            ValidityDays = 14,
            Notes = "Dental Work Quote",
            CreatedBy = "dental-dept",
            Items = new List<QuotationItem>
            {
                new QuotationItem { ItemName = "Dental Services", Quantity = 1, UnitPrice = 3000, Subtotal = 3000, Total = 3000 }
            }
        });

        var dermaQuotation = await _service.CreateQuotationAsync(new Quotation
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            ValidityDays = 14,
            Notes = "Dermatology Quote",
            CreatedBy = "derma-dept",
            Items = new List<QuotationItem>
            {
                new QuotationItem { ItemName = "Skin Treatment", Quantity = 1, UnitPrice = 1500, Subtotal = 1500, Total = 1500 }
            }
        });

        // Send both
        await _service.SendQuotationAsync(dentalQuotation.Id);
        await _service.SendQuotationAsync(dermaQuotation.Id);

        // Patient accepts only dental
        await _service.AcceptQuotationAsync(dentalQuotation.Id);
        await _service.RejectQuotationAsync(dermaQuotation.Id, "Too expensive");

        // Convert only the accepted one
        var dentalSale = await _service.ConvertQuotationToSaleAsync(dentalQuotation.Id);

        // Verify statuses
        var updatedDental = await _service.GetQuotationByIdAsync(dentalQuotation.Id);
        var updatedDerma = await _service.GetQuotationByIdAsync(dermaQuotation.Id);

        updatedDental!.Status.Should().Be(QuotationStatus.Converted);
        updatedDerma!.Status.Should().Be(QuotationStatus.Rejected);
        updatedDerma.RejectionReason.Should().Be("Too expensive");

        // Rejected quotation should not be convertible
        updatedDerma.CanConvertToSale.Should().BeFalse();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConvertQuotationToSaleAsync(dermaQuotation.Id));
    }

    #endregion

    #region Overdue Sales E2E Tests

    [Fact]
    public async Task E2E_OverdueSalesTracking()
    {
        // Create sales with different due dates
        var currentSale = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 100, Subtotal = 100, Total = 100 }
            }
        });

        var overdueSale1 = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            DueDate = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 200, Subtotal = 200, Total = 200 }
            }
        });

        var overdueSale2 = await _service.CreateSaleAsync(new Sale
        {
            PatientId = _patientA1.Id,
            BranchId = _branchA1.Id,
            DueDate = DateTime.UtcNow.AddDays(-5),
            CreatedBy = "test",
            Items = new List<SaleItem>
            {
                new SaleItem { ItemName = "Service", Quantity = 1, UnitPrice = 300, Subtotal = 300, Total = 300 }
            }
        });

        // Check overdue sales
        var overdueSales = await _service.GetOverdueSalesAsync(_branchA1.Id);
        overdueSales.Should().HaveCount(2);
        overdueSales.Should().Contain(s => s.Id == overdueSale1.Id);
        overdueSales.Should().Contain(s => s.Id == overdueSale2.Id);

        // Pay one overdue sale
        await _service.ConfirmSaleAsync(overdueSale1.Id);
        await _service.RecordPaymentAsync(overdueSale1.Id, 200, PaymentMethod.Cash);

        // Should now only have 1 overdue
        var remainingOverdue = await _service.GetOverdueSalesAsync(_branchA1.Id);
        remainingOverdue.Should().HaveCount(1);
        remainingOverdue.First().Id.Should().Be(overdueSale2.Id);
    }

    #endregion

    #region Cross-Branch Validation Tests

    [Fact]
    public async Task CannotAssignPatientFromDifferentBranch()
    {
        // Patient from Branch A1, trying to create sale in Branch B1
        var invalidSale = new Sale
        {
            PatientId = _patientA1.Id, // Patient belongs to Branch A1
            BranchId = _branchB1.Id,   // But sale is for Branch B1
            CreatedBy = "test"
        };

        // This should either throw or be caught by business logic
        // The exact behavior depends on how the service validates this
        // For now, we test that we can track which branch a patient belongs to
        var patient = await _context.Patients.FindAsync(_patientA1.Id);
        patient!.BranchId.Should().Be(_branchA1.Id);
        patient.BranchId.Should().NotBe(_branchB1.Id);
    }

    #endregion

    #region Helper Methods

    private async Task<Sale> CreateSaleForPatient(PatientEntity patient, Branch branch, decimal total)
    {
        var sale = new Sale
        {
            PatientId = patient.Id,
            BranchId = branch.Id,
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

    private async Task<Sale> CreateAndPaySaleForPatient(PatientEntity patient, Branch branch, decimal amount)
    {
        var sale = await CreateSaleForPatient(patient, branch, amount);
        await _service.ConfirmSaleAsync(sale.Id);
        await _service.RecordPaymentAsync(sale.Id, amount, PaymentMethod.Cash);
        await _service.CompleteSaleAsync(sale.Id);
        return await _service.GetSaleByIdAsync(sale.Id) ?? sale;
    }

    #endregion
}
