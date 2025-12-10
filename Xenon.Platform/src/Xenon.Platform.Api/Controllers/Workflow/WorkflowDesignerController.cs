namespace Xenon.Platform.Api.Controllers.Workflow;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Models.Designer;
using XenonClinic.WorkflowEngine.Services;
using XenonClinic.WorkflowEngine.Validation;

/// <summary>
/// API controller for the visual workflow designer.
/// </summary>
[ApiController]
[Route("api/v1/workflow/designer")]
[Authorize]
public class WorkflowDesignerController : ControllerBase
{
    private readonly IWorkflowDesignerService _designerService;
    private readonly ILogger<WorkflowDesignerController> _logger;

    public WorkflowDesignerController(
        IWorkflowDesignerService designerService,
        ILogger<WorkflowDesignerController> logger)
    {
        _designerService = designerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the node type catalog for the designer palette
    /// </summary>
    [HttpGet("node-types")]
    [ProducesResponseType(typeof(NodeTypeCatalog), StatusCodes.Status200OK)]
    public async Task<ActionResult<NodeTypeCatalog>> GetNodeTypes()
    {
        var catalog = await _designerService.GetNodeTypeCatalogAsync();
        return Ok(catalog);
    }

    /// <summary>
    /// Lists workflow designs with filtering and pagination
    /// </summary>
    [HttpGet("workflows")]
    [ProducesResponseType(typeof(WorkflowDesignListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowDesignListResult>> ListWorkflows(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isDraft = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool orderDesc = false)
    {
        var query = new WorkflowDesignQuery
        {
            SearchTerm = search,
            Category = category,
            IsDraft = isDraft,
            PageNumber = page,
            PageSize = pageSize,
            OrderBy = orderBy,
            OrderDescending = orderDesc
        };

        var result = await _designerService.ListAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific workflow design
    /// </summary>
    [HttpGet("workflows/{id}")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowDesignModel>> GetWorkflow(string id, [FromQuery] int? version = null)
    {
        var design = await _designerService.GetAsync(id, version);
        if (design == null)
        {
            return NotFound(new { error = "Workflow not found", id });
        }
        return Ok(design);
    }

    /// <summary>
    /// Creates a new workflow design
    /// </summary>
    [HttpPost("workflows")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkflowDesignModel>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        var design = new WorkflowDesignModel
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags ?? new List<string>()
        };

        var created = await _designerService.CreateAsync(design);

        _logger.LogInformation("Workflow design created: {WorkflowId}", created.Id);

        return CreatedAtAction(nameof(GetWorkflow), new { id = created.Id }, created);
    }

    /// <summary>
    /// Saves/updates a workflow design
    /// </summary>
    [HttpPut("workflows/{id}")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkflowDesignModel>> SaveWorkflow(string id, [FromBody] WorkflowDesignModel design)
    {
        if (id != design.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }

        var saved = await _designerService.SaveAsync(design);

        _logger.LogInformation("Workflow design saved: {WorkflowId} v{Version}", saved.Id, saved.Version);

        return Ok(saved);
    }

    /// <summary>
    /// Validates a workflow design
    /// </summary>
    [HttpPost("workflows/{id}/validate")]
    [ProducesResponseType(typeof(WorkflowValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowValidationResult>> ValidateWorkflow(string id)
    {
        var design = await _designerService.GetAsync(id);
        if (design == null)
        {
            return NotFound(new { error = "Workflow not found", id });
        }

        var result = await _designerService.ValidateAsync(design);
        return Ok(result);
    }

    /// <summary>
    /// Validates a workflow design from request body (for unsaved designs)
    /// </summary>
    [HttpPost("workflows/validate")]
    [ProducesResponseType(typeof(WorkflowValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowValidationResult>> ValidateWorkflowDesign([FromBody] WorkflowDesignModel design)
    {
        var result = await _designerService.ValidateAsync(design);
        return Ok(result);
    }

    /// <summary>
    /// Publishes a workflow design
    /// </summary>
    [HttpPost("workflows/{id}/publish")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowDesignModel>> PublishWorkflow(string id, [FromQuery] int version)
    {
        try
        {
            var published = await _designerService.PublishAsync(id, version);

            _logger.LogInformation("Workflow design published: {WorkflowId} v{Version}", id, version);

            return Ok(published);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Unpublishes a workflow design
    /// </summary>
    [HttpPost("workflows/{id}/unpublish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpublishWorkflow(string id, [FromQuery] int version)
    {
        await _designerService.UnpublishAsync(id, version);

        _logger.LogInformation("Workflow design unpublished: {WorkflowId} v{Version}", id, version);

        return NoContent();
    }

    /// <summary>
    /// Deletes a workflow design
    /// </summary>
    [HttpDelete("workflows/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteWorkflow(string id)
    {
        await _designerService.DeleteAsync(id);

        _logger.LogInformation("Workflow design deleted: {WorkflowId}", id);

        return NoContent();
    }

    /// <summary>
    /// Gets all versions of a workflow design
    /// </summary>
    [HttpGet("workflows/{id}/versions")]
    [ProducesResponseType(typeof(IList<WorkflowDesignVersionInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<WorkflowDesignVersionInfo>>> GetVersions(string id)
    {
        var versions = await _designerService.GetVersionsAsync(id);
        return Ok(versions);
    }

    /// <summary>
    /// Clones a workflow design
    /// </summary>
    [HttpPost("workflows/{id}/clone")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status201Created)]
    public async Task<ActionResult<WorkflowDesignModel>> CloneWorkflow(string id, [FromBody] CloneWorkflowRequest request)
    {
        var cloned = await _designerService.CloneAsync(id, request.NewName);

        _logger.LogInformation("Workflow design cloned: {SourceId} -> {NewId}", id, cloned.Id);

        return CreatedAtAction(nameof(GetWorkflow), new { id = cloned.Id }, cloned);
    }

    /// <summary>
    /// Exports a workflow design to JSON
    /// </summary>
    [HttpGet("workflows/{id}/export")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportWorkflow(string id, [FromQuery] int? version = null)
    {
        try
        {
            var json = await _designerService.ExportAsync(id, version);
            return Content(json, "application/json");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Imports a workflow design from JSON
    /// </summary>
    [HttpPost("workflows/import")]
    [ProducesResponseType(typeof(WorkflowDesignModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkflowDesignModel>> ImportWorkflow([FromBody] ImportWorkflowRequest request)
    {
        try
        {
            var imported = await _designerService.ImportAsync(request.Json, request.Overwrite);

            _logger.LogInformation("Workflow design imported: {WorkflowId}", imported.Id);

            return CreatedAtAction(nameof(GetWorkflow), new { id = imported.Id }, imported);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets execution statistics for a workflow
    /// </summary>
    [HttpGet("workflows/{id}/statistics")]
    [ProducesResponseType(typeof(WorkflowStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowStatistics>> GetStatistics(string id)
    {
        var stats = await _designerService.GetStatisticsAsync(id);
        return Ok(stats);
    }
}

#region Request/Response Models

public class CreateWorkflowRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
}

public class CloneWorkflowRequest
{
    public required string NewName { get; set; }
}

public class ImportWorkflowRequest
{
    public required string Json { get; set; }
    public bool Overwrite { get; set; }
}

#endregion
