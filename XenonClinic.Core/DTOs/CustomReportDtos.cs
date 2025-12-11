namespace XenonClinic.Core.DTOs;

#region Report Definition DTOs

/// <summary>
/// Custom report definition DTO
/// </summary>
public class CustomReportDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<ReportFilterDto>? Filters { get; set; }
    public List<ReportSortDto>? Sorting { get; set; }
    public List<ReportGroupingDto>? Grouping { get; set; }
    public ReportAggregationDto? Aggregation { get; set; }
    public ReportVisualizationDto? Visualization { get; set; }
    public ReportLayoutDto? Layout { get; set; }
    public bool IsPublic { get; set; }
    public bool IsTemplate { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Report column definition
/// </summary>
public class ReportColumnDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DataType { get; set; } = string.Empty; // String, Number, Date, Boolean, Currency
    public string? Format { get; set; } // Date format, number format, etc.
    public int Width { get; set; }
    public string? Alignment { get; set; } // Left, Center, Right
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public string? AggregateFunction { get; set; } // Sum, Avg, Count, Min, Max
    public string? ConditionalFormatting { get; set; }
    public string? Formula { get; set; } // For calculated columns
}

/// <summary>
/// Report filter definition
/// </summary>
public class ReportFilterDto
{
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty; // Equals, NotEquals, Contains, StartsWith, GreaterThan, etc.
    public object? Value { get; set; }
    public object? Value2 { get; set; } // For Between operator
    public string? LogicalOperator { get; set; } // And, Or
    public bool IsParameter { get; set; } // If true, value is provided at runtime
    public string? ParameterName { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Report sort definition
/// </summary>
public class ReportSortDto
{
    public string FieldName { get; set; } = string.Empty;
    public string Direction { get; set; } = "Ascending"; // Ascending, Descending
    public int Order { get; set; }
}

/// <summary>
/// Report grouping definition
/// </summary>
public class ReportGroupingDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? GroupingInterval { get; set; } // For dates: Day, Week, Month, Quarter, Year
    public bool ShowSubtotals { get; set; } = true;
    public bool Collapsed { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// Report aggregation definition
/// </summary>
public class ReportAggregationDto
{
    public List<ReportAggregateColumnDto> Columns { get; set; } = new();
    public bool ShowGrandTotal { get; set; }
}

/// <summary>
/// Report aggregate column
/// </summary>
public class ReportAggregateColumnDto
{
    public string FieldName { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty; // Sum, Avg, Count, Min, Max, CountDistinct
    public string? DisplayName { get; set; }
}

/// <summary>
/// Report visualization settings
/// </summary>
public class ReportVisualizationDto
{
    public string ChartType { get; set; } = string.Empty; // None, Bar, Line, Pie, Area, Doughnut, Scatter
    public string? XAxisField { get; set; }
    public List<string>? YAxisFields { get; set; }
    public string? SeriesField { get; set; }
    public string? Title { get; set; }
    public bool ShowLegend { get; set; } = true;
    public bool ShowLabels { get; set; } = true;
    public string? ColorScheme { get; set; }
}

/// <summary>
/// Report layout settings
/// </summary>
public class ReportLayoutDto
{
    public string Orientation { get; set; } = "Portrait"; // Portrait, Landscape
    public string PaperSize { get; set; } = "A4"; // A4, Letter, Legal
    public decimal MarginTop { get; set; } = 1;
    public decimal MarginBottom { get; set; } = 1;
    public decimal MarginLeft { get; set; } = 1;
    public decimal MarginRight { get; set; } = 1;
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public bool ShowPageNumbers { get; set; } = true;
    public bool ShowDateTime { get; set; } = true;
    public bool ShowLogo { get; set; } = true;
}

#endregion

#region Create/Update Report DTOs

/// <summary>
/// Create custom report request
/// </summary>
public class CreateCustomReportDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<ReportFilterDto>? Filters { get; set; }
    public List<ReportSortDto>? Sorting { get; set; }
    public List<ReportGroupingDto>? Grouping { get; set; }
    public ReportAggregationDto? Aggregation { get; set; }
    public ReportVisualizationDto? Visualization { get; set; }
    public ReportLayoutDto? Layout { get; set; }
    public bool IsPublic { get; set; }
    public bool IsTemplate { get; set; }
}

/// <summary>
/// Update custom report request
/// </summary>
public class UpdateCustomReportDto : CreateCustomReportDto
{
}

#endregion

#region Report Execution DTOs

/// <summary>
/// Execute report request
/// </summary>
public class ExecuteReportRequestDto
{
    public int ReportId { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? ExportFormat { get; set; } // null for data, PDF, Excel, CSV
}

/// <summary>
/// Execute report response
/// </summary>
public class ExecuteReportResponseDto
{
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public ReportSummaryDto? Summary { get; set; }
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Report summary with aggregates
/// </summary>
public class ReportSummaryDto
{
    public Dictionary<string, object> Aggregates { get; set; } = new();
    public List<ReportGroupSummaryDto>? GroupSummaries { get; set; }
}

/// <summary>
/// Group summary
/// </summary>
public class ReportGroupSummaryDto
{
    public string GroupField { get; set; } = string.Empty;
    public object? GroupValue { get; set; }
    public int RecordCount { get; set; }
    public Dictionary<string, object> Aggregates { get; set; } = new();
}

/// <summary>
/// Report export result
/// </summary>
public class ReportExportResultDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

#endregion

#region Data Source DTOs

/// <summary>
/// Available data source
/// </summary>
public class DataSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<DataSourceFieldDto> Fields { get; set; } = new();
    public List<DataSourceRelationDto>? Relations { get; set; }
}

/// <summary>
/// Data source field
/// </summary>
public class DataSourceFieldDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string? RelatedTable { get; set; }
    public List<string>? AllowedValues { get; set; } // For enum/lookup fields
}

/// <summary>
/// Data source relation
/// </summary>
public class DataSourceRelationDto
{
    public string RelatedDataSource { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty; // OneToOne, OneToMany, ManyToMany
    public string LocalField { get; set; } = string.Empty;
    public string ForeignField { get; set; } = string.Empty;
}

#endregion

#region Report Schedule DTOs

/// <summary>
/// Report schedule DTO
/// </summary>
public class ReportScheduleDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string ScheduleName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Yearly
    public string? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string? TimeOfDay { get; set; }
    public string? Timezone { get; set; }
    public string OutputFormat { get; set; } = string.Empty; // PDF, Excel, CSV
    public List<ReportRecipientDto> Recipients { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
    public bool IsActive { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
}

/// <summary>
/// Create/update report schedule request
/// </summary>
public class CreateReportScheduleDto
{
    public int ReportId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string? TimeOfDay { get; set; }
    public string? Timezone { get; set; }
    public string OutputFormat { get; set; } = string.Empty;
    public List<ReportRecipientDto> Recipients { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Report recipient
/// </summary>
public class ReportRecipientDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string DeliveryMethod { get; set; } = "Email"; // Email, Portal, Both
}

#endregion

#region Report Category/Template DTOs

/// <summary>
/// Report category
/// </summary>
public class ReportCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int ReportCount { get; set; }
}

/// <summary>
/// Report template summary
/// </summary>
public class ReportTemplateSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UsageCount { get; set; }
}

#endregion

#region Report History DTOs

/// <summary>
/// Report execution history
/// </summary>
public class ReportExecutionHistoryDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public int ExecutedByUserId { get; set; }
    public string ExecutedByUserName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
    public string? ExportFormat { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failed
    public string? Error { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Report history filter
/// </summary>
public class ReportHistoryFilterDto
{
    public int? ReportId { get; set; }
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region Saved Report DTOs

/// <summary>
/// Saved report instance
/// </summary>
public class SavedReportDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string SavedName { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime SavedAt { get; set; }
    public int SavedByUserId { get; set; }
    public string SavedByUserName { get; set; } = string.Empty;
}

/// <summary>
/// Save report request
/// </summary>
public class SaveReportRequestDto
{
    public int ReportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

#endregion

#region Dashboard Widget DTOs

/// <summary>
/// Report dashboard widget
/// </summary>
public class ReportWidgetDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string WidgetName { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty; // Chart, Table, KPI, Summary
    public int Width { get; set; } = 4; // Grid columns (1-12)
    public int Height { get; set; } = 2; // Grid rows
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public ReportVisualizationDto? Visualization { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

/// <summary>
/// Create report widget request
/// </summary>
public class CreateReportWidgetDto
{
    public int ReportId { get; set; }
    public string WidgetName { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public ReportVisualizationDto? Visualization { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 300;
}

#endregion

#region Report Permission DTOs

/// <summary>
/// Report permission
/// </summary>
public class ReportPermissionDto
{
    public int ReportId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanExport { get; set; }
    public bool CanSchedule { get; set; }
    public bool CanShare { get; set; }
}

/// <summary>
/// Update report permissions request
/// </summary>
public class UpdateReportPermissionsDto
{
    public int ReportId { get; set; }
    public List<ReportPermissionDto> Permissions { get; set; } = new();
}

#endregion
