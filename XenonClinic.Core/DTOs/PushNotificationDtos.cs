namespace XenonClinic.Core.DTOs;

#region Push Notification DTOs

/// <summary>
/// Push notification request DTO
/// </summary>
public class PushNotificationRequestDto
{
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? PatientIds { get; set; }
    public string? Topic { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public PushNotificationPriorityDto Priority { get; set; } = PushNotificationPriorityDto.Normal;
    public int? TimeToLiveSeconds { get; set; }
    public string? Sound { get; set; }
    public int? Badge { get; set; }
    public string? ClickAction { get; set; }
    public string? ActionUrl { get; set; }
    public bool Silent { get; set; }
    public DateTime? ScheduledFor { get; set; }
}

/// <summary>
/// Push notification priority
/// </summary>
public enum PushNotificationPriorityDto
{
    Low,
    Normal,
    High
}

/// <summary>
/// Push notification response DTO
/// </summary>
public class PushNotificationResponseDto
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<PushNotificationResultDto>? Results { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Individual push notification result
/// </summary>
public class PushNotificationResultDto
{
    public string? DeviceToken { get; set; }
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Device registration DTO
/// </summary>
public class DeviceRegistrationDto
{
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // iOS, Android, Web
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceModel { get; set; }
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
    public string? PushEndpoint { get; set; } // For Web Push
    public string? P256dh { get; set; } // For Web Push
    public string? Auth { get; set; } // For Web Push
}

/// <summary>
/// Device registration response
/// </summary>
public class DeviceRegistrationResponseDto
{
    public bool Success { get; set; }
    public int? DeviceId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Registered device DTO
/// </summary>
public class RegisteredDeviceDto
{
    public int Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceModel { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Notification Template DTOs

/// <summary>
/// Notification template DTO
/// </summary>
public class NotificationTemplateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ClickAction { get; set; }
    public Dictionary<string, string>? DefaultData { get; set; }
    public bool IsActive { get; set; }
    public List<NotificationTemplateLocalizationDto>? Localizations { get; set; }
}

/// <summary>
/// Notification template localization
/// </summary>
public class NotificationTemplateLocalizationDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
}

/// <summary>
/// Create/update notification template request
/// </summary>
public class CreateNotificationTemplateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ClickAction { get; set; }
    public Dictionary<string, string>? DefaultData { get; set; }
    public List<NotificationTemplateLocalizationDto>? Localizations { get; set; }
}

/// <summary>
/// Send templated notification request
/// </summary>
public class SendTemplatedNotificationDto
{
    public string TemplateCode { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? PatientIds { get; set; }
    public string? Topic { get; set; }
    public Dictionary<string, string>? TemplateVariables { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
    public string? LanguageCode { get; set; }
    public DateTime? ScheduledFor { get; set; }
}

#endregion

#region Notification Topic DTOs

/// <summary>
/// Notification topic DTO
/// </summary>
public class NotificationTopicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubscriberCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Topic subscription request
/// </summary>
public class TopicSubscriptionDto
{
    public string TopicName { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
}

/// <summary>
/// Topic subscription response
/// </summary>
public class TopicSubscriptionResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

#endregion

#region Notification History DTOs

/// <summary>
/// Notification history DTO
/// </summary>
public class NotificationHistoryDto
{
    public int Id { get; set; }
    public string? MessageId { get; set; }
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public string? Topic { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public string Status { get; set; } = string.Empty; // Sent, Delivered, Failed, Scheduled
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Notification history filter
/// </summary>
public class NotificationHistoryFilterDto
{
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Notification statistics DTO
/// </summary>
public class NotificationStatisticsDto
{
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalFailed { get; set; }
    public int TotalScheduled { get; set; }
    public decimal DeliveryRate { get; set; }
    public List<NotificationStatsByDayDto>? DailyStats { get; set; }
    public List<NotificationStatsByPlatformDto>? PlatformStats { get; set; }
}

/// <summary>
/// Daily notification statistics
/// </summary>
public class NotificationStatsByDayDto
{
    public DateTime Date { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
}

/// <summary>
/// Platform notification statistics
/// </summary>
public class NotificationStatsByPlatformDto
{
    public string Platform { get; set; } = string.Empty;
    public int DeviceCount { get; set; }
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public decimal DeliveryRate { get; set; }
}

#endregion

#region Push Notification Configuration DTOs

/// <summary>
/// FCM configuration DTO
/// </summary>
public class FcmConfigurationDto
{
    public string? ProjectId { get; set; }
    public string? PrivateKeyId { get; set; }
    public string? ClientEmail { get; set; }
    public bool IsConfigured { get; set; }
}

/// <summary>
/// APNs configuration DTO
/// </summary>
public class ApnsConfigurationDto
{
    public string? TeamId { get; set; }
    public string? KeyId { get; set; }
    public string? BundleId { get; set; }
    public bool UseSandbox { get; set; }
    public bool IsConfigured { get; set; }
}

/// <summary>
/// Web Push configuration DTO
/// </summary>
public class WebPushConfigurationDto
{
    public string? VapidPublicKey { get; set; }
    public string? Subject { get; set; }
    public bool IsConfigured { get; set; }
}

/// <summary>
/// Push notification configuration status
/// </summary>
public class PushConfigurationStatusDto
{
    public FcmConfigurationDto? Fcm { get; set; }
    public ApnsConfigurationDto? Apns { get; set; }
    public WebPushConfigurationDto? WebPush { get; set; }
}

/// <summary>
/// Update FCM configuration request
/// </summary>
public class UpdateFcmConfigurationDto
{
    public string ProjectId { get; set; } = string.Empty;
    public string PrivateKeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}

/// <summary>
/// Update APNs configuration request
/// </summary>
public class UpdateApnsConfigurationDto
{
    public string TeamId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string BundleId { get; set; } = string.Empty;
    public bool UseSandbox { get; set; }
}

/// <summary>
/// Update Web Push configuration request
/// </summary>
public class UpdateWebPushConfigurationDto
{
    public string VapidPublicKey { get; set; } = string.Empty;
    public string VapidPrivateKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

#endregion

#region Scheduled Notification DTOs

/// <summary>
/// Scheduled notification DTO
/// </summary>
public class ScheduledNotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public string? Topic { get; set; }
    public DateTime ScheduledFor { get; set; }
    public string Status { get; set; } = string.Empty; // Scheduled, Sent, Cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

/// <summary>
/// Scheduled notification filter
/// </summary>
public class ScheduledNotificationFilterDto
{
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region Clinic Notification Types

/// <summary>
/// Appointment reminder notification
/// </summary>
public class AppointmentReminderNotificationDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDateTime { get; set; }
    public string? Location { get; set; }
    public int ReminderMinutesBefore { get; set; } = 60;
}

/// <summary>
/// Lab result notification
/// </summary>
public class LabResultNotificationDto
{
    public int LabOrderId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public bool IsAbnormal { get; set; }
}

/// <summary>
/// Prescription refill notification
/// </summary>
public class RefillReminderNotificationDto
{
    public int PrescriptionId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

/// <summary>
/// Invoice notification
/// </summary>
public class InvoiceNotificationDto
{
    public int InvoiceId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
}

/// <summary>
/// Staff notification DTO
/// </summary>
public class StaffNotificationDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty; // TaskAssigned, MessageReceived, Alert, etc.
    public string? ActionUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

#endregion
