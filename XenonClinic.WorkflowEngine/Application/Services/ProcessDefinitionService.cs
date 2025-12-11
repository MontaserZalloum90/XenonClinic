namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.Abstractions;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for managing process definitions.
/// </summary>
public class ProcessDefinitionService : IProcessDefinitionService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly ILogger<ProcessDefinitionService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProcessDefinitionService(
        WorkflowEngineDbContext context,
        ILogger<ProcessDefinitionService> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<ProcessDefinitionDetailDto> CreateAsync(
        int tenantId,
        CreateProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Validate key uniqueness
        var existingKey = await _context.ProcessDefinitions
            .AnyAsync(d => d.TenantId == tenantId && d.Key == request.Key, cancellationToken);

        if (existingKey)
        {
            throw new InvalidOperationException($"A process definition with key '{request.Key}' already exists.");
        }

        // Validate model if provided
        if (request.Model != null)
        {
            var validation = await ValidateAsync(request.Model, cancellationToken);
            if (!validation.IsValid && !request.SaveAsDraft)
            {
                throw new InvalidOperationException(
                    $"Process model validation failed: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
            }
        }

        var now = DateTime.UtcNow;
        var definition = new ProcessDefinition
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags ?? new List<string>(),
            Status = ProcessDefinitionStatus.Active,
            LatestVersion = 1,
            PublishedVersion = request.SaveAsDraft ? null : 1,
            CreatedBy = userId,
            CreatedAt = now,
            UpdatedBy = userId,
            UpdatedAt = now
        };

        var version = new ProcessVersion
        {
            Id = Guid.NewGuid().ToString(),
            ProcessDefinitionId = definition.Id,
            Version = 1,
            Status = request.SaveAsDraft ? ProcessVersionStatus.Draft : ProcessVersionStatus.Published,
            ModelJson = request.Model != null
                ? JsonSerializer.Serialize(request.Model, _jsonOptions)
                : "{}",
            ChangeDescription = "Initial version",
            CreatedBy = userId,
            CreatedAt = now,
            PublishedBy = request.SaveAsDraft ? null : userId,
            PublishedAt = request.SaveAsDraft ? null : now
        };

        _context.ProcessDefinitions.Add(definition);
        _context.ProcessVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created process definition {DefinitionId} with key {Key} for tenant {TenantId}",
            definition.Id, definition.Key, tenantId);

        var result = definition.ToDetailDto();
        result.LatestVersionDetail = version.ToDetailDto(request.Model);
        result.Versions = new List<ProcessVersionSummaryDto> { version.ToSummaryDto() };

        return result;
    }

    public async Task<ProcessDefinitionDetailDto?> GetByIdAsync(
        string id,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .Include(d => d.Versions.OrderByDescending(v => v.Version))
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
            return null;

        var result = definition.ToDetailDto();

        // Load latest version model
        var latestVersion = definition.Versions.FirstOrDefault();
        if (latestVersion != null)
        {
            var model = DeserializeModel(latestVersion.ModelJson);
            result.LatestVersionDetail = latestVersion.ToDetailDto(model);
        }

        return result;
    }

    public async Task<ProcessDefinitionDetailDto?> GetByKeyAsync(
        string key,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .Include(d => d.Versions.OrderByDescending(v => v.Version))
            .FirstOrDefaultAsync(d => d.Key == key && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
            return null;

        var result = definition.ToDetailDto();

        var latestVersion = definition.Versions.FirstOrDefault();
        if (latestVersion != null)
        {
            var model = DeserializeModel(latestVersion.ModelJson);
            result.LatestVersionDetail = latestVersion.ToDetailDto(model);
        }

        return result;
    }

    public async Task<PagedResultDto<ProcessDefinitionSummaryDto>> ListAsync(
        int tenantId,
        string? searchTerm,
        string? category,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ProcessDefinitions
            .Where(d => d.TenantId == tenantId && d.Status != ProcessDefinitionStatus.Deleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(d =>
                d.Name.ToLower().Contains(term) ||
                d.Key.ToLower().Contains(term) ||
                (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(d => d.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get instance counts
        var definitionIds = items.Select(d => d.Id).ToList();
        var instanceCounts = await _context.ProcessInstances
            .Where(i => definitionIds.Contains(i.ProcessDefinitionId))
            .GroupBy(i => i.ProcessDefinitionId)
            .Select(g => new { DefinitionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DefinitionId, x => x.Count, cancellationToken);

        return new PagedResultDto<ProcessDefinitionSummaryDto>
        {
            Items = items.Select(d => d.ToSummaryDto(
                instanceCounts.GetValueOrDefault(d.Id, 0))).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<ProcessDefinitionDetailDto> UpdateAsync(
        string id,
        int tenantId,
        UpdateProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .Include(d => d.Versions.OrderByDescending(v => v.Version))
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition '{id}' not found.");
        }

        if (request.Name != null)
            definition.Name = request.Name;

        if (request.Description != null)
            definition.Description = request.Description;

        if (request.Category != null)
            definition.Category = request.Category;

        if (request.Tags != null)
            definition.Tags = request.Tags;

        if (request.Status.HasValue)
            definition.Status = request.Status.Value;

        definition.UpdatedBy = userId;
        definition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated process definition {DefinitionId} for tenant {TenantId}",
            id, tenantId);

        var result = definition.ToDetailDto();
        var latestVersion = definition.Versions.FirstOrDefault();
        if (latestVersion != null)
        {
            var model = DeserializeModel(latestVersion.ModelJson);
            result.LatestVersionDetail = latestVersion.ToDetailDto(model);
        }

        return result;
    }

    public async Task DeleteAsync(
        string id,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition '{id}' not found.");
        }

        // Check for active instances
        var hasActiveInstances = await _context.ProcessInstances
            .AnyAsync(i => i.ProcessDefinitionId == id &&
                         i.Status != ProcessInstanceStatus.Completed &&
                         i.Status != ProcessInstanceStatus.Cancelled &&
                         i.Status != ProcessInstanceStatus.Terminated, cancellationToken);

        if (hasActiveInstances)
        {
            throw new InvalidOperationException(
                "Cannot delete process definition with active instances. Please terminate or complete all instances first.");
        }

        // Soft delete
        definition.Status = ProcessDefinitionStatus.Deleted;
        definition.UpdatedBy = userId;
        definition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deleted process definition {DefinitionId} for tenant {TenantId}",
            id, tenantId);
    }

    public async Task<ProcessVersionDetailDto> CreateVersionAsync(
        string definitionId,
        int tenantId,
        CreateProcessVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == definitionId && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition '{definitionId}' not found.");
        }

        // Validate model
        var validation = await ValidateAsync(request.Model, cancellationToken);
        if (!validation.IsValid && !request.SaveAsDraft)
        {
            throw new InvalidOperationException(
                $"Process model validation failed: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
        }

        var now = DateTime.UtcNow;
        var newVersion = definition.LatestVersion + 1;

        var version = new ProcessVersion
        {
            Id = Guid.NewGuid().ToString(),
            ProcessDefinitionId = definitionId,
            Version = newVersion,
            Status = request.SaveAsDraft ? ProcessVersionStatus.Draft : ProcessVersionStatus.Published,
            ModelJson = JsonSerializer.Serialize(request.Model, _jsonOptions),
            ChangeDescription = request.ChangeDescription,
            CreatedBy = userId,
            CreatedAt = now,
            PublishedBy = request.SaveAsDraft ? null : userId,
            PublishedAt = request.SaveAsDraft ? null : now
        };

        definition.LatestVersion = newVersion;
        if (!request.SaveAsDraft)
        {
            definition.PublishedVersion = newVersion;
        }
        definition.UpdatedBy = userId;
        definition.UpdatedAt = now;

        _context.ProcessVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created version {Version} for process definition {DefinitionId}",
            newVersion, definitionId);

        return version.ToDetailDto(request.Model);
    }

    public async Task<ProcessVersionDetailDto?> GetVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var processVersion = await _context.ProcessVersions
            .Include(v => v.ProcessDefinition)
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definitionId &&
                v.Version == version &&
                v.ProcessDefinition.TenantId == tenantId, cancellationToken);

        if (processVersion == null)
            return null;

        var model = DeserializeModel(processVersion.ModelJson);
        return processVersion.ToDetailDto(model);
    }

    public async Task<ProcessVersionDetailDto> UpdateVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        UpdateProcessVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var processVersion = await _context.ProcessVersions
            .Include(v => v.ProcessDefinition)
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definitionId &&
                v.Version == version &&
                v.ProcessDefinition.TenantId == tenantId, cancellationToken);

        if (processVersion == null)
        {
            throw new KeyNotFoundException($"Process version {version} not found for definition '{definitionId}'.");
        }

        if (processVersion.Status != ProcessVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only draft versions can be updated.");
        }

        ProcessModel? model = null;
        if (request.Model != null)
        {
            var validation = await ValidateAsync(request.Model, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Saving draft version with validation warnings for {DefinitionId} v{Version}",
                    definitionId, version);
            }

            processVersion.ModelJson = JsonSerializer.Serialize(request.Model, _jsonOptions);
            model = request.Model;
        }
        else
        {
            model = DeserializeModel(processVersion.ModelJson);
        }

        if (request.ChangeDescription != null)
        {
            processVersion.ChangeDescription = request.ChangeDescription;
        }

        processVersion.ProcessDefinition.UpdatedBy = userId;
        processVersion.ProcessDefinition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated version {Version} for process definition {DefinitionId}",
            version, definitionId);

        return processVersion.ToDetailDto(model);
    }

    public async Task<ProcessVersionDetailDto> PublishVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var processVersion = await _context.ProcessVersions
            .Include(v => v.ProcessDefinition)
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definitionId &&
                v.Version == version &&
                v.ProcessDefinition.TenantId == tenantId, cancellationToken);

        if (processVersion == null)
        {
            throw new KeyNotFoundException($"Process version {version} not found for definition '{definitionId}'.");
        }

        // Validate before publishing
        var model = DeserializeModel(processVersion.ModelJson);
        var validation = await ValidateAsync(model!, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Cannot publish invalid process model: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
        }

        var now = DateTime.UtcNow;
        processVersion.Status = ProcessVersionStatus.Published;
        processVersion.PublishedBy = userId;
        processVersion.PublishedAt = now;

        // Deprecate previously published version
        if (processVersion.ProcessDefinition.PublishedVersion.HasValue &&
            processVersion.ProcessDefinition.PublishedVersion != version)
        {
            var previousVersion = await _context.ProcessVersions
                .FirstOrDefaultAsync(v =>
                    v.ProcessDefinitionId == definitionId &&
                    v.Version == processVersion.ProcessDefinition.PublishedVersion, cancellationToken);

            if (previousVersion != null)
            {
                previousVersion.Status = ProcessVersionStatus.Deprecated;
            }
        }

        processVersion.ProcessDefinition.PublishedVersion = version;
        processVersion.ProcessDefinition.UpdatedBy = userId;
        processVersion.ProcessDefinition.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Published version {Version} for process definition {DefinitionId}",
            version, definitionId);

        return processVersion.ToDetailDto(model);
    }

    public async Task DeprecateVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var processVersion = await _context.ProcessVersions
            .Include(v => v.ProcessDefinition)
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definitionId &&
                v.Version == version &&
                v.ProcessDefinition.TenantId == tenantId, cancellationToken);

        if (processVersion == null)
        {
            throw new KeyNotFoundException($"Process version {version} not found for definition '{definitionId}'.");
        }

        if (processVersion.ProcessDefinition.PublishedVersion == version)
        {
            throw new InvalidOperationException(
                "Cannot deprecate the currently published version. Publish a new version first.");
        }

        processVersion.Status = ProcessVersionStatus.Deprecated;
        processVersion.ProcessDefinition.UpdatedBy = userId;
        processVersion.ProcessDefinition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deprecated version {Version} for process definition {DefinitionId}",
            version, definitionId);
    }

    public Task<ProcessValidationResultDto> ValidateAsync(
        ProcessModel model,
        CancellationToken cancellationToken = default)
    {
        var result = new ProcessValidationResultDto { IsValid = true };
        var errors = new List<ProcessValidationErrorDto>();
        var warnings = new List<ProcessValidationWarningDto>();

        // Check for start event
        var startEvents = model.Activities?.Where(a => a is StartEventDefinition).ToList() ?? new();
        if (startEvents.Count == 0)
        {
            errors.Add(new ProcessValidationErrorDto
            {
                Code = "NO_START_EVENT",
                Message = "Process must have at least one start event."
            });
        }
        else if (startEvents.Count > 1)
        {
            warnings.Add(new ProcessValidationWarningDto
            {
                Code = "MULTIPLE_START_EVENTS",
                Message = "Process has multiple start events. Ensure this is intentional."
            });
        }

        // Check for end event
        var endEvents = model.Activities?.Where(a => a is EndEventDefinition).ToList() ?? new();
        if (endEvents.Count == 0)
        {
            errors.Add(new ProcessValidationErrorDto
            {
                Code = "NO_END_EVENT",
                Message = "Process must have at least one end event."
            });
        }

        // Check all activities have IDs
        var activitiesWithoutIds = model.Activities?.Where(a => string.IsNullOrEmpty(a.Id)).ToList() ?? new();
        foreach (var activity in activitiesWithoutIds)
        {
            errors.Add(new ProcessValidationErrorDto
            {
                Code = "MISSING_ACTIVITY_ID",
                Message = $"Activity of type '{activity.Type}' is missing an ID.",
                ActivityId = activity.Id
            });
        }

        // Check sequence flows reference valid activities
        var activityIds = model.Activities?.Select(a => a.Id).ToHashSet() ?? new HashSet<string?>();
        foreach (var flow in model.SequenceFlows ?? new())
        {
            if (!activityIds.Contains(flow.SourceRef))
            {
                errors.Add(new ProcessValidationErrorDto
                {
                    Code = "INVALID_FLOW_SOURCE",
                    Message = $"Sequence flow '{flow.Id}' references non-existent source activity '{flow.SourceRef}'."
                });
            }

            if (!activityIds.Contains(flow.TargetRef))
            {
                errors.Add(new ProcessValidationErrorDto
                {
                    Code = "INVALID_FLOW_TARGET",
                    Message = $"Sequence flow '{flow.Id}' references non-existent target activity '{flow.TargetRef}'."
                });
            }
        }

        // Check gateway conditions
        foreach (var gateway in model.Activities?.OfType<ExclusiveGatewayDefinition>() ?? Enumerable.Empty<ExclusiveGatewayDefinition>())
        {
            var outgoingFlows = model.SequenceFlows?.Where(f => f.SourceRef == gateway.Id).ToList() ?? new();
            if (outgoingFlows.Count > 1)
            {
                var flowsWithConditions = outgoingFlows.Count(f => !string.IsNullOrEmpty(f.ConditionExpression));
                var flowsWithDefault = outgoingFlows.Count(f => f.IsDefault);

                if (flowsWithConditions + flowsWithDefault < outgoingFlows.Count)
                {
                    errors.Add(new ProcessValidationErrorDto
                    {
                        Code = "GATEWAY_MISSING_CONDITIONS",
                        Message = $"Exclusive gateway '{gateway.Id}' has outgoing flows without conditions.",
                        ActivityId = gateway.Id
                    });
                }

                if (flowsWithDefault > 1)
                {
                    errors.Add(new ProcessValidationErrorDto
                    {
                        Code = "GATEWAY_MULTIPLE_DEFAULTS",
                        Message = $"Exclusive gateway '{gateway.Id}' has multiple default flows.",
                        ActivityId = gateway.Id
                    });
                }
            }
        }

        // Check user tasks have assignments
        foreach (var userTask in model.Activities?.OfType<UserTaskDefinition>() ?? Enumerable.Empty<UserTaskDefinition>())
        {
            var assignment = userTask.Assignment;
            if (assignment == null ||
                (string.IsNullOrEmpty(assignment.AssigneeExpression) &&
                 (assignment.CandidateUserExpressions == null || assignment.CandidateUserExpressions.Count == 0) &&
                 (assignment.CandidateGroupExpressions == null || assignment.CandidateGroupExpressions.Count == 0) &&
                 (assignment.CandidateRoleExpressions == null || assignment.CandidateRoleExpressions.Count == 0)))
            {
                warnings.Add(new ProcessValidationWarningDto
                {
                    Code = "USER_TASK_NO_ASSIGNMENT",
                    Message = $"User task '{userTask.Id}' has no assignment defined. Tasks may become unassigned.",
                    ActivityId = userTask.Id
                });
            }
        }

        // Check service tasks have implementation
        foreach (var serviceTask in model.Activities?.OfType<ServiceTaskDefinition>() ?? Enumerable.Empty<ServiceTaskDefinition>())
        {
            if (string.IsNullOrEmpty(serviceTask.ServiceName) && string.IsNullOrEmpty(serviceTask.HttpEndpoint))
            {
                errors.Add(new ProcessValidationErrorDto
                {
                    Code = "SERVICE_TASK_NO_IMPLEMENTATION",
                    Message = $"Service task '{serviceTask.Id}' has no service name or HTTP endpoint defined.",
                    ActivityId = serviceTask.Id
                });
            }
        }

        result.Errors = errors;
        result.Warnings = warnings;
        result.IsValid = errors.Count == 0;

        return Task.FromResult(result);
    }

    public async Task<ProcessDefinitionExportDto> ExportAsync(
        string id,
        int tenantId,
        int? version = null,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition '{id}' not found.");
        }

        var targetVersion = version ?? definition.PublishedVersion ?? definition.LatestVersion;
        var processVersion = await _context.ProcessVersions
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == id && v.Version == targetVersion, cancellationToken);

        if (processVersion == null)
        {
            throw new KeyNotFoundException($"Process version {targetVersion} not found.");
        }

        var model = DeserializeModel(processVersion.ModelJson) ?? new ProcessModel();
        return definition.ToExportDto(model);
    }

    public async Task<ProcessDefinitionDetailDto> ImportAsync(
        int tenantId,
        ImportProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ProcessDefinitionExportDto? exported;
        try
        {
            exported = JsonSerializer.Deserialize<ProcessDefinitionExportDto>(request.Json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid import format.", ex);
        }

        if (exported == null)
        {
            throw new InvalidOperationException("Import data is empty.");
        }

        var key = request.KeyOverride ?? exported.Key;

        // Check if key exists
        var existing = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Key == key, cancellationToken);

        if (existing != null && !request.CreateNew)
        {
            // Create new version
            var createVersionRequest = new CreateProcessVersionRequest
            {
                Model = exported.Model,
                ChangeDescription = "Imported version",
                SaveAsDraft = true
            };

            await CreateVersionAsync(existing.Id, tenantId, createVersionRequest, userId, cancellationToken);
            return (await GetByIdAsync(existing.Id, tenantId, cancellationToken))!;
        }

        // Create new definition
        var finalKey = key;
        if (existing != null && request.CreateNew)
        {
            // Generate unique key
            var counter = 1;
            while (await _context.ProcessDefinitions.AnyAsync(
                d => d.TenantId == tenantId && d.Key == finalKey, cancellationToken))
            {
                finalKey = $"{key}-{counter++}";
            }
        }

        var createRequest = new CreateProcessDefinitionRequest
        {
            Key = finalKey,
            Name = exported.Name,
            Description = exported.Description,
            Category = exported.Category,
            Tags = exported.Tags,
            Model = exported.Model,
            SaveAsDraft = true
        };

        return await CreateAsync(tenantId, createRequest, userId, cancellationToken);
    }

    public async Task<ProcessDefinitionDetailDto> CloneAsync(
        string id,
        int tenantId,
        CloneProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Validate new key uniqueness
        var keyExists = await _context.ProcessDefinitions
            .AnyAsync(d => d.TenantId == tenantId && d.Key == request.NewKey, cancellationToken);

        if (keyExists)
        {
            throw new InvalidOperationException($"A process definition with key '{request.NewKey}' already exists.");
        }

        var source = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, cancellationToken);

        if (source == null)
        {
            throw new KeyNotFoundException($"Process definition '{id}' not found.");
        }

        // Get latest version model
        var sourceVersion = await _context.ProcessVersions
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == id && v.Version == source.LatestVersion, cancellationToken);

        var model = sourceVersion != null ? DeserializeModel(sourceVersion.ModelJson) : new ProcessModel();

        var createRequest = new CreateProcessDefinitionRequest
        {
            Key = request.NewKey,
            Name = request.NewName,
            Description = source.Description,
            Category = source.Category,
            Tags = new List<string>(source.Tags),
            Model = model,
            SaveAsDraft = true
        };

        var cloned = await CreateAsync(tenantId, createRequest, userId, cancellationToken);

        _logger.LogInformation(
            "Cloned process definition {SourceId} to {ClonedId} with key {NewKey}",
            id, cloned.Id, request.NewKey);

        return cloned;
    }

    public async Task<IList<string>> GetCategoriesAsync(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProcessDefinitions
            .Where(d => d.TenantId == tenantId &&
                       d.Status != ProcessDefinitionStatus.Deleted &&
                       d.Category != null)
            .Select(d => d.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    private ProcessModel? DeserializeModel(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}")
            return new ProcessModel();

        try
        {
            return JsonSerializer.Deserialize<ProcessModel>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize process model");
            return new ProcessModel();
        }
    }
}
