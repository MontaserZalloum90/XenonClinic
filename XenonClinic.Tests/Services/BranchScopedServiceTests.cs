using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

public class BranchScopedServiceTests
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
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task GetUserBranchIdsAsync_WhenUserHasNoBranches_ReturnsEmptyList()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var user = new ApplicationUser { Id = "user1", UserName = "test@test.com" };
        mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var service = new BranchScopedService(
            mockHttpContextAccessor.Object, mockUserManager.Object, context);

        // Act
        var branchIds = await service.GetUserBranchIdsAsync();

        // Assert
        Assert.Empty(branchIds);
    }

    [Fact]
    public async Task GetUserBranchIdsAsync_WhenUserHasBranches_ReturnsBranchIds()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var user = new ApplicationUser { Id = "user1", UserName = "test@test.com" };

        context.UserBranches.AddRange(
            new UserBranch { UserId = user.Id, BranchId = 1 },
            new UserBranch { UserId = user.Id, BranchId = 2 }
        );
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var service = new BranchScopedService(
            mockHttpContextAccessor.Object, mockUserManager.Object, context);

        // Act
        var branchIds = await service.GetUserBranchIdsAsync();

        // Assert
        Assert.Equal(2, branchIds.Count);
        Assert.Contains(1, branchIds);
        Assert.Contains(2, branchIds);
    }

    [Fact]
    public async Task HasAccessToBranchAsync_WhenUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var user = new ApplicationUser { Id = "admin1", UserName = "admin@test.com" };

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        mockUserManager.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(true);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var service = new BranchScopedService(
            mockHttpContextAccessor.Object, mockUserManager.Object, context);

        // Act
        var hasAccess = await service.HasAccessToBranchAsync(999);

        // Assert
        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasAccessToBranchAsync_WhenUserHasAccessToBranch_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var user = new ApplicationUser { Id = "user1", UserName = "test@test.com" };

        context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = 1 });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        mockUserManager.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var service = new BranchScopedService(
            mockHttpContextAccessor.Object, mockUserManager.Object, context);

        // Act
        var hasAccess = await service.HasAccessToBranchAsync(1);

        // Assert
        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasAccessToBranchAsync_WhenUserDoesNotHaveAccessToBranch_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var user = new ApplicationUser { Id = "user1", UserName = "test@test.com" };

        context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = 1 });
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        mockUserManager.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var service = new BranchScopedService(
            mockHttpContextAccessor.Object, mockUserManager.Object, context);

        // Act
        var hasAccess = await service.HasAccessToBranchAsync(2);

        // Assert
        Assert.False(hasAccess);
    }
}
