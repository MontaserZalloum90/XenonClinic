namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Marker interface for entities that belong to a specific branch.
/// Entities implementing this interface will have automatic branch-level query filtering
/// applied by the DbContext for multi-tenant data isolation.
/// </summary>
public interface IBranchEntity
{
    /// <summary>
    /// The ID of the branch this entity belongs to.
    /// This property is used by global query filters to ensure tenant isolation.
    /// </summary>
    int BranchId { get; set; }
}
