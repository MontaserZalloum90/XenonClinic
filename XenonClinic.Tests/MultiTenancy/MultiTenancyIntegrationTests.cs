using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Middleware;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.MultiTenancy;

/// <summary>
/// Integration tests that verify the full multi-tenancy stack works together:
/// - Tenant Resolution Middleware
/// - Tenant Context Accessor
/// - Global Query Filters in DbContext
/// - Service-level tenant isolation
/// </summary>
public class MultiTenancyIntegrationTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly IMemoryCache _cache;
    private readonly TenantContextAccessor _tenantContextAccessor;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<TenantService>> _loggerMock;

    // Test data
    private Tenant _tenantA = null!;
    private Tenant _tenantB = null!;
    private Company _companyA1 = null!;
    private Company _companyB1 = null!;
    private ApplicationUser _userA = null!;
    private ApplicationUser _userB = null!;
    private ApplicationUser _superAdmin = null!;

    public MultiTenancyIntegrationTests()
    {
        _tenantContextAccessor = new TenantContextAccessor();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _userManagerMock = CreateUserManagerMock();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<TenantService>>();

        // Create DbContext WITH tenant context accessor for global query filters
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options, _tenantContextAccessor);

        SetupTestData();
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Test Setup

    private void SetupTestData()
    {
        // Create tenants
        _tenantA = new Tenant
        {
            Id = 1,
            Name = "Tenant A",
            Code = "TENANT-A",
            IsActive = true,
            MaxCompanies = 5,
            MaxUsersPerTenant = 50
        };

        _tenantB = new Tenant
        {
            Id = 2,
            Name = "Tenant B",
            Code = "TENANT-B",
            IsActive = true,
            MaxCompanies = 3,
            MaxUsersPerTenant = 30
        };

        _context.Tenants.AddRange(_tenantA, _tenantB);

        // Create companies
        _companyA1 = new Company
        {
            Id = 1,
            TenantId = _tenantA.Id,
            Name = "Company A1",
            Code = "COMP-A1",
            IsActive = true
        };

        _companyB1 = new Company
        {
            Id = 2,
            TenantId = _tenantB.Id,
            Name = "Company B1",
            Code = "COMP-B1",
            IsActive = true
        };

        _context.Companies.AddRange(_companyA1, _companyB1);

        // Create users
        _superAdmin = new ApplicationUser
        {
            Id = "super-admin",
            UserName = "superadmin@xenon.com",
            TenantId = null,
            IsSuperAdmin = true,
            IsActive = true
        };

        _userA = new ApplicationUser
        {
            Id = "user-a",
            UserName = "user@tenant-a.com",
            TenantId = _tenantA.Id,
            CompanyId = _companyA1.Id,
            IsSuperAdmin = false,
            IsActive = true
        };

        _userB = new ApplicationUser
        {
            Id = "user-b",
            UserName = "user@tenant-b.com",
            TenantId = _tenantB.Id,
            CompanyId = _companyB1.Id,
            IsSuperAdmin = false,
            IsActive = true
        };

        _context.Users.AddRange(_superAdmin, _userA, _userB);
        _context.SaveChanges();
    }

    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private void SetupCurrentUser(ApplicationUser user, params string[] roles)
    {
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(user.Id);

        foreach (var role in roles)
        {
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, role))
                .ReturnsAsync(true);
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        }, "TestAuth");

        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
    }

    #endregion

    #region Tenant Context Accessor Tests

    [Fact]
    public void TenantContextAccessor_SetsAndGetsTenantContext()
    {
        // Act
        _tenantContextAccessor.SetTenantContext(tenantId: 1, companyId: 2, isSuperAdmin: false);

        // Assert
        _tenantContextAccessor.TenantId.Should().Be(1);
        _tenantContextAccessor.CompanyId.Should().Be(2);
        _tenantContextAccessor.IsSuperAdmin.Should().BeFalse();
        _tenantContextAccessor.ShouldFilterByTenant.Should().BeTrue();
    }

    [Fact]
    public void TenantContextAccessor_SuperAdminBypassesFiltering()
    {
        // Act
        _tenantContextAccessor.SetTenantContext(tenantId: null, companyId: null, isSuperAdmin: true);

        // Assert
        _tenantContextAccessor.TenantId.Should().BeNull();
        _tenantContextAccessor.IsSuperAdmin.Should().BeTrue();
        _tenantContextAccessor.ShouldFilterByTenant.Should().BeFalse();
    }

    [Fact]
    public void TenantContextAccessor_ClearsContext()
    {
        // Arrange
        _tenantContextAccessor.SetTenantContext(tenantId: 1, companyId: 2, isSuperAdmin: false);

        // Act
        _tenantContextAccessor.Clear();

        // Assert
        _tenantContextAccessor.TenantId.Should().BeNull();
        _tenantContextAccessor.CompanyId.Should().BeNull();
        _tenantContextAccessor.IsSuperAdmin.Should().BeFalse();
    }

    #endregion

    #region Global Query Filter Tests

    [Fact]
    public async Task GlobalQueryFilter_FiltersCompaniesByTenant()
    {
        // Arrange - Set context to Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        // Act
        var companies = await _context.Companies.ToListAsync();

        // Assert - Should only see Tenant A's companies
        companies.Should().HaveCount(1);
        companies.First().TenantId.Should().Be(_tenantA.Id);
        companies.First().Name.Should().Be("Company A1");
    }

    [Fact]
    public async Task GlobalQueryFilter_SuperAdminSeesAllCompanies()
    {
        // Arrange - Set context as super admin
        _tenantContextAccessor.SetTenantContext(
            tenantId: null,
            companyId: null,
            isSuperAdmin: true);

        // Act
        var companies = await _context.Companies.ToListAsync();

        // Assert - Super admin sees all companies
        companies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GlobalQueryFilter_FiltersUsersByTenant()
    {
        // Arrange - Set context to Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        // Act
        var users = await _context.Users.ToListAsync();

        // Assert - Should see Tenant A users + super admin (TenantId = null)
        users.Should().Contain(u => u.Id == _userA.Id);
        users.Should().Contain(u => u.Id == _superAdmin.Id); // Super admins visible
        users.Should().NotContain(u => u.Id == _userB.Id); // Tenant B user not visible
    }

    [Fact]
    public async Task GlobalQueryFilter_IgnoreQueryFiltersShowsAll()
    {
        // Arrange - Set context to Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        // Act - Use IgnoreQueryFilters to bypass
        var companies = await _context.Companies
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert - Should see all companies
        companies.Should().HaveCount(2);
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public async Task TenantService_RespectsGlobalFilters()
    {
        // Arrange
        SetupCurrentUser(_userA);
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        var service = CreateTenantService();

        // Act
        var tenant = await service.GetCurrentTenantAsync();

        // Assert
        tenant.Should().NotBeNull();
        tenant!.Id.Should().Be(_tenantA.Id);
    }

    [Fact]
    public async Task TenantService_CanCreateCompany_WithPessimisticLocking()
    {
        // Arrange
        SetupCurrentUser(_userA);
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        var service = CreateTenantService();

        // Act
        var canCreate = await service.CanCreateCompanyAsync(_tenantA.Id);

        // Assert - Tenant A has 1 company, max is 5
        canCreate.Should().BeTrue();
    }

    [Fact]
    public async Task TenantService_CanCreateUser_EnforcesLimits()
    {
        // Arrange
        SetupCurrentUser(_userA);
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        var service = CreateTenantService();

        // Act
        var canCreate = await service.CanCreateUserAsync(_tenantA.Id);

        // Assert - Tenant A has 1 user, max is 50
        canCreate.Should().BeTrue();
    }

    #endregion

    #region Cross-Tenant Prevention Tests

    [Fact]
    public async Task TenantA_CannotAccessTenantB_Data()
    {
        // Arrange - Set context to Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        // Act - Try to query Tenant B's company directly
        var tenantBCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == _companyB1.Id);

        // Assert - Should not find it due to global filter
        tenantBCompany.Should().BeNull();
    }

    [Fact]
    public async Task TenantA_CannotAccessTenantB_Users()
    {
        // Arrange - Set context to Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        // Act
        var tenantBUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _userB.Id);

        // Assert - Should not find Tenant B's user
        tenantBUser.Should().BeNull();
    }

    [Fact]
    public async Task SuperAdmin_CanAccessAllTenantData()
    {
        // Arrange - Set context as super admin
        _tenantContextAccessor.SetTenantContext(
            tenantId: null,
            companyId: null,
            isSuperAdmin: true);

        // Act
        var allCompanies = await _context.Companies.ToListAsync();
        var allUsers = await _context.Users.ToListAsync();

        // Assert
        allCompanies.Should().HaveCount(2);
        allUsers.Should().HaveCount(3);
    }

    #endregion

    #region Context Switching Tests

    [Fact]
    public async Task SwitchingTenantContext_ChangesVisibleData()
    {
        // Arrange & Act - First as Tenant A
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantA.Id,
            companyId: _companyA1.Id,
            isSuperAdmin: false);

        var companiesAsTenantA = await _context.Companies.ToListAsync();

        // Switch to Tenant B
        _tenantContextAccessor.Clear();
        _tenantContextAccessor.SetTenantContext(
            tenantId: _tenantB.Id,
            companyId: _companyB1.Id,
            isSuperAdmin: false);

        var companiesAsTenantB = await _context.Companies.ToListAsync();

        // Assert
        companiesAsTenantA.Should().HaveCount(1);
        companiesAsTenantA.First().TenantId.Should().Be(_tenantA.Id);

        companiesAsTenantB.Should().HaveCount(1);
        companiesAsTenantB.First().TenantId.Should().Be(_tenantB.Id);
    }

    #endregion

    #region Audit Logging Tests

    [Fact]
    public async Task TenantService_LogsAuditOnCreate()
    {
        // Arrange
        SetupCurrentUser(_superAdmin);
        _userManagerMock.Setup(m => m.IsInRoleAsync(_superAdmin, "SuperAdmin"))
            .ReturnsAsync(true);
        _tenantContextAccessor.SetTenantContext(null, null, true);

        var service = CreateTenantService();

        var newTenant = new Tenant
        {
            Name = "New Test Tenant",
            Code = "NEW-TEST",
            MaxCompanies = 5,
            MaxUsersPerTenant = 50
        };

        // Act
        await service.CreateTenantAsync(newTenant);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(It.Is<AuditEntry>(e =>
                e.EntityType == nameof(Tenant) &&
                e.Action == AuditAction.Create)),
            Times.Once);
    }

    [Fact]
    public async Task TenantService_LogsAuditOnDeactivate()
    {
        // Arrange
        SetupCurrentUser(_superAdmin);
        _userManagerMock.Setup(m => m.IsInRoleAsync(_superAdmin, "SuperAdmin"))
            .ReturnsAsync(true);
        _tenantContextAccessor.SetTenantContext(null, null, true);

        var service = CreateTenantService();

        // Act
        await service.DeactivateTenantAsync(_tenantA.Id);

        // Assert
        _auditServiceMock.Verify(
            a => a.LogAsync(It.Is<AuditEntry>(e =>
                e.EntityType == nameof(Tenant) &&
                e.Action == AuditAction.SoftDelete)),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private TenantService CreateTenantService()
    {
        return new TenantService(
            _httpContextAccessorMock.Object,
            _userManagerMock.Object,
            _context,
            _cache,
            _auditServiceMock.Object,
            _loggerMock.Object);
    }

    #endregion
}
