using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Push Notification operations
/// Supports FCM (Firebase Cloud Messaging), APNs (Apple Push Notification Service), and Web Push
/// </summary>
public interface IPushNotificationService
{
    #region Device Registration

    /// <summary>
    /// Register a device for push notifications
    /// </summary>
    Task<DeviceRegistrationResponseDto> RegisterDeviceAsync(int? userId, int? patientId, DeviceRegistrationDto dto);

    /// <summary>
    /// Update device registration
    /// </summary>
    Task<DeviceRegistrationResponseDto> UpdateDeviceAsync(int deviceId, DeviceRegistrationDto dto);

    /// <summary>
    /// Unregister a device
    /// </summary>
    Task<bool> UnregisterDeviceAsync(int deviceId);

    /// <summary>
    /// Unregister device by token
    /// </summary>
    Task<bool> UnregisterDeviceByTokenAsync(string deviceToken);

    /// <summary>
    /// Get registered devices for a user
    /// </summary>
    Task<IEnumerable<RegisteredDeviceDto>> GetUserDevicesAsync(int userId);

    /// <summary>
    /// Get registered devices for a patient
    /// </summary>
    Task<IEnumerable<RegisteredDeviceDto>> GetPatientDevicesAsync(int patientId);

    #endregion

    #region Send Notifications

    /// <summary>
    /// Send push notification
    /// </summary>
    Task<PushNotificationResponseDto> SendNotificationAsync(PushNotificationRequestDto request);

    /// <summary>
    /// Send push notification to a specific device token
    /// </summary>
    Task<PushNotificationResponseDto> SendToDeviceAsync(string deviceToken, PushNotificationRequestDto request);

    /// <summary>
    /// Send push notification to multiple device tokens
    /// </summary>
    Task<PushNotificationResponseDto> SendToDevicesAsync(IEnumerable<string> deviceTokens, PushNotificationRequestDto request);

    /// <summary>
    /// Send push notification to a topic
    /// </summary>
    Task<PushNotificationResponseDto> SendToTopicAsync(string topic, PushNotificationRequestDto request);

    /// <summary>
    /// Send templated notification
    /// </summary>
    Task<PushNotificationResponseDto> SendTemplatedNotificationAsync(SendTemplatedNotificationDto request);

    /// <summary>
    /// Schedule a notification for later delivery
    /// </summary>
    Task<int> ScheduleNotificationAsync(PushNotificationRequestDto request, DateTime scheduledFor);

    /// <summary>
    /// Cancel a scheduled notification
    /// </summary>
    Task<bool> CancelScheduledNotificationAsync(int scheduledNotificationId);

    /// <summary>
    /// Process scheduled notifications (called by background job)
    /// </summary>
    Task ProcessScheduledNotificationsAsync();

    #endregion

    #region Clinic-Specific Notifications

    /// <summary>
    /// Send appointment reminder notification
    /// </summary>
    Task<PushNotificationResponseDto> SendAppointmentReminderAsync(AppointmentReminderNotificationDto dto);

    /// <summary>
    /// Send lab result ready notification
    /// </summary>
    Task<PushNotificationResponseDto> SendLabResultNotificationAsync(LabResultNotificationDto dto);

    /// <summary>
    /// Send prescription refill reminder
    /// </summary>
    Task<PushNotificationResponseDto> SendRefillReminderAsync(RefillReminderNotificationDto dto);

    /// <summary>
    /// Send invoice notification
    /// </summary>
    Task<PushNotificationResponseDto> SendInvoiceNotificationAsync(InvoiceNotificationDto dto);

    /// <summary>
    /// Send staff notification
    /// </summary>
    Task<PushNotificationResponseDto> SendStaffNotificationAsync(StaffNotificationDto dto);

    /// <summary>
    /// Send bulk appointment reminders (for scheduled job)
    /// </summary>
    Task<int> SendBulkAppointmentRemindersAsync(int minutesBefore = 60);

    #endregion

    #region Topics

    /// <summary>
    /// Create a notification topic
    /// </summary>
    Task<NotificationTopicDto> CreateTopicAsync(int branchId, string name, string? description);

    /// <summary>
    /// Get all topics
    /// </summary>
    Task<IEnumerable<NotificationTopicDto>> GetTopicsAsync(int branchId);

    /// <summary>
    /// Subscribe device to topic
    /// </summary>
    Task<TopicSubscriptionResponseDto> SubscribeToTopicAsync(TopicSubscriptionDto dto);

    /// <summary>
    /// Unsubscribe device from topic
    /// </summary>
    Task<TopicSubscriptionResponseDto> UnsubscribeFromTopicAsync(TopicSubscriptionDto dto);

    /// <summary>
    /// Get device topic subscriptions
    /// </summary>
    Task<IEnumerable<string>> GetDeviceTopicsAsync(string deviceToken);

    #endregion

    #region Templates

    /// <summary>
    /// Create notification template
    /// </summary>
    Task<NotificationTemplateDto> CreateTemplateAsync(int branchId, CreateNotificationTemplateDto dto);

    /// <summary>
    /// Update notification template
    /// </summary>
    Task<NotificationTemplateDto> UpdateTemplateAsync(int templateId, CreateNotificationTemplateDto dto);

    /// <summary>
    /// Get notification template
    /// </summary>
    Task<NotificationTemplateDto?> GetTemplateAsync(int templateId);

    /// <summary>
    /// Get notification template by code
    /// </summary>
    Task<NotificationTemplateDto?> GetTemplateByCodeAsync(int branchId, string code);

    /// <summary>
    /// Get all notification templates
    /// </summary>
    Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(int branchId);

    /// <summary>
    /// Delete notification template
    /// </summary>
    Task<bool> DeleteTemplateAsync(int templateId);

    #endregion

    #region History & Analytics

    /// <summary>
    /// Get notification history
    /// </summary>
    Task<(IEnumerable<NotificationHistoryDto> Items, int TotalCount)> GetNotificationHistoryAsync(
        int branchId, NotificationHistoryFilterDto filter);

    /// <summary>
    /// Get notification statistics
    /// </summary>
    Task<NotificationStatisticsDto> GetStatisticsAsync(int branchId, DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Get scheduled notifications
    /// </summary>
    Task<(IEnumerable<ScheduledNotificationDto> Items, int TotalCount)> GetScheduledNotificationsAsync(
        int branchId, ScheduledNotificationFilterDto filter);

    #endregion

    #region Configuration

    /// <summary>
    /// Get push notification configuration status
    /// </summary>
    Task<PushConfigurationStatusDto> GetConfigurationStatusAsync(int branchId);

    /// <summary>
    /// Update FCM configuration
    /// </summary>
    Task<bool> UpdateFcmConfigurationAsync(int branchId, UpdateFcmConfigurationDto dto);

    /// <summary>
    /// Update APNs configuration
    /// </summary>
    Task<bool> UpdateApnsConfigurationAsync(int branchId, UpdateApnsConfigurationDto dto);

    /// <summary>
    /// Update Web Push configuration
    /// </summary>
    Task<bool> UpdateWebPushConfigurationAsync(int branchId, UpdateWebPushConfigurationDto dto);

    /// <summary>
    /// Test push notification configuration
    /// </summary>
    Task<PushNotificationResponseDto> TestConfigurationAsync(int branchId, string deviceToken);

    #endregion
}
