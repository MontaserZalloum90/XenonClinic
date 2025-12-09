namespace XenonClinic.Core.Entities.Neurology;

public class NeuroExam
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? NeuroVisitId { get; set; }
    public DateTime ExamDate { get; set; }
    public string? ExamType { get; set; }
    public int? GlasgowComaScale { get; set; }
    public int? GCSEye { get; set; }
    public int? GCSVerbal { get; set; }
    public int? GCSMotor { get; set; }
    public string? Orientation { get; set; }
    public string? Attention { get; set; }
    public string? Language { get; set; }
    public string? Memory { get; set; }
    public int? MMSEScore { get; set; }
    public int? MoCAScore { get; set; }
    public string? CN1Olfactory { get; set; }
    public string? CN2Optic { get; set; }
    public string? PupilsReaction { get; set; }
    public string? VisualFields { get; set; }
    public string? CN3456EyeMovements { get; set; }
    public string? CN5Trigeminal { get; set; }
    public string? CN7Facial { get; set; }
    public string? CN8Vestibulocochlear { get; set; }
    public string? CN9CN10PharyngealGag { get; set; }
    public string? CN11SpinalAccessory { get; set; }
    public string? CN12Hypoglossal { get; set; }
    public string? MuscleToneUpper { get; set; }
    public string? MuscleToneLower { get; set; }
    public string? StrengthUpperRight { get; set; }
    public string? StrengthUpperLeft { get; set; }
    public string? StrengthLowerRight { get; set; }
    public string? StrengthLowerLeft { get; set; }
    public string? DeepTendonReflexes { get; set; }
    public string? PlantarResponse { get; set; }
    public string? SensoryLightTouch { get; set; }
    public string? SensoryPinprick { get; set; }
    public string? SensoryVibration { get; set; }
    public string? SensoryProprioception { get; set; }
    public string? FingerToNose { get; set; }
    public string? HeelToShin { get; set; }
    public string? RapidAlternating { get; set; }
    public string? RombergTest { get; set; }
    public string? GaitDescription { get; set; }
    public string? TandemGait { get; set; }
    public string? Interpretation { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public NeuroVisit? NeuroVisit { get; set; }
}
