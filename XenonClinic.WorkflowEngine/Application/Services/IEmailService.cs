using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Email service interface for workflow notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    Task<EmailSendResult> SendTemplatedAsync(TemplatedEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends task assignment notification.
    /// </summary>
    Task SendTaskAssignmentAsync(TaskAssignmentEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends task reminder notification.
    /// </summary>
    Task SendTaskReminderAsync(TaskReminderEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends task due date approaching notification.
    /// </summary>
    Task SendTaskDueDateWarningAsync(TaskDueDateEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends process completion notification.
    /// </summary>
    Task SendProcessCompletionAsync(ProcessCompletionEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends escalation notification.
    /// </summary>
    Task SendEscalationAsync(EscalationEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers an email template.
    /// </summary>
    Task RegisterTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an email template by key.
    /// </summary>
    Task<EmailTemplate?> GetTemplateAsync(string templateKey, CancellationToken cancellationToken = default);
}

#region Email Message DTOs

public class EmailMessage
{
    public string? MessageId { get; set; }
    public List<EmailAddress> To { get; set; } = new();
    public List<EmailAddress> Cc { get; set; } = new();
    public List<EmailAddress> Bcc { get; set; } = new();
    public EmailAddress? From { get; set; }
    public EmailAddress? ReplyTo { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
    public Dictionary<string, string> CustomHeaders { get; set; } = new();
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EmailAddress
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }

    public EmailAddress() { }

    public EmailAddress(string email, string? name = null)
    {
        Email = email;
        Name = name;
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? Email : $"{Name} <{Email}>";
    }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}

public enum EmailPriority
{
    Low,
    Normal,
    High
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Templated Email DTOs

public class TemplatedEmailRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public List<EmailAddress> To { get; set; } = new();
    public List<EmailAddress> Cc { get; set; } = new();
    public EmailAddress? From { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<EmailAttachment> Attachments { get; set; } = new();
    public string? Locale { get; set; } = "en";
}

public class EmailTemplate
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string? TextBodyTemplate { get; set; }
    public string HtmlBodyTemplate { get; set; } = string.Empty;
    public string Locale { get; set; } = "en";
    public Dictionary<string, string> RequiredVariables { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

#endregion

#region Workflow Email Requests

public class TaskAssignmentEmailRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public string AssigneeEmail { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public string? AssignedByName { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? ProcessInstanceId { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
    public string? TaskUrl { get; set; }
    public Dictionary<string, object> AdditionalVariables { get; set; } = new();
}

public class TaskReminderEmailRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string AssigneeEmail { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
    public int ReminderNumber { get; set; }
    public string? TaskUrl { get; set; }
}

public class TaskDueDateEmailRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string AssigneeEmail { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public bool IsOverdue { get; set; }
    public string? TaskUrl { get; set; }
}

public class ProcessCompletionEmailRequest
{
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string? BusinessKey { get; set; }
    public List<string> RecipientEmails { get; set; } = new();
    public string? InitiatorName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ProcessUrl { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}

public class EscalationEmailRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string EscalationLevel { get; set; } = string.Empty;
    public List<string> EscalationRecipients { get; set; } = new();
    public string? OriginalAssignee { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public TimeSpan? OverdueBy { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public string? TaskUrl { get; set; }
}

#endregion

#region Email Configuration

public class EmailConfiguration
{
    public string Provider { get; set; } = "Smtp"; // Smtp, SendGrid, AmazonSES, Mailgun
    public SmtpConfiguration? Smtp { get; set; }
    public SendGridConfiguration? SendGrid { get; set; }
    public EmailAddress DefaultFrom { get; set; } = new("noreply@example.com", "Workflow System");
    public string? DefaultReplyTo { get; set; }
    public bool EnableTracking { get; set; } = true;
    public int MaxRetriesOnFailure { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
}

public class SmtpConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class SendGridConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public bool SandboxMode { get; set; } = false;
}

#endregion
