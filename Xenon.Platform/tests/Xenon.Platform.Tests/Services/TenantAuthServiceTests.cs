using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for TenantAuthService - validates tenant signup, login, and provisioning workflows.
/// Focuses on the improved database provisioning tracking and password verification.
/// </summary>
public class TenantAuthServiceTests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly Mock<ITenantProvisioningService> _provisioningServiceMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly Mock<IPasswordHashingService> _passwordServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<TenantAuthService>> _loggerMock;
    private readonly TenantAuthService _service;

    public TenantAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);
        _provisioningServiceMock = new Mock<ITenantProvisioningService>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        _passwordServiceMock = new Mock<IPasswordHashingService>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<TenantAuthService>>();

        _service = new TenantAuthService(
            _context,
            _provisioningServiceMock.Object,
            _jwtServiceMock.Object,
            _passwordServiceMock.Object,
            _auditServiceMock.Object,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenEmailNotFound()
    {
        // Arrange
        var request = new TenantLoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenAccountIsLockedOut()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant, isLockedOut: true);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("locked");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenAccountIsInactive()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant, isActive: false);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disabled");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenTenantIsSuspended()
    {
        // Arrange
        var tenant = CreateTestTenant(status: TenantStatus.Suspended);
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("suspended");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenTenantIsCancelled()
    {
        // Arrange
        var tenant = CreateTestTenant(status: TenantStatus.Cancelled);
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task LoginAsync_ShouldIncrementFailedAttempts_WhenPasswordIsWrong()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "WrongPassword"
        };

        // Act
        await _service.LoginAsync(request);

        // Assert
        var updatedAdmin = await _context.TenantAdmins.FindAsync(admin.Id);
        updatedAdmin!.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_ShouldLockAccount_After5FailedAttempts()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        admin.FailedLoginAttempts = 4; // Already 4 failed attempts
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "WrongPassword"
        };

        // Act
        await _service.LoginAsync(request);

        // Assert
        var updatedAdmin = await _context.TenantAdmins.FindAsync(admin.Id);
        updatedAdmin!.FailedLoginAttempts.Should().Be(5);
        updatedAdmin.LockoutEndAt.Should().NotBeNull();
        updatedAdmin.LockoutEndAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ShouldResetFailedAttempts_WhenPasswordIsCorrect()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        admin.FailedLoginAttempts = 3;
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateTenantToken(It.IsAny<TenantAdmin>(), It.IsAny<Tenant>()))
            .Returns("test-token");

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "CorrectPassword"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedAdmin = await _context.TenantAdmins.FindAsync(admin.Id);
        updatedAdmin!.FailedLoginAttempts.Should().Be(0);
        updatedAdmin.LockoutEndAt.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldUpdateLastLoginInfo_OnSuccess()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateTenantToken(It.IsAny<TenantAdmin>(), It.IsAny<Tenant>()))
            .Returns("test-token");

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "CorrectPassword"
        };
        var clientIp = "192.168.1.1";

        // Act
        await _service.LoginAsync(request, clientIp);

        // Assert
        var updatedAdmin = await _context.TenantAdmins.FindAsync(admin.Id);
        updatedAdmin!.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updatedAdmin.LastLoginIp.Should().Be(clientIp);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_OnSuccess()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateTenantToken(It.IsAny<TenantAdmin>(), It.IsAny<Tenant>()))
            .Returns("generated-jwt-token");

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "CorrectPassword"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("generated-jwt-token");
        result.Value.Tenant.Id.Should().Be(tenant.Id);
        result.Value.User.Id.Should().Be(admin.Id);
    }

    [Fact]
    public async Task LoginAsync_ShouldLogAuditEvent_OnSuccess()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateTenantToken(It.IsAny<TenantAdmin>(), It.IsAny<Tenant>()))
            .Returns("test-token");

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "CorrectPassword"
        };

        // Act
        await _service.LoginAsync(request);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "LoginSuccess",
                "TenantAdmin",
                admin.Id,
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                admin.TenantId,
                true,
                null),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldLogAuditEvent_OnFailure()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var request = new TenantLoginRequest
        {
            Email = admin.Email,
            Password = "WrongPassword"
        };

        // Act
        await _service.LoginAsync(request);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "LoginFailed",
                "TenantAdmin",
                admin.Id,
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                admin.TenantId,
                false,
                "Invalid password"),
            Times.Once);
    }

    #endregion

    #region GetCurrentTenantAsync Tests

    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnError_WhenTenantNotFound()
    {
        // Act
        var result = await _service.GetCurrentTenantAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnTenantContext_WhenFound()
    {
        // Arrange
        var tenant = CreateTestTenant();
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentTenantAsync(tenant.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tenant.Id.Should().Be(tenant.Id);
        result.Value.Tenant.Name.Should().Be(tenant.Name);
        result.Value.License.MaxBranches.Should().Be(tenant.MaxBranches);
        result.Value.License.MaxUsers.Should().Be(tenant.MaxUsers);
    }

    [Fact]
    public async Task GetCurrentTenantAsync_ShouldCalculateLicenseUsage()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.MaxBranches = 5;
        tenant.MaxUsers = 20;
        tenant.CurrentBranches = 3;
        tenant.CurrentUsers = 15;
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentTenantAsync(tenant.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.License.CanAddBranch.Should().BeTrue();
        result.Value.License.CanAddUser.Should().BeTrue();
        result.Value.License.CurrentBranches.Should().Be(3);
        result.Value.License.CurrentUsers.Should().Be(15);
    }

    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnFalse_WhenAtMaxCapacity()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.MaxBranches = 3;
        tenant.MaxUsers = 10;
        tenant.CurrentBranches = 3;
        tenant.CurrentUsers = 10;
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentTenantAsync(tenant.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.License.CanAddBranch.Should().BeFalse();
        result.Value.License.CanAddUser.Should().BeFalse();
    }

    #endregion

    #region IsSlugAvailableAsync Tests

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldDelegateToProvisioningService()
    {
        // Arrange
        _provisioningServiceMock.Setup(p => p.IsSlugAvailable(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsSlugAvailableAsync("test-slug");

        // Assert
        result.Should().BeTrue();
        _provisioningServiceMock.Verify(p => p.IsSlugAvailable("test-slug"), Times.Once);
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnFalse_WhenSlugExists()
    {
        // Arrange
        _provisioningServiceMock.Setup(p => p.IsSlugAvailable("existing-slug"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsSlugAvailableAsync("existing-slug");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Case Insensitive Email Tests

    [Fact]
    public async Task LoginAsync_ShouldBeCaseInsensitive_ForEmail()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestAdmin(tenant);
        admin.Email = "user@example.com";
        await _context.Tenants.AddAsync(tenant);
        await _context.TenantAdmins.AddAsync(admin);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateTenantToken(It.IsAny<TenantAdmin>(), It.IsAny<Tenant>()))
            .Returns("test-token");

        var request = new TenantLoginRequest
        {
            Email = "USER@EXAMPLE.COM", // Different case
            Password = "Password123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Tenant CreateTestTenant(TenantStatus status = TenantStatus.Active)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Clinic",
            Slug = "test-clinic",
            CompanyType = CompanyType.Clinic,
            ClinicType = ClinicType.General,
            Status = status,
            ContactEmail = "contact@test.com",
            TrialStartDate = DateTime.UtcNow.AddDays(-10),
            TrialEndDate = DateTime.UtcNow.AddDays(20),
            MaxBranches = 3,
            MaxUsers = 10,
            CurrentBranches = 1,
            CurrentUsers = 3,
            IsDatabaseProvisioned = true,
            DatabaseProvisionedAt = DateTime.UtcNow.AddDays(-10)
        };
    }

    private static TenantAdmin CreateTestAdmin(Tenant tenant, bool isActive = true, bool isLockedOut = false)
    {
        return new TenantAdmin
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Tenant = tenant,
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Admin",
            PasswordHash = "$2a$12$validhash",
            Role = "Owner",
            IsActive = isActive,
            FailedLoginAttempts = 0,
            LockoutEndAt = isLockedOut ? DateTime.UtcNow.AddMinutes(15) : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
