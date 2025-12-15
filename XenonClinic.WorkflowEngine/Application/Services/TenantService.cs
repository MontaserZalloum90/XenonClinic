using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Multi-tenancy service implementation.
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IEnumerable<ITenantResolutionStrategy> _resolutionStrategies;
    private readonly ILogger<TenantService> _logger;

    // In-memory tenant store - replace with database in production
    private static readonly ConcurrentDictionary<string, TenantInfo> _tenants = new();

    public TenantService(
        ITenantContextAccessor tenantContextAccessor,
        IEnumerable<ITenantResolutionStrategy> resolutionStrategies,
        ILogger<TenantService> logger)
    {
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Order resolution strategies by priority (handle null gracefully)
        _resolutionStrategies = (resolutionStrategies ?? Enumerable.Empty<ITenantResolutionStrategy>())
            .OrderByDescending(s => s.Priority)
            .ToList(); // Materialize to avoid multiple enumeration

        // Register default tenant
        EnsureDefaultTenant();
    }

    public TenantContext GetCurrentTenant()
    {
        var context = _tenantContextAccessor.TenantContext;
        if (context == null)
        {
            // Return default tenant context
            return new TenantContext
            {
                TenantId = "default",
                TenantName = "Default Tenant",
                IsActive = true
            };
        }
        return context;
    }

    public void SetCurrentTenant(string tenantId)
    {
        if (_tenants.TryGetValue(tenantId, out var tenant) && tenant.IsActive)
        {
            _tenantContextAccessor.TenantContext = new TenantContext
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                Settings = tenant.Settings,
                Limits = tenant.Limits,
                IsActive = tenant.IsActive
            };

            _logger.LogDebug("Set current tenant to {TenantId}", tenantId);
        }
        else
        {
            _logger.LogWarning("Attempted to set invalid or inactive tenant: {TenantId}", tenantId);
            throw new InvalidOperationException($"Tenant '{tenantId}' not found or is inactive");
        }
    }

    public Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        _tenants.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<TenantInfo> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Tenant name is required", nameof(request));
        }

        // Check for duplicate name
        if (_tenants.Values.Any(t => t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Tenant with name '{request.Name}' already exists");
        }

        var tenant = new TenantInfo
        {
            Id = GenerateTenantId(request.Name),
            Name = request.Name,
            DisplayName = request.DisplayName ?? request.Name,
            Description = request.Description,
            Type = request.Type,
            Settings = request.Settings ?? GetDefaultSettings(request.Type),
            Limits = request.Limits ?? GetDefaultLimits(request.Type),
            Branding = request.Branding,
            Metadata = request.Metadata ?? new Dictionary<string, string>(),
            ConnectionString = request.ConnectionString,
            DataRegion = request.DataRegion,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _tenants[tenant.Id] = tenant;

        _logger.LogInformation("Created tenant {TenantId} ({TenantName})", tenant.Id, tenant.Name);

        return Task.FromResult(tenant);
    }

    public Task<TenantInfo> UpdateTenantAsync(string tenantId, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var tenant))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        if (request.DisplayName != null) tenant.DisplayName = request.DisplayName;
        if (request.Description != null) tenant.Description = request.Description;
        if (request.Type.HasValue) tenant.Type = request.Type.Value;
        if (request.Settings != null) tenant.Settings = request.Settings;
        if (request.Limits != null) tenant.Limits = request.Limits;
        if (request.Branding != null) tenant.Branding = request.Branding;
        if (request.Metadata != null) tenant.Metadata = request.Metadata;

        _logger.LogInformation("Updated tenant {TenantId}", tenantId);

        return Task.FromResult(tenant);
    }

    public Task DeactivateTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var tenant))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        if (tenantId == "default")
        {
            throw new InvalidOperationException("Cannot deactivate the default tenant");
        }

        tenant.IsActive = false;
        tenant.DeactivatedAt = DateTime.UtcNow;

        _logger.LogInformation("Deactivated tenant {TenantId}", tenantId);

        return Task.CompletedTask;
    }

    public Task ReactivateTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var tenant))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        tenant.IsActive = true;
        tenant.DeactivatedAt = null;

        _logger.LogInformation("Reactivated tenant {TenantId}", tenantId);

        return Task.CompletedTask;
    }

    public Task<IList<TenantInfo>> ListTenantsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var tenants = _tenants.Values
            .Where(t => includeInactive || t.IsActive)
            .OrderBy(t => t.Name)
            .ToList();

        return Task.FromResult<IList<TenantInfo>>(tenants);
    }

    public Task<TenantUsageStats> GetUsageStatsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var tenant))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        // In production, these would be calculated from actual data
        var stats = new TenantUsageStats
        {
            TenantId = tenantId,
            AsOf = DateTime.UtcNow,
            ProcessDefinitionCount = 10,
            ActiveProcessInstanceCount = 100,
            CompletedProcessInstanceCount = 500,
            FailedProcessInstanceCount = 5,
            PendingTaskCount = 25,
            CompletedTaskCount = 475,
            AverageTaskDurationMinutes = 30.5,
            WebhookSubscriptionCount = 5,
            WebhookDeliveriesLast24Hours = 150,
            ExternalConnectorCount = 3,
            EmailsSentLast24Hours = 50,
            StorageUsedMb = 512,
            DocumentTemplateCount = 8,
            ApiCallsLast24Hours = 5000,
            LimitUtilizations = new Dictionary<string, LimitUtilization>
            {
                ["ProcessDefinitions"] = new LimitUtilization
                {
                    LimitName = "Process Definitions",
                    CurrentValue = 10,
                    MaxValue = tenant.Limits.MaxProcessDefinitions
                },
                ["ActiveProcessInstances"] = new LimitUtilization
                {
                    LimitName = "Active Process Instances",
                    CurrentValue = 100,
                    MaxValue = tenant.Limits.MaxActiveProcessInstances
                },
                ["Storage"] = new LimitUtilization
                {
                    LimitName = "Storage (MB)",
                    CurrentValue = 512,
                    MaxValue = tenant.Limits.MaxStorageMb
                }
            }
        };

        return Task.FromResult(stats);
    }

    public async Task<bool> ValidateOperationAsync(string operation, CancellationToken cancellationToken = default)
    {
        var tenant = GetCurrentTenant();
        if (!tenant.IsActive)
        {
            _logger.LogWarning("Operation {Operation} denied for inactive tenant {TenantId}", operation, tenant.TenantId);
            return false;
        }

        var settings = tenant.Settings;

        // Check operation-specific permissions
        var isAllowed = operation switch
        {
            "CreateExternalConnector" => settings.AllowExternalConnectors,
            "CreateWebhook" => settings.AllowWebhooks,
            "SendEmail" => settings.AllowEmailNotifications,
            "GenerateDocument" => settings.AllowDocumentGeneration,
            "ImportBpmn" => settings.AllowBpmnImport,
            "CancelProcess" => settings.AllowProcessCancellation,
            "ReassignTask" => settings.AllowTaskReassignment,
            _ => true
        };

        if (!isAllowed)
        {
            _logger.LogWarning("Operation {Operation} not allowed for tenant {TenantId}", operation, tenant.TenantId);
        }

        return isAllowed;
    }

    public Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == "default")
        {
            throw new InvalidOperationException("Cannot delete the default tenant");
        }

        if (!_tenants.TryRemove(tenantId, out _))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        _logger.LogInformation("Deleted tenant {TenantId}", tenantId);

        return Task.CompletedTask;
    }

    public Task SuspendTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // Alias for DeactivateTenantAsync
        return DeactivateTenantAsync(tenantId, cancellationToken);
    }

    public Task ActivateTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // Alias for ReactivateTenantAsync
        return ReactivateTenantAsync(tenantId, cancellationToken);
    }

    public async Task<TenantUsage> GetTenantUsageAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // TenantUsage inherits from TenantUsageStats, so get stats and return as TenantUsage
        var stats = await GetUsageStatsAsync(tenantId, cancellationToken);
        return new TenantUsage
        {
            TenantId = stats.TenantId,
            AsOf = stats.AsOf,
            ProcessDefinitionCount = stats.ProcessDefinitionCount,
            ActiveProcessInstanceCount = stats.ActiveProcessInstanceCount,
            CompletedProcessInstanceCount = stats.CompletedProcessInstanceCount,
            FailedProcessInstanceCount = stats.FailedProcessInstanceCount,
            PendingTaskCount = stats.PendingTaskCount,
            CompletedTaskCount = stats.CompletedTaskCount,
            AverageTaskDurationMinutes = stats.AverageTaskDurationMinutes,
            WebhookSubscriptionCount = stats.WebhookSubscriptionCount,
            WebhookDeliveriesLast24Hours = stats.WebhookDeliveriesLast24Hours,
            ExternalConnectorCount = stats.ExternalConnectorCount,
            EmailsSentLast24Hours = stats.EmailsSentLast24Hours,
            StorageUsedMb = stats.StorageUsedMb,
            DocumentTemplateCount = stats.DocumentTemplateCount,
            ApiCallsLast24Hours = stats.ApiCallsLast24Hours,
            LimitUtilizations = stats.LimitUtilizations
        };
    }

    public Task<IList<TenantInfo>> ListTenantsAsync(CancellationToken cancellationToken = default)
    {
        // Overload that defaults to excluding inactive tenants
        return ListTenantsAsync(includeInactive: false, cancellationToken);
    }

    /// <summary>
    /// Resolves tenant ID from request context using registered strategies.
    /// </summary>
    public async Task<string?> ResolveTenantIdAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        foreach (var strategy in _resolutionStrategies)
        {
            var tenantId = await strategy.ResolveTenantIdAsync(context, cancellationToken);
            if (!string.IsNullOrEmpty(tenantId))
            {
                _logger.LogDebug("Resolved tenant {TenantId} using {Strategy}",
                    tenantId, strategy.GetType().Name);
                return tenantId;
            }
        }

        return null;
    }

    #region Private Methods

    private void EnsureDefaultTenant()
    {
        if (!_tenants.ContainsKey("default"))
        {
            _tenants["default"] = new TenantInfo
            {
                Id = "default",
                Name = "Default",
                DisplayName = "Default Tenant",
                Description = "System default tenant",
                Type = TenantType.Internal,
                Settings = GetDefaultSettings(TenantType.Enterprise), // Full features for default
                Limits = GetDefaultLimits(TenantType.Enterprise),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    private static string GenerateTenantId(string name)
    {
        // Create URL-safe ID from name
        var id = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove non-alphanumeric characters except hyphens
        id = new string(id.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Ensure uniqueness with timestamp
        return $"{id}-{DateTime.UtcNow.Ticks % 10000}";
    }

    private static TenantSettings GetDefaultSettings(TenantType type)
    {
        return type switch
        {
            TenantType.Enterprise => new TenantSettings
            {
                AllowExternalConnectors = true,
                AllowWebhooks = true,
                AllowEmailNotifications = true,
                AllowDocumentGeneration = true,
                AllowBpmnImport = true,
                MaxConcurrentProcesses = 500,
                MaxConcurrentTasks = 5000,
                EnableAuditLogging = true,
                AuditRetentionDays = 365,
                AuditVariableChanges = true
            },
            TenantType.Trial => new TenantSettings
            {
                AllowExternalConnectors = false,
                AllowWebhooks = true,
                AllowEmailNotifications = true,
                AllowDocumentGeneration = true,
                AllowBpmnImport = true,
                MaxConcurrentProcesses = 10,
                MaxConcurrentTasks = 100,
                EnableAuditLogging = true,
                AuditRetentionDays = 30
            },
            _ => new TenantSettings()
        };
    }

    private static TenantLimits GetDefaultLimits(TenantType type)
    {
        return type switch
        {
            TenantType.Enterprise => new TenantLimits
            {
                MaxProcessDefinitions = 500,
                MaxActiveProcessInstances = 50000,
                MaxTasksPerProcess = 5000,
                MaxVariablesPerProcess = 1000,
                MaxVariableSizeKb = 10240,
                MaxWebhookSubscriptions = 200,
                MaxDocumentTemplates = 500,
                MaxDocumentSizeMb = 100,
                MaxExternalConnectors = 100,
                MaxEmailsPerDay = 100000,
                MaxApiCallsPerMinute = 10000,
                MaxStorageMb = 102400 // 100 GB
            },
            TenantType.Trial => new TenantLimits
            {
                MaxProcessDefinitions = 5,
                MaxActiveProcessInstances = 100,
                MaxTasksPerProcess = 100,
                MaxVariablesPerProcess = 50,
                MaxVariableSizeKb = 256,
                MaxWebhookSubscriptions = 5,
                MaxDocumentTemplates = 10,
                MaxDocumentSizeMb = 10,
                MaxExternalConnectors = 2,
                MaxEmailsPerDay = 100,
                MaxApiCallsPerMinute = 100,
                MaxStorageMb = 1024 // 1 GB
            },
            _ => new TenantLimits()
        };
    }

    #endregion
}

/// <summary>
/// Default implementation of tenant context accessor using AsyncLocal.
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new();

    public TenantContext? TenantContext
    {
        get => _tenantContextCurrent.Value?.Context;
        set
        {
            var holder = _tenantContextCurrent.Value;
            if (holder != null)
            {
                // Clear current context trapped in the AsyncLocals, as its done.
                holder.Context = null;
            }

            if (value != null)
            {
                // Use an object indirection to hold the TenantContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _tenantContextCurrent.Value = new TenantContextHolder { Context = value };
            }
        }
    }

    private class TenantContextHolder
    {
        public TenantContext? Context { get; set; }
    }
}

/// <summary>
/// Guard that validates tenant limits before operations.
/// </summary>
public class TenantLimitGuard
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantLimitGuard> _logger;

    public TenantLimitGuard(ITenantService tenantService, ILogger<TenantLimitGuard> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<bool> CheckLimitAsync(string limitName, long currentValue, CancellationToken cancellationToken = default)
    {
        var tenant = _tenantService.GetCurrentTenant();
        var limits = tenant.Limits;

        var maxValue = limitName switch
        {
            "ProcessDefinitions" => limits.MaxProcessDefinitions,
            "ActiveProcessInstances" => limits.MaxActiveProcessInstances,
            "TasksPerProcess" => limits.MaxTasksPerProcess,
            "VariablesPerProcess" => limits.MaxVariablesPerProcess,
            "WebhookSubscriptions" => limits.MaxWebhookSubscriptions,
            "DocumentTemplates" => limits.MaxDocumentTemplates,
            "ExternalConnectors" => limits.MaxExternalConnectors,
            "EmailsPerDay" => limits.MaxEmailsPerDay,
            "ApiCallsPerMinute" => limits.MaxApiCallsPerMinute,
            "StorageMb" => limits.MaxStorageMb,
            _ => long.MaxValue
        };

        if (currentValue >= maxValue)
        {
            _logger.LogWarning("Tenant {TenantId} has reached limit {LimitName}: {Current}/{Max}",
                tenant.TenantId, limitName, currentValue, maxValue);
            return false;
        }

        return true;
    }

    public async Task EnsureLimitAsync(string limitName, long currentValue, CancellationToken cancellationToken = default)
    {
        if (!await CheckLimitAsync(limitName, currentValue, cancellationToken))
        {
            var tenant = _tenantService.GetCurrentTenant();
            throw new TenantLimitExceededException(tenant.TenantId, limitName);
        }
    }
}

/// <summary>
/// Exception thrown when a tenant limit is exceeded.
/// </summary>
public class TenantLimitExceededException : Exception
{
    public string TenantId { get; }
    public string LimitName { get; }

    public TenantLimitExceededException(string tenantId, string limitName)
        : base($"Tenant '{tenantId}' has exceeded the limit for '{limitName}'")
    {
        TenantId = tenantId;
        LimitName = limitName;
    }
}
