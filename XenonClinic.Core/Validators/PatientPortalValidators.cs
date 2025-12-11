using FluentValidation;
using System.Text.RegularExpressions;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Validators;

/// <summary>
/// BUG FIX: Added missing validators for Patient Portal DTOs to prevent
/// security vulnerabilities from unvalidated authentication and payment inputs.
/// </summary>
public static class PatientPortalValidationMessages
{
    public const string EmailRequired = "Email address is required";
    public const string EmailInvalid = "Invalid email address format";
    public const string PasswordRequired = "Password is required";
    public const string PasswordTooShort = "Password must be at least 8 characters";
    public const string PasswordTooLong = "Password cannot exceed 128 characters";
    public const string PasswordComplexity = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character";
    public const string DateOfBirthRequired = "Date of birth is required";
    public const string DateOfBirthFuture = "Date of birth cannot be in the future";
    public const string DateOfBirthTooOld = "Invalid date of birth";
    public const string PhoneInvalid = "Invalid phone number format";
    public const string MrnInvalid = "MRN must be alphanumeric and 3-50 characters";
    public const string InvoiceIdRequired = "Invoice ID is required";
    public const string AmountInvalid = "Amount must be greater than zero";
    public const string AmountTooLarge = "Amount cannot exceed 999,999.99";
    public const string PaymentMethodRequired = "Payment method is required";
    public const string PaymentMethodInvalid = "Payment method must be 'Card' or 'BankTransfer'";
    public const string CardTokenRequired = "Card token is required for card payments";
    public const string CardTokenInvalid = "Invalid card token format";
}

/// <summary>
/// Validator for PortalRegistrationDto
/// </summary>
public class PortalRegistrationDtoValidator : AbstractValidator<PortalRegistrationDto>
{
    // Password complexity: at least one uppercase, lowercase, digit, and special character
    private static readonly Regex PasswordComplexityPattern = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_#^()+=\[\]{}|\\:;""'<>,./~`])",
        RegexOptions.Compiled);

    // Phone pattern: international format with optional country code
    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-()]{7,20}$",
        RegexOptions.Compiled);

    // MRN pattern: alphanumeric with optional hyphens
    private static readonly Regex MrnPattern = new(
        @"^[A-Za-z0-9\-]{3,50}$",
        RegexOptions.Compiled);

    public PortalRegistrationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(PatientPortalValidationMessages.EmailInvalid)
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.PasswordRequired)
            .MinimumLength(8).WithMessage(PatientPortalValidationMessages.PasswordTooShort)
            .MaximumLength(128).WithMessage(PatientPortalValidationMessages.PasswordTooLong)
            .Must(BeComplexPassword).WithMessage(PatientPortalValidationMessages.PasswordComplexity);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.DateOfBirthRequired)
            .LessThanOrEqualTo(DateTime.Today).WithMessage(PatientPortalValidationMessages.DateOfBirthFuture)
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage(PatientPortalValidationMessages.DateOfBirthTooOld);

        RuleFor(x => x.MRN)
            .Must(BeValidMrn).WithMessage(PatientPortalValidationMessages.MrnInvalid)
            .When(x => !string.IsNullOrEmpty(x.MRN));

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhone).WithMessage(PatientPortalValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private static bool BeComplexPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return PasswordComplexityPattern.IsMatch(password);
    }

    private static bool BeValidMrn(string? mrn)
    {
        if (string.IsNullOrEmpty(mrn))
            return true; // Empty handled separately

        return MrnPattern.IsMatch(mrn);
    }

    private static bool BeValidPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return true;

        return PhonePattern.IsMatch(phone);
    }
}

/// <summary>
/// Validator for PortalLoginDto
/// </summary>
public class PortalLoginDtoValidator : AbstractValidator<PortalLoginDto>
{
    public PortalLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(PatientPortalValidationMessages.EmailInvalid)
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.PasswordRequired)
            .MaximumLength(128).WithMessage(PatientPortalValidationMessages.PasswordTooLong);
        // Note: We don't enforce password complexity on login - just check it exists
        // Password complexity is enforced only on registration
    }
}

/// <summary>
/// Validator for PortalMakePaymentDto
/// </summary>
public class PortalMakePaymentDtoValidator : AbstractValidator<PortalMakePaymentDto>
{
    // Valid payment methods
    private static readonly string[] ValidPaymentMethods = { "Card", "BankTransfer" };

    // Card token pattern (assumes tokenized format - adjust based on payment provider)
    private static readonly Regex CardTokenPattern = new(
        @"^[A-Za-z0-9\-_]{10,2048}$",
        RegexOptions.Compiled);

    public PortalMakePaymentDtoValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage(PatientPortalValidationMessages.InvoiceIdRequired);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(PatientPortalValidationMessages.AmountInvalid)
            .LessThanOrEqualTo(999999.99m).WithMessage(PatientPortalValidationMessages.AmountTooLarge);

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.PaymentMethodRequired)
            .Must(BeValidPaymentMethod).WithMessage(PatientPortalValidationMessages.PaymentMethodInvalid);

        // Card token is required for card payments
        RuleFor(x => x.CardToken)
            .NotEmpty().WithMessage(PatientPortalValidationMessages.CardTokenRequired)
            .Must(BeValidCardToken).WithMessage(PatientPortalValidationMessages.CardTokenInvalid)
            .When(x => x.PaymentMethod == "Card");
    }

    private static bool BeValidPaymentMethod(string? method)
    {
        if (string.IsNullOrEmpty(method))
            return false;

        return ValidPaymentMethods.Contains(method, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidCardToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        return CardTokenPattern.IsMatch(token);
    }
}
