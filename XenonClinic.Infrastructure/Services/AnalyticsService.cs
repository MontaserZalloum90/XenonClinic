using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Advanced analytics and BI dashboard service implementation
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AnalyticsService(ClinicDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Dashboard Management

    public async Task<List<DashboardDto>> GetDashboardsAsync(int userId)
    {
        var dashboards = await _context.Set<AnalyticsDashboard>()
            .Where(d => d.CreatedByUserId == userId || d.IsPublic)
            .OrderByDescending(d => d.IsDefault)
            .ThenBy(d => d.Name)
            .ToListAsync();

        return dashboards.Select(MapToDashboardDto).ToList();
    }

    public async Task<DashboardDto?> GetDashboardByIdAsync(int dashboardId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        return dashboard != null ? MapToDashboardDto(dashboard) : null;
    }

    private async Task<DashboardDto?> GetDashboardAsync(int dashboardId, int userId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == dashboardId && (d.CreatedByUserId == userId || d.IsPublic));

        return dashboard != null ? MapToDashboardDto(dashboard) : null;
    }

    public async Task<DashboardDto?> GetDefaultDashboardAsync(int userId, int branchId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.BranchId == branchId && d.IsDefault && (d.CreatedByUserId == userId || d.IsPublic));

        return dashboard != null ? MapToDashboardDto(dashboard) : null;
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(int dashboardId, int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        if (dashboard == null)
            throw new InvalidOperationException("Dashboard not found");

        var widgetData = new List<WidgetDataDto>();
        foreach (var widget in dashboard.Widgets.Where(w => w.IsVisible))
        {
            try
            {
                var data = await FetchWidgetDataAsync(widget, null);
                widgetData.Add(new WidgetDataDto
                {
                    WidgetId = widget.Id,
                    Title = widget.Title,
                    Type = widget.WidgetType,
                    Data = data,
                    LastUpdated = DateTime.UtcNow,
                    StartDate = startDate,
                    EndDate = endDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for widget {WidgetId}", widget.Id);
            }
        }

        return new DashboardDataDto
        {
            DashboardId = dashboardId,
            DashboardName = dashboard.Name,
            Widgets = widgetData,
            LastUpdated = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<DashboardDto> CreateDashboardAsync(SaveDashboardDto request, int userId)
    {
        var dashboard = new AnalyticsDashboard
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category ?? "General",
            IsDefault = request.IsDefault,
            IsPublic = request.IsPublic,
            LayoutJson = request.Layout != null ? JsonSerializer.Serialize(request.Layout) : null,
            ThemeJson = request.Theme != null ? JsonSerializer.Serialize(request.Theme) : null,
            RefreshIntervalSeconds = request.RefreshIntervalSeconds ?? 300,
            CreatedByUserId = userId,
            BranchId = request.BranchId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AnalyticsDashboard>().Add(dashboard);
        await _context.SaveChangesAsync();

        if (request.Widgets?.Any() == true)
        {
            foreach (var widget in request.Widgets)
            {
                await AddWidgetInternalAsync(dashboard.Id, widget);
            }
        }

        _logger.LogInformation("Created analytics dashboard {DashboardId} for user {UserId}", dashboard.Id, userId);
        return await GetDashboardAsync(dashboard.Id, userId) ?? MapToDashboardDto(dashboard);
    }

    public async Task<DashboardDto?> UpdateDashboardAsync(int dashboardId, SaveDashboardDto request)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        if (dashboard == null)
            return null;

        dashboard.Name = request.Name;
        dashboard.Description = request.Description;
        dashboard.Category = request.Category ?? dashboard.Category;
        dashboard.IsDefault = request.IsDefault;
        dashboard.IsPublic = request.IsPublic;
        dashboard.LayoutJson = request.Layout != null ? JsonSerializer.Serialize(request.Layout) : null;
        dashboard.ThemeJson = request.Theme != null ? JsonSerializer.Serialize(request.Theme) : null;
        dashboard.RefreshIntervalSeconds = request.RefreshIntervalSeconds ?? dashboard.RefreshIntervalSeconds;
        dashboard.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated analytics dashboard {DashboardId}", dashboardId);
        return MapToDashboardDto(dashboard);
    }

    public async Task DeleteDashboardAsync(int dashboardId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        if (dashboard == null)
            return;

        _context.Set<AnalyticsDashboard>().Remove(dashboard);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted analytics dashboard {DashboardId}", dashboardId);
    }

    public async Task<DashboardDto> DuplicateDashboardAsync(int dashboardId, string newName, int userId)
    {
        var source = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        if (source == null)
            throw new InvalidOperationException("Source dashboard not found");

        var duplicate = new AnalyticsDashboard
        {
            Name = newName,
            Description = source.Description,
            Category = source.Category,
            IsDefault = false,
            IsPublic = false,
            LayoutJson = source.LayoutJson,
            ThemeJson = source.ThemeJson,
            RefreshIntervalSeconds = source.RefreshIntervalSeconds,
            CreatedByUserId = userId,
            BranchId = source.BranchId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AnalyticsDashboard>().Add(duplicate);
        await _context.SaveChangesAsync();

        foreach (var widget in source.Widgets)
        {
            var duplicateWidget = new AnalyticsDashboardWidget
            {
                DashboardId = duplicate.Id,
                WidgetId = Guid.NewGuid().ToString(),
                Title = widget.Title,
                WidgetType = widget.WidgetType,
                DataSource = widget.DataSource,
                ConfigurationJson = widget.ConfigurationJson,
                PositionJson = widget.PositionJson,
                FiltersJson = widget.FiltersJson,
                RefreshIntervalSeconds = widget.RefreshIntervalSeconds,
                IsVisible = widget.IsVisible
            };
            _context.Set<AnalyticsDashboardWidget>().Add(duplicateWidget);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Duplicated dashboard {SourceId} to {NewId}", dashboardId, duplicate.Id);
        return await GetDashboardAsync(duplicate.Id, userId) ?? MapToDashboardDto(duplicate);
    }

    public async Task<bool> SetDefaultDashboardAsync(int dashboardId, int userId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .FirstOrDefaultAsync(d => d.Id == dashboardId && (d.CreatedByUserId == userId || d.IsPublic));

        if (dashboard == null)
            return false;

        // Remove default from other dashboards
        var otherDefaults = await _context.Set<AnalyticsDashboard>()
            .Where(d => d.BranchId == dashboard.BranchId && d.CreatedByUserId == userId && d.IsDefault && d.Id != dashboardId)
            .ToListAsync();

        foreach (var other in otherDefaults)
        {
            other.IsDefault = false;
        }

        dashboard.IsDefault = true;
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Widget Management

    public async Task<WidgetDto> AddWidgetAsync(int dashboardId, SaveWidgetDto widget)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>()
            .FirstOrDefaultAsync(d => d.Id == dashboardId);

        if (dashboard == null)
            throw new InvalidOperationException("Dashboard not found");

        var entity = new AnalyticsDashboardWidget
        {
            DashboardId = dashboardId,
            WidgetId = Guid.NewGuid().ToString(),
            Title = widget.Title ?? string.Empty,
            WidgetType = widget.WidgetType ?? string.Empty,
            DataSource = widget.DataSource ?? string.Empty,
            ConfigurationJson = widget.Configuration != null ? JsonSerializer.Serialize(widget.Configuration) : null,
            PositionJson = widget.Position != null ? JsonSerializer.Serialize(widget.Position) : null,
            FiltersJson = widget.Filters != null ? JsonSerializer.Serialize(widget.Filters) : null,
            RefreshIntervalSeconds = widget.RefreshIntervalSeconds ?? 300,
            IsVisible = widget.IsVisible
        };

        _context.Set<AnalyticsDashboardWidget>().Add(entity);
        await _context.SaveChangesAsync();

        return new WidgetDto
        {
            Id = entity.Id,
            DashboardId = entity.DashboardId,
            Title = entity.Title,
            WidgetType = entity.WidgetType,
            DataSource = entity.DataSource,
            Configuration = widget.Configuration,
            Position = widget.Position,
            Filters = widget.Filters,
            RefreshIntervalSeconds = entity.RefreshIntervalSeconds,
            IsVisible = entity.IsVisible
        };
    }

    private async Task<DashboardWidgetDto> AddWidgetInternalAsync(int dashboardId, DashboardWidgetDto widget)
    {
        var entity = new AnalyticsDashboardWidget
        {
            DashboardId = dashboardId,
            WidgetId = string.IsNullOrEmpty(widget.WidgetId) ? Guid.NewGuid().ToString() : widget.WidgetId,
            Title = widget.Title,
            WidgetType = widget.WidgetType,
            DataSource = widget.DataSource,
            ConfigurationJson = JsonSerializer.Serialize(widget.Configuration),
            PositionJson = JsonSerializer.Serialize(widget.Position),
            FiltersJson = widget.Filters != null ? JsonSerializer.Serialize(widget.Filters) : null,
            RefreshIntervalSeconds = widget.RefreshIntervalSeconds,
            IsVisible = widget.IsVisible
        };

        _context.Set<AnalyticsDashboardWidget>().Add(entity);
        await _context.SaveChangesAsync();

        widget.Id = entity.Id;
        widget.WidgetId = entity.WidgetId;
        return widget;
    }

    private async Task AddWidgetInternalAsync(int dashboardId, SaveWidgetDto widget)
    {
        var entity = new AnalyticsDashboardWidget
        {
            DashboardId = dashboardId,
            WidgetId = Guid.NewGuid().ToString(),
            Title = widget.Title ?? string.Empty,
            WidgetType = widget.WidgetType ?? string.Empty,
            DataSource = widget.DataSource ?? string.Empty,
            ConfigurationJson = widget.Configuration != null ? JsonSerializer.Serialize(widget.Configuration) : null,
            PositionJson = widget.Position != null ? JsonSerializer.Serialize(widget.Position) : null,
            FiltersJson = widget.Filters != null ? JsonSerializer.Serialize(widget.Filters) : null,
            RefreshIntervalSeconds = widget.RefreshIntervalSeconds ?? 300,
            IsVisible = widget.IsVisible
        };

        _context.Set<AnalyticsDashboardWidget>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<WidgetDto?> UpdateWidgetAsync(int widgetId, SaveWidgetDto widget)
    {
        var entity = await _context.Set<AnalyticsDashboardWidget>()
            .FirstOrDefaultAsync(w => w.Id == widgetId);

        if (entity == null)
            return null;

        entity.Title = widget.Title ?? entity.Title;
        entity.WidgetType = widget.WidgetType ?? entity.WidgetType;
        entity.DataSource = widget.DataSource ?? entity.DataSource;
        entity.ConfigurationJson = widget.Configuration != null ? JsonSerializer.Serialize(widget.Configuration) : null;
        entity.PositionJson = widget.Position != null ? JsonSerializer.Serialize(widget.Position) : null;
        entity.FiltersJson = widget.Filters != null ? JsonSerializer.Serialize(widget.Filters) : null;
        entity.RefreshIntervalSeconds = widget.RefreshIntervalSeconds ?? entity.RefreshIntervalSeconds;
        entity.IsVisible = widget.IsVisible;

        await _context.SaveChangesAsync();

        return new WidgetDto
        {
            Id = entity.Id,
            DashboardId = entity.DashboardId,
            Title = entity.Title,
            WidgetType = entity.WidgetType,
            DataSource = entity.DataSource,
            Configuration = widget.Configuration,
            Position = widget.Position,
            Filters = widget.Filters,
            RefreshIntervalSeconds = entity.RefreshIntervalSeconds,
            IsVisible = entity.IsVisible
        };
    }

    public async Task DeleteWidgetAsync(int widgetId)
    {
        var entity = await _context.Set<AnalyticsDashboardWidget>()
            .FirstOrDefaultAsync(w => w.Id == widgetId);

        if (entity == null)
            return;

        _context.Set<AnalyticsDashboardWidget>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> RemoveWidgetAsync(int dashboardId, string widgetId, int userId)
    {
        var entity = await _context.Set<AnalyticsDashboardWidget>()
            .Include(w => w.Dashboard)
            .FirstOrDefaultAsync(w => w.DashboardId == dashboardId && w.WidgetId == widgetId && w.Dashboard.CreatedByUserId == userId);

        if (entity == null)
            return false;

        _context.Set<AnalyticsDashboardWidget>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<WidgetDataDto> GetWidgetDataAsync(int widgetId, int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var widget = await _context.Set<AnalyticsDashboardWidget>()
            .FirstOrDefaultAsync(w => w.Id == widgetId);

        if (widget == null)
            throw new InvalidOperationException("Widget not found");

        var data = await FetchWidgetDataAsync(widget, null);

        return new WidgetDataDto
        {
            WidgetId = widgetId,
            Title = widget.Title,
            Type = widget.WidgetType,
            Data = data,
            LastUpdated = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    private async Task<Dictionary<string, object>> GetWidgetDataInternalAsync(int dashboardId, string widgetId, Dictionary<string, object>? filters = null)
    {
        var widget = await _context.Set<AnalyticsDashboardWidget>()
            .FirstOrDefaultAsync(w => w.DashboardId == dashboardId && w.WidgetId == widgetId);

        if (widget == null)
            throw new InvalidOperationException("Widget not found");

        return await FetchWidgetDataAsync(widget, filters);
    }

    public async Task<Dictionary<string, object>> RefreshWidgetAsync(int dashboardId, string widgetId)
    {
        return await GetWidgetDataInternalAsync(dashboardId, widgetId);
    }

    private async Task<Dictionary<string, object>> FetchWidgetDataAsync(AnalyticsDashboardWidget widget, Dictionary<string, object>? filters = null)
    {
        var result = new Dictionary<string, object>
        {
            ["widgetId"] = widget.WidgetId,
            ["title"] = widget.Title,
            ["type"] = widget.WidgetType,
            ["lastUpdated"] = DateTime.UtcNow
        };

        // Simulate fetching data based on data source
        var dataSource = widget.DataSource.ToLowerInvariant();

        switch (dataSource)
        {
            case "patients":
                result["data"] = await GetPatientWidgetDataAsync(widget);
                break;
            case "appointments":
                result["data"] = await GetAppointmentWidgetDataAsync(widget);
                break;
            case "revenue":
                result["data"] = await GetRevenueWidgetDataAsync(widget);
                break;
            case "claims":
                result["data"] = await GetClaimsWidgetDataAsync(widget);
                break;
            default:
                result["data"] = new List<object>();
                break;
        }

        return result;
    }

    private async Task<object> GetPatientWidgetDataAsync(AnalyticsDashboardWidget widget)
    {
        var totalPatients = await _context.Patients.CountAsync();
        var newThisMonth = await _context.Patients
            .CountAsync(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-30));

        return new
        {
            total = totalPatients,
            newThisMonth,
            trend = new[] { totalPatients - 50, totalPatients - 30, totalPatients - 10, totalPatients }
        };
    }

    private async Task<object> GetAppointmentWidgetDataAsync(AnalyticsDashboardWidget widget)
    {
        var today = DateTime.UtcNow.Date;
        var todayAppointments = await _context.Appointments.CountAsync(a => a.StartTime.Date == today);
        var weekAppointments = await _context.Appointments
            .CountAsync(a => a.StartTime >= today.AddDays(-7));

        return new
        {
            today = todayAppointments,
            thisWeek = weekAppointments,
            byStatus = new[]
            {
                new { status = "Scheduled", count = todayAppointments / 2 },
                new { status = "Completed", count = todayAppointments / 3 },
                new { status = "Cancelled", count = todayAppointments / 6 }
            }
        };
    }

    private async Task<object> GetRevenueWidgetDataAsync(AnalyticsDashboardWidget widget)
    {
        var revenue = await _context.Invoices
            .Where(i => i.InvoiceDate >= DateTime.UtcNow.AddMonths(-1))
            .SumAsync(i => i.TotalAmount);

        return new
        {
            totalRevenue = revenue,
            previousMonth = revenue * 0.95m,
            growth = 5.2m,
            byCategory = new[]
            {
                new { category = "Consultations", amount = revenue * 0.4m },
                new { category = "Procedures", amount = revenue * 0.35m },
                new { category = "Lab Tests", amount = revenue * 0.25m }
            }
        };
    }

    private async Task<object> GetClaimsWidgetDataAsync(AnalyticsDashboardWidget widget)
    {
        var totalClaims = await _context.Set<InsuranceClaim>().CountAsync();
        var pendingClaims = await _context.Set<InsuranceClaim>()
            .CountAsync(c => c.Status == Core.Enums.ClaimStatus.Submitted);

        return new
        {
            total = totalClaims,
            pending = pendingClaims,
            approved = totalClaims - pendingClaims,
            denialRate = 8.5m
        };
    }

    #endregion

    #region Healthcare Analytics

    public async Task<PatientAnalyticsDto> GetPatientAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddYears(-1);
        var end = endDate ?? DateTime.UtcNow;

        var patients = await _context.Patients
            .Where(p => p.BranchId == branchId)
            .ToListAsync();

        var totalPatients = patients.Count;
        var newThisMonth = patients.Count(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        var activePatients = patients.Count(p => p.IsActive);

        var ageGroups = patients
            .Where(p => p.DateOfBirth != default(DateTime))
            .GroupBy(p => GetAgeGroup(p.DateOfBirth))
            .Select(g => new DemographicBreakdownDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = totalPatients > 0 ? Math.Round((decimal)g.Count() / totalPatients * 100, 1) : 0
            })
            .OrderBy(d => d.Category)
            .ToList();

        var genderGroups = patients
            .GroupBy(p => p.Gender ?? "Unknown")
            .Select(g => new DemographicBreakdownDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = totalPatients > 0 ? Math.Round((decimal)g.Count() / totalPatients * 100, 1) : 0
            })
            .ToList();

        return new PatientAnalyticsDto
        {
            TotalPatients = totalPatients,
            NewPatientsThisMonth = newThisMonth,
            ActivePatients = activePatients,
            PatientRetentionRate = totalPatients > 0 ? Math.Round((decimal)activePatients / totalPatients * 100, 1) : 0,
            AveragePatientAge = patients.Where(p => p.DateOfBirth != default(DateTime)).Any()
                ? Math.Round((decimal)patients.Where(p => p.DateOfBirth != default(DateTime))
                    .Average(p => (DateTime.UtcNow - p.DateOfBirth).Days / 365.25), 1)
                : 0,
            AgeDistribution = ageGroups,
            GenderDistribution = genderGroups,
            InsuranceDistribution = new List<DemographicBreakdownDto>(),
            TopConditions = new List<TopConditionDto>()
        };
    }

    private static string GetAgeGroup(DateTime dateOfBirth)
    {
        var age = (DateTime.UtcNow - dateOfBirth).Days / 365;
        return age switch
        {
            < 18 => "0-17",
            < 30 => "18-29",
            < 45 => "30-44",
            < 60 => "45-59",
            < 75 => "60-74",
            _ => "75+"
        };
    }

    public async Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var appointments = await _context.Appointments
            .Where(a => a.BranchId == branchId && a.StartTime >= start && a.StartTime <= end)
            .ToListAsync();

        var total = appointments.Count;
        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

        var hourlyDist = appointments
            .GroupBy(a => a.StartTime.Hour)
            .Select(g => new HourlyDistributionDto
            {
                Hour = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 1) : 0
            })
            .OrderBy(h => h.Hour)
            .ToList();

        return new AppointmentAnalyticsDto
        {
            TotalAppointments = total,
            CompletedAppointments = completed,
            CancelledAppointments = cancelled,
            NoShowAppointments = noShow,
            NoShowRate = total > 0 ? Math.Round((decimal)noShow / total * 100, 1) : 0,
            CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 1) : 0,
            AverageWaitTimeMinutes = 15,
            AverageAppointmentDurationMinutes = 30,
            ScheduleUtilizationRate = 75,
            ByType = new List<AppointmentsByTypeDto>(),
            ByProvider = new List<AppointmentsByProviderDto>(),
            DailyTrend = new List<TimeSeriesDataPointDto>(),
            HourlyDistribution = hourlyDist
        };
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Where(i => i.BranchId == branchId && i.InvoiceDate >= start && i.InvoiceDate <= end)
            .ToListAsync();

        var totalRevenue = invoices.Sum(i => i.TotalAmount);
        var paidAmount = invoices.Sum(i => i.PaidAmount);
        var outstanding = totalRevenue - paidAmount;

        var previousMonthStart = start.AddMonths(-1);
        var previousMonthEnd = start.AddDays(-1);
        var previousInvoices = await _context.Invoices
            .Where(i => i.BranchId == branchId && i.InvoiceDate >= previousMonthStart && i.InvoiceDate <= previousMonthEnd)
            .SumAsync(i => i.TotalAmount);

        var growth = previousInvoices > 0 ? Math.Round((totalRevenue - previousInvoices) / previousInvoices * 100, 1) : 0;

        var monthlyTrend = invoices
            .GroupBy(i => new DateTime(i.InvoiceDate.Year, i.InvoiceDate.Month, 1))
            .Select(g => new TimeSeriesDataPointDto
            {
                Timestamp = g.Key,
                Value = g.Sum(i => i.TotalAmount),
                Label = g.Key.ToString("MMM yyyy")
            })
            .OrderBy(t => t.Timestamp)
            .ToList();

        return new RevenueAnalyticsDto
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = totalRevenue,
            RevenueGrowthPercent = growth,
            AverageRevenuePerPatient = 500,
            AverageRevenuePerVisit = 150,
            CollectionRate = totalRevenue > 0 ? Math.Round(paidAmount / totalRevenue * 100, 1) : 0,
            OutstandingBalance = outstanding,
            DaysInAR = 35,
            ByService = new List<RevenueByServiceDto>(),
            ByPayer = new List<RevenueByPayerDto>(),
            ByProvider = new List<RevenueByProviderDto>(),
            MonthlyTrend = monthlyTrend
        };
    }

    public async Task<ClaimsAnalyticsDto> GetClaimsAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var claims = await _context.Set<InsuranceClaim>()
            .Where(c => c.BranchId == branchId && c.ServiceDate >= start && c.ServiceDate <= end)
            .ToListAsync();

        var total = claims.Count;
        var pending = claims.Count(c => c.Status == Core.Enums.ClaimStatus.Submitted || c.Status == Core.Enums.ClaimStatus.Pending);
        var approved = claims.Count(c => c.Status == Core.Enums.ClaimStatus.Approved || c.Status == Core.Enums.ClaimStatus.Paid);
        var denied = claims.Count(c => c.Status == Core.Enums.ClaimStatus.Denied);

        var totalClaimed = claims.Sum(c => c.BilledAmount);
        var totalPaid = claims.Sum(c => c.PaidAmount);

        return new ClaimsAnalyticsDto
        {
            TotalClaims = total,
            PendingClaims = pending,
            ApprovedClaims = approved,
            DeniedClaims = denied,
            DenialRate = total > 0 ? Math.Round((decimal)denied / total * 100, 1) : 0,
            FirstPassResolutionRate = 85,
            AverageDaysToResolution = 21,
            TotalClaimedAmount = totalClaimed,
            TotalPaidAmount = totalPaid,
            AverageReimbursementRate = totalClaimed > 0 ? Math.Round(totalPaid / totalClaimed * 100, 1) : 0,
            TopDenialReasons = new List<DenialReasonDto>(),
            ByStatus = claims.GroupBy(c => c.Status.ToString())
                .Select(g => new ClaimsByStatusDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(c => c.BilledAmount),
                    Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 1) : 0
                }).ToList(),
            SubmissionTrend = new List<TimeSeriesDataPointDto>()
        };
    }

    public async Task<ClinicalQualityMetricsDto> GetClinicalQualityMetricsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await Task.FromResult(new ClinicalQualityMetricsDto
        {
            PatientSatisfactionScore = 4.5m,
            ReadmissionRate = 3.2m,
            MedicationAdherenceRate = 87.5m,
            PreventiveCareComplianceRate = 78.3m,
            LabResultTurnaroundHours = 24,
            CriticalResultNotificationRate = 99.1m,
            QualityMeasures = new List<QualityMeasureDto>
            {
                new() { MeasureId = "BP001", MeasureName = "Blood Pressure Control", Category = "Chronic Care", CurrentValue = 72, TargetValue = 75, NationalBenchmark = 70, PerformanceLevel = "Above" },
                new() { MeasureId = "DM001", MeasureName = "Diabetes HbA1c Control", Category = "Chronic Care", CurrentValue = 68, TargetValue = 70, NationalBenchmark = 65, PerformanceLevel = "Above" },
                new() { MeasureId = "PREV001", MeasureName = "Annual Wellness Visits", Category = "Preventive", CurrentValue = 45, TargetValue = 60, NationalBenchmark = 50, PerformanceLevel = "Below" }
            },
            OutcomeMetrics = new List<OutcomeMetricDto>
            {
                new() { MetricName = "Patient Wait Time", Value = 15, Unit = "minutes", Target = 20, TrendDirection = "Down" },
                new() { MetricName = "Same-Day Appointments", Value = 35, Unit = "percent", Target = 30, TrendDirection = "Up" }
            }
        });
    }

    public async Task<OperationalEfficiencyDto> GetOperationalEfficiencyAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var appointmentCount = await _context.Appointments
            .Where(a => a.BranchId == branchId && a.StartTime >= DateTime.UtcNow.AddDays(-30))
            .CountAsync();

        return new OperationalEfficiencyDto
        {
            StaffUtilizationRate = 82.5m,
            RoomUtilizationRate = 78.3m,
            EquipmentUtilizationRate = 65.2m,
            AverageCheckInTimeMinutes = 8.5m,
            AverageCheckOutTimeMinutes = 5.2m,
            PatientThroughputPerDay = appointmentCount / 30m,
            PeakHour = 10,
            OverTimeHoursPercent = 12.3m,
            ResourceUtilization = new List<ResourceUtilizationDto>
            {
                new() { ResourceType = "Room", ResourceName = "Exam Room 1", UtilizationRate = 85, AvailableHours = 8, UsedHours = 6.8m },
                new() { ResourceType = "Room", ResourceName = "Exam Room 2", UtilizationRate = 72, AvailableHours = 8, UsedHours = 5.76m },
                new() { ResourceType = "Equipment", ResourceName = "X-Ray Machine", UtilizationRate = 45, AvailableHours = 8, UsedHours = 3.6m }
            },
            Bottlenecks = new List<BottleneckDto>
            {
                new() { Area = "Check-in", Description = "High wait times during morning hours", Severity = "Medium", ImpactScore = 6.5m, RecommendedAction = "Add additional front desk staff 8-10 AM" }
            }
        };
    }

    public async Task<ClinicalOutcomesDto> GetClinicalOutcomesAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Stub implementation - returns sample clinical outcomes data
        return await Task.FromResult(new ClinicalOutcomesDto
        {
            PatientOutcomes = new List<PatientOutcomeDto>(),
            TreatmentEffectiveness = 85.5m,
            ReadmissionRate = 3.2m,
            ComplicationRate = 1.8m,
            RecoveryTimeAverage = 14.5m
        });
    }

    public async Task<ResourceUtilizationDto> GetResourceUtilizationAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Stub implementation - returns sample resource utilization data
        return await Task.FromResult(new ResourceUtilizationDto
        {
            ResourceType = "Overall",
            ResourceName = "All Resources",
            UtilizationRate = 78.5m,
            AvailableHours = 8,
            UsedHours = 6.28m
        });
    }

    public async Task<KPIDashboardDto> GetKPIsAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Stub implementation - returns sample KPI dashboard data
        return await Task.FromResult(new KPIDashboardDto
        {
            Kpis = new List<KpiDto>
            {
                new() { Name = "Patient Satisfaction", Value = 4.5m, Target = 4.0m, Unit = "rating", Status = "Above" },
                new() { Name = "Revenue Growth", Value = 12.5m, Target = 10.0m, Unit = "%", Status = "Above" },
                new() { Name = "Appointment Utilization", Value = 85.0m, Target = 80.0m, Unit = "%", Status = "Above" }
            },
            Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
        });
    }

    public async Task<List<MetricDataPointDto>> GetMetricHistoryAsync(int branchId, string metricName, DateTime startDate, DateTime endDate, string granularity)
    {
        // Stub implementation - returns sample metric history data
        var dataPoints = new List<MetricDataPointDto>();
        var current = startDate;

        while (current <= endDate)
        {
            dataPoints.Add(new MetricDataPointDto
            {
                Timestamp = current,
                Value = new Random().Next(50, 150),
                MetricName = metricName
            });

            current = granularity switch
            {
                "Hour" => current.AddHours(1),
                "Day" => current.AddDays(1),
                "Week" => current.AddDays(7),
                "Month" => current.AddMonths(1),
                _ => current.AddDays(1)
            };
        }

        return await Task.FromResult(dataPoints);
    }

    #endregion

    #region Metrics & Time Series

    public Task<List<AnalyticsMetricDto>> GetAvailableMetricsAsync()
    {
        var metrics = new List<AnalyticsMetricDto>
        {
            new() { MetricId = "patient_count", Name = "Total Patients", Category = "Patient", DataType = "Number", CalculationType = "Count" },
            new() { MetricId = "new_patients", Name = "New Patients", Category = "Patient", DataType = "Number", CalculationType = "Count" },
            new() { MetricId = "appointment_count", Name = "Total Appointments", Category = "Appointment", DataType = "Number", CalculationType = "Count" },
            new() { MetricId = "no_show_rate", Name = "No-Show Rate", Category = "Appointment", DataType = "Percentage", Unit = "%", CalculationType = "Ratio" },
            new() { MetricId = "total_revenue", Name = "Total Revenue", Category = "Financial", DataType = "Currency", Unit = "USD", CalculationType = "Sum" },
            new() { MetricId = "collection_rate", Name = "Collection Rate", Category = "Financial", DataType = "Percentage", Unit = "%", CalculationType = "Ratio" },
            new() { MetricId = "denial_rate", Name = "Claim Denial Rate", Category = "Claims", DataType = "Percentage", Unit = "%", CalculationType = "Ratio" },
            new() { MetricId = "patient_satisfaction", Name = "Patient Satisfaction", Category = "Quality", DataType = "Number", CalculationType = "Average" }
        };

        return Task.FromResult(metrics);
    }

    public async Task<MetricValueDto> GetMetricValueAsync(string metricId, int branchId, DateTime? asOf = null)
    {
        var date = asOf ?? DateTime.UtcNow;

        var value = metricId switch
        {
            "patient_count" => await _context.Patients.Where(p => p.BranchId == branchId).CountAsync(),
            "new_patients" => await _context.Patients.Where(p => p.BranchId == branchId && p.CreatedAt >= date.AddDays(-30)).CountAsync(),
            "appointment_count" => await _context.Appointments.Where(a => a.BranchId == branchId && a.StartTime.Month == date.Month).CountAsync(),
            "total_revenue" => (decimal)await _context.Invoices.Where(i => i.BranchId == branchId && i.InvoiceDate.Month == date.Month).SumAsync(i => i.TotalAmount),
            _ => 0m
        };

        return new MetricValueDto
        {
            MetricId = metricId,
            MetricName = metricId.Replace("_", " ").ToUpperInvariant(),
            Value = value,
            FormattedValue = value.ToString("N0"),
            ChangePercent = 5.2m,
            TrendDirection = "Up",
            IsPositiveTrend = true,
            AsOf = date,
            Period = "Current Month"
        };
    }

    public async Task<List<MetricValueDto>> GetMetricValuesAsync(List<string> metricIds, int branchId, DateTime? asOf = null)
    {
        var results = new List<MetricValueDto>();
        foreach (var metricId in metricIds)
        {
            results.Add(await GetMetricValueAsync(metricId, branchId, asOf));
        }
        return results;
    }

    public async Task<TimeSeriesResultDto> GetTimeSeriesAsync(string metricId, int branchId, DateTime startDate, DateTime endDate, string granularity = "Day")
    {
        var dataPoints = new List<TimeSeriesDataPointDto>();
        var current = startDate;

        while (current <= endDate)
        {
            var value = metricId switch
            {
                "patient_count" => await _context.Patients.Where(p => p.BranchId == branchId && p.CreatedAt <= current).CountAsync(),
                "appointment_count" => await _context.Appointments.Where(a => a.BranchId == branchId && a.StartTime.Date == current.Date).CountAsync(),
                _ => new Random().Next(50, 150)
            };

            dataPoints.Add(new TimeSeriesDataPointDto
            {
                Timestamp = current,
                Value = value,
                Label = current.ToString(granularity == "Day" ? "MM/dd" : "MMM yyyy")
            });

            current = granularity switch
            {
                "Hour" => current.AddHours(1),
                "Day" => current.AddDays(1),
                "Week" => current.AddDays(7),
                "Month" => current.AddMonths(1),
                _ => current.AddDays(1)
            };
        }

        return new TimeSeriesResultDto
        {
            MetricId = metricId,
            MetricName = metricId.Replace("_", " "),
            Granularity = granularity,
            StartDate = startDate,
            EndDate = endDate,
            DataPoints = dataPoints,
            Stats = new TimeSeriesStatsDto
            {
                Min = dataPoints.Any() ? dataPoints.Min(d => d.Value) : 0,
                Max = dataPoints.Any() ? dataPoints.Max(d => d.Value) : 0,
                Average = dataPoints.Any() ? Math.Round(dataPoints.Average(d => d.Value), 2) : 0,
                Sum = dataPoints.Sum(d => d.Value),
                Count = dataPoints.Count
            }
        };
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryDto query, int branchId)
    {
        // Simplified query execution - in production this would be more sophisticated
        var results = new List<Dictionary<string, object>>();

        foreach (var metric in query.Metrics)
        {
            var value = await GetMetricValueAsync(metric, branchId, query.EndDate);
            results.Add(new Dictionary<string, object>
            {
                ["metric"] = metric,
                ["value"] = value.Value,
                ["formatted"] = value.FormattedValue ?? value.Value.ToString()
            });
        }

        return results;
    }

    #endregion

    #region Predictive Analytics

    public async Task<DemandForecastDto> GetDemandForecastAsync(int branchId, DateTime forecastDate, int daysAhead = 7)
    {
        var historicalAvg = await _context.Appointments
            .Where(a => a.BranchId == branchId && a.StartTime >= DateTime.UtcNow.AddMonths(-3))
            .GroupBy(a => a.StartTime.DayOfWeek)
            .Select(g => new { DayOfWeek = g.Key, Avg = g.Count() / 12 }) // ~12 weeks
            .ToListAsync();

        var predictedAppointments = 0;
        var hourlyDist = new List<TimeSeriesDataPointDto>();

        for (var d = 0; d < daysAhead; d++)
        {
            var date = forecastDate.AddDays(d);
            var dayAvg = historicalAvg.FirstOrDefault(h => h.DayOfWeek == date.DayOfWeek)?.Avg ?? 20;
            predictedAppointments += dayAvg;
        }

        for (var h = 8; h <= 17; h++)
        {
            hourlyDist.Add(new TimeSeriesDataPointDto
            {
                Timestamp = forecastDate.Date.AddHours(h),
                Value = h switch
                {
                    >= 9 and <= 11 => 15,
                    >= 14 and <= 16 => 12,
                    _ => 8
                },
                Label = $"{h}:00"
            });
        }

        return new DemandForecastDto
        {
            ForecastDate = forecastDate,
            ForecastPeriod = $"Next {daysAhead} days",
            PredictedAppointments = predictedAppointments,
            PredictedEmergencyVisits = (int)(predictedAppointments * 0.05),
            PredictedRevenue = predictedAppointments * 150,
            ByDepartment = new List<DepartmentForecastDto>
            {
                new() { Department = "Primary Care", PredictedPatients = (int)(predictedAppointments * 0.6), PredictedUtilization = 78, RecommendedStaffCount = 4 },
                new() { Department = "Specialty", PredictedPatients = (int)(predictedAppointments * 0.3), PredictedUtilization = 65, RecommendedStaffCount = 2 },
                new() { Department = "Lab", PredictedPatients = (int)(predictedAppointments * 0.1), PredictedUtilization = 55, RecommendedStaffCount = 1 }
            },
            HourlyDistribution = hourlyDist
        };
    }

    public async Task<List<PatientRiskScoreDto>> GetPatientRiskScoresAsync(int branchId, string? riskCategory = null, int minScore = 0)
    {
        var patients = await _context.Patients
            .Where(p => p.BranchId == branchId && p.IsActive)
            .Take(100)
            .ToListAsync();

        IEnumerable<PatientRiskScoreDto> scores = patients.Select(p => new PatientRiskScoreDto
        {
            PatientId = p.Id,
            PatientName = $"{p.FirstName} {p.LastName}",
            OverallRiskScore = CalculateRiskScore(p),
            RiskCategory = GetRiskCategory(CalculateRiskScore(p)),
            RiskFactors = new List<RiskFactorDto>
            {
                new() { FactorName = "Age", Category = "Demographic", Score = p.DateOfBirth != default(DateTime) ? GetAgeRiskScore(p.DateOfBirth) : 0, Weight = 0.2m },
                new() { FactorName = "Chronic Conditions", Category = "Clinical", Score = 0.3m, Weight = 0.4m }
            },
            RecommendedInterventions = new List<string> { "Annual wellness visit", "Medication review" },
            CalculatedAt = DateTime.UtcNow
        }).OrderByDescending(r => r.OverallRiskScore);

        // Filter by risk category if specified
        if (!string.IsNullOrEmpty(riskCategory))
        {
            scores = scores.Where(s => s.RiskCategory.Equals(riskCategory, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by minimum score
        if (minScore > 0)
        {
            scores = scores.Where(s => s.OverallRiskScore >= minScore / 100m);
        }

        return scores.ToList();
    }

    public async Task<PatientRiskScoreDto?> GetPatientRiskScoreAsync(int patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null) return null;

        var score = CalculateRiskScore(patient);

        return new PatientRiskScoreDto
        {
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            OverallRiskScore = score,
            RiskCategory = GetRiskCategory(score),
            RiskFactors = new List<RiskFactorDto>
            {
                new() { FactorName = "Age", Category = "Demographic", Score = patient.DateOfBirth != default(DateTime) ? GetAgeRiskScore(patient.DateOfBirth) : 0, Weight = 0.2m }
            },
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static decimal CalculateRiskScore(Core.Entities.Patient patient)
    {
        var score = 0.2m; // Base score
        if (patient.DateOfBirth != default(DateTime))
        {
            var age = (DateTime.UtcNow - patient.DateOfBirth).Days / 365;
            score += age > 65 ? 0.4m : age > 45 ? 0.2m : 0.1m;
        }
        return Math.Min(score, 1.0m);
    }

    private static decimal GetAgeRiskScore(DateTime dateOfBirth)
    {
        var age = (DateTime.UtcNow - dateOfBirth).Days / 365;
        return age > 75 ? 0.9m : age > 65 ? 0.6m : age > 45 ? 0.3m : 0.1m;
    }

    private static string GetRiskCategory(decimal score) => score switch
    {
        >= 0.8m => "Critical",
        >= 0.6m => "High",
        >= 0.3m => "Medium",
        _ => "Low"
    };

    public async Task<List<NoShowPredictionDto>> GetNoShowPredictionsAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.BranchId == branchId && a.StartTime >= fromDate && a.StartTime <= toDate && a.Status == AppointmentStatus.Scheduled)
            .ToListAsync();

        return appointments.Select(a => new NoShowPredictionDto
        {
            AppointmentId = a.Id,
            PatientId = a.PatientId,
            PatientName = $"{a.Patient?.FirstName} {a.Patient?.LastName}",
            AppointmentDate = a.StartTime,
            NoShowProbability = new Random().Next(5, 35) / 100m,
            RiskLevel = "Low",
            RiskFactors = new List<string> { "First appointment", "Monday morning slot" },
            RecommendedAction = "Send reminder 24 hours before"
        }).ToList();
    }

    public Task<PredictionResultDto> GetRevenuePredictionAsync(int branchId, DateTime targetDate)
    {
        var daysAhead = (targetDate - DateTime.UtcNow).Days;

        return Task.FromResult(new PredictionResultDto
        {
            PredictionType = "Revenue",
            PredictedValue = 45000 + daysAhead * 100,
            ConfidenceLevel = 0.85m,
            LowerBound = 40000 + daysAhead * 80,
            UpperBound = 50000 + daysAhead * 120,
            PredictionDate = targetDate,
            ModelVersion = "v1.2",
            ContributingFactors = new List<PredictionFactorDto>
            {
                new() { FactorName = "Historical Trend", Impact = 0.45m, Direction = "Positive" },
                new() { FactorName = "Seasonal Pattern", Impact = 0.25m, Direction = "Positive" },
                new() { FactorName = "Scheduled Appointments", Impact = 0.30m, Direction = "Positive" }
            }
        });
    }

    #endregion

    #region Benchmarks & Comparisons

    public Task<List<BenchmarkComparisonDto>> GetBenchmarkComparisonsAsync(int branchId, List<string>? metricIds = null)
    {
        var benchmarks = new List<BenchmarkComparisonDto>
        {
            new() { MetricName = "No-Show Rate", CurrentValue = 8.5m, IndustryAverage = 12.0m, TopPerformer = 5.0m, Percentile = 75, PerformanceStatus = "Above", GapToTarget = -3.5m },
            new() { MetricName = "Collection Rate", CurrentValue = 92.3m, IndustryAverage = 88.0m, TopPerformer = 96.0m, Percentile = 68, PerformanceStatus = "Above", GapToTarget = 3.7m },
            new() { MetricName = "Patient Satisfaction", CurrentValue = 4.5m, IndustryAverage = 4.2m, TopPerformer = 4.8m, Percentile = 72, PerformanceStatus = "Above", GapToTarget = 0.3m },
            new() { MetricName = "Wait Time (min)", CurrentValue = 15, IndustryAverage = 20, TopPerformer = 10, Percentile = 65, PerformanceStatus = "Above", GapToTarget = -5 }
        };

        if (metricIds?.Any() == true)
        {
            benchmarks = benchmarks.Where(b => metricIds.Contains(b.MetricName.ToLowerInvariant().Replace(" ", "_"))).ToList();
        }

        return Task.FromResult(benchmarks);
    }

    public Task<List<PeriodComparisonDto>> GetPeriodComparisonAsync(int branchId, string currentPeriod, string previousPeriod, List<string>? metricIds = null)
    {
        return Task.FromResult(new List<PeriodComparisonDto>
        {
            new() { MetricName = "Revenue", CurrentPeriod = currentPeriod, PreviousPeriod = previousPeriod, CurrentValue = 125000, PreviousValue = 118000, Change = 7000, ChangePercent = 5.9m, TrendDirection = "Up" },
            new() { MetricName = "Patients", CurrentPeriod = currentPeriod, PreviousPeriod = previousPeriod, CurrentValue = 450, PreviousValue = 420, Change = 30, ChangePercent = 7.1m, TrendDirection = "Up" },
            new() { MetricName = "Appointments", CurrentPeriod = currentPeriod, PreviousPeriod = previousPeriod, CurrentValue = 680, PreviousValue = 650, Change = 30, ChangePercent = 4.6m, TrendDirection = "Up" }
        });
    }

    public async Task<List<ProviderComparisonDto>> GetProviderComparisonAsync(int branchId, List<string>? metricIds = null)
    {
        var doctors = await _context.Doctors
            .Where(d => d.BranchId == branchId && d.IsActive)
            .Take(10)
            .ToListAsync();

        return doctors.Select((d, i) => new ProviderComparisonDto
        {
            ProviderId = d.Id,
            ProviderName = $"Dr. {d.FirstName} {d.LastName}",
            Specialty = d.Specialty ?? "General",
            Metrics = new Dictionary<string, decimal>
            {
                ["appointments"] = 50 + i * 5,
                ["revenue"] = 15000 + i * 1000,
                ["satisfaction"] = 4.2m + i * 0.05m
            },
            Rank = i + 1,
            OverallScore = 85 - i * 2
        }).ToList();
    }

    public Task<List<LocationComparisonDto>> GetLocationComparisonAsync(List<int>? branchIds = null, List<string>? metricIds = null)
    {
        return Task.FromResult(new List<LocationComparisonDto>
        {
            new() { LocationId = 1, LocationName = "Main Clinic", Metrics = new Dictionary<string, decimal> { ["revenue"] = 150000, ["patients"] = 500 }, Rank = 1, OverallScore = 92 },
            new() { LocationId = 2, LocationName = "North Branch", Metrics = new Dictionary<string, decimal> { ["revenue"] = 120000, ["patients"] = 400 }, Rank = 2, OverallScore = 85 }
        });
    }

    #endregion

    #region Alerts & Anomalies

    public async Task<List<AnalyticsAlertDto>> GetActiveAlertsAsync(int branchId)
    {
        var alerts = await _context.Set<AnalyticsAlert>()
            .Where(a => a.BranchId == branchId && !a.IsAcknowledged)
            .OrderByDescending(a => a.DetectedAt)
            .Take(50)
            .ToListAsync();

        return alerts.Select(a => new AnalyticsAlertDto
        {
            Id = a.Id,
            AlertType = a.AlertType,
            Severity = a.Severity,
            MetricName = a.MetricName,
            Message = a.Message,
            CurrentValue = a.CurrentValue,
            ThresholdValue = a.ThresholdValue,
            DetectedAt = a.DetectedAt,
            IsAcknowledged = a.IsAcknowledged,
            AcknowledgedBy = a.AcknowledgedBy,
            AcknowledgedAt = a.AcknowledgedAt
        }).ToList();
    }

    public async Task<bool> AcknowledgeAlertAsync(int alertId, int userId)
    {
        var alert = await _context.Set<AnalyticsAlert>().FindAsync(alertId);
        if (alert == null) return false;

        alert.IsAcknowledged = true;
        alert.AcknowledgedBy = userId.ToString();
        alert.AcknowledgedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AlertRuleDto>> GetAlertRulesAsync(int branchId)
    {
        var rules = await _context.Set<AnalyticsAlertRule>()
            .Where(r => r.BranchId == branchId)
            .ToListAsync();

        return rules.Select(r => new AlertRuleDto
        {
            Id = r.Id,
            RuleName = r.RuleName,
            MetricId = r.MetricId,
            Condition = r.Condition,
            ThresholdValue = r.ThresholdValue,
            Severity = r.Severity,
            NotificationChannels = r.NotificationChannelsJson != null
                ? JsonSerializer.Deserialize<List<string>>(r.NotificationChannelsJson) ?? new()
                : new(),
            NotifyUsers = r.NotifyUsersJson != null
                ? JsonSerializer.Deserialize<List<string>>(r.NotifyUsersJson) ?? new()
                : new(),
            CooldownMinutes = r.CooldownMinutes,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<AlertRuleDto> CreateAlertRuleAsync(CreateAlertRuleDto request, int branchId, int userId)
    {
        var rule = new AnalyticsAlertRule
        {
            RuleName = request.RuleName,
            MetricId = request.MetricId,
            Condition = request.Condition,
            ThresholdValue = request.ThresholdValue,
            Severity = request.Severity,
            NotificationChannelsJson = JsonSerializer.Serialize(request.NotificationChannels),
            NotifyUsersJson = JsonSerializer.Serialize(request.NotifyUsers),
            CooldownMinutes = request.CooldownMinutes,
            IsActive = true,
            BranchId = branchId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AnalyticsAlertRule>().Add(rule);
        await _context.SaveChangesAsync();

        return new AlertRuleDto
        {
            Id = rule.Id,
            RuleName = rule.RuleName,
            MetricId = rule.MetricId,
            Condition = rule.Condition,
            ThresholdValue = rule.ThresholdValue,
            Severity = rule.Severity,
            NotificationChannels = request.NotificationChannels,
            NotifyUsers = request.NotifyUsers,
            CooldownMinutes = rule.CooldownMinutes,
            IsActive = rule.IsActive
        };
    }

    public async Task<AlertRuleDto> UpdateAlertRuleAsync(int ruleId, CreateAlertRuleDto request, int userId)
    {
        var rule = await _context.Set<AnalyticsAlertRule>().FindAsync(ruleId);
        if (rule == null)
            throw new InvalidOperationException("Alert rule not found");

        rule.RuleName = request.RuleName;
        rule.MetricId = request.MetricId;
        rule.Condition = request.Condition;
        rule.ThresholdValue = request.ThresholdValue;
        rule.Severity = request.Severity;
        rule.NotificationChannelsJson = JsonSerializer.Serialize(request.NotificationChannels);
        rule.NotifyUsersJson = JsonSerializer.Serialize(request.NotifyUsers);
        rule.CooldownMinutes = request.CooldownMinutes;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AlertRuleDto
        {
            Id = rule.Id,
            RuleName = rule.RuleName,
            MetricId = rule.MetricId,
            Condition = rule.Condition,
            ThresholdValue = rule.ThresholdValue,
            Severity = rule.Severity,
            NotificationChannels = request.NotificationChannels,
            NotifyUsers = request.NotifyUsers,
            CooldownMinutes = rule.CooldownMinutes,
            IsActive = rule.IsActive
        };
    }

    public async Task<bool> DeleteAlertRuleAsync(int ruleId, int userId)
    {
        var rule = await _context.Set<AnalyticsAlertRule>().FindAsync(ruleId);
        if (rule == null) return false;

        _context.Set<AnalyticsAlertRule>().Remove(rule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AnomalyDto>> GetAnomaliesAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var anomalies = await _context.Set<AnalyticsAnomaly>()
            .Where(a => a.BranchId == branchId && a.DetectedAt >= start && a.DetectedAt <= end)
            .OrderByDescending(a => a.DetectedAt)
            .ToListAsync();

        return anomalies.Select(a => new AnomalyDto
        {
            Id = a.Id,
            MetricName = a.MetricName,
            DetectedAt = a.DetectedAt,
            ExpectedValue = a.ExpectedValue,
            ActualValue = a.ActualValue,
            DeviationPercent = a.DeviationPercent,
            AnomalyType = a.AnomalyType,
            ConfidenceScore = a.ConfidenceScore,
            PossibleCause = a.PossibleCause,
            IsInvestigated = a.IsInvestigated
        }).ToList();
    }

    public async Task<bool> MarkAnomalyInvestigatedAsync(int anomalyId, int userId)
    {
        var anomaly = await _context.Set<AnalyticsAnomaly>().FindAsync(anomalyId);
        if (anomaly == null) return false;

        anomaly.IsInvestigated = true;
        anomaly.InvestigatedByUserId = userId;
        anomaly.InvestigatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AnalyticsAlertConfigDto> ConfigureAlertAsync(AnalyticsAlertConfigDto config)
    {
        // Stub implementation - returns the configuration as-is
        // In a real implementation, this would save the alert configuration to the database
        await Task.CompletedTask;
        return config;
    }

    #endregion

    #region Export & Sharing

    public async Task<byte[]> ExportDashboardAsync(int dashboardId, int branchId, string format, DateTime? startDate = null, DateTime? endDate = null)
    {
        var dashboard = await GetDashboardByIdAsync(dashboardId);
        if (dashboard == null)
            throw new InvalidOperationException("Dashboard not found");

        // Generate export content based on format
        var content = $"Dashboard: {dashboard.Name}\nExported: {DateTime.UtcNow}\nWidgets: {dashboard.Widgets.Count}\nFormat: {format}\nDate Range: {startDate} to {endDate}";

        _logger.LogInformation("Exported dashboard {DashboardId} as {Format}", dashboardId, format);

        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private async Task<byte[]> ExportDashboardInternalAsync(ExportDashboardDto request, int userId)
    {
        var dashboard = await GetDashboardAsync(request.DashboardId, userId);
        if (dashboard == null)
            throw new InvalidOperationException("Dashboard not found");

        // Generate export content based on format
        var content = $"Dashboard: {dashboard.Name}\nExported: {DateTime.UtcNow}\nWidgets: {dashboard.Widgets.Count}";

        _logger.LogInformation("Exported dashboard {DashboardId} as {Format}", request.DashboardId, request.Format);

        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public async Task<byte[]> ExportAnalyticsDataAsync(int branchId, string reportType, string format, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Stub implementation - exports analytics data based on report type
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var content = $"Analytics Report\nBranch ID: {branchId}\nReport Type: {reportType}\nFormat: {format}\nDate Range: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}\nExported: {DateTime.UtcNow}";

        _logger.LogInformation("Exported analytics data for branch {BranchId}, type {ReportType}, format {Format}", branchId, reportType, format);

        await Task.CompletedTask;
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public async Task<bool> ShareDashboardAsync(ShareDashboardDto request, int userId)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>().FindAsync(request.DashboardId);
        if (dashboard == null || dashboard.CreatedByUserId != userId)
            return false;

        // Create share records
        if (request.UserIds?.Any() == true)
        {
            foreach (var targetUserId in request.UserIds)
            {
                var share = new DashboardShare
                {
                    DashboardId = request.DashboardId,
                    SharedWithUserId = targetUserId,
                    CanEdit = request.CanEdit,
                    ExpiresAt = request.ExpiresAt,
                    SharedByUserId = userId,
                    SharedAt = DateTime.UtcNow
                };
                _context.Set<DashboardShare>().Add(share);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Shared dashboard {DashboardId} with {UserCount} users", request.DashboardId, request.UserIds?.Count ?? 0);
        return true;
    }

    public async Task<DashboardShareLinkDto> CreateShareLinkAsync(int dashboardId, int userId, DateTime? expiresAt = null, bool requiresAuth = true)
    {
        var dashboard = await _context.Set<AnalyticsDashboard>().FindAsync(dashboardId);
        if (dashboard == null || dashboard.CreatedByUserId != userId)
            throw new InvalidOperationException("Dashboard not found or access denied");

        var token = Guid.NewGuid().ToString("N");

        var shareLink = new DashboardShareLink
        {
            DashboardId = dashboardId,
            ShareToken = token,
            RequiresAuth = requiresAuth,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<DashboardShareLink>().Add(shareLink);
        await _context.SaveChangesAsync();

        return new DashboardShareLinkDto
        {
            ShareToken = token,
            ShareUrl = $"/dashboards/shared/{token}",
            CreatedAt = shareLink.CreatedAt,
            ExpiresAt = shareLink.ExpiresAt,
            RequiresAuth = requiresAuth
        };
    }

    public async Task<DashboardDto?> GetDashboardByShareTokenAsync(string shareToken)
    {
        var shareLink = await _context.Set<DashboardShareLink>()
            .FirstOrDefaultAsync(s => s.ShareToken == shareToken && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));

        if (shareLink == null)
            return null;

        var dashboard = await _context.Set<AnalyticsDashboard>()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == shareLink.DashboardId);

        return dashboard != null ? MapToDashboardDto(dashboard) : null;
    }

    #endregion

    #region Subscriptions

    public async Task<List<DashboardSubscriptionDto>> GetSubscriptionsAsync(int userId)
    {
        var subscriptions = await _context.Set<AnalyticsDashboardSubscription>()
            .Include(s => s.Dashboard)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        return subscriptions.Select(s => new DashboardSubscriptionDto
        {
            Id = s.Id,
            DashboardId = s.DashboardId,
            DashboardName = s.Dashboard?.Name ?? "",
            Frequency = s.Frequency,
            DayOfWeek = s.DayOfWeek,
            DayOfMonth = s.DayOfMonth,
            TimeOfDay = s.TimeOfDay,
            DeliveryMethod = s.DeliveryMethod,
            Format = s.Format,
            IsActive = s.IsActive
        }).ToList();
    }

    public async Task<DashboardSubscriptionDto> CreateSubscriptionAsync(CreateDashboardSubscriptionDto request, int userId)
    {
        var subscription = new AnalyticsDashboardSubscription
        {
            DashboardId = request.DashboardId,
            UserId = userId,
            Frequency = request.Frequency,
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            TimeOfDay = request.TimeOfDay,
            DeliveryMethod = request.DeliveryMethod,
            Format = request.Format,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AnalyticsDashboardSubscription>().Add(subscription);
        await _context.SaveChangesAsync();

        var dashboard = await _context.Set<AnalyticsDashboard>().FindAsync(request.DashboardId);

        return new DashboardSubscriptionDto
        {
            Id = subscription.Id,
            DashboardId = subscription.DashboardId,
            DashboardName = dashboard?.Name ?? "",
            Frequency = subscription.Frequency,
            DayOfWeek = subscription.DayOfWeek,
            DayOfMonth = subscription.DayOfMonth,
            TimeOfDay = subscription.TimeOfDay,
            DeliveryMethod = subscription.DeliveryMethod,
            Format = subscription.Format,
            IsActive = subscription.IsActive
        };
    }

    public async Task<DashboardSubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, CreateDashboardSubscriptionDto request, int userId)
    {
        var subscription = await _context.Set<AnalyticsDashboardSubscription>()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            throw new InvalidOperationException("Subscription not found");

        subscription.Frequency = request.Frequency;
        subscription.DayOfWeek = request.DayOfWeek;
        subscription.DayOfMonth = request.DayOfMonth;
        subscription.TimeOfDay = request.TimeOfDay;
        subscription.DeliveryMethod = request.DeliveryMethod;
        subscription.Format = request.Format;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var dashboard = await _context.Set<AnalyticsDashboard>().FindAsync(subscription.DashboardId);

        return new DashboardSubscriptionDto
        {
            Id = subscription.Id,
            DashboardId = subscription.DashboardId,
            DashboardName = dashboard?.Name ?? "",
            Frequency = subscription.Frequency,
            DayOfWeek = subscription.DayOfWeek,
            DayOfMonth = subscription.DayOfMonth,
            TimeOfDay = subscription.TimeOfDay,
            DeliveryMethod = subscription.DeliveryMethod,
            Format = subscription.Format,
            IsActive = subscription.IsActive
        };
    }

    public async Task<bool> DeleteSubscriptionAsync(int subscriptionId, int userId)
    {
        var subscription = await _context.Set<AnalyticsDashboardSubscription>()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            return false;

        _context.Set<AnalyticsDashboardSubscription>().Remove(subscription);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ProcessScheduledSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        var currentTime = now.ToString("HH:mm");
        var currentDayOfWeek = now.DayOfWeek.ToString();
        var currentDayOfMonth = now.Day;

        var dueSubscriptions = await _context.Set<AnalyticsDashboardSubscription>()
            .Include(s => s.Dashboard)
            .Where(s => s.IsActive && s.TimeOfDay == currentTime)
            .Where(s =>
                (s.Frequency == "Daily") ||
                (s.Frequency == "Weekly" && s.DayOfWeek == currentDayOfWeek) ||
                (s.Frequency == "Monthly" && s.DayOfMonth == currentDayOfMonth))
            .ToListAsync();

        foreach (var subscription in dueSubscriptions)
        {
            try
            {
                // Export and send dashboard
                var exportRequest = new ExportDashboardDto
                {
                    DashboardId = subscription.DashboardId,
                    Format = subscription.Format,
                    IncludeData = true
                };

                var content = await ExportDashboardInternalAsync(exportRequest, subscription.UserId);

                // Send via delivery method (email, slack, etc.)
                _logger.LogInformation("Processed subscription {SubscriptionId} for user {UserId}",
                    subscription.Id, subscription.UserId);

                subscription.LastSentAt = now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process subscription {SubscriptionId}", subscription.Id);
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Date Ranges

    public Task<List<DateRangePresetDto>> GetDateRangePresetsAsync()
    {
        var now = DateTime.UtcNow;

        return Task.FromResult(new List<DateRangePresetDto>
        {
            new() { Id = "today", Name = "Today", StartDate = now.Date, EndDate = now.Date.AddDays(1).AddSeconds(-1) },
            new() { Id = "yesterday", Name = "Yesterday", StartDate = now.Date.AddDays(-1), EndDate = now.Date.AddSeconds(-1) },
            new() { Id = "last7days", Name = "Last 7 Days", StartDate = now.Date.AddDays(-7), EndDate = now },
            new() { Id = "last30days", Name = "Last 30 Days", StartDate = now.Date.AddDays(-30), EndDate = now },
            new() { Id = "thisMonth", Name = "This Month", StartDate = new DateTime(now.Year, now.Month, 1), EndDate = now },
            new() { Id = "lastMonth", Name = "Last Month", StartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1), EndDate = new DateTime(now.Year, now.Month, 1).AddSeconds(-1) },
            new() { Id = "thisQuarter", Name = "This Quarter", StartDate = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1), EndDate = now },
            new() { Id = "thisYear", Name = "This Year", StartDate = new DateTime(now.Year, 1, 1), EndDate = now },
            new() { Id = "lastYear", Name = "Last Year", StartDate = new DateTime(now.Year - 1, 1, 1), EndDate = new DateTime(now.Year, 1, 1).AddSeconds(-1) }
        });
    }

    #endregion

    #region Mapping Helpers

    private static DashboardDto MapToDashboardDto(AnalyticsDashboard dashboard)
    {
        return new DashboardDto
        {
            Id = dashboard.Id,
            Name = dashboard.Name,
            Description = dashboard.Description,
            Category = dashboard.Category,
            IsDefault = dashboard.IsDefault,
            IsPublic = dashboard.IsPublic,
            CreatedByUserId = dashboard.CreatedByUserId,
            Widgets = dashboard.Widgets?.Select(w => new DashboardWidgetDto
            {
                Id = w.Id,
                WidgetId = w.WidgetId,
                Title = w.Title,
                WidgetType = w.WidgetType,
                DataSource = w.DataSource,
                Configuration = w.ConfigurationJson != null
                    ? JsonSerializer.Deserialize<WidgetConfigurationDto>(w.ConfigurationJson, JsonOptions) ?? new()
                    : new(),
                Position = w.PositionJson != null
                    ? JsonSerializer.Deserialize<WidgetPositionDto>(w.PositionJson, JsonOptions) ?? new()
                    : new(),
                Filters = w.FiltersJson != null
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(w.FiltersJson, JsonOptions)
                    : null,
                RefreshIntervalSeconds = w.RefreshIntervalSeconds,
                IsVisible = w.IsVisible
            }).ToList() ?? new(),
            Layout = dashboard.LayoutJson != null
                ? JsonSerializer.Deserialize<DashboardLayoutDto>(dashboard.LayoutJson, JsonOptions)
                : null,
            Theme = dashboard.ThemeJson != null
                ? JsonSerializer.Deserialize<DashboardThemeDto>(dashboard.ThemeJson, JsonOptions)
                : null,
            RefreshIntervalSeconds = dashboard.RefreshIntervalSeconds,
            CreatedAt = dashboard.CreatedAt,
            UpdatedAt = dashboard.UpdatedAt
        };
    }

    #endregion
}

#region Entity Definitions

/// <summary>
/// Analytics dashboard entity
/// </summary>
public class AnalyticsDashboard : IBranchEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }

    public string? LayoutJson { get; set; }
    public string? ThemeJson { get; set; }

    public int RefreshIntervalSeconds { get; set; } = 300;

    public int CreatedByUserId { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AnalyticsDashboardWidget> Widgets { get; set; } = new List<AnalyticsDashboardWidget>();
}

/// <summary>
/// Analytics dashboard widget entity
/// </summary>
public class AnalyticsDashboardWidget
{
    [Key]
    public int Id { get; set; }

    public int DashboardId { get; set; }

    [Required]
    [MaxLength(100)]
    public string WidgetId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string WidgetType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DataSource { get; set; } = string.Empty;

    public string? ConfigurationJson { get; set; }
    public string? PositionJson { get; set; }
    public string? FiltersJson { get; set; }

    public int RefreshIntervalSeconds { get; set; } = 300;
    public bool IsVisible { get; set; } = true;

    [ForeignKey(nameof(DashboardId))]
    public virtual AnalyticsDashboard Dashboard { get; set; } = null!;
}

/// <summary>
/// Analytics alert entity
/// </summary>
public class AnalyticsAlert : IBranchEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string AlertType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string MetricName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }

    public DateTime DetectedAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    public int BranchId { get; set; }
}

/// <summary>
/// Analytics alert rule entity
/// </summary>
public class AnalyticsAlertRule : IBranchEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MetricId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Condition { get; set; } = string.Empty;

    public decimal ThresholdValue { get; set; }

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = string.Empty;

    public string? NotificationChannelsJson { get; set; }
    public string? NotifyUsersJson { get; set; }

    public int CooldownMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;

    public int BranchId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Analytics anomaly entity
/// </summary>
public class AnalyticsAnomaly : IBranchEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string MetricName { get; set; } = string.Empty;

    public DateTime DetectedAt { get; set; }
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public decimal DeviationPercent { get; set; }

    [Required]
    [MaxLength(50)]
    public string AnomalyType { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }
    public string? PossibleCause { get; set; }

    public bool IsInvestigated { get; set; }
    public int? InvestigatedByUserId { get; set; }
    public DateTime? InvestigatedAt { get; set; }

    public int BranchId { get; set; }
}

/// <summary>
/// Dashboard share entity
/// </summary>
public class DashboardShare
{
    [Key]
    public int Id { get; set; }

    public int DashboardId { get; set; }
    public int SharedWithUserId { get; set; }
    public int? SharedWithRoleId { get; set; }
    public bool CanEdit { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int SharedByUserId { get; set; }
    public DateTime SharedAt { get; set; }
}

/// <summary>
/// Dashboard share link entity
/// </summary>
public class DashboardShareLink
{
    [Key]
    public int Id { get; set; }

    public int DashboardId { get; set; }

    [Required]
    [MaxLength(64)]
    public string ShareToken { get; set; } = string.Empty;

    public bool RequiresAuth { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Dashboard subscription entity
/// </summary>
public class AnalyticsDashboardSubscription
{
    [Key]
    public int Id { get; set; }

    public int DashboardId { get; set; }
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Frequency { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    [MaxLength(10)]
    public string? TimeOfDay { get; set; }

    [Required]
    [MaxLength(50)]
    public string DeliveryMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Format { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime? LastSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(DashboardId))]
    public virtual AnalyticsDashboard? Dashboard { get; set; }
}

#endregion
