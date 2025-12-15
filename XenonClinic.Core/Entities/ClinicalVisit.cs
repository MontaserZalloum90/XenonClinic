using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a clinical visit/encounter between a patient and healthcare provider
/// </summary>
public class ClinicalVisit
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public DateTime VisitDate { get; set; }

    public DateTime? EndTime { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "planned"; // planned, in-progress, finished, cancelled, no-show

    public int? DoctorId { get; set; }

    [StringLength(500)]
    public string? ChiefComplaint { get; set; }

    [StringLength(2000)]
    public string? HistoryOfPresentIllness { get; set; }

    [StringLength(2000)]
    public string? Assessment { get; set; }

    [StringLength(2000)]
    public string? Plan { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public virtual Employee? Doctor { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public virtual ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
}
