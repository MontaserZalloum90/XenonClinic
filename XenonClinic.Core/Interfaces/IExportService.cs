namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Document export service for PDF and Excel generation.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to Excel format.
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, ExcelExportOptions? options = null);

    /// <summary>
    /// Export data to PDF format.
    /// </summary>
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, PdfExportOptions? options = null);

    /// <summary>
    /// Export data to CSV format.
    /// </summary>
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, CsvExportOptions? options = null);

    /// <summary>
    /// Generate PDF from HTML template.
    /// </summary>
    Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, PdfExportOptions? options = null);

    /// <summary>
    /// Generate PDF from template with data model.
    /// </summary>
    Task<byte[]> GeneratePdfFromTemplateAsync<T>(string templateId, T model, PdfExportOptions? options = null);
}

/// <summary>
/// Excel export options.
/// </summary>
public class ExcelExportOptions
{
    public string SheetName { get; set; } = "Sheet1";
    public bool IncludeHeaders { get; set; } = true;
    public List<string>? IncludeColumns { get; set; }
    public List<string>? ExcludeColumns { get; set; }
    public Dictionary<string, string>? ColumnHeaders { get; set; }
    public Dictionary<string, string>? ColumnFormats { get; set; }
    public bool AutoFitColumns { get; set; } = true;
    public bool FreezeHeaderRow { get; set; } = true;
    public ExcelTableStyle TableStyle { get; set; } = ExcelTableStyle.Medium9;
}

/// <summary>
/// PDF export options.
/// </summary>
public class PdfExportOptions
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public PageSize PageSize { get; set; } = PageSize.A4;
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public PdfMargins Margins { get; set; } = new();
    public string? HeaderHtml { get; set; }
    public string? FooterHtml { get; set; }
    public bool IncludePageNumbers { get; set; } = true;
    public string? WatermarkText { get; set; }
    public string? CssStyles { get; set; }
}

/// <summary>
/// CSV export options.
/// </summary>
public class CsvExportOptions
{
    public char Delimiter { get; set; } = ',';
    public bool IncludeHeaders { get; set; } = true;
    public List<string>? IncludeColumns { get; set; }
    public List<string>? ExcludeColumns { get; set; }
    public Dictionary<string, string>? ColumnHeaders { get; set; }
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public bool QuoteAllFields { get; set; } = false;
}

/// <summary>
/// PDF margins.
/// </summary>
public class PdfMargins
{
    public int Top { get; set; } = 20;
    public int Bottom { get; set; } = 20;
    public int Left { get; set; } = 20;
    public int Right { get; set; } = 20;
}

/// <summary>
/// Page sizes.
/// </summary>
public enum PageSize
{
    A3,
    A4,
    A5,
    Letter,
    Legal,
    Tabloid
}

/// <summary>
/// Page orientation.
/// </summary>
public enum PageOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Excel table styles.
/// </summary>
public enum ExcelTableStyle
{
    None,
    Light1,
    Light9,
    Medium1,
    Medium9,
    Dark1,
    Dark9
}
