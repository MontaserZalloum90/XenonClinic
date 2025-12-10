using FluentAssertions;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.MultiTenancy;

/// <summary>
/// Unit tests for TenantContextAccessor including branch-level context.
/// </summary>
public class TenantContextAccessorTests
{
    private readonly TenantContextAccessor _accessor;

    public TenantContextAccessorTests()
    {
        _accessor = new TenantContextAccessor();
    }

    #region Basic Context Tests

    [Fact]
    public void SetTenantContext_SetsBasicProperties()
    {
        // Act
        _accessor.SetTenantContext(tenantId: 1, companyId: 2, isSuperAdmin: false);

        // Assert
        _accessor.TenantId.Should().Be(1);
        _accessor.CompanyId.Should().Be(2);
        _accessor.IsSuperAdmin.Should().BeFalse();
        _accessor.IsContextSet.Should().BeTrue();
    }

    [Fact]
    public void SetContext_SetsFullContext()
    {
        // Arrange
        var accessibleBranches = new List<int> { 1, 2, 3 };

        // Act
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 3,
            accessibleBranchIds: accessibleBranches,
            isSuperAdmin: false,
            isCompanyAdmin: true);

        // Assert
        _accessor.TenantId.Should().Be(1);
        _accessor.CompanyId.Should().Be(2);
        _accessor.BranchId.Should().Be(3);
        _accessor.AccessibleBranchIds.Should().BeEquivalentTo(accessibleBranches);
        _accessor.IsSuperAdmin.Should().BeFalse();
        _accessor.IsCompanyAdmin.Should().BeTrue();
        _accessor.IsContextSet.Should().BeTrue();
    }

    [Fact]
    public void Clear_ResetsAllProperties()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 3,
            accessibleBranchIds: new List<int> { 1, 2 },
            isSuperAdmin: false,
            isCompanyAdmin: true);

        // Act
        _accessor.Clear();

        // Assert
        _accessor.TenantId.Should().BeNull();
        _accessor.CompanyId.Should().BeNull();
        _accessor.BranchId.Should().BeNull();
        _accessor.AccessibleBranchIds.Should().BeNull();
        _accessor.IsSuperAdmin.Should().BeFalse();
        _accessor.IsCompanyAdmin.Should().BeFalse();
        _accessor.IsContextSet.Should().BeFalse();
    }

    #endregion

    #region ShouldFilterByTenant Tests

    [Fact]
    public void ShouldFilterByTenant_TenantSet_ReturnsTrue()
    {
        // Arrange
        _accessor.SetTenantContext(tenantId: 1, companyId: 2, isSuperAdmin: false);

        // Assert
        _accessor.ShouldFilterByTenant.Should().BeTrue();
    }

    [Fact]
    public void ShouldFilterByTenant_SuperAdmin_ReturnsFalse()
    {
        // Arrange
        _accessor.SetTenantContext(tenantId: 1, companyId: 2, isSuperAdmin: true);

        // Assert
        _accessor.ShouldFilterByTenant.Should().BeFalse();
    }

    [Fact]
    public void ShouldFilterByTenant_NoTenantId_ReturnsFalse()
    {
        // Arrange
        _accessor.SetTenantContext(tenantId: null, companyId: null, isSuperAdmin: false);

        // Assert
        _accessor.ShouldFilterByTenant.Should().BeFalse();
    }

    #endregion

    #region ShouldFilterByBranch Tests

    [Fact]
    public void ShouldFilterByBranch_BranchIdSet_ReturnsTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 3,
            accessibleBranchIds: null,
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeTrue();
    }

    [Fact]
    public void ShouldFilterByBranch_AccessibleBranchesSet_ReturnsTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: new List<int> { 1, 2 },
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeTrue();
    }

    [Fact]
    public void ShouldFilterByBranch_SuperAdmin_ReturnsFalse()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 3,
            accessibleBranchIds: new List<int> { 1, 2, 3 },
            isSuperAdmin: true,
            isCompanyAdmin: false);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeFalse();
    }

    [Fact]
    public void ShouldFilterByBranch_CompanyAdmin_ReturnsFalse()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: null,
            isSuperAdmin: false,
            isCompanyAdmin: true);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeFalse();
    }

    [Fact]
    public void ShouldFilterByBranch_NoBranchContext_ReturnsFalse()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: null,
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeFalse();
    }

    [Fact]
    public void ShouldFilterByBranch_EmptyAccessibleBranches_ReturnsFalse()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: new List<int>(),
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.ShouldFilterByBranch.Should().BeFalse();
    }

    #endregion

    #region HasBranchAccess Tests

    [Fact]
    public void HasBranchAccess_SuperAdmin_AlwaysTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: null,
            companyId: null,
            branchId: null,
            accessibleBranchIds: null,
            isSuperAdmin: true,
            isCompanyAdmin: false);

        // Act & Assert
        _accessor.HasBranchAccess(1).Should().BeTrue();
        _accessor.HasBranchAccess(999).Should().BeTrue();
    }

    [Fact]
    public void HasBranchAccess_CompanyAdmin_AlwaysTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: null,
            isSuperAdmin: false,
            isCompanyAdmin: true);

        // Act & Assert
        _accessor.HasBranchAccess(1).Should().BeTrue();
        _accessor.HasBranchAccess(999).Should().BeTrue();
    }

    [Fact]
    public void HasBranchAccess_CurrentBranchId_ReturnsTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 5,
            accessibleBranchIds: null,
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.HasBranchAccess(5).Should().BeTrue();
        _accessor.HasBranchAccess(6).Should().BeFalse();
    }

    [Fact]
    public void HasBranchAccess_InAccessibleBranches_ReturnsTrue()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: new List<int> { 1, 2, 3 },
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.HasBranchAccess(1).Should().BeTrue();
        _accessor.HasBranchAccess(2).Should().BeTrue();
        _accessor.HasBranchAccess(3).Should().BeTrue();
        _accessor.HasBranchAccess(4).Should().BeFalse();
    }

    [Fact]
    public void HasBranchAccess_NotInAccessibleBranches_ReturnsFalse()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: null,
            accessibleBranchIds: new List<int> { 1, 2 },
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Assert
        _accessor.HasBranchAccess(999).Should().BeFalse();
    }

    [Fact]
    public void HasBranchAccess_NoContext_ReturnsFalse()
    {
        // Act & Assert (fresh accessor)
        _accessor.HasBranchAccess(1).Should().BeFalse();
    }

    #endregion

    #region Context Overwrite Tests

    [Fact]
    public void SetContext_OverwritesPreviousContext()
    {
        // Arrange
        _accessor.SetContext(
            tenantId: 1,
            companyId: 2,
            branchId: 3,
            accessibleBranchIds: new List<int> { 1, 2, 3 },
            isSuperAdmin: false,
            isCompanyAdmin: false);

        // Act - Set different context
        _accessor.SetContext(
            tenantId: 10,
            companyId: 20,
            branchId: 30,
            accessibleBranchIds: new List<int> { 10, 20 },
            isSuperAdmin: true,
            isCompanyAdmin: true);

        // Assert
        _accessor.TenantId.Should().Be(10);
        _accessor.CompanyId.Should().Be(20);
        _accessor.BranchId.Should().Be(30);
        _accessor.AccessibleBranchIds.Should().BeEquivalentTo(new[] { 10, 20 });
        _accessor.IsSuperAdmin.Should().BeTrue();
        _accessor.IsCompanyAdmin.Should().BeTrue();
    }

    #endregion
}
