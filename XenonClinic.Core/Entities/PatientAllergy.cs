namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a patient's allergy record
/// </summary>
public class PatientAllergy
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string AllergyName { get; set; } = string.Empty;
    public string? AllergyType { get; set; }
    public string? Severity { get; set; }
    public string? Reaction { get; set; }
    public DateTime? RecordedDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public Patient? Patient { get; set; }
}
