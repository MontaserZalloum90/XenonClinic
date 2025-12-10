namespace Xenon.Platform.Api.Controllers.Workflow;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// API controller for workflow execution management.
/// </summary>
[ApiController]
[Route("api/v1/workflow/execution")]
[Authorize]
public class WorkflowExecutionController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ILogger<WorkflowExecutionController> _logger;

    public WorkflowExecutionController(
        IWorkflowEngine workflowEngine,
        ILogger<WorkflowExecutionController> logger)
    {
        _workflowEngine = workflowEngine;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new workflow instance
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(WorkflowExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowExecutionResult>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        try
        {
            var result = await _workflowEngine.StartNewAsync(
                request.WorkflowId,
                request.Input,
                new WorkflowInstanceOptions
                {
                    Name = request.Name,
                    CorrelationId = request.CorrelationId,
                    Priority = request.Priority,
                    ScheduledStartTime = request.ScheduledStartTime,
                    Metadata = request.Metadata
                });

            _logger.LogInformation("Workflow started: {InstanceId} for {WorkflowId}",
                result.InstanceId, request.WorkflowId);

            return Ok(result);
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (WorkflowValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a workflow instance by ID
    /// </summary>
    [HttpGet("instances/{instanceId:guid}")]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowInstanceDto>> GetInstance(Guid instanceId)
    {
        var instance = await _workflowEngine.GetInstanceAsync(instanceId);
        if (instance == null)
        {
            return NotFound(new { error = "Instance not found", instanceId });
        }

        return Ok(MapToDto(instance));
    }

    /// <summary>
    /// Lists workflow instances with filtering
    /// </summary>
    [HttpGet("instances")]
    [ProducesResponseType(typeof(WorkflowInstanceListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowInstanceListResult>> ListInstances(
        [FromQuery] string? workflowId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? correlationId = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool orderDesc = true)
    {
        var statuses = new List<WorkflowStatus>();
        if (!string.IsNullOrEmpty(status))
        {
            foreach (var s in status.Split(','))
            {
                if (Enum.TryParse<WorkflowStatus>(s.Trim(), true, out var parsed))
                {
                    statuses.Add(parsed);
                }
            }
        }

        var result = await _workflowEngine.QueryInstancesAsync(new WorkflowInstanceQuery
        {
            WorkflowId = workflowId,
            Statuses = statuses.Count > 0 ? statuses : null,
            CorrelationId = correlationId,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            PageNumber = page,
            PageSize = pageSize,
            OrderDescending = orderDesc
        });

        return Ok(new WorkflowInstanceListResult
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        });
    }

    /// <summary>
    /// Resumes a suspended workflow instance
    /// </summary>
    [HttpPost("instances/{instanceId:guid}/resume")]
    [ProducesResponseType(typeof(WorkflowExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowExecutionResult>> ResumeWorkflow(
        Guid instanceId,
        [FromBody] ResumeWorkflowRequest request)
    {
        try
        {
            var result = await _workflowEngine.ResumeAsync(instanceId, request.BookmarkName, request.Input);

            _logger.LogInformation("Workflow resumed: {InstanceId} at bookmark {Bookmark}",
                instanceId, request.BookmarkName);

            return Ok(result);
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (WorkflowBookmarkNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (WorkflowInvalidStateException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a workflow instance
    /// </summary>
    [HttpPost("instances/{instanceId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelWorkflow(Guid instanceId, [FromBody] CancelWorkflowRequest? request = null)
    {
        try
        {
            await _workflowEngine.CancelAsync(instanceId, request?.Reason);

            _logger.LogInformation("Workflow cancelled: {InstanceId}", instanceId);

            return NoContent();
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (WorkflowInvalidStateException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Terminates a workflow instance immediately
    /// </summary>
    [HttpPost("instances/{instanceId:guid}/terminate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateWorkflow(Guid instanceId, [FromBody] TerminateWorkflowRequest? request = null)
    {
        try
        {
            await _workflowEngine.TerminateAsync(instanceId, request?.Reason);

            _logger.LogWarning("Workflow terminated: {InstanceId}", instanceId);

            return NoContent();
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retries a faulted workflow instance
    /// </summary>
    [HttpPost("instances/{instanceId:guid}/retry")]
    [ProducesResponseType(typeof(WorkflowExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowExecutionResult>> RetryWorkflow(Guid instanceId)
    {
        try
        {
            var result = await _workflowEngine.RetryAsync(instanceId);

            _logger.LogInformation("Workflow retried: {InstanceId}", instanceId);

            return Ok(result);
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (WorkflowInvalidStateException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sends a signal to a workflow instance
    /// </summary>
    [HttpPost("instances/{instanceId:guid}/signal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendSignal(Guid instanceId, [FromBody] SendSignalRequest request)
    {
        try
        {
            await _workflowEngine.SignalAsync(instanceId, request.SignalName, request.Data);

            _logger.LogInformation("Signal sent to workflow {InstanceId}: {SignalName}",
                instanceId, request.SignalName);

            return NoContent();
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Broadcasts a signal to all matching workflow instances
    /// </summary>
    [HttpPost("signal/broadcast")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BroadcastSignal([FromBody] BroadcastSignalRequest request)
    {
        await _workflowEngine.BroadcastSignalAsync(request.SignalName, request.Data, request.WorkflowId);

        _logger.LogInformation("Signal broadcasted: {SignalName}", request.SignalName);

        return NoContent();
    }

    /// <summary>
    /// Triggers event-based workflows
    /// </summary>
    [HttpPost("events/trigger")]
    [ProducesResponseType(typeof(IList<WorkflowExecutionResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<WorkflowExecutionResult>>> TriggerEvent([FromBody] TriggerEventRequest request)
    {
        var results = await _workflowEngine.TriggerEventAsync(request.EventName, request.EventData);

        _logger.LogInformation("Event triggered: {EventName}, {Count} workflows started",
            request.EventName, results.Count);

        return Ok(results);
    }

    /// <summary>
    /// Gets execution history for a workflow instance
    /// </summary>
    [HttpGet("instances/{instanceId:guid}/history")]
    [ProducesResponseType(typeof(IList<WorkflowExecutionRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<WorkflowExecutionRecord>>> GetHistory(Guid instanceId)
    {
        var history = await _workflowEngine.GetHistoryAsync(instanceId);
        return Ok(history);
    }

    private static WorkflowInstanceDto MapToDto(IWorkflowInstance instance) => new()
    {
        Id = instance.Id,
        WorkflowId = instance.WorkflowId,
        Version = instance.Version,
        Name = instance.Name,
        Status = instance.Status.ToString(),
        CorrelationId = instance.CorrelationId,
        Priority = instance.Priority,
        CreatedAt = instance.CreatedAt,
        StartedAt = instance.StartedAt,
        CompletedAt = instance.CompletedAt,
        CurrentActivityId = instance.CurrentActivityId,
        Bookmarks = instance.Bookmarks.Select(b => new BookmarkDto
        {
            Name = b.Name,
            ActivityId = b.ActivityId,
            CreatedAt = b.CreatedAt
        }).ToList(),
        Error = instance.Error != null ? new WorkflowErrorDto
        {
            Code = instance.Error.Code,
            Message = instance.Error.Message,
            ActivityId = instance.Error.ActivityId
        } : null
    };
}

#region Request/Response Models

public class StartWorkflowRequest
{
    public required string WorkflowId { get; set; }
    public string? Name { get; set; }
    public string? CorrelationId { get; set; }
    public int Priority { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public Dictionary<string, object?>? Input { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
}

public class ResumeWorkflowRequest
{
    public required string BookmarkName { get; set; }
    public Dictionary<string, object?>? Input { get; set; }
}

public class CancelWorkflowRequest
{
    public string? Reason { get; set; }
}

public class TerminateWorkflowRequest
{
    public string? Reason { get; set; }
}

public class SendSignalRequest
{
    public required string SignalName { get; set; }
    public object? Data { get; set; }
}

public class BroadcastSignalRequest
{
    public required string SignalName { get; set; }
    public object? Data { get; set; }
    public string? WorkflowId { get; set; }
}

public class TriggerEventRequest
{
    public required string EventName { get; set; }
    public object? EventData { get; set; }
}

public class WorkflowInstanceDto
{
    public Guid Id { get; init; }
    public string WorkflowId { get; init; } = string.Empty;
    public int Version { get; init; }
    public string? Name { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
    public int Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? CurrentActivityId { get; init; }
    public List<BookmarkDto> Bookmarks { get; init; } = new();
    public WorkflowErrorDto? Error { get; init; }
}

public class BookmarkDto
{
    public string Name { get; init; } = string.Empty;
    public string? ActivityId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class WorkflowErrorDto
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActivityId { get; init; }
}

public class WorkflowInstanceListResult
{
    public List<WorkflowInstanceDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

#endregion
