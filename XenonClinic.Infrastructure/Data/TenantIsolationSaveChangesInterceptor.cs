using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Interceptor that validates tenant isolation rules before saving changes.
/// Prevents accidental cross-tenant data modifications.
/// </summary>
public class TenantIsolationSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ILogger<TenantIsolationSaveChangesInterceptor> _logger;

    public TenantIsolationSaveChangesInterceptor(
        ITenantContextAccessor tenantContext,
        ILogger<TenantIsolationSaveChangesInterceptor> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidateChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ValidateChanges(DbContext? context)
    {
        if (context == null) return;

        // Super admins bypass validation
        if (_tenantContext.IsSuperAdmin) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();

            // Check for BranchId property
            var branchIdProperty = entityType.GetProperty("BranchId");
            if (branchIdProperty != null)
            {
                var branchId = branchIdProperty.GetValue(entry.Entity);
                if (branchId is int branchIdInt)
                {
                    ValidateBranchAccess(branchIdInt, entityType.Name);
                }
            }

            // Check for TenantId property (for tenant-level entities)
            var tenantIdProperty = entityType.GetProperty("TenantId");
            if (tenantIdProperty != null)
            {
                var tenantId = tenantIdProperty.GetValue(entry.Entity);
                if (tenantId is int tenantIdInt)
                {
                    ValidateTenantAccess(tenantIdInt, entityType.Name);
                }
            }
        }
    }

    private void ValidateBranchAccess(int branchId, string entityName)
    {
        if (!_tenantContext.HasBranchAccess(branchId))
        {
            _logger.LogError(
                "Tenant isolation violation: User attempted to modify {EntityName} with BranchId {BranchId} without access. TenantId: {TenantId}",
                entityName, branchId, _tenantContext.TenantId);

            throw new TenantIsolationViolationException(
                $"Access denied: You do not have permission to modify data for branch {branchId}");
        }
    }

    private void ValidateTenantAccess(int tenantId, string entityName)
    {
        if (_tenantContext.TenantId != tenantId)
        {
            _logger.LogError(
                "Tenant isolation violation: User from TenantId {CurrentTenantId} attempted to modify {EntityName} belonging to TenantId {TargetTenantId}",
                _tenantContext.TenantId, entityName, tenantId);

            throw new TenantIsolationViolationException(
                $"Access denied: You do not have permission to modify data for tenant {tenantId}");
        }
    }
}

/// <summary>
/// Exception thrown when tenant isolation rules are violated.
/// </summary>
public class TenantIsolationViolationException : Exception
{
    /// <summary>
    /// The tenant ID of the user attempting the operation.
    /// </summary>
    public string? SourceTenantId { get; }

    /// <summary>
    /// The tenant ID of the resource being accessed.
    /// </summary>
    public string? TargetTenantId { get; }

    public TenantIsolationViolationException(string message) : base(message) { }

    public TenantIsolationViolationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// BUG FIX: Add constructor for two-tenant scenario (for test compatibility and better diagnostics)
    /// </summary>
    public TenantIsolationViolationException(string sourceTenantId, string targetTenantId)
        : base($"Tenant isolation violation: User from tenant '{sourceTenantId}' attempted to access tenant '{targetTenantId}'")
    {
        SourceTenantId = sourceTenantId;
        TargetTenantId = targetTenantId;
    }
}

/// <summary>
/// Extension methods for registering the tenant isolation interceptor.
/// </summary>
public static class TenantIsolationInterceptorExtensions
{
    /// <summary>
    /// Adds the tenant isolation interceptor to the DbContext options.
    /// </summary>
    public static DbContextOptionsBuilder AddTenantIsolationInterceptor(
        this DbContextOptionsBuilder builder,
        ITenantContextAccessor tenantContext,
        ILogger<TenantIsolationSaveChangesInterceptor> logger)
    {
        return builder.AddInterceptors(new TenantIsolationSaveChangesInterceptor(tenantContext, logger));
    }
}
