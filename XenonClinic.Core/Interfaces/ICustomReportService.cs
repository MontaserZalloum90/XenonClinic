using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Custom Report Builder operations
/// </summary>
public interface ICustomReportService
{
    #region Report Definitions

    /// <summary>
    /// Create a custom report definition
    /// </summary>
    Task<CustomReportDefinitionDto> CreateReportAsync(int branchId, int userId, CreateCustomReportDto dto);

    /// <summary>
    /// Update a custom report definition
    /// </summary>
    Task<CustomReportDefinitionDto> UpdateReportAsync(int reportId, UpdateCustomReportDto dto);

    /// <summary>
    /// Get a custom report definition
    /// </summary>
    Task<CustomReportDefinitionDto?> GetReportAsync(int reportId);

    /// <summary>
    /// Get all custom reports for a branch
    /// </summary>
    Task<IEnumerable<CustomReportDefinitionDto>> GetReportsAsync(int branchId, int userId, string? category = null);

    /// <summary>
    /// Delete a custom report
    /// </summary>
    Task<bool> DeleteReportAsync(int reportId);

    /// <summary>
    /// Clone a custom report
    /// </summary>
    Task<CustomReportDefinitionDto> CloneReportAsync(int reportId, int userId, string newName);

    #endregion

    #region Report Execution

    /// <summary>
    /// Execute a custom report
    /// </summary>
    Task<ExecuteReportResponseDto> ExecuteReportAsync(int branchId, int userId, ExecuteReportRequestDto request);

    /// <summary>
    /// Export a custom report to file
    /// </summary>
    Task<ReportExportResultDto> ExportReportAsync(int branchId, int userId, ExecuteReportRequestDto request);

    /// <summary>
    /// Preview report with sample data
    /// </summary>
    Task<ExecuteReportResponseDto> PreviewReportAsync(int branchId, CreateCustomReportDto dto, int sampleSize = 10);

    #endregion

    #region Data Sources

    /// <summary>
    /// Get available data sources for reports
    /// </summary>
    Task<IEnumerable<DataSourceDto>> GetDataSourcesAsync(int branchId);

    /// <summary>
    /// Get fields for a specific data source
    /// </summary>
    Task<IEnumerable<DataSourceFieldDto>> GetDataSourceFieldsAsync(string dataSourceName);

    /// <summary>
    /// Get related data sources
    /// </summary>
    Task<IEnumerable<DataSourceRelationDto>> GetDataSourceRelationsAsync(string dataSourceName);

    /// <summary>
    /// Validate data source query
    /// </summary>
    Task<(bool IsValid, string? Error)> ValidateDataSourceQueryAsync(string dataSourceName, List<ReportFilterDto>? filters);

    #endregion

    #region Report Categories & Templates

    /// <summary>
    /// Get report categories
    /// </summary>
    Task<IEnumerable<ReportCategoryDto>> GetCategoriesAsync(int branchId);

    /// <summary>
    /// Get report templates
    /// </summary>
    Task<IEnumerable<ReportTemplateSummaryDto>> GetTemplatesAsync(int branchId, string? category = null);

    /// <summary>
    /// Create report from template
    /// </summary>
    Task<CustomReportDefinitionDto> CreateFromTemplateAsync(int branchId, int userId, int templateId, string newName);

    #endregion

    #region Report Scheduling

    /// <summary>
    /// Create a report schedule
    /// </summary>
    Task<ReportScheduleDto> CreateScheduleAsync(int branchId, int userId, CreateReportScheduleDto dto);

    /// <summary>
    /// Update a report schedule
    /// </summary>
    Task<ReportScheduleDto> UpdateScheduleAsync(int scheduleId, CreateReportScheduleDto dto);

    /// <summary>
    /// Get a report schedule
    /// </summary>
    Task<ReportScheduleDto?> GetScheduleAsync(int scheduleId);

    /// <summary>
    /// Get all schedules for a report
    /// </summary>
    Task<IEnumerable<ReportScheduleDto>> GetSchedulesAsync(int branchId, int? reportId = null);

    /// <summary>
    /// Delete a report schedule
    /// </summary>
    Task<bool> DeleteScheduleAsync(int scheduleId);

    /// <summary>
    /// Enable/disable a report schedule
    /// </summary>
    Task<bool> ToggleScheduleAsync(int scheduleId, bool isActive);

    /// <summary>
    /// Run scheduled reports (called by background job)
    /// </summary>
    Task<int> ProcessScheduledReportsAsync();

    #endregion

    #region Saved Reports

    /// <summary>
    /// Save a report with specific parameters
    /// </summary>
    Task<SavedReportDto> SaveReportAsync(int branchId, int userId, SaveReportRequestDto dto);

    /// <summary>
    /// Get saved reports for a user
    /// </summary>
    Task<IEnumerable<SavedReportDto>> GetSavedReportsAsync(int branchId, int userId);

    /// <summary>
    /// Delete a saved report
    /// </summary>
    Task<bool> DeleteSavedReportAsync(int savedReportId);

    /// <summary>
    /// Execute a saved report
    /// </summary>
    Task<ExecuteReportResponseDto> ExecuteSavedReportAsync(int branchId, int userId, int savedReportId);

    #endregion

    #region Report History

    /// <summary>
    /// Get report execution history
    /// </summary>
    Task<(IEnumerable<ReportExecutionHistoryDto> Items, int TotalCount)> GetExecutionHistoryAsync(
        int branchId, ReportHistoryFilterDto filter);

    /// <summary>
    /// Get report usage statistics
    /// </summary>
    Task<Dictionary<string, object>> GetReportStatisticsAsync(int branchId, int reportId);

    #endregion

    #region Dashboard Widgets

    /// <summary>
    /// Create a dashboard widget from a report
    /// </summary>
    Task<ReportWidgetDto> CreateWidgetAsync(int branchId, int userId, CreateReportWidgetDto dto);

    /// <summary>
    /// Update a dashboard widget
    /// </summary>
    Task<ReportWidgetDto> UpdateWidgetAsync(int widgetId, CreateReportWidgetDto dto);

    /// <summary>
    /// Get dashboard widgets for a user
    /// </summary>
    Task<IEnumerable<ReportWidgetDto>> GetWidgetsAsync(int branchId, int userId);

    /// <summary>
    /// Delete a dashboard widget
    /// </summary>
    Task<bool> DeleteWidgetAsync(int widgetId);

    /// <summary>
    /// Get widget data
    /// </summary>
    Task<ExecuteReportResponseDto> GetWidgetDataAsync(int widgetId);

    #endregion

    #region Report Permissions

    /// <summary>
    /// Get report permissions
    /// </summary>
    Task<IEnumerable<ReportPermissionDto>> GetPermissionsAsync(int reportId);

    /// <summary>
    /// Update report permissions
    /// </summary>
    Task<bool> UpdatePermissionsAsync(UpdateReportPermissionsDto dto);

    /// <summary>
    /// Check if user can access report
    /// </summary>
    Task<bool> CanUserAccessReportAsync(int reportId, int userId, string permission);

    #endregion
}
