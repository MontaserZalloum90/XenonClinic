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

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Audiogram? Audiogram { get; set; }
}
