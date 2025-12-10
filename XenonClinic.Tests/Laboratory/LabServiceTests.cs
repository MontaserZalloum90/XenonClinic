using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Tests.Laboratory;

/// <summary>
/// Unit tests for LabService covering all laboratory operations.
/// </summary>
public class LabServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock;
    private readonly LabService _service;
    private readonly Branch _testBranch;
    private readonly Patient _testPatient;

    public LabServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _sequenceGeneratorMock = new Mock<ISequenceGenerator>();
        _service = new LabService(_context, _sequenceGeneratorMock.Object);

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

    #region Lab Order Tests

    [Fact]
    public async Task GetLabOrderByIdAsync_ExistingOrder_ReturnsOrderWithIncludes()
    {
        // Arrange
        var order = await CreateTestLabOrder();

        // Act
        var result = await _service.GetLabOrderByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLabOrderByIdAsync_NonExistentOrder_ReturnsNull()
    {
        // Act
        var result = await _service.GetLabOrderByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLabOrdersByBranchIdAsync_ReturnsFilteredOrders()
    {
        // Arrange
        await CreateTestLabOrder();
        await CreateTestLabOrder();

        // Act
        var result = await _service.GetLabOrdersByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLabOrdersByPatientIdAsync_ReturnsFilteredOrders()
    {
        // Arrange
        await CreateTestLabOrder();

        // Act
        var result = await _service.GetLabOrdersByPatientIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetLabOrdersByStatusAsync_ReturnsFilteredOrders()
    {
        // Arrange
        var order = await CreateTestLabOrder();
        order.Status = LabOrderStatus.InProgress;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLabOrdersByStatusAsync(1, LabOrderStatus.InProgress);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPendingLabOrdersAsync_ReturnsPendingOrders()
    {
        // Arrange
        var pendingOrder = await CreateTestLabOrder();
        pendingOrder.Status = LabOrderStatus.Pending;

        var completedOrder = await CreateTestLabOrder();
        completedOrder.Status = LabOrderStatus.Completed;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingLabOrdersAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateLabOrderAsync_ValidOrder_CreatesOrderWithNumber()
    {
        // Arrange
        _sequenceGeneratorMock
            .Setup(x => x.GenerateLabOrderNumberAsync(It.IsAny<int>()))
            .ReturnsAsync("LAB-20241210-0001");

        var order = new LabOrder
        {
            BranchId = 1,
            PatientId = 1,
            OrderDate = DateTime.UtcNow,
            Priority = "Normal"
        };

        // Act
        var result = await _service.CreateLabOrderAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be("LAB-20241210-0001");
        result.Status.Should().Be(LabOrderStatus.Pending);
    }

    [Fact]
    public async Task UpdateLabOrderStatusAsync_ValidStatus_UpdatesStatus()
    {
        // Arrange
        var order = await CreateTestLabOrder();

        // Act
        await _service.UpdateLabOrderStatusAsync(order.Id, LabOrderStatus.InProgress);
        var result = await _service.GetLabOrderByIdAsync(order.Id);

        // Assert
        result!.Status.Should().Be(LabOrderStatus.InProgress);
    }

    [Fact]
    public async Task DeleteLabOrderAsync_ExistingOrder_DeletesOrder()
    {
        // Arrange
        var order = await CreateTestLabOrder();

        // Act
        await _service.DeleteLabOrderAsync(order.Id);
        var result = await _service.GetLabOrderByIdAsync(order.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Lab Test Tests

    [Fact]
    public async Task GetLabTestByIdAsync_ExistingTest_ReturnsTest()
    {
        // Arrange
        var test = await CreateTestLabTest();

        // Act
        var result = await _service.GetLabTestByIdAsync(test.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(test.Id);
    }

    [Fact]
    public async Task GetLabTestsByBranchIdAsync_ReturnsFilteredTests()
    {
        // Arrange
        await CreateTestLabTest();
        await CreateTestLabTest();

        // Act
        var result = await _service.GetLabTestsByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveLabTestsAsync_ReturnsOnlyActiveTests()
    {
        // Arrange
        var activeTest = await CreateTestLabTest();
        var inactiveTest = await CreateTestLabTest();
        inactiveTest.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetActiveLabTestsAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateLabTestAsync_ValidTest_CreatesTest()
    {
        // Arrange
        var test = new LabTest
        {
            BranchId = 1,
            TestCode = "TEST-001",
            TestName = "Blood Test",
            Description = "Complete blood count",
            Price = 100,
            IsActive = true
        };

        // Act
        var result = await _service.CreateLabTestAsync(test);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateLabTestAsync_DuplicateCode_ThrowsException()
    {
        // Arrange
        await CreateTestLabTest("DUP-001");

        var duplicateTest = new LabTest
        {
            BranchId = 1,
            TestCode = "DUP-001",
            TestName = "Duplicate Test",
            Price = 100,
            IsActive = true
        };

        // Act & Assert
        var act = () => _service.CreateLabTestAsync(duplicateTest);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region Lab Result Tests

    [Fact]
    public async Task GetLabResultByIdAsync_ExistingResult_ReturnsResult()
    {
        // Arrange
        var result = await CreateTestLabResult();

        // Act
        var foundResult = await _service.GetLabResultByIdAsync(result.Id);

        // Assert
        foundResult.Should().NotBeNull();
        foundResult!.Id.Should().Be(result.Id);
    }

    [Fact]
    public async Task GetLabResultsByPatientIdAsync_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestLabResult();

        // Act
        var results = await _service.GetLabResultsByPatientIdAsync(1);

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateLabResultAsync_ValidResult_CreatesResult()
    {
        // Arrange
        var order = await CreateTestLabOrder();
        var test = await CreateTestLabTest();

        var result = new LabResult
        {
            LabOrderId = order.Id,
            LabTestId = test.Id,
            PatientId = 1,
            BranchId = 1,
            ResultValue = "Normal",
            Status = LabResultStatus.Completed,
            ResultDate = DateTime.UtcNow
        };

        // Act
        var created = await _service.CreateLabResultAsync(result);

        // Assert
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateLabResultAsync_ValidResult_UpdatesResult()
    {
        // Arrange
        var result = await CreateTestLabResult();
        result.ResultValue = "Updated Value";

        // Act
        await _service.UpdateLabResultAsync(result);
        var updated = await _service.GetLabResultByIdAsync(result.Id);

        // Assert
        updated!.ResultValue.Should().Be("Updated Value");
    }

    #endregion

    #region External Lab Tests

    [Fact]
    public async Task GetExternalLabByIdAsync_ExistingLab_ReturnsLab()
    {
        // Arrange
        var lab = await CreateTestExternalLab();

        // Act
        var result = await _service.GetExternalLabByIdAsync(lab.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(lab.Id);
    }

    [Fact]
    public async Task GetExternalLabsByBranchIdAsync_ReturnsFilteredLabs()
    {
        // Arrange
        await CreateTestExternalLab();
        await CreateTestExternalLab();

        // Act
        var result = await _service.GetExternalLabsByBranchIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateExternalLabAsync_ValidLab_CreatesLab()
    {
        // Arrange
        var lab = new ExternalLab
        {
            BranchId = 1,
            Name = "External Lab",
            ContactPerson = "John Doe",
            Phone = "+971501234567",
            Email = "lab@example.com",
            IsActive = true
        };

        // Act
        var result = await _service.CreateExternalLabAsync(lab);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTodaysLabOrdersCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await CreateTestLabOrder();
        await CreateTestLabOrder();

        // Act
        var result = await _service.GetTodaysLabOrdersCountAsync(1);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetPendingLabOrdersCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var pendingOrder = await CreateTestLabOrder();
        pendingOrder.Status = LabOrderStatus.Pending;

        var completedOrder = await CreateTestLabOrder();
        completedOrder.Status = LabOrderStatus.Completed;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingLabOrdersCountAsync(1);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetLabStatisticsAsync_ReturnsComprehensiveStats()
    {
        // Arrange
        await CreateTestLabOrder();
        await CreateTestLabTest();

        // Act
        var result = await _service.GetLabStatisticsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TotalOrdersToday.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Helper Methods

    private async Task<LabOrder> CreateTestLabOrder()
    {
        _sequenceGeneratorMock
            .Setup(x => x.GenerateLabOrderNumberAsync(It.IsAny<int>()))
            .ReturnsAsync($"LAB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}");

        var order = new LabOrder
        {
            BranchId = 1,
            PatientId = 1,
            OrderDate = DateTime.UtcNow,
            Priority = "Normal"
        };

        return await _service.CreateLabOrderAsync(order);
    }

    private async Task<LabTest> CreateTestLabTest(string? testCode = null)
    {
        var test = new LabTest
        {
            BranchId = 1,
            TestCode = testCode ?? $"TEST-{Guid.NewGuid().ToString()[..4]}",
            TestName = "Test Lab Test",
            Description = "Test description",
            Price = 100,
            IsActive = true
        };

        return await _service.CreateLabTestAsync(test);
    }

    private async Task<LabResult> CreateTestLabResult()
    {
        var order = await CreateTestLabOrder();
        var test = await CreateTestLabTest();

        var result = new LabResult
        {
            LabOrderId = order.Id,
            LabTestId = test.Id,
            PatientId = 1,
            BranchId = 1,
            ResultValue = "Normal",
            Status = LabResultStatus.Completed,
            ResultDate = DateTime.UtcNow
        };

        return await _service.CreateLabResultAsync(result);
    }

    private async Task<ExternalLab> CreateTestExternalLab()
    {
        var lab = new ExternalLab
        {
            BranchId = 1,
            Name = $"External Lab {Guid.NewGuid().ToString()[..4]}",
            ContactPerson = "John Doe",
            Phone = "+971501234567",
            Email = "lab@example.com",
            IsActive = true
        };

        return await _service.CreateExternalLabAsync(lab);
    }

    #endregion
}
