using FluentValidation;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Validators;

#region Imaging Study Validators

/// <summary>
/// Validator for CreateImagingStudyDto.
/// </summary>
public class CreateImagingStudyValidator : AbstractValidator<CreateImagingStudyDto>
{
    public CreateImagingStudyValidator()
    {
        RuleFor(x => x.StudyCode)
            .NotEmpty().WithMessage(RadiologyValidationMessages.StudyCodeRequired)
            .MaximumLength(50).WithMessage(RadiologyValidationMessages.StudyCodeTooLong);

        RuleFor(x => x.StudyName)
            .NotEmpty().WithMessage(RadiologyValidationMessages.StudyNameRequired)
            .MaximumLength(200).WithMessage(RadiologyValidationMessages.StudyNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Modality)
            .IsInEnum().WithMessage(RadiologyValidationMessages.ModalityInvalid);

        RuleFor(x => x.BodyPart)
            .MaximumLength(100).WithMessage("Body part cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BodyPart));

        RuleFor(x => x.EstimatedDurationMinutes)
            .InclusiveBetween(1, 480).WithMessage(RadiologyValidationMessages.DurationInvalid)
            .When(x => x.EstimatedDurationMinutes.HasValue);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(RadiologyValidationMessages.PriceInvalid);

        RuleFor(x => x.ContrastRequired)
            .MaximumLength(100).WithMessage("Contrast info cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContrastRequired));

        RuleFor(x => x.PatientPreparation)
            .MaximumLength(1000).WithMessage("Patient preparation cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.PatientPreparation));

        RuleFor(x => x.Contraindications)
            .MaximumLength(1000).WithMessage("Contraindications cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Contraindications));

        RuleFor(x => x.RadiationDose)
            .MaximumLength(100).WithMessage("Radiation dose cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.RadiationDose));
    }
}

/// <summary>
/// Validator for UpdateImagingStudyDto.
/// </summary>
public class UpdateImagingStudyValidator : AbstractValidator<UpdateImagingStudyDto>
{
    public UpdateImagingStudyValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.StudyIdRequired);

        RuleFor(x => x.StudyCode)
            .NotEmpty().WithMessage(RadiologyValidationMessages.StudyCodeRequired)
            .MaximumLength(50).WithMessage(RadiologyValidationMessages.StudyCodeTooLong);

        RuleFor(x => x.StudyName)
            .NotEmpty().WithMessage(RadiologyValidationMessages.StudyNameRequired)
            .MaximumLength(200).WithMessage(RadiologyValidationMessages.StudyNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Modality)
            .IsInEnum().WithMessage(RadiologyValidationMessages.ModalityInvalid);

        RuleFor(x => x.BodyPart)
            .MaximumLength(100).WithMessage("Body part cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BodyPart));

        RuleFor(x => x.EstimatedDurationMinutes)
            .InclusiveBetween(1, 480).WithMessage(RadiologyValidationMessages.DurationInvalid)
            .When(x => x.EstimatedDurationMinutes.HasValue);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(RadiologyValidationMessages.PriceInvalid);

        RuleFor(x => x.ContrastRequired)
            .MaximumLength(100).WithMessage("Contrast info cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContrastRequired));

        RuleFor(x => x.PatientPreparation)
            .MaximumLength(1000).WithMessage("Patient preparation cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.PatientPreparation));

        RuleFor(x => x.Contraindications)
            .MaximumLength(1000).WithMessage("Contraindications cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Contraindications));

        RuleFor(x => x.RadiationDose)
            .MaximumLength(100).WithMessage("Radiation dose cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.RadiationDose));
    }
}

/// <summary>
/// Validator for ImagingStudyListRequestDto.
/// </summary>
public class ImagingStudyListRequestValidator : AbstractValidator<ImagingStudyListRequestDto>
{
    public ImagingStudyListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(RadiologyValidationMessages.InvalidPageSize);

        RuleFor(x => x.Modality)
            .IsInEnum().WithMessage(RadiologyValidationMessages.ModalityInvalid)
            .When(x => x.Modality.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.BodyPart)
            .MaximumLength(100).WithMessage("Body part filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BodyPart));
    }
}

#endregion

#region Radiology Order Validators

/// <summary>
/// Validator for CreateRadiologyOrderDto.
/// </summary>
public class CreateRadiologyOrderValidator : AbstractValidator<CreateRadiologyOrderDto>
{
    public CreateRadiologyOrderValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.PatientRequired);

        RuleFor(x => x.AppointmentId)
            .GreaterThan(0).WithMessage("Invalid appointment ID")
            .When(x => x.AppointmentId.HasValue);

        RuleFor(x => x.ClinicalVisitId)
            .GreaterThan(0).WithMessage("Invalid clinical visit ID")
            .When(x => x.ClinicalVisitId.HasValue);

        RuleFor(x => x.ReferringDoctorId)
            .GreaterThan(0).WithMessage("Invalid referring doctor ID")
            .When(x => x.ReferringDoctorId.HasValue);

        RuleFor(x => x.ClinicalHistory)
            .MaximumLength(2000).WithMessage("Clinical history cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalHistory));

        RuleFor(x => x.ClinicalIndication)
            .MaximumLength(1000).WithMessage("Clinical indication cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalIndication));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTime.UtcNow.AddHours(-1)).WithMessage("Scheduled date cannot be in the past")
            .When(x => x.ScheduledDate.HasValue);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(RadiologyValidationMessages.OrderItemsRequired)
            .Must(items => items.Count <= 50).WithMessage("Order cannot have more than 50 studies");

        RuleForEach(x => x.Items).SetValidator(new CreateRadiologyOrderItemValidator());
    }
}

/// <summary>
/// Validator for CreateRadiologyOrderItemDto.
/// </summary>
public class CreateRadiologyOrderItemValidator : AbstractValidator<CreateRadiologyOrderItemDto>
{
    public CreateRadiologyOrderItemValidator()
    {
        RuleFor(x => x.ImagingStudyId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.StudyIdRequired);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative")
            .When(x => x.DiscountAmount.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.SpecialInstructions));
    }
}

/// <summary>
/// Validator for UpdateRadiologyOrderDto.
/// </summary>
public class UpdateRadiologyOrderValidator : AbstractValidator<UpdateRadiologyOrderDto>
{
    public UpdateRadiologyOrderValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.ReferringDoctorId)
            .GreaterThan(0).WithMessage("Invalid referring doctor ID")
            .When(x => x.ReferringDoctorId.HasValue);

        RuleFor(x => x.ClinicalHistory)
            .MaximumLength(2000).WithMessage("Clinical history cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalHistory));

        RuleFor(x => x.ClinicalIndication)
            .MaximumLength(1000).WithMessage("Clinical indication cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicalIndication));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTime.UtcNow.AddHours(-1)).WithMessage("Scheduled date cannot be in the past")
            .When(x => x.ScheduledDate.HasValue);
    }
}

/// <summary>
/// Validator for RadiologyOrderListRequestDto.
/// </summary>
public class RadiologyOrderListRequestValidator : AbstractValidator<RadiologyOrderListRequestDto>
{
    public RadiologyOrderListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(RadiologyValidationMessages.InvalidPageSize);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.PatientInvalid)
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.ReferringDoctorId)
            .GreaterThan(0).WithMessage("Invalid referring doctor ID")
            .When(x => x.ReferringDoctorId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(RadiologyValidationMessages.OrderStatusInvalid)
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(RadiologyValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}

#endregion

#region Imaging Result Validators

/// <summary>
/// Validator for CreateImagingResultDto.
/// </summary>
public class CreateImagingResultValidator : AbstractValidator<CreateImagingResultDto>
{
    public CreateImagingResultValidator()
    {
        RuleFor(x => x.RadiologyOrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.RadiologyOrderItemId)
            .GreaterThan(0).WithMessage("Radiology order item ID is required");

        RuleFor(x => x.ImagingStudyId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.StudyIdRequired);

        RuleFor(x => x.Findings)
            .MaximumLength(5000).WithMessage("Findings cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Findings));

        RuleFor(x => x.Impression)
            .MaximumLength(2000).WithMessage("Impression cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Impression));

        RuleFor(x => x.Recommendation)
            .MaximumLength(2000).WithMessage("Recommendation cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Recommendation));

        RuleFor(x => x.Technique)
            .MaximumLength(2000).WithMessage("Technique cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Technique));

        RuleFor(x => x.Comparison)
            .MaximumLength(1000).WithMessage("Comparison cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comparison));

        RuleFor(x => x.CriticalFindings)
            .NotEmpty().WithMessage(RadiologyValidationMessages.CriticalFindingsRequired)
            .When(x => x.IsCritical);

        RuleFor(x => x.CriticalFindings)
            .MaximumLength(2000).WithMessage("Critical findings cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CriticalFindings));

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));

        RuleFor(x => x.DicomStudyUID)
            .MaximumLength(100).WithMessage("DICOM Study UID cannot exceed 100 characters")
            .Matches(@"^[0-9.]+$").WithMessage(RadiologyValidationMessages.DicomUIDInvalid)
            .When(x => !string.IsNullOrEmpty(x.DicomStudyUID));

        RuleFor(x => x.PacsLink)
            .MaximumLength(500).WithMessage("PACS link cannot exceed 500 characters")
            .Must(BeAValidUrl).WithMessage(RadiologyValidationMessages.PacsLinkInvalid)
            .When(x => !string.IsNullOrEmpty(x.PacsLink));

        RuleFor(x => x.NumberOfImages)
            .GreaterThan(0).WithMessage("Number of images must be greater than 0")
            .When(x => x.NumberOfImages.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for UpdateImagingResultDto.
/// </summary>
public class UpdateImagingResultValidator : AbstractValidator<UpdateImagingResultDto>
{
    public UpdateImagingResultValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.ResultIdRequired);

        RuleFor(x => x.Findings)
            .MaximumLength(5000).WithMessage("Findings cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Findings));

        RuleFor(x => x.Impression)
            .MaximumLength(2000).WithMessage("Impression cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Impression));

        RuleFor(x => x.Recommendation)
            .MaximumLength(2000).WithMessage("Recommendation cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Recommendation));

        RuleFor(x => x.Technique)
            .MaximumLength(2000).WithMessage("Technique cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Technique));

        RuleFor(x => x.Comparison)
            .MaximumLength(1000).WithMessage("Comparison cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comparison));

        RuleFor(x => x.CriticalFindings)
            .NotEmpty().WithMessage(RadiologyValidationMessages.CriticalFindingsRequired)
            .When(x => x.IsCritical);

        RuleFor(x => x.CriticalFindings)
            .MaximumLength(2000).WithMessage("Critical findings cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CriticalFindings));

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));

        RuleFor(x => x.DicomStudyUID)
            .MaximumLength(100).WithMessage("DICOM Study UID cannot exceed 100 characters")
            .Matches(@"^[0-9.]+$").WithMessage(RadiologyValidationMessages.DicomUIDInvalid)
            .When(x => !string.IsNullOrEmpty(x.DicomStudyUID));

        RuleFor(x => x.PacsLink)
            .MaximumLength(500).WithMessage("PACS link cannot exceed 500 characters")
            .Must(BeAValidUrl).WithMessage(RadiologyValidationMessages.PacsLinkInvalid)
            .When(x => !string.IsNullOrEmpty(x.PacsLink));

        RuleFor(x => x.NumberOfImages)
            .GreaterThan(0).WithMessage("Number of images must be greater than 0")
            .When(x => x.NumberOfImages.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

#endregion

#region Workflow Validators

/// <summary>
/// Validator for ReceiveRadiologyOrderDto.
/// </summary>
public class ReceiveRadiologyOrderValidator : AbstractValidator<ReceiveRadiologyOrderDto>
{
    public ReceiveRadiologyOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for StartImagingDto.
/// </summary>
public class StartImagingValidator : AbstractValidator<StartImagingDto>
{
    public StartImagingValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.Technician)
            .MaximumLength(200).WithMessage("Technician name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Technician));

        RuleFor(x => x.Room)
            .MaximumLength(50).WithMessage("Room cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Room));

        RuleFor(x => x.Equipment)
            .MaximumLength(100).WithMessage("Equipment cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Equipment));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for CompleteRadiologyOrderDto.
/// </summary>
public class CompleteRadiologyOrderValidator : AbstractValidator<CompleteRadiologyOrderDto>
{
    public CompleteRadiologyOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.CompletedBy)
            .MaximumLength(200).WithMessage("Completed by cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CompletedBy));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for ApproveRadiologyOrderDto.
/// </summary>
public class ApproveRadiologyOrderValidator : AbstractValidator<ApproveRadiologyOrderDto>
{
    public ApproveRadiologyOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.ApprovedBy)
            .MaximumLength(200).WithMessage("Approved by cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for RejectRadiologyOrderDto.
/// </summary>
public class RejectRadiologyOrderValidator : AbstractValidator<RejectRadiologyOrderDto>
{
    public RejectRadiologyOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.OrderIdRequired);

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage(RadiologyValidationMessages.RejectionReasonRequired)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for AddImagingReportDto.
/// </summary>
public class AddImagingReportValidator : AbstractValidator<AddImagingReportDto>
{
    public AddImagingReportValidator()
    {
        RuleFor(x => x.ResultId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.ResultIdRequired);

        RuleFor(x => x.Findings)
            .NotEmpty().WithMessage(RadiologyValidationMessages.FindingsRequired)
            .MaximumLength(5000).WithMessage("Findings cannot exceed 5000 characters");

        RuleFor(x => x.Impression)
            .NotEmpty().WithMessage(RadiologyValidationMessages.ImpressionRequired)
            .MaximumLength(2000).WithMessage("Impression cannot exceed 2000 characters");

        RuleFor(x => x.Recommendation)
            .MaximumLength(2000).WithMessage("Recommendation cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Recommendation));

        RuleFor(x => x.Technique)
            .MaximumLength(2000).WithMessage("Technique cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Technique));

        RuleFor(x => x.Comparison)
            .MaximumLength(1000).WithMessage("Comparison cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comparison));

        RuleFor(x => x.CriticalFindings)
            .NotEmpty().WithMessage(RadiologyValidationMessages.CriticalFindingsRequired)
            .When(x => x.IsCritical);

        RuleFor(x => x.CriticalFindings)
            .MaximumLength(2000).WithMessage("Critical findings cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CriticalFindings));
    }
}

/// <summary>
/// Validator for VerifyImagingResultDto.
/// </summary>
public class VerifyImagingResultValidator : AbstractValidator<VerifyImagingResultDto>
{
    public VerifyImagingResultValidator()
    {
        RuleFor(x => x.ResultId)
            .GreaterThan(0).WithMessage(RadiologyValidationMessages.ResultIdRequired);

        RuleFor(x => x.VerifiedBy)
            .MaximumLength(200).WithMessage("Verified by cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.VerifiedBy));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

#endregion
