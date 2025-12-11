using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for HL7 FHIR operations
/// </summary>
public interface IFhirService
{
    #region Patient Resource

    /// <summary>
    /// Export a patient as FHIR resource
    /// </summary>
    Task<FhirPatientDto> ExportPatientAsync(int patientId);

    /// <summary>
    /// Import a patient from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportPatientAsync(int branchId, FhirPatientDto patient);

    /// <summary>
    /// Search patients using FHIR parameters
    /// </summary>
    Task<FhirBundleDto> SearchPatientsAsync(int branchId, Dictionary<string, string> searchParams);

    #endregion

    #region Practitioner Resource

    /// <summary>
    /// Export a practitioner as FHIR resource
    /// </summary>
    Task<FhirPractitionerDto> ExportPractitionerAsync(int doctorId);

    /// <summary>
    /// Import a practitioner from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportPractitionerAsync(int branchId, FhirPractitionerDto practitioner);

    /// <summary>
    /// Search practitioners using FHIR parameters
    /// </summary>
    Task<FhirBundleDto> SearchPractitionersAsync(int branchId, Dictionary<string, string> searchParams);

    #endregion

    #region Encounter Resource

    /// <summary>
    /// Export an encounter (visit) as FHIR resource
    /// </summary>
    Task<FhirEncounterDto> ExportEncounterAsync(int visitId);

    /// <summary>
    /// Import an encounter from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportEncounterAsync(int branchId, FhirEncounterDto encounter);

    /// <summary>
    /// Search encounters using FHIR parameters
    /// </summary>
    Task<FhirBundleDto> SearchEncountersAsync(int branchId, Dictionary<string, string> searchParams);

    #endregion

    #region Observation Resource

    /// <summary>
    /// Export an observation (vital signs, lab results) as FHIR resource
    /// </summary>
    Task<FhirObservationDto> ExportObservationAsync(int observationId, string observationType);

    /// <summary>
    /// Export patient vital signs as FHIR observations
    /// </summary>
    Task<FhirBundleDto> ExportPatientVitalsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Export patient lab results as FHIR observations
    /// </summary>
    Task<FhirBundleDto> ExportPatientLabResultsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Import an observation from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportObservationAsync(int branchId, FhirObservationDto observation);

    #endregion

    #region Condition Resource

    /// <summary>
    /// Export a condition (diagnosis) as FHIR resource
    /// </summary>
    Task<FhirConditionDto> ExportConditionAsync(int diagnosisId);

    /// <summary>
    /// Export patient conditions as FHIR bundle
    /// </summary>
    Task<FhirBundleDto> ExportPatientConditionsAsync(int patientId, bool activeOnly = false);

    /// <summary>
    /// Import a condition from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportConditionAsync(int branchId, FhirConditionDto condition);

    #endregion

    #region Procedure Resource

    /// <summary>
    /// Export a procedure as FHIR resource
    /// </summary>
    Task<FhirProcedureDto> ExportProcedureAsync(int procedureId);

    /// <summary>
    /// Export patient procedures as FHIR bundle
    /// </summary>
    Task<FhirBundleDto> ExportPatientProceduresAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Import a procedure from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportProcedureAsync(int branchId, FhirProcedureDto procedure);

    #endregion

    #region MedicationRequest Resource

    /// <summary>
    /// Export a prescription as FHIR MedicationRequest
    /// </summary>
    Task<FhirMedicationRequestDto> ExportMedicationRequestAsync(int prescriptionId);

    /// <summary>
    /// Export patient prescriptions as FHIR bundle
    /// </summary>
    Task<FhirBundleDto> ExportPatientMedicationRequestsAsync(int patientId, bool activeOnly = false);

    /// <summary>
    /// Import a medication request from FHIR resource
    /// </summary>
    Task<FhirImportResponseDto> ImportMedicationRequestAsync(int branchId, FhirMedicationRequestDto medicationRequest);

    #endregion

    #region Bundle Operations

    /// <summary>
    /// Export a complete patient record as FHIR bundle
    /// </summary>
    Task<FhirBundleDto> ExportPatientRecordAsync(int patientId, bool includeHistory = false);

    /// <summary>
    /// Import a FHIR bundle
    /// </summary>
    Task<FhirImportResponseDto> ImportBundleAsync(int branchId, FhirBundleDto bundle);

    /// <summary>
    /// Process a FHIR transaction bundle
    /// </summary>
    Task<FhirBundleDto> ProcessTransactionBundleAsync(int branchId, FhirBundleDto bundle);

    #endregion

    #region Validation

    /// <summary>
    /// Validate a FHIR resource
    /// </summary>
    Task<FhirValidationResultDto> ValidateResourceAsync(string resourceJson, string resourceType);

    /// <summary>
    /// Validate a FHIR bundle
    /// </summary>
    Task<FhirValidationResultDto> ValidateBundleAsync(string bundleJson);

    #endregion

    #region Generic Operations

    /// <summary>
    /// Export a resource by type and ID
    /// </summary>
    Task<object> ExportResourceAsync(FhirExportRequestDto request);

    /// <summary>
    /// Import a resource
    /// </summary>
    Task<FhirImportResponseDto> ImportResourceAsync(int branchId, FhirImportRequestDto request);

    /// <summary>
    /// Get capability statement (metadata)
    /// </summary>
    Task<object> GetCapabilityStatementAsync();

    #endregion
}
