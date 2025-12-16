using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for monitoring and managing background jobs.
/// Provides visibility into job queue status and allows job management.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobsController : BaseApiController
{
    private readonly IBackgroundJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IBackgroundJobService jobService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// Get job statistics summary.
    /// Returns counts of jobs by state and timing information.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<JobStatisticsResponse>), StatusCodes.Status200OK)]
    public IActionResult GetStatistics()
    {
        var stats = _jobService.GetStatistics();

        var response = new JobStatisticsResponse
        {
            TotalJobs = stats.TotalJobs,
            EnqueuedCount = stats.EnqueuedCount,
            ProcessingCount = stats.ProcessingCount,
            SucceededCount = stats.SucceededCount,
            FailedCount = stats.FailedCount,
            ScheduledCount = stats.ScheduledCount,
            RecurringJobsCount = stats.RecurringJobsCount,
            OldestJobCreatedAt = stats.OldestJobCreatedAt,
            LastCompletedAt = stats.LastCompletedAt,
            Timestamp = DateTime.UtcNow
        };

        return ApiOk(response);
    }

    /// <summary>
    /// Get all jobs with optional state filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobDetailsResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetJobs(
        [FromQuery] JobState? state = null,
        [FromQuery] int limit = 100)
    {
        var jobs = _jobService.GetAllJobs(state)
            .Take(limit)
            .Select(j => new JobDetailsResponse
            {
                JobId = j.JobId,
                State = j.State.ToString(),
                JobType = j.JobType,
                CreatedAt = j.CreatedAt,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                Duration = j.CompletedAt.HasValue && j.StartedAt.HasValue
                    ? (j.CompletedAt.Value - j.StartedAt.Value).TotalMilliseconds
                    : null,
                Error = j.Exception,
                RetryCount = j.RetryCount
            });

        return ApiOk(jobs);
    }

    /// <summary>
    /// Get details of a specific job.
    /// </summary>
    [HttpGet("{jobId}")]
    [ProducesResponseType(typeof(ApiResponse<JobDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetJob(string jobId)
    {
        var job = _jobService.GetJobDetails(jobId);

        if (job == null)
        {
            return ApiNotFound($"Job {jobId} not found");
        }

        var response = new JobDetailsResponse
        {
            JobId = job.JobId,
            State = job.State.ToString(),
            JobType = job.JobType,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            Duration = job.CompletedAt.HasValue && job.StartedAt.HasValue
                ? (job.CompletedAt.Value - job.StartedAt.Value).TotalMilliseconds
                : null,
            Error = job.Exception,
            RetryCount = job.RetryCount
        };

        return ApiOk(response);
    }

    /// <summary>
    /// Get all recurring jobs.
    /// </summary>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecurringJobResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetRecurringJobs()
    {
        var jobs = _jobService.GetRecurringJobs()
            .Select(j => new RecurringJobResponse
            {
                JobId = j.JobId,
                CronExpression = j.CronExpression,
                JobType = j.JobType,
                CreatedAt = j.CreatedAt,
                LastExecution = j.LastExecution,
                NextExecution = j.NextExecution
            });

        return ApiOk(jobs);
    }

    /// <summary>
    /// Get failed jobs only.
    /// </summary>
    [HttpGet("failed")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobDetailsResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetFailedJobs([FromQuery] int limit = 50)
    {
        var jobs = _jobService.GetAllJobs(JobState.Failed)
            .Take(limit)
            .Select(j => new JobDetailsResponse
            {
                JobId = j.JobId,
                State = j.State.ToString(),
                JobType = j.JobType,
                CreatedAt = j.CreatedAt,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                Error = j.Exception,
                RetryCount = j.RetryCount
            });

        return ApiOk(jobs);
    }

    /// <summary>
    /// Requeue a failed job.
    /// </summary>
    [HttpPost("{jobId}/requeue")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public IActionResult RequeueJob(string jobId)
    {
        var job = _jobService.GetJobDetails(jobId);

        if (job == null)
        {
            return ApiNotFound($"Job {jobId} not found");
        }

        if (job.State != JobState.Failed)
        {
            return ApiBadRequest($"Only failed jobs can be requeued. Current state: {job.State}");
        }

        var success = _jobService.Requeue(jobId);

        if (success)
        {
            _logger.LogInformation("Job {JobId} requeued by admin", jobId);
            return ApiOk($"Job {jobId} has been requeued");
        }

        return ApiBadRequest($"Failed to requeue job {jobId}");
    }

    /// <summary>
    /// Delete a job.
    /// </summary>
    [HttpDelete("{jobId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public IActionResult DeleteJob(string jobId)
    {
        var success = _jobService.Delete(jobId);

        if (success)
        {
            _logger.LogInformation("Job {JobId} deleted by admin", jobId);
            return ApiOk($"Job {jobId} has been deleted");
        }

        return ApiNotFound($"Job {jobId} not found");
    }

    /// <summary>
    /// Trigger a recurring job to run immediately.
    /// </summary>
    [HttpPost("recurring/{jobId}/trigger")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public IActionResult TriggerRecurringJob(string jobId)
    {
        _jobService.TriggerRecurring(jobId);
        _logger.LogInformation("Recurring job {JobId} triggered by admin", jobId);
        return ApiOk($"Recurring job {jobId} has been triggered");
    }

    /// <summary>
    /// Remove a recurring job.
    /// </summary>
    [HttpDelete("recurring/{jobId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public IActionResult RemoveRecurringJob(string jobId)
    {
        _jobService.RemoveRecurring(jobId);
        _logger.LogInformation("Recurring job {JobId} removed by admin", jobId);
        return ApiOk($"Recurring job {jobId} has been removed");
    }
}

#region Response DTOs

/// <summary>
/// Job statistics response.
/// </summary>
public class JobStatisticsResponse
{
    public int TotalJobs { get; set; }
    public int EnqueuedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int SucceededCount { get; set; }
    public int FailedCount { get; set; }
    public int ScheduledCount { get; set; }
    public int RecurringJobsCount { get; set; }
    public DateTime? OldestJobCreatedAt { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Job details response.
/// </summary>
public class JobDetailsResponse
{
    public string JobId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? JobType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? Duration { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Recurring job response.
/// </summary>
public class RecurringJobResponse
{
    public string JobId { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string? JobType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecution { get; set; }
    public DateTime? NextExecution { get; set; }
}

#endregion
