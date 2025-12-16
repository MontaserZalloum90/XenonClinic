using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for Payroll operations including WPS file generation
/// </summary>
public class PayrollService : IPayrollService
{
    private readonly ClinicDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(ClinicDbContext context, IEmailService emailService, ILogger<PayrollService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    #region Payroll Period Management

    public async Task<IEnumerable<PayrollPeriodDto>> GetPayrollPeriodsAsync(int branchId, int? year = null)
    {
        var query = _context.PayrollPeriods
            .Where(p => p.BranchId == branchId);

        if (year.HasValue)
            query = query.Where(p => p.StartDate.Year == year.Value || p.EndDate.Year == year.Value);

        var periods = await query
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        var result = new List<PayrollPeriodDto>();
        foreach (var period in periods)
        {
            var payslips = await _context.Payslips.AsNoTracking().Where(s => s.PayrollPeriodId == period.Id).ToListAsync();
            result.Add(MapToPeriodDto(period, payslips));
        }

        return result;
    }

    public async Task<PayrollPeriodDto?> GetPayrollPeriodByIdAsync(int id)
    {
        var period = await _context.PayrollPeriods.FindAsync(id);
        if (period == null) return null;

        var payslips = await _context.Payslips.AsNoTracking().Where(s => s.PayrollPeriodId == id).ToListAsync();
        return MapToPeriodDto(period, payslips);
    }

    public async Task<PayrollPeriodDto?> GetCurrentPayrollPeriodAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var period = await _context.PayrollPeriods
            .Where(p => p.BranchId == branchId && p.StartDate <= today && p.EndDate >= today)
            .FirstOrDefaultAsync();

        if (period == null)
        {
            // Get the most recent period
            period = await _context.PayrollPeriods
                .Where(p => p.BranchId == branchId)
                .OrderByDescending(p => p.EndDate)
                .FirstOrDefaultAsync();
        }

        if (period == null) return null;

        var payslips = await _context.Payslips.AsNoTracking().Where(s => s.PayrollPeriodId == period.Id).ToListAsync();
        return MapToPeriodDto(period, payslips);
    }

    public async Task<PayrollPeriodDto> CreatePayrollPeriodAsync(int branchId, CreatePayrollPeriodDto dto)
    {
        var periodCode = await GeneratePeriodCodeAsync(branchId, dto.StartDate);

        var period = new PayrollPeriod
        {
            BranchId = branchId,
            PeriodCode = periodCode,
            PeriodName = $"{dto.StartDate:MMM yyyy}",
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PaymentDate = dto.PaymentDate,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.PayrollPeriods.Add(period);
        await _context.SaveChangesAsync();

        return MapToPeriodDto(period, new List<Payslip>());
    }

    public async Task<PayrollPeriodDto> ClosePayrollPeriodAsync(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        period.Status = "Closed";
        period.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var payslips = await _context.Payslips.AsNoTracking().Where(s => s.PayrollPeriodId == periodId).ToListAsync();
        return MapToPeriodDto(period, payslips);
    }

    #endregion

    #region Payroll Processing

    public async Task<PayrollPeriodDto> ProcessPayrollAsync(ProcessPayrollRequestDto request)
    {
        var period = await _context.PayrollPeriods.FindAsync(request.PayrollPeriodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {request.PayrollPeriodId} not found");

        // Get employees to process
        var employeeQuery = _context.Employees.AsNoTracking()
            .Where(e => e.BranchId == period.BranchId && e.IsActive);

        if (request.EmployeeIds != null && request.EmployeeIds.Any())
            employeeQuery = employeeQuery.Where(e => request.EmployeeIds.Contains(e.Id));

        var employees = await employeeQuery.ToListAsync();

        // Remove existing payslips if recalculating
        if (request.RecalculateAll)
        {
            var existingPayslips = await _context.Payslips
                .Where(p => p.PayrollPeriodId == request.PayrollPeriodId)
                .ToListAsync();

            _context.Payslips.RemoveRange(existingPayslips);
        }

        // Process each employee
        foreach (var employee in employees)
        {
            var existingPayslip = await _context.Payslips
                .FirstOrDefaultAsync(p => p.PayrollPeriodId == request.PayrollPeriodId && p.EmployeeId == employee.Id);

            if (existingPayslip != null && !request.RecalculateAll)
                continue;

            await CreateOrUpdatePayslipAsync(period, employee, request.IncludeLeaveAdjustments, request.IncludeOvertimeCalculations);
        }

        period.Status = "Processing";
        period.ProcessedDate = DateTime.UtcNow;
        period.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var payslips = await _context.Payslips.Where(s => s.PayrollPeriodId == period.Id).ToListAsync();
        return MapToPeriodDto(period, payslips);
    }

    public async Task<PayslipDto> RecalculateEmployeePayrollAsync(int periodId, int employeeId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        var employee = await _context.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        var payslip = await CreateOrUpdatePayslipAsync(period, employee, true, true);
        await _context.SaveChangesAsync();

        return await GetPayslipByIdAsync(payslip.Id) ?? throw new InvalidOperationException("Failed to retrieve payslip after creation");
    }

    public async Task<PayrollPeriodDto> ApprovePayrollAsync(ApprovePayrollRequestDto request)
    {
        var period = await _context.PayrollPeriods.FindAsync(request.PayrollPeriodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {request.PayrollPeriodId} not found");

        period.Status = "Approved";
        period.ApprovedDate = DateTime.UtcNow;
        period.ApprovedBy = request.ApprovedBy;
        period.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var payslips = await _context.Payslips.Where(s => s.PayrollPeriodId == period.Id).ToListAsync();
        return MapToPeriodDto(period, payslips);
    }

    public async Task<PayrollPeriodDto> MarkPayrollPaidAsync(int periodId, DateTime paymentDate)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        period.Status = "Paid";
        period.PaymentDate = paymentDate;
        period.UpdatedAt = DateTime.UtcNow;

        // Update all payslips
        var payslips = await _context.Payslips
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();

        foreach (var payslip in payslips)
        {
            payslip.Status = "Paid";
            payslip.PaidDate = paymentDate;
        }

        await _context.SaveChangesAsync();

        return MapToPeriodDto(period, payslips);
    }

    #endregion

    #region Payslips

    public async Task<IEnumerable<PayslipSummaryDto>> GetPayslipsForPeriodAsync(int periodId, int? departmentId = null)
    {
        var query = _context.Payslips
            .Include(p => p.Employee)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.PayrollPeriodId == periodId);

        if (departmentId.HasValue)
            query = query.Where(p => p.Employee != null && p.Employee.DepartmentId == departmentId.Value);

        var payslips = await query
            .OrderBy(p => p.Employee != null ? p.Employee.EmployeeCode : "")
            .ToListAsync();

        return payslips.Select(p => new PayslipSummaryDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeNumber = p.Employee?.EmployeeCode ?? "",
            EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}",
            PeriodName = p.PayrollPeriod?.PeriodName ?? "",
            GrossPay = p.GrossPay,
            NetPay = p.NetPay,
            Status = p.Status,
            PaymentDate = p.PayrollPeriod?.PaymentDate ?? DateTime.MinValue
        });
    }

    public async Task<PayslipDto?> GetPayslipByIdAsync(int id)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .Include(p => p.PayrollPeriod)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payslip == null) return null;

        return MapToPayslipDto(payslip);
    }

    public async Task<IEnumerable<PayslipSummaryDto>> GetEmployeePayslipsAsync(int employeeId, int? year = null)
    {
        var query = _context.Payslips
            .Include(p => p.Employee)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.EmployeeId == employeeId);

        if (year.HasValue)
            query = query.Where(p => p.PayrollPeriod != null && p.PayrollPeriod.StartDate.Year == year.Value);

        var payslips = await query
            .OrderByDescending(p => p.PayrollPeriod != null ? p.PayrollPeriod.StartDate : DateTime.MinValue)
            .ToListAsync();

        return payslips.Select(p => new PayslipSummaryDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeNumber = p.Employee?.EmployeeCode ?? "",
            EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}",
            PeriodName = p.PayrollPeriod?.PeriodName ?? "",
            GrossPay = p.GrossPay,
            NetPay = p.NetPay,
            Status = p.Status,
            PaymentDate = p.PayrollPeriod?.PaymentDate ?? DateTime.MinValue
        });
    }

    public async Task<PayslipDto?> GetLatestPayslipAsync(int employeeId)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayrollPeriod != null ? p.PayrollPeriod.EndDate : DateTime.MinValue)
            .FirstOrDefaultAsync();

        if (payslip == null) return null;

        return MapToPayslipDto(payslip);
    }

    public async Task<byte[]> GeneratePayslipPdfAsync(int payslipId)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Branch)
            .Include(p => p.PayrollPeriod)
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (payslip == null)
            return Array.Empty<byte>();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(payslip.Employee?.Branch?.Name ?? "XenonClinic")
                                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text(payslip.Employee?.Branch?.Address ?? "").FontSize(9);
                        });
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text("PAYSLIP").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Period: {payslip.PayrollPeriod?.StartDate:MMM dd} - {payslip.PayrollPeriod?.EndDate:MMM dd, yyyy}").FontSize(9);
                        });
                    });
                    header.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                });

                // Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Employee Information
                    col.Item().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("EMPLOYEE DETAILS").Bold().FontSize(11);
                            c.Item().PaddingTop(5).Text($"Name: {payslip.Employee?.FirstName} {payslip.Employee?.LastName}");
                            c.Item().Text($"Employee ID: {payslip.Employee?.Id.ToString() ?? payslip.EmployeeId.ToString()}");
                            c.Item().Text($"Department: {payslip.Employee?.Department?.Name ?? "N/A"}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PAYMENT DETAILS").Bold().FontSize(11);
                            c.Item().PaddingTop(5).Text($"Pay Date: {payslip.PaidDate:MMMM dd, yyyy}");
                            c.Item().Text($"Bank: {payslip.Employee?.BankName ?? "N/A"}");
                            c.Item().Text($"Account: {MaskAccountNumber(payslip.Employee?.BankAccountNumber)}");
                        });
                    });

                    // Earnings
                    col.Item().PaddingTop(20).Column(earnings =>
                    {
                        earnings.Item().Text("EARNINGS").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        earnings.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            AddPayslipRow(table, "Basic Salary", payslip.BasicSalary);
                            if (payslip.HousingAllowance > 0)
                                AddPayslipRow(table, "Housing Allowance", payslip.HousingAllowance);
                            if (payslip.TransportAllowance > 0)
                                AddPayslipRow(table, "Transport Allowance", payslip.TransportAllowance);
                            if (payslip.OtherAllowances > 0)
                                AddPayslipRow(table, "Other Allowances", payslip.OtherAllowances);
                            if (payslip.OvertimePay > 0)
                                AddPayslipRow(table, "Overtime Pay", payslip.OvertimePay);
                            if (payslip.Bonus > 0)
                                AddPayslipRow(table, "Bonus", payslip.Bonus);

                            // Total Earnings
                            table.Cell().BorderTop(1).PaddingTop(5).Text("Total Earnings").Bold();
                            table.Cell().BorderTop(1).PaddingTop(5).AlignRight().Text($"{payslip.GrossPay:C}").Bold().FontColor(Colors.Green.Darken2);
                        });
                    });

                    // Deductions
                    col.Item().PaddingTop(20).Column(deductions =>
                    {
                        deductions.Item().Text("DEDUCTIONS").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                        deductions.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            if (payslip.Tax > 0)
                                AddDeductionRow(table, "Income Tax", payslip.Tax);
                            if (payslip.SocialInsurance > 0)
                                AddDeductionRow(table, "Social Insurance", payslip.SocialInsurance);
                            if (payslip.HealthInsurance > 0)
                                AddDeductionRow(table, "Health Insurance", payslip.HealthInsurance);
                            if (payslip.PensionContribution > 0)
                                AddDeductionRow(table, "Pension Contribution", payslip.PensionContribution);
                            if (payslip.LoanDeduction > 0)
                                AddDeductionRow(table, "Loan Deduction", payslip.LoanDeduction);
                            if (payslip.OtherDeductions > 0)
                                AddDeductionRow(table, "Other Deductions", payslip.OtherDeductions);

                            // Total Deductions
                            var totalDeductions = payslip.Tax + payslip.SocialInsurance + payslip.HealthInsurance +
                                payslip.PensionContribution + payslip.LoanDeduction + payslip.OtherDeductions;
                            table.Cell().BorderTop(1).PaddingTop(5).Text("Total Deductions").Bold();
                            table.Cell().BorderTop(1).PaddingTop(5).AlignRight().Text($"-{totalDeductions:C}").Bold().FontColor(Colors.Red.Darken2);
                        });
                    });

                    // Net Pay
                    col.Item().PaddingTop(20).Background(Colors.Blue.Lighten4).Padding(15).Row(row =>
                    {
                        row.RelativeItem().Text("NET PAY").FontSize(14).Bold();
                        row.ConstantItem(150).AlignRight().Text($"{payslip.NetPay:C}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                    });

                    // Year-to-Date Summary
                    col.Item().PaddingTop(20).Column(ytd =>
                    {
                        ytd.Item().Text("YEAR-TO-DATE SUMMARY").Bold().FontSize(11).FontColor(Colors.Grey.Darken2);
                        ytd.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text($"YTD Gross: {payslip.YtdGross:C}");
                            r.RelativeItem().Text($"YTD Tax: {payslip.YtdTax:C}");
                            r.RelativeItem().Text($"YTD Net: {payslip.YtdNet:C}");
                        });
                    });

                    // Leave Balance (if available)
                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(8).Column(leave =>
                        {
                            leave.Item().Text("LEAVE BALANCE").Bold().FontSize(10);
                            leave.Item().Text($"Annual: {payslip.AnnualLeaveBalance} days").FontSize(9);
                            leave.Item().Text($"Sick: {payslip.SickLeaveBalance} days").FontSize(9);
                        });
                        row.ConstantItem(20);
                        row.RelativeItem();
                    });
                });

                // Footer
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    footer.Item().PaddingTop(10).Text("This is a computer-generated document. No signature required.")
                        .FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                    footer.Item().PaddingTop(3).Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                        .FontSize(7).FontColor(Colors.Grey.Lighten1).AlignCenter();
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static void AddPayslipRow(TableDescriptor table, string label, decimal amount)
    {
        table.Cell().Padding(3).Text(label);
        table.Cell().Padding(3).AlignRight().Text($"{amount:C}");
    }

    private static void AddDeductionRow(TableDescriptor table, string label, decimal amount)
    {
        table.Cell().Padding(3).Text(label);
        table.Cell().Padding(3).AlignRight().Text($"-{amount:C}").FontColor(Colors.Red.Darken1);
    }

    private static string MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";
        return $"****{accountNumber[^4..]}";
    }

    public async Task<bool> EmailPayslipAsync(int payslipId, string? emailOverride = null)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Branch)
            .Include(p => p.PayrollPeriod)
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (payslip == null)
            return false;

        var recipientEmail = emailOverride ?? payslip.Employee?.Email;
        if (string.IsNullOrEmpty(recipientEmail))
            return false;

        try
        {
            // Generate payslip PDF
            var pdfContent = await GeneratePayslipPdfAsync(payslipId);
            if (pdfContent.Length == 0)
                return false;

            var periodLabel = payslip.PayrollPeriod != null
                ? $"{payslip.PayrollPeriod.StartDate:MMM dd} - {payslip.PayrollPeriod.EndDate:MMM dd, yyyy}"
                : $"Pay Date: {payslip.PaidDate:MMMM dd, yyyy}";

            var emailMessage = new EmailMessage
            {
                To = recipientEmail,
                Subject = $"Your Payslip - {periodLabel}",
                Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                    <h2>Payslip Notification</h2>
                    <p>Dear {payslip.Employee?.FirstName ?? "Employee"},</p>
                    <p>Your payslip for the period <strong>{periodLabel}</strong> is now available.</p>
                    <p><strong>Summary:</strong></p>
                    <table style='border-collapse: collapse; margin: 15px 0;'>
                        <tr><td style='padding: 5px 15px 5px 0;'>Gross Pay:</td><td style='text-align: right;'>{payslip.GrossPay:C}</td></tr>
                        <tr><td style='padding: 5px 15px 5px 0;'>Total Deductions:</td><td style='text-align: right;'>-{(payslip.Tax + payslip.SocialInsurance + payslip.HealthInsurance + payslip.PensionContribution + payslip.LoanDeduction + payslip.OtherDeductions):C}</td></tr>
                        <tr style='font-weight: bold; border-top: 1px solid #ccc;'><td style='padding: 8px 15px 5px 0;'>Net Pay:</td><td style='text-align: right; padding-top: 8px;'>{payslip.NetPay:C}</td></tr>
                    </table>
                    <p>Please find your detailed payslip attached to this email.</p>
                    <p style='color: #666; font-size: 12px;'>This is an automated message. For questions about your pay, please contact HR.</p>
                    <hr/>
                    <p style='font-size: 11px; color: #888;'>{payslip.Employee?.Branch?.Name ?? "XenonClinic"} - HR Department</p>
                    </body>
                    </html>",
                IsHtml = true,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        FileName = $"Payslip_{payslip.Employee?.Id.ToString() ?? payslipId.ToString()}_{payslip.PaidDate:yyyyMMdd}.pdf",
                        Content = pdfContent,
                        ContentType = "application/pdf"
                    }
                }
            };

            await _emailService.SendAsync(emailMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to email payslip {PayslipId}", payslipId);
            return false;
        }
    }

    public async Task<int> BulkEmailPayslipsAsync(int periodId)
    {
        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == periodId && !string.IsNullOrEmpty(p.Employee.Email))
            .ToListAsync();

        var successCount = 0;

        foreach (var payslip in payslips)
        {
            try
            {
                var result = await EmailPayslipAsync(payslip.Id);
                if (result)
                    successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to email payslip {PayslipId}, continuing with next", payslip.Id);
            }
        }

        return successCount;
    }

    #endregion

    #region WPS (Wage Protection System)

    public async Task<WpsFileResponseDto> GenerateWpsFileAsync(WpsGenerationRequestDto request)
    {
        var response = new WpsFileResponseDto
        {
            FileFormat = request.FileFormat,
            GeneratedDate = DateTime.UtcNow,
            Records = new List<WpsRecordDto>()
        };

        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == request.PayrollPeriodId)
            .ToListAsync();

        if (request.EmployeeIds != null && request.EmployeeIds.Any())
            payslips = payslips.Where(p => request.EmployeeIds.Contains(p.EmployeeId)).ToList();

        var sb = new StringBuilder();
        var totalAmount = 0m;

        // Generate header record for SIF format
        if (request.FileFormat == "SIF")
        {
            sb.AppendLine($"EDR,{request.PayerBankCode},{request.PayerAccountNumber},{DateTime.UtcNow:yyyyMMdd},{payslips.Count}");
        }

        foreach (var payslip in payslips)
        {
            var employee = payslip.Employee;
            if (employee == null) continue;

            var record = new WpsRecordDto
            {
                RecordType = "SAL",
                EmployeeNumber = employee.EmployeeCode ?? "",
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmiratesId = employee.EmiratesId,
                LaborCardNumber = employee.LaborCardNumber,
                BankCode = employee.BankCode ?? "",
                AccountNumber = employee.BankAccountNumber ?? "",
                IBAN = employee.IBAN,
                Amount = payslip.NetPay,
                Currency = "AED",
                PaymentDate = request.PaymentDate,
                Narration = $"Salary {payslip.PayrollPeriod?.PeriodName}",
                ValidationErrors = new List<string>()
            };

            // Validate record
            ValidateWpsRecord(record);
            record.IsValid = !record.ValidationErrors.Any();

            response.Records.Add(record);

            if (record.IsValid)
            {
                totalAmount += record.Amount;

                // Generate file line based on format
                if (request.FileFormat == "SIF")
                {
                    sb.AppendLine(GenerateSifLine(record));
                }
                else if (request.FileFormat == "MOL")
                {
                    sb.AppendLine(GenerateMolLine(record));
                }
            }
            else
            {
                response.ValidationErrors.AddRange(record.ValidationErrors.Select(e => $"{record.EmployeeName}: {e}"));
            }
        }

        response.TotalRecords = response.Records.Count(r => r.IsValid);
        response.TotalAmount = totalAmount;
        response.FileName = $"WPS_{request.FileFormat}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
        response.FileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        response.ContentType = "text/plain";

        return response;
    }

    public async Task<WpsFileResponseDto> ValidateWpsDataAsync(int periodId)
    {
        var response = new WpsFileResponseDto
        {
            GeneratedDate = DateTime.UtcNow,
            Records = new List<WpsRecordDto>()
        };

        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();

        foreach (var payslip in payslips)
        {
            var employee = payslip.Employee;
            if (employee == null) continue;

            var record = new WpsRecordDto
            {
                EmployeeNumber = employee.EmployeeCode ?? "",
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmiratesId = employee.EmiratesId,
                BankCode = employee.BankCode ?? "",
                AccountNumber = employee.BankAccountNumber ?? "",
                IBAN = employee.IBAN,
                Amount = payslip.NetPay,
                ValidationErrors = new List<string>()
            };

            ValidateWpsRecord(record);
            record.IsValid = !record.ValidationErrors.Any();

            response.Records.Add(record);

            if (!record.IsValid)
                response.ValidationErrors.AddRange(record.ValidationErrors.Select(e => $"{record.EmployeeName}: {e}"));
        }

        response.TotalRecords = payslips.Count;
        response.TotalAmount = payslips.Where(p => response.Records.Any(r => r.EmployeeCode == p.Employee?.EmployeeCode && r.IsValid)).Sum(p => p.NetPay);

        return response;
    }

    public async Task<IEnumerable<WpsSubmissionDto>> GetWpsSubmissionsAsync(int branchId, int? year = null)
    {
        var query = _context.WpsSubmissions
            .Include(w => w.PayrollPeriod)
            .Where(w => w.PayrollPeriod.BranchId == branchId);

        if (year.HasValue)
            query = query.Where(w => w.SubmittedDate.Year == year.Value);

        var submissions = await query
            .OrderByDescending(w => w.SubmittedDate)
            .ToListAsync();

        return submissions.Select(w => new WpsSubmissionDto
        {
            Id = w.Id,
            PayrollPeriodId = w.PayrollPeriodId,
            FileName = w.FileName,
            SubmittedDate = w.SubmittedDate,
            TotalRecords = w.TotalRecords,
            TotalAmount = w.TotalAmount,
            Status = w.Status,
            ReferenceNumber = w.ReferenceNumber,
            ProcessedRecords = w.ProcessedRecords,
            RejectedRecords = w.RejectedRecords,
            RejectionReason = w.RejectionReason,
            ProcessedDate = w.ProcessedDate
        });
    }

    public async Task<WpsSubmissionDto> RecordWpsSubmissionAsync(int periodId, string fileName, string referenceNumber)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        var payslips = await _context.Payslips.Where(p => p.PayrollPeriodId == periodId).ToListAsync();

        var submission = new WpsSubmission
        {
            PayrollPeriodId = periodId,
            FileName = fileName,
            SubmittedDate = DateTime.UtcNow,
            TotalRecords = payslips.Count,
            TotalAmount = payslips.Sum(p => p.NetPay),
            Status = "Submitted",
            ReferenceNumber = referenceNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.WpsSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        return new WpsSubmissionDto
        {
            Id = submission.Id,
            PayrollPeriodId = submission.PayrollPeriodId,
            FileName = submission.FileName,
            SubmittedDate = submission.SubmittedDate,
            TotalRecords = submission.TotalRecords,
            TotalAmount = submission.TotalAmount,
            Status = submission.Status,
            ReferenceNumber = submission.ReferenceNumber
        };
    }

    public async Task<WpsSubmissionDto> UpdateWpsSubmissionStatusAsync(int submissionId, string status, string? rejectionReason = null)
    {
        var submission = await _context.WpsSubmissions.FindAsync(submissionId)
            ?? throw new KeyNotFoundException($"WPS submission with ID {submissionId} not found");

        submission.Status = status;
        submission.ProcessedDate = DateTime.UtcNow;
        submission.RejectionReason = rejectionReason;
        submission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new WpsSubmissionDto
        {
            Id = submission.Id,
            PayrollPeriodId = submission.PayrollPeriodId,
            FileName = submission.FileName,
            SubmittedDate = submission.SubmittedDate,
            TotalRecords = submission.TotalRecords,
            TotalAmount = submission.TotalAmount,
            Status = submission.Status,
            ReferenceNumber = submission.ReferenceNumber,
            ProcessedRecords = submission.ProcessedRecords,
            RejectedRecords = submission.RejectedRecords,
            RejectionReason = submission.RejectionReason,
            ProcessedDate = submission.ProcessedDate
        };
    }

    #endregion

    #region Reports

    public async Task<PayrollSummaryReportDto> GeneratePayrollSummaryReportAsync(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();

        var netPays = payslips.Select(p => p.NetPay).OrderBy(n => n).ToList();

        var report = new PayrollSummaryReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            PeriodName = period.PeriodName,
            PeriodStart = period.StartDate,
            PeriodEnd = period.EndDate,
            TotalEmployees = payslips.Count,
            ActiveEmployees = payslips.Count,
            TotalBasicSalary = payslips.Sum(p => p.BasicSalary),
            TotalAllowances = payslips.Sum(p => p.HousingAllowance + p.TransportAllowance + p.FoodAllowance + p.OtherAllowances),
            TotalOvertime = payslips.Sum(p => p.Overtime),
            TotalBonus = payslips.Sum(p => p.Bonus),
            TotalGrossPay = payslips.Sum(p => p.GrossPay),
            TotalSocialInsurance = payslips.Sum(p => p.SocialInsurance),
            TotalHealthInsurance = payslips.Sum(p => p.HealthInsurance),
            TotalTax = payslips.Sum(p => p.Tax),
            TotalOtherDeductions = payslips.Sum(p => p.OtherDeductions),
            TotalDeductions = payslips.Sum(p => p.TotalDeductions),
            TotalNetPay = payslips.Sum(p => p.NetPay),
            AverageNetPay = payslips.Any() ? payslips.Average(p => p.NetPay) : 0,
            MedianNetPay = netPays.Any() ? netPays[netPays.Count / 2] : 0,
            HighestNetPay = payslips.Any() ? payslips.Max(p => p.NetPay) : 0,
            LowestNetPay = payslips.Any() ? payslips.Min(p => p.NetPay) : 0
        };

        // Group by department
        report.ByDepartment = payslips
            .GroupBy(p => p.Employee?.Department?.Name ?? "Unassigned")
            .Select(g => new PayrollDepartmentSummaryDto
            {
                DepartmentName = g.Key,
                EmployeeCount = g.Count(),
                TotalGrossPay = g.Sum(p => p.GrossPay),
                TotalDeductions = g.Sum(p => p.TotalDeductions),
                TotalNetPay = g.Sum(p => p.NetPay),
                AverageNetPay = g.Average(p => p.NetPay)
            })
            .OrderByDescending(d => d.TotalNetPay)
            .ToList();

        // Group by payment method
        report.ByPaymentMethod = payslips
            .GroupBy(p => !string.IsNullOrEmpty(p.Employee?.IBAN) ? "Bank Transfer" : "Other")
            .Select(g => new PayrollPaymentMethodSummaryDto
            {
                PaymentMethod = g.Key,
                EmployeeCount = g.Count(),
                TotalAmount = g.Sum(p => p.NetPay)
            })
            .ToList();

        return report;
    }

    public async Task<PayrollRegisterReportDto> GeneratePayrollRegisterReportAsync(int periodId, int? departmentId = null)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        var query = _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .Where(p => p.PayrollPeriodId == periodId);

        if (departmentId.HasValue)
            query = query.Where(p => p.Employee != null && p.Employee.DepartmentId == departmentId.Value);

        var payslips = await query.OrderBy(p => p.Employee != null ? p.Employee.EmployeeCode : "").ToListAsync();

        var report = new PayrollRegisterReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            PeriodName = period.PeriodName,
            Entries = payslips.Select(p => new PayrollRegisterEntryDto
            {
                EmployeeNumber = p.Employee?.EmployeeCode ?? "",
                EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}",
                Department = p.Employee?.Department?.Name,
                Position = p.Employee?.JobPosition?.Title,
                JoinDate = p.Employee?.HireDate ?? DateTime.MinValue,
                DaysWorked = p.DaysWorked,
                BasicSalary = p.BasicSalary,
                HousingAllowance = p.HousingAllowance,
                TransportAllowance = p.TransportAllowance,
                OtherAllowances = p.FoodAllowance + p.OtherAllowances,
                Overtime = p.Overtime,
                GrossPay = p.GrossPay,
                SocialInsurance = p.SocialInsurance,
                Tax = p.Tax,
                OtherDeductions = p.HealthInsurance + p.LoanDeduction + p.AdvanceDeduction + p.OtherDeductions,
                TotalDeductions = p.TotalDeductions,
                NetPay = p.NetPay
            }).ToList()
        };

        report.Totals = new PayrollRegisterTotalsDto
        {
            TotalEmployees = report.Entries.Count,
            TotalBasicSalary = report.Entries.Sum(e => e.BasicSalary),
            TotalHousingAllowance = report.Entries.Sum(e => e.HousingAllowance),
            TotalTransportAllowance = report.Entries.Sum(e => e.TransportAllowance),
            TotalOtherAllowances = report.Entries.Sum(e => e.OtherAllowances),
            TotalOvertime = report.Entries.Sum(e => e.Overtime),
            TotalGrossPay = report.Entries.Sum(e => e.GrossPay),
            TotalSocialInsurance = report.Entries.Sum(e => e.SocialInsurance),
            TotalTax = report.Entries.Sum(e => e.Tax),
            TotalOtherDeductions = report.Entries.Sum(e => e.OtherDeductions),
            TotalDeductions = report.Entries.Sum(e => e.TotalDeductions),
            TotalNetPay = report.Entries.Sum(e => e.NetPay)
        };

        return report;
    }

    public async Task<BankTransferListReportDto> GenerateBankTransferListAsync(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId)
            ?? throw new KeyNotFoundException($"Payroll period with ID {periodId} not found");

        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == periodId && p.Employee != null && !string.IsNullOrEmpty(p.Employee.BankAccountNumber))
            .OrderBy(p => p.Employee != null ? p.Employee.EmployeeCode : "")
            .ToListAsync();

        var report = new BankTransferListReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            PeriodName = period.PeriodName,
            PaymentDate = period.PaymentDate,
            TotalTransfers = payslips.Count,
            TotalAmount = payslips.Sum(p => p.NetPay),
            Transfers = payslips.Select((p, i) => new BankTransferEntryDto
            {
                SequenceNumber = i + 1,
                EmployeeNumber = p.Employee?.EmployeeCode ?? "",
                EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}",
                BankName = p.Employee?.BankName ?? "",
                BankCode = p.Employee?.BankCode ?? "",
                AccountNumber = p.Employee?.BankAccountNumber ?? "",
                IBAN = p.Employee?.IBAN,
                Amount = p.NetPay,
                Currency = "AED",
                Purpose = $"Salary {period.PeriodName}"
            }).ToList()
        };

        return report;
    }

    public async Task<YtdEarningsReportDto> GenerateYtdEarningsReportAsync(int employeeId, int year)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        var payslips = await _context.Payslips
            .Include(p => p.PayrollPeriod)
            .Where(p => p.EmployeeId == employeeId && p.PayrollPeriod != null && p.PayrollPeriod.StartDate.Year == year)
            .OrderBy(p => p.PayrollPeriod != null ? p.PayrollPeriod.StartDate : DateTime.MinValue)
            .ToListAsync();

        var report = new YtdEarningsReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            Year = year,
            EmployeeId = employee.Id,
            EmployeeNumber = employee.EmployeeCode ?? "",
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Department = employee.Department?.Name,
            Position = employee.Position,
            YtdBasicSalary = payslips.Sum(p => p.BasicSalary),
            YtdAllowances = payslips.Sum(p => p.HousingAllowance + p.TransportAllowance + p.FoodAllowance + p.OtherAllowances),
            YtdOvertime = payslips.Sum(p => p.Overtime),
            YtdBonus = payslips.Sum(p => p.Bonus),
            YtdGrossPay = payslips.Sum(p => p.GrossPay),
            YtdSocialInsurance = payslips.Sum(p => p.SocialInsurance),
            YtdHealthInsurance = payslips.Sum(p => p.HealthInsurance),
            YtdTax = payslips.Sum(p => p.Tax),
            YtdOtherDeductions = payslips.Sum(p => p.OtherDeductions),
            YtdTotalDeductions = payslips.Sum(p => p.TotalDeductions),
            YtdNetPay = payslips.Sum(p => p.NetPay),
            MonthlyBreakdown = payslips.Select(p => new MonthlyEarningsSummaryDto
            {
                Month = p.PayrollPeriod.StartDate.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.PayrollPeriod.StartDate.Month),
                Year = p.PayrollPeriod.StartDate.Year,
                GrossPay = p.GrossPay,
                Deductions = p.TotalDeductions,
                NetPay = p.NetPay
            }).ToList()
        };

        return report;
    }

    public async Task<EmployeeCostReportDto> GenerateEmployeeCostReportAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.PayrollPeriod != null && p.PayrollPeriod.BranchId == branchId &&
                       p.PayrollPeriod.StartDate >= startDate &&
                       p.PayrollPeriod.EndDate <= endDate)
            .ToListAsync();

        var employeeCount = payslips.Select(p => p.EmployeeId).Distinct().Count();

        var report = new EmployeeCostReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalSalaryCost = payslips.Sum(p => p.BasicSalary),
            TotalAllowancesCost = payslips.Sum(p => p.HousingAllowance + p.TransportAllowance + p.FoodAllowance + p.OtherAllowances),
            TotalBenefitsCost = payslips.Sum(p => p.Bonus),
            TotalEmployerContributions = payslips.Sum(p => p.SocialInsurance * 0.125m), // Employer portion
            TotalEmployeeCost = payslips.Sum(p => p.GrossPay) + payslips.Sum(p => p.SocialInsurance * 0.125m),
            AverageCostPerEmployee = employeeCount > 0 ? payslips.Sum(p => p.GrossPay) / employeeCount : 0
        };

        report.ByDepartment = payslips
            .GroupBy(p => p.Employee?.Department?.Name ?? "Unassigned")
            .Select(g => new DepartmentCostDto
            {
                DepartmentName = g.Key,
                HeadCount = g.Select(p => p.EmployeeId).Distinct().Count(),
                TotalCost = g.Sum(p => p.GrossPay),
                AverageCost = g.Sum(p => p.GrossPay) / g.Select(p => p.EmployeeId).Distinct().Count(),
                PercentageOfTotal = report.TotalEmployeeCost > 0 ? (g.Sum(p => p.GrossPay) / report.TotalEmployeeCost * 100) : 0
            })
            .OrderByDescending(d => d.TotalCost)
            .ToList();

        return report;
    }

    public async Task<PayrollVarianceReportDto> GenerateVarianceReportAsync(int currentPeriodId, int previousPeriodId)
    {
        var currentPeriod = await _context.PayrollPeriods.FindAsync(currentPeriodId)
            ?? throw new KeyNotFoundException($"Current payroll period with ID {currentPeriodId} not found");

        var previousPeriod = await _context.PayrollPeriods.FindAsync(previousPeriodId)
            ?? throw new KeyNotFoundException($"Previous payroll period with ID {previousPeriodId} not found");

        var currentPayslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == currentPeriodId)
            .ToListAsync();

        var previousPayslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == previousPeriodId)
            .ToListAsync();

        var currentTotal = currentPayslips.Sum(p => p.NetPay);
        var previousTotal = previousPayslips.Sum(p => p.NetPay);

        var report = new PayrollVarianceReportDto
        {
            GeneratedDate = DateTime.UtcNow,
            CurrentPeriod = currentPeriod.PeriodName,
            PreviousPeriod = previousPeriod.PeriodName,
            CurrentTotalPayroll = currentTotal,
            PreviousTotalPayroll = previousTotal,
            Variance = currentTotal - previousTotal,
            VariancePercentage = previousTotal > 0 ? ((currentTotal - previousTotal) / previousTotal * 100) : 0
        };

        // Find significant variances
        var significantThreshold = 0.1m; // 10%
        foreach (var current in currentPayslips)
        {
            var previous = previousPayslips.FirstOrDefault(p => p.EmployeeId == current.EmployeeId);
            if (previous == null) continue;

            var variance = current.NetPay - previous.NetPay;
            var variancePct = previous.NetPay > 0 ? Math.Abs(variance / previous.NetPay) : 0;

            if (variancePct >= significantThreshold)
            {
                report.SignificantVariances.Add(new PayrollVarianceEntryDto
                {
                    EmployeeNumber = current.Employee?.EmployeeCode ?? "",
                    EmployeeName = $"{current.Employee?.FirstName} {current.Employee?.LastName}",
                    CurrentAmount = current.NetPay,
                    PreviousAmount = previous.NetPay,
                    Variance = variance,
                    VariancePercentage = variancePct * 100
                });
            }
        }

        // Category variances
        report.CategoryVariances = new List<PayrollCategoryVarianceDto>
        {
            CreateCategoryVariance("Basic Salary", currentPayslips.Sum(p => p.BasicSalary), previousPayslips.Sum(p => p.BasicSalary)),
            CreateCategoryVariance("Allowances", currentPayslips.Sum(p => p.HousingAllowance + p.TransportAllowance), previousPayslips.Sum(p => p.HousingAllowance + p.TransportAllowance)),
            CreateCategoryVariance("Overtime", currentPayslips.Sum(p => p.Overtime), previousPayslips.Sum(p => p.Overtime)),
            CreateCategoryVariance("Deductions", currentPayslips.Sum(p => p.TotalDeductions), previousPayslips.Sum(p => p.TotalDeductions))
        };

        return report;
    }

    public async Task<byte[]> ExportReportAsync(PayrollReportRequestDto request)
    {
        // Fetch payroll data based on request
        var query = _context.Payslips
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Branch)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee != null && p.Employee.BranchId == request.BranchId);

        if (request.PayrollPeriodId.HasValue)
            query = query.Where(p => p.PayrollPeriodId == request.PayrollPeriodId);

        if (request.StartDate.HasValue)
            query = query.Where(p => p.PaidDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.PaidDate <= request.EndDate.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(p => p.Employee != null && p.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeIds != null && request.EmployeeIds.Any())
            query = query.Where(p => request.EmployeeIds.Contains(p.EmployeeId));

        var payslips = await query.OrderBy(p => p.Employee != null ? p.Employee.LastName : "").ThenBy(p => p.Employee != null ? p.Employee.FirstName : "").ToListAsync();

        return request.Format?.ToLower() switch
        {
            "excel" => GeneratePayrollExcel(payslips, request),
            "csv" => GeneratePayrollCsv(payslips, request),
            _ => GeneratePayrollPdf(payslips, request)
        };
    }

    private byte[] GeneratePayrollPdf(List<Payslip> payslips, PayrollReportRequestDto request)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                // Header
                page.Header().Column(header =>
                {
                    header.Item().Text($"PAYROLL REPORT - {request.ReportType}").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    header.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9);
                    if (request.StartDate.HasValue && request.EndDate.HasValue)
                        header.Item().Text($"Period: {request.StartDate:MMM dd, yyyy} - {request.EndDate:MMM dd, yyyy}").FontSize(9);
                    header.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                });

                // Content
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Employee
                        columns.RelativeColumn(1); // Employee ID
                        columns.RelativeColumn(1.5f); // Department
                        columns.RelativeColumn(1.5f); // Basic
                        columns.RelativeColumn(1.5f); // Allowances
                        columns.RelativeColumn(1.5f); // Gross
                        columns.RelativeColumn(1.5f); // Deductions
                        columns.RelativeColumn(1.5f); // Net
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Employee").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("ID").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Department").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Basic").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Allowances").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Gross").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Deductions").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Net Pay").FontColor(Colors.White).Bold();
                    });

                    var rowIndex = 0;
                    foreach (var payslip in payslips)
                    {
                        var bgColor = rowIndex++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        var allowances = payslip.HousingAllowance + payslip.TransportAllowance + payslip.OtherAllowances;
                        var deductions = payslip.Tax + payslip.SocialInsurance + payslip.HealthInsurance +
                            payslip.PensionContribution + payslip.LoanDeduction + payslip.OtherDeductions;

                        table.Cell().Background(bgColor).Padding(4).Text($"{payslip.Employee?.FirstName} {payslip.Employee?.LastName}");
                        table.Cell().Background(bgColor).Padding(4).Text(payslip.Employee?.Id.ToString() ?? "");
                        table.Cell().Background(bgColor).Padding(4).Text(payslip.Employee?.Department?.Name ?? "");
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{payslip.BasicSalary:N2}");
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{allowances:N2}");
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{payslip.GrossPay:N2}").FontColor(Colors.Green.Darken2);
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{deductions:N2}").FontColor(Colors.Red.Darken2);
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{payslip.NetPay:N2}").Bold();
                    }

                    // Totals row
                    var totalBasic = payslips.Sum(p => p.BasicSalary);
                    var totalAllowances = payslips.Sum(p => p.HousingAllowance + p.TransportAllowance + p.OtherAllowances);
                    var totalGross = payslips.Sum(p => p.GrossPay);
                    var totalDeductions = payslips.Sum(p => p.Tax + p.SocialInsurance + p.HealthInsurance +
                        p.PensionContribution + p.LoanDeduction + p.OtherDeductions);
                    var totalNet = payslips.Sum(p => p.NetPay);

                    table.Cell().BorderTop(2).Padding(5).Text("TOTALS").Bold();
                    table.Cell().BorderTop(2).Padding(5).Text($"{payslips.Count} employees").Italic();
                    table.Cell().BorderTop(2).Padding(5);
                    table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"{totalBasic:N2}").Bold();
                    table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"{totalAllowances:N2}").Bold();
                    table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"{totalGross:N2}").Bold().FontColor(Colors.Green.Darken2);
                    table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"{totalDeductions:N2}").Bold().FontColor(Colors.Red.Darken2);
                    table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"{totalNet:N2}").Bold().FontSize(11);
                });

                // Footer
                page.Footer().AlignCenter().DefaultTextStyle(x => x.FontSize(8)).Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private byte[] GeneratePayrollExcel(List<Payslip> payslips, PayrollReportRequestDto request)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payroll Report");

        // Title
        worksheet.Cell(1, 1).Value = $"Payroll Report - {request.ReportType}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 10).Merge();

        worksheet.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";
        if (request.StartDate.HasValue && request.EndDate.HasValue)
            worksheet.Cell(2, 5).Value = $"Period: {request.StartDate:yyyy-MM-dd} - {request.EndDate:yyyy-MM-dd}";

        // Headers
        var headers = new[] { "Employee Name", "Employee ID", "Department", "Basic Salary", "Housing", "Transport",
            "Other Allow.", "Gross", "Tax", "Insurance", "Pension", "Other Ded.", "Net Pay" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F6FEB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data
        var row = 5;
        foreach (var payslip in payslips)
        {
            worksheet.Cell(row, 1).Value = $"{payslip.Employee?.FirstName} {payslip.Employee?.LastName}";
            worksheet.Cell(row, 2).Value = payslip.Employee?.Id.ToString() ?? "";
            worksheet.Cell(row, 3).Value = payslip.Employee?.Department?.Name ?? "";
            worksheet.Cell(row, 4).Value = payslip.BasicSalary;
            worksheet.Cell(row, 5).Value = payslip.HousingAllowance;
            worksheet.Cell(row, 6).Value = payslip.TransportAllowance;
            worksheet.Cell(row, 7).Value = payslip.OtherAllowances;
            worksheet.Cell(row, 8).Value = payslip.GrossPay;
            worksheet.Cell(row, 9).Value = payslip.Tax;
            worksheet.Cell(row, 10).Value = payslip.SocialInsurance + payslip.HealthInsurance;
            worksheet.Cell(row, 11).Value = payslip.PensionContribution;
            worksheet.Cell(row, 12).Value = payslip.LoanDeduction + payslip.OtherDeductions;
            worksheet.Cell(row, 13).Value = payslip.NetPay;

            // Format currency columns
            for (int col = 4; col <= 13; col++)
            {
                worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            }
            row++;
        }

        // Totals
        worksheet.Cell(row, 1).Value = "TOTALS";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        for (int col = 4; col <= 13; col++)
        {
            worksheet.Cell(row, col).FormulaA1 = $"SUM({worksheet.Cell(5, col).Address}:{worksheet.Cell(row - 1, col).Address})";
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private byte[] GeneratePayrollCsv(List<Payslip> payslips, PayrollReportRequestDto request)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"Payroll Report - {request.ReportType}");
        sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        // Column headers
        sb.AppendLine("Employee Name,Employee ID,Department,Basic Salary,Housing,Transport,Other Allowances,Gross,Tax,Insurance,Pension,Other Deductions,Net Pay");

        // Data
        foreach (var payslip in payslips)
        {
            var name = $"{payslip.Employee?.FirstName} {payslip.Employee?.LastName}".Replace(",", " ");
            sb.AppendLine(string.Join(",",
                name,
                payslip.Employee?.Id.ToString() ?? "",
                (payslip.Employee?.Department?.Name ?? "").Replace(",", " "),
                payslip.BasicSalary,
                payslip.HousingAllowance,
                payslip.TransportAllowance,
                payslip.OtherAllowances,
                payslip.GrossPay,
                payslip.Tax,
                payslip.SocialInsurance + payslip.HealthInsurance,
                payslip.PensionContribution,
                payslip.LoanDeduction + payslip.OtherDeductions,
                payslip.NetPay
            ));
        }

        // Totals
        sb.AppendLine(string.Join(",",
            "TOTALS",
            payslips.Count + " employees",
            "",
            payslips.Sum(p => p.BasicSalary),
            payslips.Sum(p => p.HousingAllowance),
            payslips.Sum(p => p.TransportAllowance),
            payslips.Sum(p => p.OtherAllowances),
            payslips.Sum(p => p.GrossPay),
            payslips.Sum(p => p.Tax),
            payslips.Sum(p => p.SocialInsurance + p.HealthInsurance),
            payslips.Sum(p => p.PensionContribution),
            payslips.Sum(p => p.LoanDeduction + p.OtherDeductions),
            payslips.Sum(p => p.NetPay)
        ));

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #endregion

    #region Configuration

    public async Task<IEnumerable<SalaryComponentDto>> GetSalaryComponentsAsync(int branchId)
    {
        var components = await _context.SalaryComponents
            .Where(c => c.BranchId == branchId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return components.Select(c => new SalaryComponentDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            NameAr = c.NameAr,
            ComponentType = c.ComponentType,
            Category = c.Category,
            IsTaxable = c.IsTaxable,
            IsPartOfGross = c.IsPartOfGross,
            CalculationType = c.CalculationType,
            DefaultValue = c.DefaultValue,
            Formula = c.Formula,
            IsActive = c.IsActive,
            SortOrder = c.SortOrder
        });
    }

    public async Task<SalaryComponentDto> SaveSalaryComponentAsync(int branchId, SalaryComponentDto dto)
    {
        SalaryComponent component;

        if (dto.Id > 0)
        {
            component = await _context.SalaryComponents.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Salary component with ID {dto.Id} not found");
        }
        else
        {
            component = new SalaryComponent { BranchId = branchId, CreatedAt = DateTime.UtcNow };
            _context.SalaryComponents.Add(component);
        }

        component.Code = dto.Code;
        component.Name = dto.Name;
        component.NameAr = dto.NameAr;
        component.ComponentType = dto.ComponentType;
        component.Category = dto.Category;
        component.IsTaxable = dto.IsTaxable;
        component.IsPartOfGross = dto.IsPartOfGross;
        component.CalculationType = dto.CalculationType;
        component.DefaultValue = dto.DefaultValue;
        component.Formula = dto.Formula;
        component.IsActive = dto.IsActive;
        component.SortOrder = dto.SortOrder;
        component.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        dto.Id = component.Id;
        return dto;
    }

    public async Task<IEnumerable<TaxConfigurationDto>> GetTaxConfigurationsAsync(int branchId)
    {
        var configs = await _context.TaxConfigurations
            .Where(t => t.BranchId == branchId && t.IsActive)
            .OrderBy(t => t.MinThreshold)
            .ToListAsync();

        return configs.Select(t => new TaxConfigurationDto
        {
            Id = t.Id,
            TaxType = t.TaxType,
            Description = t.Description,
            Rate = t.Rate,
            MinThreshold = t.MinThreshold,
            MaxThreshold = t.MaxThreshold,
            IsActive = t.IsActive,
            EffectiveDate = t.EffectiveDate,
            ExpiryDate = t.ExpiryDate
        });
    }

    public async Task<TaxConfigurationDto> SaveTaxConfigurationAsync(int branchId, TaxConfigurationDto dto)
    {
        TaxConfiguration config;

        if (dto.Id > 0)
        {
            config = await _context.TaxConfigurations.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Tax configuration with ID {dto.Id} not found");
        }
        else
        {
            config = new TaxConfiguration { BranchId = branchId, CreatedAt = DateTime.UtcNow };
            _context.TaxConfigurations.Add(config);
        }

        config.TaxType = dto.TaxType;
        config.Description = dto.Description;
        config.Rate = dto.Rate;
        config.MinThreshold = dto.MinThreshold;
        config.MaxThreshold = dto.MaxThreshold;
        config.IsActive = dto.IsActive;
        config.EffectiveDate = dto.EffectiveDate;
        config.ExpiryDate = dto.ExpiryDate;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        dto.Id = config.Id;
        return dto;
    }

    #endregion

    #region Utility

    public Task<string> GeneratePeriodCodeAsync(int branchId, DateTime periodStart)
    {
        var code = $"PAY-{periodStart:yyyyMM}";
        return Task.FromResult(code);
    }

    public async Task<decimal> CalculateGrossPayAsync(int employeeId, int periodId)
    {
        var employee = await _context.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        var basicSalary = employee.BasicSalary;
        var housingAllowance = employee.HousingAllowance ?? 0;
        var transportAllowance = employee.TransportAllowance ?? 0;
        var otherAllowances = employee.OtherAllowances ?? 0;

        return basicSalary + housingAllowance + transportAllowance + otherAllowances;
    }

    public async Task<decimal> CalculateDeductionsAsync(int employeeId, int periodId, decimal grossPay)
    {
        var employee = await _context.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

        // Social insurance (employee portion - typically 5% in UAE)
        var socialInsurance = grossPay * 0.05m;

        // Get any loan or advance deductions
        var loanDeduction = employee.MonthlyLoanDeduction ?? 0;

        return socialInsurance + loanDeduction;
    }

    #endregion

    #region Private Helper Methods

    private async Task<Payslip> CreateOrUpdatePayslipAsync(PayrollPeriod period, Employee employee, bool includeLeave, bool includeOvertime)
    {
        var existingPayslip = await _context.Payslips
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == period.Id && p.EmployeeId == employee.Id);

        var payslip = existingPayslip ?? new Payslip
        {
            PayrollPeriodId = period.Id,
            EmployeeId = employee.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate earnings
        payslip.BasicSalary = employee.BasicSalary;
        payslip.HousingAllowance = employee.HousingAllowance ?? 0;
        payslip.TransportAllowance = employee.TransportAllowance ?? 0;
        payslip.FoodAllowance = employee.FoodAllowance ?? 0;
        payslip.OtherAllowances = employee.OtherAllowances ?? 0;

        // Calculate overtime if enabled
        if (includeOvertime)
        {
            payslip.Overtime = await CalculateOvertimeAsync(employee.Id, period.StartDate, period.EndDate);
        }

        // Calculate gross pay
        payslip.GrossPay = payslip.BasicSalary + payslip.HousingAllowance + payslip.TransportAllowance +
                          payslip.FoodAllowance + payslip.OtherAllowances + payslip.Overtime + payslip.Bonus;

        // Calculate deductions
        payslip.SocialInsurance = payslip.GrossPay * 0.05m; // 5% employee contribution
        payslip.HealthInsurance = employee.HealthInsuranceDeduction ?? 0;
        payslip.LoanDeduction = employee.MonthlyLoanDeduction ?? 0;
        payslip.AdvanceDeduction = 0; // Would query advance requests

        payslip.TotalDeductions = payslip.SocialInsurance + payslip.HealthInsurance + payslip.Tax +
                                 payslip.LoanDeduction + payslip.AdvanceDeduction + payslip.OtherDeductions;

        // Calculate net pay
        payslip.NetPay = payslip.GrossPay - payslip.TotalDeductions;

        // Days worked calculation
        payslip.DaysWorked = (period.EndDate - period.StartDate).Days + 1;

        // Leave adjustments
        if (includeLeave)
        {
            var leaveDays = await CalculateLeaveDaysAsync(employee.Id, period.StartDate, period.EndDate);
            payslip.LeaveDaysTaken = leaveDays;
        }

        payslip.Status = "Calculated";
        payslip.UpdatedAt = DateTime.UtcNow;

        if (existingPayslip == null)
            _context.Payslips.Add(payslip);

        return payslip;
    }

    private async Task<decimal> CalculateOvertimeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        // Placeholder - would query timesheet data
        await Task.CompletedTask;
        return 0;
    }

    private async Task<int> CalculateLeaveDaysAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var leaveCount = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                       l.Status == LeaveStatus.Approved &&
                       l.StartDate <= endDate &&
                       l.EndDate >= startDate)
            .SumAsync(l => (l.EndDate - l.StartDate).Days + 1);

        return leaveCount;
    }

    private static PayrollPeriodDto MapToPeriodDto(PayrollPeriod period, List<Payslip> payslips)
    {
        return new PayrollPeriodDto
        {
            Id = period.Id,
            PeriodCode = period.PeriodCode,
            PeriodName = period.PeriodName,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            PaymentDate = period.PaymentDate,
            Status = period.Status,
            TotalEmployees = payslips.Count,
            TotalGrossPay = payslips.Sum(p => p.GrossPay),
            TotalDeductions = payslips.Sum(p => p.TotalDeductions),
            TotalNetPay = payslips.Sum(p => p.NetPay),
            ApprovedDate = period.ApprovedDate,
            ApprovedBy = period.ApprovedBy,
            ProcessedDate = period.ProcessedDate
        };
    }

    private static PayslipDto MapToPayslipDto(Payslip payslip)
    {
        return new PayslipDto
        {
            Id = payslip.Id,
            PayrollPeriodId = payslip.PayrollPeriodId,
            PeriodName = payslip.PayrollPeriod?.PeriodName ?? "",
            EmployeeId = payslip.EmployeeId,
            EmployeeNumber = payslip.Employee?.EmployeeCode ?? "",
            EmployeeName = $"{payslip.Employee?.FirstName} {payslip.Employee?.LastName}",
            Department = payslip.Employee?.Department?.Name,
            Position = payslip.Employee?.JobPosition?.Title,
            PaymentDate = payslip.PayrollPeriod?.PaymentDate ?? DateTime.MinValue,
            BasicSalary = payslip.BasicSalary,
            HousingAllowance = payslip.HousingAllowance,
            TransportAllowance = payslip.TransportAllowance,
            FoodAllowance = payslip.FoodAllowance,
            OtherAllowances = payslip.OtherAllowances,
            Overtime = payslip.Overtime,
            Bonus = payslip.Bonus,
            TotalEarnings = payslip.GrossPay,
            SocialInsurance = payslip.SocialInsurance,
            HealthInsurance = payslip.HealthInsurance,
            Tax = payslip.Tax,
            LoanDeduction = payslip.LoanDeduction,
            AdvanceDeduction = payslip.AdvanceDeduction,
            OtherDeductions = payslip.OtherDeductions,
            TotalDeductions = payslip.TotalDeductions,
            GrossPay = payslip.GrossPay,
            NetPay = payslip.NetPay,
            BankName = payslip.Employee?.BankName,
            BankAccountNumber = payslip.Employee?.BankAccountNumber,
            IBAN = payslip.Employee?.IBAN,
            DaysWorked = payslip.DaysWorked,
            LeaveDaysTaken = payslip.LeaveDaysTaken,
            Status = payslip.Status,
            PaidDate = payslip.PaidDate,
            PaymentReference = payslip.PaymentReference
        };
    }

    private static void ValidateWpsRecord(WpsRecordDto record)
    {
        if (string.IsNullOrEmpty(record.BankCode))
            record.ValidationErrors.Add("Bank code is required");

        if (string.IsNullOrEmpty(record.AccountNumber) && string.IsNullOrEmpty(record.IBAN))
            record.ValidationErrors.Add("Bank account number or IBAN is required");

        if (record.Amount <= 0)
            record.ValidationErrors.Add("Amount must be greater than zero");

        if (string.IsNullOrEmpty(record.EmiratesId) && string.IsNullOrEmpty(record.LaborCardNumber))
            record.ValidationErrors.Add("Emirates ID or Labor Card number is required");
    }

    private static string GenerateSifLine(WpsRecordDto record)
    {
        // SIF format: RecordType,EmiratesID,BankCode,AccountNumber,Amount,EffectiveDate
        return $"{record.RecordType},{record.EmiratesId},{record.BankCode},{record.AccountNumber ?? record.IBAN},{record.Amount:F2},{record.PaymentDate:yyyyMMdd}";
    }

    private static string GenerateMolLine(WpsRecordDto record)
    {
        // MOL format (simplified)
        return $"{record.LaborCardNumber},{record.EmployeeName},{record.BankCode},{record.IBAN ?? record.AccountNumber},{record.Amount:F2}";
    }

    private static PayrollCategoryVarianceDto CreateCategoryVariance(string category, decimal current, decimal previous)
    {
        return new PayrollCategoryVarianceDto
        {
            Category = category,
            CurrentAmount = current,
            PreviousAmount = previous,
            Variance = current - previous,
            VariancePercentage = previous > 0 ? ((current - previous) / previous * 100) : 0
        };
    }

    #endregion
}
