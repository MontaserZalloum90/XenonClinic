using System.ComponentModel.DataAnnotations;

namespace Xenon.Platform.Application.DTOs;

public record DemoRequestSubmission
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Phone]
    public string? Phone { get; init; }

    [Required]
    [MaxLength(200)]
    public string Company { get; init; } = string.Empty;

    public string? CompanyType { get; init; }

    public string? ClinicType { get; init; }

    public int? EstimatedBranches { get; init; }

    public int? EstimatedUsers { get; init; }

    public string? InquiryType { get; init; }

    [MaxLength(2000)]
    public string? Message { get; init; }

    public string[]? ModulesOfInterest { get; init; }

    public string? DeploymentPreference { get; init; }

    public string? Source { get; init; }

    public string? UtmSource { get; init; }

    public string? UtmMedium { get; init; }

    public string? UtmCampaign { get; init; }

    /// <summary>
    /// Honeypot field for spam detection - should always be empty
    /// </summary>
    public string? Honeypot { get; init; }
}

public record DemoRequestResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
}
