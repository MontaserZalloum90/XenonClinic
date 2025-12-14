using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Push Notifications
/// Supports FCM (Firebase Cloud Messaging), APNs (Apple Push Notification Service), and Web Push
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly HttpClient _httpClient;

    public PushNotificationService(
        ClinicDbContext context,
        ILogger<PushNotificationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("PushNotification");
    }

    #region Device Registration

    public async Task<DeviceRegistrationResponseDto> RegisterDeviceAsync(
        int? userId, int? patientId, DeviceRegistrationDto dto)
    {
        try
        {
            // Check if device already exists
            var existingDevice = await _context.PushDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == dto.DeviceToken);

            if (existingDevice != null)
            {
                // Update existing device
                existingDevice.UserId = userId;
                existingDevice.PatientId = patientId;
                existingDevice.Platform = dto.Platform;
                existingDevice.DeviceName = dto.DeviceName;
                existingDevice.DeviceModel = dto.DeviceModel;
                existingDevice.OsVersion = dto.OsVersion;
                existingDevice.AppVersion = dto.AppVersion;
                existingDevice.LastActiveAt = DateTime.UtcNow;
                existingDevice.IsActive = true;

                await _context.SaveChangesAsync();

                return new DeviceRegistrationResponseDto
                {
                    Success = true,
                    DeviceId = existingDevice.Id,
                    Message = "Device updated successfully"
                };
            }

            var device = new PushDevice
            {
                UserId = userId,
                PatientId = patientId,
                DeviceToken = dto.DeviceToken,
                Platform = dto.Platform,
                DeviceId = dto.DeviceId,
                DeviceName = dto.DeviceName,
                DeviceModel = dto.DeviceModel,
                OsVersion = dto.OsVersion,
                AppVersion = dto.AppVersion,
                PushEndpoint = dto.PushEndpoint,
                P256dh = dto.P256dh,
                Auth = dto.Auth,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            _context.PushDevices.Add(device);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Device registered: {DeviceId}, Platform: {Platform}",
                device.Id, device.Platform);

            return new DeviceRegistrationResponseDto
            {
                Success = true,
                DeviceId = device.Id,
                Message = "Device registered successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device");
            return new DeviceRegistrationResponseDto
            {
                Success = false,
                Message = "Error registering device"
            };
        }
    }

    public async Task<DeviceRegistrationResponseDto> UpdateDeviceAsync(int deviceId, DeviceRegistrationDto dto)
    {
        var device = await _context.PushDevices.FindAsync(deviceId);
        if (device == null)
        {
            return new DeviceRegistrationResponseDto
            {
                Success = false,
                Message = "Device not found"
            };
        }

        device.DeviceToken = dto.DeviceToken;
        device.Platform = dto.Platform;
        device.DeviceName = dto.DeviceName;
        device.DeviceModel = dto.DeviceModel;
        device.OsVersion = dto.OsVersion;
        device.AppVersion = dto.AppVersion;
        device.LastActiveAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DeviceRegistrationResponseDto
        {
            Success = true,
            DeviceId = device.Id,
            Message = "Device updated successfully"
        };
    }

    public async Task<bool> UnregisterDeviceAsync(int deviceId)
    {
        var device = await _context.PushDevices.FindAsync(deviceId);
        if (device == null)
            return false;

        device.IsActive = false;
        device.UnregisteredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnregisterDeviceByTokenAsync(string deviceToken)
    {
        var device = await _context.PushDevices
            .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken);

        if (device == null)
            return false;

        device.IsActive = false;
        device.UnregisteredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RegisteredDeviceDto>> GetUserDevicesAsync(int userId)
    {
        var devices = await _context.PushDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .OrderByDescending(d => d.LastActiveAt)
            .ToListAsync();

        return devices.Select(MapToRegisteredDeviceDto);
    }

    public async Task<IEnumerable<RegisteredDeviceDto>> GetPatientDevicesAsync(int patientId)
    {
        var devices = await _context.PushDevices
            .Where(d => d.PatientId == patientId && d.IsActive)
            .OrderByDescending(d => d.LastActiveAt)
            .ToListAsync();

        return devices.Select(MapToRegisteredDeviceDto);
    }

    #endregion

    #region Send Notifications

    /// <summary>
    /// BUG FIX: Optimized to use batch queries instead of N+1 queries for user/patient IDs.
    /// </summary>
    public async Task<PushNotificationResponseDto> SendNotificationAsync(PushNotificationRequestDto request)
    {
        var deviceTokens = new List<string>();

        // BUG FIX: Collect all user IDs to batch load instead of N+1 queries
        var allUserIds = new List<int>();
        if (request.UserId.HasValue)
        {
            allUserIds.Add(request.UserId.Value);
        }
        if (request.UserIds?.Any() == true)
        {
            allUserIds.AddRange(request.UserIds);
        }

        // Batch load device tokens for all users in a single query
        if (allUserIds.Any())
        {
            var userTokens = await GetActiveDeviceTokensForUsersAsync(allUserIds.Distinct());
            deviceTokens.AddRange(userTokens);
        }

        // BUG FIX: Collect all patient IDs to batch load instead of N+1 queries
        var allPatientIds = new List<int>();
        if (request.PatientId.HasValue)
        {
            allPatientIds.Add(request.PatientId.Value);
        }
        if (request.PatientIds?.Any() == true)
        {
            allPatientIds.AddRange(request.PatientIds);
        }

        // Batch load device tokens for all patients in a single query
        if (allPatientIds.Any())
        {
            var patientTokens = await GetActiveDeviceTokensForPatientsAsync(allPatientIds.Distinct());
            deviceTokens.AddRange(patientTokens);
        }

        if (!string.IsNullOrEmpty(request.Topic))
        {
            return await SendToTopicAsync(request.Topic, request);
        }

        if (!deviceTokens.Any())
        {
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = "No devices found for the specified recipients"
            };
        }

        return await SendToDevicesAsync(deviceTokens.Distinct(), request);
    }

    public async Task<PushNotificationResponseDto> SendToDeviceAsync(
        string deviceToken, PushNotificationRequestDto request)
    {
        return await SendToDevicesAsync(new[] { deviceToken }, request);
    }

    public async Task<PushNotificationResponseDto> SendToDevicesAsync(
        IEnumerable<string> deviceTokens, PushNotificationRequestDto request)
    {
        var tokens = deviceTokens.ToList();
        var results = new List<PushNotificationResultDto>();
        var successCount = 0;
        var failureCount = 0;

        // Group tokens by platform
        var devicesByPlatform = await _context.PushDevices
            .Where(d => tokens.Contains(d.DeviceToken) && d.IsActive)
            .GroupBy(d => d.Platform)
            .Select(g => new { Platform = g.Key, Tokens = g.Select(d => d.DeviceToken).ToList() })
            .ToListAsync();

        foreach (var platformGroup in devicesByPlatform)
        {
            PushNotificationResponseDto platformResult;

            switch (platformGroup.Platform.ToLower())
            {
                case "android":
                    platformResult = await SendViaFcmAsync(platformGroup.Tokens, request);
                    break;
                case "ios":
                    platformResult = await SendViaApnsAsync(platformGroup.Tokens, request);
                    break;
                case "web":
                    platformResult = await SendViaWebPushAsync(platformGroup.Tokens, request);
                    break;
                default:
                    platformResult = await SendViaFcmAsync(platformGroup.Tokens, request);
                    break;
            }

            successCount += platformResult.SuccessCount;
            failureCount += platformResult.FailureCount;

            if (platformResult.Results != null)
            {
                results.AddRange(platformResult.Results);
            }
        }

        // Log notification history
        await LogNotificationAsync(request, tokens.Count, successCount);

        return new PushNotificationResponseDto
        {
            Success = successCount > 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    public async Task<PushNotificationResponseDto> SendToTopicAsync(string topic, PushNotificationRequestDto request)
    {
        try
        {
            // Send via FCM topic messaging
            var payload = new
            {
                message = new
                {
                    topic,
                    notification = new
                    {
                        title = request.Title,
                        body = request.Body,
                        image = request.ImageUrl
                    },
                    data = request.Data,
                    android = new
                    {
                        priority = request.Priority == PushNotificationPriorityDto.High ? "high" : "normal",
                        notification = new
                        {
                            sound = request.Sound ?? "default",
                            click_action = request.ClickAction
                        }
                    },
                    apns = new
                    {
                        payload = new
                        {
                            aps = new
                            {
                                sound = request.Sound ?? "default",
                                badge = request.Badge
                            }
                        }
                    }
                }
            };

            // TODO: Make actual FCM API call
            _logger.LogInformation("Sending notification to topic: {Topic}", topic);

            await LogNotificationAsync(request, 0, 1, topic);

            return new PushNotificationResponseDto
            {
                Success = true,
                MessageId = Guid.NewGuid().ToString(),
                SuccessCount = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to topic: {Topic}", topic);
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<PushNotificationResponseDto> SendTemplatedNotificationAsync(SendTemplatedNotificationDto request)
    {
        var template = await _context.NotificationTemplates
            .Include(t => t.Localizations)
            .FirstOrDefaultAsync(t => t.Code == request.TemplateCode && t.IsActive);

        if (template == null)
        {
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = "Template not found"
            };
        }

        var titleTemplate = template.TitleTemplate;
        var bodyTemplate = template.BodyTemplate;

        // Use localized version if available
        if (!string.IsNullOrEmpty(request.LanguageCode) && template.Localizations?.Any() == true)
        {
            var localization = template.Localizations
                .FirstOrDefault(l => l.LanguageCode == request.LanguageCode);

            if (localization != null)
            {
                titleTemplate = localization.TitleTemplate;
                bodyTemplate = localization.BodyTemplate;
            }
        }

        // Replace template variables
        var title = ReplaceTemplateVariables(titleTemplate, request.TemplateVariables);
        var body = ReplaceTemplateVariables(bodyTemplate, request.TemplateVariables);

        // Merge data
        var data = new Dictionary<string, string>();
        if (template.DefaultData != null)
        {
            foreach (var kvp in template.DefaultData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        if (request.AdditionalData != null)
        {
            foreach (var kvp in request.AdditionalData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        var notificationRequest = new PushNotificationRequestDto
        {
            UserId = request.UserId,
            PatientId = request.PatientId,
            UserIds = request.UserIds,
            PatientIds = request.PatientIds,
            Topic = request.Topic,
            Title = title,
            Body = body,
            ImageUrl = template.ImageUrl,
            ClickAction = template.ClickAction,
            Data = data,
            ScheduledFor = request.ScheduledFor
        };

        if (request.ScheduledFor.HasValue)
        {
            await ScheduleNotificationAsync(notificationRequest, request.ScheduledFor.Value);
            return new PushNotificationResponseDto
            {
                Success = true,
                MessageId = "scheduled"
            };
        }

        return await SendNotificationAsync(notificationRequest);
    }

    public async Task<int> ScheduleNotificationAsync(PushNotificationRequestDto request, DateTime scheduledFor)
    {
        var scheduledNotification = new ScheduledPushNotification
        {
            UserId = request.UserId,
            PatientId = request.PatientId,
            Topic = request.Topic,
            Title = request.Title,
            Body = request.Body,
            ImageUrl = request.ImageUrl,
            Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
            Priority = request.Priority.ToString(),
            ScheduledFor = scheduledFor,
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };

        _context.ScheduledPushNotifications.Add(scheduledNotification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification scheduled for {ScheduledFor}, Id: {Id}",
            scheduledFor, scheduledNotification.Id);

        return scheduledNotification.Id;
    }

    public async Task<bool> CancelScheduledNotificationAsync(int scheduledNotificationId)
    {
        var notification = await _context.ScheduledPushNotifications
            .FindAsync(scheduledNotificationId);

        if (notification == null || notification.Status != "Scheduled")
            return false;

        notification.Status = "Cancelled";
        notification.CancelledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task ProcessScheduledNotificationsAsync()
    {
        var now = DateTime.UtcNow;
        var pendingNotifications = await _context.ScheduledPushNotifications
            .Where(n => n.Status == "Scheduled" && n.ScheduledFor <= now)
            .ToListAsync();

        foreach (var notification in pendingNotifications)
        {
            try
            {
                var request = new PushNotificationRequestDto
                {
                    UserId = notification.UserId,
                    PatientId = notification.PatientId,
                    Topic = notification.Topic,
                    Title = notification.Title,
                    Body = notification.Body,
                    ImageUrl = notification.ImageUrl,
                    Data = !string.IsNullOrEmpty(notification.Data)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Data)
                        : null
                };

                await SendNotificationAsync(request);

                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled notification {Id}", notification.Id);
                notification.Status = "Failed";
                notification.Error = ex.Message;
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Clinic-Specific Notifications

    public async Task<PushNotificationResponseDto> SendAppointmentReminderAsync(AppointmentReminderNotificationDto dto)
    {
        var request = new PushNotificationRequestDto
        {
            PatientId = dto.PatientId,
            Title = "Appointment Reminder",
            Body = $"You have an appointment with {dto.DoctorName} on {dto.AppointmentDateTime:MMM dd} at {dto.AppointmentDateTime:hh:mm tt}",
            Data = new Dictionary<string, string>
            {
                ["type"] = "appointment_reminder",
                ["appointmentId"] = dto.AppointmentId.ToString(),
                ["dateTime"] = dto.AppointmentDateTime.ToString("O")
            },
            ClickAction = $"/appointments/{dto.AppointmentId}",
            Priority = PushNotificationPriorityDto.High
        };

        return await SendNotificationAsync(request);
    }

    public async Task<PushNotificationResponseDto> SendLabResultNotificationAsync(LabResultNotificationDto dto)
    {
        var title = dto.IsAbnormal ? "Lab Results - Action Required" : "Lab Results Ready";
        var body = $"Your {dto.TestName} results are now available.";
        if (dto.IsAbnormal)
        {
            body += " Please review the results with your doctor.";
        }

        var request = new PushNotificationRequestDto
        {
            PatientId = dto.PatientId,
            Title = title,
            Body = body,
            Data = new Dictionary<string, string>
            {
                ["type"] = "lab_result",
                ["labOrderId"] = dto.LabOrderId.ToString(),
                ["isAbnormal"] = dto.IsAbnormal.ToString()
            },
            ClickAction = $"/lab-results/{dto.LabOrderId}",
            Priority = dto.IsAbnormal ? PushNotificationPriorityDto.High : PushNotificationPriorityDto.Normal
        };

        return await SendNotificationAsync(request);
    }

    public async Task<PushNotificationResponseDto> SendRefillReminderAsync(RefillReminderNotificationDto dto)
    {
        var request = new PushNotificationRequestDto
        {
            PatientId = dto.PatientId,
            Title = "Prescription Refill Reminder",
            Body = $"Your {dto.MedicationName} prescription has {dto.DaysRemaining} days remaining. Request a refill now.",
            Data = new Dictionary<string, string>
            {
                ["type"] = "refill_reminder",
                ["prescriptionId"] = dto.PrescriptionId.ToString(),
                ["daysRemaining"] = dto.DaysRemaining.ToString()
            },
            ClickAction = $"/prescriptions/{dto.PrescriptionId}",
            Priority = dto.DaysRemaining <= 3 ? PushNotificationPriorityDto.High : PushNotificationPriorityDto.Normal
        };

        return await SendNotificationAsync(request);
    }

    public async Task<PushNotificationResponseDto> SendInvoiceNotificationAsync(InvoiceNotificationDto dto)
    {
        var title = dto.IsOverdue ? "Overdue Invoice" : "Invoice Available";
        var body = dto.IsOverdue
            ? $"Invoice {dto.InvoiceNumber} for {dto.Amount:C} is overdue. Please pay promptly."
            : $"Invoice {dto.InvoiceNumber} for {dto.Amount:C} is now available.";

        var request = new PushNotificationRequestDto
        {
            PatientId = dto.PatientId,
            Title = title,
            Body = body,
            Data = new Dictionary<string, string>
            {
                ["type"] = "invoice",
                ["invoiceId"] = dto.InvoiceId.ToString(),
                ["isOverdue"] = dto.IsOverdue.ToString()
            },
            ClickAction = $"/invoices/{dto.InvoiceId}",
            Priority = dto.IsOverdue ? PushNotificationPriorityDto.High : PushNotificationPriorityDto.Normal
        };

        return await SendNotificationAsync(request);
    }

    public async Task<PushNotificationResponseDto> SendStaffNotificationAsync(StaffNotificationDto dto)
    {
        var request = new PushNotificationRequestDto
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Body = dto.Message,
            Data = dto.Data ?? new Dictionary<string, string>
            {
                ["type"] = dto.NotificationType
            },
            ClickAction = dto.ActionUrl,
            Priority = PushNotificationPriorityDto.Normal
        };

        return await SendNotificationAsync(request);
    }

    public async Task<int> SendBulkAppointmentRemindersAsync(int minutesBefore = 60)
    {
        var reminderTime = DateTime.UtcNow.AddMinutes(minutesBefore);
        var startTime = reminderTime.AddMinutes(-5);
        var endTime = reminderTime.AddMinutes(5);

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= startTime &&
                        a.AppointmentDate <= endTime &&
                        a.Status == "Scheduled" &&
                        !a.ReminderSent)
            .ToListAsync();

        var sentCount = 0;

        foreach (var appointment in appointments)
        {
            try
            {
                var dto = new AppointmentReminderNotificationDto
                {
                    AppointmentId = appointment.Id,
                    PatientId = appointment.PatientId,
                    PatientName = $"{appointment.Patient?.FirstName} {appointment.Patient?.LastName}",
                    DoctorName = $"Dr. {appointment.Doctor?.FirstName} {appointment.Doctor?.LastName}",
                    AppointmentDateTime = appointment.AppointmentDate,
                    Location = appointment.Location,
                    ReminderMinutesBefore = minutesBefore
                };

                var result = await SendAppointmentReminderAsync(dto);

                if (result.Success)
                {
                    appointment.ReminderSent = true;
                    appointment.ReminderSentAt = DateTime.UtcNow;
                    sentCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for appointment {Id}", appointment.Id);
            }
        }

        await _context.SaveChangesAsync();
        return sentCount;
    }

    #endregion

    #region Topics

    public async Task<NotificationTopicDto> CreateTopicAsync(int branchId, string name, string? description)
    {
        var topic = new PushNotificationTopic
        {
            BranchId = branchId,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PushNotificationTopics.Add(topic);
        await _context.SaveChangesAsync();

        return new NotificationTopicDto
        {
            Id = topic.Id,
            Name = topic.Name,
            Description = topic.Description,
            SubscriberCount = 0,
            IsActive = topic.IsActive,
            CreatedAt = topic.CreatedAt
        };
    }

    public async Task<IEnumerable<NotificationTopicDto>> GetTopicsAsync(int branchId)
    {
        var topics = await _context.PushNotificationTopics
            .Where(t => t.BranchId == branchId)
            .Select(t => new NotificationTopicDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                SubscriberCount = t.Subscriptions.Count(s => s.IsActive),
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return topics;
    }

    public async Task<TopicSubscriptionResponseDto> SubscribeToTopicAsync(TopicSubscriptionDto dto)
    {
        var topic = await _context.PushNotificationTopics
            .FirstOrDefaultAsync(t => t.Name == dto.TopicName);

        if (topic == null)
        {
            return new TopicSubscriptionResponseDto
            {
                Success = false,
                Message = "Topic not found"
            };
        }

        var device = await _context.PushDevices
            .FirstOrDefaultAsync(d => d.DeviceToken == dto.DeviceToken && d.IsActive);

        if (device == null)
        {
            return new TopicSubscriptionResponseDto
            {
                Success = false,
                Message = "Device not found"
            };
        }

        var existingSubscription = await _context.TopicSubscriptions
            .FirstOrDefaultAsync(s => s.TopicId == topic.Id && s.DeviceId == device.Id);

        if (existingSubscription != null)
        {
            existingSubscription.IsActive = true;
            existingSubscription.SubscribedAt = DateTime.UtcNow;
        }
        else
        {
            var subscription = new TopicSubscription
            {
                TopicId = topic.Id,
                DeviceId = device.Id,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            };
            _context.TopicSubscriptions.Add(subscription);
        }

        await _context.SaveChangesAsync();

        return new TopicSubscriptionResponseDto
        {
            Success = true,
            Message = "Subscribed successfully"
        };
    }

    public async Task<TopicSubscriptionResponseDto> UnsubscribeFromTopicAsync(TopicSubscriptionDto dto)
    {
        // BUG FIX: Add null checks instead of null-forgiving operators to prevent NullReferenceException
        var subscription = await _context.TopicSubscriptions
            .Include(s => s.Topic)
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.Topic != null && s.Topic.Name == dto.TopicName &&
                                      s.Device != null && s.Device.DeviceToken == dto.DeviceToken);

        if (subscription == null)
        {
            return new TopicSubscriptionResponseDto
            {
                Success = false,
                Message = "Subscription not found"
            };
        }

        subscription.IsActive = false;
        subscription.UnsubscribedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new TopicSubscriptionResponseDto
        {
            Success = true,
            Message = "Unsubscribed successfully"
        };
    }

    public async Task<IEnumerable<string>> GetDeviceTopicsAsync(string deviceToken)
    {
        // BUG FIX: Add null checks instead of null-forgiving operators to prevent NullReferenceException
        return await _context.TopicSubscriptions
            .Include(s => s.Topic)
            .Include(s => s.Device)
            .Where(s => s.Device != null && s.Device.DeviceToken == deviceToken && s.IsActive && s.Topic != null)
            .Select(s => s.Topic!.Name) // Null-forgiving is safe here due to the Where clause filter
            .ToListAsync();
    }

    #endregion

    #region Templates

    public async Task<NotificationTemplateDto> CreateTemplateAsync(int branchId, CreateNotificationTemplateDto dto)
    {
        var template = new NotificationTemplate
        {
            BranchId = branchId,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            TitleTemplate = dto.TitleTemplate,
            BodyTemplate = dto.BodyTemplate,
            ImageUrl = dto.ImageUrl,
            ClickAction = dto.ClickAction,
            DefaultData = dto.DefaultData != null ? JsonSerializer.Serialize(dto.DefaultData) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();

        if (dto.Localizations?.Any() == true)
        {
            foreach (var localization in dto.Localizations)
            {
                var templateLocalization = new NotificationTemplateLocalization
                {
                    TemplateId = template.Id,
                    LanguageCode = localization.LanguageCode,
                    TitleTemplate = localization.TitleTemplate,
                    BodyTemplate = localization.BodyTemplate
                };
                _context.NotificationTemplateLocalizations.Add(templateLocalization);
            }
            await _context.SaveChangesAsync();
        }

        return await GetTemplateAsync(template.Id) ?? new NotificationTemplateDto();
    }

    public async Task<NotificationTemplateDto> UpdateTemplateAsync(int templateId, CreateNotificationTemplateDto dto)
    {
        var template = await _context.NotificationTemplates
            .Include(t => t.Localizations)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null)
            return new NotificationTemplateDto();

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.TitleTemplate = dto.TitleTemplate;
        template.BodyTemplate = dto.BodyTemplate;
        template.ImageUrl = dto.ImageUrl;
        template.ClickAction = dto.ClickAction;
        template.DefaultData = dto.DefaultData != null ? JsonSerializer.Serialize(dto.DefaultData) : null;
        template.UpdatedAt = DateTime.UtcNow;

        // Update localizations
        if (template.Localizations != null)
        {
            _context.NotificationTemplateLocalizations.RemoveRange(template.Localizations);
        }

        if (dto.Localizations?.Any() == true)
        {
            foreach (var localization in dto.Localizations)
            {
                var templateLocalization = new NotificationTemplateLocalization
                {
                    TemplateId = template.Id,
                    LanguageCode = localization.LanguageCode,
                    TitleTemplate = localization.TitleTemplate,
                    BodyTemplate = localization.BodyTemplate
                };
                _context.NotificationTemplateLocalizations.Add(templateLocalization);
            }
        }

        await _context.SaveChangesAsync();

        return await GetTemplateAsync(template.Id) ?? new NotificationTemplateDto();
    }

    public async Task<NotificationTemplateDto?> GetTemplateAsync(int templateId)
    {
        var template = await _context.NotificationTemplates
            .Include(t => t.Localizations)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        return template != null ? MapToTemplateDto(template) : null;
    }

    public async Task<NotificationTemplateDto?> GetTemplateByCodeAsync(int branchId, string code)
    {
        var template = await _context.NotificationTemplates
            .Include(t => t.Localizations)
            .FirstOrDefaultAsync(t => t.BranchId == branchId && t.Code == code && t.IsActive);

        return template != null ? MapToTemplateDto(template) : null;
    }

    public async Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(int branchId)
    {
        var templates = await _context.NotificationTemplates
            .Include(t => t.Localizations)
            .Where(t => t.BranchId == branchId)
            .ToListAsync();

        return templates.Select(MapToTemplateDto);
    }

    public async Task<bool> DeleteTemplateAsync(int templateId)
    {
        var template = await _context.NotificationTemplates.FindAsync(templateId);
        if (template == null)
            return false;

        template.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region History & Analytics

    public async Task<(IEnumerable<NotificationHistoryDto> Items, int TotalCount)> GetNotificationHistoryAsync(
        int branchId, NotificationHistoryFilterDto filter)
    {
        var query = _context.PushNotificationHistory
            .Where(h => h.BranchId == branchId);

        if (filter.UserId.HasValue)
            query = query.Where(h => h.UserId == filter.UserId);

        if (filter.PatientId.HasValue)
            query = query.Where(h => h.PatientId == filter.PatientId);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(h => h.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(h => h.CreatedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(h => h.CreatedAt <= filter.ToDate);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(h => h.Title.Contains(filter.SearchTerm) ||
                                     h.Body.Contains(filter.SearchTerm));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(h => new NotificationHistoryDto
            {
                Id = h.Id,
                MessageId = h.MessageId,
                UserId = h.UserId,
                PatientId = h.PatientId,
                Topic = h.Topic,
                Title = h.Title,
                Body = h.Body,
                ImageUrl = h.ImageUrl,
                Status = h.Status,
                CreatedAt = h.CreatedAt,
                SentAt = h.SentAt,
                DeliveredAt = h.DeliveredAt,
                Error = h.Error
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<NotificationStatisticsDto> GetStatisticsAsync(int branchId, DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var history = await _context.PushNotificationHistory
            .Where(h => h.BranchId == branchId && h.CreatedAt >= from && h.CreatedAt <= to)
            .ToListAsync();

        var totalSent = history.Count(h => h.Status == "Sent" || h.Status == "Delivered");
        var totalDelivered = history.Count(h => h.Status == "Delivered");
        var totalFailed = history.Count(h => h.Status == "Failed");
        var totalScheduled = await _context.ScheduledPushNotifications
            .CountAsync(n => n.Status == "Scheduled");

        var dailyStats = history
            .GroupBy(h => h.CreatedAt.Date)
            .Select(g => new NotificationStatsByDayDto
            {
                Date = g.Key,
                Sent = g.Count(h => h.Status == "Sent" || h.Status == "Delivered"),
                Delivered = g.Count(h => h.Status == "Delivered"),
                Failed = g.Count(h => h.Status == "Failed")
            })
            .OrderBy(d => d.Date)
            .ToList();

        var devices = await _context.PushDevices
            .Where(d => d.IsActive)
            .GroupBy(d => d.Platform)
            .Select(g => new { Platform = g.Key, Count = g.Count() })
            .ToListAsync();

        var platformStats = devices.Select(p => new NotificationStatsByPlatformDto
        {
            Platform = p.Platform,
            DeviceCount = p.Count,
            TotalSent = history.Count(h => h.Platform == p.Platform &&
                (h.Status == "Sent" || h.Status == "Delivered")),
            TotalDelivered = history.Count(h => h.Platform == p.Platform && h.Status == "Delivered"),
            DeliveryRate = 0 // Calculate below
        }).ToList();

        foreach (var stat in platformStats)
        {
            if (stat.TotalSent > 0)
            {
                stat.DeliveryRate = Math.Round((decimal)stat.TotalDelivered / stat.TotalSent * 100, 2);
            }
        }

        return new NotificationStatisticsDto
        {
            TotalSent = totalSent,
            TotalDelivered = totalDelivered,
            TotalFailed = totalFailed,
            TotalScheduled = totalScheduled,
            DeliveryRate = totalSent > 0 ? Math.Round((decimal)totalDelivered / totalSent * 100, 2) : 0,
            DailyStats = dailyStats,
            PlatformStats = platformStats
        };
    }

    public async Task<(IEnumerable<ScheduledNotificationDto> Items, int TotalCount)> GetScheduledNotificationsAsync(
        int branchId, ScheduledNotificationFilterDto filter)
    {
        var query = _context.ScheduledPushNotifications.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(n => n.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(n => n.ScheduledFor >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(n => n.ScheduledFor <= filter.ToDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(n => n.ScheduledFor)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(n => new ScheduledNotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                UserId = n.UserId,
                PatientId = n.PatientId,
                Topic = n.Topic,
                ScheduledFor = n.ScheduledFor,
                Status = n.Status,
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt,
                CancelledAt = n.CancelledAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Configuration

    public async Task<PushConfigurationStatusDto> GetConfigurationStatusAsync(int branchId)
    {
        var fcmConfig = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "FCM");

        var apnsConfig = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "APNs");

        var webPushConfig = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "WebPush");

        return new PushConfigurationStatusDto
        {
            Fcm = new FcmConfigurationDto
            {
                ProjectId = fcmConfig?.GetConfigValue("ProjectId"),
                ClientEmail = fcmConfig?.GetConfigValue("ClientEmail"),
                IsConfigured = fcmConfig?.IsActive ?? false
            },
            Apns = new ApnsConfigurationDto
            {
                TeamId = apnsConfig?.GetConfigValue("TeamId"),
                KeyId = apnsConfig?.GetConfigValue("KeyId"),
                BundleId = apnsConfig?.GetConfigValue("BundleId"),
                UseSandbox = apnsConfig?.GetConfigValue("UseSandbox") == "true",
                IsConfigured = apnsConfig?.IsActive ?? false
            },
            WebPush = new WebPushConfigurationDto
            {
                VapidPublicKey = webPushConfig?.GetConfigValue("VapidPublicKey"),
                Subject = webPushConfig?.GetConfigValue("Subject"),
                IsConfigured = webPushConfig?.IsActive ?? false
            }
        };
    }

    public async Task<bool> UpdateFcmConfigurationAsync(int branchId, UpdateFcmConfigurationDto dto)
    {
        var config = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "FCM");

        if (config == null)
        {
            config = new PushConfiguration
            {
                BranchId = branchId,
                Provider = "FCM",
                CreatedAt = DateTime.UtcNow
            };
            _context.PushConfigurations.Add(config);
        }

        config.SetConfigValue("ProjectId", dto.ProjectId);
        config.SetConfigValue("PrivateKeyId", dto.PrivateKeyId);
        config.SetConfigValue("PrivateKey", dto.PrivateKey);
        config.SetConfigValue("ClientEmail", dto.ClientEmail);
        config.SetConfigValue("ClientId", dto.ClientId);
        config.IsActive = true;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateApnsConfigurationAsync(int branchId, UpdateApnsConfigurationDto dto)
    {
        var config = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "APNs");

        if (config == null)
        {
            config = new PushConfiguration
            {
                BranchId = branchId,
                Provider = "APNs",
                CreatedAt = DateTime.UtcNow
            };
            _context.PushConfigurations.Add(config);
        }

        config.SetConfigValue("TeamId", dto.TeamId);
        config.SetConfigValue("KeyId", dto.KeyId);
        config.SetConfigValue("PrivateKey", dto.PrivateKey);
        config.SetConfigValue("BundleId", dto.BundleId);
        config.SetConfigValue("UseSandbox", dto.UseSandbox.ToString().ToLower());
        config.IsActive = true;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateWebPushConfigurationAsync(int branchId, UpdateWebPushConfigurationDto dto)
    {
        var config = await _context.PushConfigurations
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == "WebPush");

        if (config == null)
        {
            config = new PushConfiguration
            {
                BranchId = branchId,
                Provider = "WebPush",
                CreatedAt = DateTime.UtcNow
            };
            _context.PushConfigurations.Add(config);
        }

        config.SetConfigValue("VapidPublicKey", dto.VapidPublicKey);
        config.SetConfigValue("VapidPrivateKey", dto.VapidPrivateKey);
        config.SetConfigValue("Subject", dto.Subject);
        config.IsActive = true;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PushNotificationResponseDto> TestConfigurationAsync(int branchId, string deviceToken)
    {
        var request = new PushNotificationRequestDto
        {
            Title = "Test Notification",
            Body = "This is a test notification from XenonClinic.",
            Data = new Dictionary<string, string>
            {
                ["type"] = "test"
            }
        };

        return await SendToDeviceAsync(deviceToken, request);
    }

    #endregion

    #region Private Methods

    private async Task<IEnumerable<string>> GetActiveDeviceTokensForUserAsync(int userId)
    {
        return await _context.PushDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .Select(d => d.DeviceToken)
            .ToListAsync();
    }

    /// <summary>
    /// BUG FIX: Batch method to load device tokens for multiple users in a single query.
    /// Replaces N+1 queries when sending notifications to multiple users.
    /// </summary>
    private async Task<IEnumerable<string>> GetActiveDeviceTokensForUsersAsync(IEnumerable<int> userIds)
    {
        var userIdList = userIds.ToList();
        if (!userIdList.Any())
            return Enumerable.Empty<string>();

        return await _context.PushDevices
            .AsNoTracking()
            .Where(d => d.UserId.HasValue && userIdList.Contains(d.UserId.Value) && d.IsActive)
            .Select(d => d.DeviceToken)
            .ToListAsync();
    }

    private async Task<IEnumerable<string>> GetActiveDeviceTokensForPatientAsync(int patientId)
    {
        return await _context.PushDevices
            .Where(d => d.PatientId == patientId && d.IsActive)
            .Select(d => d.DeviceToken)
            .ToListAsync();
    }

    /// <summary>
    /// BUG FIX: Batch method to load device tokens for multiple patients in a single query.
    /// Replaces N+1 queries when sending notifications to multiple patients.
    /// </summary>
    private async Task<IEnumerable<string>> GetActiveDeviceTokensForPatientsAsync(IEnumerable<int> patientIds)
    {
        var patientIdList = patientIds.ToList();
        if (!patientIdList.Any())
            return Enumerable.Empty<string>();

        return await _context.PushDevices
            .AsNoTracking()
            .Where(d => d.PatientId.HasValue && patientIdList.Contains(d.PatientId.Value) && d.IsActive)
            .Select(d => d.DeviceToken)
            .ToListAsync();
    }

    private async Task<PushNotificationResponseDto> SendViaFcmAsync(
        List<string> tokens, PushNotificationRequestDto request)
    {
        _logger.LogInformation("Sending FCM notification to {Count} devices", tokens.Count);

        var results = new List<PushNotificationResultDto>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Get FCM configuration from database (using first active branch config)
            var fcmConfig = await _context.PushConfigurations
                .FirstOrDefaultAsync(c => c.Provider == "FCM" && c.IsActive);

            if (fcmConfig == null)
            {
                _logger.LogWarning("FCM configuration not found");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "FCM configuration not found",
                    FailureCount = tokens.Count
                };
            }

            var projectId = fcmConfig.GetConfigValue("ProjectId");
            var clientEmail = fcmConfig.GetConfigValue("ClientEmail");
            var privateKey = fcmConfig.GetConfigValue("PrivateKey");

            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(clientEmail) || string.IsNullOrEmpty(privateKey))
            {
                _logger.LogWarning("FCM configuration is incomplete");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "FCM configuration is incomplete",
                    FailureCount = tokens.Count
                };
            }

            // Generate OAuth 2.0 access token using service account credentials
            var accessToken = await GenerateFcmAccessTokenAsync(clientEmail, privateKey);

            // FCM HTTP v1 API endpoint
            var fcmUrl = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

            foreach (var token in tokens)
            {
                try
                {
                    var payload = new
                    {
                        message = new
                        {
                            token,
                            notification = new
                            {
                                title = request.Title,
                                body = request.Body,
                                image = request.ImageUrl
                            },
                            data = request.Data ?? new Dictionary<string, string>(),
                            android = new
                            {
                                priority = request.Priority == PushNotificationPriorityDto.High ? "high" : "normal",
                                notification = new
                                {
                                    sound = request.Sound ?? "default",
                                    click_action = request.ClickAction
                                }
                            }
                        }
                    };

                    var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });

                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, fcmUrl);
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(httpRequest);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var fcmResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var messageId = fcmResponse.TryGetProperty("name", out var nameProp)
                            ? nameProp.GetString()
                            : Guid.NewGuid().ToString();

                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = token,
                            Success = true,
                            MessageId = messageId
                        });
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("FCM send failed for token {Token}: {Response}", token, responseContent);

                        // Check if token is invalid and should be removed
                        var shouldRemoveToken = responseContent.Contains("UNREGISTERED") ||
                                                responseContent.Contains("INVALID_ARGUMENT");

                        if (shouldRemoveToken)
                        {
                            await MarkDeviceTokenInvalidAsync(token);
                        }

                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = token,
                            Success = false,
                            Error = responseContent
                        });
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending FCM notification to token {Token}", token);
                    results.Add(new PushNotificationResultDto
                    {
                        DeviceToken = token,
                        Success = false,
                        Error = ex.Message
                    });
                    failureCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing FCM send");
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = ex.Message,
                FailureCount = tokens.Count
            };
        }

        return new PushNotificationResponseDto
        {
            Success = successCount > 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    private async Task<PushNotificationResponseDto> SendViaApnsAsync(
        List<string> tokens, PushNotificationRequestDto request)
    {
        _logger.LogInformation("Sending APNs notification to {Count} devices", tokens.Count);

        var results = new List<PushNotificationResultDto>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Get APNs configuration from database
            var apnsConfig = await _context.PushConfigurations
                .FirstOrDefaultAsync(c => c.Provider == "APNs" && c.IsActive);

            if (apnsConfig == null)
            {
                _logger.LogWarning("APNs configuration not found");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "APNs configuration not found",
                    FailureCount = tokens.Count
                };
            }

            var teamId = apnsConfig.GetConfigValue("TeamId");
            var keyId = apnsConfig.GetConfigValue("KeyId");
            var privateKey = apnsConfig.GetConfigValue("PrivateKey");
            var bundleId = apnsConfig.GetConfigValue("BundleId");
            var useSandbox = apnsConfig.GetConfigValue("UseSandbox") == "true";

            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(keyId) ||
                string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(bundleId))
            {
                _logger.LogWarning("APNs configuration is incomplete");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "APNs configuration is incomplete",
                    FailureCount = tokens.Count
                };
            }

            // Generate JWT token for APNs authentication
            var jwtToken = GenerateApnsJwtToken(teamId, keyId, privateKey);

            // APNs HTTP/2 endpoint
            var apnsHost = useSandbox
                ? "https://api.sandbox.push.apple.com"
                : "https://api.push.apple.com";

            foreach (var token in tokens)
            {
                try
                {
                    var apnsUrl = $"{apnsHost}/3/device/{token}";

                    // Build APNs payload
                    var aps = new Dictionary<string, object>
                    {
                        ["alert"] = new Dictionary<string, string>
                        {
                            ["title"] = request.Title,
                            ["body"] = request.Body
                        },
                        ["sound"] = request.Sound ?? "default"
                    };

                    if (request.Badge.HasValue)
                    {
                        aps["badge"] = request.Badge.Value;
                    }

                    if (!string.IsNullOrEmpty(request.Category))
                    {
                        aps["category"] = request.Category;
                    }

                    var payload = new Dictionary<string, object> { ["aps"] = aps };

                    // Add custom data
                    if (request.Data != null)
                    {
                        foreach (var kvp in request.Data)
                        {
                            payload[kvp.Key] = kvp.Value;
                        }
                    }

                    var jsonPayload = JsonSerializer.Serialize(payload);

                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apnsUrl);
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
                    httpRequest.Headers.TryAddWithoutValidation("apns-topic", bundleId);
                    httpRequest.Headers.TryAddWithoutValidation("apns-push-type", "alert");
                    httpRequest.Headers.TryAddWithoutValidation("apns-priority",
                        request.Priority == PushNotificationPriorityDto.High ? "10" : "5");
                    httpRequest.Headers.TryAddWithoutValidation("apns-expiration", "0");
                    httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(httpRequest);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // APNs returns the message ID in the apns-id header
                        var messageId = response.Headers.TryGetValues("apns-id", out var apnsIdValues)
                            ? apnsIdValues.FirstOrDefault()
                            : Guid.NewGuid().ToString();

                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = token,
                            Success = true,
                            MessageId = messageId
                        });
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("APNs send failed for token {Token}: {StatusCode} - {Response}",
                            token, response.StatusCode, responseContent);

                        // Check for invalid token errors
                        var shouldRemoveToken = responseContent.Contains("BadDeviceToken") ||
                                                responseContent.Contains("Unregistered") ||
                                                responseContent.Contains("DeviceTokenNotForTopic");

                        if (shouldRemoveToken)
                        {
                            await MarkDeviceTokenInvalidAsync(token);
                        }

                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = token,
                            Success = false,
                            Error = responseContent
                        });
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending APNs notification to token {Token}", token);
                    results.Add(new PushNotificationResultDto
                    {
                        DeviceToken = token,
                        Success = false,
                        Error = ex.Message
                    });
                    failureCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing APNs send");
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = ex.Message,
                FailureCount = tokens.Count
            };
        }

        return new PushNotificationResponseDto
        {
            Success = successCount > 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    private async Task<PushNotificationResponseDto> SendViaWebPushAsync(
        List<string> tokens, PushNotificationRequestDto request)
    {
        _logger.LogInformation("Sending Web Push notification to {Count} devices", tokens.Count);

        var results = new List<PushNotificationResultDto>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Get Web Push configuration from database
            var webPushConfig = await _context.PushConfigurations
                .FirstOrDefaultAsync(c => c.Provider == "WebPush" && c.IsActive);

            if (webPushConfig == null)
            {
                _logger.LogWarning("Web Push configuration not found");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "Web Push configuration not found",
                    FailureCount = tokens.Count
                };
            }

            var vapidPublicKey = webPushConfig.GetConfigValue("VapidPublicKey");
            var vapidPrivateKey = webPushConfig.GetConfigValue("VapidPrivateKey");
            var subject = webPushConfig.GetConfigValue("Subject");

            if (string.IsNullOrEmpty(vapidPublicKey) || string.IsNullOrEmpty(vapidPrivateKey) || string.IsNullOrEmpty(subject))
            {
                _logger.LogWarning("Web Push configuration is incomplete");
                return new PushNotificationResponseDto
                {
                    Success = false,
                    Error = "Web Push configuration is incomplete",
                    FailureCount = tokens.Count
                };
            }

            // Get device details for Web Push (need endpoint, p256dh, auth)
            var devices = await _context.PushDevices
                .Where(d => tokens.Contains(d.DeviceToken) && d.IsActive && d.Platform.ToLower() == "web")
                .ToListAsync();

            foreach (var device in devices)
            {
                if (string.IsNullOrEmpty(device.PushEndpoint) ||
                    string.IsNullOrEmpty(device.P256dh) ||
                    string.IsNullOrEmpty(device.Auth))
                {
                    _logger.LogWarning("Web Push device {DeviceId} missing subscription data", device.Id);
                    results.Add(new PushNotificationResultDto
                    {
                        DeviceToken = device.DeviceToken,
                        Success = false,
                        Error = "Missing subscription data"
                    });
                    failureCount++;
                    continue;
                }

                try
                {
                    // Build Web Push payload
                    var payload = new
                    {
                        title = request.Title,
                        body = request.Body,
                        icon = request.ImageUrl ?? "/icons/notification-icon.png",
                        badge = "/icons/badge-icon.png",
                        data = request.Data ?? new Dictionary<string, string>(),
                        actions = new[]
                        {
                            new { action = "open", title = "Open" },
                            new { action = "dismiss", title = "Dismiss" }
                        },
                        requireInteraction = request.Priority == PushNotificationPriorityDto.High,
                        tag = request.CollapseKey ?? Guid.NewGuid().ToString()
                    };

                    var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    // Generate VAPID JWT token
                    var vapidToken = GenerateVapidJwtToken(device.PushEndpoint, subject, vapidPublicKey, vapidPrivateKey);

                    // Encrypt the payload using the subscription keys
                    var encryptedPayload = EncryptWebPushPayload(jsonPayload, device.P256dh, device.Auth);

                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, device.PushEndpoint);
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", $"vapid t={vapidToken}, k={vapidPublicKey}");
                    httpRequest.Headers.TryAddWithoutValidation("TTL", "86400"); // 24 hours
                    httpRequest.Headers.TryAddWithoutValidation("Urgency", request.Priority == PushNotificationPriorityDto.High ? "high" : "normal");
                    httpRequest.Content = new ByteArrayContent(encryptedPayload.Payload);
                    httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpRequest.Content.Headers.ContentEncoding.Add("aes128gcm");
                    httpRequest.Headers.TryAddWithoutValidation("Crypto-Key", $"p256ecdsa={vapidPublicKey}");

                    var response = await _httpClient.SendAsync(httpRequest);

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = device.DeviceToken,
                            Success = true,
                            MessageId = Guid.NewGuid().ToString()
                        });
                        successCount++;
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Web Push send failed for device {DeviceId}: {StatusCode} - {Response}",
                            device.Id, response.StatusCode, responseContent);

                        // Check for subscription expiration
                        var shouldRemoveToken = response.StatusCode == System.Net.HttpStatusCode.Gone ||
                                                response.StatusCode == System.Net.HttpStatusCode.NotFound;

                        if (shouldRemoveToken)
                        {
                            await MarkDeviceTokenInvalidAsync(device.DeviceToken);
                        }

                        results.Add(new PushNotificationResultDto
                        {
                            DeviceToken = device.DeviceToken,
                            Success = false,
                            Error = responseContent
                        });
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending Web Push notification to device {DeviceId}", device.Id);
                    results.Add(new PushNotificationResultDto
                    {
                        DeviceToken = device.DeviceToken,
                        Success = false,
                        Error = ex.Message
                    });
                    failureCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Web Push send");
            return new PushNotificationResponseDto
            {
                Success = false,
                Error = ex.Message,
                FailureCount = tokens.Count
            };
        }

        return new PushNotificationResponseDto
        {
            Success = successCount > 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    private async Task LogNotificationAsync(PushNotificationRequestDto request,
        int recipientCount, int successCount, string? topic = null)
    {
        var history = new PushNotificationHistoryEntry
        {
            UserId = request.UserId,
            PatientId = request.PatientId,
            Topic = topic ?? request.Topic,
            Title = request.Title,
            Body = request.Body,
            ImageUrl = request.ImageUrl,
            Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
            RecipientCount = recipientCount,
            SuccessCount = successCount,
            Status = successCount > 0 ? "Sent" : "Failed",
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow
        };

        _context.PushNotificationHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    private static string ReplaceTemplateVariables(string template, Dictionary<string, string>? variables)
    {
        if (string.IsNullOrEmpty(template) || variables == null)
            return template;

        var result = template;
        foreach (var kvp in variables)
        {
            result = Regex.Replace(result, $@"\{{\{{{kvp.Key}\}}\}}", kvp.Value, RegexOptions.IgnoreCase);
        }
        return result;
    }

    private static RegisteredDeviceDto MapToRegisteredDeviceDto(PushDevice device)
    {
        return new RegisteredDeviceDto
        {
            Id = device.Id,
            Platform = device.Platform,
            DeviceName = device.DeviceName,
            DeviceModel = device.DeviceModel,
            RegisteredAt = device.RegisteredAt,
            LastActiveAt = device.LastActiveAt,
            IsActive = device.IsActive
        };
    }

    private NotificationTemplateDto MapToTemplateDto(NotificationTemplate template)
    {
        return new NotificationTemplateDto
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Description = template.Description,
            TitleTemplate = template.TitleTemplate,
            BodyTemplate = template.BodyTemplate,
            ImageUrl = template.ImageUrl,
            ClickAction = template.ClickAction,
            DefaultData = !string.IsNullOrEmpty(template.DefaultData)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(template.DefaultData)
                : null,
            IsActive = template.IsActive,
            Localizations = template.Localizations?.Select(l => new NotificationTemplateLocalizationDto
            {
                LanguageCode = l.LanguageCode,
                TitleTemplate = l.TitleTemplate,
                BodyTemplate = l.BodyTemplate
            }).ToList()
        };
    }

    /// <summary>
    /// Generates an OAuth 2.0 access token for FCM using service account credentials.
    /// Uses Google's OAuth 2.0 server-to-server authentication.
    /// </summary>
    private async Task<string> GenerateFcmAccessTokenAsync(string clientEmail, string privateKey)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiry = now + 3600; // 1 hour

        // Build JWT header
        var header = new { alg = "RS256", typ = "JWT" };
        var headerJson = JsonSerializer.Serialize(header);
        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

        // Build JWT claim set
        var claimSet = new
        {
            iss = clientEmail,
            scope = "https://www.googleapis.com/auth/firebase.messaging",
            aud = "https://oauth2.googleapis.com/token",
            iat = now,
            exp = expiry
        };
        var claimSetJson = JsonSerializer.Serialize(claimSet);
        var claimSetBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(claimSetJson));

        // Create signature input
        var signatureInput = $"{headerBase64}.{claimSetBase64}";

        // Sign with RSA private key
        using var rsa = RSA.Create();
        var privateKeyContent = privateKey
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\\n", "")
            .Replace("\n", "")
            .Trim();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyContent), out _);

        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(signatureInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var signatureBase64 = Base64UrlEncode(signature);

        var jwt = $"{signatureInput}.{signatureBase64}";

        // Exchange JWT for access token
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = jwt
        };

        using var content = new FormUrlEncodedContent(tokenRequest);
        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to get FCM access token: {responseContent}");
        }

        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return tokenResponse.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Access token not found in response");
    }

    /// <summary>
    /// Generates a JWT token for APNs authentication using ES256 algorithm.
    /// </summary>
    private static string GenerateApnsJwtToken(string teamId, string keyId, string privateKey)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Build JWT header
        var header = new { alg = "ES256", kid = keyId };
        var headerJson = JsonSerializer.Serialize(header);
        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

        // Build JWT payload
        var payload = new { iss = teamId, iat = now };
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        // Create signature input
        var signatureInput = $"{headerBase64}.{payloadBase64}";

        // Sign with ECDSA P-256 private key
        using var ecdsa = ECDsa.Create();
        var privateKeyContent = privateKey
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\\n", "")
            .Replace("\n", "")
            .Trim();
        ecdsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyContent), out _);

        var signature = ecdsa.SignData(
            Encoding.UTF8.GetBytes(signatureInput),
            HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncode(signature);

        return $"{signatureInput}.{signatureBase64}";
    }

    /// <summary>
    /// Generates a VAPID JWT token for Web Push authentication.
    /// </summary>
    private static string GenerateVapidJwtToken(string endpoint, string subject, string publicKey, string privateKey)
    {
        var uri = new Uri(endpoint);
        var audience = $"{uri.Scheme}://{uri.Host}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiry = now + 43200; // 12 hours

        // Build JWT header
        var header = new { typ = "JWT", alg = "ES256" };
        var headerJson = JsonSerializer.Serialize(header);
        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

        // Build JWT payload
        var payload = new
        {
            aud = audience,
            exp = expiry,
            sub = subject
        };
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        // Create signature input
        var signatureInput = $"{headerBase64}.{payloadBase64}";

        // Sign with ECDSA P-256 private key
        using var ecdsa = ECDsa.Create();
        var privateKeyBytes = Base64UrlDecode(privateKey);
        ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        var signature = ecdsa.SignData(
            Encoding.UTF8.GetBytes(signatureInput),
            HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncode(signature);

        return $"{signatureInput}.{signatureBase64}";
    }

    /// <summary>
    /// Encrypts Web Push payload using the subscription's p256dh and auth keys.
    /// Implements RFC 8291 (Message Encryption for Web Push).
    /// </summary>
    private static WebPushEncryptionResult EncryptWebPushPayload(string payload, string p256dh, string auth)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        // Decode subscription keys
        var clientPublicKey = Base64UrlDecode(p256dh);
        var authSecret = Base64UrlDecode(auth);

        // Generate ephemeral key pair for ECDH
        using var serverKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var serverPublicKey = serverKey.PublicKey.ExportSubjectPublicKeyInfo();

        // Import client public key
        using var clientKey = ECDiffieHellman.Create();
        clientKey.ImportSubjectPublicKeyInfo(clientPublicKey, out _);

        // Derive shared secret
        var sharedSecret = serverKey.DeriveKeyMaterial(clientKey.PublicKey);

        // Derive encryption key using HKDF
        var salt = RandomNumberGenerator.GetBytes(16);
        var context = CreateContext(clientPublicKey, serverPublicKey);

        // PRK = HKDF-Extract(auth, shared_secret)
        var prk = HkdfExtract(authSecret, sharedSecret);

        // IKM = HKDF-Expand(prk, "WebPush: info" || 0x00 || context, 32)
        var ikmInfo = CombineBytes(Encoding.ASCII.GetBytes("WebPush: info\0"), context);
        var ikm = HkdfExpand(prk, ikmInfo, 32);

        // Derive CEK and nonce
        var prkKey = HkdfExtract(salt, ikm);
        var cekInfo = Encoding.ASCII.GetBytes("Content-Encoding: aes128gcm\0");
        var nonceInfo = Encoding.ASCII.GetBytes("Content-Encoding: nonce\0");
        var cek = HkdfExpand(prkKey, cekInfo, 16);
        var nonce = HkdfExpand(prkKey, nonceInfo, 12);

        // Encrypt with AES-128-GCM
        using var aes = new AesGcm(cek, 16);
        var paddedPayload = new byte[payloadBytes.Length + 1]; // Add padding delimiter
        Buffer.BlockCopy(payloadBytes, 0, paddedPayload, 0, payloadBytes.Length);
        paddedPayload[payloadBytes.Length] = 0x02; // Padding delimiter

        var ciphertext = new byte[paddedPayload.Length];
        var tag = new byte[16];
        aes.Encrypt(nonce, paddedPayload, ciphertext, tag);

        // Build encrypted content (salt || rs || idlen || keyid || ciphertext || tag)
        var recordSize = BitConverter.GetBytes((uint)4096);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(recordSize);

        var keyIdLen = (byte)65; // Uncompressed EC point length
        var serverPublicKeyBytes = ExportUncompressedPublicKey(serverKey);

        var encryptedContent = CombineBytes(
            salt,
            recordSize,
            new[] { keyIdLen },
            serverPublicKeyBytes,
            ciphertext,
            tag);

        return new WebPushEncryptionResult
        {
            Payload = encryptedContent,
            Salt = Convert.ToBase64String(salt),
            ServerPublicKey = Base64UrlEncode(serverPublicKeyBytes)
        };
    }

    /// <summary>
    /// Marks a device token as invalid (unregistered from push notifications).
    /// </summary>
    private async Task MarkDeviceTokenInvalidAsync(string deviceToken)
    {
        var device = await _context.PushDevices
            .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken);

        if (device != null)
        {
            device.IsActive = false;
            device.UnregisteredAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked device token as invalid: {Token}", deviceToken);
        }
    }

    // Helper methods for cryptographic operations
    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private static byte[] CombineBytes(params byte[][] arrays)
    {
        var result = new byte[arrays.Sum(a => a.Length)];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }
        return result;
    }

    private static byte[] CreateContext(byte[] clientPublicKey, byte[] serverPublicKey)
    {
        var clientLen = BitConverter.GetBytes((ushort)clientPublicKey.Length);
        var serverLen = BitConverter.GetBytes((ushort)serverPublicKey.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(clientLen);
            Array.Reverse(serverLen);
        }
        return CombineBytes(clientLen, clientPublicKey, serverLen, serverPublicKey);
    }

    private static byte[] HkdfExtract(byte[] salt, byte[] ikm)
    {
        using var hmac = new HMACSHA256(salt);
        return hmac.ComputeHash(ikm);
    }

    private static byte[] HkdfExpand(byte[] prk, byte[] info, int length)
    {
        using var hmac = new HMACSHA256(prk);
        var result = new byte[length];
        var t = Array.Empty<byte>();
        var offset = 0;
        byte counter = 1;

        while (offset < length)
        {
            var input = CombineBytes(t, info, new[] { counter });
            t = hmac.ComputeHash(input);
            var toCopy = Math.Min(t.Length, length - offset);
            Buffer.BlockCopy(t, 0, result, offset, toCopy);
            offset += toCopy;
            counter++;
        }

        return result;
    }

    private static byte[] ExportUncompressedPublicKey(ECDiffieHellman key)
    {
        var parameters = key.ExportParameters(false);
        var result = new byte[65];
        result[0] = 0x04; // Uncompressed point indicator
        Buffer.BlockCopy(parameters.Q.X!, 0, result, 1, 32);
        Buffer.BlockCopy(parameters.Q.Y!, 0, result, 33, 32);
        return result;
    }

    #endregion
}

/// <summary>
/// Result of Web Push payload encryption
/// </summary>
internal class WebPushEncryptionResult
{
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public string Salt { get; set; } = string.Empty;
    public string ServerPublicKey { get; set; } = string.Empty;
}

#region Push Notification Entities

/// <summary>
/// Push device entity
/// </summary>
public class PushDevice
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
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
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime? UnregisteredAt { get; set; }

    public User? User { get; set; }
    public Patient? Patient { get; set; }
}

/// <summary>
/// Push notification topic entity
/// </summary>
public class PushNotificationTopic
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TopicSubscription> Subscriptions { get; set; } = new List<TopicSubscription>();
}

/// <summary>
/// Topic subscription entity
/// </summary>
public class TopicSubscription
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public int DeviceId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SubscribedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }

    public PushNotificationTopic? Topic { get; set; }
    public PushDevice? Device { get; set; }
}

/// <summary>
/// Notification template entity
/// </summary>
public class NotificationTemplate
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ClickAction { get; set; }
    public string? DefaultData { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<NotificationTemplateLocalization>? Localizations { get; set; }
}

/// <summary>
/// Notification template localization entity
/// </summary>
public class NotificationTemplateLocalization
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;

    public NotificationTemplate? Template { get; set; }
}

/// <summary>
/// Scheduled push notification entity
/// </summary>
public class ScheduledPushNotification
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public string? Topic { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Data { get; set; }
    public string? Priority { get; set; }
    public DateTime ScheduledFor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Push notification history entry
/// </summary>
public class PushNotificationHistoryEntry
{
    public int Id { get; set; }
    public int? BranchId { get; set; }
    public string? MessageId { get; set; }
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public string? Topic { get; set; }
    public string? Platform { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Data { get; set; }
    public int RecipientCount { get; set; }
    public int SuccessCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Push configuration entity
/// </summary>
public class PushConfiguration
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Provider { get; set; } = string.Empty; // FCM, APNs, WebPush
    public string? ConfigData { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private Dictionary<string, string>? _configDict;

    public string? GetConfigValue(string key)
    {
        if (_configDict == null && !string.IsNullOrEmpty(ConfigData))
        {
            _configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(ConfigData);
        }
        return _configDict?.GetValueOrDefault(key);
    }

    public void SetConfigValue(string key, string value)
    {
        if (_configDict == null)
        {
            _configDict = !string.IsNullOrEmpty(ConfigData)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(ConfigData) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
        }
        _configDict[key] = value;
        ConfigData = JsonSerializer.Serialize(_configDict);
    }
}

#endregion
