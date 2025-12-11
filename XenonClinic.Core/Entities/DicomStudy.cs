using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// DICOM Study - top level container for medical images
/// </summary>
public class DicomStudy : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Unique identifier for the study (DICOM tag 0020,000D)
    /// </summary>
    public string StudyInstanceUid { get; set; } = string.Empty;

    /// <summary>
    /// Study ID (DICOM tag 0020,0010)
    /// </summary>
    public string? StudyId { get; set; }

    /// <summary>
    /// Accession Number (DICOM tag 0008,0050)
    /// </summary>
    public string? AccessionNumber { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    /// <summary>
    /// Study Date (DICOM tag 0008,0020)
    /// </summary>
    public DateTime StudyDate { get; set; }

    /// <summary>
    /// Study Time (DICOM tag 0008,0030)
    /// </summary>
    public string? StudyTime { get; set; }

    /// <summary>
    /// Study Description (DICOM tag 0008,1030)
    /// </summary>
    public string? StudyDescription { get; set; }

    /// <summary>
    /// Modality (CT, MR, XR, US, etc.)
    /// </summary>
    public string Modality { get; set; } = string.Empty;

    /// <summary>
    /// Referring Physician Name (DICOM tag 0008,0090)
    /// </summary>
    public string? ReferringPhysician { get; set; }

    /// <summary>
    /// Institution Name (DICOM tag 0008,0080)
    /// </summary>
    public string? InstitutionName { get; set; }

    /// <summary>
    /// Body Part Examined (DICOM tag 0018,0015)
    /// </summary>
    public string? BodyPartExamined { get; set; }

    public int NumberOfSeries { get; set; }
    public int NumberOfInstances { get; set; }

    /// <summary>
    /// Study status (New, Viewed, Reported, Archived)
    /// </summary>
    public string Status { get; set; } = "New";

    /// <summary>
    /// Storage location path
    /// </summary>
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Total size in bytes
    /// </summary>
    public long? TotalSizeBytes { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public ICollection<DicomSeries>? Series { get; set; }
    public RadiologyReport? Report { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DICOM Series - container for instances within a study
/// </summary>
public class DicomSeries
{
    public int Id { get; set; }
    public int StudyId { get; set; }
    public DicomStudy? Study { get; set; }

    /// <summary>
    /// Series Instance UID (DICOM tag 0020,000E)
    /// </summary>
    public string SeriesInstanceUid { get; set; } = string.Empty;

    /// <summary>
    /// Series Number (DICOM tag 0020,0011)
    /// </summary>
    public int? SeriesNumber { get; set; }

    /// <summary>
    /// Series Description (DICOM tag 0008,103E)
    /// </summary>
    public string? SeriesDescription { get; set; }

    /// <summary>
    /// Modality (DICOM tag 0008,0060)
    /// </summary>
    public string Modality { get; set; } = string.Empty;

    public DateTime? SeriesDate { get; set; }
    public string? SeriesTime { get; set; }

    /// <summary>
    /// Body Part Examined (DICOM tag 0018,0015)
    /// </summary>
    public string? BodyPartExamined { get; set; }

    /// <summary>
    /// Patient Position (DICOM tag 0018,5100)
    /// </summary>
    public string? PatientPosition { get; set; }

    /// <summary>
    /// Protocol Name (DICOM tag 0018,1030)
    /// </summary>
    public string? ProtocolName { get; set; }

    public int NumberOfInstances { get; set; }

    /// <summary>
    /// Performing Physician Name (DICOM tag 0008,1050)
    /// </summary>
    public string? PerformingPhysician { get; set; }

    /// <summary>
    /// Operator Name (DICOM tag 0008,1070)
    /// </summary>
    public string? OperatorName { get; set; }

    public ICollection<DicomInstance>? Instances { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DICOM Instance - individual image or object
/// </summary>
public class DicomInstance
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public DicomSeries? Series { get; set; }

    /// <summary>
    /// SOP Instance UID (DICOM tag 0008,0018)
    /// </summary>
    public string SopInstanceUid { get; set; } = string.Empty;

    /// <summary>
    /// SOP Class UID (DICOM tag 0008,0016)
    /// </summary>
    public string SopClassUid { get; set; } = string.Empty;

    /// <summary>
    /// Instance Number (DICOM tag 0020,0013)
    /// </summary>
    public int? InstanceNumber { get; set; }

    /// <summary>
    /// Image Type (DICOM tag 0008,0008)
    /// </summary>
    public string? ImageType { get; set; }

    /// <summary>
    /// Rows (DICOM tag 0028,0010)
    /// </summary>
    public int? Rows { get; set; }

    /// <summary>
    /// Columns (DICOM tag 0028,0011)
    /// </summary>
    public int? Columns { get; set; }

    /// <summary>
    /// Bits Allocated (DICOM tag 0028,0100)
    /// </summary>
    public int? BitsAllocated { get; set; }

    /// <summary>
    /// Bits Stored (DICOM tag 0028,0101)
    /// </summary>
    public int? BitsStored { get; set; }

    /// <summary>
    /// Photometric Interpretation (DICOM tag 0028,0004)
    /// </summary>
    public string? PhotometricInterpretation { get; set; }

    /// <summary>
    /// Pixel Spacing (DICOM tag 0028,0030)
    /// </summary>
    public double? PixelSpacing { get; set; }

    /// <summary>
    /// Slice Thickness (DICOM tag 0018,0050)
    /// </summary>
    public double? SliceThickness { get; set; }

    /// <summary>
    /// Slice Location (DICOM tag 0020,1041)
    /// </summary>
    public double? SliceLocation { get; set; }

    /// <summary>
    /// Window Center (DICOM tag 0028,1050)
    /// </summary>
    public double? WindowCenter { get; set; }

    /// <summary>
    /// Window Width (DICOM tag 0028,1051)
    /// </summary>
    public double? WindowWidth { get; set; }

    public string? ContentDate { get; set; }
    public string? ContentTime { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Path to DICOM file
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Transfer Syntax UID (DICOM tag 0002,0010)
    /// </summary>
    public string? TransferSyntaxUid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Radiology report for a study
/// </summary>
public class RadiologyReport : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int StudyId { get; set; }
    public DicomStudy? Study { get; set; }

    public int? ReportingRadiologistId { get; set; }
    public Doctor? ReportingRadiologist { get; set; }

    public string? ClinicalHistory { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }

    /// <summary>
    /// Report status (Draft, Preliminary, Final, Amended, Addendum)
    /// </summary>
    public string Status { get; set; } = "Draft";

    public DateTime? DraftedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? AmendedAt { get; set; }
    public string? AmendmentReason { get; set; }

    /// <summary>
    /// Key images referenced in report (JSON array of SOP Instance UIDs)
    /// </summary>
    public string? KeyImages { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// PACS Server Configuration
/// </summary>
public class PacsServerConfig : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Name { get; set; } = string.Empty;
    public string AeTitle { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 104;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    /// <summary>
    /// Server type (Query/Retrieve, Storage, Worklist)
    /// </summary>
    public string ServerType { get; set; } = "Query/Retrieve";

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DICOM Modality Worklist entry
/// </summary>
public class DicomWorklistEntry : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string AccessionNumber { get; set; } = string.Empty;
    public string ScheduledProcedureStepId { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public DateTime ScheduledDateTime { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? ScheduledStationAETitle { get; set; }
    public string? ScheduledPerformingPhysician { get; set; }
    public string? RequestedProcedureDescription { get; set; }
    public string? RequestedProcedureCode { get; set; }
    public string? ReferringPhysician { get; set; }

    /// <summary>
    /// Status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Scheduled";

    public int? OrderId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
