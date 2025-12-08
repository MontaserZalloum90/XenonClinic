using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

/// <summary>
/// API Controller for Appointment management
/// </summary>
[Authorize(AuthenticationSchemes = "Bearer")]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsApiController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IBranchScopedService _branchScope;

    public AppointmentsApiController(
        IAppointmentService appointmentService,
        IBranchScopedService branchScope)
    {
        _appointmentService = appointmentService;
        _branchScope = branchScope;
    }

    /// <summary>
    /// Get all appointments for current branch
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetAppointmentsByBranchIdAsync(branchId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
            return NotFound();

        return Ok(appointment);
    }

    /// <summary>
    /// Get appointments by patient ID
    /// </summary>
    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by provider ID
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var appointments = await _appointmentService.GetAppointmentsByProviderIdAsync(providerId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get today's appointments
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetTodayAppointmentsAsync(branchId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get upcoming appointments
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(branchId, days);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by date
    /// </summary>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(DateTime date)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetAppointmentsByDateAsync(branchId, date);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by date range
    /// </summary>
    [HttpGet("range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(branchId, startDate, endDate);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(AppointmentStatus status)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var appointments = await _appointmentService.GetAppointmentsByStatusAsync(branchId, status);
        return Ok(appointments);
    }

    /// <summary>
    /// Check if time slot is available
    /// </summary>
    [HttpGet("availability/check")]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] int? providerId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] int? excludeAppointmentId = null)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(
            branchId, providerId, startTime, endTime, excludeAppointmentId);

        return Ok(new { isAvailable });
    }

    /// <summary>
    /// Get available time slots for a date
    /// </summary>
    [HttpGet("availability/slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] int? providerId,
        [FromQuery] DateTime date,
        [FromQuery] int durationMinutes = 30)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var slots = await _appointmentService.GetAvailableSlotsAsync(branchId, providerId, date, durationMinutes);
        return Ok(slots);
    }

    /// <summary>
    /// Create new appointment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Appointment appointment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        appointment.BranchId = _branchScope.GetCurrentBranchId();
        var created = await _appointmentService.CreateAppointmentAsync(appointment);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update appointment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Appointment appointment)
    {
        if (id != appointment.Id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _appointmentService.UpdateAppointmentAsync(appointment);
        return NoContent();
    }

    /// <summary>
    /// Delete appointment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentService.DeleteAppointmentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Confirm appointment
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        await _appointmentService.ConfirmAppointmentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Cancel appointment
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequest? request = null)
    {
        await _appointmentService.CancelAppointmentAsync(id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Check-in appointment
    /// </summary>
    [HttpPost("{id}/checkin")]
    public async Task<IActionResult> CheckIn(int id)
    {
        await _appointmentService.CheckInAppointmentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Complete appointment
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        await _appointmentService.CompleteAppointmentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Mark appointment as no-show
    /// </summary>
    [HttpPost("{id}/noshow")]
    public async Task<IActionResult> NoShow(int id)
    {
        await _appointmentService.NoShowAppointmentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Reschedule appointment
    /// </summary>
    [HttpPost("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var rescheduled = await _appointmentService.RescheduleAppointmentAsync(
            id, request.NewStartTime, request.NewEndTime);

        return Ok(rescheduled);
    }

    /// <summary>
    /// Get appointment statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var branchId = _branchScope.GetCurrentBranchId();
        var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow.Date;

        var stats = new
        {
            total = await _appointmentService.GetTotalAppointmentsCountAsync(branchId),
            today = await _appointmentService.GetTodayAppointmentsCountAsync(branchId),
            upcoming = await _appointmentService.GetUpcomingAppointmentsCountAsync(branchId),
            completed = await _appointmentService.GetCompletedAppointmentsCountAsync(branchId, start, end),
            cancelled = await _appointmentService.GetCancelledAppointmentsCountAsync(branchId, start, end),
            noShow = await _appointmentService.GetNoShowAppointmentsCountAsync(branchId, start, end),
            statusDistribution = await _appointmentService.GetAppointmentStatusDistributionAsync(branchId),
            typeDistribution = await _appointmentService.GetAppointmentTypeDistributionAsync(branchId),
            completionRate = await _appointmentService.GetAppointmentCompletionRateAsync(branchId, start, end)
        };

        return Ok(stats);
    }
}

// DTOs
public class CancelRequest
{
    public string? Reason { get; set; }
}

public class RescheduleRequest
{
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
}
