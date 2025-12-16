using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for HL7 FHIR operations - Healthcare interoperability
/// </summary>
public class FhirService : IFhirService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<FhirService> _logger;
    private const string FhirVersion = "4.0.1";
    private const string SystemUrl = "http://xenonclinic.com/fhir";

    public FhirService(ClinicDbContext context, ILogger<FhirService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Patient Resource

    public async Task<FhirPatientDto> ExportPatientAsync(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.Branch)
            .FirstOrDefaultAsync(p => p.Id == patientId)
            ?? throw new KeyNotFoundException($"Patient with ID {patientId} not found");

        return MapPatientToFhir(patient);
    }

    public async Task<FhirImportResponseDto> ImportPatientAsync(int branchId, FhirPatientDto fhirPatient)
    {
        var response = new FhirImportResponseDto { ResourceType = "Patient" };

        try
        {
            // Validate required fields
            if (fhirPatient.Name == null || !fhirPatient.Name.Any())
            {
                response.Errors.Add("Patient name is required");
                return response;
            }

            var name = fhirPatient.Name.FirstOrDefault(n => n.Use == "official") ?? fhirPatient.Name.First();

            var firstName = name.Given?.FirstOrDefault() ?? "Unknown";
            var lastName = name.Family ?? "Unknown";
            var patient = new Patient
            {
                BranchId = branchId,
                FullNameEn = $"{firstName} {lastName}".Trim(),
                Gender = MapFhirGender(fhirPatient.Gender),
                DateOfBirth = !string.IsNullOrEmpty(fhirPatient.BirthDate)
                    ? DateTime.Parse(fhirPatient.BirthDate)
                    : DateTime.MinValue,
                IsActive = fhirPatient.Active ?? true,
                CreatedAt = DateTime.UtcNow
            };

            // Map contact info
            if (fhirPatient.Telecom != null)
            {
                var phone = fhirPatient.Telecom.FirstOrDefault(t => t.System == "phone");
                var email = fhirPatient.Telecom.FirstOrDefault(t => t.System == "email");

                if (phone != null) patient.PhoneNumber = phone.Value;
                if (email != null) patient.Email = email.Value;
            }

            // Map address
            if (fhirPatient.Address != null && fhirPatient.Address.Any())
            {
                var address = fhirPatient.Address.First();
                patient.Address = string.Join(", ", address.Line ?? new List<string>());
                patient.City = address.City;
                patient.Country = address.Country;
            }

            // Map external identifier
            if (fhirPatient.Identifier != null)
            {
                var mrn = fhirPatient.Identifier.FirstOrDefault(i => i.Type?.Coding?.Any(c => c.Code == "MR") == true);
                if (mrn != null)
                {
                    patient.MRN = mrn.Value;
                }
            }

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.CreatedId = patient.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR patient");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid data format in FHIR patient import");
            response.Errors.Add($"Invalid data format: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR patient");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    public async Task<FhirBundleDto> SearchPatientsAsync(int branchId, Dictionary<string, string> searchParams)
    {
        var query = _context.Patients.Where(p => p.BranchId == branchId && p.IsActive);

        // Apply search parameters
        if (searchParams.TryGetValue("name", out var name))
        {
            query = query.Where(p =>
                p.FirstName.Contains(name) ||
                p.LastName.Contains(name));
        }

        if (searchParams.TryGetValue("family", out var family))
        {
            query = query.Where(p => p.LastName.Contains(family));
        }

        if (searchParams.TryGetValue("given", out var given))
        {
            query = query.Where(p => p.FirstName.Contains(given));
        }

        if (searchParams.TryGetValue("birthdate", out var birthdate) && DateTime.TryParse(birthdate, out var bd))
        {
            query = query.Where(p => p.DateOfBirth.Date == bd.Date);
        }

        if (searchParams.TryGetValue("gender", out var gender))
        {
            var mappedGender = MapFhirGender(gender);
            query = query.Where(p => p.Gender == mappedGender);
        }

        if (searchParams.TryGetValue("identifier", out var identifier))
        {
            query = query.Where(p => p.MRN == identifier);
        }

        // Pagination
        var count = searchParams.TryGetValue("_count", out var countStr) && int.TryParse(countStr, out var c) ? c : 20;
        var offset = searchParams.TryGetValue("_offset", out var offsetStr) && int.TryParse(offsetStr, out var o) ? o : 0;

        var patients = await query.Skip(offset).Take(count).ToListAsync();
        var total = await query.CountAsync();

        return CreateSearchBundle("Patient", patients.Select(MapPatientToFhir).Cast<object>().ToList(), total);
    }

    #endregion

    #region Practitioner Resource

    public async Task<FhirPractitionerDto> ExportPractitionerAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == doctorId)
            ?? throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");

        return MapDoctorToFhir(doctor);
    }

    public async Task<FhirImportResponseDto> ImportPractitionerAsync(int branchId, FhirPractitionerDto practitioner)
    {
        var response = new FhirImportResponseDto { ResourceType = "Practitioner" };

        try
        {
            if (practitioner.Name == null || !practitioner.Name.Any())
            {
                response.Errors.Add("Practitioner name is required");
                return response;
            }

            var name = practitioner.Name.First();

            var doctor = new Doctor
            {
                BranchId = branchId,
                FirstName = name.Given?.FirstOrDefault() ?? "Unknown",
                LastName = name.Family ?? "Unknown",
                Gender = MapFhirGender(practitioner.Gender),
                IsActive = practitioner.Active ?? true,
                CreatedAt = DateTime.UtcNow
            };

            if (practitioner.Telecom != null)
            {
                var phone = practitioner.Telecom.FirstOrDefault(t => t.System == "phone");
                var email = practitioner.Telecom.FirstOrDefault(t => t.System == "email");

                if (phone != null) doctor.Phone = phone.Value;
                if (email != null) doctor.Email = email.Value;
            }

            if (practitioner.Qualification != null && practitioner.Qualification.Any())
            {
                var qual = practitioner.Qualification.First();
                doctor.Qualification = qual.Code?.Text;
            }

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.CreatedId = doctor.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR practitioner");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR practitioner");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    public async Task<FhirBundleDto> SearchPractitionersAsync(int branchId, Dictionary<string, string> searchParams)
    {
        var query = _context.Doctors.Where(d => d.BranchId == branchId && d.IsActive);

        if (searchParams.TryGetValue("name", out var name))
        {
            query = query.Where(d =>
                d.FirstName.Contains(name) ||
                d.LastName.Contains(name));
        }

        if (searchParams.TryGetValue("family", out var family))
        {
            query = query.Where(d => d.LastName.Contains(family));
        }

        var count = searchParams.TryGetValue("_count", out var countStr) && int.TryParse(countStr, out var c) ? c : 20;
        var offset = searchParams.TryGetValue("_offset", out var offsetStr) && int.TryParse(offsetStr, out var o) ? o : 0;

        var doctors = await query.Skip(offset).Take(count).ToListAsync();
        var total = await query.CountAsync();

        return CreateSearchBundle("Practitioner", doctors.Select(MapDoctorToFhir).Cast<object>().ToList(), total);
    }

    #endregion

    #region Encounter Resource

    public async Task<FhirEncounterDto> ExportEncounterAsync(int visitId)
    {
        var visit = await _context.ClinicalVisits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.Branch)
            .FirstOrDefaultAsync(v => v.Id == visitId)
            ?? throw new KeyNotFoundException($"Visit with ID {visitId} not found");

        return MapVisitToFhirEncounter(visit);
    }

    public async Task<FhirImportResponseDto> ImportEncounterAsync(int branchId, FhirEncounterDto encounter)
    {
        var response = new FhirImportResponseDto { ResourceType = "Encounter" };

        try
        {
            // Extract patient reference
            var patientId = ExtractIdFromReference(encounter.Subject?.Reference);
            if (patientId == null)
            {
                response.Errors.Add("Patient reference is required");
                return response;
            }

            var patient = await _context.Patients.FindAsync(patientId.Value);
            if (patient == null)
            {
                response.Errors.Add($"Patient with ID {patientId} not found");
                return response;
            }

            var visit = new ClinicalVisit
            {
                BranchId = branchId,
                PatientId = patientId.Value,
                VisitDate = encounter.Period?.Start ?? DateTime.UtcNow,
                Status = MapFhirEncounterStatus(encounter.Status),
                CreatedAt = DateTime.UtcNow
            };

            // Extract practitioner
            var practitioner = encounter.Participant?.FirstOrDefault()?.Individual;
            if (practitioner != null)
            {
                var doctorId = ExtractIdFromReference(practitioner.Reference);
                if (doctorId.HasValue)
                {
                    visit.DoctorId = doctorId.Value;
                }
            }

            // Extract reason/chief complaint
            if (encounter.ReasonCode?.Any() == true)
            {
                visit.ChiefComplaint = encounter.ReasonCode.First().Text;
            }

            _context.ClinicalVisits.Add(visit);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.CreatedId = visit.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR encounter");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR encounter");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    public async Task<FhirBundleDto> SearchEncountersAsync(int branchId, Dictionary<string, string> searchParams)
    {
        var query = _context.ClinicalVisits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Where(v => v.BranchId == branchId);

        if (searchParams.TryGetValue("patient", out var patient))
        {
            var patientId = ExtractIdFromReference(patient);
            if (patientId.HasValue)
                query = query.Where(v => v.PatientId == patientId.Value);
        }

        if (searchParams.TryGetValue("date", out var date) && DateTime.TryParse(date, out var d))
        {
            query = query.Where(v => v.VisitDate.Date == d.Date);
        }

        if (searchParams.TryGetValue("status", out var status))
        {
            var mappedStatus = MapFhirEncounterStatus(status);
            query = query.Where(v => v.Status == mappedStatus);
        }

        var count = searchParams.TryGetValue("_count", out var countStr) && int.TryParse(countStr, out var c) ? c : 20;
        var offset = searchParams.TryGetValue("_offset", out var offsetStr) && int.TryParse(offsetStr, out var o) ? o : 0;

        var visits = await query.OrderByDescending(v => v.VisitDate).Skip(offset).Take(count).ToListAsync();
        var total = await query.CountAsync();

        return CreateSearchBundle("Encounter", visits.Select(MapVisitToFhirEncounter).Cast<object>().ToList(), total);
    }

    #endregion

    #region Observation Resource

    public async Task<FhirObservationDto> ExportObservationAsync(int observationId, string observationType)
    {
        if (observationType.Equals("vital", StringComparison.OrdinalIgnoreCase))
        {
            var vital = await _context.VitalSigns
                .Include(v => v.Patient)
                .FirstOrDefaultAsync(v => v.Id == observationId)
                ?? throw new KeyNotFoundException($"Vital sign with ID {observationId} not found");

            return MapVitalSignToFhirObservation(vital);
        }
        else if (observationType.Equals("lab", StringComparison.OrdinalIgnoreCase))
        {
            var labResult = await _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.LabTest)
                .FirstOrDefaultAsync(l => l.Id == observationId)
                ?? throw new KeyNotFoundException($"Lab result with ID {observationId} not found");

            return MapLabResultToFhirObservation(labResult);
        }

        throw new ArgumentException($"Unknown observation type: {observationType}");
    }

    public async Task<FhirBundleDto> ExportPatientVitalsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.VitalSigns
            .Include(v => v.Patient)
            .Where(v => v.PatientId == patientId);

        if (fromDate.HasValue)
            query = query.Where(v => v.RecordedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(v => v.RecordedAt <= toDate.Value);

        var vitals = await query.OrderByDescending(v => v.RecordedAt).Take(100).ToListAsync();

        var observations = new List<object>();
        foreach (var vital in vitals)
        {
            observations.Add(MapVitalSignToFhirObservation(vital));
        }

        return CreateSearchBundle("Observation", observations, observations.Count);
    }

    public async Task<FhirBundleDto> ExportPatientLabResultsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.LabResults
            .Include(l => l.Patient)
            .Include(l => l.LabTest)
            .Where(l => l.PatientId == patientId);

        if (fromDate.HasValue)
            query = query.Where(l => l.ResultDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.ResultDate <= toDate.Value);

        var results = await query.OrderByDescending(l => l.ResultDate).Take(100).ToListAsync();

        var observations = results.Select(r => MapLabResultToFhirObservation(r)).Cast<object>().ToList();

        return CreateSearchBundle("Observation", observations, observations.Count);
    }

    public async Task<FhirImportResponseDto> ImportObservationAsync(int branchId, FhirObservationDto observation)
    {
        var response = new FhirImportResponseDto { ResourceType = "Observation" };

        try
        {
            var patientId = ExtractIdFromReference(observation.Subject?.Reference);
            if (patientId == null)
            {
                response.Errors.Add("Subject (patient) reference is required");
                return response;
            }

            // Determine observation type from category
            var category = observation.Category?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Code;

            if (category == "vital-signs")
            {
                var vital = new VitalSign
                {
                    BranchId = branchId,
                    PatientId = patientId.Value,
                    RecordedAt = observation.EffectiveDateTime ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Map based on LOINC code
                var loincCode = observation.Code?.Coding?.FirstOrDefault(c => c.System?.Contains("loinc") == true)?.Code;
                if (observation.ValueQuantity != null)
                {
                    MapLoincToVitalSign(vital, loincCode, observation.ValueQuantity.Value ?? 0);
                }

                _context.VitalSigns.Add(vital);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.CreatedId = vital.Id;
            }
            else
            {
                response.Warnings.Add("Only vital-signs observations are currently supported for import");
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR observation");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR observation");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    #endregion

    #region Condition Resource

    public async Task<FhirConditionDto> ExportConditionAsync(int diagnosisId)
    {
        var diagnosis = await _context.Diagnoses
            .Include(d => d.Patient)
            .Include(d => d.ClinicalVisit)
            .FirstOrDefaultAsync(d => d.Id == diagnosisId)
            ?? throw new KeyNotFoundException($"Diagnosis with ID {diagnosisId} not found");

        return MapDiagnosisToFhirCondition(diagnosis);
    }

    public async Task<FhirBundleDto> ExportPatientConditionsAsync(int patientId, bool activeOnly = false)
    {
        var query = _context.Diagnoses
            .Include(d => d.Patient)
            .Include(d => d.ClinicalVisit)
            .Where(d => d.PatientId == patientId);

        if (activeOnly)
            query = query.Where(d => d.IsActive);

        var diagnoses = await query.OrderByDescending(d => d.DiagnosisDate).ToListAsync();

        var conditions = diagnoses.Select(d => MapDiagnosisToFhirCondition(d)).Cast<object>().ToList();

        return CreateSearchBundle("Condition", conditions, conditions.Count);
    }

    public async Task<FhirImportResponseDto> ImportConditionAsync(int branchId, FhirConditionDto condition)
    {
        var response = new FhirImportResponseDto { ResourceType = "Condition" };

        try
        {
            var patientId = ExtractIdFromReference(condition.Subject?.Reference);
            if (patientId == null)
            {
                response.Errors.Add("Subject (patient) reference is required");
                return response;
            }

            var diagnosis = new Diagnosis
            {
                BranchId = branchId,
                PatientId = patientId.Value,
                DiagnosisDate = condition.OnsetDateTime ?? condition.RecordedDate ?? DateTime.UtcNow,
                IsActive = condition.ClinicalStatus?.Coding?.FirstOrDefault()?.Code == "active",
                CreatedAt = DateTime.UtcNow
            };

            // Extract ICD-10 code
            var icdCoding = condition.Code?.Coding?.FirstOrDefault(c =>
                c.System?.Contains("icd-10") == true || c.System?.Contains("icd10") == true);

            if (icdCoding != null)
            {
                diagnosis.ICD10Code = icdCoding.Code ?? string.Empty;
                diagnosis.Description = icdCoding.Display ?? string.Empty;
            }
            else if (condition.Code?.Text != null)
            {
                diagnosis.Description = condition.Code.Text;
            }

            // Extract encounter reference
            var encounterId = ExtractIdFromReference(condition.Encounter?.Reference);
            if (encounterId.HasValue)
            {
                diagnosis.ClinicalVisitId = encounterId.Value;
            }

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.CreatedId = diagnosis.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR condition");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR condition");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    #endregion

    #region Procedure Resource

    public async Task<FhirProcedureDto> ExportProcedureAsync(int procedureId)
    {
        var procedure = await _context.Procedures
            .Include(p => p.Patient)
            .Include(p => p.ClinicalVisit)
            .FirstOrDefaultAsync(p => p.Id == procedureId)
            ?? throw new KeyNotFoundException($"Procedure with ID {procedureId} not found");

        return MapProcedureToFhir(procedure);
    }

    public async Task<FhirBundleDto> ExportPatientProceduresAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Procedures
            .Include(p => p.Patient)
            .Include(p => p.ClinicalVisit)
            .Where(p => p.PatientId == patientId);

        if (fromDate.HasValue)
            query = query.Where(p => p.ProcedureDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.ProcedureDate <= toDate.Value);

        var procedures = await query.OrderByDescending(p => p.ProcedureDate).ToListAsync();

        var fhirProcedures = procedures.Select(p => MapProcedureToFhir(p)).Cast<object>().ToList();

        return CreateSearchBundle("Procedure", fhirProcedures, fhirProcedures.Count);
    }

    public async Task<FhirImportResponseDto> ImportProcedureAsync(int branchId, FhirProcedureDto fhirProcedure)
    {
        var response = new FhirImportResponseDto { ResourceType = "Procedure" };

        try
        {
            var patientId = ExtractIdFromReference(fhirProcedure.Subject?.Reference);
            if (patientId == null)
            {
                response.Errors.Add("Subject (patient) reference is required");
                return response;
            }

            var procedure = new Procedure
            {
                BranchId = branchId,
                PatientId = patientId.Value,
                ProcedureDate = fhirProcedure.PerformedDateTime ?? DateTime.UtcNow,
                Status = MapFhirProcedureStatus(fhirProcedure.Status),
                CreatedAt = DateTime.UtcNow
            };

            // Extract CPT code
            var cptCoding = fhirProcedure.Code?.Coding?.FirstOrDefault(c =>
                c.System?.Contains("cpt") == true);

            if (cptCoding != null)
            {
                procedure.CPTCode = cptCoding.Code ?? string.Empty;
                procedure.Name = cptCoding.Display ?? string.Empty;
            }
            else if (fhirProcedure.Code?.Text != null)
            {
                procedure.Name = fhirProcedure.Code.Text;
            }

            // Extract encounter reference
            var encounterId = ExtractIdFromReference(fhirProcedure.Encounter?.Reference);
            if (encounterId.HasValue)
            {
                procedure.ClinicalVisitId = encounterId.Value;
            }

            // Extract performer
            var performerRef = fhirProcedure.Performer?.FirstOrDefault()?.Actor?.Reference;
            var performerId = ExtractIdFromReference(performerRef);
            if (performerId.HasValue)
            {
                procedure.DoctorId = performerId.Value;
            }

            _context.Procedures.Add(procedure);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.CreatedId = procedure.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR procedure");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR procedure");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    #endregion

    #region MedicationRequest Resource

    public async Task<FhirMedicationRequestDto> ExportMedicationRequestAsync(int prescriptionId)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId)
            ?? throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found");

        return MapPrescriptionToFhirMedicationRequest(prescription);
    }

    public async Task<FhirBundleDto> ExportPatientMedicationRequestsAsync(int patientId, bool activeOnly = false)
    {
        var query = _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId);

        if (activeOnly)
            query = query.Where(p => p.Status == "Active");

        var prescriptions = await query.OrderByDescending(p => p.PrescriptionDate).ToListAsync();

        var medicationRequests = prescriptions.Select(p => MapPrescriptionToFhirMedicationRequest(p)).Cast<object>().ToList();

        return CreateSearchBundle("MedicationRequest", medicationRequests, medicationRequests.Count);
    }

    public async Task<FhirImportResponseDto> ImportMedicationRequestAsync(int branchId, FhirMedicationRequestDto medicationRequest)
    {
        var response = new FhirImportResponseDto { ResourceType = "MedicationRequest" };

        try
        {
            var patientId = ExtractIdFromReference(medicationRequest.Subject?.Reference);
            if (patientId == null)
            {
                response.Errors.Add("Subject (patient) reference is required");
                return response;
            }

            var prescription = new Prescription
            {
                BranchId = branchId,
                PatientId = patientId.Value,
                PrescriptionDate = medicationRequest.AuthoredOn ?? DateTime.UtcNow,
                Status = MapFhirMedicationRequestStatus(medicationRequest.Status),
                CreatedAt = DateTime.UtcNow
            };

            // Extract requester (doctor)
            var requesterId = ExtractIdFromReference(medicationRequest.Requester?.Reference);
            if (requesterId.HasValue)
            {
                prescription.DoctorId = requesterId.Value;
            }

            // Extract encounter
            var encounterId = ExtractIdFromReference(medicationRequest.Encounter?.Reference);
            if (encounterId.HasValue)
            {
                prescription.ClinicalVisitId = encounterId.Value;
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Add prescription item if medication is specified
            if (medicationRequest.MedicationCodeableConcept != null)
            {
                var item = new PrescriptionItem
                {
                    PrescriptionId = prescription.Id,
                    MedicationName = medicationRequest.MedicationCodeableConcept.Text
                        ?? medicationRequest.MedicationCodeableConcept.Coding?.FirstOrDefault()?.Display
                        ?? "Unknown Medication",
                    CreatedAt = DateTime.UtcNow
                };

                // Extract dosage instructions
                if (medicationRequest.DosageInstruction?.Any() == true)
                {
                    var dosage = medicationRequest.DosageInstruction.First();
                    item.Dosage = dosage.Text;
                    item.Instructions = dosage.PatientInstruction;

                    if (medicationRequest.DispenseRequest?.Quantity != null)
                    {
                        item.Quantity = (int)(medicationRequest.DispenseRequest.Quantity.Value ?? 0);
                    }
                }

                _context.PrescriptionItems.Add(item);
                await _context.SaveChangesAsync();
            }

            response.Success = true;
            response.CreatedId = prescription.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error importing FHIR medication request");
            response.Errors.Add($"Database error during import: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing FHIR medication request");
            response.Errors.Add($"Import failed: {ex.Message}");
        }

        return response;
    }

    #endregion

    #region Bundle Operations

    public async Task<FhirBundleDto> ExportPatientRecordAsync(int patientId, bool includeHistory = false)
    {
        var bundle = new FhirBundleDto
        {
            Id = Guid.NewGuid().ToString(),
            Type = "collection",
            Timestamp = DateTime.UtcNow,
            Entry = new List<FhirBundleEntryDto>()
        };

        // Export patient
        var patient = await ExportPatientAsync(patientId);
        bundle.Entry.Add(new FhirBundleEntryDto
        {
            FullUrl = $"{SystemUrl}/Patient/{patientId}",
            Resource = patient
        });

        // Export conditions
        var conditions = await ExportPatientConditionsAsync(patientId, activeOnly: !includeHistory);
        foreach (var entry in conditions.Entry ?? new List<FhirBundleEntryDto>())
        {
            bundle.Entry.Add(entry);
        }

        // Export procedures
        var procedures = await ExportPatientProceduresAsync(patientId);
        foreach (var entry in procedures.Entry ?? new List<FhirBundleEntryDto>())
        {
            bundle.Entry.Add(entry);
        }

        // Export recent vitals
        var vitals = await ExportPatientVitalsAsync(patientId, DateTime.UtcNow.AddYears(-1));
        foreach (var entry in vitals.Entry ?? new List<FhirBundleEntryDto>())
        {
            bundle.Entry.Add(entry);
        }

        // Export medication requests
        var medications = await ExportPatientMedicationRequestsAsync(patientId, activeOnly: !includeHistory);
        foreach (var entry in medications.Entry ?? new List<FhirBundleEntryDto>())
        {
            bundle.Entry.Add(entry);
        }

        bundle.Total = bundle.Entry.Count;

        return bundle;
    }

    public async Task<FhirImportResponseDto> ImportBundleAsync(int branchId, FhirBundleDto bundle)
    {
        var response = new FhirImportResponseDto { ResourceType = "Bundle" };

        if (bundle.Entry == null || !bundle.Entry.Any())
        {
            response.Errors.Add("Bundle contains no entries");
            return response;
        }

        var imported = 0;

        foreach (var entry in bundle.Entry)
        {
            if (entry.Resource == null) continue;

            try
            {
                var resourceJson = JsonSerializer.Serialize(entry.Resource);
                var resourceType = GetResourceType(resourceJson);

                var importResult = resourceType switch
                {
                    "Patient" => await ImportPatientAsync(branchId, JsonSerializer.Deserialize<FhirPatientDto>(resourceJson)!),
                    "Practitioner" => await ImportPractitionerAsync(branchId, JsonSerializer.Deserialize<FhirPractitionerDto>(resourceJson)!),
                    "Encounter" => await ImportEncounterAsync(branchId, JsonSerializer.Deserialize<FhirEncounterDto>(resourceJson)!),
                    "Condition" => await ImportConditionAsync(branchId, JsonSerializer.Deserialize<FhirConditionDto>(resourceJson)!),
                    "Procedure" => await ImportProcedureAsync(branchId, JsonSerializer.Deserialize<FhirProcedureDto>(resourceJson)!),
                    "MedicationRequest" => await ImportMedicationRequestAsync(branchId, JsonSerializer.Deserialize<FhirMedicationRequestDto>(resourceJson)!),
                    _ => new FhirImportResponseDto { Warnings = new List<string> { $"Unsupported resource type: {resourceType}" } }
                };

                if (importResult.Success) imported++;
                response.Errors.AddRange(importResult.Errors);
                response.Warnings.AddRange(importResult.Warnings);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in FHIR bundle entry");
                response.Errors.Add($"Invalid JSON format in entry: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing FHIR bundle entry");
                response.Errors.Add($"Failed to import entry: {ex.Message}");
            }
        }

        response.Success = imported > 0;
        if (imported > 0)
        {
            response.Warnings.Add($"Successfully imported {imported} of {bundle.Entry.Count} resources");
        }

        return response;
    }

    public async Task<FhirBundleDto> ProcessTransactionBundleAsync(int branchId, FhirBundleDto bundle)
    {
        var responseBundle = new FhirBundleDto
        {
            Id = Guid.NewGuid().ToString(),
            Type = "transaction-response",
            Timestamp = DateTime.UtcNow,
            Entry = new List<FhirBundleEntryDto>()
        };

        if (bundle.Type != "transaction" && bundle.Type != "batch")
        {
            throw new ArgumentException("Bundle must be of type 'transaction' or 'batch'");
        }

        // For transactions, we'd ideally use a database transaction
        var importResult = await ImportBundleAsync(branchId, bundle);

        responseBundle.Entry.Add(new FhirBundleEntryDto
        {
            Response = new FhirBundleEntryResponseDto
            {
                Status = importResult.Success ? "200 OK" : "400 Bad Request",
                Outcome = new
                {
                    ResourceType = "OperationOutcome",
                    Issue = importResult.Errors.Select(e => new { Severity = "error", Code = "processing", Diagnostics = e })
                        .Concat(importResult.Warnings.Select(w => new { Severity = "warning", Code = "informational", Diagnostics = w }))
                }
            }
        });

        return responseBundle;
    }

    #endregion

    #region Validation

    public Task<FhirValidationResultDto> ValidateResourceAsync(string resourceJson, string resourceType)
    {
        var result = new FhirValidationResultDto { IsValid = true };

        try
        {
            var doc = JsonDocument.Parse(resourceJson);
            var root = doc.RootElement;

            // Check resourceType
            if (!root.TryGetProperty("resourceType", out var rt) || rt.GetString() != resourceType)
            {
                result.IsValid = false;
                result.Issues.Add(new FhirValidationIssueDto
                {
                    Severity = "error",
                    Code = "invalid",
                    Message = $"ResourceType must be '{resourceType}'"
                });
            }

            // Resource-specific validation
            switch (resourceType)
            {
                case "Patient":
                    ValidatePatientResource(root, result);
                    break;
                case "Encounter":
                    ValidateEncounterResource(root, result);
                    break;
                case "Observation":
                    ValidateObservationResource(root, result);
                    break;
            }
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "invalid",
                Message = $"Invalid JSON: {ex.Message}"
            });
        }

        return Task.FromResult(result);
    }

    public async Task<FhirValidationResultDto> ValidateBundleAsync(string bundleJson)
    {
        var result = await ValidateResourceAsync(bundleJson, "Bundle");

        try
        {
            var bundle = JsonSerializer.Deserialize<FhirBundleDto>(bundleJson);
            if (bundle?.Entry != null)
            {
                foreach (var entry in bundle.Entry)
                {
                    if (entry.Resource != null)
                    {
                        var resourceJson = JsonSerializer.Serialize(entry.Resource);
                        var resourceType = GetResourceType(resourceJson);
                        var entryResult = await ValidateResourceAsync(resourceJson, resourceType);

                        foreach (var issue in entryResult.Issues)
                        {
                            issue.Location = $"Bundle.entry[{bundle.Entry.IndexOf(entry)}]";
                            result.Issues.Add(issue);
                        }

                        if (!entryResult.IsValid)
                            result.IsValid = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "exception",
                Message = ex.Message
            });
        }

        return result;
    }

    #endregion

    #region Generic Operations

    public async Task<object> ExportResourceAsync(FhirExportRequestDto request)
    {
        if (!request.ResourceId.HasValue)
            throw new ArgumentException("ResourceId is required");

        return request.ResourceType switch
        {
            "Patient" => await ExportPatientAsync(request.ResourceId.Value),
            "Practitioner" => await ExportPractitionerAsync(request.ResourceId.Value),
            "Encounter" => await ExportEncounterAsync(request.ResourceId.Value),
            "Condition" => await ExportConditionAsync(request.ResourceId.Value),
            "Procedure" => await ExportProcedureAsync(request.ResourceId.Value),
            "MedicationRequest" => await ExportMedicationRequestAsync(request.ResourceId.Value),
            _ => throw new ArgumentException($"Unsupported resource type: {request.ResourceType}")
        };
    }

    public async Task<FhirImportResponseDto> ImportResourceAsync(int branchId, FhirImportRequestDto request)
    {
        return request.ResourceType switch
        {
            "Patient" => await ImportPatientAsync(branchId, JsonSerializer.Deserialize<FhirPatientDto>(request.Content)!),
            "Practitioner" => await ImportPractitionerAsync(branchId, JsonSerializer.Deserialize<FhirPractitionerDto>(request.Content)!),
            "Encounter" => await ImportEncounterAsync(branchId, JsonSerializer.Deserialize<FhirEncounterDto>(request.Content)!),
            "Observation" => await ImportObservationAsync(branchId, JsonSerializer.Deserialize<FhirObservationDto>(request.Content)!),
            "Condition" => await ImportConditionAsync(branchId, JsonSerializer.Deserialize<FhirConditionDto>(request.Content)!),
            "Procedure" => await ImportProcedureAsync(branchId, JsonSerializer.Deserialize<FhirProcedureDto>(request.Content)!),
            "MedicationRequest" => await ImportMedicationRequestAsync(branchId, JsonSerializer.Deserialize<FhirMedicationRequestDto>(request.Content)!),
            "Bundle" => await ImportBundleAsync(branchId, JsonSerializer.Deserialize<FhirBundleDto>(request.Content)!),
            _ => new FhirImportResponseDto { Errors = new List<string> { $"Unsupported resource type: {request.ResourceType}" } }
        };
    }

    public Task<object> GetCapabilityStatementAsync()
    {
        var capability = new
        {
            resourceType = "CapabilityStatement",
            id = "xenonclinic-capability",
            status = "active",
            date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            kind = "instance",
            software = new { name = "XenonClinic", version = "1.0.0" },
            fhirVersion = FhirVersion,
            format = new[] { "json", "xml" },
            rest = new[]
            {
                new
                {
                    mode = "server",
                    resource = new object[]
                    {
                        new { type = "Patient", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "Practitioner", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "Encounter", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "Observation", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "Condition", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "Procedure", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } },
                        new { type = "MedicationRequest", interaction = new[] { new { code = "read" }, new { code = "search-type" }, new { code = "create" } } }
                    }
                }
            }
        };

        return Task.FromResult<object>(capability);
    }

    #endregion

    #region Private Mapping Methods

    private FhirPatientDto MapPatientToFhir(Patient patient)
    {
        return new FhirPatientDto
        {
            Id = patient.Id.ToString(),
            Meta = new FhirMetaDto
            {
                VersionId = "1",
                LastUpdated = patient.UpdatedAt ?? patient.CreatedAt,
                Source = SystemUrl
            },
            Identifier = new List<FhirIdentifierDto>
            {
                new()
                {
                    Use = "official",
                    Type = new FhirCodeableConceptDto
                    {
                        Coding = new List<FhirCodingDto>
                        {
                            new() { System = "http://terminology.hl7.org/CodeSystem/v2-0203", Code = "MR", Display = "Medical Record Number" }
                        }
                    },
                    System = $"{SystemUrl}/patient-mrn",
                    Value = patient.MRN ?? patient.Id.ToString()
                }
            },
            Active = patient.IsActive,
            Name = new List<FhirHumanNameDto>
            {
                new()
                {
                    Use = "official",
                    Family = patient.LastName,
                    Given = new List<string> { patient.FirstName },
                    Text = $"{patient.FirstName} {patient.LastName}"
                }
            },
            Telecom = new List<FhirContactPointDto>(),
            Gender = MapGenderToFhir(patient.Gender),
            BirthDate = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            Address = new List<FhirAddressDto>()
        };
    }

    private FhirPractitionerDto MapDoctorToFhir(Doctor doctor)
    {
        return new FhirPractitionerDto
        {
            Id = doctor.Id.ToString(),
            Meta = new FhirMetaDto
            {
                VersionId = "1",
                LastUpdated = doctor.UpdatedAt ?? doctor.CreatedAt,
                Source = SystemUrl
            },
            Active = doctor.IsActive,
            Name = new List<FhirHumanNameDto>
            {
                new()
                {
                    Use = "official",
                    Family = doctor.LastName,
                    Given = new List<string> { doctor.FirstName },
                    Prefix = new List<string> { "Dr." }
                }
            },
            Telecom = new List<FhirContactPointDto>(),
            Gender = MapGenderToFhir(doctor.Gender),
            Qualification = !string.IsNullOrEmpty(doctor.Qualification)
                ? new List<FhirPractitionerQualificationDto>
                {
                    new() { Code = new FhirCodeableConceptDto { Text = doctor.Qualification } }
                }
                : null
        };
    }

    private FhirEncounterDto MapVisitToFhirEncounter(ClinicalVisit visit)
    {
        return new FhirEncounterDto
        {
            Id = visit.Id.ToString(),
            Meta = new FhirMetaDto
            {
                VersionId = "1",
                LastUpdated = visit.UpdatedAt ?? visit.CreatedAt,
                Source = SystemUrl
            },
            Status = MapVisitStatusToFhir(visit.Status),
            Class = new FhirCodingDto
            {
                System = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                Code = "AMB",
                Display = "ambulatory"
            },
            Subject = new FhirReferenceDto
            {
                Reference = $"Patient/{visit.PatientId}",
                Display = visit.Patient != null ? $"{visit.Patient.FirstName} {visit.Patient.LastName}" : null
            },
            Participant = visit.DoctorId.HasValue
                ? new List<FhirEncounterParticipantDto>
                {
                    new()
                    {
                        Individual = new FhirReferenceDto
                        {
                            Reference = $"Practitioner/{visit.DoctorId}",
                            Display = visit.Doctor != null ? $"Dr. {visit.Doctor.FirstName} {visit.Doctor.LastName}" : null
                        }
                    }
                }
                : null,
            Period = new FhirPeriodDto
            {
                Start = visit.VisitDate,
                End = visit.EndTime
            },
            ReasonCode = !string.IsNullOrEmpty(visit.ChiefComplaint)
                ? new List<FhirCodeableConceptDto>
                {
                    new() { Text = visit.ChiefComplaint }
                }
                : null
        };
    }

    private FhirObservationDto MapVitalSignToFhirObservation(VitalSign vital)
    {
        var observation = new FhirObservationDto
        {
            Id = $"vital-{vital.Id}",
            Meta = new FhirMetaDto { LastUpdated = vital.RecordedAt, Source = SystemUrl },
            Status = "final",
            Category = new List<FhirCodeableConceptDto>
            {
                new()
                {
                    Coding = new List<FhirCodingDto>
                    {
                        new() { System = "http://terminology.hl7.org/CodeSystem/observation-category", Code = "vital-signs", Display = "Vital Signs" }
                    }
                }
            },
            Subject = new FhirReferenceDto { Reference = $"Patient/{vital.PatientId}" },
            EffectiveDateTime = vital.RecordedAt,
            Component = new List<FhirObservationComponentDto>()
        };

        // Add components for each vital sign measurement
        if (vital.SystolicBP.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("8480-6", "Systolic blood pressure", vital.SystolicBP.Value, "mm[Hg]"));
        }
        if (vital.DiastolicBP.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("8462-4", "Diastolic blood pressure", vital.DiastolicBP.Value, "mm[Hg]"));
        }
        if (vital.HeartRate.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("8867-4", "Heart rate", vital.HeartRate.Value, "/min"));
        }
        if (vital.Temperature.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("8310-5", "Body temperature", vital.Temperature.Value, "Cel"));
        }
        if (vital.RespiratoryRate.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("9279-1", "Respiratory rate", vital.RespiratoryRate.Value, "/min"));
        }
        if (vital.OxygenSaturation.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("2708-6", "Oxygen saturation", vital.OxygenSaturation.Value, "%"));
        }
        if (vital.Weight.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("29463-7", "Body weight", vital.Weight.Value, "kg"));
        }
        if (vital.Height.HasValue)
        {
            observation.Component.Add(CreateVitalComponent("8302-2", "Body height", vital.Height.Value, "cm"));
        }

        // Set primary code as vital signs panel
        observation.Code = new FhirCodeableConceptDto
        {
            Coding = new List<FhirCodingDto>
            {
                new() { System = "http://loinc.org", Code = "85353-1", Display = "Vital signs, weight, height, head circumference, oxygen saturation and BMI panel" }
            }
        };

        return observation;
    }

    private FhirObservationComponentDto CreateVitalComponent(string loincCode, string display, decimal value, string unit)
    {
        return new FhirObservationComponentDto
        {
            Code = new FhirCodeableConceptDto
            {
                Coding = new List<FhirCodingDto>
                {
                    new() { System = "http://loinc.org", Code = loincCode, Display = display }
                }
            },
            ValueQuantity = new FhirQuantityDto
            {
                Value = value,
                Unit = unit,
                System = "http://unitsofmeasure.org",
                Code = unit
            }
        };
    }

    private FhirObservationDto MapLabResultToFhirObservation(LabResult result)
    {
        return new FhirObservationDto
        {
            Id = $"lab-{result.Id}",
            Meta = new FhirMetaDto { LastUpdated = result.ResultDate, Source = SystemUrl },
            Status = MapLabStatusToFhir(result.Status.ToString()),
            Category = new List<FhirCodeableConceptDto>
            {
                new()
                {
                    Coding = new List<FhirCodingDto>
                    {
                        new() { System = "http://terminology.hl7.org/CodeSystem/observation-category", Code = "laboratory", Display = "Laboratory" }
                    }
                }
            },
            Code = new FhirCodeableConceptDto
            {
                Coding = new List<FhirCodingDto>
                {
                    new() { System = "http://loinc.org", Code = result.LabTest?.LoincCode, Display = result.LabTest?.Name }
                },
                Text = result.LabTest?.Name
            },
            Subject = new FhirReferenceDto { Reference = $"Patient/{result.PatientId}" },
            EffectiveDateTime = result.ResultDate,
            ValueString = result.Result,
            ReferenceRange = !string.IsNullOrEmpty(result.ReferenceRange)
                ? new List<FhirObservationReferenceRangeDto>
                {
                    new() { Text = result.ReferenceRange }
                }
                : null,
            Interpretation = !string.IsNullOrEmpty(result.Interpretation)
                ? new List<FhirCodeableConceptDto>
                {
                    new() { Text = result.Interpretation }
                }
                : null
        };
    }

    private FhirConditionDto MapDiagnosisToFhirCondition(Diagnosis diagnosis)
    {
        return new FhirConditionDto
        {
            Id = diagnosis.Id.ToString(),
            Meta = new FhirMetaDto { LastUpdated = diagnosis.UpdatedAt ?? diagnosis.CreatedAt, Source = SystemUrl },
            ClinicalStatus = new FhirCodeableConceptDto
            {
                Coding = new List<FhirCodingDto>
                {
                    new()
                    {
                        System = "http://terminology.hl7.org/CodeSystem/condition-clinical",
                        Code = diagnosis.IsActive ? "active" : "resolved"
                    }
                }
            },
            VerificationStatus = new FhirCodeableConceptDto
            {
                Coding = new List<FhirCodingDto>
                {
                    new() { System = "http://terminology.hl7.org/CodeSystem/condition-ver-status", Code = "confirmed" }
                }
            },
            Code = new FhirCodeableConceptDto
            {
                Coding = !string.IsNullOrEmpty(diagnosis.ICD10Code)
                    ? new List<FhirCodingDto>
                    {
                        new() { System = "http://hl7.org/fhir/sid/icd-10", Code = diagnosis.ICD10Code, Display = diagnosis.Description }
                    }
                    : null,
                Text = diagnosis.Description
            },
            Subject = new FhirReferenceDto { Reference = $"Patient/{diagnosis.PatientId}" },
            Encounter = diagnosis.ClinicalVisitId.HasValue
                ? new FhirReferenceDto { Reference = $"Encounter/{diagnosis.ClinicalVisitId}" }
                : null,
            OnsetDateTime = diagnosis.DiagnosisDate,
            RecordedDate = diagnosis.CreatedAt
        };
    }

    private FhirProcedureDto MapProcedureToFhir(Procedure procedure)
    {
        return new FhirProcedureDto
        {
            Id = procedure.Id.ToString(),
            Meta = new FhirMetaDto { LastUpdated = procedure.UpdatedAt ?? procedure.CreatedAt, Source = SystemUrl },
            Status = MapProcedureStatusToFhir(procedure.Status),
            Code = new FhirCodeableConceptDto
            {
                Coding = !string.IsNullOrEmpty(procedure.CPTCode)
                    ? new List<FhirCodingDto>
                    {
                        new() { System = "http://www.ama-assn.org/go/cpt", Code = procedure.CPTCode, Display = procedure.Name }
                    }
                    : null,
                Text = procedure.Name
            },
            Subject = new FhirReferenceDto { Reference = $"Patient/{procedure.PatientId}" },
            Encounter = procedure.ClinicalVisitId.HasValue
                ? new FhirReferenceDto { Reference = $"Encounter/{procedure.ClinicalVisitId}" }
                : null,
            PerformedDateTime = procedure.ProcedureDate,
            Performer = procedure.DoctorId.HasValue
                ? new List<FhirProcedurePerformerDto>
                {
                    new() { Actor = new FhirReferenceDto { Reference = $"Practitioner/{procedure.DoctorId}" } }
                }
                : null,
            Note = !string.IsNullOrEmpty(procedure.Notes)
                ? new List<FhirAnnotationDto>
                {
                    new() { Text = procedure.Notes, Time = procedure.CreatedAt }
                }
                : null
        };
    }

    private FhirMedicationRequestDto MapPrescriptionToFhirMedicationRequest(Prescription prescription)
    {
        var request = new FhirMedicationRequestDto
        {
            Id = prescription.Id.ToString(),
            Meta = new FhirMetaDto { LastUpdated = prescription.UpdatedAt ?? prescription.CreatedAt, Source = SystemUrl },
            Status = MapPrescriptionStatusToFhir(prescription.Status),
            Intent = "order",
            Subject = new FhirReferenceDto { Reference = $"Patient/{prescription.PatientId}" },
            Encounter = prescription.ClinicalVisitId.HasValue
                ? new FhirReferenceDto { Reference = $"Encounter/{prescription.ClinicalVisitId}" }
                : null,
            AuthoredOn = prescription.PrescriptionDate,
            Requester = prescription.DoctorId.HasValue
                ? new FhirReferenceDto { Reference = $"Practitioner/{prescription.DoctorId}" }
                : null
        };

        // Add first medication item
        var firstItem = prescription.Items?.FirstOrDefault();
        if (firstItem != null)
        {
            request.MedicationCodeableConcept = new FhirCodeableConceptDto
            {
                Text = firstItem.MedicationName
            };

            request.DosageInstruction = new List<FhirDosageDto>
            {
                new()
                {
                    Text = firstItem.Dosage,
                    PatientInstruction = firstItem.Instructions
                }
            };

            if (firstItem.Quantity.HasValue)
            {
                request.DispenseRequest = new FhirMedicationRequestDispenseRequestDto
                {
                    Quantity = new FhirQuantityDto { Value = firstItem.Quantity.Value }
                };
            }
        }

        return request;
    }

    #endregion

    #region Private Helper Methods

    private static FhirBundleDto CreateSearchBundle(string resourceType, List<object> resources, int total)
    {
        return new FhirBundleDto
        {
            Id = Guid.NewGuid().ToString(),
            Type = "searchset",
            Timestamp = DateTime.UtcNow,
            Total = total,
            Entry = resources.Select((r, i) => new FhirBundleEntryDto
            {
                FullUrl = $"{SystemUrl}/{resourceType}/{i}",
                Resource = r,
                Search = new FhirBundleEntrySearchDto { Mode = "match" }
            }).ToList()
        };
    }

    private static int? ExtractIdFromReference(string? reference)
    {
        if (string.IsNullOrEmpty(reference)) return null;

        var parts = reference.Split('/');
        if (parts.Length >= 2 && int.TryParse(parts.Last(), out var id))
            return id;

        return null;
    }

    private static string MapGenderToFhir(string? gender)
    {
        return gender?.ToLower() switch
        {
            "male" or "m" => "male",
            "female" or "f" => "female",
            "other" or "o" => "other",
            _ => "unknown"
        };
    }

    private static string MapFhirGender(string? fhirGender)
    {
        return fhirGender?.ToLower() switch
        {
            "male" => "Male",
            "female" => "Female",
            "other" => "Other",
            _ => "Unknown"
        };
    }

    private static string MapVisitStatusToFhir(string? status)
    {
        return status?.ToLower() switch
        {
            "scheduled" or "planned" => "planned",
            "checkedin" or "arrived" => "arrived",
            "inprogress" or "started" => "in-progress",
            "completed" or "finished" => "finished",
            "cancelled" or "canceled" => "cancelled",
            "noshow" => "cancelled",
            _ => "unknown"
        };
    }

    private static string MapFhirEncounterStatus(string status)
    {
        return status.ToLower() switch
        {
            "planned" => "Scheduled",
            "arrived" => "CheckedIn",
            "in-progress" => "InProgress",
            "finished" => "Completed",
            "cancelled" => "Cancelled",
            _ => "Unknown"
        };
    }

    private static string MapLabStatusToFhir(string? status)
    {
        return status?.ToLower() switch
        {
            "pending" => "registered",
            "preliminary" => "preliminary",
            "final" or "completed" => "final",
            "amended" => "amended",
            "cancelled" => "cancelled",
            _ => "unknown"
        };
    }

    private static string MapProcedureStatusToFhir(string? status)
    {
        return status?.ToLower() switch
        {
            "planned" or "scheduled" => "preparation",
            "inprogress" => "in-progress",
            "completed" => "completed",
            "cancelled" => "stopped",
            _ => "unknown"
        };
    }

    private static string MapFhirProcedureStatus(string status)
    {
        return status.ToLower() switch
        {
            "preparation" => "Planned",
            "in-progress" => "InProgress",
            "completed" => "Completed",
            "stopped" or "not-done" => "Cancelled",
            _ => "Unknown"
        };
    }

    private static string MapPrescriptionStatusToFhir(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => "active",
            "onhold" => "on-hold",
            "completed" => "completed",
            "cancelled" => "cancelled",
            "stopped" => "stopped",
            "draft" => "draft",
            _ => "unknown"
        };
    }

    private static string MapFhirMedicationRequestStatus(string status)
    {
        return status.ToLower() switch
        {
            "active" => "Active",
            "on-hold" => "OnHold",
            "completed" => "Completed",
            "cancelled" => "Cancelled",
            "stopped" => "Stopped",
            "draft" => "Draft",
            _ => "Unknown"
        };
    }

    private static void MapLoincToVitalSign(VitalSign vital, string? loincCode, decimal value)
    {
        switch (loincCode)
        {
            case "8480-6": vital.SystolicBP = (int)value; break;
            case "8462-4": vital.DiastolicBP = (int)value; break;
            case "8867-4": vital.HeartRate = (int)value; break;
            case "8310-5": vital.Temperature = value; break;
            case "9279-1": vital.RespiratoryRate = (int)value; break;
            case "2708-6": vital.OxygenSaturation = value; break;
            case "29463-7": vital.Weight = value; break;
            case "8302-2": vital.Height = value; break;
        }
    }

    private static string GetResourceType(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("resourceType", out var rt))
                return rt.GetString() ?? "Unknown";
        }
        catch (JsonException)
        {
            // Invalid JSON format - return default value
        }
        return "Unknown";
    }

    private static void ValidatePatientResource(JsonElement root, FhirValidationResultDto result)
    {
        if (!root.TryGetProperty("name", out _))
        {
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "warning",
                Code = "required",
                Message = "Patient.name is recommended"
            });
        }
    }

    private static void ValidateEncounterResource(JsonElement root, FhirValidationResultDto result)
    {
        if (!root.TryGetProperty("status", out _))
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "required",
                Message = "Encounter.status is required"
            });
        }

        if (!root.TryGetProperty("class", out _))
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "required",
                Message = "Encounter.class is required"
            });
        }
    }

    private static void ValidateObservationResource(JsonElement root, FhirValidationResultDto result)
    {
        if (!root.TryGetProperty("status", out _))
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "required",
                Message = "Observation.status is required"
            });
        }

        if (!root.TryGetProperty("code", out _))
        {
            result.IsValid = false;
            result.Issues.Add(new FhirValidationIssueDto
            {
                Severity = "error",
                Code = "required",
                Message = "Observation.code is required"
            });
        }
    }

    #endregion
}
