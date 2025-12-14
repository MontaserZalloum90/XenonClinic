using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up expired security tokens
/// and old security events to maintain database performance.
/// </summary>
public class SecurityCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SecurityCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;

    public SecurityCleanupService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<SecurityCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var configValue = configuration["BackgroundServices:SecurityCleanupIntervalHours"];
        var intervalHours = int.TryParse(configValue, out var parsed) ? parsed : 6;
        _cleanupInterval = TimeSpan.FromHours(intervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Security Cleanup Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Security Cleanup Service stopping");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var passwordResetService = scope.ServiceProvider.GetRequiredService<IPasswordResetService>();

        _logger.LogInformation("Starting security cleanup tasks");

        // Cleanup expired refresh tokens
        await refreshTokenService.CleanupExpiredTokensAsync();

        // Cleanup expired password reset tokens
        await passwordResetService.CleanupExpiredTokensAsync();

        _logger.LogInformation("Security cleanup tasks completed");
    }
}
