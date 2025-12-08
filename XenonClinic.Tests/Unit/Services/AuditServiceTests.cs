using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using XenonClinic.Tests.Helpers;
using Xunit;

namespace XenonClinic.Tests.Unit.Services;

/// <summary>
/// Unit tests for audit service edge cases
/// </summary>
public class AuditServiceTests : IDisposable
{
    private readonly ClinicDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<AuditService>> _mockLogger;
    private readonly IAuditService _auditService;

    public AuditServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase($"AuditTest_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ClinicDbContext(options);
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<AuditService>>();

        // Setup default HTTP context
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        httpContext.Request.Headers["User-Agent"] = "Test User Agent";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _auditService = new AuditService(_dbContext, _mockHttpContextAccessor.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region LogAsync Tests

    [Fact]
    public async Task LogAsync_WithValidData_CreatesAuditLog()
    {
        // Arrange
        var auditLog = TestDataGenerator.GenerateAuditLog();

        // Act
        await _auditService.LogAsync(
            auditLog.UserId!,
            auditLog.UserName!,
            auditLog.Action!,
            auditLog.EntityType!,
            auditLog.EntityId!,
            auditLog.OldValues,
            auditLog.NewValues,
            auditLog.TenantId,
            auditLog.CompanyId);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.Action.Should().Be(auditLog.Action);
    }

    [Fact]
    public async Task LogAsync_WithNullUserId_StillCreatesLog()
    {
        // Act
        await _auditService.LogAsync(
            null,
            "System",
            "SystemAction",
            "Entity",
            "1",
            null,
            "{\"field\": \"value\"}",
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.UserId.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LogAsync_WithNullOldAndNewValues_StillCreatesLog()
    {
        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Read",
            "Patient",
            "123",
            null,
            null,
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.OldValues.Should().BeNull();
        savedLog.NewValues.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_CapturesIpAddress()
    {
        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Create",
            "Patient",
            "1",
            null,
            "{}",
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task LogAsync_CapturesUserAgent()
    {
        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Create",
            "Patient",
            "1",
            null,
            "{}",
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.UserAgent.Should().Be("Test User Agent");
    }

    [Fact]
    public async Task LogAsync_WhenHttpContextIsNull_StillCreatesLog()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Create",
            "Patient",
            "1",
            null,
            "{}",
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.IpAddress.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_WithVeryLongValues_TruncatesOrHandles()
    {
        // Arrange
        var veryLongJson = "{\"data\": \"" + new string('x', 100000) + "\"}";

        // Act
        var act = async () => await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Update",
            "Patient",
            "1",
            veryLongJson,
            veryLongJson,
            1,
            1);

        // Assert - Should handle gracefully
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogAsync_WithSpecialCharactersInValues_HandlesCorrectly()
    {
        // Arrange
        var jsonWithSpecialChars = "{\"note\": \"Patient said: \\\"I'm feeling better\\\"\"}";

        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Update",
            "Patient",
            "1",
            null,
            jsonWithSpecialChars,
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.NewValues.Should().Contain("I'm feeling better");
    }

    [Fact]
    public async Task LogAsync_SetsTimestampToUtcNow()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow.AddSeconds(-1);

        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Create",
            "Patient",
            "1",
            null,
            "{}",
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.Timestamp.Should().BeAfter(beforeTest);
        savedLog.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region GetAuditLogsAsync Tests

    [Fact]
    public async Task GetAuditLogsAsync_WithNoFilters_ReturnsAllLogs()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var log = TestDataGenerator.GenerateAuditLog();
            log.TenantId = 1;
            log.CompanyId = 1;
            _dbContext.AuditLogs.Add(log);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(1, 1);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithEntityTypeFilter_ReturnsFilteredLogs()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "TestUser",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "1",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "TestUser",
            Action = "Create",
            EntityType = "Appointment",
            EntityId = "1",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(1, 1, entityType: "Patient");

        // Assert
        result.Should().HaveCount(1);
        result.First().EntityType.Should().Be("Patient");
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithDateRangeFilter_ReturnsFilteredLogs()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "TestUser",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "1",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow.AddDays(-5)
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "TestUser",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "2",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(
            1, 1,
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithUserIdFilter_ReturnsFilteredLogs()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "User1",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "1",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-2",
            UserName = "User2",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "2",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(1, 1, userId: "user-1");

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetAuditLogsAsync_RespectsMultiTenancy()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "User1",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "1",
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-2",
            UserName = "User2",
            Action = "Create",
            EntityType = "Patient",
            EntityId = "2",
            TenantId = 2, // Different tenant
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(1, 1);

        // Assert - Should only return logs for tenant 1
        result.Should().HaveCount(1);
        result.First().TenantId.Should().Be(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            _dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = "user-1",
                UserName = "User1",
                Action = "Create",
                EntityType = "Patient",
                EntityId = i.ToString(),
                TenantId = 1,
                CompanyId = 1,
                Timestamp = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetAuditLogsAsync(1, 1, page: 1, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
    }

    #endregion

    #region GetEntityHistoryAsync Tests

    [Fact]
    public async Task GetEntityHistoryAsync_ReturnsChronologicalHistory()
    {
        // Arrange
        var entityId = "patient-123";

        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "User1",
            Action = "Create",
            EntityType = "Patient",
            EntityId = entityId,
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow.AddDays(-2)
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-1",
            UserName = "User1",
            Action = "Update",
            EntityType = "Patient",
            EntityId = entityId,
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow.AddDays(-1)
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = "user-2",
            UserName = "User2",
            Action = "Update",
            EntityType = "Patient",
            EntityId = entityId,
            TenantId = 1,
            CompanyId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _auditService.GetEntityHistoryAsync("Patient", entityId, 1, 1);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(x => x.Timestamp);
    }

    [Fact]
    public async Task GetEntityHistoryAsync_WithNonExistentEntity_ReturnsEmptyList()
    {
        // Act
        var result = await _auditService.GetEntityHistoryAsync("Patient", "non-existent-id", 1, 1);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Sensitive Data Handling Tests

    [Fact]
    public async Task LogAsync_ShouldNotLogSensitiveFieldsDirectly()
    {
        // Arrange - Password should be masked or not stored
        var sensitiveData = "{\"password\": \"secretpassword123\", \"name\": \"John\"}";

        // Act
        await _auditService.LogAsync(
            "user-1",
            "TestUser",
            "Update",
            "User",
            "1",
            null,
            sensitiveData,
            1,
            1);

        // Assert
        var savedLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();

        // The audit service should either mask the password or the test should verify masking
        // This test documents the expected behavior
        if (savedLog!.NewValues?.Contains("password") == true)
        {
            savedLog.NewValues.Should().NotContain("secretpassword123",
                because: "Sensitive data like passwords should be masked in audit logs");
        }
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task LogAsync_ConcurrentWrites_AllSucceed()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 50)
            .Select(i => _auditService.LogAsync(
                $"user-{i}",
                $"User{i}",
                "Create",
                "Patient",
                i.ToString(),
                null,
                "{}",
                1,
                1))
            .ToArray();

        // Act
        await Task.WhenAll(tasks);

        // Assert
        var count = await _dbContext.AuditLogs.CountAsync();
        count.Should().Be(50);
    }

    #endregion
}
