using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class ThroatExam
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ENTVisitId { get; set; }
    public DateTime ExamDate { get; set; }
    public string? OropharynxAppearance { get; set; }
    public TonsilGrade? TonsilGradeRight { get; set; }
    public TonsilGrade? TonsilGradeLeft { get; set; }
    public bool? TonsilExudate { get; set; }
    public bool? TonsilStones { get; set; }
    public string? UvulaPosition { get; set; }
    public string? SoftPalateExam { get; set; }
    public string? TongueExam { get; set; }
    public string? TongueBaseExam { get; set; }
    public string? EpiglottisExam { get; set; }
    public string? VocalCordMovementRight { get; set; }
    public string? VocalCordMovementLeft { get; set; }
    public bool? VocalCordLesions { get; set; }
    public string? VocalCordLesionDetails { get; set; }
    public bool? LaryngealEdema { get; set; }
    public bool? Erythema { get; set; }
    public bool? Masses { get; set; }
    public string? MassDetails { get; set; }
    public string? StroboscopyFindings { get; set; }
    public string? Assessment { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public ENTVisit? ENTVisit { get; set; }
}
