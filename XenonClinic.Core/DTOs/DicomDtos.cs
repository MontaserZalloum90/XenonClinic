namespace XenonClinic.Core.DTOs;

#region DICOM Study DTOs

/// <summary>
/// DICOM Study DTO
/// </summary>
public class DicomStudyDto
{
    public int Id { get; set; }
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? StudyId { get; set; }
    public string? AccessionNumber { get; set; }
    public DateTime StudyDate { get; set; }
    public string? StudyTime { get; set; }
    public string? StudyDescription { get; set; }
    public string Modality { get; set; } = string.Empty; // CT, MR, XR, US, etc.
    public string? ModalityDescription { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public string? ReferringPhysician { get; set; }
    public string? InstitutionName { get; set; }
    public string? BodyPartExamined { get; set; }
    public int NumberOfSeries { get; set; }
    public int NumberOfInstances { get; set; }
    public string Status { get; set; } = string.Empty; // New, Viewed, Reported, Archived
    public string? StorageLocation { get; set; }
    public long? TotalSizeBytes { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public List<DicomSeriesDto>? Series { get; set; }
}

/// <summary>
/// DICOM Study list item for quick display
/// </summary>
public class DicomStudySummaryDto
{
    public int Id { get; set; }
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? AccessionNumber { get; set; }
    public DateTime StudyDate { get; set; }
    public string? StudyDescription { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public int NumberOfSeries { get; set; }
    public int NumberOfInstances { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasReport { get; set; }
}

/// <summary>
/// Create DICOM study request (for manual entry)
/// </summary>
public class CreateDicomStudyDto
{
    public int PatientId { get; set; }
    public string Modality { get; set; } = string.Empty;
    public DateTime StudyDate { get; set; }
    public string? StudyDescription { get; set; }
    public string? ReferringPhysician { get; set; }
    public string? BodyPartExamined { get; set; }
    public string? AccessionNumber { get; set; }
}

#endregion

#region DICOM Series DTOs

/// <summary>
/// DICOM Series DTO
/// </summary>
public class DicomSeriesDto
{
    public int Id { get; set; }
    public int StudyId { get; set; }
    public string SeriesInstanceUid { get; set; } = string.Empty;
    public int? SeriesNumber { get; set; }
    public string? SeriesDescription { get; set; }
    public string Modality { get; set; } = string.Empty;
    public DateTime? SeriesDate { get; set; }
    public string? SeriesTime { get; set; }
    public string? BodyPartExamined { get; set; }
    public string? PatientPosition { get; set; }
    public string? ProtocolName { get; set; }
    public int NumberOfInstances { get; set; }
    public string? PerformingPhysician { get; set; }
    public string? OperatorName { get; set; }
    public List<DicomInstanceDto>? Instances { get; set; }
}

#endregion

#region DICOM Instance DTOs

/// <summary>
/// DICOM Instance (Image) DTO
/// </summary>
public class DicomInstanceDto
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public string SopInstanceUid { get; set; } = string.Empty;
    public string SopClassUid { get; set; } = string.Empty;
    public int? InstanceNumber { get; set; }
    public string? ImageType { get; set; }
    public int? Rows { get; set; }
    public int? Columns { get; set; }
    public int? BitsAllocated { get; set; }
    public int? BitsStored { get; set; }
    public string? PhotometricInterpretation { get; set; }
    public double? PixelSpacing { get; set; }
    public double? SliceThickness { get; set; }
    public double? SliceLocation { get; set; }
    public double? WindowCenter { get; set; }
    public double? WindowWidth { get; set; }
    public int? NumberOfFrames { get; set; }
    public string? ContentDate { get; set; }
    public string? ContentTime { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FilePath { get; set; }
    public string? TransferSyntaxUid { get; set; }
}

#endregion

#region DICOM Viewer DTOs

/// <summary>
/// DICOM viewer configuration
/// </summary>
public class DicomViewerConfigDto
{
    public string ViewerUrl { get; set; } = string.Empty;
    public string ViewerType { get; set; } = "OHIF"; // OHIF, Cornerstone, Custom
    public string? WadoRsUrl { get; set; }
    public string? QidoRsUrl { get; set; }
    public string? StowRsUrl { get; set; }
    public bool EnableMpr { get; set; } = true;
    public bool EnableVr { get; set; }
    public bool EnableMeasurements { get; set; } = true;
    public bool EnableAnnotations { get; set; } = true;
    public Dictionary<string, object>? AdditionalConfig { get; set; }
}

/// <summary>
/// DICOM viewer session
/// </summary>
public class DicomViewerSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string ViewerUrl { get; set; } = string.Empty;
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? SeriesInstanceUid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? AccessToken { get; set; }
}

/// <summary>
/// DICOM image request
/// </summary>
public class DicomImageRequestDto
{
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? SeriesInstanceUid { get; set; }
    public string? SopInstanceUid { get; set; }
    public int? Frame { get; set; }
    public string? Quality { get; set; } // thumbnail, preview, full
    public string? OutputFormat { get; set; } // jpeg, png, dicom
    public int? Width { get; set; }
    public int? Height { get; set; }
}

/// <summary>
/// DICOM image response
/// </summary>
public class DicomImageResponseDto
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "image/jpeg";
    public int Width { get; set; }
    public int Height { get; set; }
    public string? SopInstanceUid { get; set; }
    public int? NumberOfFrames { get; set; }
    public int? CurrentFrame { get; set; }
    public string? PhotometricInterpretation { get; set; }
    public double? PixelSpacing { get; set; }
    public string? ImageType { get; set; }
    public string? TransferSyntaxUid { get; set; }
}

#endregion

#region DICOM Search DTOs

/// <summary>
/// DICOM study search request (C-FIND equivalent)
/// </summary>
public class DicomStudySearchDto
{
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public string? StudyInstanceUid { get; set; }
    public string? AccessionNumber { get; set; }
    public string? Modality { get; set; }
    public DateTime? StudyDateFrom { get; set; }
    public DateTime? StudyDateTo { get; set; }
    public string? ReferringPhysician { get; set; }
    public string? BodyPartExamined { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DICOM search result
/// </summary>
public class DicomSearchResultDto
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<DicomStudySummaryDto> Studies { get; set; } = new();
}

#endregion

#region DICOM Worklist DTOs

/// <summary>
/// DICOM Modality Worklist entry
/// </summary>
public class DicomWorklistEntryDto
{
    public int Id { get; set; }
    public string AccessionNumber { get; set; } = string.Empty;
    public string ScheduledProcedureStepId { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientMRN { get; set; }
    public DateTime? PatientBirthDate { get; set; }
    public string? PatientSex { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? ScheduledStationAETitle { get; set; }
    public string? ScheduledPerformingPhysician { get; set; }
    public string? RequestedProcedureDescription { get; set; }
    public string? RequestedProcedureCode { get; set; }
    public string? ReferringPhysician { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled
    public int? OrderId { get; set; }
}

/// <summary>
/// Create worklist entry request
/// </summary>
public class CreateWorklistEntryDto
{
    public int PatientId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? ScheduledStationAETitle { get; set; }
    public string? RequestedProcedureDescription { get; set; }
    public string? RequestedProcedureCode { get; set; }
    public string? ReferringPhysician { get; set; }
    public int? OrderId { get; set; }
}

#endregion

#region DICOM Storage/Transfer DTOs

/// <summary>
/// DICOM storage request (C-STORE equivalent)
/// </summary>
public class DicomStoreRequestDto
{
    public int PatientId { get; set; }
    public string? StudyInstanceUid { get; set; }
    public string SourceAeTitle { get; set; } = string.Empty;
    public byte[] DicomData { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DICOM storage response
/// </summary>
public class DicomStoreResponseDto
{
    public bool Success { get; set; }
    public string? StudyInstanceUid { get; set; }
    public string? SeriesInstanceUid { get; set; }
    public string? SopInstanceUid { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
}

/// <summary>
/// DICOM move/retrieve request (C-MOVE equivalent)
/// </summary>
public class DicomMoveRequestDto
{
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? SeriesInstanceUid { get; set; }
    public string DestinationAeTitle { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
}

/// <summary>
/// DICOM export request
/// </summary>
public class DicomExportRequestDto
{
    public List<string> StudyInstanceUids { get; set; } = new();
    public string ExportFormat { get; set; } = "DICOM"; // DICOM, JPEG, PNG, PDF
    public bool Anonymize { get; set; }
    public string? DestinationPath { get; set; }
    public string? DestinationAeTitle { get; set; }
}

/// <summary>
/// DICOM export response
/// </summary>
public class DicomExportResponseDto
{
    public bool Success { get; set; }
    public string? ExportPath { get; set; }
    public byte[]? ExportData { get; set; }
    public int TotalStudies { get; set; }
    public int TotalInstances { get; set; }
    public long TotalSizeBytes { get; set; }
    public string? Message { get; set; }
}

#endregion

#region PACS Configuration DTOs

/// <summary>
/// PACS server configuration
/// </summary>
public class PacsServerConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AeTitle { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 104;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string ServerType { get; set; } = "Query/Retrieve"; // Query/Retrieve, Storage, Worklist
    public bool SupportsStore { get; set; }
    public bool SupportsFind { get; set; }
    public bool SupportsMove { get; set; }
    public bool SupportsGet { get; set; }
    public bool SupportsWorklist { get; set; }
    public string? CallingAeTitle { get; set; }
    public int? MaxPduLength { get; set; }
    public int? AssociationTimeout { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public bool IsVerified { get; set; }
}

/// <summary>
/// Create/Update PACS configuration
/// </summary>
public class CreatePacsConfigDto
{
    public string Name { get; set; } = string.Empty;
    public string AeTitle { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 104;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public string? CallingAeTitle { get; set; }
}

/// <summary>
/// PACS connection test result
/// </summary>
public class PacsConnectionTestDto
{
    public bool Success { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? Message { get; set; }
    public List<string> SupportedSopClasses { get; set; } = new();
    public List<string> SupportedTransferSyntaxes { get; set; } = new();
}

#endregion

#region Radiology Report DTOs

/// <summary>
/// Radiology report DTO
/// </summary>
public class RadiologyReportDto
{
    public int Id { get; set; }
    public int StudyId { get; set; }
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? AccessionNumber { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public DateTime StudyDate { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? StudyDescription { get; set; }
    public int? ReportingRadiologistId { get; set; }
    public string? ReportingRadiologistName { get; set; }
    public string? ClinicalHistory { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Preliminary, Final, Amended, Addendum
    public DateTime? DraftedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? AmendedAt { get; set; }
    public string? AmendmentReason { get; set; }
    public List<RadiologyReportKeyImageDto>? KeyImages { get; set; }
}

/// <summary>
/// Create radiology report request
/// </summary>
public class CreateRadiologyReportDto
{
    public int StudyId { get; set; }
    public string? ClinicalHistory { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public List<string>? KeyImageSopInstanceUids { get; set; }
}

/// <summary>
/// Key image reference in report
/// </summary>
public class RadiologyReportKeyImageDto
{
    public string SopInstanceUid { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? SequenceNumber { get; set; }
}

#endregion

#region DICOM Statistics DTOs

/// <summary>
/// DICOM/Imaging statistics
/// </summary>
public class DicomStatisticsDto
{
    public int TotalStudies { get; set; }
    public int TotalSeries { get; set; }
    public int TotalInstances { get; set; }
    public long TotalStorageBytes { get; set; }
    public string TotalStorageFormatted { get; set; } = string.Empty;
    public Dictionary<string, int> StudiesByModality { get; set; } = new();
    public Dictionary<string, int> StudiesByStatus { get; set; } = new();
    public int StudiesToday { get; set; }
    public int StudiesThisWeek { get; set; }
    public int StudiesThisMonth { get; set; }
    public int PendingReports { get; set; }
    public double AverageReportingTimeHours { get; set; }
}

#endregion
