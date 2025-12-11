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

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        // Periodically cleanup old jobs to prevent memory leaks
        CleanupOldJobs();

        var jobId = GenerateJobId();
        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Enqueued,
            JobType = typeof(T).FullName,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;
        _ = ExecuteJobAsync<T>(jobId, methodCall);

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
        _ = ExecuteJobAsync(jobId, methodCall);

        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var jobId = GenerateJobId();
        var delay = enqueueAt - DateTimeOffset.UtcNow;

        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Scheduled,
            JobType = typeof(T).FullName,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        if (delay > TimeSpan.Zero)
        {
            // Use async/await pattern instead of ContinueWith for better error handling
            _ = DelayThenExecuteAsync(delay, () => ExecuteJobAsync<T>(jobId, methodCall));
        }
        else
        {
            _ = ExecuteJobAsync<T>(jobId, methodCall);
        }

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

        if (delay > TimeSpan.Zero)
        {
            // Use async/await pattern instead of ContinueWith for better error handling
            _ = DelayThenExecuteAsync(delay, () => ExecuteJobAsync(jobId, methodCall));
        }
        else
        {
            _ = ExecuteJobAsync(jobId, methodCall);
        }

        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return Schedule(methodCall, DateTimeOffset.UtcNow.Add(delay));
    }

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        var recurringJob = new RecurringJobInfo
        {
            JobId = jobId,
            CronExpression = cronExpression,
            JobType = typeof(T).FullName,
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
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = compiled.DynamicInvoke();
                        if (result is Task task)
                        {
                            await task;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Recurring job {JobId} failed", jobId);
                    }
                });
            }
        }
    }

    public string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        var jobId = GenerateJobId();
        var job = new JobInfo
        {
            JobId = jobId,
            State = JobState.Awaiting,
            ParentJobId = parentJobId,
            JobType = typeof(T).FullName,
            MethodExpression = methodCall,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // Monitor parent job
        _ = WaitForParentAndExecuteAsync<T>(jobId, parentJobId, methodCall);

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
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var compiled = methodCall.Compile();
                        var result = compiled.DynamicInvoke();
                        if (result is Task task)
                        {
                            await task;
                        }
                        job.State = JobState.Succeeded;
                        job.CompletedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        job.State = JobState.Failed;
                        job.Exception = ex.Message;
                    }
                });
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

    #region Private Methods

    private static string GenerateJobId() => Guid.NewGuid().ToString("N")[..12];

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

    private async Task ExecuteJobAsync<T>(string jobId, Expression<Func<T, Task>> methodCall)
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

    private async Task WaitForParentAndExecuteAsync<T>(string jobId, string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        while (_jobs.TryGetValue(parentJobId, out var parentJob) &&
               parentJob.State != JobState.Succeeded &&
               parentJob.State != JobState.Failed &&
               parentJob.State != JobState.Deleted)
        {
            await Task.Delay(100);
        }

        if (_jobs.TryGetValue(parentJobId, out var parent) && parent.State == JobState.Succeeded)
        {
            await ExecuteJobAsync<T>(jobId, methodCall);
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
