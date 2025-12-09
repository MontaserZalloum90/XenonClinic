namespace XenonClinic.Core.Entities;

/// <summary>
/// Interface for entities that support soft delete.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

/// <summary>
/// Interface for entities with audit timestamps.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
    string? ModifiedBy { get; set; }
}

/// <summary>
/// Base class for entities with soft delete and audit support.
/// </summary>
public abstract class AuditableEntity : ISoftDelete, IAuditable
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Base class for entities with ID, soft delete, and audit support.
/// </summary>
public abstract class AuditableEntityWithId : AuditableEntity
{
    public int Id { get; set; }
}
