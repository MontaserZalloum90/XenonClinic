using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace XenonClinic.Infrastructure.Hubs;

/// <summary>
/// Real-time notification hub for clinic events.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _groupMembers = new();

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }
                _userConnections[userId].Add(connectionId);
            }

            // Add to user-specific group
            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.Remove(userId);
                    }
                }
            }

            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a room for real-time updates (e.g., branch, department).
    /// </summary>
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        _logger.LogDebug("Connection {ConnectionId} joined room {RoomId}", Context.ConnectionId, roomId);
    }

    /// <summary>
    /// Leave a room.
    /// </summary>
    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        _logger.LogDebug("Connection {ConnectionId} left room {RoomId}", Context.ConnectionId, roomId);
    }

    /// <summary>
    /// Subscribe to appointment updates for a specific provider.
    /// </summary>
    public async Task SubscribeToProviderSchedule(int providerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"provider_{providerId}");
    }

    /// <summary>
    /// Subscribe to branch updates.
    /// </summary>
    public async Task SubscribeToBranch(int branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"branch_{branchId}");
    }

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    public async Task MarkNotificationRead(string notificationId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        // In production: Update notification status in database
        await Clients.User(userId).SendAsync("NotificationRead", notificationId);
    }

    /// <summary>
    /// Get online status of users.
    /// </summary>
    public Task<bool> IsUserOnline(string userId)
    {
        lock (_userConnections)
        {
            return Task.FromResult(_userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0);
        }
    }
}

/// <summary>
/// Service for sending real-time notifications via SignalR.
/// </summary>
public interface INotificationService
{
    Task SendToUserAsync(string userId, string eventName, object data);
    Task SendToUsersAsync(IEnumerable<string> userIds, string eventName, object data);
    Task SendToGroupAsync(string groupId, string eventName, object data);
    Task SendToBranchAsync(int branchId, string eventName, object data);
    Task SendToProviderAsync(int providerId, string eventName, object data);
    Task BroadcastAsync(string eventName, object data);
}

/// <summary>
/// SignalR-based notification service implementation.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(string userId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync(eventName, data);
        _logger.LogDebug("Sent {Event} to user {UserId}", eventName, userId);
    }

    public async Task SendToUsersAsync(IEnumerable<string> userIds, string eventName, object data)
    {
        var tasks = userIds.Select(userId => SendToUserAsync(userId, eventName, data));
        await Task.WhenAll(tasks);
    }

    public async Task SendToGroupAsync(string groupId, string eventName, object data)
    {
        await _hubContext.Clients.Group(groupId).SendAsync(eventName, data);
        _logger.LogDebug("Sent {Event} to group {GroupId}", eventName, groupId);
    }

    public async Task SendToBranchAsync(int branchId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"branch_{branchId}").SendAsync(eventName, data);
    }

    public async Task SendToProviderAsync(int providerId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"provider_{providerId}").SendAsync(eventName, data);
    }

    public async Task BroadcastAsync(string eventName, object data)
    {
        await _hubContext.Clients.All.SendAsync(eventName, data);
        _logger.LogDebug("Broadcast {Event} to all clients", eventName);
    }
}

/// <summary>
/// Notification event types.
/// </summary>
public static class NotificationEvents
{
    public const string AppointmentCreated = "AppointmentCreated";
    public const string AppointmentUpdated = "AppointmentUpdated";
    public const string AppointmentCancelled = "AppointmentCancelled";
    public const string AppointmentReminder = "AppointmentReminder";

    public const string PatientCheckedIn = "PatientCheckedIn";
    public const string PatientCalled = "PatientCalled";

    public const string LabResultReady = "LabResultReady";
    public const string RadiologyResultReady = "RadiologyResultReady";

    public const string MessageReceived = "MessageReceived";
    public const string TaskAssigned = "TaskAssigned";
    public const string TaskCompleted = "TaskCompleted";

    public const string AlertCreated = "AlertCreated";
    public const string SystemNotification = "SystemNotification";
}
