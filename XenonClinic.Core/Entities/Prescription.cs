using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

/// <summary>
/// General prescription/medication order entity
/// </summary>
[Table("Prescriptions")]
public class Prescription
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? ClinicalVisitId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime PrescriptionDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    [MaxLength(1000)]
    public string? Instructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<PrescriptionItem>? Items { get; set; }
}

/// <summary>
/// Individual medication item in a prescription
/// </summary>
[Table("PrescriptionItems")]
public class PrescriptionItem
{
    [Key]
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    [Required]
    [MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? GenericName { get; set; }

    [MaxLength(100)]
    public string? Strength { get; set; }

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(50)]
    public string? Frequency { get; set; } // e.g., "Once daily", "Twice daily", "Every 8 hours"

    [MaxLength(50)]
    public string? Duration { get; set; } // Duration as string (e.g., "7 days", "2 weeks")

    [MaxLength(50)]
    public string? Route { get; set; } // e.g., "Oral", "Topical", "Injection"

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public int? Quantity { get; set; }

    public int? Refills { get; set; }

    public int? RefillsRemaining { get; set; }

    public int? DurationDays { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Pending, Processing, Awaiting Pickup, Completed, Cancelled

    [ForeignKey(nameof(PrescriptionId))]
    public virtual Prescription? Prescription { get; set; }
}
