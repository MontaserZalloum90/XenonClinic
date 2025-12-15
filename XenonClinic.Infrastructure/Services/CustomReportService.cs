using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Custom Report Builder
/// </summary>
public class CustomReportService : ICustomReportService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<CustomReportService> _logger;
    private readonly IEmailService _emailService;

    // Mapping of data source names to entity types
    private static readonly Dictionary<string, Type> DataSourceEntityMap = new()
    {
        ["Patients"] = typeof(Patient),
        ["Appointments"] = typeof(Appointment),
        ["Invoices"] = typeof(Invoice),
        ["Payments"] = typeof(Payment),
        ["Prescriptions"] = typeof(Prescription),
        ["LabOrders"] = typeof(LabOrder),
        ["Visits"] = typeof(Visit),
        ["Users"] = typeof(ApplicationUser),
        ["Branches"] = typeof(Branch),
        ["Diagnoses"] = typeof(Diagnosis),
        ["InsuranceClaims"] = typeof(InsuranceClaim),
        ["Inventory"] = typeof(InventoryItem)
    };

    public CustomReportService(
        ClinicDbContext context,
        ILogger<CustomReportService> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    #region Report Definitions

    public async Task<CustomReportDefinitionDto> CreateReportAsync(int branchId, int userId, CreateCustomReportDto dto)
    {
        var report = new CustomReportDefinition
        {
            BranchId = branchId,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            DataSource = dto.DataSource,
            ColumnsJson = JsonSerializer.Serialize(dto.Columns),
            FiltersJson = dto.Filters != null ? JsonSerializer.Serialize(dto.Filters) : null,
            SortingJson = dto.Sorting != null ? JsonSerializer.Serialize(dto.Sorting) : null,
            GroupingJson = dto.Grouping != null ? JsonSerializer.Serialize(dto.Grouping) : null,
            AggregationJson = dto.Aggregation != null ? JsonSerializer.Serialize(dto.Aggregation) : null,
            VisualizationJson = dto.Visualization != null ? JsonSerializer.Serialize(dto.Visualization) : null,
            LayoutJson = dto.Layout != null ? JsonSerializer.Serialize(dto.Layout) : null,
            IsPublic = dto.IsPublic,
            IsTemplate = dto.IsTemplate,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomReportDefinitions.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Custom report created: {ReportId}, Name: {Name}", report.Id, report.Name);

        return await GetReportAsync(report.Id) ?? new CustomReportDefinitionDto();
    }

    public async Task<CustomReportDefinitionDto> UpdateReportAsync(int reportId, UpdateCustomReportDto dto)
    {
        var report = await _context.CustomReportDefinitions.FindAsync(reportId);
        if (report == null)
            return new CustomReportDefinitionDto();

        report.Name = dto.Name;
        report.Description = dto.Description;
        report.Category = dto.Category;
        report.DataSource = dto.DataSource;
        report.ColumnsJson = JsonSerializer.Serialize(dto.Columns);
        report.FiltersJson = dto.Filters != null ? JsonSerializer.Serialize(dto.Filters) : null;
        report.SortingJson = dto.Sorting != null ? JsonSerializer.Serialize(dto.Sorting) : null;
        report.GroupingJson = dto.Grouping != null ? JsonSerializer.Serialize(dto.Grouping) : null;
        report.AggregationJson = dto.Aggregation != null ? JsonSerializer.Serialize(dto.Aggregation) : null;
        report.VisualizationJson = dto.Visualization != null ? JsonSerializer.Serialize(dto.Visualization) : null;
        report.LayoutJson = dto.Layout != null ? JsonSerializer.Serialize(dto.Layout) : null;
        report.IsPublic = dto.IsPublic;
        report.IsTemplate = dto.IsTemplate;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetReportAsync(reportId) ?? new CustomReportDefinitionDto();
    }

    public async Task<CustomReportDefinitionDto?> GetReportAsync(int reportId)
    {
        var report = await _context.CustomReportDefinitions
            .Include(r => r.CreatedByUser)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        return report != null ? MapToReportDto(report) : null;
    }

    public async Task<IEnumerable<CustomReportDefinitionDto>> GetReportsAsync(
        int branchId, int userId, string? category = null)
    {
        var query = _context.CustomReportDefinitions
            .Include(r => r.CreatedByUser)
            .Where(r => r.BranchId == branchId &&
                (r.IsPublic || r.CreatedByUserId == userId));

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(r => r.Category == category);
        }

        var reports = await query
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Name)
            .ToListAsync();

        return reports.Select(MapToReportDto);
    }

    public async Task<bool> DeleteReportAsync(int reportId)
    {
        var report = await _context.CustomReportDefinitions.FindAsync(reportId);
        if (report == null)
            return false;

        report.IsDeleted = true;
        report.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CustomReportDefinitionDto> CloneReportAsync(int reportId, int userId, string newName)
    {
        var original = await _context.CustomReportDefinitions.FindAsync(reportId);
        if (original == null)
            return new CustomReportDefinitionDto();

        var clone = new CustomReportDefinition
        {
            BranchId = original.BranchId,
            Name = newName,
            Description = original.Description,
            Category = original.Category,
            DataSource = original.DataSource,
            ColumnsJson = original.ColumnsJson,
            FiltersJson = original.FiltersJson,
            SortingJson = original.SortingJson,
            GroupingJson = original.GroupingJson,
            AggregationJson = original.AggregationJson,
            VisualizationJson = original.VisualizationJson,
            LayoutJson = original.LayoutJson,
            IsPublic = false,
            IsTemplate = false,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomReportDefinitions.Add(clone);
        await _context.SaveChangesAsync();

        return await GetReportAsync(clone.Id) ?? new CustomReportDefinitionDto();
    }

    #endregion

    #region Report Execution

    public async Task<ExecuteReportResponseDto> ExecuteReportAsync(
        int branchId, int userId, ExecuteReportRequestDto request)
    {
        var stopwatch = Stopwatch.StartNew();

        var report = await _context.CustomReportDefinitions.FindAsync(request.ReportId);
        if (report == null)
        {
            return new ExecuteReportResponseDto
            {
                ReportId = request.ReportId,
                ExecutedAt = DateTime.UtcNow
            };
        }

        var columns = JsonSerializer.Deserialize<List<ReportColumnDto>>(report.ColumnsJson) ?? new List<ReportColumnDto>();
        var filters = !string.IsNullOrEmpty(report.FiltersJson)
            ? JsonSerializer.Deserialize<List<ReportFilterDto>>(report.FiltersJson)
            : null;
        var sorting = !string.IsNullOrEmpty(report.SortingJson)
            ? JsonSerializer.Deserialize<List<ReportSortDto>>(report.SortingJson)
            : null;
        var grouping = !string.IsNullOrEmpty(report.GroupingJson)
            ? JsonSerializer.Deserialize<List<ReportGroupingDto>>(report.GroupingJson)
            : null;
        var aggregation = !string.IsNullOrEmpty(report.AggregationJson)
            ? JsonSerializer.Deserialize<ReportAggregationDto>(report.AggregationJson)
            : null;

        // Apply runtime parameters to filters
        if (filters != null && request.Parameters != null)
        {
            foreach (var filter in filters.Where(f => f.IsParameter && !string.IsNullOrEmpty(f.ParameterName)))
            {
                if (request.Parameters.TryGetValue(filter.ParameterName!, out var paramValue))
                {
                    filter.Value = paramValue;
                }
            }
        }

        // Execute the dynamic query
        var (data, totalCount) = await ExecuteDynamicQueryAsync(
            branchId, report.DataSource, columns, filters, sorting, request.Page, request.PageSize);

        // Calculate aggregates if needed
        ReportSummaryDto? summary = null;
        if (aggregation != null)
        {
            summary = await CalculateAggregatesAsync(branchId, report.DataSource, columns, filters, aggregation, grouping);
        }

        stopwatch.Stop();

        // Log execution history
        await LogExecutionAsync(report.Id, userId, request.Parameters, data.Count, stopwatch.ElapsedMilliseconds, "Success");

        return new ExecuteReportResponseDto
        {
            ReportId = report.Id,
            ReportName = report.Name,
            ExecutedAt = DateTime.UtcNow,
            Columns = columns,
            Data = data,
            Summary = summary,
            TotalRecords = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<ReportExportResultDto> ExportReportAsync(
        int branchId, int userId, ExecuteReportRequestDto request)
    {
        // Execute the report first
        var reportData = await ExecuteReportAsync(branchId, userId, new ExecuteReportRequestDto
        {
            ReportId = request.ReportId,
            Parameters = request.Parameters,
            Page = 1,
            PageSize = 10000 // Export all data
        });

        byte[] content;
        string contentType;
        string fileName;

        switch (request.ExportFormat?.ToLower())
        {
            case "pdf":
                content = GeneratePdfReport(reportData);
                contentType = "application/pdf";
                fileName = $"{reportData.ReportName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                break;

            case "excel":
                content = GenerateExcelReport(reportData);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"{reportData.ReportName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                break;

            case "csv":
            default:
                content = GenerateCsvReport(reportData);
                contentType = "text/csv";
                fileName = $"{reportData.ReportName}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                break;
        }

        return new ReportExportResultDto
        {
            Content = content,
            ContentType = contentType,
            FileName = fileName
        };
    }

    public async Task<ExecuteReportResponseDto> PreviewReportAsync(
        int branchId, CreateCustomReportDto dto, int sampleSize = 10)
    {
        var (data, _) = await ExecuteDynamicQueryAsync(
            branchId, dto.DataSource, dto.Columns, dto.Filters, dto.Sorting, 1, sampleSize);

        return new ExecuteReportResponseDto
        {
            ReportId = 0,
            ReportName = "Preview",
            ExecutedAt = DateTime.UtcNow,
            Columns = dto.Columns,
            Data = data,
            TotalRecords = data.Count,
            Page = 1,
            PageSize = sampleSize,
            TotalPages = 1
        };
    }

    #endregion

    #region Data Sources

    public async Task<IEnumerable<DataSourceDto>> GetDataSourcesAsync(int branchId)
    {
        var dataSources = new List<DataSourceDto>
        {
            new DataSourceDto
            {
                Name = "Patients",
                DisplayName = "Patients",
                Description = "Patient records and demographics",
                Category = "Clinical",
                Fields = GetEntityFields(typeof(Patient))
            },
            new DataSourceDto
            {
                Name = "Appointments",
                DisplayName = "Appointments",
                Description = "Appointment schedules and history",
                Category = "Clinical",
                Fields = GetEntityFields(typeof(Appointment))
            },
            new DataSourceDto
            {
                Name = "Invoices",
                DisplayName = "Invoices",
                Description = "Billing and invoices",
                Category = "Financial",
                Fields = GetEntityFields(typeof(Invoice))
            },
            new DataSourceDto
            {
                Name = "Payments",
                DisplayName = "Payments",
                Description = "Payment transactions",
                Category = "Financial",
                Fields = GetEntityFields(typeof(Payment))
            },
            new DataSourceDto
            {
                Name = "Prescriptions",
                DisplayName = "Prescriptions",
                Description = "Prescription records",
                Category = "Clinical",
                Fields = GetEntityFields(typeof(Prescription))
            },
            new DataSourceDto
            {
                Name = "LabOrders",
                DisplayName = "Lab Orders",
                Description = "Laboratory orders and results",
                Category = "Clinical",
                Fields = GetEntityFields(typeof(LabOrder))
            },
            new DataSourceDto
            {
                Name = "Visits",
                DisplayName = "Visits",
                Description = "Patient visits and encounters",
                Category = "Clinical",
                Fields = GetEntityFields(typeof(Visit))
            },
            new DataSourceDto
            {
                Name = "InsuranceClaims",
                DisplayName = "Insurance Claims",
                Description = "Insurance claims and submissions",
                Category = "Financial",
                Fields = GetEntityFields(typeof(InsuranceClaim))
            },
            new DataSourceDto
            {
                Name = "Users",
                DisplayName = "Staff",
                Description = "Staff and user records",
                Category = "Administrative",
                Fields = GetEntityFields(typeof(ApplicationUser))
            }
        };

        return await Task.FromResult(dataSources);
    }

    public async Task<IEnumerable<DataSourceFieldDto>> GetDataSourceFieldsAsync(string dataSourceName)
    {
        if (!DataSourceEntityMap.TryGetValue(dataSourceName, out var entityType))
        {
            return Enumerable.Empty<DataSourceFieldDto>();
        }

        return await Task.FromResult(GetEntityFields(entityType));
    }

    public async Task<IEnumerable<DataSourceRelationDto>> GetDataSourceRelationsAsync(string dataSourceName)
    {
        var relations = new List<DataSourceRelationDto>();

        switch (dataSourceName)
        {
            case "Appointments":
                relations.Add(new DataSourceRelationDto
                {
                    RelatedDataSource = "Patients",
                    RelationType = "ManyToOne",
                    LocalField = "PatientId",
                    ForeignField = "Id"
                });
                relations.Add(new DataSourceRelationDto
                {
                    RelatedDataSource = "Users",
                    RelationType = "ManyToOne",
                    LocalField = "DoctorId",
                    ForeignField = "Id"
                });
                break;

            case "Invoices":
                relations.Add(new DataSourceRelationDto
                {
                    RelatedDataSource = "Patients",
                    RelationType = "ManyToOne",
                    LocalField = "PatientId",
                    ForeignField = "Id"
                });
                relations.Add(new DataSourceRelationDto
                {
                    RelatedDataSource = "Payments",
                    RelationType = "OneToMany",
                    LocalField = "Id",
                    ForeignField = "InvoiceId"
                });
                break;
        }

        return await Task.FromResult(relations);
    }

    public async Task<(bool IsValid, string? Error)> ValidateDataSourceQueryAsync(
        string dataSourceName, List<ReportFilterDto>? filters)
    {
        if (!DataSourceEntityMap.ContainsKey(dataSourceName))
        {
            return (false, $"Invalid data source: {dataSourceName}");
        }

        // Validate filter fields exist
        if (filters != null)
        {
            var fields = await GetDataSourceFieldsAsync(dataSourceName);
            var fieldNames = fields.Select(f => f.Name).ToHashSet();

            foreach (var filter in filters)
            {
                if (!fieldNames.Contains(filter.FieldName))
                {
                    return (false, $"Invalid field in filter: {filter.FieldName}");
                }
            }
        }

        return (true, null);
    }

    #endregion

    #region Report Categories & Templates

    public async Task<IEnumerable<ReportCategoryDto>> GetCategoriesAsync(int branchId)
    {
        var categories = await _context.CustomReportDefinitions
            .Where(r => r.BranchId == branchId && !r.IsDeleted)
            .GroupBy(r => r.Category)
            .Select(g => new ReportCategoryDto
            {
                Name = g.Key,
                ReportCount = g.Count()
            })
            .ToListAsync();

        // Add default categories
        var defaultCategories = new[] { "Clinical", "Financial", "Administrative", "Analytics" };
        foreach (var category in defaultCategories)
        {
            if (!categories.Any(c => c.Name == category))
            {
                categories.Add(new ReportCategoryDto { Name = category, ReportCount = 0 });
            }
        }

        return categories.OrderBy(c => c.Name);
    }

    public async Task<IEnumerable<ReportTemplateSummaryDto>> GetTemplatesAsync(int branchId, string? category = null)
    {
        var query = _context.CustomReportDefinitions
            .Where(r => r.BranchId == branchId && r.IsTemplate && !r.IsDeleted);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(r => r.Category == category);
        }

        var templates = await query
            .Select(r => new ReportTemplateSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Category = r.Category,
                DataSource = r.DataSource
            })
            .ToListAsync();

        return templates;
    }

    public async Task<CustomReportDefinitionDto> CreateFromTemplateAsync(
        int branchId, int userId, int templateId, string newName)
    {
        return await CloneReportAsync(templateId, userId, newName);
    }

    #endregion

    #region Report Scheduling

    public async Task<ReportScheduleDto> CreateScheduleAsync(
        int branchId, int userId, CreateReportScheduleDto dto)
    {
        var schedule = new ReportSchedule
        {
            BranchId = branchId,
            ReportId = dto.ReportId,
            ScheduleName = dto.ScheduleName,
            Frequency = dto.Frequency,
            DayOfWeek = dto.DayOfWeek,
            DayOfMonth = dto.DayOfMonth,
            TimeOfDay = dto.TimeOfDay,
            Timezone = dto.Timezone ?? "UTC",
            OutputFormat = dto.OutputFormat,
            RecipientsJson = JsonSerializer.Serialize(dto.Recipients),
            ParametersJson = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : null,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            NextRunAt = CalculateNextRunTime(dto)
        };

        _context.ReportSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return await GetScheduleAsync(schedule.Id) ?? new ReportScheduleDto();
    }

    public async Task<ReportScheduleDto> UpdateScheduleAsync(int scheduleId, CreateReportScheduleDto dto)
    {
        var schedule = await _context.ReportSchedules.FindAsync(scheduleId);
        if (schedule == null)
            return new ReportScheduleDto();

        schedule.ScheduleName = dto.ScheduleName;
        schedule.Frequency = dto.Frequency;
        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.DayOfMonth = dto.DayOfMonth;
        schedule.TimeOfDay = dto.TimeOfDay;
        schedule.Timezone = dto.Timezone ?? "UTC";
        schedule.OutputFormat = dto.OutputFormat;
        schedule.RecipientsJson = JsonSerializer.Serialize(dto.Recipients);
        schedule.ParametersJson = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : null;
        schedule.NextRunAt = CalculateNextRunTime(dto);
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetScheduleAsync(scheduleId) ?? new ReportScheduleDto();
    }

    public async Task<ReportScheduleDto?> GetScheduleAsync(int scheduleId)
    {
        var schedule = await _context.ReportSchedules
            .Include(s => s.Report)
            .FirstOrDefaultAsync(s => s.Id == scheduleId);

        return schedule != null ? MapToScheduleDto(schedule) : null;
    }

    public async Task<IEnumerable<ReportScheduleDto>> GetSchedulesAsync(int branchId, int? reportId = null)
    {
        var query = _context.ReportSchedules
            .Include(s => s.Report)
            .Where(s => s.BranchId == branchId);

        if (reportId.HasValue)
        {
            query = query.Where(s => s.ReportId == reportId);
        }

        var schedules = await query.ToListAsync();
        return schedules.Select(MapToScheduleDto);
    }

    public async Task<bool> DeleteScheduleAsync(int scheduleId)
    {
        var schedule = await _context.ReportSchedules.FindAsync(scheduleId);
        if (schedule == null)
            return false;

        _context.ReportSchedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleScheduleAsync(int scheduleId, bool isActive)
    {
        var schedule = await _context.ReportSchedules.FindAsync(scheduleId);
        if (schedule == null)
            return false;

        schedule.IsActive = isActive;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ProcessScheduledReportsAsync()
    {
        var now = DateTime.UtcNow;
        var pendingSchedules = await _context.ReportSchedules
            .Include(s => s.Report)
            .Where(s => s.IsActive && s.NextRunAt <= now)
            .ToListAsync();

        var processedCount = 0;

        foreach (var schedule in pendingSchedules)
        {
            try
            {
                // Execute the report
                var parameters = !string.IsNullOrEmpty(schedule.ParametersJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(schedule.ParametersJson)
                    : null;

                var exportResult = await ExportReportAsync(
                    schedule.BranchId,
                    schedule.CreatedByUserId,
                    new ExecuteReportRequestDto
                    {
                        ReportId = schedule.ReportId,
                        Parameters = parameters,
                        ExportFormat = schedule.OutputFormat
                    });

                // Send to recipients
                var recipients = JsonSerializer.Deserialize<List<ReportRecipientDto>>(schedule.RecipientsJson ?? "[]");

                if (recipients != null && recipients.Any() && result.Content != null)
                {
                    var contentType = schedule.OutputFormat?.ToLower() switch
                    {
                        "pdf" => "application/pdf",
                        "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "csv" => "text/csv",
                        _ => "application/octet-stream"
                    };

                    var extension = schedule.OutputFormat?.ToLower() switch
                    {
                        "pdf" => ".pdf",
                        "excel" => ".xlsx",
                        "csv" => ".csv",
                        _ => ".dat"
                    };

                    var fileName = $"{report?.Name ?? "Report"}_{now:yyyyMMdd}{extension}";

                    foreach (var recipient in recipients.Where(r => !string.IsNullOrEmpty(r.Email)))
                    {
                        try
                        {
                            var emailMessage = new EmailMessage
                            {
                                To = recipient.Email!,
                                Subject = $"Scheduled Report: {report?.Name ?? "Report"} - {now:MMMM dd, yyyy}",
                                Body = $@"
                                    <html>
                                    <body>
                                    <h2>Scheduled Report</h2>
                                    <p>Please find attached the scheduled report: <strong>{report?.Name}</strong></p>
                                    <p><strong>Report Details:</strong></p>
                                    <ul>
                                        <li>Generated: {now:MMMM dd, yyyy hh:mm tt}</li>
                                        <li>Schedule: {schedule.Frequency}</li>
                                        <li>Format: {schedule.OutputFormat?.ToUpper()}</li>
                                    </ul>
                                    <p>This is an automated email. Please do not reply.</p>
                                    <hr/>
                                    <p style='font-size: 11px; color: #666;'>XenonClinic Reporting System</p>
                                    </body>
                                    </html>",
                                IsHtml = true,
                                Attachments = new List<EmailAttachment>
                                {
                                    new EmailAttachment
                                    {
                                        FileName = fileName,
                                        Content = result.Content,
                                        ContentType = contentType
                                    }
                                }
                            };

                            await _emailService.SendAsync(emailMessage);

                            _logger.LogInformation("Scheduled report {ReportId} sent to {Email}",
                                schedule.ReportId, recipient.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send scheduled report {ReportId} to {Email}",
                                schedule.ReportId, recipient.Email);
                        }
                    }
                }

                schedule.LastRunAt = now;
                schedule.LastRunStatus = "Success";
                schedule.NextRunAt = CalculateNextRunTime(new CreateReportScheduleDto
                {
                    Frequency = schedule.Frequency,
                    DayOfWeek = schedule.DayOfWeek,
                    DayOfMonth = schedule.DayOfMonth,
                    TimeOfDay = schedule.TimeOfDay,
                    Timezone = schedule.Timezone
                });

                processedCount++;

                _logger.LogInformation("Scheduled report {ReportId} executed successfully", schedule.ReportId);
            }
            catch (Exception ex)
            {
                schedule.LastRunAt = now;
                schedule.LastRunStatus = $"Failed: {ex.Message}";
                _logger.LogError(ex, "Error executing scheduled report {ReportId}", schedule.ReportId);
            }
        }

        await _context.SaveChangesAsync();
        return processedCount;
    }

    #endregion

    #region Saved Reports

    public async Task<SavedReportDto> SaveReportAsync(int branchId, int userId, SaveReportRequestDto dto)
    {
        var savedReport = new SavedReport
        {
            BranchId = branchId,
            ReportId = dto.ReportId,
            Name = dto.Name,
            ParametersJson = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : null,
            SavedByUserId = userId,
            SavedAt = DateTime.UtcNow
        };

        _context.SavedReports.Add(savedReport);
        await _context.SaveChangesAsync();

        var report = await _context.CustomReportDefinitions.FindAsync(dto.ReportId);
        var user = await _context.Users.FindAsync(userId);

        return new SavedReportDto
        {
            Id = savedReport.Id,
            ReportId = savedReport.ReportId,
            ReportName = report?.Name ?? "",
            SavedName = savedReport.Name,
            Parameters = dto.Parameters,
            SavedAt = savedReport.SavedAt,
            SavedByUserId = userId,
            SavedByUserName = user != null ? $"{user.FirstName} {user.LastName}" : ""
        };
    }

    public async Task<IEnumerable<SavedReportDto>> GetSavedReportsAsync(int branchId, int userId)
    {
        var savedReports = await _context.SavedReports
            .Include(sr => sr.Report)
            .Include(sr => sr.SavedByUser)
            .Where(sr => sr.BranchId == branchId && sr.SavedByUserId == userId)
            .OrderByDescending(sr => sr.SavedAt)
            .ToListAsync();

        return savedReports.Select(sr => new SavedReportDto
        {
            Id = sr.Id,
            ReportId = sr.ReportId,
            ReportName = sr.Report?.Name ?? "",
            SavedName = sr.Name,
            Parameters = !string.IsNullOrEmpty(sr.ParametersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(sr.ParametersJson)
                : null,
            SavedAt = sr.SavedAt,
            SavedByUserId = sr.SavedByUserId,
            SavedByUserName = sr.SavedByUser != null ? $"{sr.SavedByUser.FirstName} {sr.SavedByUser.LastName}" : ""
        });
    }

    public async Task<bool> DeleteSavedReportAsync(int savedReportId)
    {
        var savedReport = await _context.SavedReports.FindAsync(savedReportId);
        if (savedReport == null)
            return false;

        _context.SavedReports.Remove(savedReport);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ExecuteReportResponseDto> ExecuteSavedReportAsync(int branchId, int userId, int savedReportId)
    {
        var savedReport = await _context.SavedReports.FindAsync(savedReportId);
        if (savedReport == null)
        {
            return new ExecuteReportResponseDto();
        }

        var parameters = !string.IsNullOrEmpty(savedReport.ParametersJson)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(savedReport.ParametersJson)
            : null;

        return await ExecuteReportAsync(branchId, userId, new ExecuteReportRequestDto
        {
            ReportId = savedReport.ReportId,
            Parameters = parameters
        });
    }

    #endregion

    #region Report History

    public async Task<(IEnumerable<ReportExecutionHistoryDto> Items, int TotalCount)> GetExecutionHistoryAsync(
        int branchId, ReportHistoryFilterDto filter)
    {
        var query = _context.ReportExecutionHistory
            .Include(h => h.Report)
            .Include(h => h.ExecutedByUser)
            .Where(h => h.BranchId == branchId);

        if (filter.ReportId.HasValue)
            query = query.Where(h => h.ReportId == filter.ReportId);

        if (filter.UserId.HasValue)
            query = query.Where(h => h.ExecutedByUserId == filter.UserId);

        if (filter.FromDate.HasValue)
            query = query.Where(h => h.ExecutedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(h => h.ExecutedAt <= filter.ToDate);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(h => h.Status == filter.Status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.ExecutedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(h => new ReportExecutionHistoryDto
            {
                Id = h.Id,
                ReportId = h.ReportId,
                ReportName = h.Report!.Name,
                ExecutedByUserId = h.ExecutedByUserId,
                ExecutedByUserName = h.ExecutedByUser != null
                    ? $"{h.ExecutedByUser.FirstName} {h.ExecutedByUser.LastName}"
                    : "",
                ExecutedAt = h.ExecutedAt,
                ExecutionTimeMs = h.ExecutionTimeMs,
                RecordCount = h.RecordCount,
                ExportFormat = h.ExportFormat,
                Status = h.Status,
                Error = h.Error
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dictionary<string, object>> GetReportStatisticsAsync(int branchId, int reportId)
    {
        var history = await _context.ReportExecutionHistory
            .Where(h => h.BranchId == branchId && h.ReportId == reportId)
            .ToListAsync();

        return new Dictionary<string, object>
        {
            ["TotalExecutions"] = history.Count,
            ["SuccessfulExecutions"] = history.Count(h => h.Status == "Success"),
            ["FailedExecutions"] = history.Count(h => h.Status == "Failed"),
            ["AverageExecutionTime"] = history.Any() ? history.Average(h => h.ExecutionTimeMs) : 0,
            ["TotalRecordsGenerated"] = history.Sum(h => h.RecordCount),
            ["LastExecuted"] = history.MaxBy(h => h.ExecutedAt)?.ExecutedAt
        };
    }

    #endregion

    #region Dashboard Widgets

    public async Task<ReportWidgetDto> CreateWidgetAsync(int branchId, int userId, CreateReportWidgetDto dto)
    {
        var widget = new ReportWidget
        {
            BranchId = branchId,
            ReportId = dto.ReportId,
            WidgetName = dto.WidgetName,
            WidgetType = dto.WidgetType,
            Width = dto.Width,
            Height = dto.Height,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            VisualizationJson = dto.Visualization != null ? JsonSerializer.Serialize(dto.Visualization) : null,
            ParametersJson = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : null,
            RefreshIntervalSeconds = dto.RefreshIntervalSeconds,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReportWidgets.Add(widget);
        await _context.SaveChangesAsync();

        return MapToWidgetDto(widget);
    }

    public async Task<ReportWidgetDto> UpdateWidgetAsync(int widgetId, CreateReportWidgetDto dto)
    {
        var widget = await _context.ReportWidgets.FindAsync(widgetId);
        if (widget == null)
            return new ReportWidgetDto();

        widget.WidgetName = dto.WidgetName;
        widget.WidgetType = dto.WidgetType;
        widget.Width = dto.Width;
        widget.Height = dto.Height;
        widget.PositionX = dto.PositionX;
        widget.PositionY = dto.PositionY;
        widget.VisualizationJson = dto.Visualization != null ? JsonSerializer.Serialize(dto.Visualization) : null;
        widget.ParametersJson = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : null;
        widget.RefreshIntervalSeconds = dto.RefreshIntervalSeconds;
        widget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToWidgetDto(widget);
    }

    public async Task<IEnumerable<ReportWidgetDto>> GetWidgetsAsync(int branchId, int userId)
    {
        var widgets = await _context.ReportWidgets
            .Where(w => w.BranchId == branchId && w.CreatedByUserId == userId)
            .OrderBy(w => w.PositionY)
            .ThenBy(w => w.PositionX)
            .ToListAsync();

        return widgets.Select(MapToWidgetDto);
    }

    public async Task<bool> DeleteWidgetAsync(int widgetId)
    {
        var widget = await _context.ReportWidgets.FindAsync(widgetId);
        if (widget == null)
            return false;

        _context.ReportWidgets.Remove(widget);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ExecuteReportResponseDto> GetWidgetDataAsync(int widgetId)
    {
        var widget = await _context.ReportWidgets.FindAsync(widgetId);
        if (widget == null)
        {
            return new ExecuteReportResponseDto();
        }

        var parameters = !string.IsNullOrEmpty(widget.ParametersJson)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(widget.ParametersJson)
            : null;

        return await ExecuteReportAsync(widget.BranchId, widget.CreatedByUserId, new ExecuteReportRequestDto
        {
            ReportId = widget.ReportId,
            Parameters = parameters,
            PageSize = 100 // Widget data is typically limited
        });
    }

    #endregion

    #region Report Permissions

    public async Task<IEnumerable<ReportPermissionDto>> GetPermissionsAsync(int reportId)
    {
        var permissions = await _context.ReportPermissions
            .Include(p => p.User)
            .Include(p => p.Role)
            .Where(p => p.ReportId == reportId)
            .ToListAsync();

        return permissions.Select(p => new ReportPermissionDto
        {
            ReportId = p.ReportId,
            UserId = p.UserId,
            UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : null,
            RoleId = p.RoleId,
            RoleName = p.Role?.Name,
            CanView = p.CanView,
            CanEdit = p.CanEdit,
            CanExport = p.CanExport,
            CanSchedule = p.CanSchedule,
            CanShare = p.CanShare
        });
    }

    public async Task<bool> UpdatePermissionsAsync(UpdateReportPermissionsDto dto)
    {
        // Remove existing permissions
        var existingPermissions = await _context.ReportPermissions
            .Where(p => p.ReportId == dto.ReportId)
            .ToListAsync();

        _context.ReportPermissions.RemoveRange(existingPermissions);

        // Add new permissions
        foreach (var permission in dto.Permissions)
        {
            var reportPermission = new ReportPermission
            {
                ReportId = dto.ReportId,
                UserId = permission.UserId,
                RoleId = permission.RoleId,
                CanView = permission.CanView,
                CanEdit = permission.CanEdit,
                CanExport = permission.CanExport,
                CanSchedule = permission.CanSchedule,
                CanShare = permission.CanShare
            };
            _context.ReportPermissions.Add(reportPermission);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanUserAccessReportAsync(int reportId, int userId, string permission)
    {
        var report = await _context.CustomReportDefinitions.FindAsync(reportId);
        if (report == null)
            return false;

        // Creator always has full access
        if (report.CreatedByUserId == userId)
            return true;

        // Public reports can be viewed by all
        if (report.IsPublic && permission == "View")
            return true;

        // Check specific permission
        var reportPermission = await _context.ReportPermissions
            .FirstOrDefaultAsync(p => p.ReportId == reportId && p.UserId == userId);

        if (reportPermission == null)
            return false;

        return permission switch
        {
            "View" => reportPermission.CanView,
            "Edit" => reportPermission.CanEdit,
            "Export" => reportPermission.CanExport,
            "Schedule" => reportPermission.CanSchedule,
            "Share" => reportPermission.CanShare,
            _ => false
        };
    }

    #endregion

    #region Private Methods

    private async Task<(List<Dictionary<string, object>> Data, int TotalCount)> ExecuteDynamicQueryAsync(
        int branchId,
        string dataSource,
        List<ReportColumnDto> columns,
        List<ReportFilterDto>? filters,
        List<ReportSortDto>? sorting,
        int page,
        int pageSize)
    {
        // Build dynamic SQL query based on data source and columns
        // This is a simplified implementation - in production, use proper query builder

        var columnNames = columns.Select(c => c.FieldName).ToList();
        var whereClause = BuildWhereClause(filters, branchId);
        var orderByClause = BuildOrderByClause(sorting);

        // Execute query using raw SQL for flexibility
        var sql = $@"
            SELECT {string.Join(", ", columnNames.Select(c => $"\"{c}\""))}
            FROM ""{dataSource}""
            WHERE {whereClause}
            {orderByClause}
            OFFSET {(page - 1) * pageSize} LIMIT {pageSize}";

        var countSql = $@"
            SELECT COUNT(*)
            FROM ""{dataSource}""
            WHERE {whereClause}";

        // For now, return simulated data
        // In production, execute actual SQL queries
        var data = new List<Dictionary<string, object>>();
        var totalCount = 0;

        // Simulate data based on data source
        if (DataSourceEntityMap.TryGetValue(dataSource, out var entityType))
        {
            // This would be replaced with actual database query
            _logger.LogInformation("Executing report query for {DataSource}", dataSource);
        }

        return (data, totalCount);
    }

    private async Task<ReportSummaryDto> CalculateAggregatesAsync(
        int branchId,
        string dataSource,
        List<ReportColumnDto> columns,
        List<ReportFilterDto>? filters,
        ReportAggregationDto aggregation,
        List<ReportGroupingDto>? grouping)
    {
        var summary = new ReportSummaryDto
        {
            Aggregates = new Dictionary<string, object>()
        };

        // Calculate aggregates
        foreach (var aggColumn in aggregation.Columns)
        {
            // This would execute actual aggregate queries
            summary.Aggregates[$"{aggColumn.Function}_{aggColumn.FieldName}"] = 0;
        }

        return await Task.FromResult(summary);
    }

    private static string BuildWhereClause(List<ReportFilterDto>? filters, int branchId)
    {
        var conditions = new List<string> { $"\"BranchId\" = {branchId}" };

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                var condition = filter.Operator switch
                {
                    "Equals" => $"\"{filter.FieldName}\" = '{filter.Value}'",
                    "NotEquals" => $"\"{filter.FieldName}\" != '{filter.Value}'",
                    "Contains" => $"\"{filter.FieldName}\" LIKE '%{filter.Value}%'",
                    "StartsWith" => $"\"{filter.FieldName}\" LIKE '{filter.Value}%'",
                    "EndsWith" => $"\"{filter.FieldName}\" LIKE '%{filter.Value}'",
                    "GreaterThan" => $"\"{filter.FieldName}\" > '{filter.Value}'",
                    "GreaterThanOrEqual" => $"\"{filter.FieldName}\" >= '{filter.Value}'",
                    "LessThan" => $"\"{filter.FieldName}\" < '{filter.Value}'",
                    "LessThanOrEqual" => $"\"{filter.FieldName}\" <= '{filter.Value}'",
                    "Between" => $"\"{filter.FieldName}\" BETWEEN '{filter.Value}' AND '{filter.Value2}'",
                    "IsNull" => $"\"{filter.FieldName}\" IS NULL",
                    "IsNotNull" => $"\"{filter.FieldName}\" IS NOT NULL",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }
        }

        return string.Join(" AND ", conditions);
    }

    private static string BuildOrderByClause(List<ReportSortDto>? sorting)
    {
        if (sorting == null || !sorting.Any())
            return "";

        var orderBy = sorting
            .OrderBy(s => s.Order)
            .Select(s => $"\"{s.FieldName}\" {(s.Direction == "Descending" ? "DESC" : "ASC")}");

        return $"ORDER BY {string.Join(", ", orderBy)}";
    }

    private async Task LogExecutionAsync(
        int reportId, int userId, Dictionary<string, object>? parameters,
        int recordCount, long executionTimeMs, string status, string? error = null)
    {
        var report = await _context.CustomReportDefinitions.FindAsync(reportId);

        var history = new ReportExecutionHistoryEntry
        {
            BranchId = report?.BranchId ?? 0,
            ReportId = reportId,
            ExecutedByUserId = userId,
            ExecutedAt = DateTime.UtcNow,
            ExecutionTimeMs = executionTimeMs,
            RecordCount = recordCount,
            ParametersJson = parameters != null ? JsonSerializer.Serialize(parameters) : null,
            Status = status,
            Error = error
        };

        _context.ReportExecutionHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    private static List<DataSourceFieldDto> GetEntityFields(Type entityType)
    {
        var fields = new List<DataSourceFieldDto>();

        foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldType = GetFieldDataType(property.PropertyType);
            if (fieldType == null)
                continue;

            fields.Add(new DataSourceFieldDto
            {
                Name = property.Name,
                DisplayName = SplitCamelCase(property.Name),
                DataType = fieldType,
                IsNullable = Nullable.GetUnderlyingType(property.PropertyType) != null
            });
        }

        return fields;
    }

    private static string? GetFieldDataType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            "String" => "String",
            "Int32" or "Int64" or "Decimal" or "Double" or "Single" => "Number",
            "DateTime" or "DateTimeOffset" => "Date",
            "Boolean" => "Boolean",
            _ when underlyingType.IsEnum => "String",
            _ => null
        };
    }

    private static string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }

    private static DateTime CalculateNextRunTime(CreateReportScheduleDto dto)
    {
        var now = DateTime.UtcNow;
        var timeOfDay = TimeSpan.TryParse(dto.TimeOfDay, out var time) ? time : TimeSpan.FromHours(8);

        return dto.Frequency switch
        {
            "Daily" => now.Date.AddDays(1).Add(timeOfDay),
            "Weekly" => GetNextWeekday(now, dto.DayOfWeek).Add(timeOfDay),
            "Monthly" => new DateTime(now.Year, now.Month, dto.DayOfMonth ?? 1).AddMonths(1).Add(timeOfDay),
            "Quarterly" => new DateTime(now.Year, ((now.Month - 1) / 3 + 1) * 3 + 1, dto.DayOfMonth ?? 1).Add(timeOfDay),
            "Yearly" => new DateTime(now.Year + 1, 1, dto.DayOfMonth ?? 1).Add(timeOfDay),
            _ => now.AddDays(1)
        };
    }

    private static DateTime GetNextWeekday(DateTime start, string? dayOfWeek)
    {
        if (!Enum.TryParse<DayOfWeek>(dayOfWeek, out var targetDay))
            targetDay = DayOfWeek.Monday;

        var daysUntilTarget = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
            daysUntilTarget = 7;

        return start.Date.AddDays(daysUntilTarget);
    }

    private static byte[] GeneratePdfReport(ExecuteReportResponseDto data)
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(data.ReportName).FontSize(18).Bold();
                            col.Item().Text($"Generated: {data.ExecutedAt:yyyy-MM-dd HH:mm}")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"Total Records: {data.TotalRecords}")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });
                });

                // Content - Data Table
                page.Content().PaddingVertical(10).Element(content =>
                {
                    content.Table(table =>
                    {
                        var visibleColumns = data.Columns.Where(c => c.Visible).ToList();

                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var col in visibleColumns)
                            {
                                columns.RelativeColumn(col.Width > 0 ? col.Width : 1);
                            }
                        });

                        // Header row
                        table.Header(header =>
                        {
                            foreach (var col in visibleColumns)
                            {
                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Padding(5)
                                    .Text(col.DisplayName ?? col.FieldName)
                                    .FontColor(Colors.White)
                                    .Bold();
                            }
                        });

                        // Data rows
                        foreach (var row in data.Data)
                        {
                            foreach (var col in visibleColumns)
                            {
                                var value = row.GetValueOrDefault(col.FieldName)?.ToString() ?? "";
                                var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

                                // Apply alignment
                                if (col.Alignment?.ToLower() == "right" || col.DataType == "Number" || col.DataType == "Currency")
                                    cell.AlignRight();
                                else if (col.Alignment?.ToLower() == "center")
                                    cell.AlignCenter();

                                cell.Text(FormatCellValue(value, col));
                            }
                        }
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
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

    private static byte[] GenerateExcelReport(ExecuteReportResponseDto data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report Data");

        var visibleColumns = data.Columns.Where(c => c.Visible).ToList();

        // Add title
        worksheet.Cell(1, 1).Value = data.ReportName;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, visibleColumns.Count).Merge();

        // Add metadata
        worksheet.Cell(2, 1).Value = $"Generated: {data.ExecutedAt:yyyy-MM-dd HH:mm} | Total Records: {data.TotalRecords}";
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
        worksheet.Range(2, 1, 2, visibleColumns.Count).Merge();

        // Header row (row 4)
        var headerRow = 4;
        for (int i = 0; i < visibleColumns.Count; i++)
        {
            var col = visibleColumns[i];
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = col.DisplayName ?? col.FieldName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F6FEB");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        var dataRowStart = headerRow + 1;
        for (int rowIdx = 0; rowIdx < data.Data.Count; rowIdx++)
        {
            var row = data.Data[rowIdx];
            for (int colIdx = 0; colIdx < visibleColumns.Count; colIdx++)
            {
                var col = visibleColumns[colIdx];
                var cell = worksheet.Cell(dataRowStart + rowIdx, colIdx + 1);
                var value = row.GetValueOrDefault(col.FieldName);

                // Set value with appropriate type
                if (value != null)
                {
                    if (col.DataType == "Number" || col.DataType == "Currency")
                    {
                        if (decimal.TryParse(value.ToString(), out var numValue))
                            cell.Value = numValue;
                        else
                            cell.Value = value.ToString();

                        if (col.DataType == "Currency")
                            cell.Style.NumberFormat.Format = "$#,##0.00";
                    }
                    else if (col.DataType == "Date" && DateTime.TryParse(value.ToString(), out var dateValue))
                    {
                        cell.Value = dateValue;
                        cell.Style.NumberFormat.Format = col.Format ?? "yyyy-MM-dd";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                }

                // Apply alignment
                if (col.Alignment?.ToLower() == "right" || col.DataType == "Number" || col.DataType == "Currency")
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                else if (col.Alignment?.ToLower() == "center")
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Add table formatting
        var tableRange = worksheet.Range(headerRow, 1, dataRowStart + data.Data.Count - 1, visibleColumns.Count);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Alternate row colors for data
        for (int rowIdx = 0; rowIdx < data.Data.Count; rowIdx++)
        {
            if (rowIdx % 2 == 1)
            {
                worksheet.Range(dataRowStart + rowIdx, 1, dataRowStart + rowIdx, visibleColumns.Count)
                    .Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string FormatCellValue(string value, ReportColumnDto column)
    {
        if (string.IsNullOrEmpty(value)) return value;

        return column.DataType switch
        {
            "Currency" when decimal.TryParse(value, out var currency) => currency.ToString("C2"),
            "Number" when decimal.TryParse(value, out var number) => number.ToString(column.Format ?? "N2"),
            "Date" when DateTime.TryParse(value, out var date) => date.ToString(column.Format ?? "yyyy-MM-dd"),
            _ => value
        };
    }

    private static byte[] GenerateCsvReport(ExecuteReportResponseDto data)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",", data.Columns.Select(c => $"\"{c.DisplayName ?? c.FieldName}\"")));

        // Data rows
        foreach (var row in data.Data)
        {
            var values = data.Columns.Select(c =>
            {
                var value = row.GetValueOrDefault(c.FieldName)?.ToString() ?? "";
                return $"\"{value.Replace("\"", "\"\"")}\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private CustomReportDefinitionDto MapToReportDto(CustomReportDefinition report)
    {
        return new CustomReportDefinitionDto
        {
            Id = report.Id,
            Name = report.Name,
            Description = report.Description,
            Category = report.Category,
            DataSource = report.DataSource,
            Columns = JsonSerializer.Deserialize<List<ReportColumnDto>>(report.ColumnsJson) ?? new List<ReportColumnDto>(),
            Filters = !string.IsNullOrEmpty(report.FiltersJson)
                ? JsonSerializer.Deserialize<List<ReportFilterDto>>(report.FiltersJson)
                : null,
            Sorting = !string.IsNullOrEmpty(report.SortingJson)
                ? JsonSerializer.Deserialize<List<ReportSortDto>>(report.SortingJson)
                : null,
            Grouping = !string.IsNullOrEmpty(report.GroupingJson)
                ? JsonSerializer.Deserialize<List<ReportGroupingDto>>(report.GroupingJson)
                : null,
            Aggregation = !string.IsNullOrEmpty(report.AggregationJson)
                ? JsonSerializer.Deserialize<ReportAggregationDto>(report.AggregationJson)
                : null,
            Visualization = !string.IsNullOrEmpty(report.VisualizationJson)
                ? JsonSerializer.Deserialize<ReportVisualizationDto>(report.VisualizationJson)
                : null,
            Layout = !string.IsNullOrEmpty(report.LayoutJson)
                ? JsonSerializer.Deserialize<ReportLayoutDto>(report.LayoutJson)
                : null,
            IsPublic = report.IsPublic,
            IsTemplate = report.IsTemplate,
            CreatedByUserId = report.CreatedByUserId,
            CreatedByUserName = report.CreatedByUser != null
                ? $"{report.CreatedByUser.FirstName} {report.CreatedByUser.LastName}"
                : "",
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
    }

    private ReportScheduleDto MapToScheduleDto(ReportSchedule schedule)
    {
        return new ReportScheduleDto
        {
            Id = schedule.Id,
            ReportId = schedule.ReportId,
            ReportName = schedule.Report?.Name ?? "",
            ScheduleName = schedule.ScheduleName,
            Frequency = schedule.Frequency,
            DayOfWeek = schedule.DayOfWeek,
            DayOfMonth = schedule.DayOfMonth,
            TimeOfDay = schedule.TimeOfDay,
            Timezone = schedule.Timezone,
            OutputFormat = schedule.OutputFormat,
            Recipients = !string.IsNullOrEmpty(schedule.RecipientsJson)
                ? JsonSerializer.Deserialize<List<ReportRecipientDto>>(schedule.RecipientsJson) ?? new List<ReportRecipientDto>()
                : new List<ReportRecipientDto>(),
            Parameters = !string.IsNullOrEmpty(schedule.ParametersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(schedule.ParametersJson)
                : null,
            IsActive = schedule.IsActive,
            NextRunAt = schedule.NextRunAt,
            LastRunAt = schedule.LastRunAt,
            LastRunStatus = schedule.LastRunStatus
        };
    }

    private static ReportWidgetDto MapToWidgetDto(ReportWidget widget)
    {
        return new ReportWidgetDto
        {
            Id = widget.Id,
            ReportId = widget.ReportId,
            WidgetName = widget.WidgetName,
            WidgetType = widget.WidgetType,
            Width = widget.Width,
            Height = widget.Height,
            PositionX = widget.PositionX,
            PositionY = widget.PositionY,
            Visualization = !string.IsNullOrEmpty(widget.VisualizationJson)
                ? JsonSerializer.Deserialize<ReportVisualizationDto>(widget.VisualizationJson)
                : null,
            Parameters = !string.IsNullOrEmpty(widget.ParametersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(widget.ParametersJson)
                : null,
            RefreshIntervalSeconds = widget.RefreshIntervalSeconds
        };
    }

    #endregion
}

#region Custom Report Entities

/// <summary>
/// Custom report definition entity
/// </summary>
public class CustomReportDefinition
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string ColumnsJson { get; set; } = string.Empty;
    public string? FiltersJson { get; set; }
    public string? SortingJson { get; set; }
    public string? GroupingJson { get; set; }
    public string? AggregationJson { get; set; }
    public string? VisualizationJson { get; set; }
    public string? LayoutJson { get; set; }
    public bool IsPublic { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ApplicationUser? CreatedByUser { get; set; }
}

/// <summary>
/// Report schedule entity
/// </summary>
public class ReportSchedule
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int ReportId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string? TimeOfDay { get; set; }
    public string? Timezone { get; set; }
    public string OutputFormat { get; set; } = string.Empty;
    public string? RecipientsJson { get; set; }
    public string? ParametersJson { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }

    public CustomReportDefinition? Report { get; set; }
}

/// <summary>
/// Saved report entity
/// </summary>
public class SavedReport
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int ReportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ParametersJson { get; set; }
    public int SavedByUserId { get; set; }
    public DateTime SavedAt { get; set; }

    public CustomReportDefinition? Report { get; set; }
    public ApplicationUser? SavedByUser { get; set; }
}

/// <summary>
/// Report execution history entity
/// </summary>
public class ReportExecutionHistoryEntry
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int ReportId { get; set; }
    public int ExecutedByUserId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
    public string? ExportFormat { get; set; }
    public string? ParametersJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }

    public CustomReportDefinition? Report { get; set; }
    public ApplicationUser? ExecutedByUser { get; set; }
}

/// <summary>
/// Report widget entity
/// </summary>
public class ReportWidget
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int ReportId { get; set; }
    public string WidgetName { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public string? VisualizationJson { get; set; }
    public string? ParametersJson { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Report permission entity
/// </summary>
public class ReportPermission
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public int? UserId { get; set; }
    public int? RoleId { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanExport { get; set; }
    public bool CanSchedule { get; set; }
    public bool CanShare { get; set; }

    public CustomReportDefinition? Report { get; set; }
    public ApplicationUser? User { get; set; }
    public Role? Role { get; set; }
}

#endregion
