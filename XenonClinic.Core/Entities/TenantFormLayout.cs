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

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
