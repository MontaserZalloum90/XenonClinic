using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for SequenceGenerator service.
/// </summary>
public class SequenceGeneratorTests
{
    private readonly DbContextOptions<ClinicDbContext> _options;

    public SequenceGeneratorTests()
    {
        _options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    [Fact]
    public async Task GenerateLabOrderNumberAsync_ShouldReturnFirstNumber_WhenNoExistingOrders()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var service = new SequenceGenerator(context);
        var branchId = 1;

        // Act
        var result = await service.GenerateLabOrderNumberAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("LAB-", result);
        Assert.EndsWith("-0001", result);
    }

    [Fact]
    public async Task GenerateInvoiceNumberAsync_ShouldReturnFirstNumber_WhenNoExistingInvoices()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var service = new SequenceGenerator(context);
        var branchId = 1;

        // Act
        var result = await service.GenerateInvoiceNumberAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("INV-", result);
        Assert.EndsWith("-0001", result);
    }

    [Fact]
    public async Task GenerateEmployeeCodeAsync_ShouldUseMonthlyFormat()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var service = new SequenceGenerator(context);
        var branchId = 1;

        // Act
        var result = await service.GenerateEmployeeCodeAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("EMP-", result);
        // Should have format EMP-YYYYMM-001
        var parts = result.Split('-');
        Assert.Equal(3, parts.Length);
        Assert.Equal(6, parts[1].Length); // YYYYMM format
        Assert.Equal("001", parts[2]); // 3 digits for employee code
    }

    [Fact]
    public async Task GenerateSequenceAsync_ShouldIncludeCorrectDateFormat()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var service = new SequenceGenerator(context);
        var branchId = 1;
        var today = DateTime.UtcNow;

        // Act
        var result = await service.GenerateLabOrderNumberAsync(branchId);

        // Assert
        var expectedDatePart = today.ToString("yyyyMMdd");
        Assert.Contains(expectedDatePart, result);
    }

    [Fact]
    public async Task GenerateRadiologyOrderNumberAsync_ShouldReturnValidFormat()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var service = new SequenceGenerator(context);
        var branchId = 1;

        // Act
        var result = await service.GenerateRadiologyOrderNumberAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("RAD-", result);
        Assert.EndsWith("-0001", result);
    }
}
