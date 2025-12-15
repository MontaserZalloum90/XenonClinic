namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for background job processing.
/// </summary>
public class JobProcessor : IJobProcessor
{
    private readonly WorkflowEngineDbContext _context;
    private readonly IServiceTaskExecutor _serviceTaskExecutor;
    private readonly ILogger<JobProcessor> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public JobProcessor(
        WorkflowEngineDbContext context,
        IServiceTaskExecutor serviceTaskExecutor,
        ILogger<JobProcessor> logger)
    {
        _context = context;
        _serviceTaskExecutor = serviceTaskExecutor;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AsyncJob> EnqueueAsync(
        EnqueueJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var activityInstanceId = !string.IsNullOrEmpty(request.ActivityInstanceId) && Guid.TryParse(request.ActivityInstanceId, out var aid)
            ? aid
            : (Guid?)null;

        var job = new AsyncJob
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ProcessInstanceId = request.ProcessInstanceId,
            ActivityInstanceId = activityInstanceId,
            JobType = request.JobType,
            Status = JobStatus.Pending,
            Priority = request.Priority,
            JobDataJson = request.Payload != null
                ? JsonSerializer.Serialize(request.Payload, _jsonOptions)
                : "{}",
            MaxRetries = request.MaxRetries,
            RetryDelaySeconds = (int)request.RetryDelay.TotalSeconds,
            CreatedAt = DateTime.UtcNow,
            NextRetryAt = request.ScheduledFor ?? DateTime.UtcNow
        };

        _context.AsyncJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Enqueued job {JobId} of type {JobType} for process {ProcessInstanceId}",
            job.Id, job.JobType, request.ProcessInstanceId);

        return job;
    }

    public async Task<IList<AsyncJob>> GetPendingJobsAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.AsyncJobs
            .Where(j => (j.Status == JobStatus.Pending || j.Status == JobStatus.Retrying) &&
                       j.NextRetryAt <= now &&
                       (j.LockExpiry == null || j.LockExpiry < now))
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.NextRetryAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryAcquireJobLockAsync(
        string jobId,
        string lockOwner,
        TimeSpan lockDuration,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
            return false;

        var now = DateTime.UtcNow;
        var lockExpiry = now.Add(lockDuration);

        // Use optimistic concurrency
        var affectedRows = await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE WF_AsyncJobs
              SET LockOwner = {0}, LockExpiry = {1}
              WHERE Id = {2}
                AND (LockExpiry IS NULL OR LockExpiry < {3})
                AND Status IN (0, 4)",
            lockOwner, lockExpiry, jobGuid, now);

        return affectedRows > 0;
    }

    public async Task ProcessJobAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
        {
            _logger.LogWarning("Invalid job ID format: {JobId}", jobId);
            return;
        }

        var job = await _context.AsyncJobs
            .FirstOrDefaultAsync(j => j.Id == jobGuid, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Job {JobId} not found for processing", jobId);
            return;
        }

        var now = DateTime.UtcNow;
        job.Status = JobStatus.Processing;
        job.StartedAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            Dictionary<string, object>? payload;
            if (!string.IsNullOrEmpty(job.JobDataJson))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<Dictionary<string, object>>(job.JobDataJson, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize job payload for {JobId}", jobId);
                    payload = new Dictionary<string, object>();
                }
            }
            else
            {
                payload = new Dictionary<string, object>();
            }

            object? result = null;

            switch (job.JobType)
            {
                case JobTypes.ServiceTask:
                case JobTypes.HttpCall:
                    result = await _serviceTaskExecutor.ExecuteAsync(
                        job.ProcessInstanceId,
                        job.ActivityInstanceId,
                        payload ?? new Dictionary<string, object>(),
                        cancellationToken);
                    break;

                case JobTypes.ScriptTask:
                    // Script execution handled by ProcessExecutionService
                    break;

                case JobTypes.SendEmail:
                    // Would integrate with email service
                    _logger.LogInformation("Email job {JobId} - would send email", jobId);
                    break;

                case JobTypes.SendMessage:
                    // Would integrate with message broker
                    _logger.LogInformation("Message job {JobId} - would send message", jobId);
                    break;

                case JobTypes.RetryActivity:
                    if (job.ProcessInstanceId.HasValue && !string.IsNullOrEmpty(job.ActivityInstanceId))
                    {
                        var instance = await _context.ProcessInstances
                            .FirstOrDefaultAsync(i => i.Id == job.ProcessInstanceId, cancellationToken);

                        if (instance != null)
                        {
                            // Trigger activity retry through execution service
                            _logger.LogInformation(
                                "Retrying activity {ActivityInstanceId} for process {ProcessInstanceId}",
                                job.ActivityInstanceId, job.ProcessInstanceId);
                        }
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown job type: {JobType}", job.JobType);
                    break;
            }

            await CompleteJobAsync(jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job {JobId}", jobId);
            await FailJobAsync(jobId, ex.Message, shouldRetry: true, cancellationToken);
        }
    }

    public async Task CompleteJobAsync(
        string jobId,
        string? result = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
            return;

        var job = await _context.AsyncJobs
            .FirstOrDefaultAsync(j => j.Id == jobGuid, cancellationToken);

        if (job == null)
            return;

        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.LockOwner = null;
        job.LockExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Job {JobId} completed successfully", jobId);
    }

    public async Task FailJobAsync(
        string jobId,
        string error,
        bool shouldRetry = true,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
            return;

        var job = await _context.AsyncJobs
            .FirstOrDefaultAsync(j => j.Id == jobGuid, cancellationToken);

        if (job == null)
            return;

        job.RetryCount++;
        job.ErrorMessage = error;
        job.LockOwner = null;
        job.LockExpiry = null;

        if (shouldRetry && job.RetryCount < job.MaxRetries)
        {
            // Schedule retry with exponential backoff, capped to prevent overflow
            var exponent = Math.Min(job.RetryCount - 1, 10); // Cap at 2^10 = 1024
            var multiplier = Math.Pow(2, exponent);
            var delaySeconds = Math.Min(job.RetryDelaySeconds * multiplier, TimeSpan.MaxValue.TotalSeconds - 1);
            var delay = TimeSpan.FromSeconds(delaySeconds);

            job.Status = JobStatus.Retrying;
            job.NextRetryAt = DateTime.UtcNow.Add(delay);

            _logger.LogWarning(
                "Job {JobId} failed, scheduling retry {RetryCount}/{MaxRetries} at {NextRetryAt}",
                jobId, job.RetryCount, job.MaxRetries, job.NextRetryAt);
        }
        else
        {
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(
                "Job {JobId} failed permanently after {RetryCount} retries: {Error}",
                jobId, job.RetryCount, error);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobDto?> GetJobAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
            return null;

        var job = await _context.AsyncJobs
            .FirstOrDefaultAsync(j => j.Id == jobGuid, cancellationToken);

        if (job == null)
            return null;

        return MapToDto(job);
    }

    public async Task<IList<JobDto>> GetProcessJobsAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _context.AsyncJobs
            .Where(j => j.ProcessInstanceId == processInstanceId && j.TenantId == tenantId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

        return jobs.Select(MapToDto).ToList();
    }

    public async Task CancelJobAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(jobId, out var jobGuid))
            return;

        var job = await _context.AsyncJobs
            .FirstOrDefaultAsync(j => j.Id == jobGuid, cancellationToken);

        if (job == null)
            return;

        if (job.Status == JobStatus.Processing)
        {
            throw new InvalidOperationException("Cannot cancel a processing job.");
        }

        if (job.Status == JobStatus.Completed || job.Status == JobStatus.Failed)
        {
            return;
        }

        // Move to DeadLetter status instead of Cancelled since that doesn't exist
        job.Status = JobStatus.DeadLetter;
        job.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Job {JobId} cancelled", jobId);
    }

    public async Task CleanupOldJobsAsync(
        TimeSpan olderThan,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);

        var deletedCount = await _context.AsyncJobs
            .Where(j => (j.Status == JobStatus.Completed || j.Status == JobStatus.Failed || j.Status == JobStatus.DeadLetter) &&
                       j.CompletedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} old jobs", deletedCount);
    }

    private JobDto MapToDto(AsyncJob job)
    {
        Dictionary<string, object>? payload = null;
        if (!string.IsNullOrEmpty(job.JobDataJson))
        {
            try
            {
                payload = JsonSerializer.Deserialize<Dictionary<string, object>>(job.JobDataJson, _jsonOptions);
            }
            catch (JsonException)
            {
                // Keep payload as null if deserialization fails
            }
        }

        return new JobDto
        {
            Id = job.Id.ToString(),
            TenantId = job.TenantId,
            ProcessInstanceId = job.ProcessInstanceId,
            ActivityInstanceId = job.ActivityInstanceId?.ToString(),
            JobType = job.JobType,
            Status = job.Status,
            Priority = job.Priority,
            Payload = payload,
            Result = null, // ResultJson doesn't exist in AsyncJob
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            MaxRetries = job.MaxRetries,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            NextRetryAt = job.NextRetryAt
        };
    }
}
