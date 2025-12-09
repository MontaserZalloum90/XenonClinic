using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class DemoRequest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Company { get; set; } = string.Empty;

    public CompanyType? CompanyType { get; set; }
    public ClinicType? ClinicType { get; set; }

    public int? EstimatedBranches { get; set; }
    public int? EstimatedUsers { get; set; }

    public string InquiryType { get; set; } = "demo"; // demo, contact, quote
    public string? Message { get; set; }
    public string? ModulesOfInterest { get; set; } // Comma-separated
    public string? DeploymentPreference { get; set; } // CLOUD, ON_PREM, HYBRID

    public string Status { get; set; } = "New"; // New, Contacted, Qualified, Converted, Lost
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }

    public DateTime? ContactedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public Guid? ConvertedTenantId { get; set; }

    public string? Source { get; set; } // website, referral, campaign
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
