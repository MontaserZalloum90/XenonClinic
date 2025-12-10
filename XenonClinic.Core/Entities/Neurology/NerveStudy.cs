using XenonClinic.Core.Enums.Neurology;

namespace XenonClinic.Core.Entities.Neurology;

public class NerveStudy
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? NeuroVisitId { get; set; }
    public DateTime StudyDate { get; set; }
    public NerveStudyType StudyType { get; set; }
    public string? Indication { get; set; }
    public string? NervesStudied { get; set; }
    public string? MotorNCSFindings { get; set; }
    public string? SensoryNCSFindings { get; set; }
    public string? FWaveFindings { get; set; }
    public string? HReflexFindings { get; set; }
    public string? EMGFindings { get; set; }
    public string? MusclesExamined { get; set; }
    public string? InsertionalActivity { get; set; }
    public string? SpontaneousActivity { get; set; }
    public string? MUAPAnalysis { get; set; }
    public string? RecruitmentPattern { get; set; }
    public string? RepetitiveStimulation { get; set; }
    public string? SFEMGFindings { get; set; }
    public string? SomatosensoryEvokedPotentials { get; set; }
    public string? VisualEvokedPotentials { get; set; }
    public string? BrainstemAuditoryEvokedPotentials { get; set; }
    public string? Localization { get; set; }
    public string? Interpretation { get; set; }
    public string? ClinicalCorrelation { get; set; }
    public string? Conclusion { get; set; }
    public string? FilePath { get; set; }
    public string? PerformingPhysician { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public NeuroVisit? NeuroVisit { get; set; }
}
