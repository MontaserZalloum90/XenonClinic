using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        // TODO: Implement actual FCM API call using Firebase Admin SDK
        _logger.LogInformation("Sending FCM notification to {Count} devices", tokens.Count);

        // Simulated response
        var results = tokens.Select(t => new PushNotificationResultDto
        {
            DeviceToken = t,
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        }).ToList();

        return new PushNotificationResponseDto
        {
            Success = true,
            SuccessCount = tokens.Count,
            FailureCount = 0,
            Results = results
        };
    }

    private async Task<PushNotificationResponseDto> SendViaApnsAsync(
        List<string> tokens, PushNotificationRequestDto request)
    {
        // TODO: Implement actual APNs API call
        _logger.LogInformation("Sending APNs notification to {Count} devices", tokens.Count);

        var results = tokens.Select(t => new PushNotificationResultDto
        {
            DeviceToken = t,
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        }).ToList();

        return new PushNotificationResponseDto
        {
            Success = true,
            SuccessCount = tokens.Count,
            FailureCount = 0,
            Results = results
        };
    }

    private async Task<PushNotificationResponseDto> SendViaWebPushAsync(
        List<string> tokens, PushNotificationRequestDto request)
    {
        // TODO: Implement actual Web Push API call using VAPID
        _logger.LogInformation("Sending Web Push notification to {Count} devices", tokens.Count);

        var results = tokens.Select(t => new PushNotificationResultDto
        {
            DeviceToken = t,
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        }).ToList();

        return new PushNotificationResponseDto
        {
            Success = true,
            SuccessCount = tokens.Count,
            FailureCount = 0,
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

    #endregion
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
