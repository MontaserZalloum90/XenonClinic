using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for appointment management operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly IValidator<CreateAppointmentDto> _createValidator;
    private readonly IValidator<UpdateAppointmentDto> _updateValidator;
    private readonly IValidator<AppointmentListRequestDto> _listValidator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientService patientService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        IValidator<CreateAppointmentDto> createValidator,
        IValidator<UpdateAppointmentDto> updateValidator,
        IValidator<AppointmentListRequestDto> listValidator,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _listValidator = listValidator;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Gets a paginated list of appointments for the current branch.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AppointmentListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments([FromQuery] AppointmentListRequestDto request)
    {
        var validationResult = await _listValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<Appointment> appointments;

        if (request.DateFrom.HasValue && request.DateTo.HasValue)
        {
            // Validate date range to prevent excessive queries
            if ((request.DateTo.Value - request.DateFrom.Value).TotalDays > 365)
            {
                return ApiBadRequest("Date range cannot exceed 365 days");
            }
            appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(
                branchId.Value, request.DateFrom.Value, request.DateTo.Value);
        }
        else if (request.PatientId.HasValue)
        {
            // SECURITY FIX: Verify user has access to this patient's data
            var patient = await _patientService.GetPatientByIdAsync(request.PatientId.Value);
            if (patient == null)
            {
                return ApiBadRequest("Patient not found");
            }
            if (!HasBranchAccess(patient.BranchId))
            {
                _logger.LogWarning("Unauthorized patient appointment access attempt: PatientId={PatientId}, UserId={UserId}",
                    request.PatientId.Value, _userContext.UserId);
                return ApiBadRequest("You do not have access to this patient's appointments");
            }
            appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(request.PatientId.Value);
        }
        else if (request.ProviderId.HasValue)
        {
            // SECURITY FIX: Verify provider belongs to current branch
            appointments = await _appointmentService.GetAppointmentsByProviderIdAsync(request.ProviderId.Value);
            // Filter to only appointments in accessible branches
            appointments = appointments.Where(a => HasBranchAccess(a.BranchId));
        }
        else if (request.Status.HasValue)
        {
            appointments = await _appointmentService.GetAppointmentsByStatusAsync(branchId.Value, request.Status.Value);
        }
        else
        {
            appointments = await _appointmentService.GetAppointmentsByBranchIdAsync(branchId.Value);
        }

        // Apply additional filters
        var query = appointments.AsQueryable();

        if (request.Type.HasValue)
        {
            query = query.Where(a => a.Type == request.Type.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToListItem)
            .ToList();

        var paginatedResult = new PaginatedResponse<AppointmentListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets an appointment by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        var dto = MapToDto(appointment);
        return ApiOk(dto);
    }

    /// <summary>
    /// Gets appointments for a specific date.
    /// </summary>
    [HttpGet("date/{date}")]
    [ProducesResponseType(typeof(ApiResponse<DailyScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentsByDate(DateTime date)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var appointments = await _appointmentService.GetAppointmentsByDateAsync(branchId.Value, date);
        var appointmentList = appointments.ToList();

        var schedule = new DailyScheduleDto
        {
            Date = date.Date,
            TotalAppointments = appointmentList.Count,
            CompletedAppointments = appointmentList.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointmentList.Count(a => a.Status == AppointmentStatus.Cancelled),
            RemainingAppointments = appointmentList.Count(a =>
                a.Status is AppointmentStatus.Scheduled or AppointmentStatus.Confirmed or AppointmentStatus.CheckedIn),
            Appointments = appointmentList.Select(MapToListItem).ToList()
        };

        return ApiOk(schedule);
    }

    /// <summary>
    /// Gets today's appointments.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayAppointments()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var appointments = await _appointmentService.GetTodayAppointmentsAsync(branchId.Value);
        var items = appointments.Select(MapToListItem);

        return ApiOk(items);
    }

    /// <summary>
    /// Gets upcoming appointments.
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingAppointments([FromQuery] int days = 7)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        if (days < 1 || days > 90)
        {
            return ApiBadRequest("Days must be between 1 and 90");
        }

        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(branchId.Value, days);
        var items = appointments.Select(MapToListItem);

        return ApiOk(items);
    }

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        // Verify patient exists
        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
        {
            return ApiBadRequest(AppointmentValidationMessages.PatientNotFound);
        }

        // Check time slot availability
        var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(
            branchId.Value, dto.ProviderId, dto.StartTime, dto.EndTime);

        if (!isAvailable)
        {
            return ApiConflict(AppointmentValidationMessages.SlotNotAvailable);
        }

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            BranchId = branchId.Value,
            ProviderId = dto.ProviderId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Type = dto.Type,
            Status = AppointmentStatus.Scheduled,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var created = await _appointmentService.CreateAppointmentAsync(appointment);

        _logger.LogInformation(
            "Appointment created: {AppointmentId}, Patient: {PatientId}, Branch: {BranchId}, By: {UserId}",
            created.Id, created.PatientId, branchId, _userContext.UserId);

        // Reload to get navigation properties
        var result = await _appointmentService.GetAppointmentByIdAsync(created.Id);
        var resultDto = MapToDto(result!);

        return ApiCreated(resultDto, $"/api/appointments/{created.Id}");
    }

    /// <summary>
    /// Updates an existing appointment.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return ApiBadRequest(AppointmentValidationMessages.CannotModifyCancelled);
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return ApiBadRequest(AppointmentValidationMessages.CannotModifyCompleted);
        }

        // Check time slot if time changed
        if (appointment.StartTime != dto.StartTime || appointment.EndTime != dto.EndTime)
        {
            var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(
                appointment.BranchId, dto.ProviderId, dto.StartTime, dto.EndTime, id);

            if (!isAvailable)
            {
                return ApiConflict(AppointmentValidationMessages.SlotNotAvailable);
            }
        }

        appointment.PatientId = dto.PatientId;
        appointment.ProviderId = dto.ProviderId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Type = dto.Type;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _userContext.UserId;

        await _appointmentService.UpdateAppointmentAsync(appointment);

        _logger.LogInformation(
            "Appointment updated: {AppointmentId}, By: {UserId}",
            id, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        var resultDto = MapToDto(result!);

        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes an appointment.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        // BUG FIX: Prevent deletion of completed or in-progress appointments
        if (appointment.Status == AppointmentStatus.Completed)
        {
            return ApiBadRequest("Cannot delete a completed appointment. Completed appointments must be retained for medical records.");
        }

        if (appointment.Status == AppointmentStatus.CheckedIn)
        {
            return ApiBadRequest("Cannot delete an appointment after patient check-in. Please cancel the appointment instead.");
        }

        await _appointmentService.DeleteAppointmentAsync(id);

        _logger.LogInformation(
            "Appointment deleted: {AppointmentId}, Status: {Status}, By: {UserId}",
            id, appointment.Status, _userContext.UserId);

        return ApiOk("Appointment deleted successfully");
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Confirms an appointment.
    /// </summary>
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        await _appointmentService.ConfirmAppointmentAsync(id);

        _logger.LogInformation(
            "Appointment confirmed: {AppointmentId}, By: {UserId}",
            id, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return ApiOk(MapToDto(result!));
    }

    /// <summary>
    /// Cancels an appointment.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentDto dto)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        await _appointmentService.CancelAppointmentAsync(id, dto.Reason);

        _logger.LogInformation(
            "Appointment cancelled: {AppointmentId}, Reason: {Reason}, By: {UserId}",
            id, dto.Reason, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return ApiOk(MapToDto(result!));
    }

    /// <summary>
    /// Checks in a patient for an appointment.
    /// </summary>
    [HttpPost("{id:int}/checkin")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckInAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        await _appointmentService.CheckInAppointmentAsync(id);

        _logger.LogInformation(
            "Patient checked in: {AppointmentId}, By: {UserId}",
            id, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return ApiOk(MapToDto(result!));
    }

    /// <summary>
    /// Completes an appointment.
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        await _appointmentService.CompleteAppointmentAsync(id);

        _logger.LogInformation(
            "Appointment completed: {AppointmentId}, By: {UserId}",
            id, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return ApiOk(MapToDto(result!));
    }

    /// <summary>
    /// Marks an appointment as no-show.
    /// </summary>
    [HttpPost("{id:int}/noshow")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> NoShowAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        await _appointmentService.NoShowAppointmentAsync(id);

        _logger.LogInformation(
            "Appointment marked as no-show: {AppointmentId}, By: {UserId}",
            id, _userContext.UserId);

        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return ApiOk(MapToDto(result!));
    }

    #endregion

    #region Scheduling

    /// <summary>
    /// Gets available time slots for a date.
    /// </summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TimeSlotDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailability([FromQuery] AvailabilityRequestDto request)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var availableSlots = await _appointmentService.GetAvailableSlotsAsync(
            branchId.Value,
            request.ProviderId,
            request.Date,
            request.DurationMinutes);

        var slots = availableSlots.Select(s => new TimeSlotDto
        {
            StartTime = s,
            EndTime = s.AddMinutes(request.DurationMinutes),
            DurationMinutes = request.DurationMinutes,
            IsAvailable = true
        });

        return ApiOk(slots);
    }

    /// <summary>
    /// Reschedules an appointment.
    /// </summary>
    [HttpPost("{id:int}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentDto dto)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return ApiNotFound(AppointmentValidationMessages.AppointmentNotFound);
        }

        if (!HasBranchAccess(appointment.BranchId))
        {
            throw new ForbiddenException(AppointmentValidationMessages.BranchAccessDenied);
        }

        var rescheduled = await _appointmentService.RescheduleAppointmentAsync(
            id, dto.NewStartTime, dto.NewEndTime);

        _logger.LogInformation(
            "Appointment rescheduled: {AppointmentId}, NewTime: {NewStartTime}, By: {UserId}",
            id, dto.NewStartTime, _userContext.UserId);

        return ApiOk(MapToDto(rescheduled));
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets appointment statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var statusDistribution = await _appointmentService.GetAppointmentStatusDistributionAsync(branchId.Value);
        var typeDistribution = await _appointmentService.GetAppointmentTypeDistributionAsync(branchId.Value);

        var statistics = new AppointmentStatisticsDto
        {
            TotalAppointments = await _appointmentService.GetTotalAppointmentsCountAsync(branchId.Value),
            TodayAppointments = await _appointmentService.GetTodayAppointmentsCountAsync(branchId.Value),
            UpcomingAppointments = await _appointmentService.GetUpcomingAppointmentsCountAsync(branchId.Value),
            CompletedThisMonth = await _appointmentService.GetCompletedAppointmentsCountAsync(
                branchId.Value, startOfMonth, endOfMonth),
            CancelledThisMonth = await _appointmentService.GetCancelledAppointmentsCountAsync(
                branchId.Value, startOfMonth, endOfMonth),
            NoShowThisMonth = await _appointmentService.GetNoShowAppointmentsCountAsync(
                branchId.Value, startOfMonth, endOfMonth),
            CompletionRate = await _appointmentService.GetAppointmentCompletionRateAsync(
                branchId.Value, startOfMonth, endOfMonth),
            StatusDistribution = statusDistribution.ToDictionary(
                k => k.Key.ToString(),
                v => v.Value),
            TypeDistribution = typeDistribution.ToDictionary(
                k => k.Key.ToString(),
                v => v.Value)
        };

        return ApiOk(statistics);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentBranchId() => _tenantContext.BranchId;

    private bool HasBranchAccess(int branchId)
    {
        if (_tenantContext.IsCompanyAdmin)
            return true;

        return _tenantContext.HasBranchAccess(branchId);
    }

    private static IQueryable<Appointment> ApplySorting(
        IQueryable<Appointment> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "patientname" => descending
                ? query.OrderByDescending(a => a.Patient!.FullNameEn)
                : query.OrderBy(a => a.Patient!.FullNameEn),
            "providername" => descending
                ? query.OrderByDescending(a => a.Provider!.FullNameEn)
                : query.OrderBy(a => a.Provider!.FullNameEn),
            "status" => descending
                ? query.OrderByDescending(a => a.Status)
                : query.OrderBy(a => a.Status),
            "type" => descending
                ? query.OrderByDescending(a => a.Type)
                : query.OrderBy(a => a.Type),
            _ => descending
                ? query.OrderByDescending(a => a.StartTime)
                : query.OrderBy(a => a.StartTime)
        };
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.FullNameEn,
            // SECURITY FIX: Removed PatientEmiratesId - sensitive PII should not be exposed in appointment responses
            // Emirates ID should only be accessible through dedicated patient identity verification endpoints
            BranchId = appointment.BranchId,
            BranchName = appointment.Branch?.Name,
            ProviderId = appointment.ProviderId,
            ProviderName = appointment.Provider?.FullNameEn,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Type = appointment.Type,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            CreatedBy = appointment.CreatedBy,
            UpdatedAt = appointment.UpdatedAt,
            UpdatedBy = appointment.UpdatedBy
        };
    }

    private static AppointmentListItemDto MapToListItem(Appointment appointment)
    {
        return new AppointmentListItemDto
        {
            Id = appointment.Id,
            PatientName = appointment.Patient?.FullNameEn ?? "Unknown",
            ProviderName = appointment.Provider?.FullNameEn,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Type = appointment.Type,
            Status = appointment.Status
        };
    }

    #endregion
}
