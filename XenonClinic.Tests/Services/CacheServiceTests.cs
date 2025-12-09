using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for CacheService.
/// </summary>
public class CacheServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<CacheService>>();
        _cacheService = new CacheService(_memoryCache, logger);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyNotExists()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("non-existent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldReturnCachedValue_WhenExists()
    {
        // Arrange
        var key = "cached-key";
        var cachedValue = "cached-value";
        await _cacheService.SetAsync(key, cachedValue);

        var factoryCalled = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult("new-value");
        });

        // Assert
        Assert.Equal(cachedValue, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCallFactory_WhenNotCached()
    {
        // Arrange
        var key = "new-key";
        var factoryCalled = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult("factory-value");
        });

        // Assert
        Assert.Equal("factory-value", result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteCachedValue()
    {
        // Arrange
        var key = "to-remove";
        await _cacheService.SetAsync(key, "value");

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var key = "exists-key";
        await _cacheService.SetAsync(key, "value");

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenKeyNotExists()
    {
        // Act
        var exists = await _cacheService.ExistsAsync("not-exists");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task RemoveByPatternAsync_ShouldRemoveMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("user:1", "value1");
        await _cacheService.SetAsync("user:2", "value2");
        await _cacheService.SetAsync("other:1", "value3");

        // Act
        await _cacheService.RemoveByPatternAsync("user:*");

        // Assert
        Assert.False(await _cacheService.ExistsAsync("user:1"));
        Assert.False(await _cacheService.ExistsAsync("user:2"));
        Assert.True(await _cacheService.ExistsAsync("other:1"));
    }

    [Fact]
    public async Task SetAsync_ShouldRespectExpiration()
    {
        // Arrange
        var key = "expiring-key";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cacheService.SetAsync(key, "value", expiration);

        // Immediately should exist
        Assert.True(await _cacheService.ExistsAsync(key));

        // Wait for expiration
        await Task.Delay(150);

        // Should be expired
        Assert.False(await _cacheService.ExistsAsync(key));
    }

    [Fact]
    public async Task SetAsync_ShouldStoreComplexObjects()
    {
        // Arrange
        var key = "complex-object";
        var value = new TestObject { Id = 1, Name = "Test", IsActive = true };

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.True(result.IsActive);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

/// <summary>
/// Tests for CacheKeys helper class.
/// </summary>
public class CacheKeysTests
{
    [Fact]
    public void User_ShouldReturnCorrectFormat()
    {
        // Act
        var key = CacheKeys.User("user-123");

        // Assert
        Assert.Equal("user:user-123", key);
    }

    [Fact]
    public void Tenant_ShouldReturnCorrectFormat()
    {
        // Act
        var key = CacheKeys.Tenant(42);

        // Assert
        Assert.Equal("tenant:42", key);
    }

    [Fact]
    public void Company_ShouldReturnCorrectFormat()
    {
        // Act
        var key = CacheKeys.Company(10);

        // Assert
        Assert.Equal("company:10", key);
    }

    [Fact]
    public void Lookup_WithBranchId_ShouldReturnCorrectFormat()
    {
        // Act
        var key = CacheKeys.Lookup("countries", 5);

        // Assert
        Assert.Equal("lookup:countries:5", key);
    }
}
