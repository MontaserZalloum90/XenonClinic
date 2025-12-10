using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class LabTest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TestCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public TestCategory Category { get; set; }

    public SpecimenType? SpecimenType { get; set; }

    [MaxLength(100)]
    public string? SpecimenVolume { get; set; }

    public int? TurnaroundTimeHours { get; set; }

    [Required]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public string? Methodology { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequiresFasting { get; set; } = false;

    public string? PreparationInstructions { get; set; }

    // External lab if test is outsourced
    public int? ExternalLabId { get; set; }
    public ExternalLab? ExternalLab { get; set; }

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}
