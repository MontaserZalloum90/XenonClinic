using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an insurance company/provider
/// </summary>
public class InsuranceProvider : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Insurance provider code (unique identifier)
    /// </summary>
    public string ProviderCode { get; set; } = string.Empty;

    /// <summary>
    /// Insurance company name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Name in Arabic
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// Short name or abbreviation
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// Insurance type (Health, Life, Dental, Vision, etc.)
    /// </summary>
    public string InsuranceType { get; set; } = "Health";

    /// <summary>
    /// Whether provider is active for claims
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Contact person name
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact phone
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Fax number for claims
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Full address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; } = "UAE";

    /// <summary>
    /// Tax registration number
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// License number
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Electronic claims submission endpoint
    /// </summary>
    public string? ClaimsEndpoint { get; set; }

    /// <summary>
    /// Eligibility check endpoint
    /// </summary>
    public string? EligibilityEndpoint { get; set; }

    /// <summary>
    /// API credentials (encrypted JSON)
    /// </summary>
    public string? ApiCredentials { get; set; }

    /// <summary>
    /// Payment terms in days
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Default discount percentage
    /// </summary>
    public decimal DefaultDiscountPercent { get; set; }

    /// <summary>
    /// Notes about this provider
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<InsurancePlan> Plans { get; set; } = new List<InsurancePlan>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
