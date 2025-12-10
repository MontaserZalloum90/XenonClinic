namespace XenonClinic.Core.Entities;

public class AudiologyVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? Plan { get; set; }
    public string? ProviderId { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Audiogram? Audiogram { get; set; }
}
