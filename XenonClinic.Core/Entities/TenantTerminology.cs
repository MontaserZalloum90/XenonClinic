namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level terminology overrides
/// </summary>
public class TenantTerminology
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
