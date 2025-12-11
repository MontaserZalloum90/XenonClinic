using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        var sourceDefinition = await _definitionService.GetByIdAsync(request.SourceProcessDefinitionId, cancellationToken);
        if (sourceDefinition == null)
        {
            throw new InvalidOperationException($"Source process definition '{request.SourceProcessDefinitionId}' not found");
        }

        var targetDefinition = await _definitionService.GetByIdAsync(request.TargetProcessDefinitionId, cancellationToken);
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
        var sourceDefinition = await _definitionService.GetByIdAsync(plan.SourceProcessDefinitionId, cancellationToken);
        var targetDefinition = await _definitionService.GetByIdAsync(plan.TargetProcessDefinitionId, cancellationToken);

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
        var sourceActivities = sourceDefinition.Model.Activities.Keys.ToHashSet();
        var targetActivities = targetDefinition.Model.Activities.Keys.ToHashSet();

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
                // Validate expression syntax (basic check)
                try
                {
                    // Would use expression evaluator here
                }
                catch (Exception)
                {
                    result.Errors.Add(new MigrationValidationError
                    {
                        Code = "INVALID_TRANSFORM_EXPRESSION",
                        Message = $"Invalid transform expression for variable '{mapping.SourceVariableName}'",
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

        // Get migration records for this execution
        var records = _migrationRecords.Values
            .SelectMany(r => r)
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
        // Get all active instances for the source definition
        var instances = await _executionService.QueryInstancesAsync(new ProcessInstanceQuery
        {
            ProcessDefinitionId = sourceDefinitionId,
            Statuses = new List<string> { "Running", "Suspended" }
        }, cancellationToken);

        var sourceDefinition = await _definitionService.GetByIdAsync(sourceDefinitionId, cancellationToken);
        var targetDefinition = await _definitionService.GetByIdAsync(targetDefinitionId, cancellationToken);

        var targetActivities = targetDefinition?.Model.Activities.Keys.ToHashSet() ?? new HashSet<string>();

        var migratableInstances = new List<MigratableInstance>();

        foreach (var instance in instances.Items)
        {
            var migratable = new MigratableInstance
            {
                InstanceId = instance.Id,
                BusinessKey = instance.BusinessKey ?? "",
                CurrentActivityId = instance.CurrentActivityId ?? "",
                CurrentActivityName = sourceDefinition?.Model.Activities
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
            return Task.FromResult<IList<MigrationRecord>>(records.OrderByDescending(r => r.MigratedAt).ToList());
        }

        return Task.FromResult<IList<MigrationRecord>>(new List<MigrationRecord>());
    }

    #region Private Methods

    private void GenerateAutoMappings(MigrationPlan plan, Domain.Entities.ProcessDefinition sourceDefinition, Domain.Entities.ProcessDefinition targetDefinition)
    {
        var sourceActivities = sourceDefinition.Model.Activities;
        var targetActivities = targetDefinition.Model.Activities;

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
        foreach (var instruction in plan.Instructions.OrderBy(i => i.Order))
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

    #endregion
}
