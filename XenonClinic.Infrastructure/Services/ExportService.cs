using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Export service for generating PDF, Excel, and CSV files.
/// Uses lightweight implementations; for production, consider
/// ClosedXML, EPPlus, or PuppeteerSharp for more features.
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, ExcelExportOptions? options = null)
    {
        options ??= new ExcelExportOptions();
        var items = data.ToList();

        // Generate simple XML-based Excel (SpreadsheetML)
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.AppendLine($"<Worksheet ss:Name=\"{options.SheetName}\">");
        sb.AppendLine("<Table>");

        var properties = GetProperties<T>(options.IncludeColumns, options.ExcludeColumns);

        // Header row
        if (options.IncludeHeaders)
        {
            sb.AppendLine("<Row>");
            foreach (var prop in properties)
            {
                var header = options.ColumnHeaders?.GetValueOrDefault(prop.Name) ?? prop.Name;
                sb.AppendLine($"<Cell><Data ss:Type=\"String\">{EscapeXml(header)}</Data></Cell>");
            }
            sb.AppendLine("</Row>");
        }

        // Data rows
        foreach (var item in items)
        {
            sb.AppendLine("<Row>");
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var (type, formattedValue) = FormatCellValue(value, prop, options.ColumnFormats);
                sb.AppendLine($"<Cell><Data ss:Type=\"{type}\">{EscapeXml(formattedValue)}</Data></Cell>");
            }
            sb.AppendLine("</Row>");
        }

        sb.AppendLine("</Table>");
        sb.AppendLine("</Worksheet>");
        sb.AppendLine("</Workbook>");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        _logger.LogInformation("Generated Excel with {RowCount} rows", items.Count);

        return Task.FromResult(bytes);
    }

    public Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, PdfExportOptions? options = null)
    {
        options ??= new PdfExportOptions();
        var items = data.ToList();

        // Generate HTML then convert to simple PDF-like format
        // For production, use PuppeteerSharp, wkhtmltopdf, or a commercial library
        var html = GenerateHtmlTable(items, options);
        return GeneratePdfFromHtmlAsync(html, options);
    }

    public Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, CsvExportOptions? options = null)
    {
        options ??= new CsvExportOptions();
        var items = data.ToList();
        var sb = new StringBuilder();

        var properties = GetProperties<T>(options.IncludeColumns, options.ExcludeColumns);

        // Header row
        if (options.IncludeHeaders)
        {
            var headers = properties.Select(p =>
                FormatCsvField(options.ColumnHeaders?.GetValueOrDefault(p.Name) ?? p.Name, options));
            sb.AppendLine(string.Join(options.Delimiter, headers));
        }

        // Data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var formatted = FormatValue(value, options.DateFormat);
                return FormatCsvField(formatted, options);
            });
            sb.AppendLine(string.Join(options.Delimiter, values));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        _logger.LogInformation("Generated CSV with {RowCount} rows", items.Count);

        return Task.FromResult(bytes);
    }

    public Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, PdfExportOptions? options = null)
    {
        options ??= new PdfExportOptions();

        // Simple PDF generation - wraps HTML in PDF structure
        // For production, use a proper HTML-to-PDF converter
        var pdf = GenerateSimplePdf(htmlContent, options);
        _logger.LogInformation("Generated PDF from HTML");

        return Task.FromResult(pdf);
    }

    public Task<byte[]> GeneratePdfFromTemplateAsync<T>(string templateId, T model, PdfExportOptions? options = null)
    {
        // Load template and apply model
        var html = LoadAndApplyTemplate(templateId, model);
        return GeneratePdfFromHtmlAsync(html, options);
    }

    #region Private Methods

    private static PropertyInfo[] GetProperties<T>(List<string>? include, List<string>? exclude)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (include?.Count > 0)
        {
            properties = properties.Where(p => include.Contains(p.Name)).ToArray();
        }
        else if (exclude?.Count > 0)
        {
            properties = properties.Where(p => !exclude.Contains(p.Name)).ToArray();
        }

        return properties;
    }

    private static (string Type, string Value) FormatCellValue(object? value, PropertyInfo prop, Dictionary<string, string>? formats)
    {
        if (value == null) return ("String", "");

        var format = formats?.GetValueOrDefault(prop.Name);

        return value switch
        {
            int or long or short or byte => ("Number", value.ToString() ?? ""),
            float or double or decimal d => ("Number", format != null ? d.ToString(format) : d.ToString()),
            DateTime dt => ("String", format != null ? dt.ToString(format) : dt.ToString("yyyy-MM-dd")),
            bool b => ("String", b ? "Yes" : "No"),
            _ => ("String", value.ToString() ?? "")
        };
    }

    private static string FormatValue(object? value, string dateFormat)
    {
        return value switch
        {
            null => "",
            DateTime dt => dt.ToString(dateFormat),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? ""
        };
    }

    private static string FormatCsvField(string value, CsvExportOptions options)
    {
        if (string.IsNullOrEmpty(value)) return options.QuoteAllFields ? "\"\"" : "";

        var needsQuotes = options.QuoteAllFields ||
                          value.Contains(options.Delimiter) ||
                          value.Contains('"') ||
                          value.Contains('\n') ||
                          value.Contains('\r');

        if (!needsQuotes) return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string GenerateHtmlTable<T>(List<T> items, PdfExportOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #4CAF50; color: white; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
        if (!string.IsNullOrEmpty(options.CssStyles))
        {
            sb.AppendLine(options.CssStyles);
        }
        sb.AppendLine("</style></head><body>");

        if (!string.IsNullOrEmpty(options.Title))
        {
            sb.AppendLine($"<h1>{options.Title}</h1>");
        }

        if (!string.IsNullOrEmpty(options.WatermarkText))
        {
            sb.AppendLine($"<div style=\"position:fixed;top:50%;left:50%;transform:translate(-50%,-50%)rotate(-45deg);opacity:0.1;font-size:72px;\">{options.WatermarkText}</div>");
        }

        var properties = typeof(T).GetProperties();
        sb.AppendLine("<table><thead><tr>");
        foreach (var prop in properties)
        {
            sb.AppendLine($"<th>{prop.Name}</th>");
        }
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var item in items)
        {
            sb.AppendLine("<tr>");
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                sb.AppendLine($"<td>{value}</td>");
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }

    private static byte[] GenerateSimplePdf(string html, PdfExportOptions options)
    {
        // This is a minimal PDF implementation for demo purposes
        // In production, use a proper PDF library

        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        sb.AppendLine("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj");

        var pageWidth = options.PageSize switch
        {
            PageSize.Letter => 612,
            PageSize.Legal => 612,
            PageSize.A3 => 842,
            PageSize.A5 => 420,
            _ => 595 // A4
        };

        var pageHeight = options.PageSize switch
        {
            PageSize.Letter => 792,
            PageSize.Legal => 1008,
            PageSize.A3 => 1190,
            PageSize.A5 => 595,
            _ => 842 // A4
        };

        if (options.Orientation == PageOrientation.Landscape)
        {
            (pageWidth, pageHeight) = (pageHeight, pageWidth);
        }

        sb.AppendLine($"3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Contents 4 0 R >> endobj");

        // Simplified content - just adds the HTML as a comment
        var content = $"BT /F1 12 Tf 50 {pageHeight - 50} Td (Document generated from HTML) Tj ET";
        sb.AppendLine($"4 0 obj << /Length {content.Length} >> stream");
        sb.AppendLine(content);
        sb.AppendLine("endstream endobj");
        sb.AppendLine("xref");
        sb.AppendLine("0 5");
        sb.AppendLine("trailer << /Size 5 /Root 1 0 R >>");
        sb.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string LoadAndApplyTemplate<T>(string templateId, T model)
    {
        // In production, load from database or file system
        var template = $"<html><body><h1>Template: {templateId}</h1><pre>{System.Text.Json.JsonSerializer.Serialize(model)}</pre></body></html>";
        return template;
    }

    #endregion
}
