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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(ClinicalVisitId))]
    public virtual ClinicalVisit? ClinicalVisit { get; set; }

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

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public decimal? Quantity { get; set; }

    public int? DurationDays { get; set; }

    [ForeignKey(nameof(PrescriptionId))]
    public virtual Prescription? Prescription { get; set; }
}
