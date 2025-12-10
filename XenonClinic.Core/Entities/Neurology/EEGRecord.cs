using XenonClinic.Core.Enums.Neurology;

namespace XenonClinic.Core.Entities.Neurology;

public class EEGRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? NeuroVisitId { get; set; }
    public DateTime StudyDate { get; set; }
    public EEGType EEGType { get; set; }
    public int? RecordingDuration { get; set; }
    public string? Indication { get; set; }
    public string? Medications { get; set; }
    public string? BackgroundActivity { get; set; }
    public string? DominantRhythm { get; set; }
    public decimal? DominantFrequency { get; set; }
    public bool? Symmetry { get; set; }
    public string? AsymmetryDetails { get; set; }
    public bool? EpilepitiformDischarges { get; set; }
    public string? EpilepitiformDetails { get; set; }
    public string? EpilepitiformLocation { get; set; }
    public bool? SlowWaveActivity { get; set; }
    public string? SlowWaveDetails { get; set; }
    public string? HyperventilationResponse { get; set; }
    public string? PhototicResponse { get; set; }
    public string? SleepFeatures { get; set; }
    public bool? SeizuresCaptured { get; set; }
    public int? NumberOfSeizures { get; set; }
    public string? SeizureDescription { get; set; }
    public string? ClinicalCorrelation { get; set; }
    public string? Interpretation { get; set; }
    public string? Conclusion { get; set; }
    public string? FilePath { get; set; }
    public string? TechnicianName { get; set; }
    public string? InterpretingPhysician { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public NeuroVisit? NeuroVisit { get; set; }
}
