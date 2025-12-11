using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Links a patient to their insurance coverage
/// </summary>
public class PatientInsurance : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Reference to the patient
    /// </summary>
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Reference to the insurance provider
    /// </summary>
    public int ProviderId { get; set; }
    public InsuranceProvider Provider { get; set; } = null!;

    /// <summary>
    /// Reference to the insurance plan
    /// </summary>
    public int PlanId { get; set; }
    public InsurancePlan Plan { get; set; } = null!;

    /// <summary>
    /// Insurance member ID / Policy number
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// Group number (for employer-sponsored plans)
    /// </summary>
    public string? GroupNumber { get; set; }

    /// <summary>
    /// Subscriber ID (if different from member ID)
    /// </summary>
    public string? SubscriberId { get; set; }

    /// <summary>
    /// Whether this is primary insurance
    /// </summary>
    public bool IsPrimary { get; set; } = true;

    /// <summary>
    /// Relationship to subscriber (Self, Spouse, Child, Other)
    /// </summary>
    public string RelationshipToSubscriber { get; set; } = "Self";

    /// <summary>
    /// Subscriber name (if not self)
    /// </summary>
    public string? SubscriberName { get; set; }

    /// <summary>
    /// Subscriber date of birth
    /// </summary>
    public DateTime? SubscriberDateOfBirth { get; set; }

    /// <summary>
    /// Coverage start date
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// Coverage end date
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Whether coverage is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last eligibility verification date
    /// </summary>
    public DateTime? LastVerifiedDate { get; set; }

    /// <summary>
    /// Eligibility verification status
    /// </summary>
    public string? VerificationStatus { get; set; }

    /// <summary>
    /// Remaining deductible amount
    /// </summary>
    public decimal? RemainingDeductible { get; set; }

    /// <summary>
    /// Remaining annual benefit
    /// </summary>
    public decimal? RemainingAnnualBenefit { get; set; }

    /// <summary>
    /// Year-to-date claims amount
    /// </summary>
    public decimal YtdClaimsAmount { get; set; }

    /// <summary>
    /// Card image front (file path or base64)
    /// </summary>
    public string? CardImageFront { get; set; }

    /// <summary>
    /// Card image back (file path or base64)
    /// </summary>
    public string? CardImageBack { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation
    public ICollection<InsuranceClaim> Claims { get; set; } = new List<InsuranceClaim>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
