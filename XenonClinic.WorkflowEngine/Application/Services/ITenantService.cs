using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Multi-tenancy service for managing tenant configurations and isolation.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant context.
    /// </summary>
    TenantContext GetCurrentTenant();

    /// <summary>
    /// Sets the current tenant context.
    /// </summary>
    void SetCurrentTenant(string tenantId);

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    Task<TenantInfo> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tenant configuration.
    /// </summary>
    Task<TenantInfo> UpdateTenantAsync(string tenantId, UpdateTenantRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a tenant.
    /// </summary>
    Task DeactivateTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates a tenant.
    /// </summary>
    Task ReactivateTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all tenants.
    /// </summary>
    Task<IList<TenantInfo>> ListTenantsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tenant usage statistics.
    /// </summary>
    Task<TenantUsageStats> GetUsageStatsAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if an operation is allowed for the current tenant.
    /// </summary>
    Task<bool> ValidateOperationAsync(string operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Accessor for the current tenant context (similar to IHttpContextAccessor pattern).
/// </summary>
public interface ITenantContextAccessor
{
    TenantContext? TenantContext { get; set; }
}

#region Tenant DTOs

public class TenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public TenantSettings Settings { get; set; } = new();
    public TenantLimits Limits { get; set; } = new();
    public Dictionary<string, string> Claims { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class TenantInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public TenantType Type { get; set; } = TenantType.Standard;
    public TenantSettings Settings { get; set; } = new();
    public TenantLimits Limits { get; set; } = new();
    public TenantBranding? Branding { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeactivatedAt { get; set; }
    public string? ConnectionString { get; set; } // For tenant-specific database
    public string? DataRegion { get; set; } // For data residency requirements
}

public enum TenantType
{
    Standard,
    Enterprise,
    Trial,
    Internal
}

public class TenantSettings
{
    // Workflow Settings
    public bool AllowExternalConnectors { get; set; } = true;
    public bool AllowWebhooks { get; set; } = true;
    public bool AllowEmailNotifications { get; set; } = true;
    public bool AllowDocumentGeneration { get; set; } = true;
    public bool AllowBpmnImport { get; set; } = true;

    // Process Execution Settings
    public int DefaultTaskTimeoutMinutes { get; set; } = 1440; // 24 hours
    public int DefaultProcessTimeoutMinutes { get; set; } = 43200; // 30 days
    public int MaxConcurrentProcesses { get; set; } = 100;
    public int MaxConcurrentTasks { get; set; } = 1000;
    public int DefaultTaskPriority { get; set; } = 50;

    // Retry Settings
    public int DefaultMaxRetries { get; set; } = 3;
    public int DefaultRetryDelaySeconds { get; set; } = 60;
    public RetryBackoffStrategy RetryBackoffStrategy { get; set; } = RetryBackoffStrategy.Exponential;

    // Audit Settings
    public bool EnableAuditLogging { get; set; } = true;
    public int AuditRetentionDays { get; set; } = 90;
    public bool AuditVariableChanges { get; set; } = true;

    // Security Settings
    public bool RequireTaskClaimBeforeComplete { get; set; } = false;
    public bool AllowTaskReassignment { get; set; } = true;
    public bool AllowProcessCancellation { get; set; } = true;
    public List<string> AllowedIpRanges { get; set; } = new();

    // Integration Settings
    public int WebhookTimeoutSeconds { get; set; } = 30;
    public int ExternalServiceTimeoutSeconds { get; set; } = 60;
    public bool ValidateWebhookSignatures { get; set; } = true;
}

public enum RetryBackoffStrategy
{
    Fixed,
    Linear,
    Exponential
}

public class TenantLimits
{
    public int MaxProcessDefinitions { get; set; } = 100;
    public int MaxActiveProcessInstances { get; set; } = 10000;
    public int MaxTasksPerProcess { get; set; } = 1000;
    public int MaxVariablesPerProcess { get; set; } = 500;
    public int MaxVariableSizeKb { get; set; } = 1024;
    public int MaxWebhookSubscriptions { get; set; } = 50;
    public int MaxDocumentTemplates { get; set; } = 100;
    public int MaxDocumentSizeMb { get; set; } = 50;
    public int MaxExternalConnectors { get; set; } = 20;
    public int MaxEmailsPerDay { get; set; } = 10000;
    public int MaxApiCallsPerMinute { get; set; } = 1000;
    public long MaxStorageMb { get; set; } = 10240; // 10 GB
}

public class TenantBranding
{
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? EmailFooterHtml { get; set; }
    public string? CustomCss { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public TenantType Type { get; set; } = TenantType.Standard;
    public TenantSettings? Settings { get; set; }
    public TenantLimits? Limits { get; set; }
    public TenantBranding? Branding { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? ConnectionString { get; set; }
    public string? DataRegion { get; set; }
}

public class UpdateTenantRequest
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public TenantType? Type { get; set; }
    public TenantSettings? Settings { get; set; }
    public TenantLimits? Limits { get; set; }
    public TenantBranding? Branding { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class TenantUsageStats
{
    public string TenantId { get; set; } = string.Empty;
    public DateTime AsOf { get; set; } = DateTime.UtcNow;

    // Process Statistics
    public int ProcessDefinitionCount { get; set; }
    public int ActiveProcessInstanceCount { get; set; }
    public int CompletedProcessInstanceCount { get; set; }
    public int FailedProcessInstanceCount { get; set; }

    // Task Statistics
    public int PendingTaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public double AverageTaskDurationMinutes { get; set; }

    // Integration Statistics
    public int WebhookSubscriptionCount { get; set; }
    public int WebhookDeliveriesLast24Hours { get; set; }
    public int ExternalConnectorCount { get; set; }
    public int EmailsSentLast24Hours { get; set; }

    // Resource Usage
    public long StorageUsedMb { get; set; }
    public int DocumentTemplateCount { get; set; }
    public int ApiCallsLast24Hours { get; set; }

    // Limits Utilization
    public Dictionary<string, LimitUtilization> LimitUtilizations { get; set; } = new();
}

public class LimitUtilization
{
    public string LimitName { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
    public long MaxValue { get; set; }
    public double UtilizationPercent => MaxValue > 0 ? (CurrentValue * 100.0 / MaxValue) : 0;
    public bool IsNearLimit => UtilizationPercent >= 80;
    public bool IsAtLimit => UtilizationPercent >= 100;
}

#endregion

#region Tenant Resolution

/// <summary>
/// Strategy for resolving the current tenant.
/// </summary>
public interface ITenantResolutionStrategy
{
    Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default);
    int Priority { get; }
}

public class TenantResolutionContext
{
    public string? RequestPath { get; set; }
    public string? Host { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public Dictionary<string, object> Items { get; set; } = new();
}

/// <summary>
/// Resolves tenant from HTTP header (X-Tenant-Id).
/// </summary>
public class HeaderTenantResolutionStrategy : ITenantResolutionStrategy
{
    public int Priority => 100;
    public const string HeaderName = "X-Tenant-Id";

    public Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        context.Headers.TryGetValue(HeaderName, out var tenantId);
        return Task.FromResult(tenantId);
    }
}

/// <summary>
/// Resolves tenant from subdomain (tenant1.app.com).
/// </summary>
public class SubdomainTenantResolutionStrategy : ITenantResolutionStrategy
{
    public int Priority => 90;

    public Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(context.Host))
            return Task.FromResult<string?>(null);

        var parts = context.Host.Split('.');
        if (parts.Length >= 3)
        {
            var subdomain = parts[0];
            if (subdomain != "www" && subdomain != "api")
            {
                return Task.FromResult<string?>(subdomain);
            }
        }

        return Task.FromResult<string?>(null);
    }
}

/// <summary>
/// Resolves tenant from URL path (/tenants/{tenantId}/...).
/// </summary>
public class PathTenantResolutionStrategy : ITenantResolutionStrategy
{
    public int Priority => 80;

    public Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(context.RequestPath))
            return Task.FromResult<string?>(null);

        var parts = context.RequestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && parts[0].Equals("tenants", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<string?>(parts[1]);
        }

        return Task.FromResult<string?>(null);
    }
}

/// <summary>
/// Resolves tenant from query parameter (?tenant=xxx).
/// </summary>
public class QueryParamTenantResolutionStrategy : ITenantResolutionStrategy
{
    public int Priority => 70;
    public const string ParamName = "tenant";

    public Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        context.QueryParams.TryGetValue(ParamName, out var tenantId);
        return Task.FromResult(tenantId);
    }
}

#endregion
