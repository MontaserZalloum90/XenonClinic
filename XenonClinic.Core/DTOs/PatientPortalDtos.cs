namespace XenonClinic.Core.DTOs;

#region Patient Portal Profile

/// <summary>
/// Patient portal profile DTO
/// </summary>
public class PatientPortalProfileDto
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? MRN { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsSmsOptIn { get; set; }
    public bool IsEmailOptIn { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Update patient profile request
/// </summary>
public class UpdatePatientProfileDto
{
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public bool? IsSmsOptIn { get; set; }
    public bool? IsEmailOptIn { get; set; }
}

/// <summary>
/// Patient portal dashboard summary
/// </summary>
public class PatientPortalDashboardDto
{
    public PatientPortalProfileDto Profile { get; set; } = new();
    public int UpcomingAppointments { get; set; }
    public int PendingPrescriptions { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingInvoices { get; set; }
    public decimal OutstandingBalance { get; set; }
    public List<PortalAppointmentSummaryDto> NextAppointments { get; set; } = new();
    public List<PortalMedicationSummaryDto> ActiveMedications { get; set; } = new();
    public List<PortalNotificationDto> RecentNotifications { get; set; } = new();
}

#endregion

#region Portal Appointments

/// <summary>
/// Portal appointment DTO
/// </summary>
public class PortalAppointmentDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialty { get; set; }
    public string? DoctorPhotoUrl { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string AppointmentType { get; set; } = string.Empty; // Regular, Follow-up, Consultation
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool CanCancel { get; set; }
    public bool CanReschedule { get; set; }
    public bool IsTelemedicine { get; set; }
    public string? TelemedicineLink { get; set; }
    public string? PreVisitInstructions { get; set; }
}

/// <summary>
/// Portal appointment summary for list view
/// </summary>
public class PortalAppointmentSummaryDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialty { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsTelemedicine { get; set; }
}

/// <summary>
/// Portal book appointment request
/// </summary>
public class PortalBookAppointmentDto
{
    public int DoctorId { get; set; }
    public DateTime PreferredDate { get; set; }
    public string? PreferredTime { get; set; }
    public string? AppointmentType { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool IsTelemedicine { get; set; }
}

/// <summary>
/// Available appointment slot
/// </summary>
public class PortalAppointmentSlotDto
{
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsTelemedicineAvailable { get; set; }
}

#endregion

#region Portal Medical Records

/// <summary>
/// Portal medical records summary
/// </summary>
public class PortalMedicalRecordsSummaryDto
{
    public List<PortalVisitSummaryDto> RecentVisits { get; set; } = new();
    public List<PortalDiagnosisSummaryDto> ActiveDiagnoses { get; set; } = new();
    public List<PortalAllergyDto> Allergies { get; set; } = new();
    public List<PortalImmunizationDto> Immunizations { get; set; } = new();
    public PortalVitalSignsDto? LatestVitals { get; set; }
}

/// <summary>
/// Portal visit summary
/// </summary>
public class PortalVisitSummaryDto
{
    public int Id { get; set; }
    public DateTime VisitDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? VisitType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasDiagnosis { get; set; }
    public bool HasPrescription { get; set; }
    public bool HasLabOrders { get; set; }
}

/// <summary>
/// Portal visit details
/// </summary>
public class PortalVisitDetailDto
{
    public int Id { get; set; }
    public DateTime VisitDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public List<PortalDiagnosisSummaryDto> Diagnoses { get; set; } = new();
    public List<PortalMedicationSummaryDto> Prescriptions { get; set; } = new();
    public PortalVitalSignsDto? VitalSigns { get; set; }
    public List<PortalLabResultSummaryDto> LabResults { get; set; } = new();
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
}

/// <summary>
/// Portal diagnosis summary
/// </summary>
public class PortalDiagnosisSummaryDto
{
    public int Id { get; set; }
    public string DiagnosisName { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public string? DoctorName { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Resolved, Chronic
}

/// <summary>
/// Portal allergy DTO
/// </summary>
public class PortalAllergyDto
{
    public int Id { get; set; }
    public string AllergyName { get; set; } = string.Empty;
    public string? AllergyType { get; set; } // Drug, Food, Environmental
    public string? Severity { get; set; } // Mild, Moderate, Severe
    public string? Reaction { get; set; }
    public DateTime? RecordedDate { get; set; }
}

/// <summary>
/// Portal immunization DTO
/// </summary>
public class PortalImmunizationDto
{
    public int Id { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime AdministeredDate { get; set; }
    public string? DoseNumber { get; set; }
    public string? LotNumber { get; set; }
    public string? AdministeredBy { get; set; }
    public DateTime? NextDueDate { get; set; }
}

/// <summary>
/// Portal vital signs DTO
/// </summary>
public class PortalVitalSignsDto
{
    public DateTime RecordedAt { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public decimal? Temperature { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? BMI { get; set; }
}

#endregion

#region Portal Lab Results

/// <summary>
/// Portal lab result summary
/// </summary>
public class PortalLabResultSummaryDto
{
    public int Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Completed, Cancelled
    public string? OrderingDoctor { get; set; }
    public bool IsAbnormal { get; set; }
}

/// <summary>
/// Portal lab result details
/// </summary>
public class PortalLabResultDetailDto
{
    public int Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? TestCode { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? OrderingDoctor { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PortalLabResultItemDto> Results { get; set; } = new();
    public string? Comments { get; set; }
    public bool CanDownloadReport { get; set; }
}

/// <summary>
/// Portal lab result item
/// </summary>
public class PortalLabResultItemDto
{
    public string ComponentName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Flag { get; set; } // Normal, High, Low, Critical
    public bool IsAbnormal { get; set; }
}

#endregion

#region Portal Prescriptions

/// <summary>
/// Portal medication summary
/// </summary>
public class PortalMedicationSummaryDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Completed, Discontinued
    public string? PrescribingDoctor { get; set; }
    public int? RefillsRemaining { get; set; }
    public bool CanRequestRefill { get; set; }
}

/// <summary>
/// Portal prescription details
/// </summary>
public class PortalPrescriptionDetailDto
{
    public int Id { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string PrescribingDoctor { get; set; } = string.Empty;
    public string? DiagnosisRelated { get; set; }
    public List<PortalPrescriptionItemDto> Medications { get; set; } = new();
    public string? Instructions { get; set; }
    public bool CanRequestRefill { get; set; }
}

/// <summary>
/// Portal prescription item
/// </summary>
public class PortalPrescriptionItemDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public int? Quantity { get; set; }
    public int? Refills { get; set; }
    public string? Instructions { get; set; }
}

/// <summary>
/// Portal refill request
/// </summary>
public class PortalRefillRequestDto
{
    public int PrescriptionItemId { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacyAddress { get; set; }
    public string? PharmacyPhone { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Portal Messaging

/// <summary>
/// Portal message thread
/// </summary>
public class PortalMessageThreadDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialty { get; set; }
    public string? DoctorPhotoUrl { get; set; }
    public DateTime LastMessageAt { get; set; }
    public string LastMessagePreview { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public bool IsClosed { get; set; }
}

/// <summary>
/// Portal message
/// </summary>
public class PortalMessageDto
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string SenderType { get; set; } = string.Empty; // Patient, Doctor, Staff
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public List<PortalMessageAttachmentDto>? Attachments { get; set; }
}

/// <summary>
/// Portal message attachment
/// </summary>
public class PortalMessageAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? DownloadUrl { get; set; }
}

/// <summary>
/// Send message request
/// </summary>
public class PortalSendMessageDto
{
    public int? ThreadId { get; set; }
    public int? DoctorId { get; set; }
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<int>? AttachmentIds { get; set; }
}

#endregion

#region Portal Billing

/// <summary>
/// Portal invoice summary
/// </summary>
public class PortalInvoiceSummaryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Paid, Overdue, Partial
    public string? ServiceDescription { get; set; }
}

/// <summary>
/// Portal invoice details
/// </summary>
public class PortalInvoiceDetailDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PortalInvoiceLineItemDto> LineItems { get; set; } = new();
    public List<PortalPaymentHistoryDto> PaymentHistory { get; set; } = new();
    public bool CanPayOnline { get; set; }
}

/// <summary>
/// Portal invoice line item
/// </summary>
public class PortalInvoiceLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? ServiceDate { get; set; }
}

/// <summary>
/// Portal payment history
/// </summary>
public class PortalPaymentHistoryDto
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Portal payment request
/// </summary>
public class PortalMakePaymentDto
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Card, Bank Transfer
    public string? CardToken { get; set; } // For card payments
    public string? SaveCard { get; set; }
}

/// <summary>
/// Portal payment response
/// </summary>
public class PortalPaymentResponseDto
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public string? ReceiptUrl { get; set; }
    public decimal? NewBalance { get; set; }
}

#endregion

#region Portal Notifications

/// <summary>
/// Portal notification DTO
/// </summary>
public class PortalNotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty; // Appointment, LabResult, Prescription, Message, Billing
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

/// <summary>
/// Portal notification preferences
/// </summary>
public class PortalNotificationPreferencesDto
{
    public bool AppointmentReminders { get; set; } = true;
    public bool LabResultsReady { get; set; } = true;
    public bool PrescriptionRefills { get; set; } = true;
    public bool NewMessages { get; set; } = true;
    public bool BillingReminders { get; set; } = true;
    public bool HealthTips { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; } = true;
}

#endregion

#region Portal Authentication

/// <summary>
/// Portal registration request
/// </summary>
public class PortalRegistrationDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? MRN { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Portal registration response
/// </summary>
public class PortalRegistrationResponseDto
{
    public bool Success { get; set; }
    public int? PatientId { get; set; }
    public string? Message { get; set; }
    public bool RequiresVerification { get; set; }
}

/// <summary>
/// Portal login request
/// </summary>
public class PortalLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

/// <summary>
/// Portal login response
/// </summary>
public class PortalLoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PatientPortalProfileDto? Profile { get; set; }
    public string? Message { get; set; }
}

#endregion
