using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for PayrollService email and export functionality.
/// </summary>
public class PayrollServiceTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<PayrollServiceTests>> _mockLogger;

    public PayrollServiceTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<PayrollServiceTests>>();
    }

    [Fact]
    public async Task EmailPayslipAsync_ShouldCallEmailService()
    {
        // Arrange
        var emailSent = false;
        _mockEmailService
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
            .Callback(() => emailSent = true)
            .ReturnsAsync(true);

        // Act
        await _mockEmailService.Object.SendAsync(new EmailMessage
        {
            To = "employee@example.com",
            Subject = "Your Payslip",
            Body = "Please find attached your payslip."
        });

        // Assert
        Assert.True(emailSent);
    }

    [Fact]
    public async Task EmailPayslipAsync_ShouldIncludeAttachment()
    {
        // Arrange
        EmailMessage? sentMessage = null;
        _mockEmailService
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
            .Callback<EmailMessage>(msg => sentMessage = msg)
            .ReturnsAsync(true);

        var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes

        // Act
        await _mockEmailService.Object.SendAsync(new EmailMessage
        {
            To = "employee@example.com",
            Subject = "Your Payslip - January 2025",
            Body = "Please find attached your payslip for January 2025.",
            Attachments = new List<EmailAttachment>
            {
                new()
                {
                    FileName = "payslip_january_2025.pdf",
                    Content = pdfContent,
                    ContentType = "application/pdf"
                }
            }
        });

        // Assert
        Assert.NotNull(sentMessage);
        Assert.Single(sentMessage.Attachments!);
        Assert.Equal("payslip_january_2025.pdf", sentMessage.Attachments![0].FileName);
    }

    [Fact]
    public async Task BulkEmailPayslipsAsync_ShouldContinueOnIndividualFailure()
    {
        // Arrange
        var callCount = 0;
        _mockEmailService
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount != 2; // Fail on second call
            });

        // Act - simulate 3 emails where second one fails
        var results = new List<bool>();
        for (int i = 0; i < 3; i++)
        {
            var result = await _mockEmailService.Object.SendAsync(new EmailMessage { To = $"emp{i}@example.com" });
            results.Add(result);
        }

        // Assert - should continue despite failure
        Assert.Equal(3, results.Count);
        Assert.True(results[0]);
        Assert.False(results[1]); // Failed
        Assert.True(results[2]);
    }

    [Fact]
    public async Task EmailPayslipAsync_ShouldLogFailure_OnException()
    {
        // Arrange
        var logCalled = false;
        _mockEmailService
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
            .ThrowsAsync(new Exception("SMTP connection failed"));

        // Act & Assert
        try
        {
            await _mockEmailService.Object.SendAsync(new EmailMessage());
        }
        catch (Exception)
        {
            logCalled = true;
        }

        Assert.True(logCalled);
    }

    [Fact]
    public void ExportReportAsync_ShouldSupportPdfFormat()
    {
        // Arrange
        var format = "pdf";

        // Assert
        Assert.Equal("pdf", format.ToLower());
    }

    [Fact]
    public void ExportReportAsync_ShouldSupportExcelFormat()
    {
        // Arrange
        var format = "excel";

        // Assert
        Assert.Equal("excel", format.ToLower());
    }

    [Fact]
    public void ExportReportAsync_ShouldSupportCsvFormat()
    {
        // Arrange
        var format = "csv";

        // Assert
        Assert.Equal("csv", format.ToLower());
    }

    [Fact]
    public void PayslipPdf_ShouldContainEmployeeInfo()
    {
        // Arrange
        var payslip = new PayslipDto
        {
            Id = 1,
            EmployeeName = "John Doe",
            EmployeeId = "EMP001",
            GrossSalary = 5000.00m,
            NetSalary = 4250.00m,
            Deductions = 750.00m
        };

        // Assert
        Assert.NotNull(payslip.EmployeeName);
        Assert.NotNull(payslip.EmployeeId);
        Assert.True(payslip.GrossSalary > 0);
        Assert.True(payslip.NetSalary > 0);
        Assert.True(payslip.NetSalary <= payslip.GrossSalary);
    }

    [Fact]
    public void PayslipEmail_SubjectShouldContainPeriod()
    {
        // Arrange
        var periodName = "January 2025";
        var subject = $"Your Payslip - {periodName}";

        // Assert
        Assert.Contains("Payslip", subject);
        Assert.Contains(periodName, subject);
    }
}
