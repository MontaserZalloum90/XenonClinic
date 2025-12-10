using FluentValidation;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

#region Account Validators

/// <summary>
/// Validator for CreateAccountDto.
/// </summary>
public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage(FinancialValidationMessages.AccountCodeRequired)
            .MaximumLength(50).WithMessage(FinancialValidationMessages.AccountCodeTooLong);

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage(FinancialValidationMessages.AccountNameRequired)
            .MaximumLength(200).WithMessage(FinancialValidationMessages.AccountNameTooLong);

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage(FinancialValidationMessages.AccountTypeInvalid);

        RuleFor(x => x.ParentAccountId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.ParentAccountInvalid)
            .When(x => x.ParentAccountId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage(FinancialValidationMessages.InitialBalanceInvalid);
    }
}

/// <summary>
/// Validator for UpdateAccountDto.
/// </summary>
public class UpdateAccountValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Account ID is required");

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage(FinancialValidationMessages.AccountCodeRequired)
            .MaximumLength(50).WithMessage(FinancialValidationMessages.AccountCodeTooLong);

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage(FinancialValidationMessages.AccountNameRequired)
            .MaximumLength(200).WithMessage(FinancialValidationMessages.AccountNameTooLong);

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage(FinancialValidationMessages.AccountTypeInvalid);

        RuleFor(x => x.ParentAccountId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.ParentAccountInvalid)
            .When(x => x.ParentAccountId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

#endregion

#region Invoice Validators

/// <summary>
/// Validator for CreateInvoiceDto.
/// </summary>
public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.PatientRequired);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(FinancialValidationMessages.DueDateInvalid)
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.SubTotalInvalid);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(FinancialValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative")
            .LessThanOrEqualTo(x => x.SubTotal).WithMessage("Discount cannot exceed subtotal")
            .When(x => x.DiscountAmount.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(FinancialValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method")
            .When(x => x.PaymentMethod.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.SaleId)
            .GreaterThan(0).WithMessage("Invalid sale reference")
            .When(x => x.SaleId.HasValue);
    }
}

/// <summary>
/// Validator for UpdateInvoiceDto.
/// </summary>
public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invoice ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid invoice status");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage(FinancialValidationMessages.DiscountPercentageInvalid)
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.TaxPercentage)
            .InclusiveBetween(0, 100).WithMessage(FinancialValidationMessages.TaxPercentageInvalid)
            .When(x => x.TaxPercentage.HasValue);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method")
            .When(x => x.PaymentMethod.HasValue);
    }
}

/// <summary>
/// Validator for RecordPaymentDto.
/// </summary>
public class RecordPaymentValidator : AbstractValidator<RecordPaymentDto>
{
    public RecordPaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("Invoice ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for InvoiceListRequestDto.
/// </summary>
public class InvoiceListRequestValidator : AbstractValidator<InvoiceListRequestDto>
{
    public InvoiceListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(FinancialValidationMessages.InvalidPageSize);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid invoice status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(FinancialValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

#endregion

#region Expense Validators

/// <summary>
/// Validator for CreateExpenseDto.
/// </summary>
public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.ExpenseDate)
            .NotEmpty().WithMessage(FinancialValidationMessages.ExpenseDateRequired)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(FinancialValidationMessages.ExpenseDateFuture);

        RuleFor(x => x.ExpenseCategoryId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.CategoryRequired);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(FinancialValidationMessages.DescriptionRequired)
            .MaximumLength(200).WithMessage(FinancialValidationMessages.DescriptionTooLong);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.AmountInvalid);

        RuleFor(x => x.Vendor)
            .MaximumLength(100).WithMessage("Vendor name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Vendor));

        RuleFor(x => x.InvoiceNumber)
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.InvoiceNumber));

        RuleFor(x => x.InvoiceDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Invoice date cannot be in the future")
            .When(x => x.InvoiceDate.HasValue);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method")
            .When(x => x.PaymentMethod.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));
    }
}

/// <summary>
/// Validator for UpdateExpenseDto.
/// </summary>
public class UpdateExpenseValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Expense ID is required");

        RuleFor(x => x.ExpenseDate)
            .NotEmpty().WithMessage(FinancialValidationMessages.ExpenseDateRequired)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(FinancialValidationMessages.ExpenseDateFuture);

        RuleFor(x => x.ExpenseCategoryId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.CategoryRequired);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(FinancialValidationMessages.DescriptionRequired)
            .MaximumLength(200).WithMessage(FinancialValidationMessages.DescriptionTooLong);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.AmountInvalid);

        RuleFor(x => x.Vendor)
            .MaximumLength(100).WithMessage("Vendor name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Vendor));
    }
}

/// <summary>
/// Validator for ApproveExpenseDto.
/// </summary>
public class ApproveExpenseValidator : AbstractValidator<ApproveExpenseDto>
{
    public ApproveExpenseValidator()
    {
        RuleFor(x => x.ExpenseId)
            .GreaterThan(0).WithMessage("Expense ID is required");

        RuleFor(x => x.Comments)
            .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}

/// <summary>
/// Validator for RejectExpenseDto.
/// </summary>
public class RejectExpenseValidator : AbstractValidator<RejectExpenseDto>
{
    public RejectExpenseValidator()
    {
        RuleFor(x => x.ExpenseId)
            .GreaterThan(0).WithMessage("Expense ID is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage(FinancialValidationMessages.RejectionReasonRequired)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for ExpenseListRequestDto.
/// </summary>
public class ExpenseListRequestValidator : AbstractValidator<ExpenseListRequestDto>
{
    public ExpenseListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(FinancialValidationMessages.InvalidPageSize);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Invalid category ID")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid expense status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(FinancialValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

#endregion

#region Transaction Validators

/// <summary>
/// Validator for CreateTransactionDto.
/// </summary>
public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage(FinancialValidationMessages.TransactionDateRequired);

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.AccountRequired);

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage(FinancialValidationMessages.TransactionTypeInvalid);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.AmountInvalid);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(FinancialValidationMessages.TransactionDescriptionRequired)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.ExpenseId)
            .GreaterThan(0).WithMessage("Invalid expense reference")
            .When(x => x.ExpenseId.HasValue);

        RuleFor(x => x.SaleId)
            .GreaterThan(0).WithMessage("Invalid sale reference")
            .When(x => x.SaleId.HasValue);
    }
}

/// <summary>
/// Validator for TransactionListRequestDto.
/// </summary>
public class TransactionListRequestValidator : AbstractValidator<TransactionListRequestDto>
{
    public TransactionListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(FinancialValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(FinancialValidationMessages.InvalidPageSize);

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Invalid account ID")
            .When(x => x.AccountId.HasValue);

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage(FinancialValidationMessages.TransactionTypeInvalid)
            .When(x => x.TransactionType.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(FinancialValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

#endregion
