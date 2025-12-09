namespace XenonClinic.Core.Entities;

/// <summary>
/// Defines the type of company (CLINIC, TRADING, etc.)
/// </summary>
public class CompanyType
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    // Navigation
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public CompanyTypeTemplate? Template { get; set; }
}
