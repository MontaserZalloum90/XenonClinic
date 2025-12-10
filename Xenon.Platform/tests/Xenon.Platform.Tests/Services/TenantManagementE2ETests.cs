using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Comprehensive End-to-End tests for Platform Tenant Management.
/// Validates tenant lifecycle, multi-tenant isolation, and SaaS operations.
/// </summary>
public class TenantManagementE2ETests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<TenantManagementService>> _loggerMock;
    private readonly TenantManagementService _service;

    // Test data
    private Tenant _tenantAlpha = null!;
    private Tenant _tenantBeta = null!;
    private Tenant _tenantTrial = null!;
    private Tenant _tenantExpired = null!;
    private Tenant _tenantSuspended = null!;
    private TenantAdmin _adminAlpha = null!;
    private TenantAdmin _adminBeta = null!;

    public TenantManagementE2ETests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<TenantManagementService>>();

        _service = new TenantManagementService(
            _context,
            _auditServiceMock.Object,
            _loggerMock.Object);

        SetupTestData();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Test Setup

    private void SetupTestData()
    {
        // Active tenant
        _tenantAlpha = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Alpha Clinic",
            Slug = "alpha-clinic",
            CompanyType = CompanyType.Clinic,
            ClinicType = ClinicType.Audiology,
            Status = TenantStatus.Active,
            ContactEmail = "admin@alpha.com",
            Country = "USA",
            TrialStartDate = DateTime.UtcNow.AddDays(-60),
            TrialEndDate = DateTime.UtcNow.AddDays(-30),
            MaxBranches = 5,
            MaxUsers = 25,
            CurrentBranches = 3,
            CurrentUsers = 12,
            IsDatabaseProvisioned = true,
            DatabaseProvisionedAt = DateTime.UtcNow.AddDays(-60),
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        // Another active tenant
        _tenantBeta = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Beta Dental",
            Slug = "beta-dental",
            CompanyType = CompanyType.Clinic,
            ClinicType = ClinicType.Dental,
            Status = TenantStatus.Active,
            ContactEmail = "admin@beta.com",
            Country = "UK",
            TrialStartDate = DateTime.UtcNow.AddDays(-45),
            TrialEndDate = DateTime.UtcNow.AddDays(-15),
            MaxBranches = 3,
            MaxUsers = 15,
            CurrentBranches = 1,
            CurrentUsers = 5,
            IsDatabaseProvisioned = true,
            DatabaseProvisionedAt = DateTime.UtcNow.AddDays(-45),
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        // Trial tenant
        _tenantTrial = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Trial Vet Clinic",
            Slug = "trial-vet",
            CompanyType = CompanyType.Clinic,
            ClinicType = ClinicType.Veterinary,
            Status = TenantStatus.Trial,
            ContactEmail = "trial@vet.com",
            Country = "UAE",
            TrialStartDate = DateTime.UtcNow.AddDays(-10),
            TrialEndDate = DateTime.UtcNow.AddDays(20),
            MaxBranches = 1,
            MaxUsers = 5,
            CurrentBranches = 1,
            CurrentUsers = 2,
            IsDatabaseProvisioned = true,
            DatabaseProvisionedAt = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        // Expired trial tenant
        _tenantExpired = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Expired Clinic",
            Slug = "expired-clinic",
            CompanyType = CompanyType.Clinic,
            ClinicType = ClinicType.General,
            Status = TenantStatus.Expired,
            ContactEmail = "expired@clinic.com",
            Country = "India",
            TrialStartDate = DateTime.UtcNow.AddDays(-45),
            TrialEndDate = DateTime.UtcNow.AddDays(-15),
            MaxBranches = 1,
            MaxUsers = 5,
            CurrentBranches = 1,
            CurrentUsers = 1,
            IsDatabaseProvisioned = true,
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        // Suspended tenant
        _tenantSuspended = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Suspended Clinic",
            Slug = "suspended-clinic",
            CompanyType = CompanyType.Clinic,
            Status = TenantStatus.Suspended,
            ContactEmail = "suspended@clinic.com",
            Country = "Canada",
            TrialStartDate = DateTime.UtcNow.AddDays(-90),
            TrialEndDate = DateTime.UtcNow.AddDays(-60),
            MaxBranches = 2,
            MaxUsers = 10,
            CurrentBranches = 2,
            CurrentUsers = 8,
            IsDatabaseProvisioned = true,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        _context.Tenants.AddRange(_tenantAlpha, _tenantBeta, _tenantTrial, _tenantExpired, _tenantSuspended);

        // Create admins
        _adminAlpha = new TenantAdmin
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantAlpha.Id,
            Email = "admin@alpha.com",
            PasswordHash = "$2a$12$validhash",
            FirstName = "Alpha",
            LastName = "Admin",
            Role = "TENANT_ADMIN",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        _adminBeta = new TenantAdmin
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantBeta.Id,
            Email = "admin@beta.com",
            PasswordHash = "$2a$12$validhash",
            FirstName = "Beta",
            LastName = "Admin",
            Role = "TENANT_ADMIN",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        _context.TenantAdmins.AddRange(_adminAlpha, _adminBeta);
        _context.SaveChanges();
    }

    #endregion

    #region Tenant Listing Tests

    [Fact]
    public async Task GetTenantsAsync_ReturnsAllTenants_WhenNoFilters()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetTenantsAsync_FiltersbyStatus_WhenStatusProvided()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10, Status = "Active" };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(2);
        result.Items.Should().AllSatisfy(t => t.Status.Should().Be("Active"));
    }

    [Fact]
    public async Task GetTenantsAsync_FiltersByCompanyType_WhenProvided()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10, CompanyType = "Clinic" };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(5);
        result.Items.Should().AllSatisfy(t => t.CompanyType.Should().Be("Clinic"));
    }

    [Fact]
    public async Task GetTenantsAsync_SearchesByName_WhenSearchProvided()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10, Search = "Alpha" };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(1);
        result.Items.First().Name.Should().Be("Alpha Clinic");
    }

    [Fact]
    public async Task GetTenantsAsync_SearchesByEmail_WhenSearchProvided()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10, Search = "admin@beta" };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(1);
        result.Items.First().ContactEmail.Should().Be("admin@beta.com");
    }

    [Fact]
    public async Task GetTenantsAsync_SupportsPagination()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 2 };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Total.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetTenantsAsync_ReturnsCorrectLicenseInfo()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10, Search = "Alpha" };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        var tenant = result.Items.First();
        tenant.MaxBranches.Should().Be(5);
        tenant.MaxUsers.Should().Be(25);
        tenant.CurrentBranches.Should().Be(3);
        tenant.CurrentUsers.Should().Be(12);
    }

    #endregion

    #region Tenant Details Tests

    [Fact]
    public async Task GetTenantDetailsAsync_ReturnsFullDetails_WhenTenantExists()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tenant.Id.Should().Be(_tenantAlpha.Id);
        result.Value.Tenant.Name.Should().Be("Alpha Clinic");
        result.Value.Tenant.Status.Should().Be("Active");
        result.Value.Tenant.CompanyType.Should().Be("Clinic");
        result.Value.Tenant.ClinicType.Should().Be("Audiology");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_IncludesAdmins()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Admins.Should().HaveCount(1);
        result.Value.Admins.First().Email.Should().Be("admin@alpha.com");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_ReturnsError_WhenTenantNotFound()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_IncludesLicenseUsage()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);

        // Assert
        result.Value.Tenant.MaxBranches.Should().Be(5);
        result.Value.Tenant.CurrentBranches.Should().Be(3);
        result.Value.Tenant.MaxUsers.Should().Be(25);
        result.Value.Tenant.CurrentUsers.Should().Be(12);
    }

    #endregion

    #region Tenant Suspension Tests

    [Fact]
    public async Task SuspendTenantAsync_SuspendsTenant_WhenActive()
    {
        // Arrange
        var request = new SuspendTenantRequest { Reason = "Non-payment" };

        // Act
        var result = await _service.SuspendTenantAsync(_tenantAlpha.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantAlpha.Id);
        tenant!.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public async Task SuspendTenantAsync_LogsAuditEvent()
    {
        // Arrange
        var request = new SuspendTenantRequest { Reason = "Violation" };

        // Act
        await _service.SuspendTenantAsync(_tenantBeta.Id, request);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "TenantSuspended",
                "Tenant",
                _tenantBeta.Id,
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                _tenantBeta.Id,
                true,
                null),
            Times.Once);
    }

    [Fact]
    public async Task SuspendTenantAsync_ReturnsError_WhenAlreadySuspended()
    {
        // Arrange
        var request = new SuspendTenantRequest { Reason = "Test" };

        // Act
        var result = await _service.SuspendTenantAsync(_tenantSuspended.Id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already suspended");
    }

    [Fact]
    public async Task SuspendTenantAsync_ReturnsError_WhenTenantNotFound()
    {
        // Arrange
        var request = new SuspendTenantRequest { Reason = "Test" };

        // Act
        var result = await _service.SuspendTenantAsync(Guid.NewGuid(), request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    #endregion

    #region Tenant Activation Tests

    [Fact]
    public async Task ActivateTenantAsync_ActivatesTenant_WhenSuspended()
    {
        // Act
        var result = await _service.ActivateTenantAsync(_tenantSuspended.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantSuspended.Id);
        tenant!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task ActivateTenantAsync_LogsAuditEvent()
    {
        // Act
        await _service.ActivateTenantAsync(_tenantSuspended.Id);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "TenantActivated",
                "Tenant",
                _tenantSuspended.Id,
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                _tenantSuspended.Id,
                true,
                null),
            Times.Once);
    }

    [Fact]
    public async Task ActivateTenantAsync_ReturnsError_WhenAlreadyActive()
    {
        // Act
        var result = await _service.ActivateTenantAsync(_tenantAlpha.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already active");
    }

    [Fact]
    public async Task ActivateTenantAsync_CanActivateExpiredTenant()
    {
        // Act
        var result = await _service.ActivateTenantAsync(_tenantExpired.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantExpired.Id);
        tenant!.Status.Should().Be(TenantStatus.Active);
    }

    #endregion

    #region Trial Extension Tests

    [Fact]
    public async Task ExtendTrialAsync_ExtendsTrial_WhenInTrial()
    {
        // Arrange
        var originalEndDate = _tenantTrial.TrialEndDate;
        var request = new ExtendTrialRequest { Days = 14, Reason = "Sales discussion" };

        // Act
        var result = await _service.ExtendTrialAsync(_tenantTrial.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NewTrialEndDate.Should().Be(originalEndDate.AddDays(14));
    }

    [Fact]
    public async Task ExtendTrialAsync_CanExtendExpiredTrial()
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = 30, Reason = "Second chance" };

        // Act
        var result = await _service.ExtendTrialAsync(_tenantExpired.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantExpired.Id);
        tenant!.Status.Should().Be(TenantStatus.Trial);
    }

    [Fact]
    public async Task ExtendTrialAsync_ReturnsError_WhenActiveSubscription()
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = 14, Reason = "Test" };

        // Act
        var result = await _service.ExtendTrialAsync(_tenantAlpha.Id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("trial or expired");
    }

    [Fact]
    public async Task ExtendTrialAsync_LogsAuditEvent()
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = 7, Reason = "Requested" };

        // Act
        await _service.ExtendTrialAsync(_tenantTrial.Id, request);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "TrialExtended",
                "Tenant",
                _tenantTrial.Id,
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                _tenantTrial.Id,
                true,
                null),
            Times.Once);
    }

    #endregion

    #region Tenant Usage Tests

    [Fact]
    public async Task GetTenantUsageAsync_ReturnsError_WhenTenantNotFound()
    {
        // Arrange
        var query = new TenantUsageQuery { Days = 30 };

        // Act
        var result = await _service.GetTenantUsageAsync(Guid.NewGuid(), query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetTenantUsageAsync_ReturnsCurrentUsage()
    {
        // Arrange
        var query = new TenantUsageQuery { Days = 30 };

        // Act
        var result = await _service.GetTenantUsageAsync(_tenantAlpha.Id, query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(_tenantAlpha.Id);
        result.Value.Summary.CurrentUsers.Should().Be(12);
        result.Value.Summary.CurrentBranches.Should().Be(3);
        result.Value.Summary.MaxUsers.Should().Be(25);
        result.Value.Summary.MaxBranches.Should().Be(5);
    }

    [Fact]
    public async Task GetTenantUsageAsync_IncludesUsageHistory()
    {
        // Arrange - Add some usage snapshots
        _context.UsageSnapshots.AddRange(
            new UsageSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantAlpha.Id,
                SnapshotType = "Daily",
                SnapshotDate = DateTime.UtcNow.AddDays(-1),
                ActiveUsers = 10,
                TotalUsers = 12,
                ActiveBranches = 3,
                ApiCallsCount = 500,
                ApiErrorsCount = 5,
                PatientsCount = 100,
                AppointmentsCount = 50,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new UsageSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantAlpha.Id,
                SnapshotType = "Daily",
                SnapshotDate = DateTime.UtcNow.AddDays(-2),
                ActiveUsers = 8,
                TotalUsers = 12,
                ActiveBranches = 3,
                ApiCallsCount = 450,
                ApiErrorsCount = 3,
                PatientsCount = 98,
                AppointmentsCount = 45,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        );
        await _context.SaveChangesAsync();

        var query = new TenantUsageQuery { Days = 7 };

        // Act
        var result = await _service.GetTenantUsageAsync(_tenantAlpha.Id, query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.History.Should().HaveCount(2);
        result.Value.Summary.TotalApiCalls.Should().Be(950);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task TenantData_IsIsolatedByTenantId()
    {
        // Arrange - Get admins for each tenant
        var alphaAdmins = await _context.TenantAdmins
            .Where(a => a.TenantId == _tenantAlpha.Id)
            .ToListAsync();

        var betaAdmins = await _context.TenantAdmins
            .Where(a => a.TenantId == _tenantBeta.Id)
            .ToListAsync();

        // Assert
        alphaAdmins.Should().HaveCount(1);
        alphaAdmins.First().Email.Should().Be("admin@alpha.com");

        betaAdmins.Should().HaveCount(1);
        betaAdmins.First().Email.Should().Be("admin@beta.com");

        // Verify no cross-contamination
        alphaAdmins.Should().NotContain(a => a.Email == "admin@beta.com");
        betaAdmins.Should().NotContain(a => a.Email == "admin@alpha.com");
    }

    [Fact]
    public async Task TenantDetails_OnlyIncludesOwnAdmins()
    {
        // Act
        var alphaDetails = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);
        var betaDetails = await _service.GetTenantDetailsAsync(_tenantBeta.Id);

        // Assert
        alphaDetails.Value.Admins.Should().AllSatisfy(a => a.Email.Should().NotBe("admin@beta.com"));
        betaDetails.Value.Admins.Should().AllSatisfy(a => a.Email.Should().NotBe("admin@alpha.com"));
    }

    [Fact]
    public async Task TenantUsage_OnlyReturnsOwnUsageData()
    {
        // Arrange - Add usage for both tenants
        _context.UsageSnapshots.AddRange(
            new UsageSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantAlpha.Id,
                SnapshotType = "Daily",
                SnapshotDate = DateTime.UtcNow,
                ActiveUsers = 10,
                TotalUsers = 12,
                ApiCallsCount = 1000,
                CreatedAt = DateTime.UtcNow
            },
            new UsageSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantBeta.Id,
                SnapshotType = "Daily",
                SnapshotDate = DateTime.UtcNow,
                ActiveUsers = 5,
                TotalUsers = 5,
                ApiCallsCount = 200,
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var query = new TenantUsageQuery { Days = 7 };

        // Act
        var alphaUsage = await _service.GetTenantUsageAsync(_tenantAlpha.Id, query);
        var betaUsage = await _service.GetTenantUsageAsync(_tenantBeta.Id, query);

        // Assert - Each tenant only sees their own data
        alphaUsage.Value.Summary.TotalApiCalls.Should().Be(1000);
        betaUsage.Value.Summary.TotalApiCalls.Should().Be(200);
    }

    #endregion

    #region License Limit Tests

    [Fact]
    public async Task Tenant_TracksCurrentUsageAgainstLimits()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);

        // Assert
        var tenant = result.Value.Tenant;
        tenant.CurrentBranches.Should().BeLessThanOrEqualTo(tenant.MaxBranches);
        tenant.CurrentUsers.Should().BeLessThanOrEqualTo(tenant.MaxUsers);
    }

    [Fact]
    public async Task TenantListing_IncludesLicenseCapacity()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Items.Should().AllSatisfy(t =>
        {
            t.MaxBranches.Should().BeGreaterThan(0);
            t.MaxUsers.Should().BeGreaterThan(0);
            t.CurrentBranches.Should().BeGreaterThanOrEqualTo(0);
            t.CurrentUsers.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    #endregion

    #region Tenant Lifecycle Tests

    [Fact]
    public async Task TenantLifecycle_TrialToActive()
    {
        // Arrange - Start with trial tenant

        // Act - Activate the tenant
        var result = await _service.ActivateTenantAsync(_tenantTrial.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantTrial.Id);
        tenant!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task TenantLifecycle_ActiveToSuspended()
    {
        // Act
        var result = await _service.SuspendTenantAsync(_tenantAlpha.Id,
            new SuspendTenantRequest { Reason = "Payment issue" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantAlpha.Id);
        tenant!.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public async Task TenantLifecycle_SuspendedToActive()
    {
        // Act
        var result = await _service.ActivateTenantAsync(_tenantSuspended.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantSuspended.Id);
        tenant!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task TenantLifecycle_ExpiredToTrialExtension()
    {
        // Act
        var result = await _service.ExtendTrialAsync(_tenantExpired.Id,
            new ExtendTrialRequest { Days = 14, Reason = "Extension request" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = await _context.Tenants.FindAsync(_tenantExpired.Id);
        tenant!.Status.Should().Be(TenantStatus.Trial);
    }

    #endregion

    #region Trial Expiration Tests

    [Fact]
    public void Tenant_TrialDaysRemaining_CalculatedCorrectly()
    {
        // Assert
        _tenantTrial.TrialDaysRemaining.Should().BeGreaterThan(0);
        _tenantTrial.TrialDaysRemaining.Should().BeLessThanOrEqualTo(20);
        _tenantTrial.IsTrialExpired.Should().BeFalse();
    }

    [Fact]
    public void Tenant_IsTrialExpired_TrueWhenPastEndDate()
    {
        // Arrange
        var expiredTenant = new Tenant
        {
            Status = TenantStatus.Trial,
            TrialEndDate = DateTime.UtcNow.AddDays(-5)
        };

        // Assert
        expiredTenant.IsTrialExpired.Should().BeTrue();
        expiredTenant.TrialDaysRemaining.Should().Be(0);
    }

    [Fact]
    public void Tenant_IsTrialExpired_FalseWhenActive()
    {
        // Assert - Active tenants are not in trial
        _tenantAlpha.IsTrialExpired.Should().BeFalse();
    }

    #endregion

    #region Database Provisioning Tests

    [Fact]
    public async Task TenantListing_IncludesProvisioningStatus()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Items.Should().AllSatisfy(t => t.IsDatabaseProvisioned.Should().BeTrue());
    }

    [Fact]
    public async Task TenantDetails_IncludesProvisioningTimestamp()
    {
        // Act
        var result = await _service.GetTenantDetailsAsync(_tenantAlpha.Id);

        // Assert
        result.Value.Tenant.IsDatabaseProvisioned.Should().BeTrue();
        result.Value.Tenant.DatabaseProvisionedAt.Should().NotBeNull();
    }

    #endregion

    #region Sorting and Ordering Tests

    [Fact]
    public async Task TenantListing_OrdersByCreatedAtDescending()
    {
        // Arrange
        var query = new TenantListQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetTenantsAsync(query);

        // Assert
        result.Items.Should().BeInDescendingOrder(t => t.CreatedAt);
    }

    #endregion
}
