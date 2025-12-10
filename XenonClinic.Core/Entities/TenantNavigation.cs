namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level navigation overrides
/// </summary>
public class TenantNavigation
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string NavigationJson { get; set; } = "[]";

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation property
    public Tenant Tenant { get; set; } = null!;
}
