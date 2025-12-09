using XenonClinic.Core.Enums.Cardiology;

namespace XenonClinic.Core.Entities.Cardiology;

public class ECGRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? CardioVisitId { get; set; }
    public DateTime RecordDate { get; set; }
    public ECGRhythm Rhythm { get; set; }
    public int? HeartRate { get; set; }
    public int? PRInterval { get; set; }
    public int? QRSDuration { get; set; }
    public int? QTInterval { get; set; }
    public int? QTcInterval { get; set; }
    public string? Axis { get; set; }
    public bool? STElevation { get; set; }
    public string? STElevationLeads { get; set; }
    public bool? STDepression { get; set; }
    public string? STDepressionLeads { get; set; }
    public bool? TWaveInversion { get; set; }
    public string? TWaveInversionLeads { get; set; }
    public bool? QWaves { get; set; }
    public string? QWaveLeads { get; set; }
    public bool? LeftVentricularHypertrophy { get; set; }
    public bool? RightVentricularHypertrophy { get; set; }
    public bool? BundleBranchBlock { get; set; }
    public string? BundleBranchBlockType { get; set; }
    public string? Interpretation { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public CardioVisit? CardioVisit { get; set; }
}
