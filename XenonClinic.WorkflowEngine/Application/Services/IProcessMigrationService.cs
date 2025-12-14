using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for managing process version migrations.
/// </summary>
public interface IProcessMigrationService
{
    /// <summary>
    /// Creates a migration plan from one process version to another.
    /// </summary>
    Task<MigrationPlan> CreateMigrationPlanAsync(CreateMigrationPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a migration plan without executing it.
    /// </summary>
    Task<MigrationValidationResult> ValidateMigrationPlanAsync(string migrationPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a migration plan.
    /// </summary>
    Task<MigrationExecution> ExecuteMigrationAsync(ExecuteMigrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets migration execution status.
    /// </summary>
    Task<MigrationExecution?> GetMigrationExecutionAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running migration.
    /// </summary>
    Task CancelMigrationAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back a completed migration (if possible).
    /// </summary>
    Task<MigrationExecution> RollbackMigrationAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets instances that can be migrated between versions.
    /// </summary>
    Task<IList<MigratableInstance>> GetMigratableInstancesAsync(string sourceDefinitionId, string targetDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets migration history for a process instance.
    /// </summary>
    Task<IList<MigrationRecord>> GetInstanceMigrationHistoryAsync(string instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a migration plan by ID.
    /// </summary>
    Task<MigrationPlan?> GetMigrationPlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates activity mappings automatically between source and target definitions.
    /// </summary>
    Task<IList<ActivityMapping>> GenerateActivityMappingsAsync(string sourceDefinitionId, string targetDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists migration plans.
    /// </summary>
    Task<IList<MigrationPlan>> ListMigrationPlansAsync(string? tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists migration executions.
    /// </summary>
    Task<IList<MigrationExecution>> ListMigrationExecutionsAsync(string? planId, MigrationStatus? status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets migration statistics.
    /// </summary>
    Task<MigrationStatistics> GetMigrationStatisticsAsync(string? tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a migration plan.
    /// </summary>
    Task DeleteMigrationPlanAsync(string planId, CancellationToken cancellationToken = default);
}

#region Migration Plan DTOs

public class MigrationPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Source and Target Definitions
    public string SourceProcessDefinitionId { get; set; } = string.Empty;
    public int SourceVersion { get; set; }
    public string TargetProcessDefinitionId { get; set; } = string.Empty;
    public int TargetVersion { get; set; }

    // Activity Mappings
    public List<ActivityMapping> ActivityMappings { get; set; } = new();

    // Variable Mappings
    public List<VariableMapping> VariableMappings { get; set; } = new();

    // Migration Instructions
    public List<MigrationInstruction> Instructions { get; set; } = new();

    // Options
    public MigrationOptions Options { get; set; } = new();

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public bool IsValidated { get; set; }
    public DateTime? ValidatedAt { get; set; }
}

public class ActivityMapping
{
    public string SourceActivityId { get; set; } = string.Empty;
    public string TargetActivityId { get; set; } = string.Empty;
    public bool UpdateEventTrigger { get; set; }
    public Dictionary<string, string> PropertyMappings { get; set; } = new();
}

public class VariableMapping
{
    public string SourceVariableName { get; set; } = string.Empty;
    public string? TargetVariableName { get; set; } // null means delete
    public string? TransformExpression { get; set; } // Expression to transform value
    public object? DefaultValue { get; set; } // For new variables
    public bool IsRequired { get; set; }
}

public class MigrationInstruction
{
    public MigrationInstructionType Type { get; set; }
    public string? SourceActivityId { get; set; }
    public string? TargetActivityId { get; set; }
    public string? VariableName { get; set; }
    public object? Value { get; set; }
    public string? Expression { get; set; }
    public int Order { get; set; }
}

public enum MigrationInstructionType
{
    MapActivity,
    SkipActivity,
    AddActivity,
    RemoveActivity,
    SetVariable,
    RemoveVariable,
    TransformVariable,
    ExecuteScript,
    CompletePendingTask,
    CancelPendingTask
}

public class MigrationOptions
{
    public bool SkipCustomListeners { get; set; } = false;
    public bool SkipIoMappings { get; set; } = false;
    public bool UpdateEventTriggers { get; set; } = true;
    public bool PreserveTaskHistory { get; set; } = true;
    public bool ContinueOnError { get; set; } = false;
    public int BatchSize { get; set; } = 50;
    public int MaxParallelism { get; set; } = 4;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(1);
}

#endregion

#region Migration Request/Response DTOs

public class CreateMigrationPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceProcessDefinitionId { get; set; } = string.Empty;
    public string TargetProcessDefinitionId { get; set; } = string.Empty;
    public List<ActivityMapping>? ActivityMappings { get; set; }
    public List<VariableMapping>? VariableMappings { get; set; }
    public List<MigrationInstruction>? Instructions { get; set; }
    public MigrationOptions? Options { get; set; }
    public bool AutoGenerateMappings { get; set; } = true;
}

public class ExecuteMigrationRequest
{
    public string MigrationPlanId { get; set; } = string.Empty;
    public List<string>? InstanceIds { get; set; } // null means all matching instances
    public string? InstanceFilter { get; set; } // Expression to filter instances
    public bool DryRun { get; set; } = false;
    public DateTime? ScheduledAt { get; set; }
    public string? ExecutedBy { get; set; }
}

#endregion

#region Migration Validation DTOs

public class MigrationValidationResult
{
    public bool IsValid { get; set; }
    public string MigrationPlanId { get; set; } = string.Empty;
    public int ValidInstanceCount { get; set; }
    public int InvalidInstanceCount { get; set; }
    public List<MigrationValidationError> Errors { get; set; } = new();
    public List<MigrationValidationWarning> Warnings { get; set; } = new();
    public List<InvalidInstanceInfo> InvalidInstances { get; set; } = new();
}

public class MigrationValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SourceActivityId { get; set; }
    public string? TargetActivityId { get; set; }
    public string? VariableName { get; set; }
}

public class MigrationValidationWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public string? Suggestion { get; set; }
}

public class InvalidInstanceInfo
{
    public string InstanceId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? CurrentActivityId { get; set; }
}

#endregion

#region Migration Execution DTOs

public class MigrationExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MigrationPlanId { get; set; } = string.Empty;
    public MigrationExecutionStatus Status { get; set; } = MigrationExecutionStatus.Pending;

    // Progress
    public int TotalInstances { get; set; }
    public int ProcessedInstances { get; set; }
    public int SuccessfulMigrations { get; set; }
    public int FailedMigrations { get; set; }
    public int SkippedInstances { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;

    // Errors
    public List<MigrationExecutionError> Errors { get; set; } = new();

    // Options
    public bool IsDryRun { get; set; }
    public bool CanRollback { get; set; }
    public string? ExecutedBy { get; set; }

    // Rollback Info
    public string? RollbackExecutionId { get; set; }
    public bool IsRolledBack { get; set; }
}

public enum MigrationExecutionStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled,
    RollingBack,
    RolledBack
}

public class MigrationExecutionError
{
    public string InstanceId { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public bool IsRetryable { get; set; }
}

#endregion

#region Migratable Instance DTOs

public class MigratableInstance
{
    public string InstanceId { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public string CurrentActivityId { get; set; } = string.Empty;
    public string CurrentActivityName { get; set; } = string.Empty;
    public string? TargetActivityId { get; set; } // Proposed target activity
    public bool CanMigrate { get; set; }
    public string? MigrationIssue { get; set; }
    public DateTime StartedAt { get; set; }
    public int PendingTaskCount { get; set; }
}

#endregion

#region Migration History DTOs

public class MigrationRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InstanceId { get; set; } = string.Empty;
    public string MigrationPlanId { get; set; } = string.Empty;
    public string MigrationExecutionId { get; set; } = string.Empty;

    // Version Info
    public string SourceProcessDefinitionId { get; set; } = string.Empty;
    public int SourceVersion { get; set; }
    public string TargetProcessDefinitionId { get; set; } = string.Empty;
    public int TargetVersion { get; set; }

    // Activity Migration
    public string SourceActivityId { get; set; } = string.Empty;
    public string TargetActivityId { get; set; } = string.Empty;

    // Timing
    public DateTime MigratedAt { get; set; } = DateTime.UtcNow;
    public string? MigratedBy { get; set; }

    // State Snapshot (for rollback)
    public string? PreMigrationState { get; set; }
    public Dictionary<string, object> PreMigrationVariables { get; set; } = new();
}

#endregion

#region Migration Statistics

/// <summary>
/// Status filter for migration queries.
/// </summary>
public enum MigrationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    RolledBack
}

/// <summary>
/// Migration statistics for a tenant.
/// </summary>
public class MigrationStatistics
{
    public string? TenantId { get; set; }
    public DateTime AsOf { get; set; } = DateTime.UtcNow;
    public int TotalMigrationPlans { get; set; }
    public int TotalMigrationExecutions { get; set; }
    public int SuccessfulMigrations { get; set; }
    public int FailedMigrations { get; set; }
    public int PendingMigrations { get; set; }
    public int TotalInstancesMigrated { get; set; }
    public double AverageMigrationDurationSeconds { get; set; }
    public DateTime? LastMigrationAt { get; set; }
}

#endregion
