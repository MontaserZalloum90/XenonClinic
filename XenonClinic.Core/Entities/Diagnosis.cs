using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a medical diagnosis for a patient
/// </summary>
public class Diagnosis
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public DateTime DiagnosisDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(20)]
    public string? ICD10Code { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string? DiagnosisName { get; set; } // Friendly name for the diagnosis

    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Chronic, Resolved, etc.

    [StringLength(100)]
    public string? DiagnosisType { get; set; } // primary, secondary, complication, etc.

    public int? ClinicalVisitId { get; set; }

    public int? VisitId { get; set; }

    public int? DiagnosedByDoctorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedDate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(ClinicalVisitId))]
    public virtual ClinicalVisit? ClinicalVisit { get; set; }

    [ForeignKey(nameof(VisitId))]
    public virtual Visit? Visit { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(DiagnosedByDoctorId))]
    public virtual Employee? DiagnosedByDoctor { get; set; }
}
