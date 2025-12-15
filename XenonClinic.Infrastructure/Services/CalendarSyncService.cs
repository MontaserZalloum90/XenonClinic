using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Calendar Sync with Google Calendar and Microsoft Outlook
/// </summary>
public class CalendarSyncService : ICalendarSyncService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<CalendarSyncService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ISecretEncryptionService _secretEncryptionService;

    // OAuth endpoints
    private const string GoogleAuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string GoogleCalendarApiBase = "https://www.googleapis.com/calendar/v3";

    private const string MicrosoftAuthEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private const string MicrosoftTokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
    private const string MicrosoftGraphApiBase = "https://graph.microsoft.com/v1.0";

    public CalendarSyncService(
        ClinicDbContext context,
        ILogger<CalendarSyncService> logger,
        IHttpClientFactory httpClientFactory,
        ISecretEncryptionService secretEncryptionService)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("CalendarSync");
        _secretEncryptionService = secretEncryptionService;
    }

    #region Connection Management

    public async Task<OAuthAuthorizationResponseDto> GetAuthorizationUrlAsync(
        int userId, OAuthAuthorizationRequestDto request)
    {
        var state = Guid.NewGuid().ToString();
        var config = await GetOAuthConfigAsync(request.Provider);

        if (config == null)
        {
            return new OAuthAuthorizationResponseDto
            {
                AuthorizationUrl = string.Empty,
                State = state
            };
        }

        string authUrl;
        var redirectUri = request.RedirectUri ?? config.RedirectUri;

        switch (request.Provider.ToLower())
        {
            case "google":
                var googleScopes = request.Scopes ?? new List<string>
                {
                    "https://www.googleapis.com/auth/calendar",
                    "https://www.googleapis.com/auth/calendar.events"
                };

                authUrl = $"{GoogleAuthEndpoint}?" +
                    $"client_id={config.ClientId}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString(string.Join(" ", googleScopes))}" +
                    $"&access_type=offline" +
                    $"&prompt=consent" +
                    $"&state={state}";
                break;

            case "outlook":
            case "microsoft":
                var outlookScopes = request.Scopes ?? new List<string>
                {
                    "Calendars.ReadWrite",
                    "offline_access"
                };

                authUrl = $"{MicrosoftAuthEndpoint}?" +
                    $"client_id={config.ClientId}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString(string.Join(" ", outlookScopes))}" +
                    $"&state={state}";
                break;

            default:
                authUrl = string.Empty;
                break;
        }

        // Store state for verification
        await StoreOAuthStateAsync(userId, state, request.Provider);

        return new OAuthAuthorizationResponseDto
        {
            AuthorizationUrl = authUrl,
            State = state
        };
    }

    public async Task<OAuthTokenResponseDto> HandleOAuthCallbackAsync(int userId, OAuthCallbackDto callback)
    {
        if (!string.IsNullOrEmpty(callback.Error))
        {
            return new OAuthTokenResponseDto
            {
                Success = false,
                Error = callback.ErrorDescription ?? callback.Error
            };
        }

        // Verify state
        var storedState = await VerifyOAuthStateAsync(userId, callback.State!);
        if (!storedState)
        {
            return new OAuthTokenResponseDto
            {
                Success = false,
                Error = "Invalid state parameter"
            };
        }

        var config = await GetOAuthConfigAsync(callback.Provider);
        if (config == null)
        {
            return new OAuthTokenResponseDto
            {
                Success = false,
                Error = "OAuth configuration not found"
            };
        }

        // Exchange code for tokens
        return await ExchangeCodeForTokensAsync(callback.Provider, callback.Code!, config);
    }

    public async Task<ConnectCalendarResponseDto> ConnectCalendarAsync(
        int userId, ConnectCalendarRequestDto request)
    {
        try
        {
            // If no tokens provided, return authorization URL
            if (string.IsNullOrEmpty(request.AccessToken))
            {
                var authResponse = await GetAuthorizationUrlAsync(userId, new OAuthAuthorizationRequestDto
                {
                    Provider = request.Provider
                });

                return new ConnectCalendarResponseDto
                {
                    Success = false,
                    AuthorizationUrl = authResponse.AuthorizationUrl,
                    Message = "Authorization required"
                };
            }

            // Get account info
            var accountInfo = await GetAccountInfoAsync(request.Provider, request.AccessToken);

            // Create connection
            var connection = new CalendarConnection
            {
                UserId = userId,
                Provider = request.Provider,
                AccountEmail = accountInfo?.Email,
                AccountName = accountInfo?.Name,
                CalendarId = request.CalendarId,
                AccessToken = _secretEncryptionService.Encrypt(request.AccessToken),
                RefreshToken = _secretEncryptionService.EncryptIfNotEmpty(request.RefreshToken),
                SyncDirection = request.SyncDirection.ToString(),
                IsActive = true,
                IsDefault = request.SetAsDefault,
                ConnectedAt = DateTime.UtcNow
            };

            // If setting as default, unset other defaults
            if (request.SetAsDefault)
            {
                var existingDefaults = await _context.CalendarConnections
                    .Where(c => c.UserId == userId && c.IsDefault)
                    .ToListAsync();

                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            _context.CalendarConnections.Add(connection);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Calendar connected for UserId: {UserId}, Provider: {Provider}",
                userId, request.Provider);

            return new ConnectCalendarResponseDto
            {
                Success = true,
                ConnectionId = connection.Id,
                Message = "Calendar connected successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting calendar for UserId: {UserId}", userId);
            return new ConnectCalendarResponseDto
            {
                Success = false,
                Message = "Error connecting calendar"
            };
        }
    }

    public async Task<bool> DisconnectCalendarAsync(int connectionId)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return false;

        // Revoke tokens with provider (optional)
        try
        {
            await RevokeTokensAsync(connection);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke tokens for connection {ConnectionId}", connectionId);
        }

        connection.IsActive = false;
        connection.DisconnectedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CalendarConnectionDto>> GetConnectionsAsync(int userId)
    {
        var connections = await _context.CalendarConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.IsDefault)
            .ThenByDescending(c => c.ConnectedAt)
            .ToListAsync();

        return connections.Select(MapToConnectionDto);
    }

    public async Task<CalendarConnectionDto?> GetConnectionAsync(int connectionId)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        return connection != null ? MapToConnectionDto(connection) : null;
    }

    public async Task<bool> SetDefaultConnectionAsync(int userId, int connectionId)
    {
        var connections = await _context.CalendarConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .ToListAsync();

        foreach (var connection in connections)
        {
            connection.IsDefault = connection.Id == connectionId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AvailableCalendarDto>> GetAvailableCalendarsAsync(int connectionId)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return Enumerable.Empty<AvailableCalendarDto>();

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await GetGoogleCalendarsAsync(accessToken);
            case "outlook":
            case "microsoft":
                return await GetOutlookCalendarsAsync(accessToken);
            default:
                return Enumerable.Empty<AvailableCalendarDto>();
        }
    }

    public async Task<CalendarConnectionDto> UpdateConnectionAsync(
        int connectionId, string? calendarId, CalendarSyncDirectionDto? syncDirection)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return new CalendarConnectionDto();

        if (!string.IsNullOrEmpty(calendarId))
        {
            connection.CalendarId = calendarId;
        }

        if (syncDirection.HasValue)
        {
            connection.SyncDirection = syncDirection.Value.ToString();
        }

        connection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToConnectionDto(connection);
    }

    #endregion

    #region Sync Settings

    public async Task<CalendarSyncSettingsDto> GetSyncSettingsAsync(int userId)
    {
        var settings = await _context.CalendarSyncSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            return new CalendarSyncSettingsDto();
        }

        return MapToSyncSettingsDto(settings);
    }

    public async Task<CalendarSyncSettingsDto> UpdateSyncSettingsAsync(
        int userId, UpdateCalendarSyncSettingsDto dto)
    {
        var settings = await _context.CalendarSyncSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new CalendarSyncSettings { UserId = userId };
            _context.CalendarSyncSettings.Add(settings);
        }

        if (dto.AutoSyncEnabled.HasValue) settings.AutoSyncEnabled = dto.AutoSyncEnabled.Value;
        if (dto.SyncIntervalMinutes.HasValue) settings.SyncIntervalMinutes = dto.SyncIntervalMinutes.Value;
        if (dto.DefaultSyncDirection.HasValue) settings.DefaultSyncDirection = dto.DefaultSyncDirection.Value.ToString();
        if (dto.SyncAppointments.HasValue) settings.SyncAppointments = dto.SyncAppointments.Value;
        if (dto.SyncBlockedTime.HasValue) settings.SyncBlockedTime = dto.SyncBlockedTime.Value;
        if (dto.SyncMeetings.HasValue) settings.SyncMeetings = dto.SyncMeetings.Value;
        if (dto.CreateConferenceLinks.HasValue) settings.CreateConferenceLinks = dto.CreateConferenceLinks.Value;
        if (dto.DefaultConferenceProvider != null) settings.DefaultConferenceProvider = dto.DefaultConferenceProvider;
        if (dto.IncludePatientDetails.HasValue) settings.IncludePatientDetails = dto.IncludePatientDetails.Value;
        if (dto.IncludeNotes.HasValue) settings.IncludeNotes = dto.IncludeNotes.Value;
        if (dto.SyncWindowDaysPast.HasValue) settings.SyncWindowDaysPast = dto.SyncWindowDaysPast.Value;
        if (dto.SyncWindowDaysFuture.HasValue) settings.SyncWindowDaysFuture = dto.SyncWindowDaysFuture.Value;

        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToSyncSettingsDto(settings);
    }

    #endregion

    #region Event Operations

    public async Task<IEnumerable<CalendarEventDto>> GetEventsAsync(
        int connectionId, DateTime startDate, DateTime endDate)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return Enumerable.Empty<CalendarEventDto>();

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await GetGoogleEventsAsync(accessToken, connection.CalendarId!, startDate, endDate);
            case "outlook":
            case "microsoft":
                return await GetOutlookEventsAsync(accessToken, connection.CalendarId, startDate, endDate);
            default:
                return Enumerable.Empty<CalendarEventDto>();
        }
    }

    public async Task<CalendarEventDto?> CreateEventAsync(int connectionId, CreateCalendarEventDto eventDto)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return null;

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await CreateGoogleEventAsync(accessToken, connection.CalendarId!, eventDto);
            case "outlook":
            case "microsoft":
                return await CreateOutlookEventAsync(accessToken, connection.CalendarId, eventDto);
            default:
                return null;
        }
    }

    public async Task<CalendarEventDto?> UpdateEventAsync(
        int connectionId, string externalEventId, CreateCalendarEventDto eventDto)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return null;

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await UpdateGoogleEventAsync(accessToken, connection.CalendarId!, externalEventId, eventDto);
            case "outlook":
            case "microsoft":
                return await UpdateOutlookEventAsync(accessToken, externalEventId, eventDto);
            default:
                return null;
        }
    }

    public async Task<bool> DeleteEventAsync(int connectionId, string externalEventId)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return false;

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await DeleteGoogleEventAsync(accessToken, connection.CalendarId!, externalEventId);
            case "outlook":
            case "microsoft":
                return await DeleteOutlookEventAsync(accessToken, externalEventId);
            default:
                return false;
        }
    }

    public async Task<CalendarEventDto?> GetEventAsync(int connectionId, string externalEventId)
    {
        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
            return null;

        var accessToken = await RefreshAccessTokenIfNeededAsync(connection);

        switch (connection.Provider.ToLower())
        {
            case "google":
                return await GetGoogleEventAsync(accessToken, connection.CalendarId!, externalEventId);
            case "outlook":
            case "microsoft":
                return await GetOutlookEventAsync(accessToken, externalEventId);
            default:
                return null;
        }
    }

    #endregion

    #region Sync Operations

    public async Task<SyncOperationResultDto> SyncConnectionAsync(int connectionId, bool forceFullSync = false)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SyncOperationResultDto
        {
            SyncStartedAt = DateTime.UtcNow
        };

        var connection = await _context.CalendarConnections.FindAsync(connectionId);
        if (connection == null)
        {
            result.Success = false;
            result.Errors = new List<SyncErrorDto>
            {
                new SyncErrorDto { ErrorType = "ConnectionNotFound", ErrorMessage = "Calendar connection not found" }
            };
            return result;
        }

        try
        {
            // Start sync history entry
            var historyEntry = new CalendarSyncHistoryEntry
            {
                ConnectionId = connectionId,
                Provider = connection.Provider,
                SyncStartedAt = DateTime.UtcNow,
                Status = "InProgress"
            };
            _context.CalendarSyncHistory.Add(historyEntry);
            await _context.SaveChangesAsync();

            var settings = await GetSyncSettingsAsync(connection.UserId);
            var syncDirection = Enum.TryParse<CalendarSyncDirectionDto>(connection.SyncDirection, out var dir)
                ? dir
                : CalendarSyncDirectionDto.TwoWay;

            var startDate = DateTime.UtcNow.AddDays(-settings.SyncWindowDaysPast);
            var endDate = DateTime.UtcNow.AddDays(settings.SyncWindowDaysFuture);

            // Get external events
            var externalEvents = await GetEventsAsync(connectionId, startDate, endDate);

            // Get local appointments
            var localAppointments = await _context.Appointments
                .Where(a => a.ProviderId == connection.UserId &&
                    a.StartTime >= startDate &&
                    a.StartTime <= endDate)
                .ToListAsync();

            // Get existing mappings
            var mappings = await _context.AppointmentCalendarMappings
                .Where(m => m.ConnectionId == connectionId)
                .ToListAsync();

            // Sync logic based on direction
            if (syncDirection == CalendarSyncDirectionDto.TwoWay ||
                syncDirection == CalendarSyncDirectionDto.OneWayToExternal)
            {
                // Push local changes to external
                foreach (var appointment in localAppointments)
                {
                    var mapping = mappings.FirstOrDefault(m => m.AppointmentId == appointment.Id);

                    if (mapping == null)
                    {
                        // Create new external event
                        var eventDto = MapAppointmentToEvent(appointment, settings);
                        var createdEvent = await CreateEventAsync(connectionId, eventDto);

                        if (createdEvent != null)
                        {
                            var newMapping = new AppointmentCalendarMapping
                            {
                                AppointmentId = appointment.Id,
                                ConnectionId = connectionId,
                                ExternalEventId = createdEvent.ExternalId!,
                                ExternalCalendarId = connection.CalendarId,
                                LastSyncedAt = DateTime.UtcNow,
                                SyncStatus = "Synced"
                            };
                            _context.AppointmentCalendarMappings.Add(newMapping);
                            result.EventsCreated++;
                        }
                    }
                    else
                    {
                        // Update existing external event if appointment changed
                        if (appointment.UpdatedAt > mapping.LastSyncedAt)
                        {
                            var eventDto = MapAppointmentToEvent(appointment, settings);
                            await UpdateEventAsync(connectionId, mapping.ExternalEventId, eventDto);
                            mapping.LastSyncedAt = DateTime.UtcNow;
                            result.EventsUpdated++;
                        }
                    }
                }
            }

            stopwatch.Stop();

            // Update history
            historyEntry.SyncCompletedAt = DateTime.UtcNow;
            historyEntry.Status = "Completed";
            historyEntry.EventsCreated = result.EventsCreated;
            historyEntry.EventsUpdated = result.EventsUpdated;
            historyEntry.EventsDeleted = result.EventsDeleted;
            historyEntry.DurationMs = stopwatch.ElapsedMilliseconds;

            // Update connection
            connection.LastSyncAt = DateTime.UtcNow;
            connection.LastSyncStatus = "Success";
            connection.LastSyncEventCount = result.EventsCreated + result.EventsUpdated;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.SyncCompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Calendar sync completed for ConnectionId: {ConnectionId}, Created: {Created}, Updated: {Updated}",
                connectionId, result.EventsCreated, result.EventsUpdated);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.SyncCompletedAt = DateTime.UtcNow;
            result.Errors = new List<SyncErrorDto>
            {
                new SyncErrorDto
                {
                    ErrorType = "SyncError",
                    ErrorMessage = ex.Message
                }
            };

            connection.LastSyncStatus = $"Failed: {ex.Message}";
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Error syncing calendar connection {ConnectionId}", connectionId);
        }

        return result;
    }

    public async Task<Dictionary<int, SyncOperationResultDto>> SyncAllConnectionsAsync(int userId)
    {
        var results = new Dictionary<int, SyncOperationResultDto>();

        var connections = await _context.CalendarConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .ToListAsync();

        foreach (var connection in connections)
        {
            var result = await SyncConnectionAsync(connection.Id);
            results[connection.Id] = result;
        }

        return results;
    }

    public async Task<int> ProcessPendingSyncsAsync()
    {
        var settings = await _context.CalendarSyncSettings
            .Where(s => s.AutoSyncEnabled)
            .ToListAsync();

        var processedCount = 0;

        foreach (var setting in settings)
        {
            var connections = await _context.CalendarConnections
                .Where(c => c.UserId == setting.UserId &&
                    c.IsActive &&
                    (c.LastSyncAt == null ||
                     c.LastSyncAt < DateTime.UtcNow.AddMinutes(-setting.SyncIntervalMinutes)))
                .ToListAsync();

            foreach (var connection in connections)
            {
                await SyncConnectionAsync(connection.Id);
                processedCount++;
            }
        }

        return processedCount;
    }

    public async Task<bool> ResolveConflictAsync(int connectionId, ResolveConflictRequestDto request)
    {
        var conflict = await _context.CalendarSyncConflicts
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.EventId == request.EventId);

        if (conflict == null)
            return false;

        switch (request.Resolution)
        {
            case "KeepLocal":
                if (!string.IsNullOrEmpty(conflict.ExternalEventId))
                {
                    await UpdateEventAsync(connectionId, conflict.ExternalEventId, new CreateCalendarEventDto
                    {
                        Title = conflict.LocalTitle ?? "",
                        StartDateTime = conflict.LocalStartTime ?? DateTime.UtcNow,
                        EndDateTime = conflict.LocalEndTime ?? DateTime.UtcNow
                    });
                }
                break;

            case "KeepExternal":
                // Update local appointment with external data
                // Implementation depends on appointment structure
                break;

            case "Skip":
                break;
        }

        conflict.IsResolved = true;
        conflict.ResolvedAt = DateTime.UtcNow;
        conflict.Resolution = request.Resolution;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SyncConflictDto>> GetPendingConflictsAsync(int connectionId)
    {
        var conflicts = await _context.CalendarSyncConflicts
            .Where(c => c.ConnectionId == connectionId && !c.IsResolved)
            .ToListAsync();

        return conflicts.Select(c => new SyncConflictDto
        {
            EventId = c.EventId,
            LocalEventId = c.LocalAppointmentId?.ToString(),
            ExternalEventId = c.ExternalEventId,
            ConflictType = c.ConflictType
        });
    }

    #endregion

    #region Appointment Sync

    public async Task<SyncAppointmentResponseDto> SyncAppointmentAsync(
        int userId, SyncAppointmentToCalendarDto request)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId);

        if (appointment == null)
        {
            return new SyncAppointmentResponseDto
            {
                Success = false,
                Message = "Appointment not found"
            };
        }

        CalendarConnection? connection;
        if (request.ConnectionId.HasValue)
        {
            connection = await _context.CalendarConnections.FindAsync(request.ConnectionId.Value);
        }
        else
        {
            connection = await _context.CalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDefault && c.IsActive);
        }

        if (connection == null)
        {
            return new SyncAppointmentResponseDto
            {
                Success = false,
                Message = "No calendar connection found"
            };
        }

        var settings = await GetSyncSettingsAsync(userId);
        var eventDto = MapAppointmentToEvent(appointment, settings);

        var existingMapping = await _context.AppointmentCalendarMappings
            .FirstOrDefaultAsync(m => m.AppointmentId == request.AppointmentId &&
                                      m.ConnectionId == connection.Id);

        CalendarEventDto? result;

        if (existingMapping != null && !request.ForceSync)
        {
            result = await UpdateEventAsync(connection.Id, existingMapping.ExternalEventId, eventDto);
            if (result != null)
            {
                existingMapping.LastSyncedAt = DateTime.UtcNow;
                existingMapping.SyncStatus = "Synced";
            }
        }
        else
        {
            result = await CreateEventAsync(connection.Id, eventDto);
            if (result != null)
            {
                if (existingMapping != null)
                {
                    existingMapping.ExternalEventId = result.ExternalId!;
                    existingMapping.LastSyncedAt = DateTime.UtcNow;
                    existingMapping.SyncStatus = "Synced";
                }
                else
                {
                    var mapping = new AppointmentCalendarMapping
                    {
                        AppointmentId = appointment.Id,
                        ConnectionId = connection.Id,
                        ExternalEventId = result.ExternalId!,
                        ExternalCalendarId = connection.CalendarId,
                        LastSyncedAt = DateTime.UtcNow,
                        SyncStatus = "Synced"
                    };
                    _context.AppointmentCalendarMappings.Add(mapping);
                }
            }
        }

        await _context.SaveChangesAsync();

        return new SyncAppointmentResponseDto
        {
            Success = result != null,
            ExternalEventId = result?.ExternalId,
            ConferenceUrl = result?.ConferenceUrl,
            Message = result != null ? "Appointment synced successfully" : "Failed to sync appointment"
        };
    }

    public async Task<bool> RemoveAppointmentFromCalendarAsync(int appointmentId)
    {
        var mappings = await _context.AppointmentCalendarMappings
            .Where(m => m.AppointmentId == appointmentId)
            .ToListAsync();

        foreach (var mapping in mappings)
        {
            await DeleteEventAsync(mapping.ConnectionId, mapping.ExternalEventId);
            _context.AppointmentCalendarMappings.Remove(mapping);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AppointmentCalendarMappingDto?> GetAppointmentMappingAsync(int appointmentId)
    {
        var mapping = await _context.AppointmentCalendarMappings
            .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);

        if (mapping == null)
            return null;

        return new AppointmentCalendarMappingDto
        {
            AppointmentId = mapping.AppointmentId,
            ConnectionId = mapping.ConnectionId,
            ExternalEventId = mapping.ExternalEventId,
            ExternalCalendarId = mapping.ExternalCalendarId,
            LastSyncedAt = mapping.LastSyncedAt,
            SyncStatus = mapping.SyncStatus
        };
    }

    public async Task<SyncOperationResultDto> SyncAppointmentsAsync(
        int userId, DateTime startDate, DateTime endDate)
    {
        var result = new SyncOperationResultDto { SyncStartedAt = DateTime.UtcNow };

        var appointments = await _context.Appointments
            .Where(a => a.ProviderId == userId &&
                a.StartTime >= startDate &&
                a.StartTime <= endDate)
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            var syncResult = await SyncAppointmentAsync(userId, new SyncAppointmentToCalendarDto
            {
                AppointmentId = appointment.Id
            });

            if (syncResult.Success)
            {
                result.EventsCreated++;
            }
        }

        result.Success = true;
        result.SyncCompletedAt = DateTime.UtcNow;
        return result;
    }

    #endregion

    #region Availability

    public async Task<FreeBusyResponseDto> GetFreeBusyAsync(int userId, FreeBusyQueryDto query)
    {
        var response = new FreeBusyResponseDto();

        var connections = await _context.CalendarConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .ToListAsync();

        foreach (var connection in connections)
        {
            var events = await GetEventsAsync(connection.Id, query.StartTime, query.EndTime);

            var calendarInfo = new FreeBusyCalendarDto
            {
                CalendarId = connection.CalendarId ?? connection.Id.ToString(),
                BusyPeriods = events.Select(e => new BusyPeriodDto
                {
                    Start = e.StartDateTime,
                    End = e.EndDateTime,
                    Title = e.Title
                }).ToList()
            };

            response.Calendars.Add(calendarInfo);
        }

        return response;
    }

    public async Task<IEnumerable<AvailableSlotDto>> FindAvailableSlotsAsync(
        int branchId, FindAvailableSlotsRequestDto request)
    {
        var slots = new List<AvailableSlotDto>();

        var users = request.UserIds ?? await _context.Users
            .Where(u => u.BranchId == branchId && u.Role == "Doctor" && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        var startTime = TimeSpan.TryParse(request.StartTimeOfDay, out var st) ? st : TimeSpan.FromHours(9);
        var endTime = TimeSpan.TryParse(request.EndTimeOfDay, out var et) ? et : TimeSpan.FromHours(17);

        for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
        {
            // Skip excluded days
            if (request.ExcludeDaysOfWeek?.Contains(date.DayOfWeek.ToString()) == true)
                continue;

            // Generate slots for the day
            var currentTime = date.Add(startTime);
            var dayEnd = date.Add(endTime);

            while (currentTime.AddMinutes(request.DurationMinutes) <= dayEnd)
            {
                var slotEnd = currentTime.AddMinutes(request.DurationMinutes);
                var availableUserIds = new List<int>();

                foreach (var userId in users)
                {
                    // Check appointments
                    var hasAppointment = await _context.Appointments
                        .AnyAsync(a => a.ProviderId == userId &&
                            a.StartTime < slotEnd &&
                            a.StartTime.AddMinutes(30) > currentTime &&
                            a.Status != AppointmentStatus.Cancelled);

                    if (!hasAppointment)
                    {
                        // Check external calendars
                        var connections = await _context.CalendarConnections
                            .Where(c => c.UserId == userId && c.IsActive)
                            .ToListAsync();

                        var isBusy = false;
                        foreach (var connection in connections)
                        {
                            var events = await GetEventsAsync(connection.Id, currentTime, slotEnd);
                            if (events.Any())
                            {
                                isBusy = true;
                                break;
                            }
                        }

                        if (!isBusy)
                        {
                            availableUserIds.Add(userId);
                        }
                    }
                }

                if (availableUserIds.Any())
                {
                    slots.Add(new AvailableSlotDto
                    {
                        Start = currentTime,
                        End = slotEnd,
                        DurationMinutes = request.DurationMinutes,
                        AvailableUserIds = availableUserIds
                    });
                }

                currentTime = currentTime.AddMinutes(request.DurationMinutes);
            }
        }

        return slots;
    }

    #endregion

    #region Sync History

    public async Task<(IEnumerable<CalendarSyncHistoryDto> Items, int TotalCount)> GetSyncHistoryAsync(
        int userId, CalendarSyncHistoryFilterDto filter)
    {
        var connectionIds = await _context.CalendarConnections
            .Where(c => c.UserId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        var query = _context.CalendarSyncHistory
            .Where(h => connectionIds.Contains(h.ConnectionId));

        if (filter.ConnectionId.HasValue)
            query = query.Where(h => h.ConnectionId == filter.ConnectionId);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(h => h.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(h => h.SyncStartedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(h => h.SyncStartedAt <= filter.ToDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.SyncStartedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(h => new CalendarSyncHistoryDto
            {
                Id = h.Id,
                ConnectionId = h.ConnectionId,
                Provider = h.Provider,
                SyncStartedAt = h.SyncStartedAt,
                SyncCompletedAt = h.SyncCompletedAt,
                Status = h.Status,
                EventsCreated = h.EventsCreated,
                EventsUpdated = h.EventsUpdated,
                EventsDeleted = h.EventsDeleted,
                Errors = h.Errors,
                ErrorMessage = h.ErrorMessage,
                DurationMs = h.DurationMs
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CalendarSyncHistoryDto?> GetLatestSyncStatusAsync(int connectionId)
    {
        var history = await _context.CalendarSyncHistory
            .Where(h => h.ConnectionId == connectionId)
            .OrderByDescending(h => h.SyncStartedAt)
            .FirstOrDefaultAsync();

        if (history == null)
            return null;

        return new CalendarSyncHistoryDto
        {
            Id = history.Id,
            ConnectionId = history.ConnectionId,
            Provider = history.Provider,
            SyncStartedAt = history.SyncStartedAt,
            SyncCompletedAt = history.SyncCompletedAt,
            Status = history.Status,
            EventsCreated = history.EventsCreated,
            EventsUpdated = history.EventsUpdated,
            EventsDeleted = history.EventsDeleted,
            Errors = history.Errors,
            ErrorMessage = history.ErrorMessage,
            DurationMs = history.DurationMs
        };
    }

    #endregion

    #region Private Helper Methods

    private async Task<OAuthConfig?> GetOAuthConfigAsync(string provider)
    {
        return await _context.OAuthConfigs
            .FirstOrDefaultAsync(c => c.Provider.ToLower() == provider.ToLower() && c.IsActive);
    }

    private async Task StoreOAuthStateAsync(int userId, string state, string provider)
    {
        var stateEntry = new OAuthState
        {
            UserId = userId,
            State = state,
            Provider = provider,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _context.OAuthStates.Add(stateEntry);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> VerifyOAuthStateAsync(int userId, string state)
    {
        var stateEntry = await _context.OAuthStates
            .FirstOrDefaultAsync(s => s.UserId == userId &&
                                      s.State == state &&
                                      s.ExpiresAt > DateTime.UtcNow);

        if (stateEntry != null)
        {
            _context.OAuthStates.Remove(stateEntry);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private async Task<OAuthTokenResponseDto> ExchangeCodeForTokensAsync(
        string provider, string code, OAuthConfig config)
    {
        var tokenEndpoint = provider.ToLower() switch
        {
            "google" => GoogleTokenEndpoint,
            "outlook" or "microsoft" => MicrosoftTokenEndpoint,
            _ => null
        };

        if (tokenEndpoint == null)
        {
            return new OAuthTokenResponseDto { Success = false, Error = "Unknown provider" };
        }

        var requestBody = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["redirect_uri"] = config.RedirectUri!,
            ["grant_type"] = "authorization_code"
        };

        var response = await _httpClient.PostAsync(tokenEndpoint,
            new FormUrlEncodedContent(requestBody));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new OAuthTokenResponseDto { Success = false, Error = error };
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

        return new OAuthTokenResponseDto
        {
            Success = true,
            AccessToken = tokenResponse.GetProperty("access_token").GetString(),
            RefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            ExpiresIn = tokenResponse.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : null,
            TokenType = tokenResponse.TryGetProperty("token_type", out var tt) ? tt.GetString() : null,
            Scope = tokenResponse.TryGetProperty("scope", out var sc) ? sc.GetString() : null
        };
    }

    private async Task<(string? Email, string? Name)?> GetAccountInfoAsync(string provider, string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        string endpoint;
        switch (provider.ToLower())
        {
            case "google":
                endpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                break;
            case "outlook":
            case "microsoft":
                endpoint = $"{MicrosoftGraphApiBase}/me";
                break;
            default:
                return null;
        }

        var response = await _httpClient.GetFromJsonAsync<JsonElement>(endpoint);
        var email = response.TryGetProperty("email", out var e) ? e.GetString() : null;
        var name = response.TryGetProperty("name", out var n) ? n.GetString() : null;

        return (email, name);
    }

    private async Task<string> RefreshAccessTokenIfNeededAsync(CalendarConnection connection)
    {
        var accessToken = _secretEncryptionService.Decrypt(connection.AccessToken);

        // Check if token is still valid (with 5 minute buffer)
        if (connection.TokenExpiresAt.HasValue &&
            connection.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return accessToken;
        }

        // Token expired or expiring soon - refresh it
        var refreshToken = _secretEncryptionService.DecryptIfNotEmpty(connection.RefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Cannot refresh token for ConnectionId {ConnectionId}: no refresh token available",
                connection.Id);
            return accessToken; // Return existing token and hope for the best
        }

        try
        {
            var config = await GetOAuthConfigAsync(connection.Provider);
            if (config == null)
            {
                _logger.LogError("OAuth config not found for provider {Provider}", connection.Provider);
                return accessToken;
            }

            var tokenEndpoint = connection.Provider.ToLower() switch
            {
                "google" => GoogleTokenEndpoint,
                "outlook" or "microsoft" => MicrosoftTokenEndpoint,
                _ => null
            };

            if (tokenEndpoint == null)
            {
                _logger.LogError("Unknown provider: {Provider}", connection.Provider);
                return accessToken;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            var response = await _httpClient.PostAsync(tokenEndpoint,
                new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refresh token for ConnectionId {ConnectionId}: {Error}",
                    connection.Id, error);

                // Mark connection as needing re-authentication if refresh failed permanently
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    connection.LastSyncStatus = "RefreshTokenExpired";
                    await _context.SaveChangesAsync();
                }

                return accessToken;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            var newAccessToken = tokenResponse.GetProperty("access_token").GetString()!;
            var expiresIn = tokenResponse.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

            // Update connection with new tokens
            connection.AccessToken = _secretEncryptionService.Encrypt(newAccessToken);
            connection.TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

            // Some providers issue new refresh tokens
            if (tokenResponse.TryGetProperty("refresh_token", out var newRefreshToken))
            {
                var newRefreshTokenStr = newRefreshToken.GetString();
                if (!string.IsNullOrEmpty(newRefreshTokenStr))
                {
                    connection.RefreshToken = _secretEncryptionService.Encrypt(newRefreshTokenStr);
                }
            }

            connection.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully refreshed OAuth token for ConnectionId {ConnectionId}, Provider {Provider}",
                connection.Id, connection.Provider);

            return newAccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing OAuth token for ConnectionId {ConnectionId}", connection.Id);
            return accessToken;
        }
    }

    private async Task RevokeTokensAsync(CalendarConnection connection)
    {
        // Provider-specific token revocation
        _logger.LogInformation("Revoking tokens for connection {ConnectionId}", connection.Id);
    }

    private async Task<IEnumerable<AvailableCalendarDto>> GetGoogleCalendarsAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetFromJsonAsync<JsonElement>($"{GoogleCalendarApiBase}/users/me/calendarList");

        var calendars = new List<AvailableCalendarDto>();
        if (response.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                calendars.Add(new AvailableCalendarDto
                {
                    CalendarId = item.GetProperty("id").GetString()!,
                    Name = item.GetProperty("summary").GetString()!,
                    IsPrimary = item.TryGetProperty("primary", out var p) && p.GetBoolean(),
                    Color = item.TryGetProperty("backgroundColor", out var bg) ? bg.GetString() : null
                });
            }
        }

        return calendars;
    }

    private async Task<IEnumerable<AvailableCalendarDto>> GetOutlookCalendarsAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetFromJsonAsync<JsonElement>($"{MicrosoftGraphApiBase}/me/calendars");

        var calendars = new List<AvailableCalendarDto>();
        if (response.TryGetProperty("value", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                calendars.Add(new AvailableCalendarDto
                {
                    CalendarId = item.GetProperty("id").GetString()!,
                    Name = item.GetProperty("name").GetString()!,
                    IsPrimary = item.TryGetProperty("isDefaultCalendar", out var d) && d.GetBoolean(),
                    Color = item.TryGetProperty("color", out var c) ? c.GetString() : null
                });
            }
        }

        return calendars;
    }

    private async Task<IEnumerable<CalendarEventDto>> GetGoogleEventsAsync(
        string accessToken, string calendarId, DateTime startDate, DateTime endDate)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"{GoogleCalendarApiBase}/calendars/{Uri.EscapeDataString(calendarId)}/events" +
            $"?timeMin={startDate:O}&timeMax={endDate:O}&singleEvents=true";

        var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
        var events = new List<CalendarEventDto>();

        if (response.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                events.Add(ParseGoogleEvent(item));
            }
        }

        return events;
    }

    private async Task<IEnumerable<CalendarEventDto>> GetOutlookEventsAsync(
        string accessToken, string? calendarId, DateTime startDate, DateTime endDate)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var baseUrl = string.IsNullOrEmpty(calendarId)
            ? $"{MicrosoftGraphApiBase}/me/events"
            : $"{MicrosoftGraphApiBase}/me/calendars/{calendarId}/events";

        var url = $"{baseUrl}?$filter=start/dateTime ge '{startDate:O}' and end/dateTime le '{endDate:O}'";

        var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
        var events = new List<CalendarEventDto>();

        if (response.TryGetProperty("value", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                events.Add(ParseOutlookEvent(item));
            }
        }

        return events;
    }

    private async Task<CalendarEventDto?> CreateGoogleEventAsync(
        string accessToken, string calendarId, CreateCalendarEventDto eventDto)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var googleEvent = MapToGoogleEvent(eventDto);
        var response = await _httpClient.PostAsJsonAsync(
            $"{GoogleCalendarApiBase}/calendars/{Uri.EscapeDataString(calendarId)}/events",
            googleEvent);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return ParseGoogleEvent(result);
    }

    private async Task<CalendarEventDto?> CreateOutlookEventAsync(
        string accessToken, string? calendarId, CreateCalendarEventDto eventDto)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var outlookEvent = MapToOutlookEvent(eventDto);
        var url = string.IsNullOrEmpty(calendarId)
            ? $"{MicrosoftGraphApiBase}/me/events"
            : $"{MicrosoftGraphApiBase}/me/calendars/{calendarId}/events";

        var response = await _httpClient.PostAsJsonAsync(url, outlookEvent);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return ParseOutlookEvent(result);
    }

    private async Task<CalendarEventDto?> UpdateGoogleEventAsync(
        string accessToken, string calendarId, string eventId, CreateCalendarEventDto eventDto)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var googleEvent = MapToGoogleEvent(eventDto);
        var response = await _httpClient.PutAsJsonAsync(
            $"{GoogleCalendarApiBase}/calendars/{Uri.EscapeDataString(calendarId)}/events/{eventId}",
            googleEvent);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return ParseGoogleEvent(result);
    }

    private async Task<CalendarEventDto?> UpdateOutlookEventAsync(
        string accessToken, string eventId, CreateCalendarEventDto eventDto)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var outlookEvent = MapToOutlookEvent(eventDto);
        var response = await _httpClient.PatchAsync(
            $"{MicrosoftGraphApiBase}/me/events/{eventId}",
            new StringContent(JsonSerializer.Serialize(outlookEvent), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return ParseOutlookEvent(result);
    }

    private async Task<bool> DeleteGoogleEventAsync(string accessToken, string calendarId, string eventId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.DeleteAsync(
            $"{GoogleCalendarApiBase}/calendars/{Uri.EscapeDataString(calendarId)}/events/{eventId}");
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> DeleteOutlookEventAsync(string accessToken, string eventId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.DeleteAsync($"{MicrosoftGraphApiBase}/me/events/{eventId}");
        return response.IsSuccessStatusCode;
    }

    private async Task<CalendarEventDto?> GetGoogleEventAsync(string accessToken, string calendarId, string eventId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{GoogleCalendarApiBase}/calendars/{Uri.EscapeDataString(calendarId)}/events/{eventId}");
            return ParseGoogleEvent(response);
        }
        catch
        {
            return null;
        }
    }

    private async Task<CalendarEventDto?> GetOutlookEventAsync(string accessToken, string eventId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{MicrosoftGraphApiBase}/me/events/{eventId}");
            return ParseOutlookEvent(response);
        }
        catch
        {
            return null;
        }
    }

    private static CalendarEventDto ParseGoogleEvent(JsonElement item)
    {
        var eventDto = new CalendarEventDto
        {
            ExternalId = item.GetProperty("id").GetString(),
            Title = item.TryGetProperty("summary", out var s) ? s.GetString()! : "",
            Description = item.TryGetProperty("description", out var d) ? d.GetString() : null
        };

        if (item.TryGetProperty("start", out var start))
        {
            if (start.TryGetProperty("dateTime", out var dt))
            {
                eventDto.StartDateTime = DateTime.Parse(dt.GetString()!);
            }
            else if (start.TryGetProperty("date", out var date))
            {
                eventDto.StartDateTime = DateTime.Parse(date.GetString()!);
                eventDto.IsAllDay = true;
            }
        }

        if (item.TryGetProperty("end", out var end))
        {
            if (end.TryGetProperty("dateTime", out var dt))
            {
                eventDto.EndDateTime = DateTime.Parse(dt.GetString()!);
            }
            else if (end.TryGetProperty("date", out var date))
            {
                eventDto.EndDateTime = DateTime.Parse(date.GetString()!);
            }
        }

        return eventDto;
    }

    private static CalendarEventDto ParseOutlookEvent(JsonElement item)
    {
        var eventDto = new CalendarEventDto
        {
            ExternalId = item.GetProperty("id").GetString(),
            Title = item.GetProperty("subject").GetString()!,
            Description = item.TryGetProperty("bodyPreview", out var b) ? b.GetString() : null
        };

        if (item.TryGetProperty("start", out var start))
        {
            eventDto.StartDateTime = DateTime.Parse(start.GetProperty("dateTime").GetString()!);
        }

        if (item.TryGetProperty("end", out var end))
        {
            eventDto.EndDateTime = DateTime.Parse(end.GetProperty("dateTime").GetString()!);
        }

        eventDto.IsAllDay = item.TryGetProperty("isAllDay", out var allDay) && allDay.GetBoolean();

        return eventDto;
    }

    private static object MapToGoogleEvent(CreateCalendarEventDto eventDto)
    {
        return new
        {
            summary = eventDto.Title,
            description = eventDto.Description,
            location = eventDto.Location,
            start = eventDto.IsAllDay
                ? new { date = eventDto.StartDateTime.ToString("yyyy-MM-dd") }
                : new { dateTime = eventDto.StartDateTime.ToString("O"), timeZone = eventDto.TimeZone ?? "UTC" } as object,
            end = eventDto.IsAllDay
                ? new { date = eventDto.EndDateTime.ToString("yyyy-MM-dd") }
                : new { dateTime = eventDto.EndDateTime.ToString("O"), timeZone = eventDto.TimeZone ?? "UTC" } as object,
            attendees = eventDto.AttendeeEmails?.Select(e => new { email = e }).ToList()
        };
    }

    private static object MapToOutlookEvent(CreateCalendarEventDto eventDto)
    {
        return new
        {
            subject = eventDto.Title,
            body = new { contentType = "Text", content = eventDto.Description },
            start = new { dateTime = eventDto.StartDateTime.ToString("O"), timeZone = eventDto.TimeZone ?? "UTC" },
            end = new { dateTime = eventDto.EndDateTime.ToString("O"), timeZone = eventDto.TimeZone ?? "UTC" },
            location = new { displayName = eventDto.Location },
            isAllDay = eventDto.IsAllDay,
            attendees = eventDto.AttendeeEmails?.Select(e => new
            {
                emailAddress = new { address = e },
                type = "required"
            }).ToList()
        };
    }

    private static CreateCalendarEventDto MapAppointmentToEvent(
        Appointment appointment, CalendarSyncSettingsDto settings)
    {
        var title = settings.IncludePatientDetails
            ? $"Appointment: {appointment.Patient?.FirstName} {appointment.Patient?.LastName}"
            : "Patient Appointment";

        var description = settings.IncludeNotes ? appointment.Notes : null;

        return new CreateCalendarEventDto
        {
            Title = title,
            Description = description,
            StartDateTime = appointment.StartTime,
            EndDateTime = appointment.EndTime,
            Location = appointment.Location,
            CreateConference = settings.CreateConferenceLinks && appointment.IsTelemedicine,
            ConferenceProvider = settings.DefaultConferenceProvider
        };
    }

    private static CalendarConnectionDto MapToConnectionDto(CalendarConnection connection)
    {
        return new CalendarConnectionDto
        {
            Id = connection.Id,
            Provider = connection.Provider,
            AccountEmail = connection.AccountEmail,
            AccountName = connection.AccountName,
            CalendarId = connection.CalendarId,
            CalendarName = connection.CalendarName,
            CalendarColor = connection.CalendarColor,
            SyncDirection = Enum.TryParse<CalendarSyncDirectionDto>(connection.SyncDirection, out var dir)
                ? dir
                : CalendarSyncDirectionDto.TwoWay,
            IsActive = connection.IsActive,
            IsDefault = connection.IsDefault,
            LastSyncAt = connection.LastSyncAt,
            LastSyncStatus = connection.LastSyncStatus,
            LastSyncEventCount = connection.LastSyncEventCount,
            ConnectedAt = connection.ConnectedAt
        };
    }

    private static CalendarSyncSettingsDto MapToSyncSettingsDto(CalendarSyncSettings settings)
    {
        return new CalendarSyncSettingsDto
        {
            AutoSyncEnabled = settings.AutoSyncEnabled,
            SyncIntervalMinutes = settings.SyncIntervalMinutes,
            DefaultSyncDirection = Enum.TryParse<CalendarSyncDirectionDto>(settings.DefaultSyncDirection, out var dir)
                ? dir
                : CalendarSyncDirectionDto.TwoWay,
            SyncAppointments = settings.SyncAppointments,
            SyncBlockedTime = settings.SyncBlockedTime,
            SyncMeetings = settings.SyncMeetings,
            CreateConferenceLinks = settings.CreateConferenceLinks,
            DefaultConferenceProvider = settings.DefaultConferenceProvider,
            IncludePatientDetails = settings.IncludePatientDetails,
            IncludeNotes = settings.IncludeNotes,
            SyncWindowDaysPast = settings.SyncWindowDaysPast,
            SyncWindowDaysFuture = settings.SyncWindowDaysFuture
        };
    }

    #endregion
}

#region Calendar Sync Entities

public class CalendarConnection
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? AccountEmail { get; set; }
    public string? AccountName { get; set; }
    public string? CalendarId { get; set; }
    public string? CalendarName { get; set; }
    public string? CalendarColor { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public string SyncDirection { get; set; } = "TwoWay";
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public int? LastSyncEventCount { get; set; }

    public ApplicationUser? User { get; set; }
}

public class CalendarSyncSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool AutoSyncEnabled { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 15;
    public string DefaultSyncDirection { get; set; } = "TwoWay";
    public bool SyncAppointments { get; set; } = true;
    public bool SyncBlockedTime { get; set; } = true;
    public bool SyncMeetings { get; set; } = true;
    public bool CreateConferenceLinks { get; set; }
    public string? DefaultConferenceProvider { get; set; }
    public bool IncludePatientDetails { get; set; }
    public bool IncludeNotes { get; set; }
    public int SyncWindowDaysPast { get; set; } = 30;
    public int SyncWindowDaysFuture { get; set; } = 90;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }
}

public class AppointmentCalendarMapping
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int ConnectionId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string? ExternalCalendarId { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public string? SyncStatus { get; set; }

    public Appointment? Appointment { get; set; }
    public CalendarConnection? Connection { get; set; }
}

public class CalendarSyncHistoryEntry
{
    public int Id { get; set; }
    public int ConnectionId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EventsCreated { get; set; }
    public int EventsUpdated { get; set; }
    public int EventsDeleted { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }

    public CalendarConnection? Connection { get; set; }
}

public class CalendarSyncConflict
{
    public int Id { get; set; }
    public int ConnectionId { get; set; }
    public string EventId { get; set; } = string.Empty;
    public int? LocalAppointmentId { get; set; }
    public string? ExternalEventId { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public string? LocalTitle { get; set; }
    public DateTime? LocalStartTime { get; set; }
    public DateTime? LocalEndTime { get; set; }
    public string? ExternalTitle { get; set; }
    public DateTime? ExternalStartTime { get; set; }
    public DateTime? ExternalEndTime { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OAuthConfig
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public bool IsActive { get; set; }
}

public class OAuthState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string State { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

#endregion
