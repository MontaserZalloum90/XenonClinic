using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class AllergyTest
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ENTVisitId { get; set; }
    public DateTime TestDate { get; set; }
    public AllergyTestType TestType { get; set; }
    public string? AllergensTestedJson { get; set; }
    public string? PositiveAllergensJson { get; set; }
    public bool? DustMites { get; set; }
    public int? DustMitesReaction { get; set; }
    public bool? Mold { get; set; }
    public int? MoldReaction { get; set; }
    public bool? Pollen { get; set; }
    public int? PollenReaction { get; set; }
    public bool? AnimalDander { get; set; }
    public int? AnimalDanderReaction { get; set; }
    public bool? Foods { get; set; }
    public string? FoodAllergensJson { get; set; }
    public decimal? TotalIgE { get; set; }
    public string? SpecificIgEJson { get; set; }
    public AllergySeverity OverallSeverity { get; set; }
    public string? Interpretation { get; set; }
    public string? RecommendedAvoidance { get; set; }
    public bool? ImmunotherapyRecommended { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public ENTVisit? ENTVisit { get; set; }
}
