namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level form layout overrides for entities
/// </summary>
public class TenantFormLayout
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string LayoutJson { get; set; } = "{}";

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
