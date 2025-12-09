using System.Linq.Expressions;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Background job scheduling service abstraction.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue a job to run immediately in the background.
    /// </summary>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Enqueue a job to run immediately in the background.
    /// </summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Schedule a job to run at a specific time.
    /// </summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Schedule a job to run at a specific time.
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Schedule a job to run after a delay.
    /// </summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Create or update a recurring job.
    /// </summary>
    void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    /// <summary>
    /// Remove a recurring job.
    /// </summary>
    void RemoveRecurring(string jobId);

    /// <summary>
    /// Trigger a recurring job to run immediately.
    /// </summary>
    void TriggerRecurring(string jobId);

    /// <summary>
    /// Continue with another job after the specified job completes.
    /// </summary>
    string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Delete a scheduled or enqueued job.
    /// </summary>
    bool Delete(string jobId);

    /// <summary>
    /// Requeue a failed job.
    /// </summary>
    bool Requeue(string jobId);

    /// <summary>
    /// Get job details.
    /// </summary>
    JobDetails? GetJobDetails(string jobId);
}

/// <summary>
/// Job details.
/// </summary>
public record JobDetails(
    string JobId,
    JobState State,
    string? JobType,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Exception,
    int RetryCount
);

/// <summary>
/// Job state.
/// </summary>
public enum JobState
{
    Enqueued = 0,
    Scheduled = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Deleted = 5,
    Awaiting = 6
}
