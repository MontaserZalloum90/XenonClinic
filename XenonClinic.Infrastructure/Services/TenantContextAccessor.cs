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
    private bool _isSuperAdmin;
    private bool _isSet;

    /// <inheritdoc />
    public int? TenantId => _tenantId;

    /// <inheritdoc />
    public int? CompanyId => _companyId;

    /// <inheritdoc />
    public bool IsSuperAdmin => _isSuperAdmin;

    /// <inheritdoc />
    public bool ShouldFilterByTenant => !_isSuperAdmin && _tenantId.HasValue;

    /// <inheritdoc />
    public void SetTenantContext(int? tenantId, int? companyId, bool isSuperAdmin)
    {
        _tenantId = tenantId;
        _companyId = companyId;
        _isSuperAdmin = isSuperAdmin;
        _isSet = true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _tenantId = null;
        _companyId = null;
        _isSuperAdmin = false;
        _isSet = false;
    }

    /// <summary>
    /// Indicates whether the tenant context has been set by middleware.
    /// </summary>
    public bool IsContextSet => _isSet;
}
