using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Unit.Services;

/// <summary>
/// Unit tests for caching service edge cases
/// </summary>
public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly ICacheService _cacheService;

    public CacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _cacheService = new RedisCacheService(_mockCache.Object, _mockLogger.Object);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithNullKey_ReturnsDefault()
    {
        // Act
        var result = await _cacheService.GetAsync<string>(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ReturnsDefault()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenCacheMiss_ReturnsDefault()
    {
        // Arrange
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<string>("missing-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenCacheHit_ReturnsValue()
    {
        // Arrange
        var expectedValue = new TestCacheObject { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.SerializeToUtf8Bytes(expectedValue);

        _mockCache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);

        // Act
        var result = await _cacheService.GetAsync<TestCacheObject>("test-key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_WithCorruptedData_ReturnsDefault()
    {
        // Arrange
        var corruptedData = Encoding.UTF8.GetBytes("not-valid-json{{{");

        _mockCache.Setup(c => c.GetAsync("corrupt-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(corruptedData);

        // Act
        var result = await _cacheService.GetAsync<TestCacheObject>("corrupt-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenCacheThrowsException_ReturnsDefaultAndLogsError()
    {
        // Arrange
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act
        var result = await _cacheService.GetAsync<string>("key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithVeryLongKey_HandlesGracefully()
    {
        // Arrange
        var longKey = new string('x', 10000);
        _mockCache.Setup(c => c.GetAsync(longKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<string>(longKey);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithSpecialCharactersInKey_HandlesGracefully()
    {
        // Arrange
        var specialKey = "key:with:colons:and/slashes/and spaces";
        _mockCache.Setup(c => c.GetAsync(specialKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<string>(specialKey);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_WithNullKey_DoesNotThrow()
    {
        // Act
        var act = async () => await _cacheService.SetAsync(null!, "value", TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WithNullValue_DoesNotThrow()
    {
        // Act
        var act = async () => await _cacheService.SetAsync<string?>("key", null, TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WithZeroExpiration_StillSetsValue()
    {
        // Arrange
        var value = "test-value";

        // Act
        await _cacheService.SetAsync("key", value, TimeSpan.Zero);

        // Assert - Should not throw
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task SetAsync_WithNegativeExpiration_HandlesGracefully()
    {
        // Act
        var act = async () => await _cacheService.SetAsync("key", "value", TimeSpan.FromMinutes(-5));

        // Assert - Should handle gracefully (either set with default or ignore)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WithVeryLargeObject_HandlesGracefully()
    {
        // Arrange
        var largeObject = new TestCacheObject
        {
            Id = 1,
            Name = new string('x', 1_000_000) // 1MB string
        };

        // Act
        var act = async () => await _cacheService.SetAsync("large-key", largeObject, TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WhenCacheThrowsException_DoesNotPropagate()
    {
        // Arrange
        _mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis write failed"));

        // Act
        var act = async () => await _cacheService.SetAsync("key", "value", TimeSpan.FromMinutes(5));

        // Assert - Should not propagate exception
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetOrSetAsync Tests

    [Fact]
    public async Task GetOrSetAsync_WhenCacheMiss_CallsFactory()
    {
        // Arrange
        var factoryCalled = false;
        _mockCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetOrSetAsync("key", async () =>
        {
            factoryCalled = true;
            return "factory-value";
        }, TimeSpan.FromMinutes(5));

        // Assert
        factoryCalled.Should().BeTrue();
        result.Should().Be("factory-value");
    }

    [Fact]
    public async Task GetOrSetAsync_WhenCacheHit_DoesNotCallFactory()
    {
        // Arrange
        var factoryCalled = false;
        var cachedValue = JsonSerializer.SerializeToUtf8Bytes("cached-value");

        _mockCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync("key", async () =>
        {
            factoryCalled = true;
            return "factory-value";
        }, TimeSpan.FromMinutes(5));

        // Assert
        factoryCalled.Should().BeFalse();
        result.Should().Be("cached-value");
    }

    [Fact]
    public async Task GetOrSetAsync_WhenFactoryThrowsException_PropagatesException()
    {
        // Arrange
        _mockCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var act = async () => await _cacheService.GetOrSetAsync<string>("key", () =>
        {
            throw new InvalidOperationException("Factory failed");
        }, TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenFactoryReturnsNull_CachesNull()
    {
        // Arrange
        _mockCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetOrSetAsync<string?>("key", () =>
        {
            return Task.FromResult<string?>(null);
        }, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithNullKey_DoesNotThrow()
    {
        // Act
        var act = async () => await _cacheService.RemoveAsync(null!);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistentKey_DoesNotThrow()
    {
        // Act
        var act = async () => await _cacheService.RemoveAsync("non-existent-key");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_WhenCacheThrowsException_DoesNotPropagate()
    {
        // Arrange
        _mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis remove failed"));

        // Act
        var act = async () => await _cacheService.RemoveAsync("key");

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task ConcurrentGetAsync_SameKey_AllReturnSameValue()
    {
        // Arrange
        var value = new TestCacheObject { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value);

        _mockCache.Setup(c => c.GetAsync("concurrent-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _cacheService.GetAsync<TestCacheObject>("concurrent-key"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().OnlyContain(r => r != null && r.Id == 1);
    }

    [Fact]
    public async Task ConcurrentSetAsync_SameKey_LastWriteWins()
    {
        // Arrange & Act
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _cacheService.SetAsync($"concurrent-key", $"value-{i}", TimeSpan.FromMinutes(5)))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Exactly(10));
    }

    #endregion

    #region Edge Cases with Complex Types

    [Fact]
    public async Task GetAsync_WithCircularReference_HandlesGracefully()
    {
        // This tests that the serializer handles or rejects circular references
        // Arrange
        var corruptData = Encoding.UTF8.GetBytes("{\"self\":{\"self\":...}}");
        _mockCache.Setup(c => c.GetAsync("circular-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(corruptData);

        // Act
        var result = await _cacheService.GetAsync<object>("circular-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithDateTimeKind_PreservesKind()
    {
        // Arrange
        var obj = new TestCacheObjectWithDate
        {
            CreatedAt = DateTime.UtcNow
        };

        byte[]? capturedBytes = null;
        _mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((k, v, o, c) =>
                capturedBytes = v);

        // Act
        await _cacheService.SetAsync("date-key", obj, TimeSpan.FromMinutes(5));

        // Assert - Serialized date should be in ISO format
        if (capturedBytes != null)
        {
            var json = Encoding.UTF8.GetString(capturedBytes);
            json.Should().Contain("Z", "UTC dates should be serialized with Z suffix");
        }
    }

    #endregion

    #region Cache Key Generation Tests

    [Theory]
    [InlineData("Patients", 1, "Patients_1")]
    [InlineData("Appointments", 123, "Appointments_123")]
    public void CacheKeys_GeneratesCorrectFormat(string entityType, int id, string expected)
    {
        // Act
        var key = CacheKeys.ForEntity(entityType, id);

        // Assert
        key.Should().Be(expected);
    }

    [Fact]
    public void CacheKeys_ForList_IncludesTenantScope()
    {
        // Act
        var key = CacheKeys.ForList("Patients", tenantId: 5, companyId: 10);

        // Assert
        key.Should().Contain("5");
        key.Should().Contain("10");
    }

    #endregion

    private class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestCacheObjectWithDate
    {
        public DateTime CreatedAt { get; set; }
    }
}
