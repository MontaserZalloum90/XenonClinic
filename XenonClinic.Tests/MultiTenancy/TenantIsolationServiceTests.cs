using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.MultiTenancy;

/// <summary>
/// Tests for TenantIsolationService which validates tenant data isolation at runtime.
/// </summary>
public class TenantIsolationServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Mock<ITenantContextAccessor> _tenantContextMock;
    private readonly Mock<ILogger<TenantIsolationService>> _loggerMock;
    private readonly TenantIsolationService _service;

    // Test data
    private Tenant _tenantA = null!;
    private Tenant _tenantB = null!;
    private Company _companyA = null!;
    private Company _companyB = null!;
    private Branch _branchA1 = null!;
    private Branch _branchA2 = null!;
    private Branch _branchB1 = null!;
    private ApplicationUser _userA = null!;

    public TenantIsolationServiceTests()
    {
        _tenantContextMock = new Mock<ITenantContextAccessor>();
        _loggerMock = new Mock<ILogger<TenantIsolationService>>();

        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Create context without tenant filtering for setup
        _context = new ClinicDbContext(options);

        SetupTestData();

        _service = new TenantIsolationService(_context, _tenantContextMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SetupTestData()
    {
        // Tenants
        _tenantA = new Tenant { Id = 1, Name = "Tenant A", Code = "TENANT-A", IsActive = true };
        _tenantB = new Tenant { Id = 2, Name = "Tenant B", Code = "TENANT-B", IsActive = true };
        _context.Tenants.AddRange(_tenantA, _tenantB);

        // Companies
        _companyA = new Company { Id = 1, TenantId = 1, Name = "Company A", Code = "COMP-A", IsActive = true };
        _companyB = new Company { Id = 2, TenantId = 2, Name = "Company B", Code = "COMP-B", IsActive = true };
        _context.Companies.AddRange(_companyA, _companyB);

        // Branches
        _branchA1 = new Branch { Id = 1, CompanyId = 1, Code = "BR-A1", Name = "Branch A1", IsActive = true };
        _branchA2 = new Branch { Id = 2, CompanyId = 1, Code = "BR-A2", Name = "Branch A2", IsActive = true };
        _branchB1 = new Branch { Id = 3, CompanyId = 2, Code = "BR-B1", Name = "Branch B1", IsActive = true };
        _context.Branches.AddRange(_branchA1, _branchA2, _branchB1);

        // User
        _userA = new ApplicationUser
        {
            Id = "user-a",
            UserName = "usera@test.com",
            TenantId = 1,
            CompanyId = 1
        };
        _context.Users.Add(_userA);

        // User branch assignments
        _context.UserBranches.Add(new UserBranch { UserId = "user-a", BranchId = 1 });
        _context.UserBranches.Add(new UserBranch { UserId = "user-a", BranchId = 2 });

        _context.SaveChanges();
    }

    private void SetupTenantContext(int? tenantId, bool isSuperAdmin = false)
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.IsSuperAdmin).Returns(isSuperAdmin);
    }

    #region ValidateBranchAccessAsync Tests

    [Fact]
    public async Task ValidateBranchAccessAsync_SuperAdmin_AlwaysReturnsTrue()
    {
        // Arrange
        SetupTenantContext(tenantId: null, isSuperAdmin: true);

        // Act - Try to access any branch
        var result = await _service.ValidateBranchAccessAsync(_branchB1.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_TenantA_CanAccessOwnBranch()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var result = await _service.ValidateBranchAccessAsync(_branchA1.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_TenantA_CannotAccessTenantBBranch()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var result = await _service.ValidateBranchAccessAsync(_branchB1.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_NoTenantContext_ReturnsFalse()
    {
        // Arrange
        SetupTenantContext(tenantId: null, isSuperAdmin: false);

        // Act
        var result = await _service.ValidateBranchAccessAsync(_branchA1.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_NonExistentBranch_ReturnsFalse()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var result = await _service.ValidateBranchAccessAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ValidateCompanyAccessAsync Tests

    [Fact]
    public async Task ValidateCompanyAccessAsync_SuperAdmin_AlwaysReturnsTrue()
    {
        // Arrange
        SetupTenantContext(tenantId: null, isSuperAdmin: true);

        // Act
        var result = await _service.ValidateCompanyAccessAsync(_companyB.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCompanyAccessAsync_TenantA_CanAccessOwnCompany()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var result = await _service.ValidateCompanyAccessAsync(_companyA.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCompanyAccessAsync_TenantA_CannotAccessTenantBCompany()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var result = await _service.ValidateCompanyAccessAsync(_companyB.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetTenantIdForBranchAsync Tests

    [Fact]
    public async Task GetTenantIdForBranchAsync_ReturnsTenantId()
    {
        // Act
        var tenantId = await _service.GetTenantIdForBranchAsync(_branchA1.Id);

        // Assert
        tenantId.Should().Be(1);
    }

    [Fact]
    public async Task GetTenantIdForBranchAsync_NonExistentBranch_ReturnsNull()
    {
        // Act
        var tenantId = await _service.GetTenantIdForBranchAsync(9999);

        // Assert
        tenantId.Should().BeNull();
    }

    #endregion

    #region GetAccessibleBranchIdsAsync Tests

    [Fact]
    public async Task GetAccessibleBranchIdsAsync_SuperAdmin_ReturnsAllBranches()
    {
        // Arrange
        SetupTenantContext(tenantId: null, isSuperAdmin: true);

        // Act
        var branchIds = await _service.GetAccessibleBranchIdsAsync("any-user");

        // Assert
        branchIds.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAccessibleBranchIdsAsync_RegularUser_ReturnsAssignedBranches()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var branchIds = await _service.GetAccessibleBranchIdsAsync("user-a");

        // Assert
        branchIds.Should().HaveCount(2);
        branchIds.Should().Contain(1);
        branchIds.Should().Contain(2);
    }

    [Fact]
    public async Task GetAccessibleBranchIdsAsync_UserWithNoBranches_ReturnsEmpty()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        // Act
        var branchIds = await _service.GetAccessibleBranchIdsAsync("non-existent-user");

        // Assert
        branchIds.Should().BeEmpty();
    }

    #endregion

    #region ValidateCrossEntityRelationshipAsync Tests

    [Fact]
    public async Task ValidateCrossEntityRelationshipAsync_SameBranch_IsValid()
    {
        // Act
        var result = await _service.ValidateCrossEntityRelationshipAsync(
            sourceBranchId: _branchA1.Id,
            targetBranchId: _branchA1.Id,
            relationshipDescription: "Patient to Appointment");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCrossEntityRelationshipAsync_SameTenantDifferentBranches_IsValid()
    {
        // Act - Both branches belong to Tenant A
        var result = await _service.ValidateCrossEntityRelationshipAsync(
            sourceBranchId: _branchA1.Id,
            targetBranchId: _branchA2.Id,
            relationshipDescription: "Transfer between branches");

        // Assert
        result.IsValid.Should().BeTrue();
        result.SourceTenantId.Should().Be(result.TargetTenantId);
    }

    [Fact]
    public async Task ValidateCrossEntityRelationshipAsync_DifferentTenants_IsInvalid()
    {
        // Act - Branches belong to different tenants
        var result = await _service.ValidateCrossEntityRelationshipAsync(
            sourceBranchId: _branchA1.Id,
            targetBranchId: _branchB1.Id,
            relationshipDescription: "Cross-tenant attempt");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ViolationDescription.Should().NotBeNullOrEmpty();
        result.SourceTenantId.Should().Be(1);
        result.TargetTenantId.Should().Be(2);
    }

    #endregion

    #region ValidateEntityBranchAsync Tests

    [Fact]
    public async Task ValidateEntityBranchAsync_SuperAdmin_AlwaysReturnsTrue()
    {
        // Arrange
        SetupTenantContext(tenantId: null, isSuperAdmin: true);

        // Add a patient to branch B1
        var patient = new Patient
        {
            Id = 1,
            BranchId = _branchB1.Id,
            EmiratesId = "123",
            FullNameEn = "Test Patient",
            DateOfBirth = DateTime.Now.AddYears(-30)
        };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateEntityBranchAsync<Patient>(patient.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEntityBranchAsync_TenantA_CanAccessOwnPatient()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        var patient = new Patient
        {
            Id = 2,
            BranchId = _branchA1.Id,
            EmiratesId = "124",
            FullNameEn = "Tenant A Patient",
            DateOfBirth = DateTime.Now.AddYears(-25)
        };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateEntityBranchAsync<Patient>(patient.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEntityBranchAsync_TenantA_CannotAccessTenantBPatient()
    {
        // Arrange
        SetupTenantContext(tenantId: 1);

        var patient = new Patient
        {
            Id = 3,
            BranchId = _branchB1.Id,
            EmiratesId = "125",
            FullNameEn = "Tenant B Patient",
            DateOfBirth = DateTime.Now.AddYears(-20)
        };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateEntityBranchAsync<Patient>(patient.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AuditEntityIsolationAsync Tests

    [Fact]
    public async Task AuditEntityIsolationAsync_Patient_ReturnsCorrectStats()
    {
        // Arrange
        var patients = new[]
        {
            new Patient { Id = 10, BranchId = _branchA1.Id, EmiratesId = "P1", FullNameEn = "P1", DateOfBirth = DateTime.Now },
            new Patient { Id = 11, BranchId = _branchA2.Id, EmiratesId = "P2", FullNameEn = "P2", DateOfBirth = DateTime.Now },
            new Patient { Id = 12, BranchId = _branchB1.Id, EmiratesId = "P3", FullNameEn = "P3", DateOfBirth = DateTime.Now }
        };
        _context.Patients.AddRange(patients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AuditEntityIsolationAsync<Patient>();

        // Assert
        result.EntityType.Should().Be("Patient");
        result.HasBranchId.Should().BeTrue();
        result.TotalRecords.Should().Be(3);
        result.TenantDistribution.Should().ContainKey(1);
        result.TenantDistribution.Should().ContainKey(2);
        result.TenantDistribution[1].Should().Be(2); // 2 patients in Tenant A
        result.TenantDistribution[2].Should().Be(1); // 1 patient in Tenant B
    }

    [Fact]
    public async Task AuditEntityIsolationAsync_EntityWithoutBranchId_ReturnsAppropriateResult()
    {
        // Act - Tenant entity doesn't have BranchId
        var result = await _service.AuditEntityIsolationAsync<Tenant>();

        // Assert
        result.HasBranchId.Should().BeFalse();
        result.Notes.Should().Contain("does not have BranchId");
    }

    #endregion
}
