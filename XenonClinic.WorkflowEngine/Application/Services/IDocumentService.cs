using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for generating documents from templates.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Generates a document from a template.
    /// </summary>
    Task<GeneratedDocument> GenerateAsync(DocumentGenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a document and attaches it to a process instance.
    /// </summary>
    Task<GeneratedDocument> GenerateAndAttachAsync(string processInstanceId, DocumentGenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a document template.
    /// </summary>
    Task<DocumentTemplate> RegisterTemplateAsync(RegisterDocumentTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a document template.
    /// </summary>
    Task<DocumentTemplate> UpdateTemplateAsync(string templateId, UpdateDocumentTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    Task<DocumentTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all templates, optionally filtered.
    /// </summary>
    Task<IList<DocumentTemplate>> ListTemplatesAsync(string? tenantId = null, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a template.
    /// </summary>
    Task DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents attached to a process instance.
    /// </summary>
    Task<IList<GeneratedDocument>> GetProcessDocumentsAsync(string processInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a document to a different format.
    /// </summary>
    Task<GeneratedDocument> ConvertAsync(string documentId, DocumentFormat targetFormat, CancellationToken cancellationToken = default);
}

#region Enums

public enum DocumentFormat
{
    Html,
    Pdf,
    Docx,
    Xlsx,
    Txt,
    Markdown,
    Json,
    Xml
}

public enum TemplateEngine
{
    Simple,      // Simple {{variable}} replacement
    Handlebars,  // Handlebars-style templating
    Razor,       // Razor templating
    Liquid       // Liquid templating
}

#endregion

#region Request DTOs

public class DocumentGenerationRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
    public DocumentFormat OutputFormat { get; set; } = DocumentFormat.Pdf;
    public string? FileName { get; set; }
    public Dictionary<string, object> Options { get; set; } = new();
    public string? Locale { get; set; } = "en";
}

public class RegisterDocumentTemplateRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TemplateEngine Engine { get; set; } = TemplateEngine.Simple;
    public string Content { get; set; } = string.Empty;
    public byte[]? BinaryContent { get; set; }
    public DocumentFormat SourceFormat { get; set; } = DocumentFormat.Html;
    public List<DocumentFormat> SupportedOutputFormats { get; set; } = new() { DocumentFormat.Pdf, DocumentFormat.Html };
    public Dictionary<string, VariableDefinition> Variables { get; set; } = new();
    public DocumentStyleOptions? Styles { get; set; }
}

public class UpdateDocumentTemplateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Content { get; set; }
    public byte[]? BinaryContent { get; set; }
    public Dictionary<string, VariableDefinition>? Variables { get; set; }
    public DocumentStyleOptions? Styles { get; set; }
    public bool? IsActive { get; set; }
}

#endregion

#region Entity DTOs

public class DocumentTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TemplateEngine Engine { get; set; } = TemplateEngine.Simple;
    public string Content { get; set; } = string.Empty;
    public byte[]? BinaryContent { get; set; }
    public DocumentFormat SourceFormat { get; set; } = DocumentFormat.Html;
    public List<DocumentFormat> SupportedOutputFormats { get; set; } = new();
    public Dictionary<string, VariableDefinition> Variables { get; set; } = new();
    public DocumentStyleOptions? Styles { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class VariableDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, date, boolean, array, object
    public string? Description { get; set; }
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public string? Format { get; set; } // For dates, numbers
    public List<object>? AllowedValues { get; set; }
}

public class DocumentStyleOptions
{
    public string? FontFamily { get; set; } = "Arial, sans-serif";
    public string? FontSize { get; set; } = "12pt";
    public string? PageSize { get; set; } = "A4"; // A4, Letter, Legal
    public string? PageOrientation { get; set; } = "Portrait"; // Portrait, Landscape
    public PageMargins? Margins { get; set; }
    public string? HeaderHtml { get; set; }
    public string? FooterHtml { get; set; }
    public bool IncludePageNumbers { get; set; }
    public string? CustomCss { get; set; }
}

public class PageMargins
{
    public string Top { get; set; } = "1in";
    public string Bottom { get; set; } = "1in";
    public string Left { get; set; } = "1in";
    public string Right { get; set; } = "1in";
}

public class GeneratedDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DocumentFormat Format { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public long SizeBytes { get; set; }
    public string? ProcessInstanceId { get; set; }
    public Dictionary<string, object> UsedVariables { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
    public string? StoragePath { get; set; }
}

#endregion

#region Common Document Types

public static class DocumentCategories
{
    public const string Contract = "Contract";
    public const string Invoice = "Invoice";
    public const string Report = "Report";
    public const string Letter = "Letter";
    public const string Certificate = "Certificate";
    public const string Form = "Form";
    public const string Receipt = "Receipt";
    public const string Agreement = "Agreement";
}

public static class ContentTypes
{
    public const string Html = "text/html";
    public const string Pdf = "application/pdf";
    public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string Txt = "text/plain";
    public const string Markdown = "text/markdown";
    public const string Json = "application/json";
    public const string Xml = "application/xml";

    public static string GetContentType(DocumentFormat format)
    {
        return format switch
        {
            DocumentFormat.Html => Html,
            DocumentFormat.Pdf => Pdf,
            DocumentFormat.Docx => Docx,
            DocumentFormat.Xlsx => Xlsx,
            DocumentFormat.Txt => Txt,
            DocumentFormat.Markdown => Markdown,
            DocumentFormat.Json => Json,
            DocumentFormat.Xml => Xml,
            _ => "application/octet-stream"
        };
    }

    public static string GetFileExtension(DocumentFormat format)
    {
        return format switch
        {
            DocumentFormat.Html => ".html",
            DocumentFormat.Pdf => ".pdf",
            DocumentFormat.Docx => ".docx",
            DocumentFormat.Xlsx => ".xlsx",
            DocumentFormat.Txt => ".txt",
            DocumentFormat.Markdown => ".md",
            DocumentFormat.Json => ".json",
            DocumentFormat.Xml => ".xml",
            _ => ".bin"
        };
    }
}

#endregion
