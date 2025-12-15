using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Patient vital signs measurement
/// </summary>
[Table("VitalSigns")]
public class VitalSign
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int BranchId { get; set; }

    public int? ClinicalVisitId { get; set; }

    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Systolic blood pressure (mmHg)
    /// </summary>
    public int? SystolicBP { get; set; }

    /// <summary>
    /// Diastolic blood pressure (mmHg)
    /// </summary>
    public int? DiastolicBP { get; set; }

    /// <summary>
    /// Heart rate (beats per minute)
    /// </summary>
    public int? HeartRate { get; set; }

    /// <summary>
    /// Body temperature (Celsius)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Respiratory rate (breaths per minute)
    /// </summary>
    public int? RespiratoryRate { get; set; }

    /// <summary>
    /// Oxygen saturation (percentage)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? OxygenSaturation { get; set; }

    /// <summary>
    /// Weight (kg)
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// Height (cm)
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal? Height { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient? Patient { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(ClinicalVisitId))]
    public virtual ClinicalVisit? ClinicalVisit { get; set; }

    // Alias properties for service compatibility
    [NotMapped]
    public int? BloodPressureSystolic => SystolicBP;

    [NotMapped]
    public int? BloodPressureDiastolic => DiastolicBP;

    // Computed BMI (Body Mass Index)
    [NotMapped]
    public decimal? BMI => (Weight.HasValue && Height.HasValue && Height.Value > 0)
        ? Math.Round(Weight.Value / ((Height.Value / 100) * (Height.Value / 100)), 1)
        : null;
}
