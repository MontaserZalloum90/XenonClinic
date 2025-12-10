namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Provides synchronous access to tenant context for use in DbContext query filters.
/// Unlike ICurrentUserContext (which is async), this provides cached synchronous access
/// suitable for EF Core query filters that cannot be async.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant ID. Returns null for super admins or unauthenticated requests.
    /// This is resolved synchronously from a cached value set by middleware.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Gets the current company ID. Returns null if not set.
    /// </summary>
    int? CompanyId { get; }

    /// <summary>
    /// Gets whether the current user is a super admin (bypasses tenant filtering).
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Gets whether tenant filtering should be applied.
    /// Returns false for super admins or when no tenant context is set.
    /// </summary>
    bool ShouldFilterByTenant => !IsSuperAdmin && TenantId.HasValue;

    /// <summary>
    /// Sets the tenant context. Called by middleware after authentication.
    /// </summary>
    void SetTenantContext(int? tenantId, int? companyId, bool isSuperAdmin);

    /// <summary>
    /// Clears the tenant context. Called at the end of request or for cleanup.
    /// </summary>
    void Clear();
}
