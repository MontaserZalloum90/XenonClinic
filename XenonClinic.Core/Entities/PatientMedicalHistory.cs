using System.ComponentModel.DataAnnotations.Schema;

namespace XenonClinic.Core.Entities;

public class PatientMedicalHistory
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    // Chronic Conditions
    public string? ChronicConditions { get; set; }

    // Allergies
    public string? Allergies { get; set; }
    public string? AllergyReactions { get; set; }

    // Current Medications
    public string? CurrentMedications { get; set; }

    // Past Medical History
    public string? PastMedicalHistory { get; set; }

    // Surgical History
    public string? SurgicalHistory { get; set; }

    [NotMapped]
    public string? PastSurgeries { get => SurgicalHistory; set => SurgicalHistory = value; } // Alias for SurgicalHistory

    // Family History
    public string? FamilyHistory { get; set; }

    // Social History
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? OccupationalExposure { get; set; }

    [NotMapped]
    public bool AlcoholConsumption { get => ConsumesAlcohol; set => ConsumesAlcohol = value; } // Alias for ConsumesAlcohol

    [NotMapped]
    public string? SocialHistory { get; set; } // Combined social history field (not persisted)

    // Hearing Specific
    public string? NoiseExposureHistory { get; set; }

    [NotMapped]
    public string? NoiseExposure { get => NoiseExposureHistory; set => NoiseExposureHistory = value; } // Alias for NoiseExposureHistory

    public string? PreviousHearingAids { get; set; }
    public string? TinnitusHistory { get; set; }

    [NotMapped]
    public string? Tinnitus { get => TinnitusHistory; set => TinnitusHistory = value; } // Alias for TinnitusHistory

    public string? BalanceProblems { get; set; }

    // Additional Notes
    public string? AdditionalNotes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
}
