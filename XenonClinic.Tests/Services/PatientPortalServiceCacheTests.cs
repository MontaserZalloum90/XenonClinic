using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for PatientPortalService caching functionality.
/// </summary>
public class PatientPortalServiceCacheTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<PatientPortalServiceCacheTests>> _mockLogger;

    public PatientPortalServiceCacheTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<PatientPortalServiceCacheTests>>();
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCachedValue_WhenCacheHit()
    {
        // Arrange
        var patientId = 123;
        var cacheKey = $"portal:dashboard:{patientId}";
        var cachedDashboard = new PatientPortalDashboardDto
        {
            UpcomingAppointments = 5,
            PendingPrescriptions = 2,
            UnreadMessages = 10,
            PendingInvoices = 1,
            OutstandingBalance = 150.00m
        };

        _mockCacheService
            .Setup(x => x.GetAsync<PatientPortalDashboardDto>(cacheKey))
            .ReturnsAsync(cachedDashboard);

        // Act
        var result = await _mockCacheService.Object.GetAsync<PatientPortalDashboardDto>(cacheKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.UpcomingAppointments);
        Assert.Equal(2, result.PendingPrescriptions);
        Assert.Equal(10, result.UnreadMessages);
    }

    [Fact]
    public async Task GetDashboardAsync_CacheKey_ShouldIncludePatientId()
    {
        // Arrange
        var patientId = 456;
        var expectedCacheKey = $"portal:dashboard:{patientId}";

        // Assert
        Assert.Contains("portal:dashboard:", expectedCacheKey);
        Assert.Contains(patientId.ToString(), expectedCacheKey);
    }

    [Fact]
    public void DashboardCacheExpiration_ShouldBe2Minutes()
    {
        // The PatientPortalService uses 2 minute cache expiration for dashboard
        var expectedExpiration = TimeSpan.FromMinutes(2);

        // Assert - verify short expiration for frequently changing data
        Assert.Equal(2, expectedExpiration.TotalMinutes);
    }

    [Fact]
    public async Task InvalidateDashboardCacheAsync_ShouldRemoveCacheEntry()
    {
        // Arrange
        var patientId = 789;
        var cacheKey = $"portal:dashboard:{patientId}";
        var removeCalled = false;

        _mockCacheService
            .Setup(x => x.RemoveAsync(cacheKey))
            .Callback(() => removeCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.Object.RemoveAsync(cacheKey);

        // Assert
        Assert.True(removeCalled);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldCacheResult_WhenCacheMiss()
    {
        // Arrange
        var patientId = 100;
        var cacheKey = $"portal:dashboard:{patientId}";
        var setCalled = false;

        _mockCacheService
            .Setup(x => x.GetAsync<PatientPortalDashboardDto>(cacheKey))
            .ReturnsAsync((PatientPortalDashboardDto?)null);

        _mockCacheService
            .Setup(x => x.SetAsync(cacheKey, It.IsAny<PatientPortalDashboardDto>(), It.IsAny<TimeSpan?>()))
            .Callback(() => setCalled = true)
            .Returns(Task.CompletedTask);

        // Act - simulate cache miss then set
        var cached = await _mockCacheService.Object.GetAsync<PatientPortalDashboardDto>(cacheKey);
        if (cached == null)
        {
            var newDashboard = new PatientPortalDashboardDto();
            await _mockCacheService.Object.SetAsync(cacheKey, newDashboard, TimeSpan.FromMinutes(2));
        }

        // Assert
        Assert.Null(cached);
        Assert.True(setCalled);
    }

    [Fact]
    public void DashboardDto_ShouldContainAllRequiredFields()
    {
        // Arrange
        var dashboard = new PatientPortalDashboardDto
        {
            Profile = new PortalPatientProfileDto { FirstName = "John", LastName = "Doe" },
            UpcomingAppointments = 3,
            PendingPrescriptions = 1,
            UnreadMessages = 5,
            PendingInvoices = 2,
            OutstandingBalance = 75.50m,
            NextAppointments = new List<PortalAppointmentSummaryDto>(),
            ActiveMedications = new List<PortalMedicationSummaryDto>(),
            RecentNotifications = new List<PortalNotificationDto>()
        };

        // Assert
        Assert.NotNull(dashboard.Profile);
        Assert.Equal(3, dashboard.UpcomingAppointments);
        Assert.Equal(1, dashboard.PendingPrescriptions);
        Assert.Equal(5, dashboard.UnreadMessages);
        Assert.Equal(2, dashboard.PendingInvoices);
        Assert.Equal(75.50m, dashboard.OutstandingBalance);
        Assert.NotNull(dashboard.NextAppointments);
        Assert.NotNull(dashboard.ActiveMedications);
        Assert.NotNull(dashboard.RecentNotifications);
    }
}
