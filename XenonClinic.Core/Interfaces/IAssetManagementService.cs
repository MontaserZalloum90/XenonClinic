using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Asset management service.
/// </summary>
public interface IAssetManagementService
{
    // Assets
    Task<Asset> CreateAssetAsync(CreateAssetRequest request);
    Task<Asset?> GetAssetAsync(int assetId);
    Task<Asset?> GetAssetByTagAsync(string assetTag);
    Task<IEnumerable<Asset>> GetAssetsAsync(AssetSearchCriteria criteria);
    Task<Asset> UpdateAssetAsync(int assetId, UpdateAssetRequest request);
    Task UpdateAssetStatusAsync(int assetId, AssetStatus status);
    Task UpdateAssetConditionAsync(int assetId, AssetCondition condition);

    // Categories
    Task<AssetCategory> CreateCategoryAsync(CreateAssetCategoryRequest request);
    Task<IEnumerable<AssetCategory>> GetCategoriesAsync(bool includeInactive = false);
    Task<AssetCategory> UpdateCategoryAsync(int categoryId, UpdateAssetCategoryRequest request);

    // Locations
    Task<AssetLocation> CreateLocationAsync(CreateAssetLocationRequest request);
    Task<IEnumerable<AssetLocation>> GetLocationsAsync(int? branchId = null);

    // Maintenance
    Task<AssetMaintenanceRecord> ScheduleMaintenanceAsync(ScheduleMaintenanceRequest request);
    Task<AssetMaintenanceRecord> CompleteMaintenanceAsync(int maintenanceId, CompleteMaintenanceRequest request);
    Task<IEnumerable<AssetMaintenanceRecord>> GetMaintenanceHistoryAsync(int assetId);
    Task<IEnumerable<AssetMaintenanceRecord>> GetUpcomingMaintenanceAsync(int daysAhead = 30);
    Task<IEnumerable<AssetMaintenanceRecord>> GetOverdueMaintenanceAsync();

    // Depreciation
    Task CalculateDepreciationAsync(int assetId);
    Task RunMonthlyDepreciationAsync(int year, int month);
    Task<IEnumerable<AssetDepreciationEntry>> GetDepreciationScheduleAsync(int assetId);
    Task<DepreciationSummary> GetDepreciationSummaryAsync(int? categoryId = null);

    // Transfers
    Task<AssetTransfer> TransferAssetAsync(TransferAssetRequest request);
    Task<IEnumerable<AssetTransfer>> GetTransferHistoryAsync(int assetId);

    // Disposal
    Task<AssetDisposal> DisposeAssetAsync(DisposeAssetRequest request);
    Task<IEnumerable<AssetDisposal>> GetDisposalHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null);

    // Reporting
    Task<AssetSummary> GetAssetSummaryAsync();
    Task<IEnumerable<AssetUtilization>> GetAssetUtilizationReportAsync(int? categoryId = null);
    Task<IEnumerable<MaintenanceCostAnalysis>> GetMaintenanceCostAnalysisAsync(DateTime startDate, DateTime endDate);
}

// Request DTOs
public record CreateAssetRequest(
    string AssetTag,
    string Name,
    string? Description,
    string? SerialNumber,
    string? ModelNumber,
    string? Manufacturer,
    int CategoryId,
    int? LocationId,
    int? DepartmentId,
    DateTime? PurchaseDate,
    decimal? PurchasePrice,
    int? SupplierId,
    DateTime? WarrantyStartDate,
    DateTime? WarrantyEndDate,
    DepreciationMethod DepreciationMethod = DepreciationMethod.StraightLine,
    int UsefulLifeMonths = 60,
    decimal? SalvageValue = 0
);

public record UpdateAssetRequest(
    string? Name,
    string? Description,
    string? SerialNumber,
    int? LocationId,
    int? DepartmentId,
    int? AssignedToEmployeeId,
    DateTime? WarrantyEndDate,
    int? MaintenanceIntervalDays,
    int? CalibrationIntervalDays,
    string? Notes
);

public record CreateAssetCategoryRequest(
    string Code,
    string Name,
    string? Description,
    int? ParentCategoryId,
    int DefaultUsefulLifeMonths = 60,
    DepreciationMethod DefaultDepreciationMethod = DepreciationMethod.StraightLine,
    int? DefaultMaintenanceIntervalDays,
    string? AccountCode
);

public record UpdateAssetCategoryRequest(
    string? Name,
    string? Description,
    int? DefaultUsefulLifeMonths,
    bool? IsActive
);

public record CreateAssetLocationRequest(
    string Code,
    string Name,
    string? Description,
    int? ParentLocationId,
    string? Building,
    string? Floor,
    string? Room,
    int BranchId
);

public record ScheduleMaintenanceRequest(
    int AssetId,
    MaintenanceType Type,
    DateTime ScheduledDate,
    string? Description,
    int? TechnicianEmployeeId,
    int? VendorId
);

public record CompleteMaintenanceRequest(
    string? WorkPerformed,
    decimal? LaborCost,
    decimal? PartsCost,
    decimal? ExternalServiceCost,
    string? Findings,
    string? Recommendations,
    AssetCondition? ConditionAfter,
    DateTime? NextMaintenanceDate
);

public record TransferAssetRequest(
    int AssetId,
    int? ToLocationId,
    int? ToDepartmentId,
    int? ToBranchId,
    int? ToEmployeeId,
    string? Reason
);

public record DisposeAssetRequest(
    int AssetId,
    DateTime DisposalDate,
    DisposalMethod Method,
    string? DisposalReason,
    decimal SalePrice = 0,
    string? BuyerName,
    string? DocumentReference
);

public record AssetSearchCriteria(
    int? CategoryId = null,
    int? LocationId = null,
    int? DepartmentId = null,
    AssetStatus? Status = null,
    string? SearchTerm = null,
    bool IncludeDisposed = false,
    int Page = 1,
    int PageSize = 50
);

// Response DTOs
public record AssetSummary(
    int TotalAssets,
    int ActiveAssets,
    int InRepairAssets,
    int DisposedAssets,
    decimal TotalPurchaseValue,
    decimal TotalNetBookValue,
    decimal TotalAccumulatedDepreciation,
    Dictionary<string, int> AssetsByCategory,
    Dictionary<string, int> AssetsByStatus
);

public record DepreciationSummary(
    decimal TotalOriginalCost,
    decimal TotalAccumulatedDepreciation,
    decimal TotalNetBookValue,
    decimal MonthlyDepreciationExpense,
    decimal YTDDepreciationExpense
);

public record AssetUtilization(
    int AssetId,
    string AssetTag,
    string AssetName,
    string Category,
    int TotalDays,
    int ActiveDays,
    int DowntimeDays,
    decimal UtilizationRate,
    decimal MaintenanceCost
);

public record MaintenanceCostAnalysis(
    int AssetId,
    string AssetTag,
    string AssetName,
    string Category,
    int MaintenanceCount,
    decimal TotalLaborCost,
    decimal TotalPartsCost,
    decimal TotalExternalCost,
    decimal TotalCost,
    decimal CostPerMaintenance
);
