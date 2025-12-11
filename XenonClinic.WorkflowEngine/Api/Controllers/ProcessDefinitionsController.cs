namespace XenonClinic.WorkflowEngine.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Domain.Models;

/// <summary>
/// API controller for managing process definitions.
/// </summary>
[ApiController]
[Route("api/workflow/definitions")]
[Authorize]
public class ProcessDefinitionsController : ControllerBase
{
    private readonly IProcessDefinitionService _service;
    private readonly ILogger<ProcessDefinitionsController> _logger;

    public ProcessDefinitionsController(
        IProcessDefinitionService service,
        ILogger<ProcessDefinitionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Lists process definitions with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProcessDefinitionSummaryDto>), 200)]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.ListAsync(tenantId, search, category, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a process definition by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByIdAsync(id, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Process definition '{id}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Gets a process definition by key.
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByKeyAsync(key, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Process definition with key '{key}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new process definition.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProcessDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CreateAsync(tenantId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a process definition metadata.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateProcessDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateAsync(id, tenantId, request, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process definition '{id}' not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a process definition (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            await _service.DeleteAsync(id, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process definition '{id}' not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new version of a process definition.
    /// </summary>
    [HttpPost("{id}/versions")]
    [ProducesResponseType(typeof(ProcessVersionDetailDto), 201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> CreateVersion(
        string id,
        [FromBody] CreateProcessVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CreateVersionAsync(id, tenantId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetVersion), new { id, version = result.Version }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process definition '{id}' not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific version of a process definition.
    /// </summary>
    [HttpGet("{id}/versions/{version:int}")]
    [ProducesResponseType(typeof(ProcessVersionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVersion(
        string id,
        int version,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetVersionAsync(id, version, tenantId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Process version {version} not found for definition '{id}'." });

        return Ok(result);
    }

    /// <summary>
    /// Updates a draft version.
    /// </summary>
    [HttpPut("{id}/versions/{version:int}")]
    [ProducesResponseType(typeof(ProcessVersionDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> UpdateVersion(
        string id,
        int version,
        [FromBody] UpdateProcessVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateVersionAsync(id, version, tenantId, request, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process version {version} not found for definition '{id}'." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Publishes a version, making it the active version.
    /// </summary>
    [HttpPost("{id}/versions/{version:int}/publish")]
    [ProducesResponseType(typeof(ProcessVersionDetailDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PublishVersion(
        string id,
        int version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.PublishVersionAsync(id, version, tenantId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process version {version} not found for definition '{id}'." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deprecates a version.
    /// </summary>
    [HttpPost("{id}/versions/{version:int}/deprecate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeprecateVersion(
        string id,
        int version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            await _service.DeprecateVersionAsync(id, version, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process version {version} not found for definition '{id}'." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Validates a process model without saving.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ProcessValidationResultDto), 200)]
    public async Task<IActionResult> Validate(
        [FromBody] ProcessModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ValidateAsync(model, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Exports a process definition.
    /// </summary>
    [HttpGet("{id}/export")]
    [ProducesResponseType(typeof(ProcessDefinitionExportDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Export(
        string id,
        [FromQuery] int? version = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.ExportAsync(id, tenantId, version, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Imports a process definition.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Import(
        [FromBody] ImportProcessDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.ImportAsync(tenantId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clones a process definition.
    /// </summary>
    [HttpPost("{id}/clone")]
    [ProducesResponseType(typeof(ProcessDefinitionDetailDto), 201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Clone(
        string id,
        [FromBody] CloneProcessDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CloneAsync(id, tenantId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Process definition '{id}' not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IList<string>), 200)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetCategoriesAsync(tenantId, cancellationToken);
        return Ok(result);
    }

    private int GetTenantId()
    {
        // In production, extract from claims or tenant resolver
        var claim = User.FindFirst("tenant_id");
        if (claim != null && int.TryParse(claim.Value, out var tenantId))
            return tenantId;

        // Default for development
        return 1;
    }

    private string GetUserId()
    {
        // In production, extract from claims
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim?.Value ?? "system";
    }
}
