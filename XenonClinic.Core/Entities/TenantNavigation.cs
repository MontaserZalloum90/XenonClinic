namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level navigation overrides
/// </summary>
public class TenantNavigation
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string NavigationJson { get; set; } = "[]";

    // Navigation property
    public Tenant Tenant { get; set; } = null!;
}
