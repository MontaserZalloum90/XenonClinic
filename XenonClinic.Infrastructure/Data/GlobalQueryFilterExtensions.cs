using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Extension methods for applying global query filters for multi-tenant data isolation.
/// </summary>
public static class GlobalQueryFilterExtensions
{
    /// <summary>
    /// Applies global query filters to all entities implementing IBranchEntity.
    /// The filter ensures that only entities belonging to the user's accessible branches are returned.
    /// Super admins and company admins bypass these filters.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <param name="tenantContextAccessor">The tenant context accessor for determining filter conditions.</param>
    public static void ApplyBranchQueryFilters(
        this ModelBuilder modelBuilder,
        ITenantContextAccessor? tenantContextAccessor)
    {
        if (tenantContextAccessor == null)
            return;

        // Get all entity types implementing IBranchEntity
        var branchEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IBranchEntity).IsAssignableFrom(e.ClrType) && e.ClrType != typeof(IBranchEntity))
            .ToList();

        foreach (var entityType in branchEntityTypes)
        {
            ApplyBranchFilter(modelBuilder, entityType.ClrType, tenantContextAccessor);
        }
    }

    /// <summary>
    /// Applies global query filter to all entities with a BranchId property (convention-based).
    /// This method uses reflection to find all entities with BranchId and applies filters.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <param name="tenantContextAccessor">The tenant context accessor for determining filter conditions.</param>
    public static void ApplyBranchQueryFiltersByConvention(
        this ModelBuilder modelBuilder,
        ITenantContextAccessor? tenantContextAccessor)
    {
        if (tenantContextAccessor == null)
            return;

        // Get all entity types with BranchId property
        var entityTypesWithBranchId = modelBuilder.Model.GetEntityTypes()
            .Where(e => e.ClrType.GetProperty("BranchId") != null)
            .ToList();

        foreach (var entityType in entityTypesWithBranchId)
        {
            var branchIdProperty = entityType.ClrType.GetProperty("BranchId");
            if (branchIdProperty != null && branchIdProperty.PropertyType == typeof(int))
            {
                ApplyBranchFilter(modelBuilder, entityType.ClrType, tenantContextAccessor);
            }
        }
    }

    private static void ApplyBranchFilter(
        ModelBuilder modelBuilder,
        Type entityType,
        ITenantContextAccessor tenantContextAccessor)
    {
        // Create the filter expression: e => !ShouldFilterByBranch || HasBranchAccess(e.BranchId)
        var parameter = Expression.Parameter(entityType, "e");
        var branchIdProperty = entityType.GetProperty("BranchId");

        if (branchIdProperty == null)
            return;

        // Get the BranchId value: e.BranchId
        var branchIdAccess = Expression.Property(parameter, branchIdProperty);

        // Create: !tenantContextAccessor.ShouldFilterByBranch
        var shouldFilterProperty = typeof(ITenantContextAccessor).GetProperty(nameof(ITenantContextAccessor.ShouldFilterByBranch));
        var tenantAccessorConstant = Expression.Constant(tenantContextAccessor);
        var shouldFilterAccess = Expression.Property(tenantAccessorConstant, shouldFilterProperty!);
        var notShouldFilter = Expression.Not(shouldFilterAccess);

        // Create: tenantContextAccessor.HasBranchAccess(e.BranchId)
        var hasBranchAccessMethod = typeof(ITenantContextAccessor).GetMethod(nameof(ITenantContextAccessor.HasBranchAccess));
        var hasBranchAccessCall = Expression.Call(tenantAccessorConstant, hasBranchAccessMethod!, branchIdAccess);

        // Combine: !ShouldFilterByBranch || HasBranchAccess(e.BranchId)
        var filterExpression = Expression.OrElse(notShouldFilter, hasBranchAccessCall);

        // Create lambda: e => !ShouldFilterByBranch || HasBranchAccess(e.BranchId)
        var lambda = Expression.Lambda(filterExpression, parameter);

        // Apply the filter using reflection since we have a runtime type
        var entityMethod = typeof(ModelBuilder).GetMethod(nameof(ModelBuilder.Entity), Type.EmptyTypes);
        var genericEntityMethod = entityMethod!.MakeGenericMethod(entityType);
        var entityBuilder = genericEntityMethod.Invoke(modelBuilder, null);

        var hasQueryFilterMethod = entityBuilder!.GetType().GetMethod("HasQueryFilter");
        hasQueryFilterMethod!.Invoke(entityBuilder, new object[] { lambda });
    }

    /// <summary>
    /// Applies a simple accessible branches filter using Contains().
    /// This is an alternative approach when the branch list is known at filter time.
    /// </summary>
    /// <typeparam name="T">The entity type with BranchId property.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="shouldFilter">Function to determine if filtering should apply.</param>
    /// <param name="getAccessibleBranches">Function to get the accessible branch IDs.</param>
    public static void ApplyBranchContainsFilter<T>(
        this ModelBuilder modelBuilder,
        Func<bool> shouldFilter,
        Func<IReadOnlyList<int>?> getAccessibleBranches) where T : class, IBranchEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            !shouldFilter() ||
            getAccessibleBranches() == null ||
            getAccessibleBranches()!.Contains(e.BranchId));
    }
}
