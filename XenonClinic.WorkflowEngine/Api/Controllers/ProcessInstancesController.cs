namespace XenonClinic.WorkflowEngine.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// API controller for managing process instances.
/// </summary>
[ApiController]
[Route("api/workflow/instances")]
[Authorize]
public class ProcessInstancesController : ControllerBase
{
    private readonly IProcessExecutionService _service;
    private readonly ILogger<ProcessInstancesController> _logger;

    public ProcessInstancesController(
        IProcessExecutionService service,
        ILogger<ProcessInstancesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new process instance.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProcessInstanceDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> StartProcess(
        [FromBody] StartProcessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.StartProcessAsync(request, tenantId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetInstance), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a process instance by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProcessInstanceDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetInstance(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetInstanceAsync(id, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Process instance '{id}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Lists process instances with filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProcessInstanceSummaryDto>), 200)]
    public async Task<IActionResult> ListInstances(
        [FromQuery] ProcessInstanceQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.ListInstancesAsync(query, tenantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets process instance variables.
    /// </summary>
    [HttpGet("{id:guid}/variables")]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVariables(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetVariablesAsync(id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sets process instance variables.
    /// </summary>
    [HttpPut("{id:guid}/variables")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetVariables(
        Guid id,
        [FromBody] Dictionary<string, object>? variables,
        CancellationToken cancellationToken = default)
    {
        if (variables == null)
        {
            return BadRequest(new { message = "Variables are required" });
        }

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            await _service.SetVariablesAsync(id, variables, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Signals an event to a process instance.
    /// </summary>
    [HttpPost("{id:guid}/signal")]
    [ProducesResponseType(typeof(ProcessInstanceDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Signal(
        Guid id,
        [FromQuery] string signalName,
        [FromBody] Dictionary<string, object>? variables,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(signalName))
        {
            return BadRequest(new { message = "Signal name is required" });
        }

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.SignalAsync(id, signalName, variables, tenantId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a process instance.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            await _service.CancelAsync(id, reason, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Suspends a process instance.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Suspend(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            await _service.SuspendAsync(id, reason, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Resumes a suspended process instance.
    /// </summary>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(ProcessInstanceDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Resume(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.ResumeAsync(id, tenantId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retries a failed activity.
    /// </summary>
    [HttpPost("{id:guid}/activities/{activityInstanceId}/retry")]
    [ProducesResponseType(typeof(ProcessInstanceDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RetryActivity(
        Guid id,
        string activityInstanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.RetryActivityAsync(id, activityInstanceId, tenantId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the execution history of a process instance.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IList<ActivityExecutionDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetHistoryAsync(id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        if (claim != null && int.TryParse(claim.Value, out var tenantId))
            return tenantId;
        return 1;
    }

    private string GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim?.Value ?? "system";
    }
}
