using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for tenant usage tracking and reporting
/// </summary>
public interface IUsageService
{
    /// <summary>
    /// Get usage metrics for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="days">Number of days to retrieve</param>
    /// <returns>Usage metrics and history</returns>
    Task<Result<TenantUsageDto>> GetUsageAsync(Guid tenantId, int days = 30);

    /// <summary>
    /// Report a usage snapshot (called by ERP periodically)
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="request">Usage data to record</param>
    /// <returns>Created snapshot ID</returns>
    Task<Result<UsageReportResponse>> ReportUsageAsync(Guid tenantId, UsageReportRequest request);
}
