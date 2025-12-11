using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Patient Portal operations
/// </summary>
public interface IPatientPortalService
{
    #region Authentication

    /// <summary>
    /// Register a new patient portal account
    /// </summary>
    Task<PortalRegistrationResponseDto> RegisterAsync(int branchId, PortalRegistrationDto dto);

    /// <summary>
    /// Authenticate patient login
    /// </summary>
    Task<PortalLoginResponseDto> LoginAsync(PortalLoginDto dto);

    /// <summary>
    /// Verify email address
    /// </summary>
    Task<bool> VerifyEmailAsync(string token);

    /// <summary>
    /// Request password reset
    /// </summary>
    Task<bool> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Reset password
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    /// <summary>
    /// Change password
    /// </summary>
    Task<bool> ChangePasswordAsync(int patientId, string currentPassword, string newPassword);

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    Task<PortalLoginResponseDto> RefreshTokenAsync(string refreshToken);

    #endregion

    #region Profile & Dashboard

    /// <summary>
    /// Get patient portal profile
    /// </summary>
    Task<PatientPortalProfileDto> GetProfileAsync(int patientId);

    /// <summary>
    /// Update patient profile
    /// </summary>
    Task<PatientPortalProfileDto> UpdateProfileAsync(int patientId, UpdatePatientProfileDto dto);

    /// <summary>
    /// Get portal dashboard summary
    /// </summary>
    Task<PatientPortalDashboardDto> GetDashboardAsync(int patientId);

    /// <summary>
    /// Upload profile photo
    /// </summary>
    Task<string> UploadProfilePhotoAsync(int patientId, byte[] photoData, string fileName);

    #endregion

    #region Appointments

    /// <summary>
    /// Get upcoming appointments
    /// </summary>
    Task<IEnumerable<PortalAppointmentSummaryDto>> GetUpcomingAppointmentsAsync(int patientId);

    /// <summary>
    /// Get past appointments
    /// </summary>
    Task<IEnumerable<PortalAppointmentSummaryDto>> GetPastAppointmentsAsync(int patientId, int limit = 20);

    /// <summary>
    /// Get appointment details
    /// </summary>
    Task<PortalAppointmentDto?> GetAppointmentAsync(int patientId, int appointmentId);

    /// <summary>
    /// Get available appointment slots
    /// </summary>
    Task<IEnumerable<PortalAppointmentSlotDto>> GetAvailableSlotsAsync(int branchId, int doctorId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Book a new appointment
    /// </summary>
    Task<PortalAppointmentDto> BookAppointmentAsync(int patientId, PortalBookAppointmentDto dto);

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    Task<bool> CancelAppointmentAsync(int patientId, int appointmentId, string? reason);

    /// <summary>
    /// Reschedule an appointment
    /// </summary>
    Task<PortalAppointmentDto> RescheduleAppointmentAsync(int patientId, int appointmentId, DateTime newDate, string? newTime);

    /// <summary>
    /// Get doctors for booking
    /// </summary>
    Task<IEnumerable<PortalDoctorDto>> GetDoctorsForBookingAsync(int branchId, string? specialty = null);

    #endregion

    #region Medical Records

    /// <summary>
    /// Get medical records summary
    /// </summary>
    Task<PortalMedicalRecordsSummaryDto> GetMedicalRecordsSummaryAsync(int patientId);

    /// <summary>
    /// Get visit history
    /// </summary>
    Task<IEnumerable<PortalVisitSummaryDto>> GetVisitHistoryAsync(int patientId, int? year = null, int limit = 50);

    /// <summary>
    /// Get visit details
    /// </summary>
    Task<PortalVisitDetailDto?> GetVisitDetailAsync(int patientId, int visitId);

    /// <summary>
    /// Get active diagnoses
    /// </summary>
    Task<IEnumerable<PortalDiagnosisSummaryDto>> GetActiveDiagnosesAsync(int patientId);

    /// <summary>
    /// Get allergies
    /// </summary>
    Task<IEnumerable<PortalAllergyDto>> GetAllergiesAsync(int patientId);

    /// <summary>
    /// Get immunizations
    /// </summary>
    Task<IEnumerable<PortalImmunizationDto>> GetImmunizationsAsync(int patientId);

    /// <summary>
    /// Get vital signs history
    /// </summary>
    Task<IEnumerable<PortalVitalSignsDto>> GetVitalSignsHistoryAsync(int patientId, int limit = 20);

    /// <summary>
    /// Download medical records
    /// </summary>
    Task<byte[]> DownloadMedicalRecordsAsync(int patientId, string format = "pdf");

    #endregion

    #region Lab Results

    /// <summary>
    /// Get lab results
    /// </summary>
    Task<IEnumerable<PortalLabResultSummaryDto>> GetLabResultsAsync(int patientId, int? year = null, int limit = 50);

    /// <summary>
    /// Get lab result details
    /// </summary>
    Task<PortalLabResultDetailDto?> GetLabResultDetailAsync(int patientId, int labResultId);

    /// <summary>
    /// Download lab report
    /// </summary>
    Task<byte[]> DownloadLabReportAsync(int patientId, int labResultId);

    #endregion

    #region Prescriptions

    /// <summary>
    /// Get active medications
    /// </summary>
    Task<IEnumerable<PortalMedicationSummaryDto>> GetActiveMedicationsAsync(int patientId);

    /// <summary>
    /// Get prescription history
    /// </summary>
    Task<IEnumerable<PortalPrescriptionDetailDto>> GetPrescriptionHistoryAsync(int patientId, int limit = 20);

    /// <summary>
    /// Get prescription details
    /// </summary>
    Task<PortalPrescriptionDetailDto?> GetPrescriptionDetailAsync(int patientId, int prescriptionId);

    /// <summary>
    /// Request prescription refill
    /// </summary>
    Task<bool> RequestRefillAsync(int patientId, PortalRefillRequestDto dto);

    #endregion

    #region Messaging

    /// <summary>
    /// Get message threads
    /// </summary>
    Task<IEnumerable<PortalMessageThreadDto>> GetMessageThreadsAsync(int patientId);

    /// <summary>
    /// Get messages in a thread
    /// </summary>
    Task<IEnumerable<PortalMessageDto>> GetMessagesAsync(int patientId, int threadId);

    /// <summary>
    /// Send a message
    /// </summary>
    Task<PortalMessageDto> SendMessageAsync(int patientId, PortalSendMessageDto dto);

    /// <summary>
    /// Mark messages as read
    /// </summary>
    Task MarkMessagesAsReadAsync(int patientId, int threadId);

    /// <summary>
    /// Get unread message count
    /// </summary>
    Task<int> GetUnreadMessageCountAsync(int patientId);

    #endregion

    #region Billing

    /// <summary>
    /// Get invoices
    /// </summary>
    Task<IEnumerable<PortalInvoiceSummaryDto>> GetInvoicesAsync(int patientId, string? status = null);

    /// <summary>
    /// Get invoice details
    /// </summary>
    Task<PortalInvoiceDetailDto?> GetInvoiceDetailAsync(int patientId, int invoiceId);

    /// <summary>
    /// Get outstanding balance
    /// </summary>
    Task<decimal> GetOutstandingBalanceAsync(int patientId);

    /// <summary>
    /// Make a payment
    /// </summary>
    Task<PortalPaymentResponseDto> MakePaymentAsync(int patientId, PortalMakePaymentDto dto);

    /// <summary>
    /// Download invoice PDF
    /// </summary>
    Task<byte[]> DownloadInvoiceAsync(int patientId, int invoiceId);

    /// <summary>
    /// Download receipt PDF
    /// </summary>
    Task<byte[]> DownloadReceiptAsync(int patientId, int paymentId);

    #endregion

    #region Notifications

    /// <summary>
    /// Get notifications
    /// </summary>
    Task<IEnumerable<PortalNotificationDto>> GetNotificationsAsync(int patientId, bool unreadOnly = false);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task MarkNotificationAsReadAsync(int patientId, int notificationId);

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    Task MarkAllNotificationsAsReadAsync(int patientId);

    /// <summary>
    /// Get notification preferences
    /// </summary>
    Task<PortalNotificationPreferencesDto> GetNotificationPreferencesAsync(int patientId);

    /// <summary>
    /// Update notification preferences
    /// </summary>
    Task<PortalNotificationPreferencesDto> UpdateNotificationPreferencesAsync(int patientId, PortalNotificationPreferencesDto dto);

    #endregion
}

/// <summary>
/// Doctor summary for portal booking
/// </summary>
public class PortalDoctorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public double? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public bool AcceptsNewPatients { get; set; }
    public bool OffersTelemedicine { get; set; }
    public List<string>? Languages { get; set; }
    public decimal? ConsultationFee { get; set; }
}
