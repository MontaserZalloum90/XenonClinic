using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for workflow management and execution.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class WorkflowsController : BaseApiController
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowDefinitionStore _definitionStore;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowEngine workflowEngine,
        IWorkflowDefinitionStore definitionStore,
        ICurrentUserContext userContext,
        ILogger<WorkflowsController> logger)
    {
        _workflowEngine = workflowEngine;
        _definitionStore = definitionStore;
        _userContext = userContext;
        _logger = logger;
    }

    #region Workflow Definitions

    /// <summary>
    /// Lists workflow definitions.
    /// </summary>
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<WorkflowDefinitionListDto>>), 200)]
    public async Task<IActionResult> GetDefinitions(
        [FromQuery] WorkflowDefinitionListRequestDto request)
    {
        var query = new WorkflowDefinitionQuery
        {
            SearchTerm = request.SearchTerm,
            Category = request.Category,
            Tags = request.Tags,
            IsActive = request.IsActive,
            IsDraft = request.IsDraft,
            TenantId = _userContext.TenantId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.SortBy,
            OrderDescending = request.SortDescending
        };

        var result = await _definitionStore.ListAsync(query);
        var items = result.Items.Select(MapToDefinitionListDto).ToList();

        return ApiPaginated(items, result.TotalCount, result.PageNumber, result.PageSize);
    }

    /// <summary>
    /// Gets a workflow definition by ID.
    /// </summary>
    [HttpGet("definitions/{id}")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDefinition(
        string id,
        [FromQuery] int? version = null)
    {
        var definition = await _definitionStore.GetAsync(id, version);
        if (definition == null)
            return ApiNotFound(WorkflowValidationMessages.WorkflowNotFound);

        // Check tenant access
        if (definition.TenantId.HasValue && definition.TenantId != _userContext.TenantId)
            return ApiForbidden(WorkflowValidationMessages.UnauthorizedAccess);

        return ApiOk(MapToDefinitionDto(definition));
    }

    /// <summary>
    /// Gets all versions of a workflow definition.
    /// </summary>
    [HttpGet("definitions/{id}/versions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WorkflowDefinitionListDto>>), 200)]
    public async Task<IActionResult> GetDefinitionVersions(string id)
    {
        var versions = await _definitionStore.GetVersionsAsync(id);
        var dtos = versions.Select(MapToDefinitionListDto);
        return ApiOk(dtos);
    }

    /// <summary>
    /// Publishes a workflow definition version.
    /// </summary>
    [HttpPost("definitions/{id}/publish")]
    [Authorize(Roles = "Admin,WorkflowAdmin")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> PublishDefinition(string id, [FromQuery] int version)
    {
        await _definitionStore.PublishAsync(id, version);
        _logger.LogInformation("Workflow definition published: {WorkflowId} v{Version}", id, version);
        return ApiOk("Workflow published successfully");
    }

    /// <summary>
    /// Unpublishes a workflow definition version.
    /// </summary>
    [HttpPost("definitions/{id}/unpublish")]
    [Authorize(Roles = "Admin,WorkflowAdmin")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> UnpublishDefinition(string id, [FromQuery] int version)
    {
        await _definitionStore.UnpublishAsync(id, version);
        _logger.LogInformation("Workflow definition unpublished: {WorkflowId} v{Version}", id, version);
        return ApiOk("Workflow unpublished successfully");
    }

    #endregion

    #region Workflow Instances

    /// <summary>
    /// Lists workflow instances.
    /// </summary>
    [HttpGet("instances")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<WorkflowInstanceListDto>>), 200)]
    public async Task<IActionResult> GetInstances(
        [FromQuery] WorkflowInstanceListRequestDto request)
    {
        var statuses = request.Statuses?
            .Select(s => Enum.TryParse<WorkflowStatus>(s, true, out var status) ? status : (WorkflowStatus?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        var query = new WorkflowInstanceQuery
        {
            WorkflowId = request.WorkflowId,
            Statuses = statuses,
            CorrelationId = request.CorrelationId,
            TenantId = _userContext.TenantId,
            CreatedAfter = request.CreatedAfter,
            CreatedBefore = request.CreatedBefore,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.SortBy,
            OrderDescending = request.SortDescending
        };

        var result = await _workflowEngine.QueryInstancesAsync(query);
        var items = result.Items.Select(MapToInstanceListDto).ToList();

        return ApiPaginated(items, result.TotalCount, result.PageNumber, result.PageSize);
    }

    /// <summary>
    /// Gets a workflow instance by ID.
    /// </summary>
    [HttpGet("instances/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetInstance(Guid id)
    {
        var instance = await _workflowEngine.GetInstanceAsync(id);
        if (instance == null)
            return ApiNotFound(WorkflowValidationMessages.InstanceNotFound);

        // Check tenant access
        if (instance.TenantId.HasValue && instance.TenantId != _userContext.TenantId)
            return ApiForbidden(WorkflowValidationMessages.UnauthorizedAccess);

        return ApiOk(MapToInstanceDto(instance));
    }

    /// <summary>
    /// Gets execution history for a workflow instance.
    /// </summary>
    [HttpGet("instances/{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WorkflowExecutionRecordDto>>), 200)]
    public async Task<IActionResult> GetInstanceHistory(Guid id)
    {
        var history = await _workflowEngine.GetHistoryAsync(id);
        var dtos = history.Select(MapToExecutionRecordDto);
        return ApiOk(dtos);
    }

    #endregion

    #region Workflow Execution

    /// <summary>
    /// Starts a new workflow instance.
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowExecutionResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> StartWorkflow(
        [FromBody] StartWorkflowRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.WorkflowId))
            return ApiBadRequest(WorkflowValidationMessages.WorkflowIdRequired);

        try
        {
            var options = new WorkflowInstanceOptions
            {
                TenantId = _userContext.TenantId,
                UserId = _userContext.UserId,
                Name = request.Name,
                Priority = request.Priority ?? 0,
                CorrelationId = request.CorrelationId,
                ScheduledStartTime = request.ScheduledStartTime,
                Version = request.Version,
                Metadata = request.Metadata
            };

            var result = await _workflowEngine.StartNewAsync(request.WorkflowId, request.Input, options);

            _logger.LogInformation("Workflow started: {InstanceId} for {WorkflowId}",
                result.InstanceId, request.WorkflowId);

            return ApiOk(MapToExecutionResultDto(result));
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
        catch (WorkflowValidationException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Resumes a suspended workflow instance.
    /// </summary>
    [HttpPost("instances/{id:guid}/resume")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowExecutionResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ResumeWorkflow(
        Guid id,
        [FromBody] ResumeWorkflowRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BookmarkName))
            return ApiBadRequest(WorkflowValidationMessages.BookmarkNameRequired);

        try
        {
            var result = await _workflowEngine.ResumeAsync(id, request.BookmarkName, request.Input);

            _logger.LogInformation("Workflow resumed: {InstanceId} at bookmark {Bookmark}",
                id, request.BookmarkName);

            return ApiOk(MapToExecutionResultDto(result));
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
        catch (WorkflowBookmarkNotFoundException ex)
        {
            return ApiBadRequest(ex.Message);
        }
        catch (WorkflowInvalidStateException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancels a running workflow instance.
    /// </summary>
    [HttpPost("instances/{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CancelWorkflow(
        Guid id,
        [FromBody] CancelWorkflowRequestDto? request = null)
    {
        try
        {
            await _workflowEngine.CancelAsync(id, request?.Reason);

            _logger.LogInformation("Workflow cancelled: {InstanceId}, Reason: {Reason}",
                id, request?.Reason ?? "Not specified");

            return ApiOk("Workflow cancelled successfully");
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
        catch (WorkflowInvalidStateException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Terminates a workflow instance.
    /// </summary>
    [HttpPost("instances/{id:guid}/terminate")]
    [Authorize(Roles = "Admin,WorkflowAdmin")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> TerminateWorkflow(
        Guid id,
        [FromBody] CancelWorkflowRequestDto? request = null)
    {
        try
        {
            await _workflowEngine.TerminateAsync(id, request?.Reason);

            _logger.LogWarning("Workflow terminated: {InstanceId}, Reason: {Reason}",
                id, request?.Reason ?? "Not specified");

            return ApiOk("Workflow terminated");
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retries a faulted workflow instance.
    /// </summary>
    [HttpPost("instances/{id:guid}/retry")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowExecutionResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> RetryWorkflow(Guid id)
    {
        try
        {
            var result = await _workflowEngine.RetryAsync(id);

            _logger.LogInformation("Workflow retried: {InstanceId}", id);

            return ApiOk(MapToExecutionResultDto(result));
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
        catch (WorkflowInvalidStateException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    #endregion

    #region Signals and Events

    /// <summary>
    /// Sends a signal to a workflow instance.
    /// </summary>
    [HttpPost("instances/{id:guid}/signal")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> SendSignal(
        Guid id,
        [FromBody] SendSignalRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SignalName))
            return ApiBadRequest(WorkflowValidationMessages.SignalNameRequired);

        try
        {
            await _workflowEngine.SignalAsync(id, request.SignalName, request.Data as IDictionary<string, object?>);

            _logger.LogInformation("Signal sent to workflow: {InstanceId}, Signal: {SignalName}",
                id, request.SignalName);

            return ApiOk("Signal sent successfully");
        }
        catch (WorkflowNotFoundException ex)
        {
            return ApiNotFound(ex.Message);
        }
    }

    /// <summary>
    /// Broadcasts a signal to all matching workflow instances.
    /// </summary>
    [HttpPost("broadcast-signal")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> BroadcastSignal([FromBody] BroadcastSignalRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SignalName))
            return ApiBadRequest(WorkflowValidationMessages.SignalNameRequired);

        await _workflowEngine.BroadcastSignalAsync(request.SignalName, request.Data as IDictionary<string, object?>, request.WorkflowId);

        _logger.LogInformation("Signal broadcast: {SignalName}, WorkflowId: {WorkflowId}",
            request.SignalName, request.WorkflowId ?? "All");

        return ApiOk("Signal broadcast successfully");
    }

    /// <summary>
    /// Triggers event-based workflows.
    /// </summary>
    [HttpPost("trigger-event")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WorkflowExecutionResultDto>>), 200)]
    public async Task<IActionResult> TriggerEvent(
        [FromBody] TriggerEventRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
            return ApiBadRequest(WorkflowValidationMessages.EventNameRequired);

        var results = await _workflowEngine.TriggerEventAsync(request.EventName, request.EventData as IDictionary<string, object?>);

        _logger.LogInformation("Event triggered: {EventName}, Workflows started: {Count}",
            request.EventName, results.Count);

        var dtos = results.Select(MapToExecutionResultDto);
        return ApiOk(dtos);
    }

    #endregion

    #region Mapping Methods

    private static WorkflowDefinitionListDto MapToDefinitionListDto(IWorkflowDefinition definition)
    {
        return new WorkflowDefinitionListDto
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Category = definition.Category,
            Version = definition.Version,
            IsActive = definition.IsActive,
            IsDraft = definition.IsDraft,
            Tags = definition.Tags?.ToList() ?? new List<string>(),
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.ModifiedAt
        };
    }

    private static WorkflowDefinitionDto MapToDefinitionDto(IWorkflowDefinition definition)
    {
        return new WorkflowDefinitionDto
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Category = definition.Category,
            Version = definition.Version,
            IsActive = definition.IsActive,
            IsDraft = definition.IsDraft,
            Tags = definition.Tags?.ToList() ?? new List<string>(),
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.ModifiedAt,
            TenantId = definition.TenantId,
            InputParameters = definition.InputParameters?.Select(p => new WorkflowParameterDto
            {
                Name = p.Name,
                Type = p.Type ?? "string",
                Description = p.Description,
                IsRequired = p.IsRequired,
                DefaultValue = p.DefaultValue,
                ValidationRule = p.Schema
            }).ToList() ?? new List<WorkflowParameterDto>(),
            OutputParameters = definition.OutputParameters?.Select(p => new WorkflowParameterDto
            {
                Name = p.Name,
                Type = p.Type ?? "string",
                Description = p.Description,
                DefaultValue = p.DefaultValue
            }).ToList() ?? new List<WorkflowParameterDto>(),
            Variables = definition.Variables?.Select(v => new WorkflowVariableDto
            {
                Name = v.Name,
                Type = v.Type ?? "string",
                DefaultValue = v.DefaultValue,
                Scope = v.Scope.ToString()
            }).ToList() ?? new List<WorkflowVariableDto>(),
            Triggers = definition.Triggers?.Select(t => new WorkflowTriggerDto
            {
                Name = t.Name,
                Type = t.Type.ToString(),
                IsEnabled = t.IsEnabled,
                Config = t.Configuration?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>()
            }).ToList() ?? new List<WorkflowTriggerDto>()
        };
    }

    private static WorkflowInstanceListDto MapToInstanceListDto(IWorkflowInstance instance)
    {
        return new WorkflowInstanceListDto
        {
            Id = instance.Id,
            WorkflowId = instance.WorkflowId,
            Name = instance.Name,
            Version = instance.Version,
            Status = instance.Status.ToString(),
            Priority = instance.Priority,
            CorrelationId = instance.CorrelationId,
            CreatedAt = instance.CreatedAt,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            CreatedBy = instance.CreatedBy
        };
    }

    private static WorkflowInstanceDto MapToInstanceDto(IWorkflowInstance instance)
    {
        return new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowId = instance.WorkflowId,
            Name = instance.Name,
            Version = instance.Version,
            Status = instance.Status.ToString(),
            Priority = instance.Priority,
            CorrelationId = instance.CorrelationId,
            CreatedAt = instance.CreatedAt,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            CreatedBy = instance.CreatedBy,
            TenantId = instance.TenantId,
            CurrentActivityId = instance.CurrentActivityId,
            CompletedActivityIds = instance.CompletedActivityIds?.ToList() ?? new List<string>(),
            Input = instance.Input?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>(),
            Output = instance.Output?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>(),
            Variables = instance.Variables?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>(),
            ScheduledStartTime = instance.ScheduledStartTime,
            FaultCount = instance.FaultCount,
            Error = instance.Error != null ? new WorkflowErrorDto
            {
                Code = instance.Error.Code,
                Message = instance.Error.Message,
                ActivityId = instance.Error.ActivityId
            } : null,
            Bookmarks = instance.Bookmarks?.Select(b => new WorkflowBookmarkDto
            {
                Name = b.Name,
                ActivityId = b.ActivityId,
                CreatedAt = b.CreatedAt
            }).ToList() ?? new List<WorkflowBookmarkDto>(),
            AuditEntries = instance.AuditEntries?.Select(a => new WorkflowAuditEntryDto
            {
                Timestamp = a.Timestamp,
                Action = a.Action,
                UserId = a.UserId,
                Details = a.Details
            }).ToList() ?? new List<WorkflowAuditEntryDto>()
        };
    }

    private static WorkflowExecutionResultDto MapToExecutionResultDto(WorkflowExecutionResult result)
    {
        return new WorkflowExecutionResultDto
        {
            InstanceId = result.InstanceId,
            Status = result.Status.ToString(),
            Output = result.Output?.ToDictionary(k => k.Key, v => v.Value),
            DurationMs = result.Duration.TotalMilliseconds,
            ActivitiesExecuted = result.ActivitiesExecuted,
            IsCompleted = result.IsCompleted,
            IsRunning = result.IsRunning,
            Error = result.Error != null ? new WorkflowErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                ActivityId = result.Error.ActivityId
            } : null,
            Bookmarks = result.Bookmarks?.Select(b => new WorkflowBookmarkDto
            {
                Name = b.Name,
                ActivityId = b.ActivityId,
                CreatedAt = b.CreatedAt
            }).ToList()
        };
    }

    private static WorkflowExecutionRecordDto MapToExecutionRecordDto(WorkflowExecutionRecord record)
    {
        return new WorkflowExecutionRecordDto
        {
            Id = record.Id,
            ActivityId = record.ActivityId ?? string.Empty,
            ActivityName = record.ActivityName ?? string.Empty,
            ActivityType = record.ActivityType ?? string.Empty,
            RecordType = record.Type.ToString(),
            Timestamp = record.Timestamp,
            DurationMs = record.Duration?.TotalMilliseconds,
            Output = record.Output?.ToDictionary(k => k.Key, v => v.Value),
            Error = record.Error != null ? new WorkflowActivityErrorDto
            {
                Code = record.Error.Code,
                Message = record.Error.Message
            } : null
        };
    }

    #endregion
}
