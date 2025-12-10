using Microsoft.Extensions.Logging;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class DemoRequestService : IDemoRequestService
{
    private readonly PlatformDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<DemoRequestService> _logger;

    public DemoRequestService(
        PlatformDbContext context,
        IAuditService auditService,
        ILogger<DemoRequestService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<DemoRequestResponse>> SubmitAsync(
        DemoRequestSubmission request,
        string? clientIpAddress = null,
        string? userAgent = null)
    {
        // Honeypot check for spam
        if (!string.IsNullOrEmpty(request.Honeypot))
        {
            _logger.LogWarning("Honeypot triggered for demo request from {Email}", request.Email);
            // Return success to not tip off bots, but with empty ID
            return new DemoRequestResponse
            {
                Id = Guid.Empty,
                Status = "Received"
            };
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
            IpAddress = clientIpAddress,
            UserAgent = userAgent
        };

        _context.DemoRequests.Add(demoRequest);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("DemoRequestCreated", "DemoRequest", demoRequest.Id,
            newValues: new { demoRequest.Email, demoRequest.Company, demoRequest.InquiryType });

        _logger.LogInformation("Demo request submitted by {Email} from {Company}",
            demoRequest.Email, demoRequest.Company);

        return new DemoRequestResponse
        {
            Id = demoRequest.Id,
            Status = demoRequest.Status
        };
    }
}
