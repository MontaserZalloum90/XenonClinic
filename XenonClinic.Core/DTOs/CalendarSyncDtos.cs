namespace XenonClinic.Core.DTOs;

#region Calendar Event DTOs

/// <summary>
/// Calendar event DTO
/// </summary>
public class CalendarEventDto
{
    public string? Id { get; set; }
    public string? ExternalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? TimeZone { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public string? LocationUrl { get; set; }
    public CalendarEventStatusDto Status { get; set; } = CalendarEventStatusDto.Confirmed;
    public List<CalendarAttendeeDto>? Attendees { get; set; }
    public List<CalendarReminderDto>? Reminders { get; set; }
    public CalendarRecurrenceDto? Recurrence { get; set; }
    public string? ConferenceUrl { get; set; }
    public string? ConferenceProvider { get; set; } // Zoom, Teams, Google Meet
    public Dictionary<string, string>? ExtendedProperties { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Calendar event status
/// </summary>
public enum CalendarEventStatusDto
{
    Confirmed,
    Tentative,
    Cancelled
}

/// <summary>
/// Calendar event attendee
/// </summary>
public class CalendarAttendeeDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public AttendeeResponseStatusDto ResponseStatus { get; set; } = AttendeeResponseStatusDto.NeedsAction;
    public bool IsOrganizer { get; set; }
    public bool IsOptional { get; set; }
}

/// <summary>
/// Attendee response status
/// </summary>
public enum AttendeeResponseStatusDto
{
    NeedsAction,
    Accepted,
    Declined,
    Tentative
}

/// <summary>
/// Calendar reminder
/// </summary>
public class CalendarReminderDto
{
    public string Method { get; set; } = "popup"; // popup, email, sms
    public int MinutesBefore { get; set; }
}

/// <summary>
/// Calendar recurrence rule
/// </summary>
public class CalendarRecurrenceDto
{
    public string Frequency { get; set; } = string.Empty; // DAILY, WEEKLY, MONTHLY, YEARLY
    public int Interval { get; set; } = 1;
    public List<string>? ByDay { get; set; } // MO, TU, WE, TH, FR, SA, SU
    public int? ByMonthDay { get; set; }
    public int? Count { get; set; }
    public DateTime? Until { get; set; }
    public List<DateTime>? ExceptionDates { get; set; }
}

/// <summary>
/// Create/update calendar event request
/// </summary>
public class CreateCalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? TimeZone { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public List<string>? AttendeeEmails { get; set; }
    public bool SendNotifications { get; set; } = true;
    public List<CalendarReminderDto>? Reminders { get; set; }
    public CalendarRecurrenceDto? Recurrence { get; set; }
    public bool CreateConference { get; set; }
    public string? ConferenceProvider { get; set; }
    public Dictionary<string, string>? ExtendedProperties { get; set; }
}

#endregion

#region Calendar Sync Connection DTOs

/// <summary>
/// Calendar connection DTO
/// </summary>
public class CalendarConnectionDto
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, Outlook, Apple
    public string? AccountEmail { get; set; }
    public string? AccountName { get; set; }
    public string? CalendarId { get; set; }
    public string? CalendarName { get; set; }
    public string? CalendarColor { get; set; }
    public CalendarSyncDirectionDto SyncDirection { get; set; } = CalendarSyncDirectionDto.TwoWay;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public int? LastSyncEventCount { get; set; }
    public DateTime ConnectedAt { get; set; }
}

/// <summary>
/// Calendar sync direction
/// </summary>
public enum CalendarSyncDirectionDto
{
    OneWayToExternal,
    OneWayFromExternal,
    TwoWay
}

/// <summary>
/// Connect calendar request
/// </summary>
public class ConnectCalendarRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string? AuthorizationCode { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? CalendarId { get; set; }
    public CalendarSyncDirectionDto SyncDirection { get; set; } = CalendarSyncDirectionDto.TwoWay;
    public bool SetAsDefault { get; set; }
}

/// <summary>
/// Connect calendar response
/// </summary>
public class ConnectCalendarResponseDto
{
    public bool Success { get; set; }
    public int? ConnectionId { get; set; }
    public string? Message { get; set; }
    public string? AuthorizationUrl { get; set; }
}

/// <summary>
/// Available calendar for connection
/// </summary>
public class AvailableCalendarDto
{
    public string CalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsPrimary { get; set; }
    public bool CanEdit { get; set; }
    public string? TimeZone { get; set; }
}

#endregion

#region Calendar Sync Settings DTOs

/// <summary>
/// Calendar sync settings
/// </summary>
public class CalendarSyncSettingsDto
{
    public bool AutoSyncEnabled { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 15;
    public CalendarSyncDirectionDto DefaultSyncDirection { get; set; } = CalendarSyncDirectionDto.TwoWay;
    public bool SyncAppointments { get; set; } = true;
    public bool SyncBlockedTime { get; set; } = true;
    public bool SyncMeetings { get; set; } = true;
    public bool CreateConferenceLinks { get; set; }
    public string? DefaultConferenceProvider { get; set; }
    public bool IncludePatientDetails { get; set; }
    public bool IncludeNotes { get; set; }
    public int SyncWindowDaysPast { get; set; } = 30;
    public int SyncWindowDaysFuture { get; set; } = 90;
    public List<string>? ExcludedEventTypes { get; set; }
}

/// <summary>
/// Update sync settings request
/// </summary>
public class UpdateCalendarSyncSettingsDto
{
    public bool? AutoSyncEnabled { get; set; }
    public int? SyncIntervalMinutes { get; set; }
    public CalendarSyncDirectionDto? DefaultSyncDirection { get; set; }
    public bool? SyncAppointments { get; set; }
    public bool? SyncBlockedTime { get; set; }
    public bool? SyncMeetings { get; set; }
    public bool? CreateConferenceLinks { get; set; }
    public string? DefaultConferenceProvider { get; set; }
    public bool? IncludePatientDetails { get; set; }
    public bool? IncludeNotes { get; set; }
    public int? SyncWindowDaysPast { get; set; }
    public int? SyncWindowDaysFuture { get; set; }
    public List<string>? ExcludedEventTypes { get; set; }
}

#endregion

#region Sync Operation DTOs

/// <summary>
/// Sync operation result
/// </summary>
public class SyncOperationResultDto
{
    public bool Success { get; set; }
    public DateTime SyncStartedAt { get; set; }
    public DateTime SyncCompletedAt { get; set; }
    public int EventsCreated { get; set; }
    public int EventsUpdated { get; set; }
    public int EventsDeleted { get; set; }
    public int Conflicts { get; set; }
    public List<SyncConflictDto>? ConflictDetails { get; set; }
    public List<SyncErrorDto>? Errors { get; set; }
}

/// <summary>
/// Sync conflict
/// </summary>
public class SyncConflictDto
{
    public string EventId { get; set; } = string.Empty;
    public string? LocalEventId { get; set; }
    public string? ExternalEventId { get; set; }
    public string ConflictType { get; set; } = string.Empty; // Modified, Deleted, Duplicate
    public CalendarEventDto? LocalVersion { get; set; }
    public CalendarEventDto? ExternalVersion { get; set; }
    public string? RecommendedAction { get; set; }
}

/// <summary>
/// Sync error
/// </summary>
public class SyncErrorDto
{
    public string? EventId { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

/// <summary>
/// Resolve conflict request
/// </summary>
public class ResolveConflictRequestDto
{
    public string EventId { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty; // KeepLocal, KeepExternal, Merge, Skip
    public CalendarEventDto? MergedEvent { get; set; } // For Merge resolution
}

#endregion

#region Calendar Availability DTOs

/// <summary>
/// Free/busy query request
/// </summary>
public class FreeBusyQueryDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<string>? CalendarIds { get; set; }
    public string? TimeZone { get; set; }
}

/// <summary>
/// Free/busy response
/// </summary>
public class FreeBusyResponseDto
{
    public List<FreeBusyCalendarDto> Calendars { get; set; } = new();
}

/// <summary>
/// Free/busy calendar info
/// </summary>
public class FreeBusyCalendarDto
{
    public string CalendarId { get; set; } = string.Empty;
    public List<BusyPeriodDto> BusyPeriods { get; set; } = new();
}

/// <summary>
/// Busy period
/// </summary>
public class BusyPeriodDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Title { get; set; }
}

/// <summary>
/// Find available slots request
/// </summary>
public class FindAvailableSlotsRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationMinutes { get; set; }
    public List<int>? UserIds { get; set; }
    public string? TimeZone { get; set; }
    public string? StartTimeOfDay { get; set; } // e.g., "09:00"
    public string? EndTimeOfDay { get; set; } // e.g., "17:00"
    public List<string>? ExcludeDaysOfWeek { get; set; }
}

/// <summary>
/// Available slot
/// </summary>
public class AvailableSlotDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int DurationMinutes { get; set; }
    public List<int>? AvailableUserIds { get; set; }
}

#endregion

#region Sync History DTOs

/// <summary>
/// Calendar sync history entry
/// </summary>
public class CalendarSyncHistoryDto
{
    public int Id { get; set; }
    public int ConnectionId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public string Status { get; set; } = string.Empty; // InProgress, Completed, Failed
    public int EventsCreated { get; set; }
    public int EventsUpdated { get; set; }
    public int EventsDeleted { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }
}

/// <summary>
/// Sync history filter
/// </summary>
public class CalendarSyncHistoryFilterDto
{
    public int? ConnectionId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region OAuth DTOs

/// <summary>
/// OAuth authorization request
/// </summary>
public class OAuthAuthorizationRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public List<string>? Scopes { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// OAuth authorization response
/// </summary>
public class OAuthAuthorizationResponseDto
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// OAuth callback request
/// </summary>
public class OAuthCallbackDto
{
    public string Provider { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// OAuth token response
/// </summary>
public class OAuthTokenResponseDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public string? Scope { get; set; }
    public string? Error { get; set; }
}

#endregion

#region Appointment Mapping DTOs

/// <summary>
/// Appointment to calendar event mapping
/// </summary>
public class AppointmentCalendarMappingDto
{
    public int AppointmentId { get; set; }
    public int ConnectionId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string? ExternalCalendarId { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public string? SyncStatus { get; set; }
}

/// <summary>
/// Sync appointment to calendar request
/// </summary>
public class SyncAppointmentToCalendarDto
{
    public int AppointmentId { get; set; }
    public int? ConnectionId { get; set; } // If null, uses default connection
    public bool ForceSync { get; set; }
}

/// <summary>
/// Sync appointment response
/// </summary>
public class SyncAppointmentResponseDto
{
    public bool Success { get; set; }
    public string? ExternalEventId { get; set; }
    public string? ConferenceUrl { get; set; }
    public string? Message { get; set; }
}

#endregion
