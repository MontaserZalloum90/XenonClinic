using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.BackgroundServices;

public class TenantHealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantHealthCheckService> _logger;
    private readonly TimeSpan _checkInterval;

    public TenantHealthCheckService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<TenantHealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var intervalMinutes = configuration.GetValue<int>("BackgroundServices:HealthCheckIntervalMinutes", 5);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tenant Health Check Service started");

        // Wait a bit before first run to allow app to fully start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAllTenantsHealth(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant health check");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Tenant Health Check Service stopped");
    }

    private async Task CheckAllTenantsHealth(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var healthService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

        _logger.LogInformation("Starting health check for all tenants");

        var results = await healthService.CheckAllTenantsHealthAsync();

        var healthy = results.Count(r => r.OverallStatus == HealthStatus.Healthy);
        var unhealthy = results.Count(r => r.OverallStatus == HealthStatus.Unhealthy);

        _logger.LogInformation(
            "Health check completed: {Total} tenants checked, {Healthy} healthy, {Unhealthy} unhealthy",
            results.Count(), healthy, unhealthy);

        // Log unhealthy tenants
        foreach (var result in results.Where(r => r.OverallStatus == HealthStatus.Unhealthy))
        {
            _logger.LogWarning(
                "Tenant {TenantId} is unhealthy: {Error}",
                result.TenantId, result.DatabaseError);
        }
    }
}
