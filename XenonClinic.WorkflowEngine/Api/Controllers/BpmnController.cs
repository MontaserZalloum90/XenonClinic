using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for BPMN 2.0 import/export operations.
/// </summary>
[ApiController]
[Route("api/workflow/bpmn")]
[Authorize]
public class BpmnController : ControllerBase
{
    private readonly IBpmnService _bpmnService;

    public BpmnController(IBpmnService bpmnService)
    {
        _bpmnService = bpmnService;
    }

    /// <summary>
    /// Imports a BPMN 2.0 XML file.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<BpmnImportResult>> Import(
        [FromBody] BpmnImportRequest request,
        CancellationToken cancellationToken)
    {
        // Set tenant and user context if not provided
        if (request.TenantId == 0)
        {
            request.TenantId = GetTenantId();
        }
        if (string.IsNullOrEmpty(request.UserId))
        {
            request.UserId = GetUserId();
        }

        var result = await _bpmnService.ImportAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Imports a BPMN 2.0 file upload.
    /// </summary>
    [HttpPost("import/file")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<BpmnImportResult>> ImportFile(
        [FromForm] int? tenantId,
        [FromForm] bool deployImmediately,
        [FromForm] bool overwriteExisting,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var request = new BpmnImportRequest
        {
            TenantId = tenantId ?? GetTenantId(),
            UserId = GetUserId(),
            BpmnFile = memoryStream.ToArray(),
            FileName = file.FileName,
            DeployImmediately = deployImmediately,
            OverwriteExisting = overwriteExisting
        };

        var result = await _bpmnService.ImportAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Exports a process definition to BPMN 2.0 XML.
    /// </summary>
    [HttpGet("export/{processDefinitionId}")]
    public async Task<ActionResult<BpmnExportResult>> Export(
        string processDefinitionId,
        [FromQuery] bool includeDiagram = true,
        [FromQuery] bool prettyPrint = true,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var options = new BpmnExportOptions
        {
            IncludeDiagram = includeDiagram,
            PrettyPrint = prettyPrint
        };

        var result = await _bpmnService.ExportAsync(processDefinitionId, tenantId, options, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Downloads a process definition as BPMN 2.0 XML file.
    /// </summary>
    [HttpGet("export/{processDefinitionId}/download")]
    public async Task<IActionResult> DownloadBpmn(
        string processDefinitionId,
        [FromQuery] bool includeDiagram = true,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var options = new BpmnExportOptions
        {
            IncludeDiagram = includeDiagram,
            PrettyPrint = true
        };

        var result = await _bpmnService.ExportAsync(processDefinitionId, tenantId, options, cancellationToken);

        if (!result.Success || result.BpmnXml == null)
        {
            return BadRequest(new { message = result.ErrorMessage ?? "Export failed" });
        }

        var fileName = result.FileName ?? $"{processDefinitionId}.bpmn";
        var bytes = System.Text.Encoding.UTF8.GetBytes(result.BpmnXml);

        return File(bytes, "application/xml", fileName);
    }

    /// <summary>
    /// Validates BPMN 2.0 XML without importing.
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<BpmnValidationResult>> Validate(
        [FromBody] BpmnValidateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _bpmnService.ValidateAsync(request.BpmnXml, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validates a BPMN 2.0 file upload.
    /// </summary>
    [HttpPost("validate/file")]
    public async Task<ActionResult<BpmnValidationResult>> ValidateFile(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var bpmnXml = await reader.ReadToEndAsync(cancellationToken);

        var result = await _bpmnService.ValidateAsync(bpmnXml, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets diagram information from BPMN XML.
    /// </summary>
    [HttpPost("diagram")]
    public async Task<ActionResult<BpmnDiagramInfo>> GetDiagramInfo(
        [FromBody] BpmnValidateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _bpmnService.GetDiagramInfoAsync(request.BpmnXml, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates diagram coordinates in BPMN XML.
    /// </summary>
    [HttpPut("diagram")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<string>> UpdateDiagram(
        [FromBody] BpmnDiagramUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _bpmnService.UpdateDiagramAsync(
            request.BpmnXml, request.Update, cancellationToken);
        return Ok(new { bpmnXml = result });
    }

    /// <summary>
    /// Parses BPMN 2.0 XML to a process model.
    /// </summary>
    [HttpPost("parse")]
    public async Task<ActionResult<Domain.Models.ProcessModel>> Parse(
        [FromBody] BpmnValidateRequest request,
        CancellationToken cancellationToken)
    {
        var model = await _bpmnService.ParseAsync(request.BpmnXml, cancellationToken);
        return Ok(model);
    }

    /// <summary>
    /// Converts a process model to BPMN 2.0 XML.
    /// </summary>
    [HttpPost("serialize")]
    public async Task<ActionResult<string>> Serialize(
        [FromBody] Domain.Models.ProcessModel model,
        CancellationToken cancellationToken)
    {
        var xml = await _bpmnService.SerializeAsync(model, cancellationToken);
        return Ok(new { bpmnXml = xml });
    }

    private int GetTenantId()
    {
        // In production, extract from claims or tenant resolver
        var claim = User.FindFirst("tenant_id");
        if (claim != null && int.TryParse(claim.Value, out var tenantId))
        {
            return tenantId;
        }
        return 1; // Default tenant for development
    }

    private string GetUserId()
    {
        return User.FindFirst("sub")?.Value
            ?? User.FindFirst("user_id")?.Value
            ?? "system";
    }
}

public class BpmnValidateRequest
{
    public string BpmnXml { get; set; } = string.Empty;
}

public class BpmnDiagramUpdateRequest
{
    public string BpmnXml { get; set; } = string.Empty;
    public BpmnDiagramUpdate Update { get; set; } = new();
}
