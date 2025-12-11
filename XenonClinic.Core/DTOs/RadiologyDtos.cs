using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Imaging Study DTOs

/// <summary>
/// DTO for displaying imaging study/procedure information.
/// </summary>
public class ImagingStudyDto
{
    public int Id { get; set; }
    public string StudyCode { get; set; } = string.Empty;
    public string StudyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ImagingModality Modality { get; set; }
    public string ModalityName => Modality.ToString();
    public string? BodyPart { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string? ContrastRequired { get; set; }
    public string? PatientPreparation { get; set; }
    public string? Contraindications { get; set; }
    public string? RadiationDose { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresContrast { get; set; }
    public bool RequiresFasting { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating a new imaging study.
/// </summary>
public class CreateImagingStudyDto
{
    public string StudyCode { get; set; } = string.Empty;
    public string StudyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ImagingModality Modality { get; set; }
    public string? BodyPart { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string? ContrastRequired { get; set; }
    public string? PatientPreparation { get; set; }
    public string? Contraindications { get; set; }
    public string? RadiationDose { get; set; }
    public bool RequiresContrast { get; set; }
    public bool RequiresFasting { get; set; }
}

/// <summary>
/// DTO for updating an existing imaging study.
/// </summary>
public class UpdateImagingStudyDto
{
    public int Id { get; set; }
    public string StudyCode { get; set; } = string.Empty;
    public string StudyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ImagingModality Modality { get; set; }
    public string? BodyPart { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string? ContrastRequired { get; set; }
    public string? PatientPreparation { get; set; }
    public string? Contraindications { get; set; }
    public string? RadiationDose { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresContrast { get; set; }
    public bool RequiresFasting { get; set; }
}

/// <summary>
/// DTO for imaging study list request with filtering and pagination.
/// </summary>
public class ImagingStudyListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ImagingModality? Modality { get; set; }
    public string? BodyPart { get; set; }
    public bool? IsActive { get; set; }
    public bool? RequiresContrast { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

#endregion

#region Radiology Order DTOs

/// <summary>
/// DTO for displaying radiology order information.
/// </summary>
public class RadiologyOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public RadiologyOrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public int? AppointmentId { get; set; }
    public int? ClinicalVisitId { get; set; }
    public int? ReferringDoctorId { get; set; }
    public string? ReferringDoctorName { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsStat { get; set; }
    public string? ClinicalHistory { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? Notes { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? PerformedDate { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public int ItemCount { get; set; }
    public List<RadiologyOrderItemDto> Items { get; set; } = new();
    public List<ImagingResultDto> Results { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for radiology order item information.
/// </summary>
public class RadiologyOrderItemDto
{
    public int Id { get; set; }
    public int RadiologyOrderId { get; set; }
    public int ImagingStudyId { get; set; }
    public string? StudyCode { get; set; }
    public string? StudyName { get; set; }
    public ImagingModality? Modality { get; set; }
    public string? ModalityName => Modality?.ToString();
    public string? BodyPart { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
}

/// <summary>
/// DTO for creating a new radiology order.
/// </summary>
public class CreateRadiologyOrderDto
{
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public int? ClinicalVisitId { get; set; }
    public int? ReferringDoctorId { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsStat { get; set; }
    public string? ClinicalHistory { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public List<CreateRadiologyOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a radiology order item.
/// </summary>
public class CreateRadiologyOrderItemDto
{
    public int ImagingStudyId { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// DTO for updating an existing radiology order.
/// </summary>
public class UpdateRadiologyOrderDto
{
    public int Id { get; set; }
    public int? ReferringDoctorId { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsStat { get; set; }
    public string? ClinicalHistory { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledDate { get; set; }
}

/// <summary>
/// DTO for radiology order list request with filtering and pagination.
/// </summary>
public class RadiologyOrderListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public int? ReferringDoctorId { get; set; }
    public RadiologyOrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsUrgent { get; set; }
    public bool? IsStat { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Imaging Result DTOs

/// <summary>
/// DTO for displaying imaging result information.
/// </summary>
public class ImagingResultDto
{
    public int Id { get; set; }
    public int RadiologyOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int RadiologyOrderItemId { get; set; }
    public int ImagingStudyId { get; set; }
    public string? StudyName { get; set; }
    public ImagingModality? Modality { get; set; }
    public ImagingResultStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? ResultDate { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendation { get; set; }
    public string? Technique { get; set; }
    public string? Comparison { get; set; }
    public bool IsCritical { get; set; }
    public string? CriticalFindings { get; set; }
    public string? ImagePath { get; set; }
    public string? DicomStudyUID { get; set; }
    public string? PacsLink { get; set; }
    public int? NumberOfImages { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? PerformedDate { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime? ReportedDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? Notes { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating/updating imaging result.
/// </summary>
public class CreateImagingResultDto
{
    public int RadiologyOrderId { get; set; }
    public int RadiologyOrderItemId { get; set; }
    public int ImagingStudyId { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendation { get; set; }
    public string? Technique { get; set; }
    public string? Comparison { get; set; }
    public bool IsCritical { get; set; }
    public string? CriticalFindings { get; set; }
    public string? ImagePath { get; set; }
    public string? DicomStudyUID { get; set; }
    public string? PacsLink { get; set; }
    public int? NumberOfImages { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing imaging result.
/// </summary>
public class UpdateImagingResultDto
{
    public int Id { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendation { get; set; }
    public string? Technique { get; set; }
    public string? Comparison { get; set; }
    public bool IsCritical { get; set; }
    public string? CriticalFindings { get; set; }
    public string? ImagePath { get; set; }
    public string? DicomStudyUID { get; set; }
    public string? PacsLink { get; set; }
    public int? NumberOfImages { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Workflow DTOs

/// <summary>
/// DTO for receiving a radiology order.
/// </summary>
public class ReceiveRadiologyOrderDto
{
    public int OrderId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for starting imaging on an order.
/// </summary>
public class StartImagingDto
{
    public int OrderId { get; set; }
    public string? Technician { get; set; }
    public string? Room { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for completing a radiology order.
/// </summary>
public class CompleteRadiologyOrderDto
{
    public int OrderId { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for approving a radiology order/report.
/// </summary>
public class ApproveRadiologyOrderDto
{
    public int OrderId { get; set; }
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for rejecting a radiology order.
/// </summary>
public class RejectRadiologyOrderDto
{
    public int OrderId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for adding a report to imaging result.
/// </summary>
public class AddImagingReportDto
{
    public int ResultId { get; set; }
    public string Findings { get; set; } = string.Empty;
    public string Impression { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public string? Technique { get; set; }
    public string? Comparison { get; set; }
    public bool IsCritical { get; set; }
    public string? CriticalFindings { get; set; }
}

/// <summary>
/// DTO for verifying an imaging result.
/// </summary>
public class VerifyImagingResultDto
{
    public int ResultId { get; set; }
    public string? VerifiedBy { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Statistics DTOs

/// <summary>
/// DTO for radiology statistics.
/// </summary>
public class RadiologyStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int UrgentOrders { get; set; }
    public int StatOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalStudies { get; set; }
    public int ActiveStudies { get; set; }
    public Dictionary<RadiologyOrderStatus, int> OrdersByStatus { get; set; } = new();
    public Dictionary<string, int> OrdersByModality { get; set; } = new();
    public List<TopImagingStudyDto> TopStudies { get; set; } = new();
    public double AverageTurnaroundTimeHours { get; set; }
    public int CriticalFindingsCount { get; set; }
}

/// <summary>
/// DTO for top imaging study in statistics.
/// </summary>
public class TopImagingStudyDto
{
    public string StudyName { get; set; } = string.Empty;
    public ImagingModality Modality { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

#region Enums

/// <summary>
/// Imaging modality types.
/// </summary>
public enum ImagingModality
{
    XRay = 0,
    CT = 1,
    MRI = 2,
    Ultrasound = 3,
    Mammography = 4,
    Fluoroscopy = 5,
    PET = 6,
    SPECT = 7,
    Angiography = 8,
    DEXA = 9,
    Nuclear = 10,
    Interventional = 11,
    Other = 99
}

/// <summary>
/// Radiology order status.
/// </summary>
public enum RadiologyOrderStatus
{
    Pending = 0,
    Scheduled = 1,
    Received = 2,
    InProgress = 3,
    Completed = 4,
    Reported = 5,
    Verified = 6,
    Approved = 7,
    Rejected = 8,
    Cancelled = 9
}

/// <summary>
/// Imaging result status.
/// </summary>
public enum ImagingResultStatus
{
    Pending = 0,
    InProgress = 1,
    Preliminary = 2,
    Final = 3,
    Amended = 4,
    Verified = 5,
    Cancelled = 6
}

#endregion

#region Validation Messages

/// <summary>
/// Validation messages for radiology operations.
/// </summary>
public static class RadiologyValidationMessages
{
    // Study
    public const string StudyIdRequired = "Imaging study ID is required";
    public const string StudyCodeRequired = "Study code is required";
    public const string StudyCodeTooLong = "Study code cannot exceed 50 characters";
    public const string StudyNameRequired = "Study name is required";
    public const string StudyNameTooLong = "Study name cannot exceed 200 characters";
    public const string ModalityRequired = "Modality is required";
    public const string ModalityInvalid = "Invalid modality";
    public const string PriceInvalid = "Price must be greater than or equal to 0";
    public const string DurationInvalid = "Duration must be between 1 and 480 minutes";

    // Order
    public const string OrderIdRequired = "Radiology order ID is required";
    public const string PatientRequired = "Patient is required";
    public const string PatientInvalid = "Invalid patient ID";
    public const string OrderItemsRequired = "At least one imaging study is required";
    public const string OrderStatusInvalid = "Invalid order status";

    // Result
    public const string ResultIdRequired = "Imaging result ID is required";
    public const string FindingsRequired = "Findings are required";
    public const string ImpressionRequired = "Impression is required";
    public const string CriticalFindingsRequired = "Critical findings description is required when marked as critical";

    // Workflow
    public const string RejectionReasonRequired = "Rejection reason is required";
    public const string TechnicianRequired = "Technician name is required";

    // Pagination
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";

    // Date Range
    public const string DateRangeInvalid = "End date must be greater than or equal to start date";

    // DICOM/PACS
    public const string DicomUIDInvalid = "Invalid DICOM Study UID format";
    public const string PacsLinkInvalid = "Invalid PACS link URL";
}

#endregion
