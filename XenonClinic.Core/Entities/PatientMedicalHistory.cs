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

    // Family History
    public string? FamilyHistory { get; set; }

    // Social History
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? OccupationalExposure { get; set; }

    // Hearing Specific
    public string? NoiseExposureHistory { get; set; }
    public string? PreviousHearingAids { get; set; }
    public string? TinnitusHistory { get; set; }
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
