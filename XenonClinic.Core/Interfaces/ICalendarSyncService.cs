using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Calendar Sync operations
/// Supports Google Calendar, Microsoft Outlook, and Apple Calendar
/// </summary>
public interface ICalendarSyncService
{
    #region Connection Management

    /// <summary>
    /// Get OAuth authorization URL for calendar provider
    /// </summary>
    Task<OAuthAuthorizationResponseDto> GetAuthorizationUrlAsync(int userId, OAuthAuthorizationRequestDto request);

    /// <summary>
    /// Handle OAuth callback and exchange code for tokens
    /// </summary>
    Task<OAuthTokenResponseDto> HandleOAuthCallbackAsync(int userId, OAuthCallbackDto callback);

    /// <summary>
    /// Connect a calendar account
    /// </summary>
    Task<ConnectCalendarResponseDto> ConnectCalendarAsync(int userId, ConnectCalendarRequestDto request);

    /// <summary>
    /// Disconnect a calendar connection
    /// </summary>
    Task<bool> DisconnectCalendarAsync(int connectionId);

    /// <summary>
    /// Get calendar connections for a user
    /// </summary>
    Task<IEnumerable<CalendarConnectionDto>> GetConnectionsAsync(int userId);

    /// <summary>
    /// Get a specific calendar connection
    /// </summary>
    Task<CalendarConnectionDto?> GetConnectionAsync(int connectionId);

    /// <summary>
    /// Set default calendar connection
    /// </summary>
    Task<bool> SetDefaultConnectionAsync(int userId, int connectionId);

    /// <summary>
    /// Get available calendars for a connection
    /// </summary>
    Task<IEnumerable<AvailableCalendarDto>> GetAvailableCalendarsAsync(int connectionId);

    /// <summary>
    /// Update calendar connection settings
    /// </summary>
    Task<CalendarConnectionDto> UpdateConnectionAsync(int connectionId, string? calendarId, CalendarSyncDirectionDto? syncDirection);

    #endregion

    #region Sync Settings

    /// <summary>
    /// Get sync settings for a user
    /// </summary>
    Task<CalendarSyncSettingsDto> GetSyncSettingsAsync(int userId);

    /// <summary>
    /// Update sync settings
    /// </summary>
    Task<CalendarSyncSettingsDto> UpdateSyncSettingsAsync(int userId, UpdateCalendarSyncSettingsDto settings);

    #endregion

    #region Event Operations

    /// <summary>
    /// Get calendar events from external calendar
    /// </summary>
    Task<IEnumerable<CalendarEventDto>> GetEventsAsync(int connectionId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Create an event in external calendar
    /// </summary>
    Task<CalendarEventDto?> CreateEventAsync(int connectionId, CreateCalendarEventDto eventDto);

    /// <summary>
    /// Update an event in external calendar
    /// </summary>
    Task<CalendarEventDto?> UpdateEventAsync(int connectionId, string externalEventId, CreateCalendarEventDto eventDto);

    /// <summary>
    /// Delete an event from external calendar
    /// </summary>
    Task<bool> DeleteEventAsync(int connectionId, string externalEventId);

    /// <summary>
    /// Get a specific event
    /// </summary>
    Task<CalendarEventDto?> GetEventAsync(int connectionId, string externalEventId);

    #endregion

    #region Sync Operations

    /// <summary>
    /// Perform full sync for a connection
    /// </summary>
    Task<SyncOperationResultDto> SyncConnectionAsync(int connectionId, bool forceFullSync = false);

    /// <summary>
    /// Sync all connections for a user
    /// </summary>
    Task<Dictionary<int, SyncOperationResultDto>> SyncAllConnectionsAsync(int userId);

    /// <summary>
    /// Process pending sync operations (called by background job)
    /// </summary>
    Task<int> ProcessPendingSyncsAsync();

    /// <summary>
    /// Resolve a sync conflict
    /// </summary>
    Task<bool> ResolveConflictAsync(int connectionId, ResolveConflictRequestDto request);

    /// <summary>
    /// Get pending conflicts for a connection
    /// </summary>
    Task<IEnumerable<SyncConflictDto>> GetPendingConflictsAsync(int connectionId);

    #endregion

    #region Appointment Sync

    /// <summary>
    /// Sync an appointment to external calendar
    /// </summary>
    Task<SyncAppointmentResponseDto> SyncAppointmentAsync(int userId, SyncAppointmentToCalendarDto request);

    /// <summary>
    /// Remove appointment from external calendar
    /// </summary>
    Task<bool> RemoveAppointmentFromCalendarAsync(int appointmentId);

    /// <summary>
    /// Get calendar mapping for an appointment
    /// </summary>
    Task<AppointmentCalendarMappingDto?> GetAppointmentMappingAsync(int appointmentId);

    /// <summary>
    /// Sync all appointments for a user within date range
    /// </summary>
    Task<SyncOperationResultDto> SyncAppointmentsAsync(int userId, DateTime startDate, DateTime endDate);

    #endregion

    #region Availability

    /// <summary>
    /// Get free/busy information
    /// </summary>
    Task<FreeBusyResponseDto> GetFreeBusyAsync(int userId, FreeBusyQueryDto query);

    /// <summary>
    /// Find available slots across calendars
    /// </summary>
    Task<IEnumerable<AvailableSlotDto>> FindAvailableSlotsAsync(int branchId, FindAvailableSlotsRequestDto request);

    #endregion

    #region Sync History

    /// <summary>
    /// Get sync history
    /// </summary>
    Task<(IEnumerable<CalendarSyncHistoryDto> Items, int TotalCount)> GetSyncHistoryAsync(
        int userId, CalendarSyncHistoryFilterDto filter);

    /// <summary>
    /// Get latest sync status for a connection
    /// </summary>
    Task<CalendarSyncHistoryDto?> GetLatestSyncStatusAsync(int connectionId);

    #endregion
}
