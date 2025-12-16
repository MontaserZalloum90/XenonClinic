using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

public class LicenseGuardServiceTests
{
    private ClinicDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ClinicDbContext(options);
    }

    private Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        return mockUserManager;
    }

    [Fact]
    public async Task CanCreateBranchAsync_WhenLicenseIsNull_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();
        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateBranchAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanCreateBranchAsync_WhenLicenseIsInactive_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = false,
            MaxBranches = 5,
            MaxUsers = 10,
            LicenseKey = "TEST-KEY"
        });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateBranchAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanCreateBranchAsync_WhenLicenseExpired_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = true,
            MaxBranches = 5,
            MaxUsers = 10,
            LicenseKey = "TEST-KEY",
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateBranchAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanCreateBranchAsync_WhenBranchLimitReached_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = true,
            MaxBranches = 2,
            MaxUsers = 10,
            LicenseKey = "TEST-KEY",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });

        context.Branches.AddRange(
            new Branch { Name = "Branch 1", Code = "B1" },
            new Branch { Name = "Branch 2", Code = "B2" }
        );
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateBranchAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanCreateBranchAsync_WhenValidLicenseAndUnderLimit_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = true,
            MaxBranches = 5,
            MaxUsers = 10,
            LicenseKey = "TEST-KEY",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });

        context.Branches.Add(new Branch { Name = "Branch 1", Code = "B1" });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateBranchAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanCreateUserAsync_WhenUserLimitReached_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = true,
            MaxBranches = 5,
            MaxUsers = 2,
            LicenseKey = "TEST-KEY",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "user1@test.com" },
            new ApplicationUser { Id = "2", UserName = "user2@test.com" }
        }.AsQueryable();

        mockUserManager.Setup(m => m.Users).Returns(users);

        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateUserAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanCreateUserAsync_WhenValidLicenseAndUnderLimit_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.LicenseConfigs.Add(new LicenseConfig
        {
            IsActive = true,
            MaxBranches = 5,
            MaxUsers = 10,
            LicenseKey = "TEST-KEY",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "user1@test.com" }
        }.AsQueryable();

        mockUserManager.Setup(m => m.Users).Returns(users);

        var service = new LicenseGuardService(context, mockUserManager.Object);

        // Act
        var result = await service.CanCreateUserAsync();

        // Assert
        Assert.True(result);
    }
}
