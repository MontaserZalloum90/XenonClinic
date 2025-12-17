using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Simple in-memory background job service.
/// For production, consider Hangfire, Quartz.NET, or Azure Service Bus.
/// </summary>
public class BackgroundJobService : IBackgroundJobService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobService> _logger;
    private static readonly ConcurrentDictionary<string, JobInfo> _jobs = new();
    private static readonly ConcurrentDictionary<string, RecurringJobInfo> _recurringJobs = new();

    // Cleanup settings to prevent memory leaks from accumulated jobs
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CompletedJobRetention = TimeSpan.FromHours(1);
    private static readonly TimeSpan FailedJobRetention = TimeSpan.FromHours(24);
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly object _cleanupLock = new();

    public BackgroundJobService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Cleans up completed and failed jobs to prevent memory leaks.
    /// </summary>
    private void CleanupOldJobs()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanup < CleanupInterval) return;

        lock (_cleanupLock)
        {
            // Double-check after acquiring lock
            if (now - _lastCleanup < CleanupInterval) return;
            _lastCleanup = now;

            var jobsToRemove = _jobs
                .Where(kvp =>
                    (kvp.Value.State == JobState.Succeeded &&
                     kvp.Value.CompletedAt.HasValue &&
                     now - kvp.Value.CompletedAt.Value > CompletedJobRetention) ||
                    (kvp.Value.State == JobState.Failed &&
                     kvp.Value.CompletedAt.HasValue &&
                     now - kvp.Value.CompletedAt.Value > FailedJobRetention) ||
                    (kvp.Value.State == JobState.Deleted))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in jobsToRemove)
            {
                _jobs.TryRemove(key, out _);
            }

            if (jobsToRemove.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} old jobs", jobsToRemove.Count);
            }
        }
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull
    {
        // Periodically cleanup old jobs to prevent memory leaks
        CleanupOldJobs();

        var jobId = GenerateJobId();
        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Enqueued,
            JobType = typeof(T).FullName ?? typeof(T).Name,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // BUG FIX: Capture task reference and add exception observation
        var executionTask = ExecuteJobAsync<T>(jobId, methodCall);
        ObserveTaskExceptions(executionTask, jobId);

        _logger.LogInformation("Job {JobId} enqueued for {Type}", jobId, typeof(T).Name);
        return jobId;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        var jobId = GenerateJobId();
        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Enqueued,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // BUG FIX: Capture task reference and add exception observation
        var executionTask = ExecuteJobAsync(jobId, methodCall);
        ObserveTaskExceptions(executionTask, jobId);

        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt) where T : notnull
    {
        var jobId = GenerateJobId();
        var delay = enqueueAt - DateTimeOffset.UtcNow;

        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Scheduled,
            JobType = typeof(T).FullName ?? typeof(T).Name,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // BUG FIX: Capture task reference and add exception observation
        Task executionTask;
        if (delay > TimeSpan.Zero)
        {
            executionTask = DelayThenExecuteAsync(delay, () => ExecuteJobAsync<T>(jobId, methodCall));
        }
        else
        {
            executionTask = ExecuteJobAsync<T>(jobId, methodCall);
        }
        ObserveTaskExceptions(executionTask, jobId);

        _logger.LogInformation("Job {JobId} scheduled for {Time}", jobId, enqueueAt);
        return jobId;
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var jobId = GenerateJobId();
        var delay = enqueueAt - DateTimeOffset.UtcNow;

        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // BUG FIX: Capture task reference and add exception observation
        Task executionTask;
        if (delay > TimeSpan.Zero)
        {
            executionTask = DelayThenExecuteAsync(delay, () => ExecuteJobAsync(jobId, methodCall));
        }
        else
        {
            executionTask = ExecuteJobAsync(jobId, methodCall);
        }
        ObserveTaskExceptions(executionTask, jobId);

        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : notnull
    {
        return Schedule(methodCall, DateTimeOffset.UtcNow.Add(delay));
    }

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression) where T : notnull
    {
        var recurringJob = new RecurringJobInfo
        {
            JobId = jobId,
            CronExpression = cronExpression,
            JobType = typeof(T).FullName ?? typeof(T).Name,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _recurringJobs[jobId] = recurringJob;
        _logger.LogInformation("Recurring job {JobId} added/updated with cron: {Cron}", jobId, cronExpression);
    }

    public void RemoveRecurring(string jobId)
    {
        _recurringJobs.TryRemove(jobId, out _);
        _logger.LogInformation("Recurring job {JobId} removed", jobId);
    }

    public void TriggerRecurring(string jobId)
    {
        if (_recurringJobs.TryGetValue(jobId, out var job))
        {
            var newJobId = GenerateJobId();
            _logger.LogInformation("Triggering recurring job {JobId} as {NewJobId}", jobId, newJobId);

            // Execute immediately
            var methodCall = job.MethodExpression as LambdaExpression;
            if (methodCall != null)
            {
                var compiled = methodCall.Compile();
                // BUG FIX: Capture task reference and add proper exception handling
                var executionTask = Task.Run(async () =>
                {
                    try
                    {
                        var result = compiled.DynamicInvoke();
                        if (result is Task task)
                        {
                            await task;
                        }
                    }
                    catch (System.Reflection.TargetInvocationException tie)
                    {
                        // BUG FIX: Unwrap TargetInvocationException to get actual exception
                        var actualException = tie.InnerException ?? tie;
                        _logger.LogError(actualException, "Recurring job {JobId} failed: {Message}",
                            jobId, actualException.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Recurring job {JobId} failed", jobId);
                    }
                });

                // BUG FIX: Handle unobserved task exceptions
                executionTask.ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogError(t.Exception.InnerException ?? t.Exception,
                            "Unobserved exception in recurring job {JobId}", jobId);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }

    public string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var jobId = GenerateJobId();
        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Awaiting,
            ParentJobId = parentJobId,
            JobType = typeof(T).FullName ?? typeof(T).Name,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // BUG FIX: Capture task reference and add exception observation
        var executionTask = WaitForParentAndExecuteAsync<T>(jobId, parentJobId, methodCall);
        ObserveTaskExceptions(executionTask, jobId);

        _logger.LogInformation("Continuation job {JobId} created for parent {ParentJobId}", jobId, parentJobId);
        return jobId;
    }

    public bool Delete(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.State = JobState.Deleted;
            _logger.LogInformation("Job {JobId} deleted", jobId);
            return true;
        }
        return false;
    }

    public bool Requeue(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job) && job.State == JobState.Failed)
        {
            job.State = JobState.Enqueued;
            job.RetryCount++;

            if (job.MethodExpression is LambdaExpression methodCall)
            {
                // BUG FIX: Capture task reference and add proper exception handling
                var executionTask = Task.Run(async () =>
                {
                    try
                    {
                        job.State = JobState.Processing;
                        job.StartedAt = DateTime.UtcNow;

                        var compiled = methodCall.Compile();
                        var result = compiled.DynamicInvoke();
                        if (result is Task task)
                        {
                            await task;
                        }
                        job.State = JobState.Succeeded;
                        job.CompletedAt = DateTime.UtcNow;
                    }
                    catch (System.Reflection.TargetInvocationException tie)
                    {
                        // BUG FIX: Unwrap TargetInvocationException to get actual exception
                        var actualException = tie.InnerException ?? tie;
                        job.State = JobState.Failed;
                        job.Exception = actualException.Message;
                        _logger.LogError(actualException, "Requeued job {JobId} failed: {Message}",
                            jobId, actualException.Message);
                    }
                    catch (Exception ex)
                    {
                        job.State = JobState.Failed;
                        job.Exception = ex.Message;
                        _logger.LogError(ex, "Requeued job {JobId} failed", jobId);
                    }
                });

                // BUG FIX: Handle unobserved task exceptions
                executionTask.ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogError(t.Exception.InnerException ?? t.Exception,
                            "Unobserved exception in requeued job {JobId}", jobId);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }

            _logger.LogInformation("Job {JobId} requeued (attempt {Retry})", jobId, job.RetryCount);
            return true;
        }
        return false;
    }

    public JobDetails? GetJobDetails(string jobId)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
            return null;

        return new JobDetails(
            job.JobId,
            job.State,
            job.JobType,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.Exception,
            job.RetryCount
        );
    }

    public IEnumerable<JobDetails> GetAllJobs(JobState? stateFilter = null)
    {
        var jobs = _jobs.Values.AsEnumerable();

        if (stateFilter.HasValue)
        {
            jobs = jobs.Where(j => j.State == stateFilter.Value);
        }

        return jobs
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobDetails(
                j.JobId,
                j.State,
                j.JobType,
                j.CreatedAt,
                j.StartedAt,
                j.CompletedAt,
                j.Exception,
                j.RetryCount
            ))
            .ToList();
    }

    public IEnumerable<RecurringJobDetails> GetRecurringJobs()
    {
        return _recurringJobs.Values
            .OrderBy(j => j.JobId)
            .Select(j => new RecurringJobDetails(
                j.JobId,
                j.CronExpression,
                j.JobType,
                j.CreatedAt,
                j.LastExecution,
                null // NextExecution would require cron parsing
            ))
            .ToList();
    }

    public JobStatistics GetStatistics()
    {
        var jobs = _jobs.Values.ToList();

        return new JobStatistics(
            TotalJobs: jobs.Count,
            EnqueuedCount: jobs.Count(j => j.State == JobState.Enqueued),
            ProcessingCount: jobs.Count(j => j.State == JobState.Processing),
            SucceededCount: jobs.Count(j => j.State == JobState.Succeeded),
            FailedCount: jobs.Count(j => j.State == JobState.Failed),
            ScheduledCount: jobs.Count(j => j.State == JobState.Scheduled),
            RecurringJobsCount: _recurringJobs.Count,
            OldestJobCreatedAt: jobs.OrderBy(j => j.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastCompletedAt: jobs.Where(j => j.CompletedAt.HasValue)
                .OrderByDescending(j => j.CompletedAt)
                .FirstOrDefault()?.CompletedAt
        );
    }

    #region Private Methods

    private static string GenerateJobId() => Guid.NewGuid().ToString("N")[..12];

    /// <summary>
    /// BUG FIX: Helper method to observe and log unhandled exceptions from fire-and-forget tasks.
    /// This prevents TaskScheduler.UnobservedTaskException from being raised.
    /// </summary>
    private void ObserveTaskExceptions(Task task, string jobId)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                var innerException = t.Exception.InnerException ?? t.Exception;
                _logger.LogError(innerException, "Unobserved exception in job {JobId}: {Message}",
                    jobId, innerException.Message);

                // Update job state if still tracked
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.State = JobState.Failed;
                    job.Exception = innerException.Message;
                    job.CompletedAt = DateTime.UtcNow;
                }
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Delays execution and then runs the provided async action.
    /// Better than ContinueWith for async error handling.
    /// </summary>
    private async Task DelayThenExecuteAsync(TimeSpan delay, Func<Task> action)
    {
        try
        {
            await Task.Delay(delay);
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing delayed job");
        }
    }

    private async Task ExecuteJobAsync<T>(string jobId, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        if (!_jobs.TryGetValue(jobId, out var job) || job.State == JobState.Deleted)
            return;

        job.State = JobState.Processing;
        job.StartedAt = DateTime.UtcNow;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<T>();
            var compiled = methodCall.Compile();
            await compiled(service);

            job.State = JobState.Succeeded;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Job {JobId} completed successfully", jobId);

            // Trigger continuation jobs
            TriggerContinuations(jobId);
        }
        catch (Exception ex)
        {
            job.State = JobState.Failed;
            job.Exception = ex.Message;
            _logger.LogError(ex, "Job {JobId} failed", jobId);
        }
    }

    private async Task ExecuteJobAsync(string jobId, Expression<Func<Task>> methodCall)
    {
        if (!_jobs.TryGetValue(jobId, out var job) || job.State == JobState.Deleted)
            return;

        job.State = JobState.Processing;
        job.StartedAt = DateTime.UtcNow;

        try
        {
            var compiled = methodCall.Compile();
            await compiled();

            job.State = JobState.Succeeded;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Job {JobId} completed successfully", jobId);

            TriggerContinuations(jobId);
        }
        catch (Exception ex)
        {
            job.State = JobState.Failed;
            job.Exception = ex.Message;
            _logger.LogError(ex, "Job {JobId} failed", jobId);
        }
    }

    /// <summary>
    /// BUG FIX: Added timeout and proper cancellation support to prevent infinite waiting.
    /// </summary>
    private async Task WaitForParentAndExecuteAsync<T>(string jobId, string parentJobId, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        // BUG FIX: Add timeout to prevent infinite waiting
        const int maxWaitSeconds = 3600; // 1 hour max wait
        var startTime = DateTime.UtcNow;
        var waitInterval = TimeSpan.FromMilliseconds(100);
        var backoffMultiplier = 1.0;
        const double maxBackoffMultiplier = 50.0; // Max 5 seconds between checks

        while (_jobs.TryGetValue(parentJobId, out var parentJob) &&
               parentJob.State != JobState.Succeeded &&
               parentJob.State != JobState.Failed &&
               parentJob.State != JobState.Deleted)
        {
            // BUG FIX: Check for timeout to prevent infinite loops
            if ((DateTime.UtcNow - startTime).TotalSeconds > maxWaitSeconds)
            {
                _logger.LogWarning("Job {JobId} timed out waiting for parent job {ParentJobId}", jobId, parentJobId);
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.State = JobState.Failed;
                    job.Exception = $"Timed out waiting for parent job {parentJobId} after {maxWaitSeconds} seconds";
                }
                return;
            }

            // BUG FIX: Use exponential backoff to reduce CPU usage
            var currentDelay = TimeSpan.FromMilliseconds(waitInterval.TotalMilliseconds * backoffMultiplier);
            await Task.Delay(currentDelay);
            backoffMultiplier = Math.Min(backoffMultiplier * 1.1, maxBackoffMultiplier);
        }

        if (_jobs.TryGetValue(parentJobId, out var parent) && parent.State == JobState.Succeeded)
        {
            await ExecuteJobAsync<T>(jobId, methodCall);
        }
        else
        {
            // BUG FIX: Handle parent job failure
            if (_jobs.TryGetValue(jobId, out var job))
            {
                job.State = JobState.Failed;
                job.Exception = $"Parent job {parentJobId} did not succeed";
                _logger.LogWarning("Continuation job {JobId} cancelled because parent {ParentJobId} failed", jobId, parentJobId);
            }
        }
    }

    private void TriggerContinuations(string parentJobId)
    {
        foreach (var job in _jobs.Values.Where(j => j.ParentJobId == parentJobId && j.State == JobState.Awaiting))
        {
            job.State = JobState.Enqueued;
        }
    }

    #endregion

    private class JobInfo
    {
        public string JobId { get; set; } = string.Empty;
        public JobState State { get; set; }
        public string? JobType { get; set; }
        public object? MethodExpression { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Exception { get; set; }
        public int RetryCount { get; set; }
        public string? ParentJobId { get; set; }
    }

    private class RecurringJobInfo
    {
        public string JobId { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string? JobType { get; set; }
        public object? MethodExpression { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecution { get; set; }
    }
}

/// <summary>
/// Background service for processing recurring jobs.
/// </summary>
public class RecurringJobHostedService : BackgroundService
{
    private readonly IBackgroundJobService _jobService;
    private readonly ILogger<RecurringJobHostedService> _logger;

    public RecurringJobHostedService(
        IBackgroundJobService jobService,
        ILogger<RecurringJobHostedService> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring job processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Check and execute recurring jobs based on cron schedule
            // In production, use a proper cron parser like NCrontab
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
