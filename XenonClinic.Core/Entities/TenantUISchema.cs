namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level UI schema overrides for entities
/// </summary>
public class TenantUISchema
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string SchemaJson { get; set; } = "{}";

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
