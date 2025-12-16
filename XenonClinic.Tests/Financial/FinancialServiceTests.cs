using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Tests.Financial;

/// <summary>
/// Unit tests for FinancialService covering all financial operations.
/// </summary>
public class FinancialServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly FinancialService _service;
    private readonly Branch _testBranch;
    private readonly Patient _testPatient;
    private readonly Mock<ISequenceGenerator> _mockSequenceGenerator;

    public FinancialServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _mockSequenceGenerator = new Mock<ISequenceGenerator>();
        _mockSequenceGenerator.Setup(x => x.GenerateInvoiceNumberAsync(It.IsAny<int>()))
            .ReturnsAsync(() => $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}");
        _service = new FinancialService(_context, _mockSequenceGenerator.Object);

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

    #region Account Tests

    [Fact]
    public async Task GetAccountByIdAsync_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var account = await CreateTestAccount();

        // Act
        var result = await _service.GetAccountByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
    }

    [Fact]
    public async Task GetAccountByIdAsync_NonExistentAccount_ReturnsNull()
    {
        // Act
        var result = await _service.GetAccountByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountsByBranchIdAsync_ReturnsFilteredAccounts()
    {
        // Arrange
        await CreateTestAccount();
        await CreateTestAccount();

        // Act
        var result = await _service.GetAccountsByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAccountsByTypeAsync_ReturnsFilteredAccounts()
    {
        // Arrange
        var assetAccount = await CreateTestAccount(AccountType.Asset);
        var liabilityAccount = await CreateTestAccount(AccountType.Liability);

        // Act
        var result = await _service.GetAccountsByTypeAsync(1, AccountType.Asset);

        // Assert
        result.Should().HaveCount(1);
        result.First().AccountType.Should().Be(AccountType.Asset);
    }

    [Fact]
    public async Task CreateAccountAsync_ValidAccount_CreatesAccount()
    {
        // Arrange
        var account = new Account
        {
            BranchId = 1,
            AccountCode = "ACC-001",
            AccountName = "Test Account",
            AccountType = AccountType.Asset,
            Balance = 0,
            IsActive = true
        };

        // Act
        var result = await _service.CreateAccountAsync(account);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAccountBalanceAsync_ReturnsCorrectBalance()
    {
        // Arrange
        var account = await CreateTestAccount();
        account.Balance = 1500.50m;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAccountBalanceAsync(account.Id);

        // Assert
        result.Should().Be(1500.50m);
    }

    #endregion

    #region Invoice Tests

    [Fact]
    public async Task GetInvoiceByIdAsync_ExistingInvoice_ReturnsInvoice()
    {
        // Arrange
        var invoice = await CreateTestInvoice();

        // Act
        var result = await _service.GetInvoiceByIdAsync(invoice.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invoice.Id);
    }

    [Fact]
    public async Task GetInvoiceByNumberAsync_ExistingNumber_ReturnsInvoice()
    {
        // Arrange
        var invoice = await CreateTestInvoice("INV-001");

        // Act
        var result = await _service.GetInvoiceByNumberAsync("INV-001");

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be("INV-001");
    }

    [Fact]
    public async Task GetInvoicesByBranchIdAsync_ReturnsFilteredInvoices()
    {
        // Arrange
        await CreateTestInvoice();
        await CreateTestInvoice();

        // Act
        var result = await _service.GetInvoicesByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInvoicesByStatusAsync_ReturnsFilteredInvoices()
    {
        // Arrange
        var paidInvoice = await CreateTestInvoice();
        paidInvoice.Status = InvoiceStatus.Paid;

        var pendingInvoice = await CreateTestInvoice();
        pendingInvoice.Status = InvoiceStatus.Pending;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetInvoicesByStatusAsync(1, InvoiceStatus.Paid);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ValidInvoice_CreatesInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceNumber = "INV-NEW-001",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            SubTotal = 100,
            TotalAmount = 100
        };

        // Act
        var result = await _service.CreateInvoiceAsync(invoice);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateInvoiceNumberAsync_ReturnsUniqueNumber()
    {
        // Act
        var result = await _service.GenerateInvoiceNumberAsync(1);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("INV-");
    }

    [Fact]
    public async Task GetUnpaidInvoicesCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var unpaidInvoice = await CreateTestInvoice();
        unpaidInvoice.Status = InvoiceStatus.Pending;

        var paidInvoice = await CreateTestInvoice();
        paidInvoice.Status = InvoiceStatus.Paid;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUnpaidInvoicesCountAsync(1);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetUnpaidInvoicesTotalAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var invoice1 = await CreateTestInvoice();
        invoice1.Status = InvoiceStatus.Pending;
        invoice1.TotalAmount = 100;
        invoice1.PaidAmount = 30;

        var invoice2 = await CreateTestInvoice();
        invoice2.Status = InvoiceStatus.Pending;
        invoice2.TotalAmount = 200;
        invoice2.PaidAmount = 0;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUnpaidInvoicesTotalAsync(1);

        // Assert
        result.Should().Be(270); // (100-30) + 200
    }

    #endregion

    #region Expense Tests

    [Fact]
    public async Task GetExpenseByIdAsync_ExistingExpense_ReturnsExpense()
    {
        // Arrange
        var expense = await CreateTestExpense();

        // Act
        var result = await _service.GetExpenseByIdAsync(expense.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expense.Id);
    }

    [Fact]
    public async Task GetExpensesByBranchIdAsync_ReturnsFilteredExpenses()
    {
        // Arrange
        await CreateTestExpense();
        await CreateTestExpense();

        // Act
        var result = await _service.GetExpensesByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateExpenseAsync_ValidExpense_CreatesExpense()
    {
        // Arrange
        var expense = new Expense
        {
            BranchId = 1,
            ExpenseNumber = "EXP-001",
            ExpenseDate = DateTime.UtcNow,
            Description = "Test Expense",
            Amount = 500,
            Status = ExpenseStatus.Pending
        };

        // Act
        var result = await _service.CreateExpenseAsync(expense);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPendingExpensesAsync_ReturnsPendingExpenses()
    {
        // Arrange
        var pendingExpense = await CreateTestExpense();
        pendingExpense.Status = ExpenseStatus.Pending;

        var approvedExpense = await CreateTestExpense();
        approvedExpense.Status = ExpenseStatus.Approved;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingExpensesAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ReturnsFilteredTransactions()
    {
        // Arrange
        var account = await CreateTestAccount();
        await CreateTestTransaction(account.Id);
        await CreateTestTransaction(account.Id);

        // Act
        var result = await _service.GetTransactionsByAccountIdAsync(account.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_ReturnsFilteredTransactions()
    {
        // Arrange
        var account = await CreateTestAccount();
        await CreateTestTransaction(account.Id);

        // Act
        var result = await _service.GetTransactionsByDateRangeAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidTransaction_CreatesTransaction()
    {
        // Arrange
        var account = await CreateTestAccount();
        var transaction = new FinancialTransaction
        {
            BranchId = 1,
            AccountId = account.Id,
            TransactionNumber = "TXN-001",
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.Debit,
            Amount = 100,
            Description = "Test Transaction"
        };

        // Act
        var result = await _service.CreateTransactionAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalRevenueAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var paidInvoice1 = await CreateTestInvoice();
        paidInvoice1.Status = InvoiceStatus.Paid;
        paidInvoice1.TotalAmount = 500;

        var paidInvoice2 = await CreateTestInvoice();
        paidInvoice2.Status = InvoiceStatus.Paid;
        paidInvoice2.TotalAmount = 300;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalRevenueAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().Be(800);
    }

    [Fact]
    public async Task GetTotalExpensesAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var expense1 = await CreateTestExpense();
        expense1.Amount = 200;
        expense1.Status = ExpenseStatus.Approved;

        var expense2 = await CreateTestExpense();
        expense2.Amount = 150;
        expense2.Status = ExpenseStatus.Approved;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalExpensesAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().Be(350);
    }

    [Fact]
    public async Task GetNetProfitAsync_ReturnsCorrectProfit()
    {
        // Arrange
        var invoice = await CreateTestInvoice();
        invoice.Status = InvoiceStatus.Paid;
        invoice.TotalAmount = 1000;

        var expense = await CreateTestExpense();
        expense.Amount = 400;
        expense.Status = ExpenseStatus.Approved;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetNetProfitAsync(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().Be(600); // 1000 - 400
    }

    #endregion

    #region Helper Methods

    private async Task<Account> CreateTestAccount(AccountType type = AccountType.Asset)
    {
        var account = new Account
        {
            BranchId = 1,
            AccountCode = $"ACC-{Guid.NewGuid().ToString()[..4]}",
            AccountName = "Test Account",
            AccountType = type,
            Balance = 0,
            IsActive = true
        };

        return await _service.CreateAccountAsync(account);
    }

    private async Task<Invoice> CreateTestInvoice(string? invoiceNumber = null)
    {
        var invoice = new Invoice
        {
            BranchId = 1,
            PatientId = 1,
            InvoiceNumber = invoiceNumber ?? $"INV-{Guid.NewGuid().ToString()[..4]}",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            SubTotal = 100,
            TotalAmount = 100
        };

        return await _service.CreateInvoiceAsync(invoice);
    }

    private async Task<Expense> CreateTestExpense()
    {
        var expense = new Expense
        {
            BranchId = 1,
            ExpenseNumber = $"EXP-{Guid.NewGuid().ToString()[..4]}",
            ExpenseDate = DateTime.UtcNow,
            Description = "Test Expense",
            Amount = 100,
            Status = ExpenseStatus.Pending
        };

        return await _service.CreateExpenseAsync(expense);
    }

    private async Task<FinancialTransaction> CreateTestTransaction(int accountId)
    {
        var transaction = new FinancialTransaction
        {
            BranchId = 1,
            AccountId = accountId,
            TransactionNumber = $"TXN-{Guid.NewGuid().ToString()[..4]}",
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.Credit,
            Amount = 100,
            Description = "Test Transaction"
        };

        return await _service.CreateTransactionAsync(transaction);
    }

    #endregion
}
