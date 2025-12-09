using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for ExportService.
/// </summary>
public class ExportServiceTests
{
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _loggerMock = new Mock<ILogger<ExportService>>();
        _service = new ExportService(_loggerMock.Object);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldGenerateValidCsv()
    {
        // Arrange
        var data = new[]
        {
            new TestRecord("John", 30, true),
            new TestRecord("Jane", 25, false)
        };

        // Act
        var bytes = await _service.ExportToCsvAsync(data);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.NotNull(bytes);
        Assert.Contains("Name", csv);
        Assert.Contains("Age", csv);
        Assert.Contains("John", csv);
        Assert.Contains("Jane", csv);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldRespectOptions()
    {
        // Arrange
        var data = new[]
        {
            new TestRecord("John", 30, true)
        };

        var options = new CsvExportOptions
        {
            Delimiter = ';',
            IncludeHeaders = true,
            ColumnHeaders = new Dictionary<string, string>
            {
                ["Name"] = "Full Name",
                ["Age"] = "Years Old"
            }
        };

        // Act
        var bytes = await _service.ExportToCsvAsync(data, options);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains(";", csv);
        Assert.Contains("Full Name", csv);
        Assert.Contains("Years Old", csv);
    }

    [Fact]
    public async Task ExportToExcelAsync_ShouldGenerateXml()
    {
        // Arrange
        var data = new[]
        {
            new TestRecord("John", 30, true),
            new TestRecord("Jane", 25, false)
        };

        // Act
        var bytes = await _service.ExportToExcelAsync(data);
        var xml = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.NotNull(bytes);
        Assert.Contains("<?xml", xml);
        Assert.Contains("Workbook", xml);
        Assert.Contains("John", xml);
    }

    [Fact]
    public async Task ExportToExcelAsync_ShouldRespectOptions()
    {
        // Arrange
        var data = new[]
        {
            new TestRecord("John", 30, true)
        };

        var options = new ExcelExportOptions
        {
            SheetName = "TestSheet",
            IncludeHeaders = true
        };

        // Act
        var bytes = await _service.ExportToExcelAsync(data, options);
        var xml = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("TestSheet", xml);
    }

    [Fact]
    public async Task GeneratePdfFromHtmlAsync_ShouldGeneratePdf()
    {
        // Arrange
        var html = "<html><body><h1>Test</h1></body></html>";

        // Act
        var bytes = await _service.GeneratePdfFromHtmlAsync(html);
        var pdf = System.Text.Encoding.ASCII.GetString(bytes);

        // Assert
        Assert.NotNull(bytes);
        Assert.StartsWith("%PDF", pdf);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldHandleEmptyData()
    {
        // Arrange
        var data = Array.Empty<TestRecord>();

        // Act
        var bytes = await _service.ExportToCsvAsync(data);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.NotNull(bytes);
        Assert.Contains("Name", csv); // Headers should still be present
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var data = new[]
        {
            new TestRecord("John, Jr.", 30, true),
            new TestRecord("Jane \"the great\"", 25, false)
        };

        // Act
        var bytes = await _service.ExportToCsvAsync(data);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("\"John, Jr.\"", csv);
        Assert.Contains("\"Jane \"\"the great\"\"\"", csv);
    }

    private record TestRecord(string Name, int Age, bool IsActive);
}
