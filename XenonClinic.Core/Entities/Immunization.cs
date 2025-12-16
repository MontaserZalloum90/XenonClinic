using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Patient immunization/vaccination records
/// </summary>
public class Immunization : IBranchEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string VaccineName { get; set; } = string.Empty;
    public string? VaccineCode { get; set; } // CDC/WHO vaccine code
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public DateTime AdministrationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Site { get; set; } // Injection site
    public string? Route { get; set; } // Intramuscular, Subcutaneous, etc.
    public decimal? DoseAmount { get; set; }
    public string? DoseUnit { get; set; }
    public int? DoseNumber { get; set; } // For multi-dose vaccines
    public int? TotalDoses { get; set; } // Total doses in series

    public string? AdministeredBy { get; set; }
    public string? AdministeredByUser { get; set; }
    public int? AdministeredByEmployeeId { get; set; }
    public Employee? AdministeredByEmployee { get; set; }

    public string? Notes { get; set; }
    public string? AdverseReactions { get; set; }

    // Next dose scheduling
    public DateTime? NextDoseDate { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
