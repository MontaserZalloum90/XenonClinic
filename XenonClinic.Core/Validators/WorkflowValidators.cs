using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Validators;

/// <summary>
/// Validator for StartWorkflowRequestDto.
/// </summary>
public class StartWorkflowRequestValidator : AbstractValidator<StartWorkflowRequestDto>
{
    public StartWorkflowRequestValidator()
    {
        RuleFor(x => x.WorkflowId)
            .NotEmpty().WithMessage(WorkflowValidationMessages.WorkflowIdRequired)
            .MaximumLength(100).WithMessage("Workflow ID cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 100).WithMessage("Priority must be between 0 and 100")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.CorrelationId)
            .MaximumLength(100).WithMessage("Correlation ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CorrelationId));

        RuleFor(x => x.ScheduledStartTime)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("Scheduled start time cannot be in the past")
            .When(x => x.ScheduledStartTime.HasValue);

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than 0")
            .When(x => x.Version.HasValue);
    }
}

/// <summary>
/// Validator for ResumeWorkflowRequestDto.
/// </summary>
public class ResumeWorkflowRequestValidator : AbstractValidator<ResumeWorkflowRequestDto>
{
    public ResumeWorkflowRequestValidator()
    {
        RuleFor(x => x.BookmarkName)
            .NotEmpty().WithMessage(WorkflowValidationMessages.BookmarkNameRequired)
            .MaximumLength(200).WithMessage("Bookmark name cannot exceed 200 characters");
    }
}

/// <summary>
/// Validator for CancelWorkflowRequestDto.
/// </summary>
public class CancelWorkflowRequestValidator : AbstractValidator<CancelWorkflowRequestDto>
{
    public CancelWorkflowRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

/// <summary>
/// Validator for SendSignalRequestDto.
/// </summary>
public class SendSignalRequestValidator : AbstractValidator<SendSignalRequestDto>
{
    public SendSignalRequestValidator()
    {
        RuleFor(x => x.SignalName)
            .NotEmpty().WithMessage(WorkflowValidationMessages.SignalNameRequired)
            .MaximumLength(100).WithMessage("Signal name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Signal name must start with a letter and contain only letters, numbers, and underscores");
    }
}

/// <summary>
/// Validator for BroadcastSignalRequestDto.
/// </summary>
public class BroadcastSignalRequestValidator : AbstractValidator<BroadcastSignalRequestDto>
{
    public BroadcastSignalRequestValidator()
    {
        RuleFor(x => x.SignalName)
            .NotEmpty().WithMessage(WorkflowValidationMessages.SignalNameRequired)
            .MaximumLength(100).WithMessage("Signal name cannot exceed 100 characters");

        RuleFor(x => x.WorkflowId)
            .MaximumLength(100).WithMessage("Workflow ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.WorkflowId));
    }
}

/// <summary>
/// Validator for TriggerEventRequestDto.
/// </summary>
public class TriggerEventRequestValidator : AbstractValidator<TriggerEventRequestDto>
{
    public TriggerEventRequestValidator()
    {
        RuleFor(x => x.EventName)
            .NotEmpty().WithMessage(WorkflowValidationMessages.EventNameRequired)
            .MaximumLength(100).WithMessage("Event name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_\.]*$")
            .WithMessage("Event name must start with a letter and contain only letters, numbers, underscores, and dots");
    }
}

/// <summary>
/// Validator for WorkflowDefinitionListRequestDto.
/// </summary>
public class WorkflowDefinitionListRequestValidator : AbstractValidator<WorkflowDefinitionListRequestDto>
{
    private static readonly string[] ValidSortFields =
    {
        "Name", "CreatedAt", "UpdatedAt", "Category", "Version"
    };

    public WorkflowDefinitionListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private static bool BeValidSortField(string? field)
    {
        return string.IsNullOrEmpty(field) ||
               ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for WorkflowInstanceListRequestDto.
/// </summary>
public class WorkflowInstanceListRequestValidator : AbstractValidator<WorkflowInstanceListRequestDto>
{
    private static readonly string[] ValidSortFields =
    {
        "CreatedAt", "StartedAt", "CompletedAt", "Status", "Priority"
    };

    private static readonly string[] ValidStatuses =
    {
        "Pending", "Running", "Suspended", "Completed", "Faulted", "Cancelled", "Compensating", "Compensated"
    };

    public WorkflowInstanceListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.WorkflowId)
            .MaximumLength(100).WithMessage("Workflow ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.WorkflowId));

        RuleFor(x => x.CorrelationId)
            .MaximumLength(100).WithMessage("Correlation ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CorrelationId));

        RuleFor(x => x.Statuses)
            .Must(BeValidStatuses).WithMessage("Invalid status value")
            .When(x => x.Statuses != null && x.Statuses.Count > 0);

        RuleFor(x => x.CreatedAfter)
            .LessThanOrEqualTo(x => x.CreatedBefore)
            .WithMessage("CreatedAfter must be before or equal to CreatedBefore")
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private static bool BeValidSortField(string? field)
    {
        return string.IsNullOrEmpty(field) ||
               ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidStatuses(List<string>? statuses)
    {
        if (statuses == null || statuses.Count == 0)
            return true;

        return statuses.All(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Validator for WorkflowParameterDto.
/// </summary>
public class WorkflowParameterValidator : AbstractValidator<WorkflowParameterDto>
{
    private static readonly string[] ValidTypes =
    {
        "string", "number", "integer", "boolean", "date", "datetime", "object", "array"
    };

    public WorkflowParameterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Parameter name is required")
            .MaximumLength(100).WithMessage("Parameter name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Parameter name must start with a letter and contain only letters, numbers, and underscores");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Parameter type is required")
            .Must(BeValidType).WithMessage("Invalid parameter type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ValidationRule)
            .MaximumLength(200).WithMessage("Validation rule cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ValidationRule));
    }

    private static bool BeValidType(string type)
    {
        return ValidTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for WorkflowTriggerDto.
/// </summary>
public class WorkflowTriggerValidator : AbstractValidator<WorkflowTriggerDto>
{
    private static readonly string[] ValidTriggerTypes =
    {
        "Manual", "Scheduled", "Event", "Webhook", "Message"
    };

    public WorkflowTriggerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Trigger name is required")
            .MaximumLength(100).WithMessage("Trigger name cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Trigger type is required")
            .Must(BeValidTriggerType).WithMessage("Invalid trigger type");

        // Conditional validation based on type
        RuleFor(x => x.Config)
            .Must((dto, config) => ValidateTriggerConfig(dto.Type, config))
            .WithMessage("Invalid trigger configuration");
    }

    private static bool BeValidTriggerType(string type)
    {
        return ValidTriggerTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }

    private static bool ValidateTriggerConfig(string type, Dictionary<string, object?> config)
    {
        return type.ToLowerInvariant() switch
        {
            "scheduled" => config.ContainsKey("cron") && !string.IsNullOrEmpty(config["cron"]?.ToString()),
            "webhook" => config.ContainsKey("path") && !string.IsNullOrEmpty(config["path"]?.ToString()),
            "event" => config.ContainsKey("eventName") && !string.IsNullOrEmpty(config["eventName"]?.ToString()),
            _ => true
        };
    }
}

/// <summary>
/// Extension methods for registering workflow validators.
/// </summary>
public static class WorkflowValidatorExtensions
{
    public static IServiceCollection AddWorkflowValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<StartWorkflowRequestDto>, StartWorkflowRequestValidator>();
        services.AddScoped<IValidator<ResumeWorkflowRequestDto>, ResumeWorkflowRequestValidator>();
        services.AddScoped<IValidator<CancelWorkflowRequestDto>, CancelWorkflowRequestValidator>();
        services.AddScoped<IValidator<SendSignalRequestDto>, SendSignalRequestValidator>();
        services.AddScoped<IValidator<BroadcastSignalRequestDto>, BroadcastSignalRequestValidator>();
        services.AddScoped<IValidator<TriggerEventRequestDto>, TriggerEventRequestValidator>();
        services.AddScoped<IValidator<WorkflowDefinitionListRequestDto>, WorkflowDefinitionListRequestValidator>();
        services.AddScoped<IValidator<WorkflowInstanceListRequestDto>, WorkflowInstanceListRequestValidator>();
        services.AddScoped<IValidator<WorkflowParameterDto>, WorkflowParameterValidator>();
        services.AddScoped<IValidator<WorkflowTriggerDto>, WorkflowTriggerValidator>();

        return services;
    }
}
