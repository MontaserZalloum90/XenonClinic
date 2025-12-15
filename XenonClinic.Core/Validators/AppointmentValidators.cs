using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

/// <summary>
/// Validator for CreateAppointmentDto.
/// </summary>
public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
{
    private const int MinDurationMinutes = 5;
    private const int MaxDurationMinutes = 480; // 8 hours

    public CreateAppointmentValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(AppointmentValidationMessages.PatientIdRequired);

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.StartTimeRequired)
            .Must(BeInTheFuture).WithMessage(AppointmentValidationMessages.StartTimeInPast);

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.EndTimeRequired)
            .GreaterThan(x => x.StartTime).WithMessage(AppointmentValidationMessages.EndTimeBeforeStartTime);

        RuleFor(x => x)
            .Must(HaveValidDuration).WithMessage(AppointmentValidationMessages.InvalidDuration);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(AppointmentValidationMessages.InvalidType);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private static bool BeInTheFuture(DateTime startTime)
    {
        // Allow appointments starting today (same day scheduling)
        return startTime.Date >= DateTime.UtcNow.Date;
    }

    private static bool HaveValidDuration(CreateAppointmentDto dto)
    {
        var duration = (dto.EndTime - dto.StartTime).TotalMinutes;
        return duration >= MinDurationMinutes && duration <= MaxDurationMinutes;
    }
}

/// <summary>
/// Validator for UpdateAppointmentDto.
/// </summary>
public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentDto>
{
    private const int MinDurationMinutes = 5;
    private const int MaxDurationMinutes = 480;

    public UpdateAppointmentValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid appointment ID");

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(AppointmentValidationMessages.PatientIdRequired);

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.StartTimeRequired);

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.EndTimeRequired)
            .GreaterThan(x => x.StartTime).WithMessage(AppointmentValidationMessages.EndTimeBeforeStartTime);

        RuleFor(x => x)
            .Must(HaveValidDuration).WithMessage(AppointmentValidationMessages.InvalidDuration);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(AppointmentValidationMessages.InvalidType);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private static bool HaveValidDuration(UpdateAppointmentDto dto)
    {
        var duration = (dto.EndTime - dto.StartTime).TotalMinutes;
        return duration >= MinDurationMinutes && duration <= MaxDurationMinutes;
    }
}

/// <summary>
/// Validator for RescheduleAppointmentDto.
/// </summary>
public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentDto>
{
    private const int MinDurationMinutes = 5;
    private const int MaxDurationMinutes = 480;

    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .GreaterThan(0).WithMessage("Invalid appointment ID");

        RuleFor(x => x.NewStartTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.StartTimeRequired)
            .Must(BeInTheFuture).WithMessage(AppointmentValidationMessages.StartTimeInPast);

        RuleFor(x => x.NewEndTime)
            .NotEmpty().WithMessage(AppointmentValidationMessages.EndTimeRequired)
            .GreaterThan(x => x.NewStartTime).WithMessage(AppointmentValidationMessages.EndTimeBeforeStartTime);

        RuleFor(x => x)
            .Must(HaveValidDuration).WithMessage(AppointmentValidationMessages.InvalidDuration);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }

    private static bool BeInTheFuture(DateTime startTime)
    {
        return startTime.Date >= DateTime.UtcNow.Date;
    }

    private static bool HaveValidDuration(RescheduleAppointmentDto dto)
    {
        var duration = (dto.NewEndTime - dto.NewStartTime).TotalMinutes;
        return duration >= MinDurationMinutes && duration <= MaxDurationMinutes;
    }
}

/// <summary>
/// Validator for CancelAppointmentDto.
/// </summary>
public class CancelAppointmentValidator : AbstractValidator<CancelAppointmentDto>
{
    public CancelAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .GreaterThan(0).WithMessage("Invalid appointment ID");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for AppointmentListRequestDto.
/// </summary>
public class AppointmentListRequestValidator : AbstractValidator<AppointmentListRequestDto>
{
    private static readonly string[] ValidSortFields =
    {
        "StartTime", "PatientName", "ProviderName", "Status", "Type"
    };

    public AppointmentListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .WithMessage(AppointmentValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage(AppointmentValidationMessages.DateRangeTooLarge)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("Invalid provider ID")
            .When(x => x.ProviderId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid type")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private static bool HaveValidDateRange(AppointmentListRequestDto dto)
    {
        if (!dto.DateFrom.HasValue || !dto.DateTo.HasValue)
            return true;

        var daysDifference = (dto.DateTo.Value - dto.DateFrom.Value).TotalDays;
        return daysDifference <= 90;
    }

    private static bool BeValidSortField(string? field)
    {
        return string.IsNullOrEmpty(field) ||
               ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for AvailabilityRequestDto.
/// </summary>
public class AvailabilityRequestValidator : AbstractValidator<AvailabilityRequestDto>
{
    public AvailabilityRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(BeValidDate).WithMessage("Date must be today or in the future");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Duration must be between 5 and 480 minutes");

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("Invalid provider ID")
            .When(x => x.ProviderId.HasValue);
    }

    private static bool BeValidDate(DateTime date)
    {
        return date.Date >= DateTime.UtcNow.Date;
    }
}

/// <summary>
/// Extension methods for registering appointment validators.
/// </summary>
public static class AppointmentValidatorExtensions
{
    public static IServiceCollection AddAppointmentValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateAppointmentDto>, CreateAppointmentValidator>();
        services.AddScoped<IValidator<UpdateAppointmentDto>, UpdateAppointmentValidator>();
        services.AddScoped<IValidator<RescheduleAppointmentDto>, RescheduleAppointmentValidator>();
        services.AddScoped<IValidator<CancelAppointmentDto>, CancelAppointmentValidator>();
        services.AddScoped<IValidator<AppointmentListRequestDto>, AppointmentListRequestValidator>();
        services.AddScoped<IValidator<AvailabilityRequestDto>, AvailabilityRequestValidator>();

        return services;
    }
}
