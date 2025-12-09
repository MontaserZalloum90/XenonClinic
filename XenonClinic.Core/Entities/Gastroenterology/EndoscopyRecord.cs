using XenonClinic.Core.Enums.Gastroenterology;

namespace XenonClinic.Core.Entities.Gastroenterology;

public class EndoscopyRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? GastroVisitId { get; set; }
    public DateTime ProcedureDate { get; set; }
    public EndoscopyType EndoscopyType { get; set; }
    public string? Indication { get; set; }
    public string? SedationType { get; set; }
    public string? ScopeUsed { get; set; }
    public string? EsophagusFindings { get; set; }
    public string? StomachFindings { get; set; }
    public string? DuodenumFindings { get; set; }
    public string? ColonFindings { get; set; }
    public string? TerminalIleumFindings { get; set; }
    public string? PolypFindings { get; set; }
    public int? NumberOfPolyps { get; set; }
    public string? PolypLocations { get; set; }
    public string? PolypSizes { get; set; }
    public bool? BiopsiesTaken { get; set; }
    public string? BiopsyLocations { get; set; }
    public string? PathologyResults { get; set; }
    public bool? HelicobacterPylori { get; set; }
    public string? TherapeuticInterventions { get; set; }
    public string? BowelPrepQuality { get; set; }
    public int? BostonBowelPrepScore { get; set; }
    public bool? CecumReached { get; set; }
    public decimal? WithdrawalTime { get; set; }
    public string? Complications { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? NextEndoscopyDue { get; set; }
    public string? PerformingPhysician { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public GastroVisit? GastroVisit { get; set; }
}
