using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class LabResult
{
    public int Id { get; set; }

    [Required]
    public int LabOrderId { get; set; }
    public LabOrder? LabOrder { get; set; }

    [Required]
    public int LabOrderItemId { get; set; }
    public LabOrderItem? LabOrderItem { get; set; }

    [Required]
    public int LabTestId { get; set; }
    public LabTest? LabTest { get; set; }

    [Required]
    public LabResultStatus Status { get; set; } = LabResultStatus.Pending;

    public DateTime? ResultDate { get; set; }

    [MaxLength(100)]
    public string? ResultValue { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public bool IsAbnormal { get; set; } = false;

    public string? Interpretation { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }

    [MaxLength(450)]
    public string? PerformedBy { get; set; }

    public DateTime? PerformedDate { get; set; }

    [MaxLength(450)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    [MaxLength(450)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }

    // Audit fields
    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
