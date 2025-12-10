using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Provides synchronous access to tenant context for use in DbContext query filters.
/// This is a scoped service that stores tenant information resolved by middleware.
/// Thread-safe for the duration of a single HTTP request.
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private int? _tenantId;
    private int? _companyId;
    private int? _branchId;
    private IReadOnlyList<int>? _accessibleBranchIds;
    private bool _isSuperAdmin;
    private bool _isCompanyAdmin;
    private bool _isSet;

    /// <inheritdoc />
    public int? TenantId => _tenantId;

    /// <inheritdoc />
    public int? CompanyId => _companyId;

    /// <inheritdoc />
    public int? BranchId => _branchId;

    /// <inheritdoc />
    public IReadOnlyList<int>? AccessibleBranchIds => _accessibleBranchIds;

    /// <inheritdoc />
    public bool IsSuperAdmin => _isSuperAdmin;

    /// <inheritdoc />
    public bool IsCompanyAdmin => _isCompanyAdmin;

    /// <inheritdoc />
    public bool ShouldFilterByTenant => !_isSuperAdmin && _tenantId.HasValue;

    /// <inheritdoc />
    public bool ShouldFilterByBranch => !_isSuperAdmin && !_isCompanyAdmin &&
        (_branchId.HasValue || (_accessibleBranchIds?.Count > 0));

    /// <inheritdoc />
    public bool HasBranchAccess(int branchId)
    {
        // Super admins have access to all branches
        if (_isSuperAdmin)
            return true;

        // Company admins have access to all branches in their company
        // (Branch validation against company is done elsewhere)
        if (_isCompanyAdmin)
            return true;

        // Check if user has specific branch access
        if (_branchId.HasValue && _branchId.Value == branchId)
            return true;

        // Check if branch is in accessible branches list
        if (_accessibleBranchIds != null && _accessibleBranchIds.Contains(branchId))
            return true;

        return false;
    }

    /// <inheritdoc />
    public void SetTenantContext(int? tenantId, int? companyId, bool isSuperAdmin)
    {
        _tenantId = tenantId;
        _companyId = companyId;
        _isSuperAdmin = isSuperAdmin;
        _isSet = true;
    }

    /// <inheritdoc />
    public void SetContext(int? tenantId, int? companyId, int? branchId,
        IReadOnlyList<int>? accessibleBranchIds, bool isSuperAdmin, bool isCompanyAdmin)
    {
        _tenantId = tenantId;
        _companyId = companyId;
        _branchId = branchId;
        _accessibleBranchIds = accessibleBranchIds;
        _isSuperAdmin = isSuperAdmin;
        _isCompanyAdmin = isCompanyAdmin;
        _isSet = true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _tenantId = null;
        _companyId = null;
        _branchId = null;
        _accessibleBranchIds = null;
        _isSuperAdmin = false;
        _isCompanyAdmin = false;
        _isSet = false;
    }

    /// <summary>
    /// Indicates whether the tenant context has been set by middleware.
    /// </summary>
    public bool IsContextSet => _isSet;
}
