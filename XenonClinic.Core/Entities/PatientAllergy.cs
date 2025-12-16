namespace XenonClinic.Core.Entities;

/// <summary>
/// Patient allergy record for drug interaction checking
/// </summary>
public class PatientAllergy
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string AllergyName { get; set; } = string.Empty;
    public string? AllergyType { get; set; }
    public string? Severity { get; set; }
    public string? Reaction { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? OnsetDate { get; set; }
    public DateTime? RecordedDate { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
}
