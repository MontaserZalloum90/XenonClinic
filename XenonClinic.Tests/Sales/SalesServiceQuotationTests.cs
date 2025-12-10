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
/// Unit tests for SalesService - Quotation workflow operations
/// </summary>
public class SalesServiceQuotationTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly SalesService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;

    public SalesServiceQuotationTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"QuotationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();

        _sequenceGeneratorMock.Setup(s => s.GenerateSequenceAsync(
                It.IsAny<int>(), "QT", SequenceType.Quotation, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}");

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

    #region Create Quotation Tests

    [Fact]
    public async Task CreateQuotationAsync_WithValidData_CreatesQuotation()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            Notes = "Test quotation",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateQuotationAsync(quotation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.QuotationNumber.Should().StartWith("QT-");
        result.Status.Should().Be(QuotationStatus.Draft);
    }

    [Fact]
    public async Task CreateQuotationAsync_SetsExpiryDate_BasedOnValidityDays()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 14,
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateQuotationAsync(quotation);

        // Assert
        result.ExpiryDate.Should().NotBeNull();
        result.ExpiryDate!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(14), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateQuotationAsync_WithInvalidPatient_ThrowsException()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = 9999, // Non-existent
            BranchId = _testBranchId,
            CreatedBy = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateQuotationAsync(quotation));
    }

    [Fact]
    public async Task CreateQuotationAsync_GeneratesNumber_WhenNotProvided()
    {
        // Arrange
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateQuotationAsync(quotation);

        // Assert
        result.QuotationNumber.Should().NotBeNullOrEmpty();
        _sequenceGeneratorMock.Verify(
            s => s.GenerateSequenceAsync(_testBranchId, "QT", SequenceType.Quotation, It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region Read Quotation Tests

    [Fact]
    public async Task GetQuotationByIdAsync_ReturnsQuotation()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act
        var result = await _service.GetQuotationByIdAsync(quotation.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(quotation.Id);
    }

    [Fact]
    public async Task GetQuotationByNumberAsync_ReturnsQuotation()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act
        var result = await _service.GetQuotationByNumberAsync(quotation.QuotationNumber);

        // Assert
        result.Should().NotBeNull();
        result!.QuotationNumber.Should().Be(quotation.QuotationNumber);
    }

    [Fact]
    public async Task GetQuotationsByBranchIdAsync_ReturnsOnlyBranchQuotations()
    {
        // Arrange
        await CreateTestQuotation();
        await CreateTestQuotation();

        // Act
        var result = await _service.GetQuotationsByBranchIdAsync(_testBranchId);

        // Assert
        result.Should().HaveCount(2);
        result.All(q => q.BranchId == _testBranchId).Should().BeTrue();
    }

    [Fact]
    public async Task GetQuotationsByPatientIdAsync_ReturnsOnlyPatientQuotations()
    {
        // Arrange
        await CreateTestQuotation();
        await CreateTestQuotation();

        // Act
        var result = await _service.GetQuotationsByPatientIdAsync(_testPatientId);

        // Assert
        result.Should().HaveCount(2);
        result.All(q => q.PatientId == _testPatientId).Should().BeTrue();
    }

    [Fact]
    public async Task GetQuotationsByStatusAsync_FiltersCorrectly()
    {
        // Arrange
        var draft = await CreateTestQuotation();
        var sent = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(sent.Id);

        // Act
        var draftQuotations = await _service.GetQuotationsByStatusAsync(_testBranchId, QuotationStatus.Draft);
        var sentQuotations = await _service.GetQuotationsByStatusAsync(_testBranchId, QuotationStatus.Sent);

        // Assert
        draftQuotations.Should().HaveCount(1);
        sentQuotations.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveQuotationsAsync_ReturnsOnlyActive()
    {
        // Arrange
        var activeQuotation = await CreateTestQuotationWithItems();
        activeQuotation.ExpiryDate = DateTime.UtcNow.AddDays(10);
        await _service.SendQuotationAsync(activeQuotation.Id);

        var expiredQuotation = await CreateTestQuotationWithItems();
        expiredQuotation.ExpiryDate = DateTime.UtcNow.AddDays(-5);
        expiredQuotation.Status = QuotationStatus.Sent;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetActiveQuotationsAsync(_testBranchId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeQuotation.Id);
    }

    [Fact]
    public async Task GetExpiredQuotationsAsync_ReturnsOnlyExpired()
    {
        // Arrange
        var activeQuotation = await CreateTestQuotation();
        activeQuotation.ExpiryDate = DateTime.UtcNow.AddDays(10);
        await _context.SaveChangesAsync();

        var expiredQuotation = await CreateTestQuotation();
        expiredQuotation.ExpiryDate = DateTime.UtcNow.AddDays(-5);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetExpiredQuotationsAsync(_testBranchId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(expiredQuotation.Id);
    }

    #endregion

    #region Update Quotation Tests

    [Fact]
    public async Task UpdateQuotationAsync_WithDraftQuotation_UpdatesSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        quotation.Notes = "Updated notes";

        // Act
        await _service.UpdateQuotationAsync(quotation);

        // Assert
        var updated = await _context.Quotations.FindAsync(quotation.Id);
        updated!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateQuotationAsync_WithAcceptedQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        quotation.Notes = "Try to update";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateQuotationAsync(quotation));
    }

    [Fact]
    public async Task UpdateQuotationAsync_WithRejectedQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.RejectQuotationAsync(quotation.Id, "Too expensive");

        quotation.Notes = "Try to update";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateQuotationAsync(quotation));
    }

    #endregion

    #region Delete Quotation Tests

    [Fact]
    public async Task DeleteQuotationAsync_WithDraftQuotation_DeletesSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act
        await _service.DeleteQuotationAsync(quotation.Id);

        // Assert
        var deleted = await _context.Quotations.FindAsync(quotation.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteQuotationAsync_WithSentQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task DeleteQuotationAsync_WithConvertedQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);
        await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteQuotationAsync(quotation.Id));
    }

    #endregion

    #region Quotation Workflow Tests

    [Fact]
    public async Task SendQuotationAsync_WithDraftQuotationWithItems_SendsSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();

        // Act
        var result = await _service.SendQuotationAsync(quotation.Id);

        // Assert
        result.Status.Should().Be(QuotationStatus.Sent);
    }

    [Fact]
    public async Task SendQuotationAsync_WithDraftQuotationWithoutItems_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SendQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task SendQuotationAsync_WithNonDraftQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SendQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task AcceptQuotationAsync_WithSentQuotation_AcceptsSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        // Act
        var result = await _service.AcceptQuotationAsync(quotation.Id);

        // Assert
        result.Status.Should().Be(QuotationStatus.Accepted);
        result.AcceptedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task AcceptQuotationAsync_WithExpiredQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        quotation.ExpiryDate = DateTime.UtcNow.AddDays(-1);
        quotation.Status = QuotationStatus.Sent;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task AcceptQuotationAsync_WithDraftQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptQuotationAsync(quotation.Id));
    }

    [Fact]
    public async Task RejectQuotationAsync_WithSentQuotation_RejectsSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        // Act
        var result = await _service.RejectQuotationAsync(quotation.Id, "Too expensive");

        // Assert
        result.Status.Should().Be(QuotationStatus.Rejected);
        result.RejectedDate.Should().NotBeNull();
        result.RejectionReason.Should().Be("Too expensive");
    }

    [Fact]
    public async Task RejectQuotationAsync_WithDraftQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotation();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RejectQuotationAsync(quotation.Id, "Test"));
    }

    #endregion

    #region Convert Quotation to Sale Tests

    [Fact]
    public async Task ConvertQuotationToSaleAsync_WithAcceptedQuotation_CreatesSale()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        // Act
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Assert
        sale.Should().NotBeNull();
        sale.QuotationId.Should().Be(quotation.Id);
        sale.Status.Should().Be(SaleStatus.Draft);
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_CopiesFinancialDetails()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        quotation.SubTotal = 500;
        quotation.DiscountPercentage = 10;
        quotation.TaxPercentage = 5;
        quotation.Total = 472.50m;
        await _context.SaveChangesAsync();

        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        // Act
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Assert
        sale.SubTotal.Should().Be(quotation.SubTotal);
        sale.DiscountPercentage.Should().Be(quotation.DiscountPercentage);
        sale.TaxPercentage.Should().Be(quotation.TaxPercentage);
        sale.Total.Should().Be(quotation.Total);
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_CopiesItems()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithMultipleItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        // Act
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Assert
        sale.Items.Should().HaveCount(quotation.Items.Count);
        sale.Items.First().ItemName.Should().Be(quotation.Items.First().ItemName);
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_IncludesQuotationReference()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.AcceptQuotationAsync(quotation.Id);

        // Act
        var sale = await _service.ConvertQuotationToSaleAsync(quotation.Id);

        // Assert
        sale.Notes.Should().Contain(quotation.QuotationNumber);
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_WithNonAcceptedQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConvertQuotationToSaleAsync(quotation.Id));
    }

    [Fact]
    public async Task ConvertQuotationToSaleAsync_WithRejectedQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        await _service.RejectQuotationAsync(quotation.Id, "No longer needed");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConvertQuotationToSaleAsync(quotation.Id));
    }

    #endregion

    #region Quotation Item Tests

    [Fact]
    public async Task AddQuotationItemAsync_ToDraftQuotation_AddsSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        var item = new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Service",
            Quantity = 2,
            UnitPrice = 100
        };

        // Act
        var result = await _service.AddQuotationItemAsync(item);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddQuotationItemAsync_CalculatesItemTotals()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        var item = new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Service",
            Quantity = 2,
            UnitPrice = 100,
            DiscountPercentage = 10,
            TaxPercentage = 5
        };

        // Act
        var result = await _service.AddQuotationItemAsync(item);

        // Assert
        result.Subtotal.Should().Be(180); // 200 - 10% discount
        result.TaxAmount.Should().Be(9); // 5% of 180
        result.Total.Should().Be(189);
    }

    [Fact]
    public async Task AddQuotationItemAsync_RecalculatesQuotationTotals()
    {
        // Arrange
        var quotation = await CreateTestQuotation();
        var item = new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Test Service",
            Quantity = 1,
            UnitPrice = 500,
            Subtotal = 500,
            Total = 500
        };

        // Act
        await _service.AddQuotationItemAsync(item);

        // Assert
        var updatedQuotation = await _context.Quotations.FindAsync(quotation.Id);
        updatedQuotation!.SubTotal.Should().Be(500);
    }

    [Fact]
    public async Task AddQuotationItemAsync_ToSentQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);

        var item = new QuotationItem
        {
            QuotationId = quotation.Id,
            ItemName = "Another Service",
            Quantity = 1,
            UnitPrice = 100
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddQuotationItemAsync(item));
    }

    [Fact]
    public async Task DeleteQuotationItemAsync_FromDraftQuotation_DeletesSuccessfully()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        var itemId = quotation.Items.First().Id;

        // Act
        await _service.DeleteQuotationItemAsync(itemId);

        // Assert
        var items = await _service.GetQuotationItemsByQuotationIdAsync(quotation.Id);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteQuotationItemAsync_FromSentQuotation_ThrowsException()
    {
        // Arrange
        var quotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(quotation.Id);
        var itemId = quotation.Items.First().Id;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteQuotationItemAsync(itemId));
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetPendingQuotationsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await CreateTestQuotation(); // Draft
        var sentQuotation = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(sentQuotation.Id); // Sent

        // Act
        var count = await _service.GetPendingQuotationsCountAsync(_testBranchId);

        // Assert
        count.Should().Be(2); // Draft + Sent = pending
    }

    [Fact]
    public async Task GetQuotationConversionRateAsync_CalculatesCorrectly()
    {
        // Arrange
        // Create 4 quotations: 2 accepted, 1 rejected, 1 draft
        var q1 = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(q1.Id);
        await _service.AcceptQuotationAsync(q1.Id);

        var q2 = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(q2.Id);
        await _service.AcceptQuotationAsync(q2.Id);

        var q3 = await CreateTestQuotationWithItems();
        await _service.SendQuotationAsync(q3.Id);
        await _service.RejectQuotationAsync(q3.Id, "No");

        await CreateTestQuotation(); // Draft

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var rate = await _service.GetQuotationConversionRateAsync(_testBranchId, startDate, endDate);

        // Assert
        rate.Should().Be(50); // 2 out of 4 = 50%
    }

    #endregion

    #region Helper Methods

    private async Task<Quotation> CreateTestQuotation()
    {
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user"
        };
        return await _service.CreateQuotationAsync(quotation);
    }

    private async Task<Quotation> CreateTestQuotationWithItems()
    {
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user",
            Items = new List<QuotationItem>
            {
                new QuotationItem
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
        return await _service.CreateQuotationAsync(quotation);
    }

    private async Task<Quotation> CreateTestQuotationWithMultipleItems()
    {
        var quotation = new Quotation
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            ValidityDays = 30,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedBy = "test-user",
            Items = new List<QuotationItem>
            {
                new QuotationItem
                {
                    ItemName = "Service A",
                    Quantity = 2,
                    UnitPrice = 100,
                    Subtotal = 200,
                    Total = 200,
                    CreatedAt = DateTime.UtcNow
                },
                new QuotationItem
                {
                    ItemName = "Service B",
                    Quantity = 1,
                    UnitPrice = 150,
                    Subtotal = 150,
                    Total = 150,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        return await _service.CreateQuotationAsync(quotation);
    }

    #endregion
}
