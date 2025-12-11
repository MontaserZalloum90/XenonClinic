namespace XenonClinic.WorkflowEngine.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// API controller for managing human tasks.
/// </summary>
[ApiController]
[Route("api/workflow/tasks")]
[Authorize]
public class HumanTasksController : ControllerBase
{
    private readonly IHumanTaskService _service;
    private readonly ILogger<HumanTasksController> _logger;

    public HumanTasksController(
        IHumanTaskService service,
        ILogger<HumanTasksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTask(string id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetTaskAsync(id, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Task '{id}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Lists tasks with filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<HumanTaskListDto>), 200)]
    public async Task<IActionResult> ListTasks(
        [FromQuery] HumanTaskQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.ListTasksAsync(query, tenantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the task inbox for the current user.
    /// </summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(TaskInboxDto), 200)]
    public async Task<IActionResult> GetInbox(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userGroups = GetUserGroups();
        var userRoles = GetUserRoles();

        var result = await _service.GetTaskInboxAsync(userId, userGroups, userRoles, tenantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Claims a task.
    /// </summary>
    [HttpPost("{id}/claim")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ClaimTask(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.ClaimTaskAsync(id, tenantId, userId, cancellationToken);
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
    /// Unclaims a task (returns to pool).
    /// </summary>
    [HttpPost("{id}/unclaim")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UnclaimTask(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UnclaimTaskAsync(id, tenantId, userId, cancellationToken);
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
    /// Assigns a task to a user.
    /// </summary>
    [HttpPost("{id}/assign")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AssignTask(
        string id,
        [FromQuery] string assigneeUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var performedBy = GetUserId();
            var result = await _service.AssignTaskAsync(id, assigneeUserId, tenantId, performedBy, cancellationToken);
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
    /// Delegates a task to another user.
    /// </summary>
    [HttpPost("{id}/delegate")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DelegateTask(
        string id,
        [FromQuery] string delegateUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var performedBy = GetUserId();
            var result = await _service.DelegateTaskAsync(id, delegateUserId, tenantId, performedBy, cancellationToken);
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
    /// Completes a task with output variables.
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CompleteTask(
        string id,
        [FromBody] CompleteTaskRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CompleteTaskAsync(
                id,
                request?.Variables,
                request?.Action,
                tenantId,
                userId,
                cancellationToken);
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
    /// Gets the form definition for a task.
    /// </summary>
    [HttpGet("{id}/form")]
    [ProducesResponseType(typeof(TaskFormDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaskForm(string id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetTaskFormAsync(id, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Task '{id}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Adds a comment to a task.
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(TaskCommentDto), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddComment(
        string id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.AddCommentAsync(id, request.Content, tenantId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetComments), new { id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets comments for a task.
    /// </summary>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(IList<TaskCommentDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetComments(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetCommentsAsync(id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds an attachment to a task.
    /// </summary>
    [HttpPost("{id}/attachments")]
    [ProducesResponseType(typeof(TaskAttachmentDto), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddAttachment(
        string id,
        [FromBody] AddAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.AddAttachmentAsync(
                id, request.FileName, request.ContentType, request.Url, tenantId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetAttachments), new { id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets attachments for a task.
    /// </summary>
    [HttpGet("{id}/attachments")]
    [ProducesResponseType(typeof(IList<TaskAttachmentDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAttachments(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetAttachmentsAsync(id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the action history for a task.
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IList<TaskActionDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaskHistory(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetTaskHistoryAsync(id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates task priority.
    /// </summary>
    [HttpPatch("{id}/priority")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdatePriority(
        string id,
        [FromBody] UpdatePriorityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdatePriorityAsync(id, request.Priority, tenantId, userId, cancellationToken);
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
    /// Updates task due date.
    /// </summary>
    [HttpPatch("{id}/due-date")]
    [ProducesResponseType(typeof(HumanTaskDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateDueDate(
        string id,
        [FromBody] UpdateDueDateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateDueDateAsync(id, request.DueDate, tenantId, userId, cancellationToken);
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

    private List<string> GetUserGroups()
    {
        var claims = User.FindAll("groups");
        return claims.Select(c => c.Value).ToList();
    }

    private List<string> GetUserRoles()
    {
        var claims = User.FindAll("roles");
        return claims.Select(c => c.Value).ToList();
    }
}

#region Request DTOs

public class CompleteTaskRequest
{
    public Dictionary<string, object>? Variables { get; set; }
    public string? Action { get; set; }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class AddAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class UpdatePriorityRequest
{
    public TaskPriority Priority { get; set; }
}

public class UpdateDueDateRequest
{
    public DateTime? DueDate { get; set; }
}

#endregion
