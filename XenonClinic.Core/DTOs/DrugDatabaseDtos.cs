namespace XenonClinic.Core.DTOs;

#region Drug Search DTOs

/// <summary>
/// Drug search request
/// </summary>
public class DrugSearchRequestDto
{
    public string Query { get; set; } = string.Empty;
    public DrugSearchTypeDto SearchType { get; set; } = DrugSearchTypeDto.Name;
    public int MaxResults { get; set; } = 20;
    public bool IncludeBrandNames { get; set; } = true;
    public bool IncludeGenerics { get; set; } = true;
    public bool ActiveOnly { get; set; } = true;
}

/// <summary>
/// Drug search type
/// </summary>
public enum DrugSearchTypeDto
{
    Name,
    NDC,
    RxCUI,
    Ingredient,
    BrandName
}

/// <summary>
/// Drug search result
/// </summary>
public class DrugSearchResultDto
{
    public List<DrugSummaryDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public string? SearchTerm { get; set; }
    public DrugSearchTypeDto SearchType { get; set; }
}

/// <summary>
/// Drug summary DTO
/// </summary>
public class DrugSummaryDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DoseForm { get; set; }
    public string? Route { get; set; }
    public string? Manufacturer { get; set; }
    public string? NDC { get; set; }
    public DrugTermTypeDto TermType { get; set; }
    public bool IsGeneric { get; set; }
    public bool IsBrand { get; set; }
    public bool IsOTC { get; set; }
    public string? DrugClass { get; set; }
    public string? Schedule { get; set; } // DEA Schedule
}

/// <summary>
/// Drug term type
/// </summary>
public enum DrugTermTypeDto
{
    Ingredient,
    BrandName,
    ClinicalDrug,
    BrandedPack,
    GenericPack,
    DrugComponent,
    Other
}

#endregion

#region Drug Detail DTOs

/// <summary>
/// Detailed drug information
/// </summary>
public class DrugDetailDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DoseForm { get; set; }
    public string? Route { get; set; }
    public List<DrugIngredientDto> Ingredients { get; set; } = new();
    public List<DrugNDCDto> NDCs { get; set; } = new();
    public List<string>? Manufacturers { get; set; }
    public DrugPricingDto? Pricing { get; set; }
    public List<DrugInteractionDto>? KnownInteractions { get; set; }
    public DrugFDAInfoDto? FDAInfo { get; set; }
    public List<DrugAlternativeDto>? Alternatives { get; set; }
    public string? MechanismOfAction { get; set; }
    public string? Pharmacokinetics { get; set; }
    public List<string>? Indications { get; set; }
    public List<string>? Contraindications { get; set; }
    public List<string>? Warnings { get; set; }
    public List<string>? SideEffects { get; set; }
    public DrugDosageInfoDto? DosageInfo { get; set; }
    public string? StorageConditions { get; set; }
    public string? PatientCounseling { get; set; }
}

/// <summary>
/// Drug ingredient
/// </summary>
public class DrugIngredientDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Drug NDC (National Drug Code)
/// </summary>
public class DrugNDCDto
{
    public string NDC { get; set; } = string.Empty;
    public string? NDC10 { get; set; }
    public string? NDC11 { get; set; }
    public string? PackageDescription { get; set; }
    public int? PackageSize { get; set; }
    public string? PackageUnit { get; set; }
    public string? Manufacturer { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Drug pricing information
/// </summary>
public class DrugPricingDto
{
    public decimal? AWP { get; set; } // Average Wholesale Price
    public decimal? WAC { get; set; } // Wholesale Acquisition Cost
    public decimal? MAC { get; set; } // Maximum Allowable Cost
    public decimal? NADAC { get; set; } // National Average Drug Acquisition Cost
    public string? Unit { get; set; }
    public DateTime? PriceDate { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// FDA drug information
/// </summary>
public class DrugFDAInfoDto
{
    public string? ApplicationNumber { get; set; }
    public string? ApplicationType { get; set; } // NDA, ANDA, BLA
    public string? ApprovalDate { get; set; }
    public string? Sponsor { get; set; }
    public string? MarketingStatus { get; set; }
    public bool? RequiresREMS { get; set; }
    public string? REMSDescription { get; set; }
    public bool? HasBoxedWarning { get; set; }
    public string? BoxedWarningText { get; set; }
    public List<string>? ActiveIngredients { get; set; }
}

/// <summary>
/// Drug alternative
/// </summary>
public class DrugAlternativeDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DoseForm { get; set; }
    public AlternativeTypeDto AlternativeType { get; set; }
    public bool IsGeneric { get; set; }
    public decimal? EstimatedCostSavings { get; set; }
}

/// <summary>
/// Alternative type
/// </summary>
public enum AlternativeTypeDto
{
    GenericEquivalent,
    TherapeuticAlternative,
    BrandAlternative,
    DifferentStrength,
    DifferentForm
}

/// <summary>
/// Drug dosage information
/// </summary>
public class DrugDosageInfoDto
{
    public string? UsualAdultDose { get; set; }
    public string? UsualPediatricDose { get; set; }
    public string? GeriatricDose { get; set; }
    public string? RenalDoseAdjustment { get; set; }
    public string? HepaticDoseAdjustment { get; set; }
    public string? MaximumDose { get; set; }
    public string? AdminInstruction { get; set; }
    public List<DrugDoseFormulationDto>? Formulations { get; set; }
}

/// <summary>
/// Drug dose formulation
/// </summary>
public class DrugDoseFormulationDto
{
    public string Form { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Instructions { get; set; }
}

#endregion

#region Drug Interaction DTOs

/// <summary>
/// Drug interaction check request
/// </summary>
public class DrugInteractionCheckRequestDto
{
    public List<string> RxCUIs { get; set; } = new();
    public List<string>? NDCs { get; set; }
    public bool IncludeSeverity { get; set; } = true;
    public bool IncludeAllergies { get; set; } = true;
    public int? PatientId { get; set; }
}

/// <summary>
/// Drug interaction check response
/// </summary>
public class DrugInteractionCheckResponseDto
{
    public bool HasInteractions { get; set; }
    public int TotalInteractions { get; set; }
    public int SevereInteractions { get; set; }
    public int ModerateInteractions { get; set; }
    public int MinorInteractions { get; set; }
    public List<DrugInteractionDto> Interactions { get; set; } = new();
    public List<DrugAllergyAlertDto>? AllergyAlerts { get; set; }
}

/// <summary>
/// Drug interaction
/// </summary>
public class DrugInteractionDto
{
    public string Drug1RxCUI { get; set; } = string.Empty;
    public string Drug1Name { get; set; } = string.Empty;
    public string Drug2RxCUI { get; set; } = string.Empty;
    public string Drug2Name { get; set; } = string.Empty;
    public InteractionSeverityDto Severity { get; set; }
    public string? Description { get; set; }
    public string? ClinicalSignificance { get; set; }
    public string? ManagementRecommendation { get; set; }
    public string? Source { get; set; }
    public string? EvidenceLevel { get; set; }
    public List<string>? References { get; set; }
}

/// <summary>
/// Interaction severity
/// </summary>
public enum InteractionSeverityDto
{
    Contraindicated,
    Severe,
    Moderate,
    Minor,
    Unknown
}

/// <summary>
/// Drug allergy alert
/// </summary>
public class DrugAllergyAlertDto
{
    public string DrugRxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string AllergenName { get; set; } = string.Empty;
    public string? AllergyType { get; set; }
    public string? CrossReactivity { get; set; }
    public string? RecommendedAction { get; set; }
    public string? AlternativeSuggestion { get; set; }
}

#endregion

#region Drug Class DTOs

/// <summary>
/// Drug class
/// </summary>
public class DrugClassDto
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ClassType { get; set; } = string.Empty; // ATC, EPC, MESH, VA
    public string? ParentClassId { get; set; }
    public string? Description { get; set; }
    public List<DrugClassDto>? SubClasses { get; set; }
    public int DrugCount { get; set; }
}

/// <summary>
/// Get drugs by class request
/// </summary>
public class DrugsByClassRequestDto
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassType { get; set; } = "ATC"; // ATC, EPC, MESH, VA
    public bool IncludeSubClasses { get; set; } = true;
    public int MaxResults { get; set; } = 100;
}

#endregion

#region Formulary DTOs

/// <summary>
/// Formulary drug
/// </summary>
public class FormularyDrugDto
{
    public int Id { get; set; }
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DoseForm { get; set; }
    public FormularyTierDto Tier { get; set; }
    public bool RequiresPriorAuth { get; set; }
    public bool HasQuantityLimit { get; set; }
    public int? QuantityLimit { get; set; }
    public string? QuantityLimitUnit { get; set; }
    public int? DaysSupplyLimit { get; set; }
    public bool HasStepTherapy { get; set; }
    public string? StepTherapyRequirement { get; set; }
    public decimal? Copay { get; set; }
    public decimal? CoinsurancePercent { get; set; }
    public bool IsPreferred { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Formulary tier
/// </summary>
public enum FormularyTierDto
{
    Tier1_Generic,
    Tier2_PreferredBrand,
    Tier3_NonPreferredBrand,
    Tier4_Specialty,
    Tier5_NotCovered,
    NonFormulary
}

/// <summary>
/// Add to formulary request
/// </summary>
public class AddToFormularyRequestDto
{
    public string RxCUI { get; set; } = string.Empty;
    public FormularyTierDto Tier { get; set; }
    public bool RequiresPriorAuth { get; set; }
    public bool HasQuantityLimit { get; set; }
    public int? QuantityLimit { get; set; }
    public string? QuantityLimitUnit { get; set; }
    public int? DaysSupplyLimit { get; set; }
    public bool HasStepTherapy { get; set; }
    public string? StepTherapyRequirement { get; set; }
    public decimal? Copay { get; set; }
    public decimal? CoinsurancePercent { get; set; }
    public bool IsPreferred { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Formulary check response
/// </summary>
public class FormularyCheckResponseDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public bool IsOnFormulary { get; set; }
    public FormularyDrugDto? FormularyInfo { get; set; }
    public List<FormularyDrugDto>? Alternatives { get; set; }
    public string? Message { get; set; }
}

#endregion

#region Prescription Assistance DTOs

/// <summary>
/// Prescription assistance program
/// </summary>
public class PatientAssistanceProgramDto
{
    public int Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? Sponsor { get; set; }
    public string? DrugName { get; set; }
    public string? RxCUI { get; set; }
    public string? Description { get; set; }
    public string? EligibilityRequirements { get; set; }
    public string? IncomeRequirement { get; set; }
    public string? InsuranceRequirement { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public decimal? MaxSavings { get; set; }
    public string? CouponCode { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Find assistance programs request
/// </summary>
public class FindAssistanceProgramsRequestDto
{
    public string? RxCUI { get; set; }
    public string? DrugName { get; set; }
    public decimal? AnnualIncome { get; set; }
    public bool? HasInsurance { get; set; }
    public string? InsuranceType { get; set; }
}

#endregion

#region Drug Monograph DTOs

/// <summary>
/// Drug monograph
/// </summary>
public class DrugMonographDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public MonographSectionDto? Description { get; set; }
    public MonographSectionDto? Indications { get; set; }
    public MonographSectionDto? DosageAndAdministration { get; set; }
    public MonographSectionDto? Contraindications { get; set; }
    public MonographSectionDto? WarningsAndPrecautions { get; set; }
    public MonographSectionDto? AdverseReactions { get; set; }
    public MonographSectionDto? DrugInteractions { get; set; }
    public MonographSectionDto? UseInSpecificPopulations { get; set; }
    public MonographSectionDto? Overdosage { get; set; }
    public MonographSectionDto? ClinicalPharmacology { get; set; }
    public MonographSectionDto? Storage { get; set; }
    public MonographSectionDto? PatientCounseling { get; set; }
    public string? LastUpdated { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Monograph section
/// </summary>
public class MonographSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<MonographSubSectionDto>? SubSections { get; set; }
}

/// <summary>
/// Monograph subsection
/// </summary>
public class MonographSubSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

#endregion

#region Controlled Substance DTOs

/// <summary>
/// Controlled substance info
/// </summary>
public class ControlledSubstanceInfoDto
{
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string DEASchedule { get; set; } = string.Empty; // I, II, III, IV, V
    public string? StateSchedule { get; set; }
    public bool RequiresEPCS { get; set; } // Electronic Prescribing for Controlled Substances
    public int? MaxDaysSupply { get; set; }
    public bool? RefillsAllowed { get; set; }
    public int? MaxRefills { get; set; }
    public string? SpecialRequirements { get; set; }
    public bool RequiresPDMP { get; set; } // Prescription Drug Monitoring Program
    public List<string>? StateRestrictions { get; set; }
}

#endregion

#region Adverse Event DTOs

/// <summary>
/// Report adverse event request
/// </summary>
public class ReportAdverseEventDto
{
    public string DrugRxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public string? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public string? Outcome { get; set; } // Death, Life-threatening, Hospitalization, etc.
    public DateTime? OnsetDate { get; set; }
    public DateTime? StopDate { get; set; }
    public string? DoseAtEvent { get; set; }
    public string? RouteOfAdmin { get; set; }
    public string? Indication { get; set; }
    public List<string>? ConcomitantMedications { get; set; }
    public string? ReporterType { get; set; } // Physician, Pharmacist, Patient, etc.
    public string? ReporterName { get; set; }
    public string? ReporterContact { get; set; }
    public bool? WasReportedToFDA { get; set; }
}

/// <summary>
/// Adverse event response
/// </summary>
public class AdverseEventResponseDto
{
    public bool Success { get; set; }
    public string? ReportId { get; set; }
    public string? FDAReportNumber { get; set; }
    public string? Message { get; set; }
}

#endregion
