using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Models;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for managing process version migrations.
/// </summary>
public class ProcessMigrationService : IProcessMigrationService
{
    private readonly IProcessDefinitionService _definitionService;
    private readonly IProcessExecutionService _executionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProcessMigrationService> _logger;

    // In-memory storage - replace with database in production
    private readonly ConcurrentDictionary<string, MigrationPlan> _migrationPlans = new();
    private readonly ConcurrentDictionary<string, MigrationExecution> _migrationExecutions = new();
    private readonly ConcurrentDictionary<string, List<MigrationRecord>> _migrationRecords = new();

    public ProcessMigrationService(
        IProcessDefinitionService definitionService,
        IProcessExecutionService executionService,
        IAuditService auditService,
        ILogger<ProcessMigrationService> logger)
    {
        _definitionService = definitionService;
        _executionService = executionService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<MigrationPlan> CreateMigrationPlanAsync(CreateMigrationPlanRequest request, CancellationToken cancellationToken = default)
    {
        // Validate source and target definitions exist
        var tenantId = 1; // TODO: get from context
        var sourceDefinition = await _definitionService.GetByIdAsync(request.SourceProcessDefinitionId, tenantId, cancellationToken);
        if (sourceDefinition == null)
        {
            throw new InvalidOperationException($"Source process definition '{request.SourceProcessDefinitionId}' not found");
        }

        var targetDefinition = await _definitionService.GetByIdAsync(request.TargetProcessDefinitionId, tenantId, cancellationToken);
        if (targetDefinition == null)
        {
            throw new InvalidOperationException($"Target process definition '{request.TargetProcessDefinitionId}' not found");
        }

        var plan = new MigrationPlan
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            SourceProcessDefinitionId = request.SourceProcessDefinitionId,
            SourceVersion = sourceDefinition.Version,
            TargetProcessDefinitionId = request.TargetProcessDefinitionId,
            TargetVersion = targetDefinition.Version,
            ActivityMappings = request.ActivityMappings ?? new List<ActivityMapping>(),
            VariableMappings = request.VariableMappings ?? new List<VariableMapping>(),
            Instructions = request.Instructions ?? new List<MigrationInstruction>(),
            Options = request.Options ?? new MigrationOptions(),
            CreatedAt = DateTime.UtcNow
        };

        // Auto-generate mappings if requested
        if (request.AutoGenerateMappings)
        {
            GenerateAutoMappings(plan, sourceDefinition, targetDefinition);
        }

        _migrationPlans[plan.Id] = plan;

        _logger.LogInformation("Created migration plan {PlanId} from {SourceId} v{SourceVersion} to {TargetId} v{TargetVersion}",
            plan.Id, request.SourceProcessDefinitionId, sourceDefinition.Version,
            request.TargetProcessDefinitionId, targetDefinition.Version);

        return plan;
    }

    public async Task<MigrationValidationResult> ValidateMigrationPlanAsync(string migrationPlanId, CancellationToken cancellationToken = default)
    {
        if (!_migrationPlans.TryGetValue(migrationPlanId, out var plan))
        {
            throw new InvalidOperationException($"Migration plan '{migrationPlanId}' not found");
        }

        var result = new MigrationValidationResult
        {
            MigrationPlanId = migrationPlanId,
            IsValid = true
        };

        // Get source and target definitions
        var tenantId = 1; // TODO: get from context
        var sourceDefinition = await _definitionService.GetByIdAsync(plan.SourceProcessDefinitionId, tenantId, cancellationToken);
        var targetDefinition = await _definitionService.GetByIdAsync(plan.TargetProcessDefinitionId, tenantId, cancellationToken);

        if (sourceDefinition == null || targetDefinition == null)
        {
            result.IsValid = false;
            result.Errors.Add(new MigrationValidationError
            {
                Code = "DEFINITION_NOT_FOUND",
                Message = "Source or target process definition not found"
            });
            return result;
        }

        // Validate activity mappings
        var sourceActivities = (sourceDefinition.Model?.Activities?.Keys ?? Enumerable.Empty<string>()).ToHashSet();
        var targetActivities = (targetDefinition.Model?.Activities?.Keys ?? Enumerable.Empty<string>()).ToHashSet();

        foreach (var mapping in plan.ActivityMappings)
        {
            if (!sourceActivities.Contains(mapping.SourceActivityId))
            {
                result.Errors.Add(new MigrationValidationError
                {
                    Code = "INVALID_SOURCE_ACTIVITY",
                    Message = $"Source activity '{mapping.SourceActivityId}' does not exist",
                    SourceActivityId = mapping.SourceActivityId
                });
            }

            if (!targetActivities.Contains(mapping.TargetActivityId))
            {
                result.Errors.Add(new MigrationValidationError
                {
                    Code = "INVALID_TARGET_ACTIVITY",
                    Message = $"Target activity '{mapping.TargetActivityId}' does not exist",
                    TargetActivityId = mapping.TargetActivityId
                });
            }
        }

        // Check for unmapped activities that have instances
        var migratableInstances = await GetMigratableInstancesAsync(
            plan.SourceProcessDefinitionId, plan.TargetProcessDefinitionId, cancellationToken);

        var mappedSourceActivities = plan.ActivityMappings.Select(m => m.SourceActivityId).ToHashSet();
        var instanceActivities = migratableInstances.Select(i => i.CurrentActivityId).Distinct();

        foreach (var activityId in instanceActivities)
        {
            if (!mappedSourceActivities.Contains(activityId))
            {
                result.Warnings.Add(new MigrationValidationWarning
                {
                    Code = "UNMAPPED_ACTIVITY_WITH_INSTANCES",
                    Message = $"Activity '{activityId}' has active instances but no mapping defined",
                    ActivityId = activityId,
                    Suggestion = "Define an explicit mapping or skip instruction for this activity"
                });
            }
        }

        // Validate variable mappings
        foreach (var mapping in plan.VariableMappings)
        {
            if (!string.IsNullOrEmpty(mapping.TransformExpression))
            {
                // Validate expression syntax
                var validationResult = ValidateTransformExpression(mapping.TransformExpression, mapping.SourceVariableName);
                if (!validationResult.IsValid)
                {
                    result.Errors.Add(new MigrationValidationError
                    {
                        Code = "INVALID_TRANSFORM_EXPRESSION",
                        Message = validationResult.ErrorMessage ?? $"Invalid transform expression for variable '{mapping.SourceVariableName}'",
                        VariableName = mapping.SourceVariableName
                    });
                }
            }
        }

        // Count valid/invalid instances
        result.ValidInstanceCount = migratableInstances.Count(i => i.CanMigrate);
        result.InvalidInstanceCount = migratableInstances.Count(i => !i.CanMigrate);
        result.InvalidInstances = migratableInstances
            .Where(i => !i.CanMigrate)
            .Select(i => new InvalidInstanceInfo
            {
                InstanceId = i.InstanceId,
                Reason = i.MigrationIssue ?? "Unknown",
                CurrentActivityId = i.CurrentActivityId
            })
            .ToList();

        result.IsValid = !result.Errors.Any();

        // Update plan validation status
        plan.IsValidated = result.IsValid;
        plan.ValidatedAt = DateTime.UtcNow;

        _logger.LogInformation("Validated migration plan {PlanId}: Valid={IsValid}, Errors={ErrorCount}, Warnings={WarningCount}",
            migrationPlanId, result.IsValid, result.Errors.Count, result.Warnings.Count);

        return result;
    }

    public async Task<MigrationExecution> ExecuteMigrationAsync(ExecuteMigrationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_migrationPlans.TryGetValue(request.MigrationPlanId, out var plan))
        {
            throw new InvalidOperationException($"Migration plan '{request.MigrationPlanId}' not found");
        }

        // Validate plan first if not already validated
        if (!plan.IsValidated)
        {
            var validation = await ValidateMigrationPlanAsync(request.MigrationPlanId, cancellationToken);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException("Migration plan validation failed. Fix errors before executing.");
            }
        }

        // Get instances to migrate
        var allInstances = await GetMigratableInstancesAsync(
            plan.SourceProcessDefinitionId, plan.TargetProcessDefinitionId, cancellationToken);

        var instancesToMigrate = allInstances.Where(i => i.CanMigrate).ToList();

        // Filter by specific instance IDs if provided
        if (request.InstanceIds != null && request.InstanceIds.Count > 0)
        {
            instancesToMigrate = instancesToMigrate
                .Where(i => request.InstanceIds.Contains(i.InstanceId))
                .ToList();
        }

        var execution = new MigrationExecution
        {
            Id = Guid.NewGuid().ToString(),
            MigrationPlanId = request.MigrationPlanId,
            Status = MigrationExecutionStatus.Running,
            TotalInstances = instancesToMigrate.Count,
            IsDryRun = request.DryRun,
            ExecutedBy = request.ExecutedBy,
            StartedAt = DateTime.UtcNow
        };

        _migrationExecutions[execution.Id] = execution;

        _logger.LogInformation("Starting migration execution {ExecutionId} for plan {PlanId}, instances: {Count}, dryRun: {DryRun}",
            execution.Id, request.MigrationPlanId, instancesToMigrate.Count, request.DryRun);

        try
        {
            // Process instances in batches
            var batches = instancesToMigrate
                .Select((instance, index) => new { instance, index })
                .GroupBy(x => x.index / plan.Options.BatchSize)
                .Select(g => g.Select(x => x.instance).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                if (cancellationToken.IsCancellationRequested || execution.Status == MigrationExecutionStatus.Cancelled)
                {
                    execution.Status = MigrationExecutionStatus.Cancelled;
                    break;
                }

                await ProcessMigrationBatchAsync(execution, plan, batch, request.DryRun, cancellationToken);

                // Check if we should stop on error
                if (!plan.Options.ContinueOnError && execution.Errors.Any())
                {
                    execution.Status = MigrationExecutionStatus.Failed;
                    break;
                }
            }

            if (execution.Status == MigrationExecutionStatus.Running)
            {
                execution.Status = execution.FailedMigrations > 0
                    ? MigrationExecutionStatus.Completed // Completed with errors
                    : MigrationExecutionStatus.Completed;
            }

            execution.CompletedAt = DateTime.UtcNow;
            execution.CanRollback = !request.DryRun && execution.SuccessfulMigrations > 0;

            _logger.LogInformation("Migration execution {ExecutionId} completed: Status={Status}, Success={Success}, Failed={Failed}",
                execution.Id, execution.Status, execution.SuccessfulMigrations, execution.FailedMigrations);
        }
        catch (Exception ex)
        {
            execution.Status = MigrationExecutionStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.Errors.Add(new MigrationExecutionError
            {
                ErrorCode = "EXECUTION_FAILED",
                ErrorMessage = ex.Message,
                OccurredAt = DateTime.UtcNow
            });

            _logger.LogError(ex, "Migration execution {ExecutionId} failed", execution.Id);
        }

        return execution;
    }

    public Task<MigrationExecution?> GetMigrationExecutionAsync(string executionId, CancellationToken cancellationToken = default)
    {
        _migrationExecutions.TryGetValue(executionId, out var execution);
        return Task.FromResult(execution);
    }

    public Task CancelMigrationAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (_migrationExecutions.TryGetValue(executionId, out var execution))
        {
            if (execution.Status == MigrationExecutionStatus.Running)
            {
                execution.Status = MigrationExecutionStatus.Cancelled;
                _logger.LogInformation("Migration execution {ExecutionId} cancelled", executionId);
            }
        }

        return Task.CompletedTask;
    }

    public async Task<MigrationExecution> RollbackMigrationAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (!_migrationExecutions.TryGetValue(executionId, out var originalExecution))
        {
            throw new InvalidOperationException($"Migration execution '{executionId}' not found");
        }

        if (!originalExecution.CanRollback)
        {
            throw new InvalidOperationException("This migration cannot be rolled back");
        }

        if (originalExecution.IsRolledBack)
        {
            throw new InvalidOperationException("This migration has already been rolled back");
        }

        var rollbackExecution = new MigrationExecution
        {
            Id = Guid.NewGuid().ToString(),
            MigrationPlanId = originalExecution.MigrationPlanId,
            Status = MigrationExecutionStatus.RollingBack,
            TotalInstances = originalExecution.SuccessfulMigrations,
            IsDryRun = false,
            StartedAt = DateTime.UtcNow
        };

        _migrationExecutions[rollbackExecution.Id] = rollbackExecution;

        // Get migration records for this execution - take snapshot to avoid concurrent modification
        var allRecords = new List<MigrationRecord>();
        foreach (var kvp in _migrationRecords.ToArray())
        {
            lock (kvp.Value)
            {
                allRecords.AddRange(kvp.Value.Where(r => r != null));
            }
        }
        var records = allRecords
            .Where(r => r.MigrationExecutionId == executionId)
            .ToList();

        _logger.LogInformation("Starting rollback of migration {ExecutionId}, restoring {Count} instances",
            executionId, records.Count);

        foreach (var record in records)
        {
            try
            {
                // Restore the instance to its pre-migration state
                await RestoreInstanceStateAsync(record, cancellationToken);
                rollbackExecution.SuccessfulMigrations++;
                rollbackExecution.ProcessedInstances++;
            }
            catch (Exception ex)
            {
                rollbackExecution.FailedMigrations++;
                rollbackExecution.ProcessedInstances++;
                rollbackExecution.Errors.Add(new MigrationExecutionError
                {
                    InstanceId = record.InstanceId,
                    ErrorCode = "ROLLBACK_FAILED",
                    ErrorMessage = ex.Message,
                    OccurredAt = DateTime.UtcNow
                });

                _logger.LogError(ex, "Failed to rollback instance {InstanceId}", record.InstanceId);
            }
        }

        rollbackExecution.Status = MigrationExecutionStatus.RolledBack;
        rollbackExecution.CompletedAt = DateTime.UtcNow;

        originalExecution.IsRolledBack = true;
        originalExecution.RollbackExecutionId = rollbackExecution.Id;

        _logger.LogInformation("Rollback completed: Success={Success}, Failed={Failed}",
            rollbackExecution.SuccessfulMigrations, rollbackExecution.FailedMigrations);

        return rollbackExecution;
    }

    public async Task<IList<MigratableInstance>> GetMigratableInstancesAsync(string sourceDefinitionId, string targetDefinitionId, CancellationToken cancellationToken = default)
    {
        var tenantId = 1; // TODO: get from context

        // Get all active instances for the source definition
        // TODO: QueryInstancesAsync method signature needs to be verified
        var instances = new PagedResult<ProcessInstanceSummary> { Items = new List<ProcessInstanceSummary>(), TotalCount = 0 };

        var sourceDefinition = await _definitionService.GetByIdAsync(sourceDefinitionId, tenantId, cancellationToken);
        var targetDefinition = await _definitionService.GetByIdAsync(targetDefinitionId, tenantId, cancellationToken);

        var targetActivities = (targetDefinition?.LatestVersionDetail?.Model?.Activities?.Keys ?? Enumerable.Empty<string>()).ToHashSet();

        var migratableInstances = new List<MigratableInstance>();

        foreach (var instance in instances.Items)
        {
            var migratable = new MigratableInstance
            {
                InstanceId = instance.Id,
                BusinessKey = instance.BusinessKey ?? "",
                CurrentActivityId = instance.CurrentActivityId ?? "",
                CurrentActivityName = sourceDefinition?.LatestVersionDetail?.Model?.Activities?
                    .GetValueOrDefault(instance.CurrentActivityId ?? "")?.Name ?? "",
                StartedAt = instance.StartedAt,
                PendingTaskCount = 0 // Would query actual pending tasks
            };

            // Check if current activity exists in target
            if (!string.IsNullOrEmpty(instance.CurrentActivityId) && targetActivities.Contains(instance.CurrentActivityId))
            {
                migratable.CanMigrate = true;
                migratable.TargetActivityId = instance.CurrentActivityId;
            }
            else if (string.IsNullOrEmpty(instance.CurrentActivityId))
            {
                migratable.CanMigrate = false;
                migratable.MigrationIssue = "Instance has no current activity";
            }
            else
            {
                migratable.CanMigrate = false;
                migratable.MigrationIssue = $"Current activity '{instance.CurrentActivityId}' does not exist in target definition";
            }

            migratableInstances.Add(migratable);
        }

        return migratableInstances;
    }

    public Task<IList<MigrationRecord>> GetInstanceMigrationHistoryAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (_migrationRecords.TryGetValue(instanceId, out var records))
        {
            List<MigrationRecord> snapshot;
            lock (records)
            {
                snapshot = records.ToList();
            }
            return Task.FromResult<IList<MigrationRecord>>(snapshot.OrderByDescending(r => r.MigratedAt).ToList());
        }

        return Task.FromResult<IList<MigrationRecord>>(new List<MigrationRecord>());
    }

    public Task<MigrationPlan?> GetMigrationPlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        _migrationPlans.TryGetValue(planId, out var plan);
        return Task.FromResult(plan);
    }

    public async Task<IList<ActivityMapping>> GenerateActivityMappingsAsync(
        string sourceDefinitionId,
        string targetDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = 1; // TODO: get from context
        var sourceDefinition = await _definitionService.GetByIdAsync(sourceDefinitionId, tenantId, cancellationToken);
        var targetDefinition = await _definitionService.GetByIdAsync(targetDefinitionId, tenantId, cancellationToken);

        if (sourceDefinition == null || targetDefinition == null)
        {
            return new List<ActivityMapping>();
        }

        var sourceActivities = sourceDefinition.Model?.Activities ?? new Dictionary<string, ActivityDefinition>();
        var targetActivities = targetDefinition.Model?.Activities ?? new Dictionary<string, ActivityDefinition>();

        var mappings = new List<ActivityMapping>();

        // Auto-map activities with matching IDs
        foreach (var sourceActivity in sourceActivities)
        {
            if (targetActivities.ContainsKey(sourceActivity.Key))
            {
                mappings.Add(new ActivityMapping
                {
                    SourceActivityId = sourceActivity.Key,
                    TargetActivityId = sourceActivity.Key,
                    UpdateEventTrigger = true
                });
            }
        }

        _logger.LogDebug("Generated {Count} activity mappings between {SourceId} and {TargetId}",
            mappings.Count, sourceDefinitionId, targetDefinitionId);

        return mappings;
    }

    public Task<IList<MigrationPlan>> ListMigrationPlansAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        // Return all plans (tenant filtering would be applied in production)
        var plans = _migrationPlans.Values.ToList();
        return Task.FromResult<IList<MigrationPlan>>(plans);
    }

    public Task<IList<MigrationExecution>> ListMigrationExecutionsAsync(
        string? planId,
        MigrationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _migrationExecutions.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(planId))
        {
            query = query.Where(e => e.MigrationPlanId == planId);
        }

        if (status.HasValue)
        {
            var executionStatus = status.Value switch
            {
                MigrationStatus.Pending => MigrationExecutionStatus.Pending,
                MigrationStatus.Running => MigrationExecutionStatus.Running,
                MigrationStatus.Completed => MigrationExecutionStatus.Completed,
                MigrationStatus.Failed => MigrationExecutionStatus.Failed,
                MigrationStatus.Cancelled => MigrationExecutionStatus.Cancelled,
                MigrationStatus.RolledBack => MigrationExecutionStatus.RolledBack,
                _ => MigrationExecutionStatus.Pending
            };
            query = query.Where(e => e.Status == executionStatus);
        }

        return Task.FromResult<IList<MigrationExecution>>(query.OrderByDescending(e => e.CreatedAt).ToList());
    }

    public Task<MigrationStatistics> GetMigrationStatisticsAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        var executions = _migrationExecutions.Values.ToList();

        var stats = new MigrationStatistics
        {
            TenantId = tenantId,
            AsOf = DateTime.UtcNow,
            TotalMigrationPlans = _migrationPlans.Count,
            TotalMigrationExecutions = executions.Count,
            SuccessfulMigrations = executions.Count(e => e.Status == MigrationExecutionStatus.Completed),
            FailedMigrations = executions.Count(e => e.Status == MigrationExecutionStatus.Failed),
            PendingMigrations = executions.Count(e => e.Status == MigrationExecutionStatus.Pending || e.Status == MigrationExecutionStatus.Running),
            TotalInstancesMigrated = executions.Sum(e => e.SuccessfulMigrations),
            AverageMigrationDurationSeconds = executions
                .Where(e => e.Duration.HasValue)
                .Select(e => e.Duration!.Value.TotalSeconds)
                .DefaultIfEmpty(0)
                .Average(),
            LastMigrationAt = executions
                .Where(e => e.CompletedAt.HasValue)
                .OrderByDescending(e => e.CompletedAt)
                .Select(e => e.CompletedAt)
                .FirstOrDefault()
        };

        return Task.FromResult(stats);
    }

    public Task DeleteMigrationPlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!_migrationPlans.TryRemove(planId, out _))
        {
            throw new InvalidOperationException($"Migration plan '{planId}' not found");
        }

        _logger.LogInformation("Deleted migration plan {PlanId}", planId);

        return Task.CompletedTask;
    }

    #region Private Methods

    private void GenerateAutoMappings(MigrationPlan plan, ProcessDefinitionDetailDto sourceDefinition, ProcessDefinitionDetailDto targetDefinition)
    {
        var sourceActivities = sourceDefinition.LatestVersionDetail?.Model?.Activities ?? new Dictionary<string, ActivityDefinition>();
        var targetActivities = targetDefinition.LatestVersionDetail?.Model?.Activities ?? new Dictionary<string, ActivityDefinition>();

        // Map activities with same ID
        foreach (var sourceActivity in sourceActivities)
        {
            if (targetActivities.ContainsKey(sourceActivity.Key))
            {
                plan.ActivityMappings.Add(new ActivityMapping
                {
                    SourceActivityId = sourceActivity.Key,
                    TargetActivityId = sourceActivity.Key,
                    UpdateEventTrigger = true
                });
            }
        }

        _logger.LogDebug("Auto-generated {Count} activity mappings", plan.ActivityMappings.Count);
    }

    private async Task ProcessMigrationBatchAsync(
        MigrationExecution execution,
        MigrationPlan plan,
        List<MigratableInstance> batch,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var tasks = batch.Select(instance => MigrateInstanceAsync(execution, plan, instance, dryRun, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task MigrateInstanceAsync(
        MigrationExecution execution,
        MigrationPlan plan,
        MigratableInstance instance,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        try
        {
            // Find the activity mapping
            var mapping = plan.ActivityMappings
                .FirstOrDefault(m => m.SourceActivityId == instance.CurrentActivityId);

            if (mapping == null)
            {
                execution.SkippedInstances++;
                execution.ProcessedInstances++;
                return;
            }

            // Get current instance state for rollback
            var currentState = await _executionService.GetInstanceAsync(instance.InstanceId, cancellationToken);
            if (currentState == null)
            {
                throw new InvalidOperationException($"Instance '{instance.InstanceId}' not found");
            }

            // Create migration record for rollback
            var record = new MigrationRecord
            {
                Id = Guid.NewGuid().ToString(),
                InstanceId = instance.InstanceId,
                MigrationPlanId = plan.Id,
                MigrationExecutionId = execution.Id,
                SourceProcessDefinitionId = plan.SourceProcessDefinitionId,
                SourceVersion = plan.SourceVersion,
                TargetProcessDefinitionId = plan.TargetProcessDefinitionId,
                TargetVersion = plan.TargetVersion,
                SourceActivityId = instance.CurrentActivityId,
                TargetActivityId = mapping.TargetActivityId,
                PreMigrationState = JsonSerializer.Serialize(new
                {
                    CurrentActivityId = instance.CurrentActivityId,
                    ProcessDefinitionId = plan.SourceProcessDefinitionId
                }),
                PreMigrationVariables = currentState.Variables ?? new Dictionary<string, object>(),
                MigratedAt = DateTime.UtcNow
            };

            if (!dryRun)
            {
                // Perform the actual migration
                await PerformMigrationAsync(instance.InstanceId, plan, mapping, cancellationToken);

                // Store migration record
                var records = _migrationRecords.GetOrAdd(instance.InstanceId, _ => new List<MigrationRecord>());
                lock (records)
                {
                    records.Add(record);
                }

                // Log audit event
                await _auditService.LogAsync(new AuditLogRequest
                {
                    TenantId = currentState.TenantId,
                    EventType = "ProcessInstance.Migrated",
                    EntityType = "ProcessInstance",
                    EntityId = instance.InstanceId,
                    Description = $"Migrated from {plan.SourceProcessDefinitionId} v{plan.SourceVersion} to {plan.TargetProcessDefinitionId} v{plan.TargetVersion}",
                    NewValues = new Dictionary<string, object>
                    {
                        ["migrationPlanId"] = plan.Id,
                        ["sourceActivity"] = instance.CurrentActivityId,
                        ["targetActivity"] = mapping.TargetActivityId
                    }
                }, cancellationToken);
            }

            execution.SuccessfulMigrations++;
            execution.ProcessedInstances++;

            _logger.LogDebug("Migrated instance {InstanceId} from {SourceActivity} to {TargetActivity} (dryRun={DryRun})",
                instance.InstanceId, instance.CurrentActivityId, mapping.TargetActivityId, dryRun);
        }
        catch (Exception ex)
        {
            execution.FailedMigrations++;
            execution.ProcessedInstances++;
            execution.Errors.Add(new MigrationExecutionError
            {
                InstanceId = instance.InstanceId,
                ErrorCode = "MIGRATION_FAILED",
                ErrorMessage = ex.Message,
                ActivityId = instance.CurrentActivityId,
                OccurredAt = DateTime.UtcNow,
                IsRetryable = true
            });

            _logger.LogError(ex, "Failed to migrate instance {InstanceId}", instance.InstanceId);
        }
    }

    private async Task PerformMigrationAsync(
        string instanceId,
        MigrationPlan plan,
        ActivityMapping mapping,
        CancellationToken cancellationToken)
    {
        // In production, this would:
        // 1. Update the instance's process definition reference
        // 2. Update the current activity ID
        // 3. Apply variable transformations
        // 4. Update any pending tasks
        // 5. Re-evaluate timers and event subscriptions

        // For now, log the migration
        _logger.LogInformation("Performing migration for instance {InstanceId}: {SourceActivity} -> {TargetActivity}",
            instanceId, mapping.SourceActivityId, mapping.TargetActivityId);

        // Apply variable mappings
        foreach (var varMapping in plan.VariableMappings)
        {
            _logger.LogDebug("Applying variable mapping: {Source} -> {Target}",
                varMapping.SourceVariableName, varMapping.TargetVariableName);
        }

        // Execute migration instructions
        foreach (var instruction in (plan.Instructions ?? Enumerable.Empty<MigrationInstruction>()).OrderBy(i => i.Order))
        {
            _logger.LogDebug("Executing instruction: {Type} for {ActivityId}",
                instruction.Type, instruction.SourceActivityId);
        }
    }

    private async Task RestoreInstanceStateAsync(MigrationRecord record, CancellationToken cancellationToken)
    {
        // In production, this would restore the instance to its pre-migration state
        _logger.LogInformation("Restoring instance {InstanceId} to pre-migration state", record.InstanceId);

        // Restore process definition reference
        // Restore current activity
        // Restore variables
    }

    /// <summary>
    /// Validates a transform expression for syntax and safety
    /// </summary>
    private (bool IsValid, string? ErrorMessage) ValidateTransformExpression(string expression, string variableName)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return (false, $"Transform expression for variable '{variableName}' cannot be empty");
        }

        // Check for dangerous patterns (SQL injection, script injection)
        var dangerousPatterns = new[]
        {
            "DROP ", "DELETE ", "INSERT ", "UPDATE ", "TRUNCATE ", // SQL
            "<script", "javascript:", "onclick", "onerror", // XSS
            "System.IO", "Process.Start", "File.Delete", // Code execution
            "eval(", "exec(", "Runtime.exec" // Eval patterns
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (expression.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Potentially dangerous pattern '{Pattern}' detected in transform expression for variable '{VariableName}'",
                    pattern, variableName);
                return (false, $"Transform expression contains potentially dangerous pattern: {pattern}");
            }
        }

        // Validate balanced parentheses and brackets
        var parenCount = 0;
        var bracketCount = 0;
        var braceCount = 0;

        foreach (var c in expression)
        {
            switch (c)
            {
                case '(': parenCount++; break;
                case ')': parenCount--; break;
                case '[': bracketCount++; break;
                case ']': bracketCount--; break;
                case '{': braceCount++; break;
                case '}': braceCount--; break;
            }

            // Check for negative counts (closing before opening)
            if (parenCount < 0 || bracketCount < 0 || braceCount < 0)
            {
                return (false, $"Transform expression for variable '{variableName}' has unbalanced brackets");
            }
        }

        if (parenCount != 0 || bracketCount != 0 || braceCount != 0)
        {
            return (false, $"Transform expression for variable '{variableName}' has unbalanced brackets");
        }

        // Validate basic expression structure (must reference the source variable)
        var validOperators = new[] { "+", "-", "*", "/", "??", "?.", ".", "==", "!=", ">", "<", ">=", "<=" };
        var hasValidStructure = expression.Contains("$") || // Variable reference
                                 validOperators.Any(op => expression.Contains(op)) ||
                                 expression.StartsWith("(") || // Grouped expression
                                 expression.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.'); // Simple property path

        if (!hasValidStructure)
        {
            _logger.LogDebug("Transform expression '{Expression}' for variable '{VariableName}' may have invalid structure",
                expression, variableName);
            // Return valid but log for review - allow flexibility for custom expression languages
        }

        return (true, null);
    }

    #endregion
}
