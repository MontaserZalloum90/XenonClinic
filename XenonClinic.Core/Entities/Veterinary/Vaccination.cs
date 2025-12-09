namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a vaccination record for a pet
/// </summary>
public class Vaccination
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// Vaccine name (e.g., Rabies, DHPP, FVRCP, Bordetella)
    /// </summary>
    public string VaccineName { get; set; } = string.Empty;

    /// <summary>
    /// Vaccine type/category (Core, Non-Core, Required by Law)
    /// </summary>
    public string VaccineType { get; set; } = "Core";

    /// <summary>
    /// Manufacturer of the vaccine
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Batch/Lot number
    /// </summary>
    public string? BatchNumber { get; set; }

    public DateTime AdministrationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Date when next dose or booster is due
    /// </summary>
    public DateTime? NextDueDate { get; set; }

    /// <summary>
    /// Dose number in series (e.g., 1st, 2nd, Booster)
    /// </summary>
    public string? DoseNumber { get; set; }

    /// <summary>
    /// Site of injection
    /// </summary>
    public string? InjectionSite { get; set; }

    /// <summary>
    /// Any adverse reactions observed
    /// </summary>
    public string? AdverseReactions { get; set; }

    public string? AdministeredById { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Certificate number if issued
    /// </summary>
    public string? CertificateNumber { get; set; }

    public Pet? Pet { get; set; }
    public Branch? Branch { get; set; }
}
