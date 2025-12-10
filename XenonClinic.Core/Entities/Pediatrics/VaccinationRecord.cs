namespace XenonClinic.Core.Entities.Pediatrics;

/// <summary>
/// Represents a vaccination record for a pediatric patient
/// </summary>
public class VaccinationRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PediatricVisitId { get; set; }

    /// <summary>
    /// Vaccine name (e.g., DTaP, MMR, IPV, Hep B, Hib, PCV, Rotavirus, Varicella)
    /// </summary>
    public string VaccineName { get; set; } = string.Empty;

    /// <summary>
    /// Vaccine type/category (Routine, Catch-up, Travel, Special circumstances)
    /// </summary>
    public string VaccineType { get; set; } = "Routine";

    /// <summary>
    /// Disease(s) protected against
    /// </summary>
    public string? DiseasesProtected { get; set; }

    /// <summary>
    /// Dose number in series (e.g., 1st, 2nd, 3rd, Booster)
    /// </summary>
    public string DoseNumber { get; set; } = "1st";

    /// <summary>
    /// Recommended age for this dose
    /// </summary>
    public string? RecommendedAge { get; set; }

    /// <summary>
    /// Age at administration
    /// </summary>
    public string? AgeAtAdministration { get; set; }

    public DateTime AdministrationDate { get; set; }

    /// <summary>
    /// Manufacturer
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Lot/Batch number
    /// </summary>
    public string? LotNumber { get; set; }

    /// <summary>
    /// Expiry date of vaccine
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Injection site
    /// </summary>
    public string? InjectionSite { get; set; }

    /// <summary>
    /// Route of administration (IM, SC, Oral, Intranasal)
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Dosage given
    /// </summary>
    public string? Dosage { get; set; }

    /// <summary>
    /// Status (Given, Refused, Deferred, Contraindicated)
    /// </summary>
    public string Status { get; set; } = "Given";

    /// <summary>
    /// Reason if not given
    /// </summary>
    public string? ReasonNotGiven { get; set; }

    /// <summary>
    /// Date when next dose is due
    /// </summary>
    public DateTime? NextDoseDate { get; set; }

    /// <summary>
    /// Any adverse reactions observed
    /// </summary>
    public string? AdverseReactions { get; set; }

    /// <summary>
    /// VIS (Vaccine Information Statement) date given
    /// </summary>
    public DateTime? VisDateGiven { get; set; }

    /// <summary>
    /// Consent obtained from
    /// </summary>
    public string? ConsentObtainedFrom { get; set; }

    public string? AdministeredById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PediatricVisit? Visit { get; set; }
}
