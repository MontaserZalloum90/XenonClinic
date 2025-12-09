using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.BackgroundServices;

public class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryService> _logger;
    private readonly TimeSpan _checkInterval;

    public SubscriptionExpiryService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<SubscriptionExpiryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var intervalHours = configuration.GetValue<int>("BackgroundServices:SubscriptionCheckIntervalHours", 24);
        _checkInterval = TimeSpan.FromHours(intervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Service started");

        // Wait before first run
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredTrials(stoppingToken);
                await ProcessExpiredSubscriptions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during subscription expiry check");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Subscription Expiry Service stopped");
    }

    private async Task ProcessExpiredTrials(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var now = DateTime.UtcNow;

        // Find tenants with expired trials
        var expiredTrials = await context.Tenants
            .Where(t => t.Status == TenantStatus.Trial && t.TrialEndDate < now)
            .ToListAsync(cancellationToken);

        foreach (var tenant in expiredTrials)
        {
            tenant.Status = TenantStatus.Expired;
            tenant.UpdatedAt = now;

            await auditService.LogAsync("TrialExpired", "Tenant", tenant.Id,
                oldValues: new { status = TenantStatus.Trial.ToString() },
                newValues: new { status = TenantStatus.Expired.ToString() },
                tenantId: tenant.Id);

            _logger.LogInformation("Tenant {TenantId} ({TenantSlug}) trial expired",
                tenant.Id, tenant.Slug);
        }

        if (expiredTrials.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} expired trials", expiredTrials.Count);
        }
    }

    private async Task ProcessExpiredSubscriptions(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var now = DateTime.UtcNow;

        // Find expired subscriptions
        var expiredSubscriptions = await context.Subscriptions
            .Include(s => s.Tenant)
            .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate < now && !s.AutoRenew)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.Status = SubscriptionStatus.Expired;
            subscription.Tenant.Status = TenantStatus.Expired;
            subscription.Tenant.UpdatedAt = now;

            await auditService.LogAsync("SubscriptionExpired", "Subscription", subscription.Id,
                oldValues: new { status = SubscriptionStatus.Active.ToString() },
                newValues: new { status = SubscriptionStatus.Expired.ToString() },
                tenantId: subscription.TenantId);

            _logger.LogInformation("Subscription {SubscriptionId} for tenant {TenantSlug} expired",
                subscription.Id, subscription.Tenant.Slug);
        }

        if (expiredSubscriptions.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} expired subscriptions", expiredSubscriptions.Count);
        }
    }
}
