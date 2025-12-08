namespace XenonClinic.Web.Models;

public class ExternalLabDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int? TurnaroundTimeDays { get; set; }
    public bool IsActive { get; set; }
    public int TotalTests { get; set; }
    public int TotalOrders { get; set; }
}
