using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for AuditService.
/// </summary>
public class AuditServiceTests
{
    private readonly Mock<ILogger<AuditService>> _loggerMock;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuditService>>();
        _service = new AuditService(_loggerMock.Object);
    }

    [Fact]
    public async Task LogAsync_ShouldCreateAuditEntry()
    {
        // Arrange
        var entry = new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "123",
            Action = AuditAction.Create,
            UserId = "user1",
            UserName = "John Doe"
        };

        // Act
        await _service.LogAsync(entry);
        var history = await _service.GetHistoryAsync("Patient", "123");

        // Assert
        Assert.Single(history);
        Assert.Equal(AuditAction.Create, history.First().Action);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnOrderedByTimestamp()
    {
        // Arrange
        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "123",
            Action = AuditAction.Create,
            UserId = "user1"
        });

        await Task.Delay(10); // Ensure different timestamps

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "123",
            Action = AuditAction.Update,
            UserId = "user1"
        });

        // Act
        var history = (await _service.GetHistoryAsync("Patient", "123")).ToList();

        // Assert
        Assert.Equal(2, history.Count);
        Assert.Equal(AuditAction.Update, history[0].Action); // Most recent first
        Assert.Equal(AuditAction.Create, history[1].Action);
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnUserEntries()
    {
        // Arrange
        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "1",
            Action = AuditAction.Create,
            UserId = "user1"
        });

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "2",
            Action = AuditAction.Create,
            UserId = "user2"
        });

        // Act
        var user1Entries = await _service.GetByUserAsync("user1");

        // Assert
        Assert.All(user1Entries, e => Assert.Equal("user1", e.UserId));
    }

    [Fact]
    public async Task GetByActionAsync_ShouldReturnMatchingActions()
    {
        // Arrange
        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "1",
            Action = AuditAction.Create,
            UserId = "user1"
        });

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "1",
            Action = AuditAction.Delete,
            UserId = "user1"
        });

        // Act
        var deleteEntries = await _service.GetByActionAsync(AuditAction.Delete);

        // Assert
        Assert.All(deleteEntries, e => Assert.Equal(AuditAction.Delete, e.Action));
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByEntityType()
    {
        // Arrange
        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "1",
            Action = AuditAction.Create,
            UserId = "user1"
        });

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "Appointment",
            EntityId = "1",
            Action = AuditAction.Create,
            UserId = "user1"
        });

        // Act
        var result = await _service.SearchAsync(new AuditSearchCriteria
        {
            EntityType = "Patient"
        });

        // Assert
        Assert.All(result.Items, e => Assert.Equal("Patient", e.EntityType));
    }

    [Fact]
    public async Task SearchAsync_ShouldPaginateResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _service.LogAsync(new AuditEntry
            {
                EntityType = "TestEntity",
                EntityId = i.ToString(),
                Action = AuditAction.Create,
                UserId = "user1"
            });
        }

        // Act
        var page1 = await _service.SearchAsync(new AuditSearchCriteria
        {
            EntityType = "TestEntity",
            Page = 1,
            PageSize = 3
        });

        var page2 = await _service.SearchAsync(new AuditSearchCriteria
        {
            EntityType = "TestEntity",
            Page = 2,
            PageSize = 3
        });

        // Assert
        Assert.Equal(3, page1.Items.Count);
        Assert.Equal(3, page2.Items.Count);
        Assert.True(page1.HasNextPage);
        Assert.True(page2.HasPreviousPage);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var now = DateTime.UtcNow;

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "DateTest",
            EntityId = "1",
            Action = AuditAction.Create,
            UserId = "user1",
            Timestamp = now.AddDays(-5)
        });

        await _service.LogAsync(new AuditEntry
        {
            EntityType = "DateTest",
            EntityId = "2",
            Action = AuditAction.Create,
            UserId = "user1",
            Timestamp = now
        });

        // Act
        var result = await _service.SearchAsync(new AuditSearchCriteria
        {
            EntityType = "DateTest",
            FromDate = now.AddDays(-1)
        });

        // Assert - Should only get the recent entry
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task LogAsync_ShouldCaptureChangedProperties()
    {
        // Arrange
        var entry = new AuditEntry
        {
            EntityType = "Patient",
            EntityId = "123",
            Action = AuditAction.Update,
            UserId = "user1",
            OldValues = new Dictionary<string, object?> { ["Name"] = "Old Name" },
            NewValues = new Dictionary<string, object?> { ["Name"] = "New Name" },
            ChangedProperties = new List<string> { "Name" }
        };

        // Act
        await _service.LogAsync(entry);
        var history = (await _service.GetHistoryAsync("Patient", "123")).First();

        // Assert
        Assert.Single(history.ChangedProperties);
        Assert.Equal("Name", history.ChangedProperties[0]);
        Assert.Equal("Old Name", history.OldValues["Name"]);
        Assert.Equal("New Name", history.NewValues["Name"]);
    }
}
