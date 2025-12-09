using XenonClinic.Core.Enums.Dialysis;

namespace XenonClinic.Core.Entities.Dialysis;

public class DialysisAccessRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public AccessType AccessType { get; set; }
    public string? AccessLocation { get; set; }
    public DateTime PlacementDate { get; set; }
    public string? PlacingSurgeon { get; set; }
    public DateTime? MaturationDate { get; set; }
    public DateTime? FirstUseDate { get; set; }
    public AccessStatus Status { get; set; }
    public DateTime? FailureDate { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? RemovalDate { get; set; }
    public string? RemovalReason { get; set; }
    public string? CatheterType { get; set; }
    public string? CatheterBrand { get; set; }
    public decimal? CatheterLength { get; set; }
    public string? ArteryUsed { get; set; }
    public string? VeinUsed { get; set; }
    public string? GraftMaterial { get; set; }
    public string? AccessFlowRate { get; set; }
    public DateTime? LastFlowStudyDate { get; set; }
    public string? LastFlowStudyResult { get; set; }
    public bool? StenosisPresent { get; set; }
    public string? StenosisLocation { get; set; }
    public string? Interventions { get; set; }
    public int? InfectionCount { get; set; }
    public int? RevisionCount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}
