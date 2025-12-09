using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class SinusAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ENTVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public SinusConditionType ConditionType { get; set; }
    public bool? MaxillarySinusRight { get; set; }
    public bool? MaxillarySinusLeft { get; set; }
    public bool? FrontalSinusRight { get; set; }
    public bool? FrontalSinusLeft { get; set; }
    public bool? EthmoidSinusRight { get; set; }
    public bool? EthmoidSinusLeft { get; set; }
    public bool? SphenoidSinus { get; set; }
    public string? EndoscopyFindings { get; set; }
    public string? CTFindings { get; set; }
    public int? LundMackayScore { get; set; }
    public bool? NasalPolyps { get; set; }
    public string? PolypsGrade { get; set; }
    public bool? SeptalDeviation { get; set; }
    public string? SeptalDeviationSide { get; set; }
    public bool? TurbinateHypertrophy { get; set; }
    public bool? MucosalEdema { get; set; }
    public bool? PurulentDischarge { get; set; }
    public SinusSeverity Severity { get; set; }
    public string? Assessment { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public ENTVisit? ENTVisit { get; set; }
}
