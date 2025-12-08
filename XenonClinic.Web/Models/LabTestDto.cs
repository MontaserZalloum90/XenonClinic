using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class LabTestDto
{
    public int Id { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCategory Category { get; set; }
    public SpecimenType? SpecimenType { get; set; }
    public int? TurnaroundTimeHours { get; set; }
    public decimal Price { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresFasting { get; set; }
    public string? ExternalLabName { get; set; }
    public int? ExternalLabId { get; set; }
}
