using XenonClinic.Core.Enums.Gynecology;

namespace XenonClinic.Core.Entities.Gynecology;

public class ObUltrasound
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PregnancyRecordId { get; set; }
    public int? GynVisitId { get; set; }
    public DateTime UltrasoundDate { get; set; }
    public UltrasoundType UltrasoundType { get; set; }
    public int? GestationalWeeks { get; set; }
    public int? GestationalDays { get; set; }
    public int? NumberOfFetuses { get; set; }
    public bool? FetalHeartbeatPresent { get; set; }
    public int? FetalHeartRate { get; set; }
    public decimal? CrownRumpLength { get; set; }
    public decimal? BiparietalDiameter { get; set; }
    public decimal? HeadCircumference { get; set; }
    public decimal? AbdominalCircumference { get; set; }
    public decimal? FemurLength { get; set; }
    public decimal? EstimatedFetalWeight { get; set; }
    public string? FetalPosition { get; set; }
    public string? PlacentaLocation { get; set; }
    public string? PlacentaGrade { get; set; }
    public decimal? AmnioticFluidIndex { get; set; }
    public string? AmnioticFluidAssessment { get; set; }
    public decimal? CervicalLength { get; set; }
    public bool? CervicalFunneling { get; set; }
    public string? UterineFindings { get; set; }
    public string? OvarianFindings { get; set; }
    public string? FetalAnomalies { get; set; }
    public decimal? NuchalTranslucency { get; set; }
    public bool? NasalBonePresent { get; set; }
    public string? AnatomySurveyFindings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? Sonographer { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PregnancyRecord? PregnancyRecord { get; set; }
    public GynVisit? GynVisit { get; set; }
}
