using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.Public;

[ApiController]
[Route("api/public/demo-request")]
public class DemoRequestController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<DemoRequestController> _logger;

    public DemoRequestController(
        PlatformDbContext context,
        IAuditService auditService,
        ILogger<DemoRequestController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a demo request or contact inquiry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitDemoRequest([FromBody] DemoRequestSubmission request)
    {
        // Honeypot check for spam
        if (!string.IsNullOrEmpty(request.Honeypot))
        {
            _logger.LogWarning("Honeypot triggered for demo request from {Email}", request.Email);
            // Return success to not tip off bots, but don't save
            return Ok(new { success = true, message = "Thank you for your interest!" });
        }

        var demoRequest = new DemoRequest
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            CompanyType = Enum.TryParse<CompanyType>(request.CompanyType, true, out var ct) ? ct : null,
            ClinicType = Enum.TryParse<ClinicType>(request.ClinicType, true, out var clt) ? clt : null,
            EstimatedBranches = request.EstimatedBranches,
            EstimatedUsers = request.EstimatedUsers,
            InquiryType = request.InquiryType ?? "demo",
            Message = request.Message,
            ModulesOfInterest = request.ModulesOfInterest != null
                ? string.Join(",", request.ModulesOfInterest)
                : null,
            DeploymentPreference = request.DeploymentPreference,
            Status = "New",
            Source = request.Source ?? "website",
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        };

        _context.DemoRequests.Add(demoRequest);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("DemoRequestCreated", "DemoRequest", demoRequest.Id,
            newValues: new { demoRequest.Email, demoRequest.Company, demoRequest.InquiryType });

        _logger.LogInformation("Demo request submitted by {Email} from {Company}",
            demoRequest.Email, demoRequest.Company);

        return Ok(new
        {
            success = true,
            message = "Thank you for your interest! Our team will contact you shortly.",
            data = new
            {
                id = demoRequest.Id,
                status = demoRequest.Status
            }
        });
    }
}

public class DemoRequestSubmission
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    public string? CompanyType { get; set; }

    public string? ClinicType { get; set; }

    public int? EstimatedBranches { get; set; }

    public int? EstimatedUsers { get; set; }

    public string? InquiryType { get; set; } // demo, contact, quote

    [MaxLength(2000)]
    public string? Message { get; set; }

    public string[]? ModulesOfInterest { get; set; }

    public string? DeploymentPreference { get; set; } // CLOUD, ON_PREM, HYBRID

    public string? Source { get; set; }

    public string? UtmSource { get; set; }

    public string? UtmMedium { get; set; }

    public string? UtmCampaign { get; set; }

    // Honeypot field - should always be empty
    public string? Honeypot { get; set; }
}
