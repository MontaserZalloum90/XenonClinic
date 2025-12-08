using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class LabOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public LabOrderStatus Status { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string? ExternalLabName { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public bool IsUrgent { get; set; }
    public int TestCount { get; set; }
    public int CompletedTests { get; set; }
    public DateTime? CollectionDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
}
