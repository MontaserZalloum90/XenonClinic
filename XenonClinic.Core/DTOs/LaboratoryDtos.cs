using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Lab Test DTOs

/// <summary>
/// DTO for displaying lab test information.
/// </summary>
public class LabTestDto
{
    public int Id { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public SpecimenType? SpecimenType { get; set; }
    public string? SpecimenTypeName => SpecimenType?.ToString();
    public string? SpecimenVolume { get; set; }
    public int? TurnaroundTimeHours { get; set; }
    public decimal Price { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Methodology { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresFasting { get; set; }
    public string? PreparationInstructions { get; set; }
    public int? ExternalLabId { get; set; }
    public string? ExternalLabName { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating a new lab test.
/// </summary>
public class CreateLabTestDto
{
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCategory Category { get; set; }
    public SpecimenType? SpecimenType { get; set; }
    public string? SpecimenVolume { get; set; }
    public int? TurnaroundTimeHours { get; set; }
    public decimal Price { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Methodology { get; set; }
    public bool RequiresFasting { get; set; }
    public string? PreparationInstructions { get; set; }
    public int? ExternalLabId { get; set; }
}

/// <summary>
/// DTO for updating an existing lab test.
/// </summary>
public class UpdateLabTestDto
{
    public int Id { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCategory Category { get; set; }
    public SpecimenType? SpecimenType { get; set; }
    public string? SpecimenVolume { get; set; }
    public int? TurnaroundTimeHours { get; set; }
    public decimal Price { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Methodology { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresFasting { get; set; }
    public string? PreparationInstructions { get; set; }
    public int? ExternalLabId { get; set; }
}

/// <summary>
/// DTO for lab test list request with filtering and pagination.
/// </summary>
public class LabTestListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public TestCategory? Category { get; set; }
    public bool? IsActive { get; set; }
    public bool? RequiresFasting { get; set; }
    public int? ExternalLabId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

#endregion

#region Lab Order DTOs

/// <summary>
/// DTO for displaying lab order information.
/// </summary>
public class LabOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public LabOrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? ExternalLabId { get; set; }
    public string? ExternalLabName { get; set; }
    public string? OrderedBy { get; set; }
    public DateTime? CollectionDate { get; set; }
    public string? CollectedBy { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public bool IsUrgent { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public List<LabOrderItemDto> Items { get; set; } = new();
    public List<LabResultDto> Results { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for lab order item information.
/// </summary>
public class LabOrderItemDto
{
    public int Id { get; set; }
    public int LabOrderId { get; set; }
    public int LabTestId { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a new lab order.
/// </summary>
public class CreateLabOrderDto
{
    public int PatientId { get; set; }
    public int? ExternalLabId { get; set; }
    public bool IsUrgent { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public List<CreateLabOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a lab order item.
/// </summary>
public class CreateLabOrderItemDto
{
    public int LabTestId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing lab order.
/// </summary>
public class UpdateLabOrderDto
{
    public int Id { get; set; }
    public LabOrderStatus Status { get; set; }
    public int? ExternalLabId { get; set; }
    public bool IsUrgent { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
}

/// <summary>
/// DTO for updating lab order status.
/// </summary>
public class UpdateLabOrderStatusDto
{
    public int LabOrderId { get; set; }
    public LabOrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for collecting samples for a lab order.
/// </summary>
public class CollectSamplesDto
{
    public int LabOrderId { get; set; }
    public DateTime CollectionDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for lab order list request with filtering and pagination.
/// </summary>
public class LabOrderListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public LabOrderStatus? Status { get; set; }
    public bool? IsUrgent { get; set; }
    public bool? IsPaid { get; set; }
    public int? ExternalLabId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Lab Result DTOs

/// <summary>
/// DTO for displaying lab result information.
/// </summary>
public class LabResultDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int LabOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int LabOrderItemId { get; set; }
    public int LabTestId { get; set; }
    public string? TestCode { get; set; }
    public string? TestName { get; set; }
    public LabResultStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? ResultDate { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Interpretation { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? PerformedDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for entering lab result.
/// </summary>
public class EnterLabResultDto
{
    public int LabOrderItemId { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Interpretation { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// DTO for reviewing lab result.
/// </summary>
public class ReviewLabResultDto
{
    public int LabResultId { get; set; }
    public string? Interpretation { get; set; }
    public string? Notes { get; set; }
    public bool IsAbnormal { get; set; }
}

/// <summary>
/// DTO for verifying lab result.
/// </summary>
public class VerifyLabResultDto
{
    public int LabResultId { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region External Lab DTOs

/// <summary>
/// DTO for displaying external lab information.
/// </summary>
public class ExternalLabDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? TurnaroundTimeDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsLicenseExpired => LicenseExpiryDate.HasValue && LicenseExpiryDate.Value < DateTime.UtcNow.Date;
    public bool IsLicenseExpiringSoon => LicenseExpiryDate.HasValue && LicenseExpiryDate.Value <= DateTime.UtcNow.Date.AddDays(30);
    public string? Notes { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating a new external lab.
/// </summary>
public class CreateExternalLabDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? TurnaroundTimeDays { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing external lab.
/// </summary>
public class UpdateExternalLabDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? TurnaroundTimeDays { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Statistics DTOs

/// <summary>
/// DTO for laboratory statistics.
/// </summary>
public class LabStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int UrgentOrders { get; set; }
    public int PaidOrders { get; set; }
    public int UnpaidOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalTests { get; set; }
    public int ActiveTests { get; set; }
    public int TotalExternalLabs { get; set; }
    public int ActiveExternalLabs { get; set; }
    public Dictionary<LabOrderStatus, int> OrdersByStatus { get; set; } = new();
    public Dictionary<TestCategory, int> TestsByCategory { get; set; } = new();
}

#endregion

#region Validation Messages

/// <summary>
/// Validation messages for laboratory operations.
/// </summary>
public static class LabValidationMessages
{
    // Test Code
    public const string TestCodeRequired = "Test code is required";
    public const string TestCodeTooLong = "Test code cannot exceed 50 characters";
    public const string TestCodeInvalid = "Test code format is invalid";

    // Test Name
    public const string TestNameRequired = "Test name is required";
    public const string TestNameTooLong = "Test name cannot exceed 200 characters";

    // Category
    public const string CategoryInvalid = "Invalid test category";

    // Price
    public const string PriceInvalid = "Price must be greater than 0";

    // Turnaround Time
    public const string TurnaroundTimeInvalid = "Turnaround time must be greater than 0";

    // Order
    public const string PatientRequired = "Patient is required";
    public const string OrderIdRequired = "Lab order ID is required";
    public const string OrderStatusInvalid = "Invalid order status";
    public const string OrderItemsRequired = "At least one test is required";

    // Result
    public const string ResultValueRequired = "Result value is required";
    public const string LabOrderItemRequired = "Lab order item is required";
    public const string ResultIdRequired = "Lab result ID is required";
    public const string ResultStatusInvalid = "Invalid result status";

    // External Lab
    public const string LabNameRequired = "Lab name is required";
    public const string LabNameTooLong = "Lab name cannot exceed 100 characters";
    public const string EmailInvalid = "Invalid email format";
    public const string PhoneInvalid = "Invalid phone format";
    public const string LicenseExpired = "External lab license has expired";

    // Collection
    public const string CollectionDateRequired = "Collection date is required";
    public const string CollectionDateFuture = "Collection date cannot be in the future";

    // Pagination
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";

    // Date Range
    public const string DateRangeInvalid = "End date must be greater than or equal to start date";
}

#endregion
