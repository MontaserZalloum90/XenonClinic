using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class LabOrder : IBranchEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public LabOrderStatus Status { get; set; } = LabOrderStatus.Pending;

    [Required]
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Ordering doctor
    public int? OrderingDoctorId { get; set; }
    public Doctor? OrderingDoctor { get; set; }

    // Related visit
    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }

    // External lab if tests are outsourced
    public int? ExternalLabId { get; set; }
    public ExternalLab? ExternalLab { get; set; }

    [MaxLength(450)]
    public string? OrderedBy { get; set; }

    public DateTime? CollectionDate { get; set; }

    [MaxLength(100)]
    public string? CollectedBy { get; set; }

    public DateTime? ExpectedCompletionDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    // Workflow tracking properties
    public DateTime? ReceivedDate { get; set; }

    [MaxLength(450)]
    public string? ReceivedBy { get; set; }

    public DateTime? PerformedDate { get; set; }

    [MaxLength(450)]
    public string? PerformedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    public bool IsPaid { get; set; } = false;

    public bool IsUrgent { get; set; } = false;

    public string? ClinicalNotes { get; set; }

    public string? Notes { get; set; }

    // Audit fields
    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<LabOrderItem> Items { get; set; } = new List<LabOrderItem>();
    public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
}
