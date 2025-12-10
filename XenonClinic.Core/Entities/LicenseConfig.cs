namespace XenonClinic.Core.Entities;

public class LicenseConfig
{
    public int Id { get; set; }
    public int MaxBranches { get; set; }
    public int MaxUsers { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LicenseKey { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
