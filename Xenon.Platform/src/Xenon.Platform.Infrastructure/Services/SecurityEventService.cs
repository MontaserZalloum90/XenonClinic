using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

/// <summary>
/// Service for logging and analyzing security events.
/// Provides threat detection and alerting capabilities.
/// </summary>
public interface ISecurityEventService
{
    Task LogEventAsync(SecurityEventType eventType, SecurityEventContext context);
    Task<bool> IsIpSuspiciousAsync(string ipAddress);
    Task<bool> IsBruteForceAttemptAsync(string? email, string? ipAddress);
    Task<SecurityRiskAssessment> AssessLoginRiskAsync(string email, string? ipAddress, string? userAgent);
    Task<IEnumerable<SecurityEvent>> GetRecentEventsAsync(Guid userId, string userType, int count = 10);
    Task<SecurityEventStatistics> GetStatisticsAsync(DateTime from, DateTime to, Guid? tenantId = null);
}

public class SecurityEventService : ISecurityEventService
{
    private readonly PlatformDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SecurityEventService> _logger;
    private readonly SecurityEventOptions _options;

    public SecurityEventService(
        PlatformDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<SecurityEventService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _options = new SecurityEventOptions();
        configuration.GetSection("Security:Events").Bind(_options);
    }

    public async Task LogEventAsync(SecurityEventType eventType, SecurityEventContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var securityEvent = new SecurityEvent
        {
            EventType = eventType,
            UserId = context.UserId,
            UserType = context.UserType,
            TenantId = context.TenantId,
            Email = context.Email,
            IpAddress = context.IpAddress ?? httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = context.UserAgent ?? httpContext?.Request?.Headers["User-Agent"].ToString(),
            DeviceFingerprint = context.DeviceFingerprint,
            IsSuccessful = context.IsSuccessful,
            Details = context.Details,
            ErrorMessage = context.ErrorMessage,
            SessionId = context.SessionId,
            RequestId = httpContext?.TraceIdentifier,
            RiskLevel = await AssessRiskLevelAsync(eventType, context)
        };

        // Check if alert should be triggered
        if (ShouldTriggerAlert(eventType, securityEvent.RiskLevel))
        {
            securityEvent.AlertTriggered = true;
            await TriggerAlertAsync(securityEvent);
        }

        _context.SecurityEvents.Add(securityEvent);
        await _context.SaveChangesAsync();

        // Log high-risk events at warning level
        if (securityEvent.RiskLevel >= RiskLevel.High)
        {
            _logger.LogWarning(
                "Security event {EventType} for {Email} from {IpAddress} - Risk: {RiskLevel}",
                eventType, context.Email, securityEvent.IpAddress, securityEvent.RiskLevel);
        }
        else
        {
            _logger.LogInformation(
                "Security event {EventType} for {Email} from {IpAddress}",
                eventType, context.Email, securityEvent.IpAddress);
        }
    }

    public async Task<bool> IsIpSuspiciousAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return false;

        var cutoff = DateTime.UtcNow.AddHours(-24);

        // Check for multiple failed logins from this IP
        var failedAttempts = await _context.SecurityEvents
            .CountAsync(e => e.IpAddress == ipAddress &&
                            e.EventType == SecurityEventType.LoginFailed &&
                            e.CreatedAt > cutoff);

        if (failedAttempts >= _options.SuspiciousIpThreshold)
            return true;

        // Check for login attempts across multiple accounts
        var uniqueEmails = await _context.SecurityEvents
            .Where(e => e.IpAddress == ipAddress &&
                       (e.EventType == SecurityEventType.LoginFailed || e.EventType == SecurityEventType.LoginSuccess) &&
                       e.CreatedAt > cutoff &&
                       e.Email != null)
            .Select(e => e.Email)
            .Distinct()
            .CountAsync();

        return uniqueEmails >= _options.SuspiciousIpMultiAccountThreshold;
    }

    public async Task<bool> IsBruteForceAttemptAsync(string? email, string? ipAddress)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-_options.BruteForceWindowMinutes);

        var query = _context.SecurityEvents
            .Where(e => e.EventType == SecurityEventType.LoginFailed && e.CreatedAt > cutoff);

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(e => e.Email == email);
        }
        else if (!string.IsNullOrEmpty(ipAddress))
        {
            query = query.Where(e => e.IpAddress == ipAddress);
        }
        else
        {
            return false;
        }

        var failedCount = await query.CountAsync();
        return failedCount >= _options.BruteForceThreshold;
    }

    public async Task<SecurityRiskAssessment> AssessLoginRiskAsync(string email, string? ipAddress, string? userAgent)
    {
        var assessment = new SecurityRiskAssessment();

        // Check for recent successful logins
        var lastSuccessfulLogin = await _context.SecurityEvents
            .Where(e => e.Email == email &&
                       e.EventType == SecurityEventType.LoginSuccess &&
                       e.IsSuccessful)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        // New device/IP check
        if (lastSuccessfulLogin != null)
        {
            if (!string.IsNullOrEmpty(ipAddress) && lastSuccessfulLogin.IpAddress != ipAddress)
            {
                assessment.IsNewIp = true;
                assessment.Factors.Add("Login from new IP address");
            }

            if (!string.IsNullOrEmpty(userAgent) && lastSuccessfulLogin.UserAgent != userAgent)
            {
                assessment.IsNewDevice = true;
                assessment.Factors.Add("Login from new device/browser");
            }
        }
        else
        {
            assessment.IsFirstLogin = true;
            assessment.Factors.Add("First login for this account");
        }

        // Check if IP is suspicious
        if (!string.IsNullOrEmpty(ipAddress) && await IsIpSuspiciousAsync(ipAddress))
        {
            assessment.IsSuspiciousIp = true;
            assessment.Factors.Add("Login from suspicious IP address");
        }

        // Check for brute force
        if (await IsBruteForceAttemptAsync(email, ipAddress))
        {
            assessment.IsBruteForceDetected = true;
            assessment.Factors.Add("Possible brute force attack detected");
        }

        // Check for unusual time
        var hour = DateTime.UtcNow.Hour;
        if (hour >= 2 && hour <= 5)
        {
            assessment.IsUnusualTime = true;
            assessment.Factors.Add("Login at unusual time");
        }

        // Calculate overall risk level
        assessment.RiskLevel = CalculateRiskLevel(assessment);

        return assessment;
    }

    public async Task<IEnumerable<SecurityEvent>> GetRecentEventsAsync(Guid userId, string userType, int count = 10)
    {
        return await _context.SecurityEvents
            .Where(e => e.UserId == userId && e.UserType == userType)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<SecurityEventStatistics> GetStatisticsAsync(DateTime from, DateTime to, Guid? tenantId = null)
    {
        var query = _context.SecurityEvents
            .Where(e => e.CreatedAt >= from && e.CreatedAt <= to);

        if (tenantId.HasValue)
        {
            query = query.Where(e => e.TenantId == tenantId);
        }

        var events = await query.ToListAsync();

        return new SecurityEventStatistics
        {
            TotalEvents = events.Count,
            SuccessfulLogins = events.Count(e => e.EventType == SecurityEventType.LoginSuccess && e.IsSuccessful),
            FailedLogins = events.Count(e => e.EventType == SecurityEventType.LoginFailed),
            LockedAccounts = events.Count(e => e.EventType == SecurityEventType.LoginLockedOut),
            PasswordChanges = events.Count(e => e.EventType == SecurityEventType.PasswordChanged),
            HighRiskEvents = events.Count(e => e.RiskLevel >= RiskLevel.High),
            AlertsTriggered = events.Count(e => e.AlertTriggered),
            UniqueIpAddresses = events.Select(e => e.IpAddress).Where(ip => !string.IsNullOrEmpty(ip)).Distinct().Count(),
            SuspiciousIpEvents = events.Count(e => e.EventType == SecurityEventType.SuspiciousIpAddress),
            BruteForceAttempts = events.Count(e => e.EventType == SecurityEventType.BruteForceAttempt)
        };
    }

    private async Task<RiskLevel> AssessRiskLevelAsync(SecurityEventType eventType, SecurityEventContext context)
    {
        // High-risk event types
        if (eventType is SecurityEventType.BruteForceAttempt or
            SecurityEventType.TokenReuseAttempt or
            SecurityEventType.SuspiciousIpAddress)
        {
            return RiskLevel.High;
        }

        // Medium-risk event types
        if (eventType is SecurityEventType.LoginLockedOut or
            SecurityEventType.MfaFailed)
        {
            return RiskLevel.Medium;
        }

        // Check IP suspiciousness for login events
        if (eventType == SecurityEventType.LoginFailed && !string.IsNullOrEmpty(context.IpAddress))
        {
            if (await IsIpSuspiciousAsync(context.IpAddress))
            {
                return RiskLevel.High;
            }

            if (await IsBruteForceAttemptAsync(context.Email, context.IpAddress))
            {
                return RiskLevel.High;
            }
        }

        return RiskLevel.Low;
    }

    private static RiskLevel CalculateRiskLevel(SecurityRiskAssessment assessment)
    {
        var score = 0;

        if (assessment.IsBruteForceDetected) score += 4;
        if (assessment.IsSuspiciousIp) score += 3;
        if (assessment.IsNewIp) score += 1;
        if (assessment.IsNewDevice) score += 1;
        if (assessment.IsUnusualTime) score += 1;
        if (assessment.IsFirstLogin) score += 1;

        return score switch
        {
            >= 4 => RiskLevel.High,
            >= 2 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private bool ShouldTriggerAlert(SecurityEventType eventType, RiskLevel riskLevel)
    {
        if (riskLevel >= RiskLevel.High)
            return true;

        return eventType is SecurityEventType.BruteForceAttempt or
               SecurityEventType.TokenReuseAttempt or
               SecurityEventType.SuspiciousIpAddress or
               SecurityEventType.AdminImpersonation;
    }

    private async Task TriggerAlertAsync(SecurityEvent securityEvent)
    {
        // In production, this would send notifications (email, Slack, PagerDuty, etc.)
        _logger.LogWarning(
            "SECURITY ALERT: {EventType} - User: {Email}, IP: {IpAddress}, Risk: {RiskLevel}",
            securityEvent.EventType,
            securityEvent.Email,
            securityEvent.IpAddress,
            securityEvent.RiskLevel);

        // Mark event as requiring review
        securityEvent.AlertTriggered = true;

        await Task.CompletedTask; // Placeholder for actual alert implementation
    }
}

public class SecurityEventContext
{
    public Guid? UserId { get; set; }
    public string? UserType { get; set; }
    public Guid? TenantId { get; set; }
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceFingerprint { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SessionId { get; set; }
}

public class SecurityRiskAssessment
{
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    public bool IsFirstLogin { get; set; }
    public bool IsNewIp { get; set; }
    public bool IsNewDevice { get; set; }
    public bool IsSuspiciousIp { get; set; }
    public bool IsBruteForceDetected { get; set; }
    public bool IsUnusualTime { get; set; }
    public List<string> Factors { get; set; } = new();
    public bool RequiresMfa => RiskLevel >= RiskLevel.Medium;
}

public class SecurityEventStatistics
{
    public int TotalEvents { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public int LockedAccounts { get; set; }
    public int PasswordChanges { get; set; }
    public int HighRiskEvents { get; set; }
    public int AlertsTriggered { get; set; }
    public int UniqueIpAddresses { get; set; }
    public int SuspiciousIpEvents { get; set; }
    public int BruteForceAttempts { get; set; }
}

public class SecurityEventOptions
{
    public int BruteForceThreshold { get; set; } = 5;
    public int BruteForceWindowMinutes { get; set; } = 15;
    public int SuspiciousIpThreshold { get; set; } = 10;
    public int SuspiciousIpMultiAccountThreshold { get; set; } = 5;
}
