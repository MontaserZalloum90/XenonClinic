namespace XenonClinic.WorkflowEngine.Infrastructure.BackgroundServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Background service that processes workflow timers and jobs.
/// </summary>
public class WorkflowBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowBackgroundService> _logger;
    private readonly TimeSpan _timerPollInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _jobPollInterval = TimeSpan.FromSeconds(5);
    private readonly string _workerId;

    public WorkflowBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<WorkflowBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _workerId = $"worker-{Environment.MachineName}-{Guid.NewGuid():N}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Workflow background service starting with worker ID {WorkerId}",
            _workerId);

        // Start timer processing and job processing in parallel
        var timerTask = ProcessTimersAsync(stoppingToken);
        var jobTask = ProcessJobsAsync(stoppingToken);

        await Task.WhenAll(timerTask, jobTask);
    }

    private async Task ProcessTimersAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var timerService = scope.ServiceProvider.GetRequiredService<ITimerService>();

                var dueTimers = await timerService.GetDueTimersAsync(
                    DateTime.UtcNow,
                    batchSize: 50,
                    stoppingToken);

                foreach (var timer in dueTimers)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await timerService.ProcessTimerAsync(timer.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing timer {TimerId}", timer.Id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in timer processing loop");
            }

            try
            {
                await Task.Delay(_timerPollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Timer processing stopped");
    }

    private async Task ProcessJobsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var jobProcessor = scope.ServiceProvider.GetRequiredService<IJobProcessor>();

                var pendingJobs = await jobProcessor.GetPendingJobsAsync(
                    batchSize: 20,
                    stoppingToken);

                foreach (var job in pendingJobs)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Try to acquire lock
                        var acquired = await jobProcessor.TryAcquireJobLockAsync(
                            job.Id,
                            _workerId,
                            TimeSpan.FromMinutes(5),
                            stoppingToken);

                        if (acquired)
                        {
                            await jobProcessor.ProcessJobAsync(job.Id, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing job {JobId}", job.Id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job processing loop");
            }

            try
            {
                await Task.Delay(_jobPollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Job processing stopped");
    }
}

/// <summary>
/// Background service for cleanup tasks.
/// </summary>
public class WorkflowCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public WorkflowCleanupService(
        IServiceProvider serviceProvider,
        ILogger<WorkflowCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow cleanup service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var jobProcessor = scope.ServiceProvider.GetRequiredService<IJobProcessor>();

                // Clean up old completed/failed jobs (older than 7 days)
                await jobProcessor.CleanupOldJobsAsync(
                    TimeSpan.FromDays(7),
                    stoppingToken);

                _logger.LogInformation("Cleanup cycle completed");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup cycle");
            }
        }

        _logger.LogInformation("Workflow cleanup service stopped");
    }
}
