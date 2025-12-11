using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Document generation service implementation.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly ILogger<DocumentService> _logger;
    private readonly ConcurrentDictionary<string, DocumentTemplate> _templates = new();
    private readonly ConcurrentDictionary<string, List<GeneratedDocument>> _processDocuments = new();

    public DocumentService(ILogger<DocumentService> logger)
    {
        _logger = logger;

        // Register sample templates
        RegisterSampleTemplates();
    }

    public async Task<GeneratedDocument> GenerateAsync(DocumentGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(request.TemplateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template '{request.TemplateId}' not found");
        }

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Template '{request.TemplateId}' is not active");
        }

        // Validate required variables
        ValidateVariables(template, request.Variables);

        // Render the template
        var renderedContent = RenderTemplate(template, request.Variables, request.Locale ?? "en");

        // Convert to target format
        var (content, contentType) = await ConvertToFormatAsync(renderedContent, template.SourceFormat, request.OutputFormat, template.Styles, cancellationToken);

        var fileName = request.FileName ?? $"{template.Key}_{DateTime.UtcNow:yyyyMMddHHmmss}{ContentTypes.GetFileExtension(request.OutputFormat)}";

        var document = new GeneratedDocument
        {
            Id = Guid.NewGuid().ToString(),
            TemplateId = template.Id,
            TemplateName = template.Name,
            FileName = fileName,
            ContentType = contentType,
            Format = request.OutputFormat,
            Content = content,
            SizeBytes = content.Length,
            UsedVariables = request.Variables,
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Generated document {DocumentId} from template {TemplateId}, format: {Format}, size: {Size} bytes",
            document.Id, template.Id, request.OutputFormat, document.SizeBytes);

        return document;
    }

    public async Task<GeneratedDocument> GenerateAndAttachAsync(string processInstanceId, DocumentGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var document = await GenerateAsync(request, cancellationToken);
        document.ProcessInstanceId = processInstanceId;

        // Store association with process
        var documents = _processDocuments.GetOrAdd(processInstanceId, _ => new List<GeneratedDocument>());
        lock (documents)
        {
            documents.Add(document);
        }

        _logger.LogInformation("Attached document {DocumentId} to process {ProcessInstanceId}",
            document.Id, processInstanceId);

        return document;
    }

    public Task<DocumentTemplate> RegisterTemplateAsync(RegisterDocumentTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new DocumentTemplate
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = request.TenantId,
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Engine = request.Engine,
            Content = request.Content,
            BinaryContent = request.BinaryContent,
            SourceFormat = request.SourceFormat,
            SupportedOutputFormats = request.SupportedOutputFormats,
            Variables = request.Variables,
            Styles = request.Styles,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _templates[template.Id] = template;

        // Also index by key
        _templates[template.Key] = template;

        _logger.LogInformation("Registered document template {TemplateId} with key {Key}",
            template.Id, template.Key);

        return Task.FromResult(template);
    }

    public Task<DocumentTemplate> UpdateTemplateAsync(string templateId, UpdateDocumentTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (!_templates.TryGetValue(templateId, out var template))
        {
            throw new InvalidOperationException($"Template '{templateId}' not found");
        }

        if (request.Name != null) template.Name = request.Name;
        if (request.Description != null) template.Description = request.Description;
        if (request.Category != null) template.Category = request.Category;
        if (request.Content != null) template.Content = request.Content;
        if (request.BinaryContent != null) template.BinaryContent = request.BinaryContent;
        if (request.Variables != null) template.Variables = request.Variables;
        if (request.Styles != null) template.Styles = request.Styles;
        if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;

        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Updated document template {TemplateId}, new version: {Version}",
            templateId, template.Version);

        return Task.FromResult(template);
    }

    public Task<DocumentTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        _templates.TryGetValue(templateId, out var template);
        return Task.FromResult(template);
    }

    public Task<IList<DocumentTemplate>> ListTemplatesAsync(string? tenantId = null, string? category = null, CancellationToken cancellationToken = default)
    {
        var query = _templates.Values
            .Where(t => !string.IsNullOrEmpty(t.Id) && t.Id != t.Key) // Filter out key aliases
            .AsEnumerable();

        if (!string.IsNullOrEmpty(tenantId))
        {
            query = query.Where(t => t.TenantId == tenantId);
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IList<DocumentTemplate>>(query.ToList());
    }

    public Task DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        if (_templates.TryRemove(templateId, out var template))
        {
            // Also remove by key if indexed
            _templates.TryRemove(template.Key, out _);
            _logger.LogInformation("Deleted document template {TemplateId}", templateId);
        }

        return Task.CompletedTask;
    }

    public Task<IList<GeneratedDocument>> GetProcessDocumentsAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        if (_processDocuments.TryGetValue(processInstanceId, out var documents))
        {
            return Task.FromResult<IList<GeneratedDocument>>(documents.ToList());
        }

        return Task.FromResult<IList<GeneratedDocument>>(new List<GeneratedDocument>());
    }

    public Task<GeneratedDocument> ConvertAsync(string documentId, DocumentFormat targetFormat, CancellationToken cancellationToken = default)
    {
        // In production, this would look up the document and convert it
        throw new NotImplementedException("Document conversion requires document storage implementation");
    }

    #region Private Methods

    private static void ValidateVariables(DocumentTemplate template, Dictionary<string, object> variables)
    {
        foreach (var varDef in template.Variables.Where(v => v.Value.Required))
        {
            if (!variables.ContainsKey(varDef.Key) || variables[varDef.Key] == null)
            {
                throw new InvalidOperationException($"Required variable '{varDef.Key}' is missing");
            }
        }
    }

    private string RenderTemplate(DocumentTemplate template, Dictionary<string, object> variables, string locale)
    {
        var content = template.Content;

        // Apply defaults for missing variables
        foreach (var varDef in template.Variables)
        {
            if (!variables.ContainsKey(varDef.Key) && varDef.Value.DefaultValue != null)
            {
                variables[varDef.Key] = varDef.Value.DefaultValue;
            }
        }

        return template.Engine switch
        {
            TemplateEngine.Simple => RenderSimpleTemplate(content, variables, locale),
            TemplateEngine.Handlebars => RenderHandlebarsTemplate(content, variables, locale),
            _ => RenderSimpleTemplate(content, variables, locale)
        };
    }

    private static string RenderSimpleTemplate(string template, Dictionary<string, object> variables, string locale)
    {
        var result = template;
        var culture = CultureInfo.GetCultureInfo(locale);

        // Replace {{variable}} patterns
        foreach (var kvp in variables)
        {
            var value = FormatValue(kvp.Value, culture);
            result = result.Replace($"{{{{{kvp.Key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        // Handle conditionals: {{#if variable}}content{{/if}}
        result = Regex.Replace(result, @"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", match =>
        {
            var varName = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (variables.TryGetValue(varName, out var value))
            {
                var hasValue = value switch
                {
                    null => false,
                    string s => !string.IsNullOrEmpty(s),
                    bool b => b,
                    IEnumerable<object> list => list.Any(),
                    _ => true
                };
                return hasValue ? content : "";
            }
            return "";
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Handle unless: {{#unless variable}}content{{/unless}}
        result = Regex.Replace(result, @"\{\{#unless\s+(\w+)\}\}(.*?)\{\{/unless\}\}", match =>
        {
            var varName = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (variables.TryGetValue(varName, out var value))
            {
                var hasValue = value switch
                {
                    null => false,
                    string s => !string.IsNullOrEmpty(s),
                    bool b => b,
                    _ => true
                };
                return hasValue ? "" : content;
            }
            return content;
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Handle each: {{#each items}}...{{/each}}
        result = Regex.Replace(result, @"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}", match =>
        {
            var varName = match.Groups[1].Value;
            var itemTemplate = match.Groups[2].Value;

            if (variables.TryGetValue(varName, out var value) && value is IEnumerable<object> items)
            {
                var sb = new StringBuilder();
                int index = 0;
                foreach (var item in items)
                {
                    var itemContent = itemTemplate;
                    if (item is Dictionary<string, object> dict)
                    {
                        foreach (var prop in dict)
                        {
                            itemContent = itemContent.Replace($"{{{{this.{prop.Key}}}}}", FormatValue(prop.Value, culture), StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    else
                    {
                        itemContent = itemContent.Replace("{{this}}", FormatValue(item, culture), StringComparison.OrdinalIgnoreCase);
                    }
                    itemContent = itemContent.Replace("{{@index}}", index.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
                    sb.Append(itemContent);
                    index++;
                }
                return sb.ToString();
            }
            return "";
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return result;
    }

    private static string RenderHandlebarsTemplate(string template, Dictionary<string, object> variables, string locale)
    {
        // Handlebars-style is very similar to simple, just with more features
        // In production, use a proper Handlebars library
        return RenderSimpleTemplate(template, variables, locale);
    }

    private static string FormatValue(object? value, CultureInfo culture)
    {
        return value switch
        {
            null => "",
            DateTime dt => dt.ToString("g", culture),
            DateTimeOffset dto => dto.ToString("g", culture),
            decimal d => d.ToString("N2", culture),
            double dbl => dbl.ToString("N2", culture),
            float f => f.ToString("N2", culture),
            bool b => b ? "Yes" : "No",
            JsonElement json => json.ValueKind switch
            {
                JsonValueKind.String => json.GetString() ?? "",
                JsonValueKind.Number => json.GetDecimal().ToString("N2", culture),
                JsonValueKind.True => "Yes",
                JsonValueKind.False => "No",
                _ => json.ToString()
            },
            _ => value.ToString() ?? ""
        };
    }

    private async Task<(byte[] Content, string ContentType)> ConvertToFormatAsync(
        string renderedContent,
        DocumentFormat sourceFormat,
        DocumentFormat targetFormat,
        DocumentStyleOptions? styles,
        CancellationToken cancellationToken)
    {
        // If source and target are the same, just return the content
        if (sourceFormat == targetFormat)
        {
            return (Encoding.UTF8.GetBytes(renderedContent), ContentTypes.GetContentType(targetFormat));
        }

        // Apply styles to HTML content
        if (sourceFormat == DocumentFormat.Html)
        {
            renderedContent = ApplyStylesToHtml(renderedContent, styles);
        }

        // Convert based on target format
        return targetFormat switch
        {
            DocumentFormat.Html => (Encoding.UTF8.GetBytes(renderedContent), ContentTypes.Html),
            DocumentFormat.Txt => (Encoding.UTF8.GetBytes(StripHtml(renderedContent)), ContentTypes.Txt),
            DocumentFormat.Pdf => await ConvertHtmlToPdfAsync(renderedContent, styles, cancellationToken),
            DocumentFormat.Json => (Encoding.UTF8.GetBytes(renderedContent), ContentTypes.Json),
            DocumentFormat.Xml => (Encoding.UTF8.GetBytes(renderedContent), ContentTypes.Xml),
            _ => (Encoding.UTF8.GetBytes(renderedContent), "application/octet-stream")
        };
    }

    private static string ApplyStylesToHtml(string html, DocumentStyleOptions? styles)
    {
        if (styles == null)
            return html;

        var stylesCss = new StringBuilder();
        stylesCss.AppendLine("<style>");
        stylesCss.AppendLine("body {");
        if (!string.IsNullOrEmpty(styles.FontFamily))
            stylesCss.AppendLine($"  font-family: {styles.FontFamily};");
        if (!string.IsNullOrEmpty(styles.FontSize))
            stylesCss.AppendLine($"  font-size: {styles.FontSize};");
        if (styles.Margins != null)
        {
            stylesCss.AppendLine($"  margin-top: {styles.Margins.Top};");
            stylesCss.AppendLine($"  margin-bottom: {styles.Margins.Bottom};");
            stylesCss.AppendLine($"  margin-left: {styles.Margins.Left};");
            stylesCss.AppendLine($"  margin-right: {styles.Margins.Right};");
        }
        stylesCss.AppendLine("}");
        if (!string.IsNullOrEmpty(styles.CustomCss))
            stylesCss.AppendLine(styles.CustomCss);
        stylesCss.AppendLine("</style>");

        // Insert styles into head or at the beginning
        if (html.Contains("<head>", StringComparison.OrdinalIgnoreCase))
        {
            html = html.Replace("<head>", $"<head>{stylesCss}", StringComparison.OrdinalIgnoreCase);
        }
        else if (html.Contains("<html>", StringComparison.OrdinalIgnoreCase))
        {
            html = html.Replace("<html>", $"<html><head>{stylesCss}</head>", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            html = $"{stylesCss}{html}";
        }

        return html;
    }

    private static Task<(byte[] Content, string ContentType)> ConvertHtmlToPdfAsync(
        string html,
        DocumentStyleOptions? styles,
        CancellationToken cancellationToken)
    {
        // In production, use a PDF library like:
        // - PuppeteerSharp (headless Chrome)
        // - wkhtmltopdf
        // - iTextSharp
        // - DinkToPdf

        // For now, return HTML with PDF content type as placeholder
        var pdfPlaceholder = $@"
%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R >>
endobj
4 0 obj
<< /Length 44 >>
stream
BT /F1 12 Tf 100 700 Td (Document Generated) Tj ET
endstream
endobj
xref
0 5
0000000000 65535 f
0000000009 00000 n
0000000058 00000 n
0000000115 00000 n
0000000206 00000 n
trailer
<< /Size 5 /Root 1 0 R >>
startxref
299
%%EOF";

        // Return HTML content encoded as bytes with note about PDF conversion
        var content = Encoding.UTF8.GetBytes($"<!-- PDF conversion placeholder -->\n{html}");
        return Task.FromResult((content, ContentTypes.Pdf));
    }

    private static string StripHtml(string html)
    {
        // Remove HTML tags
        var result = Regex.Replace(html, "<[^>]*>", "");
        // Decode HTML entities
        result = System.Net.WebUtility.HtmlDecode(result);
        // Normalize whitespace
        result = Regex.Replace(result, @"\s+", " ");
        return result.Trim();
    }

    private void RegisterSampleTemplates()
    {
        // Invoice Template
        _templates["invoice"] = new DocumentTemplate
        {
            Id = "invoice",
            Key = "invoice",
            Name = "Invoice",
            Description = "Standard invoice template",
            Category = DocumentCategories.Invoice,
            Engine = TemplateEngine.Simple,
            SourceFormat = DocumentFormat.Html,
            SupportedOutputFormats = new List<DocumentFormat> { DocumentFormat.Html, DocumentFormat.Pdf },
            Variables = new Dictionary<string, VariableDefinition>
            {
                ["invoiceNumber"] = new() { Name = "Invoice Number", Type = "string", Required = true },
                ["customerName"] = new() { Name = "Customer Name", Type = "string", Required = true },
                ["customerAddress"] = new() { Name = "Customer Address", Type = "string", Required = false },
                ["invoiceDate"] = new() { Name = "Invoice Date", Type = "date", Required = true },
                ["dueDate"] = new() { Name = "Due Date", Type = "date", Required = true },
                ["items"] = new() { Name = "Line Items", Type = "array", Required = true },
                ["subtotal"] = new() { Name = "Subtotal", Type = "number", Required = true },
                ["tax"] = new() { Name = "Tax", Type = "number", Required = false, DefaultValue = 0 },
                ["total"] = new() { Name = "Total", Type = "number", Required = true }
            },
            Content = @"
<!DOCTYPE html>
<html>
<head>
    <title>Invoice {{invoiceNumber}}</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .header { display: flex; justify-content: space-between; margin-bottom: 40px; }
        .invoice-title { font-size: 32px; color: #333; }
        .invoice-details { text-align: right; }
        .customer-info { margin-bottom: 30px; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background-color: #f5f5f5; }
        .totals { text-align: right; }
        .totals td { border: none; }
        .grand-total { font-size: 18px; font-weight: bold; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='invoice-title'>INVOICE</div>
        <div class='invoice-details'>
            <strong>Invoice #:</strong> {{invoiceNumber}}<br>
            <strong>Date:</strong> {{invoiceDate}}<br>
            <strong>Due Date:</strong> {{dueDate}}
        </div>
    </div>
    <div class='customer-info'>
        <strong>Bill To:</strong><br>
        {{customerName}}<br>
        {{#if customerAddress}}{{customerAddress}}{{/if}}
    </div>
    <table>
        <thead>
            <tr>
                <th>Description</th>
                <th>Quantity</th>
                <th>Unit Price</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {{#each items}}
            <tr>
                <td>{{this.description}}</td>
                <td>{{this.quantity}}</td>
                <td>{{this.unitPrice}}</td>
                <td>{{this.amount}}</td>
            </tr>
            {{/each}}
        </tbody>
    </table>
    <table class='totals'>
        <tr><td>Subtotal:</td><td>{{subtotal}}</td></tr>
        {{#if tax}}<tr><td>Tax:</td><td>{{tax}}</td></tr>{{/if}}
        <tr class='grand-total'><td>Total:</td><td>{{total}}</td></tr>
    </table>
</body>
</html>"
        };

        // Appointment Confirmation Template
        _templates["appointment-confirmation"] = new DocumentTemplate
        {
            Id = "appointment-confirmation",
            Key = "appointment-confirmation",
            Name = "Appointment Confirmation",
            Description = "Patient appointment confirmation letter",
            Category = DocumentCategories.Letter,
            Engine = TemplateEngine.Simple,
            SourceFormat = DocumentFormat.Html,
            SupportedOutputFormats = new List<DocumentFormat> { DocumentFormat.Html, DocumentFormat.Pdf },
            Variables = new Dictionary<string, VariableDefinition>
            {
                ["patientName"] = new() { Name = "Patient Name", Type = "string", Required = true },
                ["appointmentDate"] = new() { Name = "Appointment Date", Type = "date", Required = true },
                ["appointmentTime"] = new() { Name = "Appointment Time", Type = "string", Required = true },
                ["doctorName"] = new() { Name = "Doctor Name", Type = "string", Required = true },
                ["department"] = new() { Name = "Department", Type = "string", Required = true },
                ["clinicAddress"] = new() { Name = "Clinic Address", Type = "string", Required = true },
                ["instructions"] = new() { Name = "Special Instructions", Type = "string", Required = false }
            },
            Content = @"
<!DOCTYPE html>
<html>
<head>
    <title>Appointment Confirmation</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }
        .header { text-align: center; margin-bottom: 40px; border-bottom: 2px solid #4a90d9; padding-bottom: 20px; }
        .header h1 { color: #4a90d9; margin: 0; }
        .appointment-box { background: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .details { margin: 20px 0; }
        .details p { margin: 8px 0; }
        .instructions { background: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; }
        .footer { margin-top: 40px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>XenonClinic</h1>
        <p>Appointment Confirmation</p>
    </div>
    <p>Dear {{patientName}},</p>
    <p>This letter confirms your upcoming appointment at XenonClinic.</p>
    <div class='appointment-box'>
        <h3>Appointment Details</h3>
        <div class='details'>
            <p><strong>Date:</strong> {{appointmentDate}}</p>
            <p><strong>Time:</strong> {{appointmentTime}}</p>
            <p><strong>Doctor:</strong> {{doctorName}}</p>
            <p><strong>Department:</strong> {{department}}</p>
            <p><strong>Location:</strong> {{clinicAddress}}</p>
        </div>
    </div>
    {{#if instructions}}
    <div class='instructions'>
        <strong>Special Instructions:</strong><br>
        {{instructions}}
    </div>
    {{/if}}
    <p>Please arrive 15 minutes before your scheduled appointment time.</p>
    <p>If you need to reschedule or cancel, please contact us at least 24 hours in advance.</p>
    <div class='footer'>
        <p>Thank you for choosing XenonClinic for your healthcare needs.</p>
    </div>
</body>
</html>"
        };

        // Medical Report Template
        _templates["medical-report"] = new DocumentTemplate
        {
            Id = "medical-report",
            Key = "medical-report",
            Name = "Medical Report",
            Description = "Patient medical report template",
            Category = DocumentCategories.Report,
            Engine = TemplateEngine.Simple,
            SourceFormat = DocumentFormat.Html,
            SupportedOutputFormats = new List<DocumentFormat> { DocumentFormat.Html, DocumentFormat.Pdf },
            Variables = new Dictionary<string, VariableDefinition>
            {
                ["patientName"] = new() { Name = "Patient Name", Type = "string", Required = true },
                ["patientId"] = new() { Name = "Patient ID", Type = "string", Required = true },
                ["dateOfBirth"] = new() { Name = "Date of Birth", Type = "date", Required = true },
                ["reportDate"] = new() { Name = "Report Date", Type = "date", Required = true },
                ["doctorName"] = new() { Name = "Doctor Name", Type = "string", Required = true },
                ["diagnosis"] = new() { Name = "Diagnosis", Type = "string", Required = true },
                ["findings"] = new() { Name = "Findings", Type = "string", Required = true },
                ["recommendations"] = new() { Name = "Recommendations", Type = "string", Required = false }
            },
            Content = @"
<!DOCTYPE html>
<html>
<head>
    <title>Medical Report - {{patientName}}</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .header { border-bottom: 2px solid #333; padding-bottom: 20px; margin-bottom: 20px; }
        .patient-info { background: #f5f5f5; padding: 15px; margin-bottom: 20px; }
        .section { margin: 20px 0; }
        .section h3 { color: #333; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
        .signature { margin-top: 50px; }
        .confidential { color: red; font-weight: bold; text-align: center; }
    </style>
</head>
<body>
    <p class='confidential'>CONFIDENTIAL MEDICAL DOCUMENT</p>
    <div class='header'>
        <h1>Medical Report</h1>
        <p>Report Date: {{reportDate}}</p>
    </div>
    <div class='patient-info'>
        <h3>Patient Information</h3>
        <p><strong>Name:</strong> {{patientName}}</p>
        <p><strong>Patient ID:</strong> {{patientId}}</p>
        <p><strong>Date of Birth:</strong> {{dateOfBirth}}</p>
    </div>
    <div class='section'>
        <h3>Diagnosis</h3>
        <p>{{diagnosis}}</p>
    </div>
    <div class='section'>
        <h3>Clinical Findings</h3>
        <p>{{findings}}</p>
    </div>
    {{#if recommendations}}
    <div class='section'>
        <h3>Recommendations</h3>
        <p>{{recommendations}}</p>
    </div>
    {{/if}}
    <div class='signature'>
        <p>_________________________</p>
        <p>{{doctorName}}</p>
        <p>Attending Physician</p>
    </div>
</body>
</html>"
        };
    }

    #endregion
}
