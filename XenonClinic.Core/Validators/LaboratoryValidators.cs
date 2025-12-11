using FluentValidation;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

#region Lab Test Validators

/// <summary>
/// Validator for CreateLabTestDto.
/// </summary>
public class CreateLabTestValidator : AbstractValidator<CreateLabTestDto>
{
    public CreateLabTestValidator()
    {
        RuleFor(x => x.TestCode)
            .NotEmpty().WithMessage(LabValidationMessages.TestCodeRequired)
            .MaximumLength(50).WithMessage(LabValidationMessages.TestCodeTooLong)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage(LabValidationMessages.TestCodeInvalid);

        RuleFor(x => x.TestName)
            .NotEmpty().WithMessage(LabValidationMessages.TestNameRequired)
            .MaximumLength(200).WithMessage(LabValidationMessages.TestNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(LabValidationMessages.CategoryInvalid);

        RuleFor(x => x.SpecimenType)
            .IsInEnum().WithMessage("Invalid specimen type")
            .When(x => x.SpecimenType.HasValue);

        RuleFor(x => x.SpecimenVolume)
            .MaximumLength(100).WithMessage("Specimen volume cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SpecimenVolume));

        RuleFor(x => x.TurnaroundTimeHours)
            .GreaterThan(0).WithMessage(LabValidationMessages.TurnaroundTimeInvalid)
            .When(x => x.TurnaroundTimeHours.HasValue);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(LabValidationMessages.PriceInvalid);

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Unit));

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);
    }
}

/// <summary>
/// Validator for UpdateLabTestDto.
/// </summary>
public class UpdateLabTestValidator : AbstractValidator<UpdateLabTestDto>
{
    public UpdateLabTestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Lab test ID is required");

        RuleFor(x => x.TestCode)
            .NotEmpty().WithMessage(LabValidationMessages.TestCodeRequired)
            .MaximumLength(50).WithMessage(LabValidationMessages.TestCodeTooLong)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage(LabValidationMessages.TestCodeInvalid);

        RuleFor(x => x.TestName)
            .NotEmpty().WithMessage(LabValidationMessages.TestNameRequired)
            .MaximumLength(200).WithMessage(LabValidationMessages.TestNameTooLong);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(LabValidationMessages.CategoryInvalid);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(LabValidationMessages.PriceInvalid);

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);
    }
}

/// <summary>
/// Validator for LabTestListRequestDto.
/// </summary>
public class LabTestListRequestValidator : AbstractValidator<LabTestListRequestDto>
{
    public LabTestListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(LabValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(LabValidationMessages.InvalidPageSize);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(LabValidationMessages.CategoryInvalid)
            .When(x => x.Category.HasValue);

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);
    }
}

#endregion

#region Lab Order Validators

/// <summary>
/// Validator for CreateLabOrderDto.
/// </summary>
public class CreateLabOrderValidator : AbstractValidator<CreateLabOrderDto>
{
    public CreateLabOrderValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(LabValidationMessages.PatientRequired);

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);

        RuleFor(x => x.ClinicalNotes)
            .MaximumLength(2000).WithMessage("Clinical notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalNotes));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(LabValidationMessages.OrderItemsRequired)
            .Must(items => items.Count <= 50).WithMessage("Order cannot have more than 50 items");

        RuleForEach(x => x.Items).SetValidator(new CreateLabOrderItemValidator());
    }
}

/// <summary>
/// Validator for CreateLabOrderItemDto.
/// </summary>
public class CreateLabOrderItemValidator : AbstractValidator<CreateLabOrderItemDto>
{
    public CreateLabOrderItemValidator()
    {
        RuleFor(x => x.LabTestId)
            .GreaterThan(0).WithMessage("Lab test ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateLabOrderDto.
/// </summary>
public class UpdateLabOrderValidator : AbstractValidator<UpdateLabOrderDto>
{
    public UpdateLabOrderValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(LabValidationMessages.OrderIdRequired);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(LabValidationMessages.OrderStatusInvalid);

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);

        RuleFor(x => x.ClinicalNotes)
            .MaximumLength(2000).WithMessage("Clinical notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalNotes));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateLabOrderStatusDto.
/// </summary>
public class UpdateLabOrderStatusValidator : AbstractValidator<UpdateLabOrderStatusDto>
{
    public UpdateLabOrderStatusValidator()
    {
        RuleFor(x => x.LabOrderId)
            .GreaterThan(0).WithMessage(LabValidationMessages.OrderIdRequired);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(LabValidationMessages.OrderStatusInvalid);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for CollectSamplesDto.
/// </summary>
public class CollectSamplesValidator : AbstractValidator<CollectSamplesDto>
{
    public CollectSamplesValidator()
    {
        RuleFor(x => x.LabOrderId)
            .GreaterThan(0).WithMessage(LabValidationMessages.OrderIdRequired);

        RuleFor(x => x.CollectionDate)
            .NotEmpty().WithMessage(LabValidationMessages.CollectionDateRequired)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(LabValidationMessages.CollectionDateFuture);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for LabOrderListRequestDto.
/// </summary>
public class LabOrderListRequestValidator : AbstractValidator<LabOrderListRequestDto>
{
    public LabOrderListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(LabValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(LabValidationMessages.InvalidPageSize);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(LabValidationMessages.OrderStatusInvalid)
            .When(x => x.Status.HasValue);

        RuleFor(x => x.ExternalLabId)
            .GreaterThan(0).WithMessage("Invalid external lab ID")
            .When(x => x.ExternalLabId.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(LabValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}

#endregion

#region Lab Result Validators

/// <summary>
/// Validator for EnterLabResultDto.
/// </summary>
public class EnterLabResultValidator : AbstractValidator<EnterLabResultDto>
{
    public EnterLabResultValidator()
    {
        RuleFor(x => x.LabOrderItemId)
            .GreaterThan(0).WithMessage(LabValidationMessages.LabOrderItemRequired);

        RuleFor(x => x.ResultValue)
            .MaximumLength(100).WithMessage("Result value cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ResultValue));

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Unit));

        RuleFor(x => x.Interpretation)
            .MaximumLength(2000).WithMessage("Interpretation cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Interpretation));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for ReviewLabResultDto.
/// </summary>
public class ReviewLabResultValidator : AbstractValidator<ReviewLabResultDto>
{
    public ReviewLabResultValidator()
    {
        RuleFor(x => x.LabResultId)
            .GreaterThan(0).WithMessage(LabValidationMessages.ResultIdRequired);

        RuleFor(x => x.Interpretation)
            .MaximumLength(2000).WithMessage("Interpretation cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Interpretation));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for VerifyLabResultDto.
/// </summary>
public class VerifyLabResultValidator : AbstractValidator<VerifyLabResultDto>
{
    public VerifyLabResultValidator()
    {
        RuleFor(x => x.LabResultId)
            .GreaterThan(0).WithMessage(LabValidationMessages.ResultIdRequired);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

#endregion

#region External Lab Validators

/// <summary>
/// Validator for CreateExternalLabDto.
/// </summary>
public class CreateExternalLabValidator : AbstractValidator<CreateExternalLabDto>
{
    public CreateExternalLabValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LabValidationMessages.LabNameRequired)
            .MaximumLength(100).WithMessage(LabValidationMessages.LabNameTooLong);

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(LabValidationMessages.EmailInvalid)
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .Matches(@"^[\d\-\+\s\(\)]+$").WithMessage(LabValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile cannot exceed 20 characters")
            .Matches(@"^[\d\-\+\s\(\)]+$").WithMessage(LabValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.LicenseNumber)
            .MaximumLength(50).WithMessage("License number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.LicenseNumber));

        RuleFor(x => x.TurnaroundTimeDays)
            .GreaterThan(0).WithMessage("Turnaround time must be greater than 0")
            .When(x => x.TurnaroundTimeDays.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateExternalLabDto.
/// </summary>
public class UpdateExternalLabValidator : AbstractValidator<UpdateExternalLabDto>
{
    public UpdateExternalLabValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("External lab ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LabValidationMessages.LabNameRequired)
            .MaximumLength(100).WithMessage(LabValidationMessages.LabNameTooLong);

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(LabValidationMessages.EmailInvalid)
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .Matches(@"^[\d\-\+\s\(\)]+$").WithMessage(LabValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.TurnaroundTimeDays)
            .GreaterThan(0).WithMessage("Turnaround time must be greater than 0")
            .When(x => x.TurnaroundTimeDays.HasValue);
    }
}

#endregion
