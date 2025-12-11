namespace XenonClinic.Core.DTOs;

#region FHIR Resource Base

/// <summary>
/// Base FHIR resource DTO
/// </summary>
public abstract class FhirResourceDto
{
    public string ResourceType { get; set; } = string.Empty;
    public string? Id { get; set; }
    public FhirMetaDto? Meta { get; set; }
    public List<FhirIdentifierDto>? Identifier { get; set; }
}

/// <summary>
/// FHIR Meta element
/// </summary>
public class FhirMetaDto
{
    public string? VersionId { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Source { get; set; }
    public List<string>? Profile { get; set; }
    public List<FhirCodingDto>? Security { get; set; }
    public List<FhirCodingDto>? Tag { get; set; }
}

/// <summary>
/// FHIR Identifier element
/// </summary>
public class FhirIdentifierDto
{
    public string? Use { get; set; } // usual, official, temp, secondary, old
    public FhirCodeableConceptDto? Type { get; set; }
    public string? System { get; set; }
    public string? Value { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

/// <summary>
/// FHIR Coding element
/// </summary>
public class FhirCodingDto
{
    public string? System { get; set; }
    public string? Version { get; set; }
    public string? Code { get; set; }
    public string? Display { get; set; }
    public bool? UserSelected { get; set; }
}

/// <summary>
/// FHIR CodeableConcept element
/// </summary>
public class FhirCodeableConceptDto
{
    public List<FhirCodingDto>? Coding { get; set; }
    public string? Text { get; set; }
}

/// <summary>
/// FHIR Period element
/// </summary>
public class FhirPeriodDto
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

/// <summary>
/// FHIR Reference element
/// </summary>
public class FhirReferenceDto
{
    public string? Reference { get; set; }
    public string? Type { get; set; }
    public FhirIdentifierDto? Identifier { get; set; }
    public string? Display { get; set; }
}

/// <summary>
/// FHIR HumanName element
/// </summary>
public class FhirHumanNameDto
{
    public string? Use { get; set; } // usual, official, temp, nickname, anonymous, old, maiden
    public string? Text { get; set; }
    public string? Family { get; set; }
    public List<string>? Given { get; set; }
    public List<string>? Prefix { get; set; }
    public List<string>? Suffix { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

/// <summary>
/// FHIR Address element
/// </summary>
public class FhirAddressDto
{
    public string? Use { get; set; } // home, work, temp, old, billing
    public string? Type { get; set; } // postal, physical, both
    public string? Text { get; set; }
    public List<string>? Line { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

/// <summary>
/// FHIR ContactPoint element (phone, email, etc.)
/// </summary>
public class FhirContactPointDto
{
    public string? System { get; set; } // phone, fax, email, pager, url, sms, other
    public string? Value { get; set; }
    public string? Use { get; set; } // home, work, temp, old, mobile
    public int? Rank { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

/// <summary>
/// FHIR Quantity element
/// </summary>
public class FhirQuantityDto
{
    public decimal? Value { get; set; }
    public string? Comparator { get; set; } // <, <=, >=, >
    public string? Unit { get; set; }
    public string? System { get; set; }
    public string? Code { get; set; }
}

/// <summary>
/// FHIR Annotation element
/// </summary>
public class FhirAnnotationDto
{
    public FhirReferenceDto? AuthorReference { get; set; }
    public string? AuthorString { get; set; }
    public DateTime? Time { get; set; }
    public string Text { get; set; } = string.Empty;
}

#endregion

#region FHIR Patient Resource

/// <summary>
/// FHIR Patient resource DTO
/// </summary>
public class FhirPatientDto : FhirResourceDto
{
    public FhirPatientDto() { ResourceType = "Patient"; }

    public bool? Active { get; set; }
    public List<FhirHumanNameDto>? Name { get; set; }
    public List<FhirContactPointDto>? Telecom { get; set; }
    public string? Gender { get; set; } // male, female, other, unknown
    public string? BirthDate { get; set; }
    public bool? DeceasedBoolean { get; set; }
    public DateTime? DeceasedDateTime { get; set; }
    public List<FhirAddressDto>? Address { get; set; }
    public FhirCodeableConceptDto? MaritalStatus { get; set; }
    public bool? MultipleBirthBoolean { get; set; }
    public int? MultipleBirthInteger { get; set; }
    public List<FhirAttachmentDto>? Photo { get; set; }
    public List<FhirPatientContactDto>? Contact { get; set; }
    public List<FhirPatientCommunicationDto>? Communication { get; set; }
    public List<FhirReferenceDto>? GeneralPractitioner { get; set; }
    public FhirReferenceDto? ManagingOrganization { get; set; }
    public List<FhirPatientLinkDto>? Link { get; set; }
}

/// <summary>
/// FHIR Attachment element
/// </summary>
public class FhirAttachmentDto
{
    public string? ContentType { get; set; }
    public string? Language { get; set; }
    public string? Data { get; set; } // base64
    public string? Url { get; set; }
    public int? Size { get; set; }
    public string? Hash { get; set; } // base64
    public string? Title { get; set; }
    public DateTime? Creation { get; set; }
}

/// <summary>
/// FHIR Patient Contact element
/// </summary>
public class FhirPatientContactDto
{
    public List<FhirCodeableConceptDto>? Relationship { get; set; }
    public FhirHumanNameDto? Name { get; set; }
    public List<FhirContactPointDto>? Telecom { get; set; }
    public FhirAddressDto? Address { get; set; }
    public string? Gender { get; set; }
    public FhirReferenceDto? Organization { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

/// <summary>
/// FHIR Patient Communication element
/// </summary>
public class FhirPatientCommunicationDto
{
    public FhirCodeableConceptDto Language { get; set; } = new();
    public bool? Preferred { get; set; }
}

/// <summary>
/// FHIR Patient Link element
/// </summary>
public class FhirPatientLinkDto
{
    public FhirReferenceDto Other { get; set; } = new();
    public string Type { get; set; } = string.Empty; // replaced-by, replaces, refer, seealso
}

#endregion

#region FHIR Practitioner Resource

/// <summary>
/// FHIR Practitioner resource DTO
/// </summary>
public class FhirPractitionerDto : FhirResourceDto
{
    public FhirPractitionerDto() { ResourceType = "Practitioner"; }

    public bool? Active { get; set; }
    public List<FhirHumanNameDto>? Name { get; set; }
    public List<FhirContactPointDto>? Telecom { get; set; }
    public List<FhirAddressDto>? Address { get; set; }
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
    public List<FhirAttachmentDto>? Photo { get; set; }
    public List<FhirPractitionerQualificationDto>? Qualification { get; set; }
    public List<FhirCodeableConceptDto>? Communication { get; set; }
}

/// <summary>
/// FHIR Practitioner Qualification element
/// </summary>
public class FhirPractitionerQualificationDto
{
    public List<FhirIdentifierDto>? Identifier { get; set; }
    public FhirCodeableConceptDto Code { get; set; } = new();
    public FhirPeriodDto? Period { get; set; }
    public FhirReferenceDto? Issuer { get; set; }
}

#endregion

#region FHIR Encounter Resource

/// <summary>
/// FHIR Encounter resource DTO
/// </summary>
public class FhirEncounterDto : FhirResourceDto
{
    public FhirEncounterDto() { ResourceType = "Encounter"; }

    public string Status { get; set; } = string.Empty; // planned, arrived, triaged, in-progress, onleave, finished, cancelled, entered-in-error, unknown
    public List<FhirEncounterStatusHistoryDto>? StatusHistory { get; set; }
    public FhirCodingDto? Class { get; set; }
    public List<FhirEncounterClassHistoryDto>? ClassHistory { get; set; }
    public List<FhirCodeableConceptDto>? Type { get; set; }
    public FhirCodeableConceptDto? ServiceType { get; set; }
    public FhirCodeableConceptDto? Priority { get; set; }
    public FhirReferenceDto? Subject { get; set; }
    public List<FhirReferenceDto>? EpisodeOfCare { get; set; }
    public List<FhirReferenceDto>? BasedOn { get; set; }
    public List<FhirEncounterParticipantDto>? Participant { get; set; }
    public List<FhirReferenceDto>? Appointment { get; set; }
    public FhirPeriodDto? Period { get; set; }
    public string? Length { get; set; }
    public List<FhirCodeableConceptDto>? ReasonCode { get; set; }
    public List<FhirReferenceDto>? ReasonReference { get; set; }
    public List<FhirEncounterDiagnosisDto>? Diagnosis { get; set; }
    public List<FhirReferenceDto>? Account { get; set; }
    public FhirEncounterHospitalizationDto? Hospitalization { get; set; }
    public List<FhirEncounterLocationDto>? Location { get; set; }
    public FhirReferenceDto? ServiceProvider { get; set; }
    public FhirReferenceDto? PartOf { get; set; }
}

/// <summary>
/// FHIR Encounter StatusHistory element
/// </summary>
public class FhirEncounterStatusHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public FhirPeriodDto Period { get; set; } = new();
}

/// <summary>
/// FHIR Encounter ClassHistory element
/// </summary>
public class FhirEncounterClassHistoryDto
{
    public FhirCodingDto Class { get; set; } = new();
    public FhirPeriodDto Period { get; set; } = new();
}

/// <summary>
/// FHIR Encounter Participant element
/// </summary>
public class FhirEncounterParticipantDto
{
    public List<FhirCodeableConceptDto>? Type { get; set; }
    public FhirPeriodDto? Period { get; set; }
    public FhirReferenceDto? Individual { get; set; }
}

/// <summary>
/// FHIR Encounter Diagnosis element
/// </summary>
public class FhirEncounterDiagnosisDto
{
    public FhirReferenceDto Condition { get; set; } = new();
    public FhirCodeableConceptDto? Use { get; set; }
    public int? Rank { get; set; }
}

/// <summary>
/// FHIR Encounter Hospitalization element
/// </summary>
public class FhirEncounterHospitalizationDto
{
    public FhirIdentifierDto? PreAdmissionIdentifier { get; set; }
    public FhirReferenceDto? Origin { get; set; }
    public FhirCodeableConceptDto? AdmitSource { get; set; }
    public FhirCodeableConceptDto? ReAdmission { get; set; }
    public List<FhirCodeableConceptDto>? DietPreference { get; set; }
    public List<FhirCodeableConceptDto>? SpecialCourtesy { get; set; }
    public List<FhirCodeableConceptDto>? SpecialArrangement { get; set; }
    public FhirReferenceDto? Destination { get; set; }
    public FhirCodeableConceptDto? DischargeDisposition { get; set; }
}

/// <summary>
/// FHIR Encounter Location element
/// </summary>
public class FhirEncounterLocationDto
{
    public FhirReferenceDto Location { get; set; } = new();
    public string? Status { get; set; } // planned, active, reserved, completed
    public FhirCodeableConceptDto? PhysicalType { get; set; }
    public FhirPeriodDto? Period { get; set; }
}

#endregion

#region FHIR Observation Resource

/// <summary>
/// FHIR Observation resource DTO (vital signs, lab results, etc.)
/// </summary>
public class FhirObservationDto : FhirResourceDto
{
    public FhirObservationDto() { ResourceType = "Observation"; }

    public List<FhirReferenceDto>? BasedOn { get; set; }
    public List<FhirReferenceDto>? PartOf { get; set; }
    public string Status { get; set; } = string.Empty; // registered, preliminary, final, amended, corrected, cancelled, entered-in-error, unknown
    public List<FhirCodeableConceptDto>? Category { get; set; }
    public FhirCodeableConceptDto Code { get; set; } = new();
    public FhirReferenceDto? Subject { get; set; }
    public List<FhirReferenceDto>? Focus { get; set; }
    public FhirReferenceDto? Encounter { get; set; }
    public DateTime? EffectiveDateTime { get; set; }
    public FhirPeriodDto? EffectivePeriod { get; set; }
    public DateTime? Issued { get; set; }
    public List<FhirReferenceDto>? Performer { get; set; }

    // Value[x] - one of these
    public FhirQuantityDto? ValueQuantity { get; set; }
    public FhirCodeableConceptDto? ValueCodeableConcept { get; set; }
    public string? ValueString { get; set; }
    public bool? ValueBoolean { get; set; }
    public int? ValueInteger { get; set; }
    public FhirRangeDto? ValueRange { get; set; }
    public FhirRatioDto? ValueRatio { get; set; }
    public DateTime? ValueDateTime { get; set; }
    public FhirPeriodDto? ValuePeriod { get; set; }

    public FhirCodeableConceptDto? DataAbsentReason { get; set; }
    public List<FhirCodeableConceptDto>? Interpretation { get; set; }
    public List<FhirAnnotationDto>? Note { get; set; }
    public FhirCodeableConceptDto? BodySite { get; set; }
    public FhirCodeableConceptDto? Method { get; set; }
    public FhirReferenceDto? Specimen { get; set; }
    public FhirReferenceDto? Device { get; set; }
    public List<FhirObservationReferenceRangeDto>? ReferenceRange { get; set; }
    public List<FhirReferenceDto>? HasMember { get; set; }
    public List<FhirReferenceDto>? DerivedFrom { get; set; }
    public List<FhirObservationComponentDto>? Component { get; set; }
}

/// <summary>
/// FHIR Range element
/// </summary>
public class FhirRangeDto
{
    public FhirQuantityDto? Low { get; set; }
    public FhirQuantityDto? High { get; set; }
}

/// <summary>
/// FHIR Ratio element
/// </summary>
public class FhirRatioDto
{
    public FhirQuantityDto? Numerator { get; set; }
    public FhirQuantityDto? Denominator { get; set; }
}

/// <summary>
/// FHIR Observation ReferenceRange element
/// </summary>
public class FhirObservationReferenceRangeDto
{
    public FhirQuantityDto? Low { get; set; }
    public FhirQuantityDto? High { get; set; }
    public FhirCodeableConceptDto? Type { get; set; }
    public List<FhirCodeableConceptDto>? AppliesTo { get; set; }
    public FhirRangeDto? Age { get; set; }
    public string? Text { get; set; }
}

/// <summary>
/// FHIR Observation Component element
/// </summary>
public class FhirObservationComponentDto
{
    public FhirCodeableConceptDto Code { get; set; } = new();
    public FhirQuantityDto? ValueQuantity { get; set; }
    public FhirCodeableConceptDto? ValueCodeableConcept { get; set; }
    public string? ValueString { get; set; }
    public bool? ValueBoolean { get; set; }
    public int? ValueInteger { get; set; }
    public FhirCodeableConceptDto? DataAbsentReason { get; set; }
    public List<FhirCodeableConceptDto>? Interpretation { get; set; }
    public List<FhirObservationReferenceRangeDto>? ReferenceRange { get; set; }
}

#endregion

#region FHIR Condition Resource

/// <summary>
/// FHIR Condition resource DTO (diagnoses)
/// </summary>
public class FhirConditionDto : FhirResourceDto
{
    public FhirConditionDto() { ResourceType = "Condition"; }

    public FhirCodeableConceptDto? ClinicalStatus { get; set; }
    public FhirCodeableConceptDto? VerificationStatus { get; set; }
    public List<FhirCodeableConceptDto>? Category { get; set; }
    public FhirCodeableConceptDto? Severity { get; set; }
    public FhirCodeableConceptDto? Code { get; set; }
    public List<FhirCodeableConceptDto>? BodySite { get; set; }
    public FhirReferenceDto? Subject { get; set; }
    public FhirReferenceDto? Encounter { get; set; }
    public DateTime? OnsetDateTime { get; set; }
    public FhirQuantityDto? OnsetAge { get; set; }
    public FhirPeriodDto? OnsetPeriod { get; set; }
    public FhirRangeDto? OnsetRange { get; set; }
    public string? OnsetString { get; set; }
    public DateTime? AbatementDateTime { get; set; }
    public FhirQuantityDto? AbatementAge { get; set; }
    public FhirPeriodDto? AbatementPeriod { get; set; }
    public FhirRangeDto? AbatementRange { get; set; }
    public string? AbatementString { get; set; }
    public DateTime? RecordedDate { get; set; }
    public FhirReferenceDto? Recorder { get; set; }
    public FhirReferenceDto? Asserter { get; set; }
    public List<FhirConditionStageDto>? Stage { get; set; }
    public List<FhirConditionEvidenceDto>? Evidence { get; set; }
    public List<FhirAnnotationDto>? Note { get; set; }
}

/// <summary>
/// FHIR Condition Stage element
/// </summary>
public class FhirConditionStageDto
{
    public FhirCodeableConceptDto? Summary { get; set; }
    public List<FhirReferenceDto>? Assessment { get; set; }
    public FhirCodeableConceptDto? Type { get; set; }
}

/// <summary>
/// FHIR Condition Evidence element
/// </summary>
public class FhirConditionEvidenceDto
{
    public List<FhirCodeableConceptDto>? Code { get; set; }
    public List<FhirReferenceDto>? Detail { get; set; }
}

#endregion

#region FHIR Procedure Resource

/// <summary>
/// FHIR Procedure resource DTO
/// </summary>
public class FhirProcedureDto : FhirResourceDto
{
    public FhirProcedureDto() { ResourceType = "Procedure"; }

    public List<FhirIdentifierDto>? InstantiatesCanonical { get; set; }
    public List<string>? InstantiatesUri { get; set; }
    public List<FhirReferenceDto>? BasedOn { get; set; }
    public List<FhirReferenceDto>? PartOf { get; set; }
    public string Status { get; set; } = string.Empty; // preparation, in-progress, not-done, on-hold, stopped, completed, entered-in-error, unknown
    public FhirCodeableConceptDto? StatusReason { get; set; }
    public FhirCodeableConceptDto? Category { get; set; }
    public FhirCodeableConceptDto? Code { get; set; }
    public FhirReferenceDto? Subject { get; set; }
    public FhirReferenceDto? Encounter { get; set; }
    public DateTime? PerformedDateTime { get; set; }
    public FhirPeriodDto? PerformedPeriod { get; set; }
    public string? PerformedString { get; set; }
    public FhirQuantityDto? PerformedAge { get; set; }
    public FhirRangeDto? PerformedRange { get; set; }
    public FhirReferenceDto? Recorder { get; set; }
    public FhirReferenceDto? Asserter { get; set; }
    public List<FhirProcedurePerformerDto>? Performer { get; set; }
    public FhirReferenceDto? Location { get; set; }
    public List<FhirCodeableConceptDto>? ReasonCode { get; set; }
    public List<FhirReferenceDto>? ReasonReference { get; set; }
    public List<FhirCodeableConceptDto>? BodySite { get; set; }
    public FhirCodeableConceptDto? Outcome { get; set; }
    public List<FhirReferenceDto>? Report { get; set; }
    public List<FhirCodeableConceptDto>? Complication { get; set; }
    public List<FhirReferenceDto>? ComplicationDetail { get; set; }
    public List<FhirCodeableConceptDto>? FollowUp { get; set; }
    public List<FhirAnnotationDto>? Note { get; set; }
    public List<FhirProcedureFocalDeviceDto>? FocalDevice { get; set; }
    public List<FhirReferenceDto>? UsedReference { get; set; }
    public List<FhirCodeableConceptDto>? UsedCode { get; set; }
}

/// <summary>
/// FHIR Procedure Performer element
/// </summary>
public class FhirProcedurePerformerDto
{
    public FhirCodeableConceptDto? Function { get; set; }
    public FhirReferenceDto Actor { get; set; } = new();
    public FhirReferenceDto? OnBehalfOf { get; set; }
}

/// <summary>
/// FHIR Procedure FocalDevice element
/// </summary>
public class FhirProcedureFocalDeviceDto
{
    public FhirCodeableConceptDto? Action { get; set; }
    public FhirReferenceDto Manipulated { get; set; } = new();
}

#endregion

#region FHIR Medication Resources

/// <summary>
/// FHIR Medication resource DTO
/// </summary>
public class FhirMedicationDto : FhirResourceDto
{
    public FhirMedicationDto() { ResourceType = "Medication"; }

    public FhirCodeableConceptDto? Code { get; set; }
    public string? Status { get; set; } // active, inactive, entered-in-error
    public FhirReferenceDto? Manufacturer { get; set; }
    public FhirCodeableConceptDto? Form { get; set; }
    public FhirRatioDto? Amount { get; set; }
    public List<FhirMedicationIngredientDto>? Ingredient { get; set; }
    public FhirMedicationBatchDto? Batch { get; set; }
}

/// <summary>
/// FHIR Medication Ingredient element
/// </summary>
public class FhirMedicationIngredientDto
{
    public FhirCodeableConceptDto? ItemCodeableConcept { get; set; }
    public FhirReferenceDto? ItemReference { get; set; }
    public bool? IsActive { get; set; }
    public FhirRatioDto? Strength { get; set; }
}

/// <summary>
/// FHIR Medication Batch element
/// </summary>
public class FhirMedicationBatchDto
{
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// FHIR MedicationRequest resource DTO (prescriptions)
/// </summary>
public class FhirMedicationRequestDto : FhirResourceDto
{
    public FhirMedicationRequestDto() { ResourceType = "MedicationRequest"; }

    public string Status { get; set; } = string.Empty; // active, on-hold, cancelled, completed, entered-in-error, stopped, draft, unknown
    public FhirCodeableConceptDto? StatusReason { get; set; }
    public string Intent { get; set; } = string.Empty; // proposal, plan, order, original-order, reflex-order, filler-order, instance-order, option
    public List<FhirCodeableConceptDto>? Category { get; set; }
    public string? Priority { get; set; } // routine, urgent, asap, stat
    public bool? DoNotPerform { get; set; }
    public bool? ReportedBoolean { get; set; }
    public FhirReferenceDto? ReportedReference { get; set; }
    public FhirCodeableConceptDto? MedicationCodeableConcept { get; set; }
    public FhirReferenceDto? MedicationReference { get; set; }
    public FhirReferenceDto Subject { get; set; } = new();
    public FhirReferenceDto? Encounter { get; set; }
    public List<FhirReferenceDto>? SupportingInformation { get; set; }
    public DateTime? AuthoredOn { get; set; }
    public FhirReferenceDto? Requester { get; set; }
    public FhirReferenceDto? Performer { get; set; }
    public FhirCodeableConceptDto? PerformerType { get; set; }
    public FhirReferenceDto? Recorder { get; set; }
    public List<FhirCodeableConceptDto>? ReasonCode { get; set; }
    public List<FhirReferenceDto>? ReasonReference { get; set; }
    public List<string>? InstantiatesCanonical { get; set; }
    public List<string>? InstantiatesUri { get; set; }
    public List<FhirReferenceDto>? BasedOn { get; set; }
    public FhirIdentifierDto? GroupIdentifier { get; set; }
    public FhirCodeableConceptDto? CourseOfTherapyType { get; set; }
    public List<FhirReferenceDto>? Insurance { get; set; }
    public List<FhirAnnotationDto>? Note { get; set; }
    public List<FhirDosageDto>? DosageInstruction { get; set; }
    public FhirMedicationRequestDispenseRequestDto? DispenseRequest { get; set; }
    public FhirMedicationRequestSubstitutionDto? Substitution { get; set; }
    public FhirReferenceDto? PriorPrescription { get; set; }
    public List<FhirReferenceDto>? DetectedIssue { get; set; }
    public List<FhirReferenceDto>? EventHistory { get; set; }
}

/// <summary>
/// FHIR Dosage element
/// </summary>
public class FhirDosageDto
{
    public int? Sequence { get; set; }
    public string? Text { get; set; }
    public List<FhirCodeableConceptDto>? AdditionalInstruction { get; set; }
    public string? PatientInstruction { get; set; }
    public FhirTimingDto? Timing { get; set; }
    public bool? AsNeededBoolean { get; set; }
    public FhirCodeableConceptDto? AsNeededCodeableConcept { get; set; }
    public FhirCodeableConceptDto? Site { get; set; }
    public FhirCodeableConceptDto? Route { get; set; }
    public FhirCodeableConceptDto? Method { get; set; }
    public List<FhirDosageDoseAndRateDto>? DoseAndRate { get; set; }
    public FhirRatioDto? MaxDosePerPeriod { get; set; }
    public FhirQuantityDto? MaxDosePerAdministration { get; set; }
    public FhirQuantityDto? MaxDosePerLifetime { get; set; }
}

/// <summary>
/// FHIR Timing element
/// </summary>
public class FhirTimingDto
{
    public List<DateTime>? Event { get; set; }
    public FhirTimingRepeatDto? Repeat { get; set; }
    public FhirCodeableConceptDto? Code { get; set; }
}

/// <summary>
/// FHIR Timing Repeat element
/// </summary>
public class FhirTimingRepeatDto
{
    public FhirQuantityDto? BoundsDuration { get; set; }
    public FhirRangeDto? BoundsRange { get; set; }
    public FhirPeriodDto? BoundsPeriod { get; set; }
    public int? Count { get; set; }
    public int? CountMax { get; set; }
    public decimal? Duration { get; set; }
    public decimal? DurationMax { get; set; }
    public string? DurationUnit { get; set; } // s, min, h, d, wk, mo, a
    public int? Frequency { get; set; }
    public int? FrequencyMax { get; set; }
    public decimal? Period { get; set; }
    public decimal? PeriodMax { get; set; }
    public string? PeriodUnit { get; set; }
    public List<string>? DayOfWeek { get; set; }
    public List<string>? TimeOfDay { get; set; }
    public List<string>? When { get; set; }
    public int? Offset { get; set; }
}

/// <summary>
/// FHIR Dosage DoseAndRate element
/// </summary>
public class FhirDosageDoseAndRateDto
{
    public FhirCodeableConceptDto? Type { get; set; }
    public FhirRangeDto? DoseRange { get; set; }
    public FhirQuantityDto? DoseQuantity { get; set; }
    public FhirRatioDto? RateRatio { get; set; }
    public FhirRangeDto? RateRange { get; set; }
    public FhirQuantityDto? RateQuantity { get; set; }
}

/// <summary>
/// FHIR MedicationRequest DispenseRequest element
/// </summary>
public class FhirMedicationRequestDispenseRequestDto
{
    public FhirMedicationRequestDispenseRequestInitialFillDto? InitialFill { get; set; }
    public FhirQuantityDto? DispenseInterval { get; set; }
    public FhirPeriodDto? ValidityPeriod { get; set; }
    public int? NumberOfRepeatsAllowed { get; set; }
    public FhirQuantityDto? Quantity { get; set; }
    public FhirQuantityDto? ExpectedSupplyDuration { get; set; }
    public FhirReferenceDto? Performer { get; set; }
}

/// <summary>
/// FHIR MedicationRequest InitialFill element
/// </summary>
public class FhirMedicationRequestDispenseRequestInitialFillDto
{
    public FhirQuantityDto? Quantity { get; set; }
    public FhirQuantityDto? Duration { get; set; }
}

/// <summary>
/// FHIR MedicationRequest Substitution element
/// </summary>
public class FhirMedicationRequestSubstitutionDto
{
    public bool? AllowedBoolean { get; set; }
    public FhirCodeableConceptDto? AllowedCodeableConcept { get; set; }
    public FhirCodeableConceptDto? Reason { get; set; }
}

#endregion

#region FHIR Bundle

/// <summary>
/// FHIR Bundle resource DTO
/// </summary>
public class FhirBundleDto
{
    public string ResourceType { get; set; } = "Bundle";
    public string? Id { get; set; }
    public FhirMetaDto? Meta { get; set; }
    public List<FhirIdentifierDto>? Identifier { get; set; }
    public string Type { get; set; } = string.Empty; // document, message, transaction, transaction-response, batch, batch-response, history, searchset, collection
    public DateTime? Timestamp { get; set; }
    public int? Total { get; set; }
    public List<FhirBundleLinkDto>? Link { get; set; }
    public List<FhirBundleEntryDto>? Entry { get; set; }
    public FhirSignatureDto? Signature { get; set; }
}

/// <summary>
/// FHIR Bundle Link element
/// </summary>
public class FhirBundleLinkDto
{
    public string Relation { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// FHIR Bundle Entry element
/// </summary>
public class FhirBundleEntryDto
{
    public List<FhirBundleLinkDto>? Link { get; set; }
    public string? FullUrl { get; set; }
    public object? Resource { get; set; }
    public FhirBundleEntrySearchDto? Search { get; set; }
    public FhirBundleEntryRequestDto? Request { get; set; }
    public FhirBundleEntryResponseDto? Response { get; set; }
}

/// <summary>
/// FHIR Bundle Entry Search element
/// </summary>
public class FhirBundleEntrySearchDto
{
    public string? Mode { get; set; } // match, include, outcome
    public decimal? Score { get; set; }
}

/// <summary>
/// FHIR Bundle Entry Request element
/// </summary>
public class FhirBundleEntryRequestDto
{
    public string Method { get; set; } = string.Empty; // GET, HEAD, POST, PUT, DELETE, PATCH
    public string Url { get; set; } = string.Empty;
    public string? IfNoneMatch { get; set; }
    public DateTime? IfModifiedSince { get; set; }
    public string? IfMatch { get; set; }
    public string? IfNoneExist { get; set; }
}

/// <summary>
/// FHIR Bundle Entry Response element
/// </summary>
public class FhirBundleEntryResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Etag { get; set; }
    public DateTime? LastModified { get; set; }
    public object? Outcome { get; set; }
}

/// <summary>
/// FHIR Signature element
/// </summary>
public class FhirSignatureDto
{
    public List<FhirCodingDto> Type { get; set; } = new();
    public DateTime When { get; set; }
    public FhirReferenceDto Who { get; set; } = new();
    public FhirReferenceDto? OnBehalfOf { get; set; }
    public string? TargetFormat { get; set; }
    public string? SigFormat { get; set; }
    public string? Data { get; set; } // base64
}

#endregion

#region FHIR Service DTOs

/// <summary>
/// FHIR export request
/// </summary>
public class FhirExportRequestDto
{
    public string ResourceType { get; set; } = string.Empty;
    public int? ResourceId { get; set; }
    public string? Format { get; set; } = "json"; // json, xml
    public bool IncludeRelated { get; set; }
}

/// <summary>
/// FHIR import request
/// </summary>
public class FhirImportRequestDto
{
    public string ResourceType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Format { get; set; } = "json";
    public bool ValidateOnly { get; set; }
}

/// <summary>
/// FHIR import response
/// </summary>
public class FhirImportResponseDto
{
    public bool Success { get; set; }
    public string? ResourceType { get; set; }
    public int? CreatedId { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// FHIR validation result
/// </summary>
public class FhirValidationResultDto
{
    public bool IsValid { get; set; }
    public List<FhirValidationIssueDto> Issues { get; set; } = new();
}

/// <summary>
/// FHIR validation issue
/// </summary>
public class FhirValidationIssueDto
{
    public string Severity { get; set; } = string.Empty; // error, warning, information
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Location { get; set; }
}

#endregion
