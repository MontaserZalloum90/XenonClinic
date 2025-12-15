using System.ComponentModel.DataAnnotations.Schema;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a marketing lead - a potential customer/patient
/// </summary>
[Table("Leads")]
public class Lead
{
    public int Id { get; set; }

    /// <summary>
    /// Unique lead reference code (e.g., LEAD-2024-001)
    /// </summary>
    public string LeadCode { get; set; } = string.Empty;

    // Contact Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }

    // Company/Organization (for B2B)
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Lead Details
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadSource Source { get; set; }
    public string? SourceDetails { get; set; }

    // Interest and scoring
    public string? InterestedIn { get; set; }
    public int? LeadScore { get; set; }
    public string? Priority { get; set; }

    // Campaign association
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    // Branch
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    // Assignment
    public string? AssignedToUserId { get; set; }
    // Note: ApplicationUser navigation removed to avoid circular dependency with Infrastructure

    // Follow-up
    public DateTime? NextFollowUpDate { get; set; }
    public string? NextFollowUpNotes { get; set; }
    public DateTime? LastContactedDate { get; set; }
    public int ContactAttempts { get; set; }

    // Conversion
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToPatientId { get; set; }
    public Patient? ConvertedToPatient { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }

    // Lost lead info
    public string? LostReason { get; set; }

    // Notes and tags
    public string? Notes { get; set; }
    public string? Tags { get; set; }

    // Audit
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<MarketingActivity> Activities { get; set; } = new List<MarketingActivity>();

    // Computed Properties
    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsConverted => Status == LeadStatus.Converted && ConvertedToPatientId.HasValue;

    public bool NeedsFollowUp => NextFollowUpDate.HasValue
        && NextFollowUpDate.Value <= DateTime.UtcNow
        && Status != LeadStatus.Converted
        && Status != LeadStatus.Lost
        && Status != LeadStatus.Archived;

    public int DaysSinceLastContact => LastContactedDate.HasValue
        ? (int)(DateTime.UtcNow - LastContactedDate.Value).TotalDays
        : (int)(DateTime.UtcNow - CreatedAt).TotalDays;
}
