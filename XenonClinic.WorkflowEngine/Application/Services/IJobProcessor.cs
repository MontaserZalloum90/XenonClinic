namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Service for processing background jobs.
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Enqueues a new job.
    /// </summary>
    Task<AsyncJob> EnqueueAsync(
        EnqueueJobRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs ready for processing.
    /// </summary>
    Task<IList<AsyncJob>> GetPendingJobsAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a lock on a job for processing.
    /// </summary>
    Task<bool> TryAcquireJobLockAsync(
        string jobId,
        string lockOwner,
        TimeSpan lockDuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a single job.
    /// </summary>
    Task ProcessJobAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a job as completed.
    /// </summary>
    Task CompleteJobAsync(
        string jobId,
        string? result = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a job as failed and schedules retry if applicable.
    /// </summary>
    Task FailJobAsync(
        string jobId,
        string error,
        bool shouldRetry = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets job status and details.
    /// </summary>
    Task<JobDto?> GetJobAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs for a process instance.
    /// </summary>
    Task<IList<JobDto>> GetProcessJobsAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending job.
    /// </summary>
    Task CancelJobAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old completed/failed jobs.
    /// </summary>
    Task CleanupOldJobsAsync(
        TimeSpan olderThan,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class EnqueueJobRequest
{
    public int TenantId { get; set; }
    public Guid? ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public Dictionary<string, object>? Payload { get; set; }
    public int Priority { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);
}

public class JobDto
{
    public string Id { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public Guid? ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public int Priority { get; set; }
    public Dictionary<string, object>? Payload { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
}

#endregion

/// <summary>
/// Standard job types.
/// </summary>
public static class JobTypes
{
    public const string ServiceTask = "service-task";
    public const string ScriptTask = "script-task";
    public const string SendMessage = "send-message";
    public const string SendEmail = "send-email";
    public const string HttpCall = "http-call";
    public const string RetryActivity = "retry-activity";
    public const string TimerFire = "timer-fire";
    public const string Escalation = "escalation";
    public const string ProcessCleanup = "process-cleanup";
}
