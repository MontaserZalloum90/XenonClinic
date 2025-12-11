using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for SecurityEventService - validates security event logging,
/// suspicious activity detection, and risk assessment.
/// </summary>
public class SecurityEventServiceTests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<SecurityEventService>> _loggerMock;
    private readonly ISecurityEventService _service;

    public SecurityEventServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<SecurityEventService>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Events:BruteForceThreshold"] = "5",
                ["Security:Events:BruteForceWindowMinutes"] = "15",
                ["Security:Events:SuspiciousIpThreshold"] = "10",
                ["Security:Events:SuspiciousIpMultiAccountThreshold"] = "5"
            })
            .Build();

        _service = new SecurityEventService(
            _context,
            _httpContextAccessorMock.Object,
            configuration,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Event Logging Tests

    [Fact]
    public async Task LogEventAsync_ShouldPersistEvent()
    {
        // Arrange
        var context = new SecurityEventContext
        {
            UserId = Guid.NewGuid(),
            UserType = "PlatformAdmin",
            Email = "test@example.com",
            IpAddress = "192.168.1.1",
            IsSuccessful = true
        };

        // Act
        await _service.LogEventAsync(SecurityEventType.LoginSuccess, context);

        // Assert
        var events = await _context.SecurityEvents.ToListAsync();
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be(SecurityEventType.LoginSuccess);
        events[0].Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LogEventAsync_ShouldSetRiskLevel()
    {
        // Arrange
        var context = new SecurityEventContext
        {
            Email = "test@example.com",
            IpAddress = "192.168.1.1",
            IsSuccessful = false
        };

        // Act
        await _service.LogEventAsync(SecurityEventType.LoginFailed, context);

        // Assert
        var securityEvent = await _context.SecurityEvents.FirstAsync();
        securityEvent.RiskLevel.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public async Task LogEventAsync_ShouldTriggerAlert_ForHighRiskEvents()
    {
        // Arrange
        var context = new SecurityEventContext
        {
            Email = "test@example.com",
            IpAddress = "192.168.1.1"
        };

        // Act
        await _service.LogEventAsync(SecurityEventType.BruteForceAttempt, context);

        // Assert
        var securityEvent = await _context.SecurityEvents.FirstAsync();
        securityEvent.AlertTriggered.Should().BeTrue();
        securityEvent.RiskLevel.Should().Be(RiskLevel.High);
    }

    #endregion

    #region Suspicious IP Detection Tests

    [Fact]
    public async Task IsIpSuspiciousAsync_ShouldReturnFalse_ForNewIp()
    {
        // Act
        var result = await _service.IsIpSuspiciousAsync("192.168.1.100");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsIpSuspiciousAsync_ShouldReturnTrue_AfterMultipleFailures()
    {
        // Arrange - Create 10 failed login events from same IP
        var ipAddress = "192.168.1.200";
        for (int i = 0; i < 10; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginFailed,
                IpAddress = ipAddress,
                Email = $"user{i}@example.com",
                IsSuccessful = false,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsIpSuspiciousAsync(ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsIpSuspiciousAsync_ShouldReturnTrue_WhenMultipleAccountsTargeted()
    {
        // Arrange - Create login attempts across multiple accounts from same IP
        var ipAddress = "192.168.1.201";
        for (int i = 0; i < 5; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginSuccess,
                IpAddress = ipAddress,
                Email = $"distinct{i}@example.com",
                IsSuccessful = true,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsIpSuspiciousAsync(ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Brute Force Detection Tests

    [Fact]
    public async Task IsBruteForceAttemptAsync_ShouldReturnFalse_ForFewAttempts()
    {
        // Arrange
        var email = "user@example.com";
        for (int i = 0; i < 3; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginFailed,
                Email = email,
                IsSuccessful = false,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsBruteForceAttemptAsync(email, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBruteForceAttemptAsync_ShouldReturnTrue_AfterThresholdExceeded()
    {
        // Arrange
        var email = "victim@example.com";
        for (int i = 0; i < 6; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginFailed,
                Email = email,
                IsSuccessful = false,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsBruteForceAttemptAsync(email, null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBruteForceAttemptAsync_ShouldIgnoreOldAttempts()
    {
        // Arrange
        var email = "user@example.com";
        for (int i = 0; i < 10; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginFailed,
                Email = email,
                IsSuccessful = false,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddHours(-2) // Outside 15 min window
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsBruteForceAttemptAsync(email, null);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Risk Assessment Tests

    [Fact]
    public async Task AssessLoginRiskAsync_ShouldReturnLowRisk_ForRegularLogin()
    {
        // Arrange
        var email = "regular@example.com";
        var ipAddress = "192.168.1.50";

        // Create previous successful login from same IP
        _context.SecurityEvents.Add(new SecurityEvent
        {
            EventType = SecurityEventType.LoginSuccess,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = "Mozilla/5.0",
            IsSuccessful = true,
            RiskLevel = RiskLevel.Low,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        // Act
        var assessment = await _service.AssessLoginRiskAsync(email, ipAddress, "Mozilla/5.0");

        // Assert
        assessment.RiskLevel.Should().Be(RiskLevel.Low);
        assessment.RequiresMfa.Should().BeFalse();
    }

    [Fact]
    public async Task AssessLoginRiskAsync_ShouldFlagNewIp()
    {
        // Arrange
        var email = "user@example.com";

        // Create previous login from different IP
        _context.SecurityEvents.Add(new SecurityEvent
        {
            EventType = SecurityEventType.LoginSuccess,
            Email = email,
            IpAddress = "192.168.1.1",
            IsSuccessful = true,
            RiskLevel = RiskLevel.Low,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        // Act
        var assessment = await _service.AssessLoginRiskAsync(email, "10.0.0.1", null);

        // Assert
        assessment.IsNewIp.Should().BeTrue();
        assessment.Factors.Should().Contain(f => f.Contains("new IP"));
    }

    [Fact]
    public async Task AssessLoginRiskAsync_ShouldFlagFirstLogin()
    {
        // Arrange - No previous logins

        // Act
        var assessment = await _service.AssessLoginRiskAsync("newuser@example.com", "192.168.1.1", null);

        // Assert
        assessment.IsFirstLogin.Should().BeTrue();
        assessment.Factors.Should().Contain(f => f.Contains("First login"));
    }

    [Fact]
    public async Task AssessLoginRiskAsync_ShouldReturnHighRisk_WhenBruteForceDetected()
    {
        // Arrange
        var email = "target@example.com";

        // Create multiple failed attempts
        for (int i = 0; i < 6; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                EventType = SecurityEventType.LoginFailed,
                Email = email,
                IsSuccessful = false,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var assessment = await _service.AssessLoginRiskAsync(email, null, null);

        // Assert
        assessment.IsBruteForceDetected.Should().BeTrue();
        assessment.RiskLevel.Should().Be(RiskLevel.High);
        assessment.RequiresMfa.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        _context.SecurityEvents.AddRange(new[]
        {
            new SecurityEvent { EventType = SecurityEventType.LoginSuccess, IsSuccessful = true, RiskLevel = RiskLevel.Low, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new SecurityEvent { EventType = SecurityEventType.LoginSuccess, IsSuccessful = true, RiskLevel = RiskLevel.Low, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new SecurityEvent { EventType = SecurityEventType.LoginFailed, IsSuccessful = false, RiskLevel = RiskLevel.Low, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new SecurityEvent { EventType = SecurityEventType.PasswordChanged, IsSuccessful = true, RiskLevel = RiskLevel.Low, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new SecurityEvent { EventType = SecurityEventType.BruteForceAttempt, IsSuccessful = false, RiskLevel = RiskLevel.High, AlertTriggered = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        });
        await _context.SaveChangesAsync();

        // Act
        var stats = await _service.GetStatisticsAsync(from, to);

        // Assert
        stats.TotalEvents.Should().Be(5);
        stats.SuccessfulLogins.Should().Be(2);
        stats.FailedLogins.Should().Be(1);
        stats.PasswordChanges.Should().Be(1);
        stats.HighRiskEvents.Should().Be(1);
        stats.AlertsTriggered.Should().Be(1);
    }

    [Fact]
    public async Task GetRecentEventsAsync_ShouldReturnUserEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userType = "PlatformAdmin";

        for (int i = 0; i < 15; i++)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                UserId = userId,
                UserType = userType,
                EventType = SecurityEventType.LoginSuccess,
                IsSuccessful = true,
                RiskLevel = RiskLevel.Low,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var events = await _service.GetRecentEventsAsync(userId, userType, 10);

        // Assert
        events.Should().HaveCount(10);
        events.Should().BeInDescendingOrder(e => e.CreatedAt);
    }

    #endregion
}
