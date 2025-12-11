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
    /// Gets the current branch ID. Returns null if not set or user has access to all branches.
    /// </summary>
    int? BranchId { get; }

    /// <summary>
    /// Gets the list of branch IDs the current user has access to.
    /// Empty list means no branch access, null means all branches (super admin/company admin).
    /// </summary>
    IReadOnlyList<int>? AccessibleBranchIds { get; }

    /// <summary>
    /// Gets whether the current user is a super admin (bypasses tenant filtering).
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Gets whether the current user is a company admin (can access all branches in company).
    /// </summary>
    bool IsCompanyAdmin { get; }

    /// <summary>
    /// Gets whether tenant filtering should be applied.
    /// Returns false for super admins or when no tenant context is set.
    /// </summary>
    bool ShouldFilterByTenant => !IsSuperAdmin && TenantId.HasValue;

    /// <summary>
    /// Gets whether branch filtering should be applied.
    /// Returns false for super admins, company admins, or when no branch context is set.
    /// </summary>
    bool ShouldFilterByBranch => !IsSuperAdmin && !IsCompanyAdmin && (BranchId.HasValue || AccessibleBranchIds?.Count > 0);

    /// <summary>
    /// Checks if the user has access to a specific branch.
    /// Super admins have access to all branches.
    /// Company admins have access to all branches in their company.
    /// Regular users have access only to their assigned branches.
    /// </summary>
    bool HasBranchAccess(int branchId);

    /// <summary>
    /// Sets the tenant context. Called by middleware after authentication.
    /// </summary>
    void SetTenantContext(int? tenantId, int? companyId, bool isSuperAdmin);

    /// <summary>
    /// Sets the full tenant and branch context. Called by middleware after authentication.
    /// </summary>
    void SetContext(int? tenantId, int? companyId, int? branchId,
        IReadOnlyList<int>? accessibleBranchIds, bool isSuperAdmin, bool isCompanyAdmin);

    /// <summary>
    /// Clears the tenant context. Called at the end of request or for cleanup.
    /// </summary>
    void Clear();
}
