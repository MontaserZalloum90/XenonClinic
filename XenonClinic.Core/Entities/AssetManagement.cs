namespace XenonClinic.Core.Entities;

/// <summary>
/// Physical asset/equipment.
/// </summary>
public class Asset : AuditableEntityWithId
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? ModelNumber { get; set; }
    public string? Manufacturer { get; set; }

    public int CategoryId { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? AssignedToEmployeeId { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public AssetCondition Condition { get; set; } = AssetCondition.Good;

    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int? SupplierId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }

    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? WarrantyProvider { get; set; }
    public string? WarrantyNotes { get; set; }

    // Depreciation
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeMonths { get; set; } = 60;
    public decimal? SalvageValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public DateTime? DepreciationStartDate { get; set; }

    // Maintenance
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? MaintenanceIntervalDays { get; set; }

    // Calibration (for medical equipment)
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public int? CalibrationIntervalDays { get; set; }

    public DateTime? DisposalDate { get; set; }
    public string? DisposalMethod { get; set; }
    public decimal? DisposalValue { get; set; }
    public string? DisposalNotes { get; set; }

    public string? Notes { get; set; }
    public string? CustomFieldsJson { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    // Navigation
    public virtual AssetCategory? Category { get; set; }
    public virtual ICollection<AssetMaintenanceRecord> MaintenanceRecords { get; set; } = new List<AssetMaintenanceRecord>();
    public virtual ICollection<AssetDepreciationEntry> DepreciationEntries { get; set; } = new List<AssetDepreciationEntry>();
    public virtual ICollection<AssetTransfer> Transfers { get; set; } = new List<AssetTransfer>();
}

/// <summary>
/// Asset category for grouping.
/// </summary>
public class AssetCategory : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }

    public int DefaultUsefulLifeMonths { get; set; } = 60;
    public DepreciationMethod DefaultDepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int? DefaultMaintenanceIntervalDays { get; set; }
    public int? DefaultCalibrationIntervalDays { get; set; }

    public string? AccountCode { get; set; } // GL account for this category
    public string? DepreciationAccountCode { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }

    public virtual AssetCategory? ParentCategory { get; set; }
    public virtual ICollection<AssetCategory> SubCategories { get; set; } = new List<AssetCategory>();
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

/// <summary>
/// Asset location.
/// </summary>
public class AssetLocation : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentLocationId { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual AssetLocation? ParentLocation { get; set; }
}

/// <summary>
/// Maintenance record for an asset.
/// </summary>
public class AssetMaintenanceRecord : AuditableEntityWithId
{
    public int AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public string? WorkOrderNumber { get; set; }

    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public string? TechnicianName { get; set; }
    public int? TechnicianEmployeeId { get; set; }

    public decimal? LaborCost { get; set; }
    public decimal? PartsCost { get; set; }
    public decimal? ExternalServiceCost { get; set; }
    public decimal TotalCost { get; set; }

    public int? VendorId { get; set; }
    public string? VendorInvoiceNumber { get; set; }

    public string? PartsUsedJson { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }

    public DateTime? NextMaintenanceDate { get; set; }
    public AssetCondition? ConditionAfter { get; set; }

    public int CompanyId { get; set; }

    public virtual Asset? Asset { get; set; }
}

/// <summary>
/// Monthly depreciation entry.
/// </summary>
public class AssetDepreciationEntry : AuditableEntityWithId
{
    public int AssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime EntryDate { get; set; }

    public decimal OpeningValue { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal ClosingValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }

    public bool IsPosted { get; set; }
    public string? JournalEntryId { get; set; }

    public int CompanyId { get; set; }

    public virtual Asset? Asset { get; set; }
}

/// <summary>
/// Asset transfer between locations/departments.
/// </summary>
public class AssetTransfer : AuditableEntityWithId
{
    public int AssetId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;

    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }
    public int? FromBranchId { get; set; }
    public int? ToBranchId { get; set; }
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }

    public DateTime TransferDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedBy { get; set; }

    public int CompanyId { get; set; }

    public virtual Asset? Asset { get; set; }
}

/// <summary>
/// Asset disposal record.
/// </summary>
public class AssetDisposal : AuditableEntityWithId
{
    public int AssetId { get; set; }
    public string DisposalNumber { get; set; } = string.Empty;

    public DateTime DisposalDate { get; set; }
    public DisposalMethod Method { get; set; }
    public string? DisposalReason { get; set; }

    public decimal NetBookValueAtDisposal { get; set; }
    public decimal SalePrice { get; set; }
    public decimal GainLoss { get; set; }

    public string? BuyerName { get; set; }
    public string? BuyerContact { get; set; }
    public string? DocumentReference { get; set; }

    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Notes { get; set; }

    public bool IsPosted { get; set; }
    public string? JournalEntryId { get; set; }

    public int CompanyId { get; set; }

    public virtual Asset? Asset { get; set; }
}

// Enums
public enum AssetStatus
{
    Active = 1,
    InRepair = 2,
    InStorage = 3,
    OnLoan = 4,
    Reserved = 5,
    Retired = 6,
    Disposed = 7,
    Lost = 8
}

public enum AssetCondition
{
    Excellent = 1,
    Good = 2,
    Fair = 3,
    Poor = 4,
    NonFunctional = 5
}

public enum DepreciationMethod
{
    StraightLine = 1,
    DecliningBalance = 2,
    DoubleDecliningBalance = 3,
    SumOfYearsDigits = 4,
    UnitsOfProduction = 5
}

public enum MaintenanceType
{
    Preventive = 1,
    Corrective = 2,
    Emergency = 3,
    Calibration = 4,
    Inspection = 5,
    Upgrade = 6
}

public enum MaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Overdue = 5
}

public enum DisposalMethod
{
    Sale = 1,
    Donation = 2,
    TradeIn = 3,
    Scrap = 4,
    WriteOff = 5,
    Theft = 6,
    Destruction = 7
}
