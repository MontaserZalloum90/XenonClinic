namespace XenonClinic.Core.Entities.Oncology;

public class TumorMarker
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? CancerDiagnosisId { get; set; }
    public DateTime TestDate { get; set; }
    public string? MarkerName { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool? IsElevated { get; set; }
    public decimal? PercentChangeFromBaseline { get; set; }
    public decimal? PercentChangeFromPrevious { get; set; }
    public string? Trend { get; set; }
    public string? ClinicalSignificance { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public CancerDiagnosis? CancerDiagnosis { get; set; }
}
