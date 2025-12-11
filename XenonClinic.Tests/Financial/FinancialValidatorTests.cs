using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Financial;

public class FinancialValidatorTests
{
    #region CreateAccountValidator Tests

    public class CreateAccountValidatorTests
    {
        private readonly CreateAccountValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Account()
        {
            var dto = CreateValidAccountDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_AccountCode_Is_Empty(string? code)
        {
            var dto = CreateValidAccountDto();
            dto.AccountCode = code!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountCode)
                .WithErrorMessage(FinancialValidationMessages.AccountCodeRequired);
        }

        [Fact]
        public void Should_Fail_When_AccountCode_Exceeds_MaxLength()
        {
            var dto = CreateValidAccountDto();
            dto.AccountCode = new string('A', 51);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountCode)
                .WithErrorMessage(FinancialValidationMessages.AccountCodeTooLong);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_AccountName_Is_Empty(string? name)
        {
            var dto = CreateValidAccountDto();
            dto.AccountName = name!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountName)
                .WithErrorMessage(FinancialValidationMessages.AccountNameRequired);
        }

        [Fact]
        public void Should_Fail_When_AccountName_Exceeds_MaxLength()
        {
            var dto = CreateValidAccountDto();
            dto.AccountName = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountName)
                .WithErrorMessage(FinancialValidationMessages.AccountNameTooLong);
        }

        [Fact]
        public void Should_Fail_When_AccountType_Is_Invalid()
        {
            var dto = CreateValidAccountDto();
            dto.AccountType = (AccountType)999;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountType)
                .WithErrorMessage(FinancialValidationMessages.AccountTypeInvalid);
        }

        [Fact]
        public void Should_Fail_When_ParentAccountId_Is_Invalid()
        {
            var dto = CreateValidAccountDto();
            dto.ParentAccountId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ParentAccountId)
                .WithErrorMessage(FinancialValidationMessages.ParentAccountInvalid);
        }

        [Fact]
        public void Should_Pass_When_ParentAccountId_Is_Null()
        {
            var dto = CreateValidAccountDto();
            dto.ParentAccountId = null;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ParentAccountId);
        }

        [Fact]
        public void Should_Fail_When_InitialBalance_Is_Negative()
        {
            var dto = CreateValidAccountDto();
            dto.InitialBalance = -100;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InitialBalance)
                .WithErrorMessage(FinancialValidationMessages.InitialBalanceInvalid);
        }

        [Fact]
        public void Should_Fail_When_Description_Exceeds_MaxLength()
        {
            var dto = CreateValidAccountDto();
            dto.Description = new string('A', 501);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        private static CreateAccountDto CreateValidAccountDto()
        {
            return new CreateAccountDto
            {
                AccountCode = "ACC001",
                AccountName = "Cash Account",
                AccountType = AccountType.Asset,
                Description = "Main cash account",
                InitialBalance = 0
            };
        }
    }

    #endregion

    #region UpdateAccountValidator Tests

    public class UpdateAccountValidatorTests
    {
        private readonly UpdateAccountValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateAccountDto
            {
                Id = 1,
                AccountCode = "ACC001",
                AccountName = "Updated Account",
                AccountType = AccountType.Asset,
                IsActive = true
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateAccountDto
            {
                Id = 0,
                AccountCode = "ACC001",
                AccountName = "Account",
                AccountType = AccountType.Asset
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    #endregion

    #region CreateInvoiceValidator Tests

    public class CreateInvoiceValidatorTests
    {
        private readonly CreateInvoiceValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Invoice()
        {
            var dto = CreateValidInvoiceDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PatientId_Is_Zero()
        {
            var dto = CreateValidInvoiceDto();
            dto.PatientId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(FinancialValidationMessages.PatientRequired);
        }

        [Fact]
        public void Should_Fail_When_SubTotal_Is_Zero()
        {
            var dto = CreateValidInvoiceDto();
            dto.SubTotal = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SubTotal)
                .WithErrorMessage(FinancialValidationMessages.SubTotalInvalid);
        }

        [Fact]
        public void Should_Fail_When_SubTotal_Is_Negative()
        {
            var dto = CreateValidInvoiceDto();
            dto.SubTotal = -100;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SubTotal)
                .WithErrorMessage(FinancialValidationMessages.SubTotalInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Should_Fail_When_DiscountPercentage_Is_Invalid(decimal discount)
        {
            var dto = CreateValidInvoiceDto();
            dto.DiscountPercentage = discount;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(FinancialValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void Should_Pass_When_DiscountPercentage_Is_Valid(decimal discount)
        {
            var dto = CreateValidInvoiceDto();
            dto.DiscountPercentage = discount;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.DiscountPercentage);
        }

        [Fact]
        public void Should_Fail_When_DiscountAmount_Exceeds_SubTotal()
        {
            var dto = CreateValidInvoiceDto();
            dto.SubTotal = 100;
            dto.DiscountAmount = 150;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DiscountAmount);
        }

        [Fact]
        public void Should_Fail_When_DiscountAmount_Is_Negative()
        {
            var dto = CreateValidInvoiceDto();
            dto.DiscountAmount = -10;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DiscountAmount);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Should_Fail_When_TaxPercentage_Is_Invalid(decimal tax)
        {
            var dto = CreateValidInvoiceDto();
            dto.TaxPercentage = tax;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(FinancialValidationMessages.TaxPercentageInvalid);
        }

        [Fact]
        public void Should_Pass_When_TaxPercentage_Is_UAE_VAT()
        {
            var dto = CreateValidInvoiceDto();
            dto.TaxPercentage = 5; // UAE VAT rate

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TaxPercentage);
        }

        [Fact]
        public void Should_Pass_When_DueDate_Is_In_Future()
        {
            var dto = CreateValidInvoiceDto();
            dto.DueDate = DateTime.UtcNow.Date.AddDays(30);

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
        }

        [Fact]
        public void Should_Fail_When_Description_Exceeds_MaxLength()
        {
            var dto = CreateValidInvoiceDto();
            dto.Description = new string('A', 501);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Fail_When_Notes_Exceeds_MaxLength()
        {
            var dto = CreateValidInvoiceDto();
            dto.Notes = new string('A', 1001);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Should_Fail_When_SaleId_Is_Invalid()
        {
            var dto = CreateValidInvoiceDto();
            dto.SaleId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SaleId);
        }

        private static CreateInvoiceDto CreateValidInvoiceDto()
        {
            return new CreateInvoiceDto
            {
                PatientId = 1,
                SubTotal = 1000,
                TaxPercentage = 5,
                PaymentMethod = PaymentMethod.Cash,
                Description = "Consultation fee"
            };
        }
    }

    #endregion

    #region UpdateInvoiceValidator Tests

    public class UpdateInvoiceValidatorTests
    {
        private readonly UpdateInvoiceValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateInvoiceDto
            {
                Id = 1,
                Status = InvoiceStatus.Issued
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateInvoiceDto
            {
                Id = 0,
                Status = InvoiceStatus.Issued
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Fail_When_Status_Is_Invalid()
        {
            var dto = new UpdateInvoiceDto
            {
                Id = 1,
                Status = (InvoiceStatus)999
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Status);
        }
    }

    #endregion

    #region RecordPaymentValidator Tests

    public class RecordPaymentValidatorTests
    {
        private readonly RecordPaymentValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Payment()
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 1,
                Amount = 500,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = "PAY-001"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InvoiceId_Is_Zero()
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 0,
                Amount = 500,
                PaymentMethod = PaymentMethod.Cash
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_Fail_When_Amount_Is_Invalid(decimal amount)
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 1,
                Amount = amount,
                PaymentMethod = PaymentMethod.Cash
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Fail_When_PaymentMethod_Is_Invalid()
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 1,
                Amount = 500,
                PaymentMethod = (PaymentMethod)999
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
        }

        [Fact]
        public void Should_Fail_When_ReferenceNumber_Exceeds_MaxLength()
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 1,
                Amount = 500,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = new string('A', 101)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
        }

        [Fact]
        public void Should_Fail_When_Notes_Exceeds_MaxLength()
        {
            var dto = new RecordPaymentDto
            {
                InvoiceId = 1,
                Amount = 500,
                PaymentMethod = PaymentMethod.Cash,
                Notes = new string('A', 501)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region InvoiceListRequestValidator Tests

    public class InvoiceListRequestValidatorTests
    {
        private readonly InvoiceListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PageNumber_Is_Zero()
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_PatientId_Is_Invalid()
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PatientId);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(FinancialValidationMessages.DateRangeInvalid);
        }

        [Fact]
        public void Should_Pass_When_DateRange_Is_Valid()
        {
            var dto = new InvoiceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date.AddDays(-30),
                DateTo = DateTime.UtcNow.Date
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.DateTo);
        }
    }

    #endregion

    #region CreateExpenseValidator Tests

    public class CreateExpenseValidatorTests
    {
        private readonly CreateExpenseValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Expense()
        {
            var dto = CreateValidExpenseDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_ExpenseDate_Is_In_Future()
        {
            var dto = CreateValidExpenseDto();
            dto.ExpenseDate = DateTime.UtcNow.Date.AddDays(1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpenseDate)
                .WithErrorMessage(FinancialValidationMessages.ExpenseDateFuture);
        }

        [Fact]
        public void Should_Fail_When_ExpenseCategoryId_Is_Zero()
        {
            var dto = CreateValidExpenseDto();
            dto.ExpenseCategoryId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpenseCategoryId)
                .WithErrorMessage(FinancialValidationMessages.CategoryRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Description_Is_Empty(string? description)
        {
            var dto = CreateValidExpenseDto();
            dto.Description = description!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage(FinancialValidationMessages.DescriptionRequired);
        }

        [Fact]
        public void Should_Fail_When_Description_Exceeds_MaxLength()
        {
            var dto = CreateValidExpenseDto();
            dto.Description = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage(FinancialValidationMessages.DescriptionTooLong);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_Fail_When_Amount_Is_Invalid(decimal amount)
        {
            var dto = CreateValidExpenseDto();
            dto.Amount = amount;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage(FinancialValidationMessages.AmountInvalid);
        }

        [Fact]
        public void Should_Fail_When_Vendor_Exceeds_MaxLength()
        {
            var dto = CreateValidExpenseDto();
            dto.Vendor = new string('A', 101);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Vendor);
        }

        [Fact]
        public void Should_Fail_When_InvoiceNumber_Exceeds_MaxLength()
        {
            var dto = CreateValidExpenseDto();
            dto.InvoiceNumber = new string('A', 51);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber);
        }

        [Fact]
        public void Should_Fail_When_InvoiceDate_Is_In_Future()
        {
            var dto = CreateValidExpenseDto();
            dto.InvoiceDate = DateTime.UtcNow.Date.AddDays(1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceDate);
        }

        [Fact]
        public void Should_Fail_When_ReferenceNumber_Exceeds_MaxLength()
        {
            var dto = CreateValidExpenseDto();
            dto.ReferenceNumber = new string('A', 101);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
        }

        private static CreateExpenseDto CreateValidExpenseDto()
        {
            return new CreateExpenseDto
            {
                ExpenseDate = DateTime.UtcNow.Date,
                ExpenseCategoryId = 1,
                Description = "Office supplies purchase",
                Amount = 500,
                Vendor = "Office Mart",
                PaymentMethod = PaymentMethod.Cash
            };
        }
    }

    #endregion

    #region UpdateExpenseValidator Tests

    public class UpdateExpenseValidatorTests
    {
        private readonly UpdateExpenseValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateExpenseDto
            {
                Id = 1,
                ExpenseDate = DateTime.UtcNow.Date,
                ExpenseCategoryId = 1,
                Description = "Updated expense",
                Amount = 600
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateExpenseDto
            {
                Id = 0,
                ExpenseDate = DateTime.UtcNow.Date,
                ExpenseCategoryId = 1,
                Description = "Expense",
                Amount = 500
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    #endregion

    #region ApproveExpenseValidator Tests

    public class ApproveExpenseValidatorTests
    {
        private readonly ApproveExpenseValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Approval()
        {
            var dto = new ApproveExpenseDto
            {
                ExpenseId = 1,
                Comments = "Approved by manager"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_ExpenseId_Is_Zero()
        {
            var dto = new ApproveExpenseDto
            {
                ExpenseId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
        }

        [Fact]
        public void Should_Fail_When_Comments_Exceeds_MaxLength()
        {
            var dto = new ApproveExpenseDto
            {
                ExpenseId = 1,
                Comments = new string('A', 501)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Comments);
        }
    }

    #endregion

    #region RejectExpenseValidator Tests

    public class RejectExpenseValidatorTests
    {
        private readonly RejectExpenseValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Rejection()
        {
            var dto = new RejectExpenseDto
            {
                ExpenseId = 1,
                RejectionReason = "Budget exceeded"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_ExpenseId_Is_Zero()
        {
            var dto = new RejectExpenseDto
            {
                ExpenseId = 0,
                RejectionReason = "Not approved"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_RejectionReason_Is_Empty(string? reason)
        {
            var dto = new RejectExpenseDto
            {
                ExpenseId = 1,
                RejectionReason = reason!
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
                .WithErrorMessage(FinancialValidationMessages.RejectionReasonRequired);
        }

        [Fact]
        public void Should_Fail_When_RejectionReason_Exceeds_MaxLength()
        {
            var dto = new RejectExpenseDto
            {
                ExpenseId = 1,
                RejectionReason = new string('A', 501)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.RejectionReason);
        }
    }

    #endregion

    #region ExpenseListRequestValidator Tests

    public class ExpenseListRequestValidatorTests
    {
        private readonly ExpenseListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new ExpenseListRequestDto
            {
                PageNumber = 1,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PageNumber_Is_Zero()
        {
            var dto = new ExpenseListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new ExpenseListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_CategoryId_Is_Invalid()
        {
            var dto = new ExpenseListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                CategoryId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new ExpenseListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(FinancialValidationMessages.DateRangeInvalid);
        }
    }

    #endregion

    #region CreateTransactionValidator Tests

    public class CreateTransactionValidatorTests
    {
        private readonly CreateTransactionValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Transaction()
        {
            var dto = CreateValidTransactionDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_AccountId_Is_Zero()
        {
            var dto = CreateValidTransactionDto();
            dto.AccountId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountId)
                .WithErrorMessage(FinancialValidationMessages.AccountRequired);
        }

        [Fact]
        public void Should_Fail_When_TransactionType_Is_Invalid()
        {
            var dto = CreateValidTransactionDto();
            dto.TransactionType = (TransactionType)999;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TransactionType)
                .WithErrorMessage(FinancialValidationMessages.TransactionTypeInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_Fail_When_Amount_Is_Invalid(decimal amount)
        {
            var dto = CreateValidTransactionDto();
            dto.Amount = amount;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage(FinancialValidationMessages.AmountInvalid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Description_Is_Empty(string? description)
        {
            var dto = CreateValidTransactionDto();
            dto.Description = description!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage(FinancialValidationMessages.TransactionDescriptionRequired);
        }

        [Fact]
        public void Should_Fail_When_Description_Exceeds_MaxLength()
        {
            var dto = CreateValidTransactionDto();
            dto.Description = new string('A', 501);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Fail_When_ReferenceNumber_Exceeds_MaxLength()
        {
            var dto = CreateValidTransactionDto();
            dto.ReferenceNumber = new string('A', 101);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
        }

        [Fact]
        public void Should_Fail_When_ExpenseId_Is_Invalid()
        {
            var dto = CreateValidTransactionDto();
            dto.ExpenseId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
        }

        [Fact]
        public void Should_Fail_When_SaleId_Is_Invalid()
        {
            var dto = CreateValidTransactionDto();
            dto.SaleId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SaleId);
        }

        private static CreateTransactionDto CreateValidTransactionDto()
        {
            return new CreateTransactionDto
            {
                TransactionDate = DateTime.UtcNow,
                AccountId = 1,
                TransactionType = TransactionType.Credit,
                Amount = 1000,
                Description = "Payment received"
            };
        }
    }

    #endregion

    #region TransactionListRequestValidator Tests

    public class TransactionListRequestValidatorTests
    {
        private readonly TransactionListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PageNumber_Is_Zero()
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(FinancialValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_AccountId_Is_Invalid()
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                AccountId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AccountId);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(FinancialValidationMessages.DateRangeInvalid);
        }

        [Fact]
        public void Should_Pass_When_DateRange_Is_Valid()
        {
            var dto = new TransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date.AddMonths(-1),
                DateTo = DateTime.UtcNow.Date
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.DateTo);
        }
    }

    #endregion
}
