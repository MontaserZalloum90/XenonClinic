namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a marketing activity or interaction with a lead/campaign
/// </summary>
public class MarketingActivity
{
    public int Id { get; set; }

    /// <summary>
    /// Type of activity (e.g., Call, Email, Meeting, Demo, Follow-up)
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Activity timing
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public int? DurationMinutes { get; set; }

    // Status
    public string Status { get; set; } = "Completed";
    public string? Outcome { get; set; }

    // Associated entities
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Branch
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    // User who performed the activity
    public string? PerformedByUserId { get; set; }
    // Note: ApplicationUser navigation removed to avoid circular dependency with Infrastructure

    // Contact details
    public string? ContactMethod { get; set; }
    public string? ContactedPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    // Next steps
    public string? NextSteps { get; set; }
    public DateTime? FollowUpDate { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Attachments (reference to file storage)
    public string? AttachmentIds { get; set; }

    // Audit
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
