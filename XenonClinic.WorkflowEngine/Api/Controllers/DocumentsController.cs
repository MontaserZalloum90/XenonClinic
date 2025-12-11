using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for document generation and template management.
/// </summary>
[ApiController]
[Route("api/workflow/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    #region Document Generation

    /// <summary>
    /// Generates a document from a template.
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedDocument>> GenerateDocument(
        [FromBody] DocumentGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GenerateAsync(request, cancellationToken);
        return Ok(document);
    }

    /// <summary>
    /// Generates a document and attaches it to a process instance.
    /// </summary>
    [HttpPost("process/{processInstanceId}/generate")]
    public async Task<ActionResult<GeneratedDocument>> GenerateAndAttach(
        string processInstanceId,
        [FromBody] DocumentGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GenerateAndAttachAsync(
            processInstanceId, request, cancellationToken);
        return Ok(document);
    }

    /// <summary>
    /// Downloads a generated document.
    /// </summary>
    [HttpGet("{documentId}/download")]
    public async Task<IActionResult> DownloadDocument(
        string documentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return BadRequest(new { message = "Document ID is required" });
        }

        // Try to get document directly if service supports it
        var document = await _documentService.GetDocumentAsync(documentId, cancellationToken);

        if (document == null)
        {
            return NotFound(new { message = $"Document '{documentId}' not found" });
        }

        if (document.Content == null || document.Content.Length == 0)
        {
            return NotFound(new { message = $"Document '{documentId}' has no content" });
        }

        var contentType = document.ContentType ?? "application/octet-stream";
        var fileName = document.FileName ?? $"document-{documentId}";

        return File(document.Content, contentType, fileName);
    }

    /// <summary>
    /// Gets documents for a process instance.
    /// </summary>
    [HttpGet("process/{processInstanceId}")]
    public async Task<ActionResult<IList<GeneratedDocument>>> GetProcessDocuments(
        string processInstanceId,
        CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetProcessDocumentsAsync(
            processInstanceId, cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Converts a document to a different format.
    /// </summary>
    [HttpPost("{documentId}/convert")]
    public async Task<ActionResult<GeneratedDocument>> ConvertDocument(
        string documentId,
        [FromQuery] DocumentFormat targetFormat,
        CancellationToken cancellationToken)
    {
        var converted = await _documentService.ConvertAsync(
            documentId, targetFormat, cancellationToken);
        return Ok(converted);
    }

    #endregion

    #region Template Management

    /// <summary>
    /// Registers a new document template.
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<DocumentTemplate>> CreateTemplate(
        [FromBody] RegisterDocumentTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _documentService.RegisterTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { templateId = template.Id }, template);
    }

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    [HttpGet("templates/{templateId}")]
    public async Task<ActionResult<DocumentTemplate>> GetTemplate(
        string templateId,
        CancellationToken cancellationToken)
    {
        var template = await _documentService.GetTemplateAsync(templateId, cancellationToken);
        if (template == null)
        {
            return NotFound(new { message = $"Template '{templateId}' not found" });
        }
        return Ok(template);
    }

    /// <summary>
    /// Updates a template.
    /// </summary>
    [HttpPut("templates/{templateId}")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<DocumentTemplate>> UpdateTemplate(
        string templateId,
        [FromBody] UpdateDocumentTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _documentService.UpdateTemplateAsync(
            templateId, request, cancellationToken);
        return Ok(template);
    }

    /// <summary>
    /// Deletes a template.
    /// </summary>
    [HttpDelete("templates/{templateId}")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<IActionResult> DeleteTemplate(
        string templateId,
        CancellationToken cancellationToken)
    {
        await _documentService.DeleteTemplateAsync(templateId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lists templates.
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<IList<DocumentTemplate>>> ListTemplates(
        [FromQuery] string? tenantId,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var templates = await _documentService.ListTemplatesAsync(
            tenantId, category, cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Previews a template with sample data.
    /// </summary>
    [HttpPost("templates/{templateId}/preview")]
    public async Task<ActionResult<GeneratedDocument>> PreviewTemplate(
        string templateId,
        [FromBody] Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        var request = new DocumentGenerationRequest
        {
            TemplateId = templateId,
            Variables = variables,
            OutputFormat = DocumentFormat.Html
        };

        var preview = await _documentService.GenerateAsync(request, cancellationToken);
        return Ok(preview);
    }

    /// <summary>
    /// Uploads a template file.
    /// </summary>
    [HttpPost("templates/upload")]
    [Authorize(Policy = "ProcessDesigner")]
    public async Task<ActionResult<DocumentTemplate>> UploadTemplate(
        [FromForm] string tenantId,
        [FromForm] string key,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] string? category,
        [FromForm] TemplateEngine engine,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "A file is required" });
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { message = "Tenant ID is required" });
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Template key is required" });
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Template name is required" });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var request = new RegisterDocumentTemplateRequest
        {
            TenantId = tenantId,
            Key = key,
            Name = name,
            Description = description ?? string.Empty,
            Category = category ?? "General",
            Engine = engine,
            BinaryContent = memoryStream.ToArray(),
            SourceFormat = GetDocumentFormat(file.FileName)
        };

        var template = await _documentService.RegisterTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { templateId = template.Id }, template);
    }

    #endregion

    #region Helper Methods

    private static DocumentFormat GetDocumentFormat(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return DocumentFormat.Html;
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
        return extension switch
        {
            ".html" or ".htm" => DocumentFormat.Html,
            ".docx" => DocumentFormat.Docx,
            ".xlsx" => DocumentFormat.Xlsx,
            ".md" => DocumentFormat.Markdown,
            ".txt" => DocumentFormat.Txt,
            ".json" => DocumentFormat.Json,
            ".xml" => DocumentFormat.Xml,
            _ => DocumentFormat.Html
        };
    }

    #endregion
}
