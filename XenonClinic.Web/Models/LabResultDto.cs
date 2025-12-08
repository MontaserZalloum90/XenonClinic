using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class LabResultDto
{
    public int Id { get; set; }
    public int LabOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public LabResultStatus Status { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Interpretation { get; set; }
    public string? PerformedBy { get; set; }
    public string? ReviewedBy { get; set; }
    public string? VerifiedBy { get; set; }
}
