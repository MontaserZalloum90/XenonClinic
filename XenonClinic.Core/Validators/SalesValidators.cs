using FluentValidation;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

#region Sale Validators

/// <summary>
/// Validator for CreateSaleDto.
/// </summary>
public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.PatientRequired);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Terms)
            .MaximumLength(2000).WithMessage("Terms cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Terms));

        RuleFor(x => x.QuotationId)
            .GreaterThan(0).WithMessage("Invalid quotation ID")
            .When(x => x.QuotationId.HasValue);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(SalesValidationMessages.SaleItemsRequired)
            .Must(items => items.Count <= 100).WithMessage("Sale cannot have more than 100 items");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemValidator());
    }
}

/// <summary>
/// Validator for CreateSaleItemDto.
/// </summary>
public class CreateSaleItemValidator : AbstractValidator<CreateSaleItemDto>
{
    public CreateSaleItemValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage(SalesValidationMessages.ItemNameRequired)
            .MaximumLength(200).WithMessage(SalesValidationMessages.ItemNameTooLong);

        RuleFor(x => x.ItemDescription)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ItemDescription));

        RuleFor(x => x.ItemCode)
            .MaximumLength(50).WithMessage("Item code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ItemCode));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuantityInvalid);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage(SalesValidationMessages.UnitPriceInvalid);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.InventoryItemInvalid)
            .When(x => x.InventoryItemId.HasValue);

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100).WithMessage("Serial number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SerialNumber));

        RuleFor(x => x.WarrantyMonths)
            .InclusiveBetween(1, 120).WithMessage(SalesValidationMessages.WarrantyMonthsInvalid)
            .When(x => x.WarrantyMonths.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateSaleDto.
/// </summary>
public class UpdateSaleValidator : AbstractValidator<UpdateSaleDto>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(SalesValidationMessages.SaleIdRequired);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Terms)
            .MaximumLength(2000).WithMessage("Terms cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Terms));
    }
}

/// <summary>
/// Validator for SaleListRequestDto.
/// </summary>
public class SaleListRequestValidator : AbstractValidator<SaleListRequestDto>
{
    public SaleListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(SalesValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(SalesValidationMessages.InvalidPageSize);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.PatientInvalid)
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(SalesValidationMessages.SaleStatusInvalid)
            .When(x => x.Status.HasValue);

        RuleFor(x => x.PaymentStatus)
            .IsInEnum().WithMessage("Invalid payment status")
            .When(x => x.PaymentStatus.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(SalesValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}

#endregion

#region Payment Validators

/// <summary>
/// Validator for RecordSalePaymentDto.
/// </summary>
public class RecordSalePaymentValidator : AbstractValidator<RecordSalePaymentDto>
{
    public RecordSalePaymentValidator()
    {
        RuleFor(x => x.SaleId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.SaleIdRequired);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(SalesValidationMessages.PaymentAmountInvalid);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage(SalesValidationMessages.PaymentMethodInvalid);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        RuleFor(x => x.CardLastFourDigits)
            .Length(4).WithMessage("Card last four digits must be exactly 4 characters")
            .Matches(@"^\d{4}$").WithMessage("Card last four digits must be numeric")
            .When(x => !string.IsNullOrEmpty(x.CardLastFourDigits));

        RuleFor(x => x.InsuranceCompany)
            .MaximumLength(100).WithMessage("Insurance company cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.InsuranceCompany));

        RuleFor(x => x.InsuranceClaimNumber)
            .MaximumLength(50).WithMessage("Insurance claim number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.InsuranceClaimNumber));

        RuleFor(x => x.InsurancePolicyNumber)
            .MaximumLength(50).WithMessage("Insurance policy number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.InsurancePolicyNumber));

        RuleFor(x => x.InstallmentNumber)
            .GreaterThan(0).WithMessage(SalesValidationMessages.InstallmentNumberInvalid)
            .When(x => x.InstallmentNumber.HasValue);

        RuleFor(x => x.TotalInstallments)
            .GreaterThan(0).WithMessage(SalesValidationMessages.TotalInstallmentsInvalid)
            .When(x => x.TotalInstallments.HasValue);

        RuleFor(x => x.InstallmentNumber)
            .LessThanOrEqualTo(x => x.TotalInstallments ?? int.MaxValue)
            .WithMessage(SalesValidationMessages.InstallmentNumberExceedsTotal)
            .When(x => x.InstallmentNumber.HasValue && x.TotalInstallments.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

#endregion

#region Quotation Validators

/// <summary>
/// Validator for CreateQuotationDto.
/// </summary>
public class CreateQuotationValidator : AbstractValidator<CreateQuotationDto>
{
    public CreateQuotationValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.PatientRequired);

        RuleFor(x => x.ValidityDays)
            .InclusiveBetween(1, 365).WithMessage(SalesValidationMessages.ValidityDaysInvalid);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Terms)
            .MaximumLength(2000).WithMessage("Terms cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Terms));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(SalesValidationMessages.SaleItemsRequired)
            .Must(items => items.Count <= 100).WithMessage("Quotation cannot have more than 100 items");

        RuleForEach(x => x.Items).SetValidator(new CreateQuotationItemValidator());
    }
}

/// <summary>
/// Validator for CreateQuotationItemDto.
/// </summary>
public class CreateQuotationItemValidator : AbstractValidator<CreateQuotationItemDto>
{
    public CreateQuotationItemValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage(SalesValidationMessages.ItemNameRequired)
            .MaximumLength(200).WithMessage(SalesValidationMessages.ItemNameTooLong);

        RuleFor(x => x.ItemDescription)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ItemDescription));

        RuleFor(x => x.ItemCode)
            .MaximumLength(50).WithMessage("Item code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ItemCode));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuantityInvalid);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage(SalesValidationMessages.UnitPriceInvalid);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.InventoryItemInvalid)
            .When(x => x.InventoryItemId.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateQuotationDto.
/// </summary>
public class UpdateQuotationValidator : AbstractValidator<UpdateQuotationDto>
{
    public UpdateQuotationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuotationIdRequired);

        RuleFor(x => x.ValidityDays)
            .InclusiveBetween(1, 365).WithMessage(SalesValidationMessages.ValidityDaysInvalid);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(SalesValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Terms)
            .MaximumLength(2000).WithMessage("Terms cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Terms));
    }
}

/// <summary>
/// Validator for SendQuotationDto.
/// </summary>
public class SendQuotationValidator : AbstractValidator<SendQuotationDto>
{
    public SendQuotationValidator()
    {
        RuleFor(x => x.QuotationId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuotationIdRequired);

        RuleFor(x => x.EmailAddress)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.EmailAddress));

        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}

/// <summary>
/// Validator for AcceptQuotationDto.
/// </summary>
public class AcceptQuotationValidator : AbstractValidator<AcceptQuotationDto>
{
    public AcceptQuotationValidator()
    {
        RuleFor(x => x.QuotationId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuotationIdRequired);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for RejectQuotationDto.
/// </summary>
public class RejectQuotationValidator : AbstractValidator<RejectQuotationDto>
{
    public RejectQuotationValidator()
    {
        RuleFor(x => x.QuotationId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.QuotationIdRequired);

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage(SalesValidationMessages.RejectionReasonRequired)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for QuotationListRequestDto.
/// </summary>
public class QuotationListRequestValidator : AbstractValidator<QuotationListRequestDto>
{
    public QuotationListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(SalesValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(SalesValidationMessages.InvalidPageSize);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(SalesValidationMessages.PatientInvalid)
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(SalesValidationMessages.QuotationStatusInvalid)
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(SalesValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}

#endregion
