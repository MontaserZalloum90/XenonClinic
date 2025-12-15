using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class LabResult : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation.
    /// Required for all transactional entities.
    /// </summary>
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

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

    /// <summary>
    /// Alias for ResultDate for service compatibility.
    /// NotMapped - computed property that maps to ResultDate.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? TestDate
    {
        get => ResultDate;
        set => ResultDate = value;
    }

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

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}
