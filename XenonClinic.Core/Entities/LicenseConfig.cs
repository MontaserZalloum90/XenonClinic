namespace XenonClinic.Core.Entities;

public class LicenseConfig
{
    public int Id { get; set; }
    public int MaxBranches { get; set; }
    public int MaxUsers { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LicenseKey { get; set; }
}
