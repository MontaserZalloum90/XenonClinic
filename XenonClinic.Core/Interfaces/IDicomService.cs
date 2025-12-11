using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for DICOM/PACS operations
/// </summary>
public interface IDicomService
{
    #region Study Operations

    /// <summary>
    /// Search for DICOM studies
    /// </summary>
    Task<DicomSearchResultDto> SearchStudiesAsync(int branchId, DicomStudySearchDto request);

    /// <summary>
    /// Get a study by ID
    /// </summary>
    Task<DicomStudyDto?> GetStudyByIdAsync(int id);

    /// <summary>
    /// Get a study by Study Instance UID
    /// </summary>
    Task<DicomStudyDto?> GetStudyByUidAsync(int branchId, string studyInstanceUid);

    /// <summary>
    /// Get all studies for a patient
    /// </summary>
    Task<IEnumerable<DicomStudySummaryDto>> GetPatientStudiesAsync(int patientId);

    /// <summary>
    /// Create a new study (manual entry or from received DICOM)
    /// </summary>
    Task<DicomStudyDto> CreateStudyAsync(int branchId, CreateDicomStudyDto dto);

    /// <summary>
    /// Update study status
    /// </summary>
    Task<DicomStudyDto> UpdateStudyStatusAsync(int studyId, string status);

    /// <summary>
    /// Delete a study
    /// </summary>
    Task DeleteStudyAsync(int studyId);

    #endregion

    #region Series & Instance Operations

    /// <summary>
    /// Get series for a study
    /// </summary>
    Task<IEnumerable<DicomSeriesDto>> GetStudySeriesAsync(int studyId);

    /// <summary>
    /// Get instances for a series
    /// </summary>
    Task<IEnumerable<DicomInstanceDto>> GetSeriesInstancesAsync(int seriesId);

    /// <summary>
    /// Get a specific instance
    /// </summary>
    Task<DicomInstanceDto?> GetInstanceAsync(string sopInstanceUid);

    #endregion

    #region Viewer Operations

    /// <summary>
    /// Get viewer configuration
    /// </summary>
    Task<DicomViewerConfigDto> GetViewerConfigAsync(int branchId);

    /// <summary>
    /// Create a viewer session for a study
    /// </summary>
    Task<DicomViewerSessionDto> CreateViewerSessionAsync(int branchId, string studyInstanceUid, int userId);

    /// <summary>
    /// Get rendered image (JPEG/PNG thumbnail or full)
    /// </summary>
    Task<DicomImageResponseDto> GetRenderedImageAsync(DicomImageRequestDto request);

    /// <summary>
    /// Get WADO-RS URL for a study
    /// </summary>
    Task<string> GetWadoRsUrlAsync(int branchId, string studyInstanceUid);

    #endregion

    #region DICOM Storage/Transfer

    /// <summary>
    /// Store DICOM instance (C-STORE)
    /// </summary>
    Task<DicomStoreResponseDto> StoreInstanceAsync(int branchId, DicomStoreRequestDto request);

    /// <summary>
    /// Move/retrieve study from PACS (C-MOVE)
    /// </summary>
    Task<bool> MoveStudyAsync(int branchId, DicomMoveRequestDto request);

    /// <summary>
    /// Export studies to specified format
    /// </summary>
    Task<DicomExportResponseDto> ExportStudiesAsync(int branchId, DicomExportRequestDto request);

    /// <summary>
    /// Import DICOM files from directory
    /// </summary>
    Task<int> ImportFromDirectoryAsync(int branchId, string directoryPath, int patientId);

    #endregion

    #region Worklist Operations

    /// <summary>
    /// Get modality worklist entries
    /// </summary>
    Task<IEnumerable<DicomWorklistEntryDto>> GetWorklistAsync(int branchId, string? modality = null, DateTime? date = null);

    /// <summary>
    /// Create worklist entry
    /// </summary>
    Task<DicomWorklistEntryDto> CreateWorklistEntryAsync(int branchId, CreateWorklistEntryDto dto);

    /// <summary>
    /// Update worklist entry status
    /// </summary>
    Task<DicomWorklistEntryDto> UpdateWorklistStatusAsync(int entryId, string status);

    /// <summary>
    /// Delete worklist entry
    /// </summary>
    Task DeleteWorklistEntryAsync(int entryId);

    /// <summary>
    /// Generate accession number
    /// </summary>
    Task<string> GenerateAccessionNumberAsync(int branchId);

    #endregion

    #region Radiology Reports

    /// <summary>
    /// Get report for a study
    /// </summary>
    Task<RadiologyReportDto?> GetStudyReportAsync(int studyId);

    /// <summary>
    /// Create or update report
    /// </summary>
    Task<RadiologyReportDto> SaveReportAsync(int branchId, CreateRadiologyReportDto dto);

    /// <summary>
    /// Finalize report
    /// </summary>
    Task<RadiologyReportDto> FinalizeReportAsync(int reportId, int radiologistId);

    /// <summary>
    /// Amend report
    /// </summary>
    Task<RadiologyReportDto> AmendReportAsync(int reportId, string amendmentReason, string amendedFindings);

    /// <summary>
    /// Get pending reports
    /// </summary>
    Task<IEnumerable<DicomStudySummaryDto>> GetPendingReportsAsync(int branchId);

    #endregion

    #region PACS Configuration

    /// <summary>
    /// Get PACS server configurations
    /// </summary>
    Task<IEnumerable<PacsServerConfigDto>> GetPacsConfigsAsync(int branchId);

    /// <summary>
    /// Get a specific PACS configuration
    /// </summary>
    Task<PacsServerConfigDto?> GetPacsConfigByIdAsync(int id);

    /// <summary>
    /// Create or update PACS configuration
    /// </summary>
    Task<PacsServerConfigDto> SavePacsConfigAsync(int branchId, CreatePacsConfigDto dto);

    /// <summary>
    /// Delete PACS configuration
    /// </summary>
    Task DeletePacsConfigAsync(int id);

    /// <summary>
    /// Test PACS connection (C-ECHO)
    /// </summary>
    Task<PacsConnectionTestDto> TestPacsConnectionAsync(int configId);

    #endregion

    #region Statistics

    /// <summary>
    /// Get DICOM/Imaging statistics
    /// </summary>
    Task<DicomStatisticsDto> GetStatisticsAsync(int branchId);

    #endregion
}
