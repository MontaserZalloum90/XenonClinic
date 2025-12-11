using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Email service implementation with templating support.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly ConcurrentDictionary<string, EmailTemplate> _templates = new();

    public EmailService(IOptions<EmailConfiguration> config, ILogger<EmailService> logger)
    {
        _config = config.Value;
        _logger = logger;

        // Register default templates
        RegisterDefaultTemplates();
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var result = new EmailSendResult
        {
            MessageId = message.MessageId ?? Guid.NewGuid().ToString()
        };

        try
        {
            // Set default from if not specified
            message.From ??= _config.DefaultFrom;

            _logger.LogDebug("Sending email to {Recipients} with subject: {Subject}",
                string.Join(", ", message.To.Select(t => t.Email)),
                message.Subject);

            switch (_config.Provider.ToLowerInvariant())
            {
                case "smtp":
                    await SendViaSmtpAsync(message, cancellationToken);
                    break;

                case "sendgrid":
                    await SendViaSendGridAsync(message, cancellationToken);
                    break;

                case "console":
                default:
                    // Log to console for development
                    LogEmailToConsole(message);
                    break;
            }

            result.Success = true;
            result.SentAt = DateTime.UtcNow;

            _logger.LogInformation("Email sent successfully to {Recipients}, MessageId: {MessageId}",
                string.Join(", ", message.To.Select(t => t.Email)),
                result.MessageId);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorCode = "SEND_FAILED";
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to send email to {Recipients}",
                string.Join(", ", message.To.Select(t => t.Email)));
        }

        return result;
    }

    public async Task<EmailSendResult> SendTemplatedAsync(TemplatedEmailRequest request, CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(request.TemplateKey, cancellationToken);
        if (template == null)
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorCode = "TEMPLATE_NOT_FOUND",
                ErrorMessage = $"Email template '{request.TemplateKey}' not found"
            };
        }

        var message = new EmailMessage
        {
            To = request.To,
            Cc = request.Cc,
            From = request.From ?? _config.DefaultFrom,
            Subject = RenderTemplate(template.SubjectTemplate, request.Variables),
            TextBody = !string.IsNullOrEmpty(template.TextBodyTemplate)
                ? RenderTemplate(template.TextBodyTemplate, request.Variables)
                : null,
            HtmlBody = RenderTemplate(template.HtmlBodyTemplate, request.Variables),
            Attachments = request.Attachments
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task SendTaskAssignmentAsync(TaskAssignmentEmailRequest request, CancellationToken cancellationToken = default)
    {
        var variables = new Dictionary<string, object>
        {
            ["taskId"] = request.TaskId,
            ["taskName"] = request.TaskName,
            ["taskDescription"] = request.TaskDescription ?? "",
            ["assigneeName"] = request.AssigneeName ?? request.AssigneeEmail,
            ["assignedByName"] = request.AssignedByName ?? "System",
            ["processName"] = request.ProcessName,
            ["dueDate"] = request.DueDate?.ToString("f") ?? "Not set",
            ["priority"] = GetPriorityLabel(request.Priority),
            ["taskUrl"] = request.TaskUrl ?? "#"
        };

        foreach (var kvp in request.AdditionalVariables)
        {
            variables[kvp.Key] = kvp.Value;
        }

        await SendTemplatedAsync(new TemplatedEmailRequest
        {
            TemplateKey = "task-assignment",
            To = new List<EmailAddress> { new(request.AssigneeEmail, request.AssigneeName) },
            Variables = variables
        }, cancellationToken);
    }

    public async Task SendTaskReminderAsync(TaskReminderEmailRequest request, CancellationToken cancellationToken = default)
    {
        var variables = new Dictionary<string, object>
        {
            ["taskId"] = request.TaskId,
            ["taskName"] = request.TaskName,
            ["assigneeName"] = request.AssigneeName ?? request.AssigneeEmail,
            ["processName"] = request.ProcessName,
            ["dueDate"] = request.DueDate?.ToString("f") ?? "Not set",
            ["timeRemaining"] = FormatTimeSpan(request.TimeRemaining),
            ["reminderNumber"] = request.ReminderNumber,
            ["taskUrl"] = request.TaskUrl ?? "#"
        };

        await SendTemplatedAsync(new TemplatedEmailRequest
        {
            TemplateKey = "task-reminder",
            To = new List<EmailAddress> { new(request.AssigneeEmail, request.AssigneeName) },
            Variables = variables
        }, cancellationToken);
    }

    public async Task SendTaskDueDateWarningAsync(TaskDueDateEmailRequest request, CancellationToken cancellationToken = default)
    {
        var templateKey = request.IsOverdue ? "task-overdue" : "task-due-soon";

        var variables = new Dictionary<string, object>
        {
            ["taskId"] = request.TaskId,
            ["taskName"] = request.TaskName,
            ["assigneeName"] = request.AssigneeName ?? request.AssigneeEmail,
            ["processName"] = request.ProcessName,
            ["dueDate"] = request.DueDate.ToString("f"),
            ["timeRemaining"] = FormatTimeSpan(request.TimeRemaining),
            ["isOverdue"] = request.IsOverdue,
            ["taskUrl"] = request.TaskUrl ?? "#"
        };

        await SendTemplatedAsync(new TemplatedEmailRequest
        {
            TemplateKey = templateKey,
            To = new List<EmailAddress> { new(request.AssigneeEmail, request.AssigneeName) },
            Variables = variables
        }, cancellationToken);
    }

    public async Task SendProcessCompletionAsync(ProcessCompletionEmailRequest request, CancellationToken cancellationToken = default)
    {
        var variables = new Dictionary<string, object>
        {
            ["processInstanceId"] = request.ProcessInstanceId,
            ["processName"] = request.ProcessName,
            ["businessKey"] = request.BusinessKey ?? "",
            ["initiatorName"] = request.InitiatorName ?? "Unknown",
            ["startedAt"] = request.StartedAt.ToString("f"),
            ["completedAt"] = request.CompletedAt.ToString("f"),
            ["duration"] = FormatTimeSpan(request.Duration),
            ["processUrl"] = request.ProcessUrl ?? "#"
        };

        foreach (var kvp in request.OutputVariables)
        {
            variables[$"output_{kvp.Key}"] = kvp.Value;
        }

        var recipients = request.RecipientEmails.Select(e => new EmailAddress(e)).ToList();

        await SendTemplatedAsync(new TemplatedEmailRequest
        {
            TemplateKey = "process-completion",
            To = recipients,
            Variables = variables
        }, cancellationToken);
    }

    public async Task SendEscalationAsync(EscalationEmailRequest request, CancellationToken cancellationToken = default)
    {
        var variables = new Dictionary<string, object>
        {
            ["taskId"] = request.TaskId,
            ["taskName"] = request.TaskName,
            ["escalationLevel"] = request.EscalationLevel,
            ["originalAssignee"] = request.OriginalAssignee ?? "Unknown",
            ["processName"] = request.ProcessName,
            ["dueDate"] = request.DueDate?.ToString("f") ?? "Not set",
            ["overdueBy"] = FormatTimeSpan(request.OverdueBy),
            ["escalationReason"] = request.EscalationReason,
            ["taskUrl"] = request.TaskUrl ?? "#"
        };

        var recipients = request.EscalationRecipients.Select(e => new EmailAddress(e)).ToList();

        await SendTemplatedAsync(new TemplatedEmailRequest
        {
            TemplateKey = "task-escalation",
            To = recipients,
            Variables = variables
        }, cancellationToken);
    }

    public Task RegisterTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _templates[template.Key] = template;
        _logger.LogInformation("Registered email template: {TemplateKey}", template.Key);
        return Task.CompletedTask;
    }

    public Task<EmailTemplate?> GetTemplateAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        _templates.TryGetValue(templateKey, out var template);
        return Task.FromResult(template);
    }

    #region Private Methods

    private async Task SendViaSmtpAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (_config.Smtp == null)
            throw new InvalidOperationException("SMTP configuration is missing");

        using var client = new SmtpClient(_config.Smtp.Host, _config.Smtp.Port)
        {
            EnableSsl = _config.Smtp.UseSsl,
            Credentials = !string.IsNullOrEmpty(_config.Smtp.Username)
                ? new NetworkCredential(_config.Smtp.Username, _config.Smtp.Password)
                : null
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(message.From!.Email, message.From.Name),
            Subject = message.Subject,
            Body = message.HtmlBody ?? message.TextBody ?? "",
            IsBodyHtml = !string.IsNullOrEmpty(message.HtmlBody)
        };

        foreach (var to in message.To)
        {
            mailMessage.To.Add(new MailAddress(to.Email, to.Name));
        }

        foreach (var cc in message.Cc)
        {
            mailMessage.CC.Add(new MailAddress(cc.Email, cc.Name));
        }

        foreach (var bcc in message.Bcc)
        {
            mailMessage.Bcc.Add(new MailAddress(bcc.Email, bcc.Name));
        }

        if (message.ReplyTo != null)
        {
            mailMessage.ReplyToList.Add(new MailAddress(message.ReplyTo.Email, message.ReplyTo.Name));
        }

        foreach (var attachment in message.Attachments)
        {
            var stream = new System.IO.MemoryStream(attachment.Content);
            mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
        }

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    private Task SendViaSendGridAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        // SendGrid implementation would go here
        // For now, log and return
        _logger.LogInformation("Would send email via SendGrid to {Recipients}",
            string.Join(", ", message.To.Select(t => t.Email)));
        return Task.CompletedTask;
    }

    private void LogEmailToConsole(EmailMessage message)
    {
        _logger.LogInformation(
            "EMAIL (Console Mode)\n" +
            "To: {To}\n" +
            "Cc: {Cc}\n" +
            "Subject: {Subject}\n" +
            "Body:\n{Body}",
            string.Join(", ", message.To.Select(t => t.ToString())),
            string.Join(", ", message.Cc.Select(c => c.ToString())),
            message.Subject,
            message.TextBody ?? StripHtml(message.HtmlBody ?? ""));
    }

    private static string RenderTemplate(string template, Dictionary<string, object> variables)
    {
        var result = template;

        // Simple {{variable}} replacement
        foreach (var kvp in variables)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            var value = kvp.Value?.ToString() ?? "";
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        // Handle conditionals: {{#if variable}}content{{/if}}
        result = Regex.Replace(result, @"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", match =>
        {
            var varName = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (variables.TryGetValue(varName, out var value))
            {
                var hasValue = value switch
                {
                    null => false,
                    string s => !string.IsNullOrEmpty(s),
                    bool b => b,
                    _ => true
                };
                return hasValue ? content : "";
            }
            return "";
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return result;
    }

    private static string StripHtml(string html)
    {
        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    private static string GetPriorityLabel(int priority)
    {
        return priority switch
        {
            >= 75 => "Critical",
            >= 50 => "High",
            >= 25 => "Normal",
            _ => "Low"
        };
    }

    private static string FormatTimeSpan(TimeSpan? timeSpan)
    {
        if (timeSpan == null)
            return "N/A";

        var ts = timeSpan.Value;
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays} day(s) {ts.Hours} hour(s)";
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours} hour(s) {ts.Minutes} minute(s)";
        return $"{(int)ts.TotalMinutes} minute(s)";
    }

    private void RegisterDefaultTemplates()
    {
        // Task Assignment Template
        _templates["task-assignment"] = new EmailTemplate
        {
            Key = "task-assignment",
            Name = "Task Assignment Notification",
            SubjectTemplate = "New Task Assigned: {{taskName}}",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #4a90d9; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #f9f9f9; } .button { display: inline-block; padding: 10px 20px; background: #4a90d9; color: white; text-decoration: none; border-radius: 5px; } .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>New Task Assigned</h1></div>
    <div class='content'>
        <p>Hello {{assigneeName}},</p>
        <p>A new task has been assigned to you:</p>
        <table style='width:100%; border-collapse: collapse;'>
            <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Task:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{{taskName}}</td></tr>
            <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Process:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{{processName}}</td></tr>
            <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Due Date:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{{dueDate}}</td></tr>
            <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Priority:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{{priority}}</td></tr>
            <tr><td style='padding: 8px;'><strong>Assigned By:</strong></td><td style='padding: 8px;'>{{assignedByName}}</td></tr>
        </table>
        {{#if taskDescription}}<p><strong>Description:</strong><br>{{taskDescription}}</p>{{/if}}
        <p style='text-align: center; margin-top: 20px;'><a href='{{taskUrl}}' class='button'>View Task</a></p>
    </div>
    <div class='footer'>This is an automated message from the Workflow System.</div>
</div>
</body>
</html>",
            TextBodyTemplate = @"Hello {{assigneeName}},

A new task has been assigned to you:

Task: {{taskName}}
Process: {{processName}}
Due Date: {{dueDate}}
Priority: {{priority}}
Assigned By: {{assignedByName}}

View task at: {{taskUrl}}

This is an automated message from the Workflow System."
        };

        // Task Reminder Template
        _templates["task-reminder"] = new EmailTemplate
        {
            Key = "task-reminder",
            Name = "Task Reminder",
            SubjectTemplate = "Reminder: Task '{{taskName}}' needs your attention",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #f0ad4e; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #f9f9f9; } .button { display: inline-block; padding: 10px 20px; background: #f0ad4e; color: white; text-decoration: none; border-radius: 5px; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>Task Reminder</h1></div>
    <div class='content'>
        <p>Hello {{assigneeName}},</p>
        <p>This is reminder #{{reminderNumber}} for your pending task:</p>
        <p><strong>Task:</strong> {{taskName}}<br><strong>Process:</strong> {{processName}}<br><strong>Due Date:</strong> {{dueDate}}<br><strong>Time Remaining:</strong> {{timeRemaining}}</p>
        <p style='text-align: center;'><a href='{{taskUrl}}' class='button'>Complete Task</a></p>
    </div>
</div>
</body>
</html>"
        };

        // Task Due Soon Template
        _templates["task-due-soon"] = new EmailTemplate
        {
            Key = "task-due-soon",
            Name = "Task Due Soon Warning",
            SubjectTemplate = "⚠️ Task '{{taskName}}' is due soon",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #f0ad4e; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #fff3cd; } .button { display: inline-block; padding: 10px 20px; background: #f0ad4e; color: white; text-decoration: none; border-radius: 5px; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>⚠️ Task Due Soon</h1></div>
    <div class='content'>
        <p>Hello {{assigneeName}},</p>
        <p>Your task <strong>{{taskName}}</strong> is due in <strong>{{timeRemaining}}</strong>.</p>
        <p><strong>Due Date:</strong> {{dueDate}}</p>
        <p style='text-align: center;'><a href='{{taskUrl}}' class='button'>Complete Task Now</a></p>
    </div>
</div>
</body>
</html>"
        };

        // Task Overdue Template
        _templates["task-overdue"] = new EmailTemplate
        {
            Key = "task-overdue",
            Name = "Task Overdue Alert",
            SubjectTemplate = "🚨 OVERDUE: Task '{{taskName}}'",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #d9534f; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #f2dede; } .button { display: inline-block; padding: 10px 20px; background: #d9534f; color: white; text-decoration: none; border-radius: 5px; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>🚨 Task Overdue</h1></div>
    <div class='content'>
        <p>Hello {{assigneeName}},</p>
        <p>Your task <strong>{{taskName}}</strong> is now <strong>OVERDUE</strong> by {{timeRemaining}}.</p>
        <p><strong>Original Due Date:</strong> {{dueDate}}</p>
        <p>Please complete this task as soon as possible.</p>
        <p style='text-align: center;'><a href='{{taskUrl}}' class='button'>Complete Task Now</a></p>
    </div>
</div>
</body>
</html>"
        };

        // Process Completion Template
        _templates["process-completion"] = new EmailTemplate
        {
            Key = "process-completion",
            Name = "Process Completion Notification",
            SubjectTemplate = "✅ Process Completed: {{processName}}",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #5cb85c; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #dff0d8; } .button { display: inline-block; padding: 10px 20px; background: #5cb85c; color: white; text-decoration: none; border-radius: 5px; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>✅ Process Completed</h1></div>
    <div class='content'>
        <p>The following process has been completed successfully:</p>
        <table style='width:100%;'>
            <tr><td><strong>Process:</strong></td><td>{{processName}}</td></tr>
            <tr><td><strong>Business Key:</strong></td><td>{{businessKey}}</td></tr>
            <tr><td><strong>Started:</strong></td><td>{{startedAt}}</td></tr>
            <tr><td><strong>Completed:</strong></td><td>{{completedAt}}</td></tr>
            <tr><td><strong>Duration:</strong></td><td>{{duration}}</td></tr>
        </table>
        <p style='text-align: center;'><a href='{{processUrl}}' class='button'>View Details</a></p>
    </div>
</div>
</body>
</html>"
        };

        // Escalation Template
        _templates["task-escalation"] = new EmailTemplate
        {
            Key = "task-escalation",
            Name = "Task Escalation Alert",
            SubjectTemplate = "🔴 ESCALATION: Task '{{taskName}}' requires attention",
            HtmlBodyTemplate = @"
<!DOCTYPE html>
<html>
<head><style>body { font-family: Arial, sans-serif; } .container { max-width: 600px; margin: 0 auto; padding: 20px; } .header { background: #d9534f; color: white; padding: 20px; text-align: center; } .content { padding: 20px; background: #f2dede; } .button { display: inline-block; padding: 10px 20px; background: #d9534f; color: white; text-decoration: none; border-radius: 5px; }</style></head>
<body>
<div class='container'>
    <div class='header'><h1>🔴 Task Escalation</h1></div>
    <div class='content'>
        <p>A task has been escalated to your attention:</p>
        <table style='width:100%;'>
            <tr><td><strong>Task:</strong></td><td>{{taskName}}</td></tr>
            <tr><td><strong>Process:</strong></td><td>{{processName}}</td></tr>
            <tr><td><strong>Original Assignee:</strong></td><td>{{originalAssignee}}</td></tr>
            <tr><td><strong>Due Date:</strong></td><td>{{dueDate}}</td></tr>
            <tr><td><strong>Overdue By:</strong></td><td>{{overdueBy}}</td></tr>
            <tr><td><strong>Escalation Level:</strong></td><td>{{escalationLevel}}</td></tr>
            <tr><td><strong>Reason:</strong></td><td>{{escalationReason}}</td></tr>
        </table>
        <p style='text-align: center;'><a href='{{taskUrl}}' class='button'>Take Action</a></p>
    </div>
</div>
</body>
</html>"
        };
    }

    #endregion
}

/// <summary>
/// Event handler that sends email notifications for workflow events.
/// </summary>
public class EmailNotificationEventHandler : IExternalEventHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationEventHandler> _logger;

    public EmailNotificationEventHandler(IEmailService emailService, ILogger<EmailNotificationEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public Task<bool> CanHandleAsync(WorkflowEvent @event, CancellationToken cancellationToken)
    {
        // Handle task and process events
        return Task.FromResult(@event.EventType.StartsWith("task.", StringComparison.OrdinalIgnoreCase) ||
                               @event.EventType.StartsWith("process.", StringComparison.OrdinalIgnoreCase));
    }

    public async Task HandleAsync(WorkflowEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            switch (@event)
            {
                case TaskCreatedEvent taskCreated:
                    if (!string.IsNullOrEmpty(taskCreated.Assignee))
                    {
                        await _emailService.SendTaskAssignmentAsync(new TaskAssignmentEmailRequest
                        {
                            TaskId = taskCreated.TaskId,
                            TaskName = taskCreated.Name,
                            AssigneeEmail = taskCreated.Assignee,
                            DueDate = taskCreated.DueDate,
                            Priority = taskCreated.Priority
                        }, cancellationToken);
                    }
                    break;

                case TaskDueDateApproachingEvent dueDateApproaching:
                    await _emailService.SendTaskDueDateWarningAsync(new TaskDueDateEmailRequest
                    {
                        TaskId = dueDateApproaching.TaskId,
                        TaskName = "Task", // Would need task name from context
                        AssigneeEmail = dueDateApproaching.Assignee,
                        DueDate = dueDateApproaching.DueDate,
                        TimeRemaining = dueDateApproaching.TimeRemaining,
                        IsOverdue = false
                    }, cancellationToken);
                    break;

                case TaskOverdueEvent taskOverdue:
                    await _emailService.SendTaskDueDateWarningAsync(new TaskDueDateEmailRequest
                    {
                        TaskId = taskOverdue.TaskId,
                        TaskName = "Task",
                        AssigneeEmail = taskOverdue.Assignee,
                        DueDate = taskOverdue.DueDate,
                        TimeRemaining = taskOverdue.OverdueBy,
                        IsOverdue = true
                    }, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification for event {EventType}", @event.EventType);
        }
    }
}
