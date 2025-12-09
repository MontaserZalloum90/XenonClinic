namespace XenonClinic.Core.Entities.PainManagement;

public class PainScore
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PainVisitId { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime RecordTime { get; set; }
    public int NRSScore { get; set; }
    public string? PainLocation { get; set; }
    public string? PainQuality { get; set; }
    public string? ActivityAtTime { get; set; }
    public string? MedicationTaken { get; set; }
    public int? MedicationRelief { get; set; }
    public string? FunctionalStatus { get; set; }
    public string? SleepQuality { get; set; }
    public string? MoodStatus { get; set; }
    public string? AggravatorsToday { get; set; }
    public string? RelieversToday { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public PainVisit? PainVisit { get; set; }
}
