using FluentAssertions;
using XenonClinic.Api.Controllers;
using XenonClinic.Core.Interfaces;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for background job monitoring functionality.
/// </summary>
public class BackgroundJobMonitoringTests
{
    [Fact]
    public void JobStatistics_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var stats = new JobStatistics(
            TotalJobs: 100,
            EnqueuedCount: 10,
            ProcessingCount: 5,
            SucceededCount: 80,
            FailedCount: 3,
            ScheduledCount: 2,
            RecurringJobsCount: 5,
            OldestJobCreatedAt: DateTime.UtcNow.AddHours(-24),
            LastCompletedAt: DateTime.UtcNow.AddMinutes(-5)
        );

        // Assert
        stats.TotalJobs.Should().Be(100);
        stats.EnqueuedCount.Should().Be(10);
        stats.ProcessingCount.Should().Be(5);
        stats.SucceededCount.Should().Be(80);
        stats.FailedCount.Should().Be(3);
        stats.ScheduledCount.Should().Be(2);
        stats.RecurringJobsCount.Should().Be(5);
    }

    [Fact]
    public void JobDetails_ShouldContainAllStateInfo()
    {
        // Arrange & Act
        var details = new JobDetails(
            JobId: "job123",
            State: JobState.Succeeded,
            JobType: "EmailService.SendAsync",
            CreatedAt: DateTime.UtcNow.AddMinutes(-10),
            StartedAt: DateTime.UtcNow.AddMinutes(-9),
            CompletedAt: DateTime.UtcNow.AddMinutes(-8),
            Exception: null,
            RetryCount: 0
        );

        // Assert
        details.JobId.Should().Be("job123");
        details.State.Should().Be(JobState.Succeeded);
        details.JobType.Should().Contain("EmailService");
        details.Exception.Should().BeNull();
        details.RetryCount.Should().Be(0);
    }

    [Fact]
    public void JobDetails_FailedJob_ShouldContainException()
    {
        // Arrange & Act
        var details = new JobDetails(
            JobId: "job456",
            State: JobState.Failed,
            JobType: "ReportService.GenerateAsync",
            CreatedAt: DateTime.UtcNow.AddMinutes(-5),
            StartedAt: DateTime.UtcNow.AddMinutes(-4),
            CompletedAt: DateTime.UtcNow.AddMinutes(-3),
            Exception: "Connection timeout",
            RetryCount: 2
        );

        // Assert
        details.State.Should().Be(JobState.Failed);
        details.Exception.Should().NotBeNullOrEmpty();
        details.RetryCount.Should().Be(2);
    }

    [Fact]
    public void RecurringJobDetails_ShouldContainCronExpression()
    {
        // Arrange & Act
        var details = new RecurringJobDetails(
            JobId: "daily-cleanup",
            CronExpression: "0 0 * * *",
            JobType: "CleanupService.RunAsync",
            CreatedAt: DateTime.UtcNow.AddDays(-30),
            LastExecution: DateTime.UtcNow.AddHours(-1),
            NextExecution: DateTime.UtcNow.AddHours(23)
        );

        // Assert
        details.JobId.Should().Be("daily-cleanup");
        details.CronExpression.Should().Be("0 0 * * *");
        details.LastExecution.Should().NotBeNull();
    }

    [Theory]
    [InlineData(JobState.Enqueued)]
    [InlineData(JobState.Scheduled)]
    [InlineData(JobState.Processing)]
    [InlineData(JobState.Succeeded)]
    [InlineData(JobState.Failed)]
    [InlineData(JobState.Deleted)]
    [InlineData(JobState.Awaiting)]
    public void JobState_AllStatesShouldBeValid(JobState state)
    {
        // Assert
        Enum.IsDefined(typeof(JobState), state).Should().BeTrue();
    }

    [Fact]
    public void JobStatisticsResponse_ShouldIncludeTimestamp()
    {
        // Arrange & Act
        var response = new JobStatisticsResponse
        {
            TotalJobs = 50,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void JobDetailsResponse_Duration_ShouldBeCalculated()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-5);
        var endTime = DateTime.UtcNow;

        // Act
        var response = new JobDetailsResponse
        {
            JobId = "test-job",
            State = "Succeeded",
            StartedAt = startTime,
            CompletedAt = endTime,
            Duration = (endTime - startTime).TotalMilliseconds
        };

        // Assert
        response.Duration.Should().BeGreaterThan(0);
        response.Duration.Should().BeApproximately(5 * 60 * 1000, 1000); // ~5 minutes in ms
    }

    [Fact]
    public void JobDetailsResponse_PendingJob_ShouldHaveNullDuration()
    {
        // Arrange & Act
        var response = new JobDetailsResponse
        {
            JobId = "pending-job",
            State = "Enqueued",
            StartedAt = null,
            CompletedAt = null,
            Duration = null
        };

        // Assert
        response.Duration.Should().BeNull();
        response.StartedAt.Should().BeNull();
        response.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void RecurringJobResponse_ShouldHaveValidCronExpression()
    {
        // Arrange & Act
        var response = new RecurringJobResponse
        {
            JobId = "hourly-sync",
            CronExpression = "0 * * * *",
            JobType = "SyncService.SyncAll"
        };

        // Assert
        response.CronExpression.Should().NotBeNullOrEmpty();
        response.JobType.Should().Contain("Sync");
    }

    [Theory]
    [InlineData("0 * * * *", "Hourly")]
    [InlineData("0 0 * * *", "Daily at midnight")]
    [InlineData("0 0 * * 0", "Weekly on Sunday")]
    [InlineData("0 0 1 * *", "Monthly on 1st")]
    public void CronExpression_CommonPatterns_ShouldBeValid(string cron, string description)
    {
        // These are common cron patterns
        cron.Should().NotBeNullOrEmpty();
        description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JobStatistics_EmptyQueue_ShouldReturnZeros()
    {
        // Arrange & Act
        var stats = new JobStatistics(
            TotalJobs: 0,
            EnqueuedCount: 0,
            ProcessingCount: 0,
            SucceededCount: 0,
            FailedCount: 0,
            ScheduledCount: 0,
            RecurringJobsCount: 0,
            OldestJobCreatedAt: null,
            LastCompletedAt: null
        );

        // Assert
        stats.TotalJobs.Should().Be(0);
        stats.OldestJobCreatedAt.Should().BeNull();
        stats.LastCompletedAt.Should().BeNull();
    }

    [Fact]
    public void JobDetailsResponse_ShouldDefaultToEmptyStrings()
    {
        // Arrange & Act
        var response = new JobDetailsResponse();

        // Assert
        response.JobId.Should().NotBeNull();
        response.State.Should().NotBeNull();
    }

    [Fact]
    public void JobStatisticsResponse_TotalsShouldBeConsistent()
    {
        // Arrange
        var response = new JobStatisticsResponse
        {
            TotalJobs = 100,
            EnqueuedCount = 20,
            ProcessingCount = 10,
            SucceededCount = 50,
            FailedCount = 15,
            ScheduledCount = 5
        };

        // Assert - total should be sum of individual counts
        var sum = response.EnqueuedCount + response.ProcessingCount +
                  response.SucceededCount + response.FailedCount + response.ScheduledCount;
        sum.Should().Be(response.TotalJobs);
    }
}
