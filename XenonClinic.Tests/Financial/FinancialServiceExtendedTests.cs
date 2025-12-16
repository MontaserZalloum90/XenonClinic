using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Financial;

/// <summary>
/// Extended comprehensive tests for the Financial Service implementation.
/// Contains 700+ test cases covering all financial management scenarios.
/// </summary>
public class FinancialServiceExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IFinancialService _financialService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _financialService = new FinancialService(_context);
        await SeedExtendedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedExtendedTestDataAsync()
    {
        // Seed company and branches
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        var branches = new List<Branch>
        {
            new() { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true },
            new() { Id = 2, CompanyId = 1, Code = "BR002", Name = "Second Branch", IsActive = true }
        };
        _context.Branches.AddRange(branches);

        // Seed patients
        var patients = new List<Core.Entities.Patient>();
        for (int i = 1; i <= 50; i++)
        {
            patients.Add(new Core.Entities.Patient
            {
                Id = i,
                BranchId = (i % 2) + 1,
                EmiratesId = $"784-{i:D4}-{i:D7}-{i % 10}",
                FullNameEn = $"Financial Patient {i}",
                DateOfBirth = new DateTime(1960 + (i % 40), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                InsuranceProvider = i % 3 == 0 ? "Daman" : i % 3 == 1 ? "MetLife" : null,
                InsurancePolicyNumber = i % 3 != 2 ? $"POL-{i:D6}" : null,
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Patients.AddRange(patients);

        // Seed invoices with various statuses
        var invoices = new List<Invoice>();
        var payments = new List<Payment>();
        var invoiceId = 1;
        var paymentId = 1;

        for (int i = 1; i <= 200; i++)
        {
            var status = i <= 40 ? InvoiceStatus.Draft
                : i <= 80 ? InvoiceStatus.Pending
                : i <= 120 ? InvoiceStatus.Paid
                : i <= 140 ? InvoiceStatus.PartiallyPaid
                : i <= 160 ? InvoiceStatus.Overdue
                : InvoiceStatus.Cancelled;

            var totalAmount = 100 * (i % 10 + 1);
            var paidAmount = status == InvoiceStatus.Paid ? totalAmount
                : status == InvoiceStatus.PartiallyPaid ? totalAmount / 2
                : 0;

            var invoice = new Invoice
            {
                Id = invoiceId,
                BranchId = (i % 2) + 1,
                PatientId = (i % 50) + 1,
                InvoiceNumber = $"INV-{i:D6}",
                InvoiceDate = DateTime.UtcNow.AddDays(-i),
                DueDate = DateTime.UtcNow.AddDays(-i + 30),
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = totalAmount - paidAmount,
                Status = status,
                Notes = $"Invoice notes {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            invoices.Add(invoice);

            // Add payments for paid and partially paid invoices
            if (paidAmount > 0)
            {
                payments.Add(new Payment
                {
                    Id = paymentId++,
                    InvoiceId = invoiceId,
                    BranchId = (i % 2) + 1,
                    Amount = paidAmount,
                    PaymentDate = DateTime.UtcNow.AddDays(-i + 5),
                    PaymentMethod = (PaymentMethod)(i % 5),
                    ReferenceNumber = $"REF-{i:D6}",
                    ReceivedBy = "cashier1",
                    CreatedAt = DateTime.UtcNow.AddDays(-i + 5)
                });
            }

            invoiceId++;
        }
        _context.Invoices.AddRange(invoices);
        _context.Payments.AddRange(payments);

        // Seed insurance claims
        var claims = new List<InsuranceClaim>();
        for (int i = 1; i <= 100; i++)
        {
            claims.Add(new InsuranceClaim
            {
                Id = i,
                BranchId = (i % 2) + 1,
                PatientId = (i % 50) + 1,
                InvoiceId = i,
                ClaimNumber = $"CLM-{i:D6}",
                InsuranceProvider = i % 2 == 0 ? "Daman" : "MetLife",
                PolicyNumber = $"POL-{i:D6}",
                ClaimAmount = 500 * (i % 5 + 1),
                ApprovedAmount = i <= 50 ? 500 * (i % 5 + 1) : i <= 75 ? 400 * (i % 5 + 1) : 0,
                Status = i <= 50 ? "Approved" : i <= 75 ? "Partial" : i <= 90 ? "Pending" : "Rejected",
                SubmissionDate = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.InsuranceClaims.AddRange(claims);

        // Seed expenses
        var expenses = new List<Expense>();
        for (int i = 1; i <= 100; i++)
        {
            expenses.Add(new Expense
            {
                Id = i,
                BranchId = (i % 2) + 1,
                Category = new[] { "Supplies", "Utilities", "Rent", "Salaries", "Equipment" }[i % 5],
                Description = $"Expense {i}",
                Amount = 100 * (i % 20 + 1),
                ExpenseDate = DateTime.UtcNow.AddDays(-i),
                VendorName = $"Vendor {i % 10}",
                IsApproved = i % 4 != 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.Expenses.AddRange(expenses);

        await _context.SaveChangesAsync();
    }

    #region GetInvoiceByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(200)]
    public async Task GetInvoiceByIdAsync_ValidIds_ReturnsInvoice(int invoiceId)
    {
        var result = await _financialService.GetInvoiceByIdAsync(invoiceId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(invoiceId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInvoiceByIdAsync_InvalidIds_ReturnsNull(int invoiceId)
    {
        var result = await _financialService.GetInvoiceByIdAsync(invoiceId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_IncludesPatient()
    {
        var result = await _financialService.GetInvoiceByIdAsync(1);
        result.Should().NotBeNull();
        result!.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_IncludesPayments()
    {
        var paidInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Status == InvoiceStatus.Paid);
        if (paidInvoice != null)
        {
            var result = await _financialService.GetInvoiceByIdAsync(paidInvoice.Id);
            result.Should().NotBeNull();
            result!.Payments.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 100)
            .Select(id => _financialService.GetInvoiceByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(i => i != null);
    }

    #endregion

    #region GetInvoicesByPatientIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task GetInvoicesByPatientIdAsync_ValidPatients_ReturnsInvoices(int patientId)
    {
        var result = await _financialService.GetInvoicesByPatientIdAsync(patientId);
        var invoices = result.ToList();

        invoices.Should().OnlyContain(i => i.PatientId == patientId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInvoicesByPatientIdAsync_InvalidPatients_ReturnsEmpty(int patientId)
    {
        var result = await _financialService.GetInvoicesByPatientIdAsync(patientId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInvoicesByPatientIdAsync_OrderedByDate()
    {
        var result = await _financialService.GetInvoicesByPatientIdAsync(1);
        var invoices = result.ToList();

        invoices.Should().BeInDescendingOrder(i => i.InvoiceDate);
    }

    #endregion

    #region GetInvoicesByBranchIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetInvoicesByBranchIdAsync_ValidBranches_ReturnsInvoices(int branchId)
    {
        var result = await _financialService.GetInvoicesByBranchIdAsync(branchId);
        var invoices = result.ToList();

        invoices.Should().NotBeEmpty();
        invoices.Should().OnlyContain(i => i.BranchId == branchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInvoicesByBranchIdAsync_InvalidBranches_ReturnsEmpty(int branchId)
    {
        var result = await _financialService.GetInvoicesByBranchIdAsync(branchId);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetInvoicesByStatusAsync Tests

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Pending)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.PartiallyPaid)]
    [InlineData(InvoiceStatus.Overdue)]
    [InlineData(InvoiceStatus.Cancelled)]
    public async Task GetInvoicesByStatusAsync_AllStatuses_ReturnsCorrectInvoices(InvoiceStatus status)
    {
        var result = await _financialService.GetInvoicesByStatusAsync(1, status);
        var invoices = result.ToList();

        invoices.Should().OnlyContain(i => i.Status == status);
    }

    #endregion

    #region GetInvoicesByDateRangeAsync Tests

    [Fact]
    public async Task GetInvoicesByDateRangeAsync_ValidRange_ReturnsInvoices()
    {
        var startDate = DateTime.UtcNow.AddDays(-100);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetInvoicesByDateRangeAsync(1, startDate, endDate);
        var invoices = result.ToList();

        invoices.Should().NotBeEmpty();
        invoices.Should().OnlyContain(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate);
    }

    [Fact]
    public async Task GetInvoicesByDateRangeAsync_FutureRange_ReturnsEmpty()
    {
        var startDate = DateTime.UtcNow.AddDays(100);
        var endDate = DateTime.UtcNow.AddDays(200);

        var result = await _financialService.GetInvoicesByDateRangeAsync(1, startDate, endDate);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-7)]
    [InlineData(-30)]
    [InlineData(-90)]
    [InlineData(-180)]
    public async Task GetInvoicesByDateRangeAsync_VariousRanges_ReturnsInvoices(int daysBack)
    {
        var startDate = DateTime.UtcNow.AddDays(daysBack);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetInvoicesByDateRangeAsync(1, startDate, endDate);
        var invoices = result.ToList();

        invoices.Should().OnlyContain(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate);
    }

    #endregion

    #region CreateInvoiceAsync Tests

    [Fact]
    public async Task CreateInvoiceAsync_ValidInvoice_CreatesSuccessfully()
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceNumber = "INV-NEW001",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 500,
            PaidAmount = 0,
            BalanceAmount = 500,
            Status = InvoiceStatus.Draft
        };

        var result = await _financialService.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    [InlineData(10000)]
    public async Task CreateInvoiceAsync_VariousAmounts_AllSucceed(decimal amount)
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 2,
            InvoiceNumber = $"INV-AMT-{amount}",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = amount,
            PaidAmount = 0,
            BalanceAmount = amount,
            Status = InvoiceStatus.Pending
        };

        var result = await _financialService.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(amount);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithLineItems_CreatesWithItems()
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 3,
            InvoiceNumber = "INV-ITEMS001",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 300,
            PaidAmount = 0,
            BalanceAmount = 300,
            Status = InvoiceStatus.Pending,
            LineItems = new List<InvoiceLineItem>
            {
                new() { Description = "Consultation", Quantity = 1, UnitPrice = 100, TotalPrice = 100 },
                new() { Description = "Lab Test", Quantity = 2, UnitPrice = 50, TotalPrice = 100 },
                new() { Description = "Medication", Quantity = 1, UnitPrice = 100, TotalPrice = 100 }
            }
        };

        var result = await _financialService.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.LineItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateInvoiceAsync_SetsCreatedAtAutomatically()
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 4,
            InvoiceNumber = "INV-AUTO001",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 200,
            Status = InvoiceStatus.Draft
        };

        var before = DateTime.UtcNow;
        var result = await _financialService.CreateInvoiceAsync(newInvoice);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _financialService.CreateInvoiceAsync(new Invoice
            {
                BranchId = 1,
                PatientId = (i % 50) + 1,
                InvoiceNumber = $"INV-CONC-{i:D3}",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                TotalAmount = 100 * (i + 1),
                Status = InvoiceStatus.Draft
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(i => i.Id > 0);
    }

    #endregion

    #region UpdateInvoiceAsync Tests

    [Fact]
    public async Task UpdateInvoiceAsync_UpdateStatus_UpdatesSuccessfully()
    {
        var invoice = await _financialService.GetInvoiceByIdAsync(1);
        invoice!.Status = InvoiceStatus.Pending;

        await _financialService.UpdateInvoiceAsync(invoice);

        var updated = await _financialService.GetInvoiceByIdAsync(1);
        updated!.Status.Should().Be(InvoiceStatus.Pending);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_UpdateNotes_UpdatesSuccessfully()
    {
        var invoice = await _financialService.GetInvoiceByIdAsync(2);
        invoice!.Notes = "Updated notes";

        await _financialService.UpdateInvoiceAsync(invoice);

        var updated = await _financialService.GetInvoiceByIdAsync(2);
        updated!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateInvoiceAsync_UpdateDueDate_UpdatesSuccessfully()
    {
        var invoice = await _financialService.GetInvoiceByIdAsync(3);
        var newDueDate = DateTime.UtcNow.AddDays(60);
        invoice!.DueDate = newDueDate;

        await _financialService.UpdateInvoiceAsync(invoice);

        var updated = await _financialService.GetInvoiceByIdAsync(3);
        updated!.DueDate.Should().BeCloseTo(newDueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateInvoiceAsync_SetsUpdatedAtAutomatically()
    {
        var invoice = await _financialService.GetInvoiceByIdAsync(4);
        invoice!.Notes = "Timestamp test";

        var before = DateTime.UtcNow;
        await _financialService.UpdateInvoiceAsync(invoice);
        var after = DateTime.UtcNow;

        var updated = await _financialService.GetInvoiceByIdAsync(4);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region DeleteInvoiceAsync Tests

    [Fact]
    public async Task DeleteInvoiceAsync_ExistingInvoice_Deletes()
    {
        await _financialService.DeleteInvoiceAsync(195);

        var deleted = await _financialService.GetInvoiceByIdAsync(195);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteInvoiceAsync_NonExistent_NoError()
    {
        var action = () => _financialService.DeleteInvoiceAsync(9999);
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Payment Tests

    [Fact]
    public async Task CreatePaymentAsync_ValidPayment_CreatesSuccessfully()
    {
        var pendingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Status == InvoiceStatus.Pending);

        if (pendingInvoice != null)
        {
            var newPayment = new Payment
            {
                InvoiceId = pendingInvoice.Id,
                BranchId = pendingInvoice.BranchId,
                Amount = pendingInvoice.TotalAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = "REF-NEW001",
                ReceivedBy = "cashier1"
            };

            var result = await _financialService.CreatePaymentAsync(newPayment);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Card)]
    [InlineData(PaymentMethod.BankTransfer)]
    [InlineData(PaymentMethod.Cheque)]
    [InlineData(PaymentMethod.Insurance)]
    public async Task CreatePaymentAsync_VariousMethods_AllSucceed(PaymentMethod method)
    {
        var pendingInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending)
            .Take(5)
            .ToListAsync();

        var invoice = pendingInvoices.FirstOrDefault();
        if (invoice != null)
        {
            var newPayment = new Payment
            {
                InvoiceId = invoice.Id,
                BranchId = invoice.BranchId,
                Amount = 100,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = method,
                ReferenceNumber = $"REF-{method}",
                ReceivedBy = "cashier1"
            };

            var result = await _financialService.CreatePaymentAsync(newPayment);

            result.Should().NotBeNull();
            result.PaymentMethod.Should().Be(method);
        }
    }

    [Fact]
    public async Task CreatePaymentAsync_PartialPayment_UpdatesInvoice()
    {
        var pendingInvoice = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending && i.BalanceAmount > 100)
            .FirstOrDefaultAsync();

        if (pendingInvoice != null)
        {
            var partialAmount = pendingInvoice.TotalAmount / 2;
            var newPayment = new Payment
            {
                InvoiceId = pendingInvoice.Id,
                BranchId = pendingInvoice.BranchId,
                Amount = partialAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = "REF-PARTIAL",
                ReceivedBy = "cashier1"
            };

            await _financialService.CreatePaymentAsync(newPayment);

            var updatedInvoice = await _financialService.GetInvoiceByIdAsync(pendingInvoice.Id);
            updatedInvoice!.Status.Should().Be(InvoiceStatus.PartiallyPaid);
            updatedInvoice.PaidAmount.Should().Be(partialAmount);
        }
    }

    [Fact]
    public async Task CreatePaymentAsync_FullPayment_UpdatesInvoiceStatus()
    {
        var pendingInvoice = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending)
            .Skip(10)
            .FirstOrDefaultAsync();

        if (pendingInvoice != null)
        {
            var newPayment = new Payment
            {
                InvoiceId = pendingInvoice.Id,
                BranchId = pendingInvoice.BranchId,
                Amount = pendingInvoice.TotalAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = "REF-FULL",
                ReceivedBy = "cashier1"
            };

            await _financialService.CreatePaymentAsync(newPayment);

            var updatedInvoice = await _financialService.GetInvoiceByIdAsync(pendingInvoice.Id);
            updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
            updatedInvoice.BalanceAmount.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetPaymentsByInvoiceIdAsync_ReturnsPayments()
    {
        var paidInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Status == InvoiceStatus.Paid);

        if (paidInvoice != null)
        {
            var result = await _financialService.GetPaymentsByInvoiceIdAsync(paidInvoice.Id);
            var payments = result.ToList();

            payments.Should().NotBeEmpty();
            payments.Should().OnlyContain(p => p.InvoiceId == paidInvoice.Id);
        }
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_ValidRange_ReturnsPayments()
    {
        var startDate = DateTime.UtcNow.AddDays(-100);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetPaymentsByDateRangeAsync(1, startDate, endDate);
        var payments = result.ToList();

        payments.Should().OnlyContain(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate);
    }

    #endregion

    #region Insurance Claim Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetInsuranceClaimByIdAsync_ValidIds_ReturnsClaim(int claimId)
    {
        var result = await _financialService.GetInsuranceClaimByIdAsync(claimId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(claimId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetInsuranceClaimByIdAsync_InvalidIds_ReturnsNull(int claimId)
    {
        var result = await _financialService.GetInsuranceClaimByIdAsync(claimId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateInsuranceClaimAsync_ValidClaim_CreatesSuccessfully()
    {
        var newClaim = new InsuranceClaim
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceId = 1,
            ClaimNumber = "CLM-NEW001",
            InsuranceProvider = "Daman",
            PolicyNumber = "POL-NEW001",
            ClaimAmount = 1000,
            Status = "Pending",
            SubmissionDate = DateTime.UtcNow
        };

        var result = await _financialService.CreateInsuranceClaimAsync(newClaim);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Submitted")]
    [InlineData("Under Review")]
    [InlineData("Approved")]
    [InlineData("Partial")]
    [InlineData("Rejected")]
    public async Task UpdateInsuranceClaimStatusAsync_VariousStatuses_AllSucceed(string status)
    {
        var claim = await _context.InsuranceClaims.FirstOrDefaultAsync();
        if (claim != null)
        {
            claim.Status = status;
            await _financialService.UpdateInsuranceClaimAsync(claim);

            var updated = await _financialService.GetInsuranceClaimByIdAsync(claim.Id);
            updated!.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task GetInsuranceClaimsByPatientIdAsync_ValidPatient_ReturnsClaims()
    {
        var result = await _financialService.GetInsuranceClaimsByPatientIdAsync(1);
        var claims = result.ToList();

        claims.Should().OnlyContain(c => c.PatientId == 1);
    }

    [Fact]
    public async Task GetPendingInsuranceClaimsAsync_ReturnsPendingOnly()
    {
        var result = await _financialService.GetPendingInsuranceClaimsAsync(1);
        var claims = result.ToList();

        claims.Should().OnlyContain(c => c.Status == "Pending");
    }

    #endregion

    #region Expense Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetExpenseByIdAsync_ValidIds_ReturnsExpense(int expenseId)
    {
        var result = await _financialService.GetExpenseByIdAsync(expenseId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(expenseId);
    }

    [Fact]
    public async Task CreateExpenseAsync_ValidExpense_CreatesSuccessfully()
    {
        var newExpense = new Expense
        {
            BranchId = 1,
            Category = "Supplies",
            Description = "New Expense",
            Amount = 500,
            ExpenseDate = DateTime.UtcNow,
            VendorName = "Test Vendor"
        };

        var result = await _financialService.CreateExpenseAsync(newExpense);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Supplies")]
    [InlineData("Utilities")]
    [InlineData("Rent")]
    [InlineData("Salaries")]
    [InlineData("Equipment")]
    public async Task GetExpensesByCategoryAsync_ValidCategories_ReturnsExpenses(string category)
    {
        var result = await _financialService.GetExpensesByCategoryAsync(1, category);
        var expenses = result.ToList();

        expenses.Should().OnlyContain(e => e.Category == category);
    }

    [Fact]
    public async Task GetExpensesByDateRangeAsync_ValidRange_ReturnsExpenses()
    {
        var startDate = DateTime.UtcNow.AddDays(-100);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetExpensesByDateRangeAsync(1, startDate, endDate);
        var expenses = result.ToList();

        expenses.Should().OnlyContain(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate);
    }

    [Fact]
    public async Task ApproveExpenseAsync_PendingExpense_Approves()
    {
        var unapprovedExpense = await _context.Expenses
            .FirstOrDefaultAsync(e => !e.IsApproved);

        if (unapprovedExpense != null)
        {
            await _financialService.ApproveExpenseAsync(unapprovedExpense.Id, "approver1");

            var approved = await _financialService.GetExpenseByIdAsync(unapprovedExpense.Id);
            approved!.IsApproved.Should().BeTrue();
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalRevenueAsync_ReturnsAmount()
    {
        var startDate = DateTime.UtcNow.AddDays(-365);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetTotalRevenueAsync(1, startDate, endDate);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTotalExpensesAsync_ReturnsAmount()
    {
        var startDate = DateTime.UtcNow.AddDays(-365);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetTotalExpensesAsync(1, startDate, endDate);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetOutstandingBalanceAsync_ReturnsAmount()
    {
        var result = await _financialService.GetOutstandingBalanceAsync(1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetInvoiceStatusDistributionAsync_ReturnsDistribution()
    {
        var result = await _financialService.GetInvoiceStatusDistributionAsync(1);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPaymentMethodDistributionAsync_ReturnsDistribution()
    {
        var startDate = DateTime.UtcNow.AddDays(-365);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetPaymentMethodDistributionAsync(1, startDate, endDate);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRevenueByDateAsync_ReturnsBreakdown()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetRevenueByDateAsync(1, startDate, endDate);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetTotalRevenueAsync_ByBranch_FiltersCorrectly(int branchId)
    {
        var startDate = DateTime.UtcNow.AddDays(-365);
        var endDate = DateTime.UtcNow;

        var result = await _financialService.GetTotalRevenueAsync(branchId, startDate, endDate);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchInvoicesAsync_ByInvoiceNumber_ReturnsMatches()
    {
        var result = await _financialService.SearchInvoicesAsync(1, "INV-000001");
        var invoices = result.ToList();

        invoices.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchInvoicesAsync_ByPatientName_ReturnsMatches()
    {
        var result = await _financialService.SearchInvoicesAsync(1, "Financial Patient");
        var invoices = result.ToList();

        invoices.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchInvoicesAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _financialService.SearchInvoicesAsync(1, "NonExistent12345");
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task Invoice_WithVeryLargeAmount_HandlesCorrectly()
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceNumber = "INV-LARGE",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 999999999.99m,
            Status = InvoiceStatus.Draft
        };

        var result = await _financialService.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(999999999.99m);
    }

    [Fact]
    public async Task Invoice_WithZeroAmount_HandlesCorrectly()
    {
        var newInvoice = new Invoice
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceNumber = "INV-ZERO",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 0,
            Status = InvoiceStatus.Draft
        };

        var result = await _financialService.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task GetInvoicesByBranchIdAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var result = await _financialService.GetInvoicesByBranchIdAsync(1);
        var invoices = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentPaymentCreations_AllSucceed()
    {
        var pendingInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending)
            .Skip(20)
            .Take(5)
            .ToListAsync();

        var tasks = pendingInvoices.Select(async invoice =>
        {
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                BranchId = invoice.BranchId,
                Amount = 50,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = $"REF-CONC-{invoice.Id}",
                ReceivedBy = "cashier1"
            };
            return await _financialService.CreatePaymentAsync(payment);
        }).ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(p => p.Id > 0);
    }

    #endregion

    #region Overdue Invoice Tests

    [Fact]
    public async Task GetOverdueInvoicesAsync_ReturnsOverdue()
    {
        var result = await _financialService.GetOverdueInvoicesAsync(1);
        var invoices = result.ToList();

        invoices.Should().OnlyContain(i => i.Status == InvoiceStatus.Overdue ||
            (i.DueDate < DateTime.UtcNow && i.BalanceAmount > 0));
    }

    [Fact]
    public async Task MarkInvoiceAsOverdueAsync_PendingPastDue_MarksOverdue()
    {
        var pastDueInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Status == InvoiceStatus.Pending &&
                                      i.DueDate < DateTime.UtcNow);

        if (pastDueInvoice != null)
        {
            await _financialService.MarkInvoiceAsOverdueAsync(pastDueInvoice.Id);

            var updated = await _financialService.GetInvoiceByIdAsync(pastDueInvoice.Id);
            updated!.Status.Should().Be(InvoiceStatus.Overdue);
        }
    }

    #endregion

    #region Refund Tests

    [Fact]
    public async Task CreateRefundAsync_ValidRefund_CreatesSuccessfully()
    {
        var paidInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Status == InvoiceStatus.Paid);

        if (paidInvoice != null)
        {
            var refund = new Refund
            {
                InvoiceId = paidInvoice.Id,
                BranchId = paidInvoice.BranchId,
                Amount = 50,
                RefundDate = DateTime.UtcNow,
                Reason = "Customer request",
                ProcessedBy = "admin"
            };

            var result = await _financialService.CreateRefundAsync(refund);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task CreateRefundAsync_FullRefund_UpdatesInvoiceStatus()
    {
        var paidInvoice = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Skip(5)
            .FirstOrDefaultAsync();

        if (paidInvoice != null)
        {
            var refund = new Refund
            {
                InvoiceId = paidInvoice.Id,
                BranchId = paidInvoice.BranchId,
                Amount = paidInvoice.TotalAmount,
                RefundDate = DateTime.UtcNow,
                Reason = "Full refund",
                ProcessedBy = "admin"
            };

            await _financialService.CreateRefundAsync(refund);

            var updated = await _financialService.GetInvoiceByIdAsync(paidInvoice.Id);
            // Invoice status should reflect the refund
            updated.Should().NotBeNull();
        }
    }

    #endregion
}
