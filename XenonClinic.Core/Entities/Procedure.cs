using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

/// <summary>
/// General medical procedure entity
/// </summary>
[Table("Procedures")]
public class Procedure
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? ClinicalVisitId { get; set; }

    public int? DoctorId { get; set; }

    [MaxLength(50)]
    public string? CPTCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime ProcedureDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Completed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(ClinicalVisitId))]
    public virtual ClinicalVisit? ClinicalVisit { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public virtual Doctor? Doctor { get; set; }
}
