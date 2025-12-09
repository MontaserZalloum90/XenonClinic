using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Extension methods for DbContext to support soft delete and auditing.
/// </summary>
public static class AuditableDbContextExtensions
{
    /// <summary>
    /// Apply soft delete filter to all entities implementing ISoftDelete.
    /// </summary>
    public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var comparison = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Process audit fields before saving changes.
    /// </summary>
    public static void ProcessAuditFields(this DbContext context, string? currentUserId)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            ProcessAuditableEntry(entry, currentUserId);
            ProcessSoftDeleteEntry(entry, currentUserId);
        }
    }

    private static void ProcessAuditableEntry(EntityEntry entry, string? currentUserId)
    {
        if (entry.Entity is not IAuditable auditable) return;

        switch (entry.State)
        {
            case EntityState.Added:
                auditable.CreatedAt = DateTime.UtcNow;
                auditable.CreatedBy = currentUserId;
                break;

            case EntityState.Modified:
                auditable.ModifiedAt = DateTime.UtcNow;
                auditable.ModifiedBy = currentUserId;
                // Prevent overwriting CreatedAt/CreatedBy
                entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                break;
        }
    }

    private static void ProcessSoftDeleteEntry(EntityEntry entry, string? currentUserId)
    {
        if (entry.Entity is not ISoftDelete softDelete) return;

        if (entry.State == EntityState.Deleted)
        {
            // Convert hard delete to soft delete
            entry.State = EntityState.Modified;
            softDelete.IsDeleted = true;
            softDelete.DeletedAt = DateTime.UtcNow;
            softDelete.DeletedBy = currentUserId;
        }
    }

    /// <summary>
    /// Get changed entries for audit logging.
    /// </summary>
    public static List<AuditEntry> GetAuditEntries(this DbContext context, string? currentUserId, string? userName)
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Skip audit entities themselves
            if (entry.Entity is AuditEntry) continue;

            var auditEntry = new AuditEntry
            {
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                UserId = currentUserId,
                UserName = userName,
                Timestamp = DateTime.UtcNow,
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Create,
                    EntityState.Modified => GetModifiedAction(entry),
                    EntityState.Deleted => AuditAction.Delete,
                    _ => AuditAction.Update
                }
            };

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                var propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified when property.IsModified:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.ChangedProperties.Add(propertyName);
                        break;
                }
            }

            if (entry.State != EntityState.Modified || auditEntry.ChangedProperties.Count > 0)
            {
                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties == null || keyProperties.Count == 0)
            return string.Empty;

        var keyValues = keyProperties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "");
        return string.Join(",", keyValues);
    }

    private static AuditAction GetModifiedAction(EntityEntry entry)
    {
        if (entry.Entity is ISoftDelete softDelete)
        {
            var isDeletedProp = entry.Property(nameof(ISoftDelete.IsDeleted));
            if (isDeletedProp.IsModified)
            {
                return (bool)isDeletedProp.CurrentValue! ? AuditAction.SoftDelete : AuditAction.Restore;
            }
        }

        return AuditAction.Update;
    }
}

/// <summary>
/// Repository extension for soft delete operations.
/// </summary>
public interface ISoftDeleteRepository<T> where T : class, ISoftDelete
{
    /// <summary>
    /// Soft delete an entity.
    /// </summary>
    Task SoftDeleteAsync(int id);

    /// <summary>
    /// Restore a soft-deleted entity.
    /// </summary>
    Task RestoreAsync(int id);

    /// <summary>
    /// Get all entities including soft-deleted ones.
    /// </summary>
    Task<IEnumerable<T>> GetAllWithDeletedAsync();

    /// <summary>
    /// Permanently delete an entity.
    /// </summary>
    Task HardDeleteAsync(int id);
}
