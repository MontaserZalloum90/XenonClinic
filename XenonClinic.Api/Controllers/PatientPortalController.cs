using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Patient Portal API - Self-service endpoints for patients
/// </summary>
[Route("api/portal")]
public class PatientPortalController : BaseApiController
{
    private readonly IPatientPortalService _portalService;
    private readonly ILogger<PatientPortalController> _logger;

    public PatientPortalController(
        IPatientPortalService portalService,
        ILogger<PatientPortalController> logger)
    {
        _portalService = portalService;
        _logger = logger;
    }

    #region Authentication

    /// <summary>
    /// Register a new patient portal account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalRegistrationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<PortalRegistrationResponseDto>>> Register(
        [FromQuery] int branchId,
        [FromBody] PortalRegistrationDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var result = await _portalService.RegisterAsync(branchId, request);

        if (!result.Success)
            return ApiBadRequest(result.Message ?? "Registration failed");

        return ApiOk(result, "Registration successful. Please verify your email.");
    }

    /// <summary>
    /// Authenticate patient login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalLoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<ActionResult<ApiResponse<PortalLoginResponseDto>>> Login([FromBody] PortalLoginDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var result = await _portalService.LoginAsync(request);

        if (!result.Success)
            return ApiUnauthorized(result.Message ?? "Invalid credentials");

        return ApiOk(result);
    }

    /// <summary>
    /// Verify email address
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse>> VerifyEmail([FromQuery] string token)
    {
        var success = await _portalService.VerifyEmailAsync(token);

        if (!success)
            return ApiBadRequest("Invalid or expired verification token");

        return ApiOk("Email verified successfully");
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _portalService.RequestPasswordResetAsync(request.Email);

        // Always return success to prevent email enumeration
        return ApiOk("If an account exists with this email, a reset link has been sent.");
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _portalService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
            return ApiBadRequest("Invalid or expired reset token");

        return ApiOk("Password reset successfully");
    }

    /// <summary>
    /// Change password (authenticated)
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromQuery] int patientId,
        [FromBody] ChangePasswordRequest request)
    {
        var success = await _portalService.ChangePasswordAsync(patientId, request.CurrentPassword, request.NewPassword);

        if (!success)
            return ApiBadRequest("Current password is incorrect");

        return ApiOk("Password changed successfully");
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalLoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<ActionResult<ApiResponse<PortalLoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _portalService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            return ApiUnauthorized("Invalid or expired refresh token");

        return ApiOk(result);
    }

    #endregion

    #region Profile & Dashboard

    /// <summary>
    /// Get patient portal dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalDashboardDto>), 200)]
    public async Task<ActionResult<ApiResponse<PatientPortalDashboardDto>>> GetDashboard([FromQuery] int patientId)
    {
        var dashboard = await _portalService.GetDashboardAsync(patientId);
        return ApiOk(dashboard);
    }

    /// <summary>
    /// Get patient profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalProfileDto>), 200)]
    public async Task<ActionResult<ApiResponse<PatientPortalProfileDto>>> GetProfile([FromQuery] int patientId)
    {
        var profile = await _portalService.GetProfileAsync(patientId);
        return ApiOk(profile);
    }

    /// <summary>
    /// Update patient profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalProfileDto>), 200)]
    public async Task<ActionResult<ApiResponse<PatientPortalProfileDto>>> UpdateProfile(
        [FromQuery] int patientId,
        [FromBody] UpdatePatientProfileDto request)
    {
        var profile = await _portalService.UpdateProfileAsync(patientId, request);
        return ApiOk(profile, "Profile updated successfully");
    }

    /// <summary>
    /// Upload profile photo
    /// </summary>
    [HttpPost("profile/photo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<ActionResult<ApiResponse<string>>> UploadProfilePhoto(
        [FromQuery] int patientId,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiBadRequest("No file uploaded");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var photoUrl = await _portalService.UploadProfilePhotoAsync(patientId, stream.ToArray(), file.FileName);

        return ApiOk(photoUrl, "Photo uploaded successfully");
    }

    #endregion

    #region Appointments

    /// <summary>
    /// Get upcoming appointments
    /// </summary>
    [HttpGet("appointments/upcoming")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>>> GetUpcomingAppointments(
        [FromQuery] int patientId)
    {
        var appointments = await _portalService.GetUpcomingAppointmentsAsync(patientId);
        return ApiOk(appointments);
    }

    /// <summary>
    /// Get past appointments
    /// </summary>
    [HttpGet("appointments/past")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>>> GetPastAppointments(
        [FromQuery] int patientId,
        [FromQuery] int limit = 20)
    {
        var appointments = await _portalService.GetPastAppointmentsAsync(patientId, limit);
        return ApiOk(appointments);
    }

    /// <summary>
    /// Get appointment details
    /// </summary>
    [HttpGet("appointments/{appointmentId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> GetAppointment(
        [FromQuery] int patientId,
        int appointmentId)
    {
        var appointment = await _portalService.GetAppointmentAsync(patientId, appointmentId);

        if (appointment == null)
            return ApiNotFound("Appointment not found");

        return ApiOk(appointment);
    }

    /// <summary>
    /// Get available appointment slots
    /// </summary>
    [HttpGet("appointments/slots")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSlotDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSlotDto>>>> GetAvailableSlots(
        [FromQuery] int branchId,
        [FromQuery] int doctorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var slots = await _portalService.GetAvailableSlotsAsync(branchId, doctorId, startDate, endDate);
        return ApiOk(slots);
    }

    /// <summary>
    /// Get doctors available for booking
    /// </summary>
    [HttpGet("appointments/doctors")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalDoctorDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalDoctorDto>>>> GetDoctorsForBooking(
        [FromQuery] int branchId,
        [FromQuery] string? specialty = null)
    {
        var doctors = await _portalService.GetDoctorsForBookingAsync(branchId, specialty);
        return ApiOk(doctors);
    }

    /// <summary>
    /// Book an appointment
    /// </summary>
    [HttpPost("appointments")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> BookAppointment(
        [FromQuery] int patientId,
        [FromBody] PortalBookAppointmentDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var appointment = await _portalService.BookAppointmentAsync(patientId, request);
        return ApiCreated(appointment, message: "Appointment booked successfully");
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [HttpDelete("appointments/{appointmentId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse>> CancelAppointment(
        [FromQuery] int patientId,
        int appointmentId,
        [FromQuery] string? reason = null)
    {
        var success = await _portalService.CancelAppointmentAsync(patientId, appointmentId, reason);

        if (!success)
            return ApiBadRequest("Unable to cancel appointment");

        return ApiOk("Appointment cancelled successfully");
    }

    /// <summary>
    /// Reschedule an appointment
    /// </summary>
    [HttpPut("appointments/{appointmentId}/reschedule")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> RescheduleAppointment(
        [FromQuery] int patientId,
        int appointmentId,
        [FromQuery] DateTime newDate,
        [FromQuery] string? newTime = null)
    {
        var appointment = await _portalService.RescheduleAppointmentAsync(patientId, appointmentId, newDate, newTime);
        return ApiOk(appointment, "Appointment rescheduled successfully");
    }

    #endregion

    #region Medical Records

    /// <summary>
    /// Get medical records summary
    /// </summary>
    [HttpGet("medical-records")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalMedicalRecordsSummaryDto>), 200)]
    public async Task<ActionResult<ApiResponse<PortalMedicalRecordsSummaryDto>>> GetMedicalRecordsSummary(
        [FromQuery] int patientId)
    {
        var records = await _portalService.GetMedicalRecordsSummaryAsync(patientId);
        return ApiOk(records);
    }

    /// <summary>
    /// Get visit history
    /// </summary>
    [HttpGet("medical-records/visits")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalVisitSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalVisitSummaryDto>>>> GetVisitHistory(
        [FromQuery] int patientId,
        [FromQuery] int? year = null,
        [FromQuery] int limit = 50)
    {
        var visits = await _portalService.GetVisitHistoryAsync(patientId, year, limit);
        return ApiOk(visits);
    }

    /// <summary>
    /// Get visit details
    /// </summary>
    [HttpGet("medical-records/visits/{visitId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalVisitDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PortalVisitDetailDto>>> GetVisitDetail(
        [FromQuery] int patientId,
        int visitId)
    {
        var visit = await _portalService.GetVisitDetailAsync(patientId, visitId);

        if (visit == null)
            return ApiNotFound("Visit not found");

        return ApiOk(visit);
    }

    /// <summary>
    /// Get allergies
    /// </summary>
    [HttpGet("medical-records/allergies")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAllergyDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAllergyDto>>>> GetAllergies(
        [FromQuery] int patientId)
    {
        var allergies = await _portalService.GetAllergiesAsync(patientId);
        return ApiOk(allergies);
    }

    /// <summary>
    /// Get immunizations
    /// </summary>
    [HttpGet("medical-records/immunizations")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalImmunizationDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalImmunizationDto>>>> GetImmunizations(
        [FromQuery] int patientId)
    {
        var immunizations = await _portalService.GetImmunizationsAsync(patientId);
        return ApiOk(immunizations);
    }

    /// <summary>
    /// Get vital signs history
    /// </summary>
    [HttpGet("medical-records/vitals")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalVitalSignsDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalVitalSignsDto>>>> GetVitalSignsHistory(
        [FromQuery] int patientId,
        [FromQuery] int limit = 20)
    {
        var vitals = await _portalService.GetVitalSignsHistoryAsync(patientId, limit);
        return ApiOk(vitals);
    }

    /// <summary>
    /// Download medical records
    /// </summary>
    [HttpGet("medical-records/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> DownloadMedicalRecords(
        [FromQuery] int patientId,
        [FromQuery] string format = "pdf")
    {
        var data = await _portalService.DownloadMedicalRecordsAsync(patientId, format);
        var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/json";
        return File(data, contentType, $"medical-records.{format}");
    }

    #endregion

    #region Lab Results

    /// <summary>
    /// Get lab results
    /// </summary>
    [HttpGet("lab-results")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalLabResultSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalLabResultSummaryDto>>>> GetLabResults(
        [FromQuery] int patientId,
        [FromQuery] int? year = null,
        [FromQuery] int limit = 50)
    {
        var results = await _portalService.GetLabResultsAsync(patientId, year, limit);
        return ApiOk(results);
    }

    /// <summary>
    /// Get lab result details
    /// </summary>
    [HttpGet("lab-results/{labResultId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalLabResultDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PortalLabResultDetailDto>>> GetLabResultDetail(
        [FromQuery] int patientId,
        int labResultId)
    {
        var result = await _portalService.GetLabResultDetailAsync(patientId, labResultId);

        if (result == null)
            return ApiNotFound("Lab result not found");

        return ApiOk(result);
    }

    /// <summary>
    /// Download lab report
    /// </summary>
    [HttpGet("lab-results/{labResultId}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> DownloadLabReport(
        [FromQuery] int patientId,
        int labResultId)
    {
        var data = await _portalService.DownloadLabReportAsync(patientId, labResultId);
        return File(data, "application/pdf", $"lab-report-{labResultId}.pdf");
    }

    #endregion

    #region Prescriptions

    /// <summary>
    /// Get active medications
    /// </summary>
    [HttpGet("medications")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMedicationSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMedicationSummaryDto>>>> GetActiveMedications(
        [FromQuery] int patientId)
    {
        var medications = await _portalService.GetActiveMedicationsAsync(patientId);
        return ApiOk(medications);
    }

    /// <summary>
    /// Get prescription history
    /// </summary>
    [HttpGet("prescriptions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalPrescriptionDetailDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalPrescriptionDetailDto>>>> GetPrescriptionHistory(
        [FromQuery] int patientId,
        [FromQuery] int limit = 20)
    {
        var prescriptions = await _portalService.GetPrescriptionHistoryAsync(patientId, limit);
        return ApiOk(prescriptions);
    }

    /// <summary>
    /// Get prescription details
    /// </summary>
    [HttpGet("prescriptions/{prescriptionId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalPrescriptionDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PortalPrescriptionDetailDto>>> GetPrescriptionDetail(
        [FromQuery] int patientId,
        int prescriptionId)
    {
        var prescription = await _portalService.GetPrescriptionDetailAsync(patientId, prescriptionId);

        if (prescription == null)
            return ApiNotFound("Prescription not found");

        return ApiOk(prescription);
    }

    /// <summary>
    /// Request prescription refill
    /// </summary>
    [HttpPost("prescriptions/refill")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse>> RequestRefill(
        [FromQuery] int patientId,
        [FromBody] PortalRefillRequestDto request)
    {
        var success = await _portalService.RequestRefillAsync(patientId, request);

        if (!success)
            return ApiBadRequest("Unable to request refill");

        return ApiOk("Refill request submitted successfully");
    }

    #endregion

    #region Messaging

    /// <summary>
    /// Get message threads
    /// </summary>
    [HttpGet("messages")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMessageThreadDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMessageThreadDto>>>> GetMessageThreads(
        [FromQuery] int patientId)
    {
        var threads = await _portalService.GetMessageThreadsAsync(patientId);
        return ApiOk(threads);
    }

    /// <summary>
    /// Get messages in a thread
    /// </summary>
    [HttpGet("messages/thread/{threadId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMessageDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMessageDto>>>> GetMessages(
        [FromQuery] int patientId,
        int threadId)
    {
        var messages = await _portalService.GetMessagesAsync(patientId, threadId);
        return ApiOk(messages);
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("messages")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalMessageDto>), 201)]
    public async Task<ActionResult<ApiResponse<PortalMessageDto>>> SendMessage(
        [FromQuery] int patientId,
        [FromBody] PortalSendMessageDto request)
    {
        var message = await _portalService.SendMessageAsync(patientId, request);
        return ApiCreated(message, message: "Message sent successfully");
    }

    /// <summary>
    /// Mark messages in thread as read
    /// </summary>
    [HttpPut("messages/thread/{threadId}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<ActionResult<ApiResponse>> MarkMessagesAsRead(
        [FromQuery] int patientId,
        int threadId)
    {
        await _portalService.MarkMessagesAsReadAsync(patientId, threadId);
        return ApiOk("Messages marked as read");
    }

    /// <summary>
    /// Get unread message count
    /// </summary>
    [HttpGet("messages/unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadMessageCount([FromQuery] int patientId)
    {
        var count = await _portalService.GetUnreadMessageCountAsync(patientId);
        return ApiOk(count);
    }

    #endregion

    #region Billing

    /// <summary>
    /// Get invoices
    /// </summary>
    [HttpGet("billing/invoices")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalInvoiceSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalInvoiceSummaryDto>>>> GetInvoices(
        [FromQuery] int patientId,
        [FromQuery] string? status = null)
    {
        var invoices = await _portalService.GetInvoicesAsync(patientId, status);
        return ApiOk(invoices);
    }

    /// <summary>
    /// Get invoice details
    /// </summary>
    [HttpGet("billing/invoices/{invoiceId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalInvoiceDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PortalInvoiceDetailDto>>> GetInvoiceDetail(
        [FromQuery] int patientId,
        int invoiceId)
    {
        var invoice = await _portalService.GetInvoiceDetailAsync(patientId, invoiceId);

        if (invoice == null)
            return ApiNotFound("Invoice not found");

        return ApiOk(invoice);
    }

    /// <summary>
    /// Get outstanding balance
    /// </summary>
    [HttpGet("billing/balance")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetOutstandingBalance([FromQuery] int patientId)
    {
        var balance = await _portalService.GetOutstandingBalanceAsync(patientId);
        return ApiOk(balance);
    }

    /// <summary>
    /// Make a payment
    /// </summary>
    [HttpPost("billing/pay")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalPaymentResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<PortalPaymentResponseDto>>> MakePayment(
        [FromQuery] int patientId,
        [FromBody] PortalMakePaymentDto request)
    {
        var result = await _portalService.MakePaymentAsync(patientId, request);

        if (!result.Success)
            return ApiBadRequest(result.Message ?? "Payment failed");

        return ApiOk(result, "Payment processed successfully");
    }

    /// <summary>
    /// Download invoice PDF
    /// </summary>
    [HttpGet("billing/invoices/{invoiceId}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> DownloadInvoice(
        [FromQuery] int patientId,
        int invoiceId)
    {
        var data = await _portalService.DownloadInvoiceAsync(patientId, invoiceId);
        return File(data, "application/pdf", $"invoice-{invoiceId}.pdf");
    }

    /// <summary>
    /// Download payment receipt
    /// </summary>
    [HttpGet("billing/receipts/{paymentId}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> DownloadReceipt(
        [FromQuery] int patientId,
        int paymentId)
    {
        var data = await _portalService.DownloadReceiptAsync(patientId, paymentId);
        return File(data, "application/pdf", $"receipt-{paymentId}.pdf");
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Get notifications
    /// </summary>
    [HttpGet("notifications")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalNotificationDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalNotificationDto>>>> GetNotifications(
        [FromQuery] int patientId,
        [FromQuery] bool unreadOnly = false)
    {
        var notifications = await _portalService.GetNotificationsAsync(patientId, unreadOnly);
        return ApiOk(notifications);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("notifications/{notificationId}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<ActionResult<ApiResponse>> MarkNotificationAsRead(
        [FromQuery] int patientId,
        int notificationId)
    {
        await _portalService.MarkNotificationAsReadAsync(patientId, notificationId);
        return ApiOk("Notification marked as read");
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("notifications/read-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<ActionResult<ApiResponse>> MarkAllNotificationsAsRead([FromQuery] int patientId)
    {
        await _portalService.MarkAllNotificationsAsReadAsync(patientId);
        return ApiOk("All notifications marked as read");
    }

    /// <summary>
    /// Get notification preferences
    /// </summary>
    [HttpGet("notifications/preferences")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalNotificationPreferencesDto>), 200)]
    public async Task<ActionResult<ApiResponse<PortalNotificationPreferencesDto>>> GetNotificationPreferences(
        [FromQuery] int patientId)
    {
        var preferences = await _portalService.GetNotificationPreferencesAsync(patientId);
        return ApiOk(preferences);
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    [HttpPut("notifications/preferences")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalNotificationPreferencesDto>), 200)]
    public async Task<ActionResult<ApiResponse<PortalNotificationPreferencesDto>>> UpdateNotificationPreferences(
        [FromQuery] int patientId,
        [FromBody] PortalNotificationPreferencesDto preferences)
    {
        var updated = await _portalService.UpdateNotificationPreferencesAsync(patientId, preferences);
        return ApiOk(updated, "Preferences updated successfully");
    }

    #endregion
}

#region Request Models

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

#endregion
