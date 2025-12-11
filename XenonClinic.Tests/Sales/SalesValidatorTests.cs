using FluentValidation.TestHelper;
using Xunit;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;

namespace XenonClinic.Tests.Sales;

/// <summary>
/// Comprehensive unit tests for Sales module validators.
/// Tests cover sales, payments, and quotations validation.
/// </summary>
public class SalesValidatorTests
{
    #region CreateSaleValidator Tests

    public class CreateSaleValidatorTests
    {
        private readonly CreateSaleValidator _validator = new();

        [Fact]
        public void Validate_ValidSale_ShouldPass()
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                DiscountPercentage = 10,
                TaxPercentage = 5,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Medical Equipment", Quantity = 1, UnitPrice = 500 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = patientId,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(SalesValidationMessages.PatientRequired);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                DiscountPercentage = discount,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void Validate_ValidDiscountPercentage_ShouldPass(decimal discount)
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                DiscountPercentage = discount,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.DiscountPercentage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                TaxPercentage = tax,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                Notes = new string('x', 1001),
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Validate_TermsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                Terms = new string('x', 2001),
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Terms);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidQuotationId_ShouldFail(int quotationId)
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                QuotationId = quotationId,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QuotationId);
        }

        [Fact]
        public void Validate_EmptyItems_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                Items = new List<CreateSaleItemDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage(SalesValidationMessages.SaleItemsRequired);
        }

        [Fact]
        public void Validate_TooManyItems_ShouldFail()
        {
            // Arrange
            var items = Enumerable.Range(1, 101)
                .Select(i => new CreateSaleItemDto
                {
                    ItemName = $"Item {i}",
                    Quantity = 1,
                    UnitPrice = 100
                }).ToList();

            var dto = new CreateSaleDto
            {
                PatientId = 1,
                Items = items
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage("Sale cannot have more than 100 items");
        }

        [Fact]
        public void Validate_InvalidItemInItems_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleDto
            {
                PatientId = 1,
                Items = new List<CreateSaleItemDto>
                {
                    new() { ItemName = "", Quantity = 0, UnitPrice = 0 } // Invalid item
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor("Items[0].ItemName");
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
            result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
        }
    }

    #endregion

    #region CreateSaleItemValidator Tests

    public class CreateSaleItemValidatorTests
    {
        private readonly CreateSaleItemValidator _validator = new();

        [Fact]
        public void Validate_ValidItem_ShouldPass()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Medical Device",
                ItemDescription = "High-quality medical device",
                ItemCode = "MD-001",
                Quantity = 5,
                UnitPrice = 150.00m,
                DiscountPercentage = 10,
                TaxPercentage = 5,
                InventoryItemId = 1,
                SerialNumber = "SN123456",
                WarrantyMonths = 12,
                Notes = "Handle with care"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyItemName_ShouldFail(string? itemName)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = itemName!,
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName)
                .WithErrorMessage(SalesValidationMessages.ItemNameRequired);
        }

        [Fact]
        public void Validate_ItemNameTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = new string('x', 201),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName)
                .WithErrorMessage(SalesValidationMessages.ItemNameTooLong);
        }

        [Fact]
        public void Validate_ItemDescriptionTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                ItemDescription = new string('x', 501),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemDescription);
        }

        [Fact]
        public void Validate_ItemCodeTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                ItemCode = new string('x', 51),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_InvalidQuantity_ShouldFail(int quantity)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = quantity,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(SalesValidationMessages.QuantityInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.50)]
        public void Validate_InvalidUnitPrice_ShouldFail(decimal price)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = price
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
                .WithErrorMessage(SalesValidationMessages.UnitPriceInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                DiscountPercentage = discount
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                TaxPercentage = tax
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidInventoryItemId_ShouldFail(int inventoryItemId)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                InventoryItemId = inventoryItemId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(SalesValidationMessages.InventoryItemInvalid);
        }

        [Fact]
        public void Validate_SerialNumberTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                SerialNumber = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SerialNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(121)]
        [InlineData(200)]
        public void Validate_InvalidWarrantyMonths_ShouldFail(int months)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                WarrantyMonths = months
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.WarrantyMonths)
                .WithErrorMessage(SalesValidationMessages.WarrantyMonthsInvalid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        [InlineData(60)]
        [InlineData(120)]
        public void Validate_ValidWarrantyMonths_ShouldPass(int months)
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                WarrantyMonths = months
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.WarrantyMonths);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateSaleItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region UpdateSaleValidator Tests

    public class UpdateSaleValidatorTests
    {
        private readonly UpdateSaleValidator _validator = new();

        [Fact]
        public void Validate_ValidUpdate_ShouldPass()
        {
            // Arrange
            var dto = new UpdateSaleDto
            {
                Id = 1,
                DiscountPercentage = 15,
                TaxPercentage = 5,
                Notes = "Updated notes",
                Terms = "Updated terms"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            // Arrange
            var dto = new UpdateSaleDto { Id = id };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(SalesValidationMessages.SaleIdRequired);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new UpdateSaleDto
            {
                Id = 1,
                DiscountPercentage = discount
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new UpdateSaleDto
            {
                Id = 1,
                TaxPercentage = tax
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new UpdateSaleDto
            {
                Id = 1,
                Notes = new string('x', 1001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Validate_TermsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new UpdateSaleDto
            {
                Id = 1,
                Terms = new string('x', 2001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Terms);
        }
    }

    #endregion

    #region SaleListRequestValidator Tests

    public class SaleListRequestValidatorTests
    {
        private readonly SaleListRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldPass()
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = 1,
                Status = SaleStatus.Confirmed,
                PaymentStatus = PaymentStatus.PartiallyPaid,
                DateFrom = DateTime.UtcNow.AddDays(-30),
                DateTo = DateTime.UtcNow,
                SearchTerm = "Invoice"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPageNumber_ShouldFail(int pageNumber)
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = pageNumber,
                PageSize = 20
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(SalesValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidPageSize_ShouldFail(int pageSize)
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(SalesValidationMessages.InvalidPageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = patientId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(SalesValidationMessages.PatientInvalid);
        }

        [Fact]
        public void Validate_InvalidStatus_ShouldFail()
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                Status = (SaleStatus)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(SalesValidationMessages.SaleStatusInvalid);
        }

        [Fact]
        public void Validate_InvalidPaymentStatus_ShouldFail()
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PaymentStatus = (PaymentStatus)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PaymentStatus);
        }

        [Fact]
        public void Validate_InvalidDateRange_ShouldFail()
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(-10) // DateTo before DateFrom
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(SalesValidationMessages.DateRangeInvalid);
        }

        [Fact]
        public void Validate_SearchTermTooLong_ShouldFail()
        {
            // Arrange
            var dto = new SaleListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        }
    }

    #endregion

    #region RecordSalePaymentValidator Tests

    public class RecordSalePaymentValidatorTests
    {
        private readonly RecordSalePaymentValidator _validator = new();

        [Fact]
        public void Validate_ValidPayment_ShouldPass()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 500.00m,
                PaymentMethod = PaymentMethod.CreditCard,
                ReferenceNumber = "REF-123456",
                BankName = "Emirates NBD",
                CardLastFourDigits = "1234",
                Notes = "Payment received"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ValidInsurancePayment_ShouldPass()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 1000.00m,
                PaymentMethod = PaymentMethod.Insurance,
                InsuranceCompany = "Daman Insurance",
                InsuranceClaimNumber = "CLM-2024-001",
                InsurancePolicyNumber = "POL-123456"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ValidInstallmentPayment_ShouldPass()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 200.00m,
                PaymentMethod = PaymentMethod.Cash,
                InstallmentNumber = 1,
                TotalInstallments = 5
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidSaleId_ShouldFail(int saleId)
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = saleId,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SaleId)
                .WithErrorMessage(SalesValidationMessages.SaleIdRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.50)]
        public void Validate_InvalidAmount_ShouldFail(decimal amount)
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = amount,
                PaymentMethod = PaymentMethod.Cash
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage(SalesValidationMessages.PaymentAmountInvalid);
        }

        [Fact]
        public void Validate_InvalidPaymentMethod_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = (PaymentMethod)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PaymentMethod)
                .WithErrorMessage(SalesValidationMessages.PaymentMethodInvalid);
        }

        [Fact]
        public void Validate_ReferenceNumberTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
        }

        [Fact]
        public void Validate_BankNameTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.BankTransfer,
                BankName = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BankName);
        }

        [Theory]
        [InlineData("123")]    // Too short
        [InlineData("12345")]  // Too long
        [InlineData("abcd")]   // Not numeric
        [InlineData("12ab")]   // Mixed
        public void Validate_InvalidCardLastFourDigits_ShouldFail(string cardDigits)
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.CreditCard,
                CardLastFourDigits = cardDigits
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CardLastFourDigits);
        }

        [Fact]
        public void Validate_ValidCardLastFourDigits_ShouldPass()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.CreditCard,
                CardLastFourDigits = "1234"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CardLastFourDigits);
        }

        [Fact]
        public void Validate_InsuranceCompanyTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Insurance,
                InsuranceCompany = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InsuranceCompany);
        }

        [Fact]
        public void Validate_InsuranceClaimNumberTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Insurance,
                InsuranceClaimNumber = new string('x', 51)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InsuranceClaimNumber);
        }

        [Fact]
        public void Validate_InsurancePolicyNumberTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Insurance,
                InsurancePolicyNumber = new string('x', 51)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InsurancePolicyNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidInstallmentNumber_ShouldFail(int installmentNumber)
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash,
                InstallmentNumber = installmentNumber
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InstallmentNumber)
                .WithErrorMessage(SalesValidationMessages.InstallmentNumberInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidTotalInstallments_ShouldFail(int totalInstallments)
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash,
                TotalInstallments = totalInstallments
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TotalInstallments)
                .WithErrorMessage(SalesValidationMessages.TotalInstallmentsInvalid);
        }

        [Fact]
        public void Validate_InstallmentNumberExceedsTotal_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash,
                InstallmentNumber = 6,
                TotalInstallments = 5
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InstallmentNumber)
                .WithErrorMessage(SalesValidationMessages.InstallmentNumberExceedsTotal);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RecordSalePaymentDto
            {
                SaleId = 1,
                Amount = 100,
                PaymentMethod = PaymentMethod.Cash,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region CreateQuotationValidator Tests

    public class CreateQuotationValidatorTests
    {
        private readonly CreateQuotationValidator _validator = new();

        [Fact]
        public void Validate_ValidQuotation_ShouldPass()
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                DiscountPercentage = 10,
                TaxPercentage = 5,
                Notes = "Quotation notes",
                Terms = "Payment terms",
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Service Package", Quantity = 1, UnitPrice = 1000 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = patientId,
                ValidityDays = 30,
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(SalesValidationMessages.PatientRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(366)]
        [InlineData(1000)]
        public void Validate_InvalidValidityDays_ShouldFail(int days)
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = days,
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ValidityDays)
                .WithErrorMessage(SalesValidationMessages.ValidityDaysInvalid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(90)]
        [InlineData(365)]
        public void Validate_ValidValidityDays_ShouldPass(int days)
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = days,
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ValidityDays);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                DiscountPercentage = discount,
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                TaxPercentage = tax,
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                Notes = new string('x', 1001),
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Validate_TermsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                Terms = new string('x', 2001),
                Items = new List<CreateQuotationItemDto>
                {
                    new() { ItemName = "Item", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Terms);
        }

        [Fact]
        public void Validate_EmptyItems_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                Items = new List<CreateQuotationItemDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage(SalesValidationMessages.SaleItemsRequired);
        }

        [Fact]
        public void Validate_TooManyItems_ShouldFail()
        {
            // Arrange
            var items = Enumerable.Range(1, 101)
                .Select(i => new CreateQuotationItemDto
                {
                    ItemName = $"Item {i}",
                    Quantity = 1,
                    UnitPrice = 100
                }).ToList();

            var dto = new CreateQuotationDto
            {
                PatientId = 1,
                ValidityDays = 30,
                Items = items
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage("Quotation cannot have more than 100 items");
        }
    }

    #endregion

    #region CreateQuotationItemValidator Tests

    public class CreateQuotationItemValidatorTests
    {
        private readonly CreateQuotationItemValidator _validator = new();

        [Fact]
        public void Validate_ValidItem_ShouldPass()
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Treatment Package",
                ItemDescription = "Comprehensive treatment package",
                ItemCode = "TP-001",
                Quantity = 1,
                UnitPrice = 5000.00m,
                DiscountPercentage = 10,
                TaxPercentage = 5,
                InventoryItemId = 1,
                Notes = "Special package"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyItemName_ShouldFail(string? itemName)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = itemName!,
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName)
                .WithErrorMessage(SalesValidationMessages.ItemNameRequired);
        }

        [Fact]
        public void Validate_ItemNameTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = new string('x', 201),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName)
                .WithErrorMessage(SalesValidationMessages.ItemNameTooLong);
        }

        [Fact]
        public void Validate_ItemDescriptionTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                ItemDescription = new string('x', 501),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemDescription);
        }

        [Fact]
        public void Validate_ItemCodeTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                ItemCode = new string('x', 51),
                Quantity = 1,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidQuantity_ShouldFail(int quantity)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = quantity,
                UnitPrice = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(SalesValidationMessages.QuantityInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidUnitPrice_ShouldFail(decimal price)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = price
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
                .WithErrorMessage(SalesValidationMessages.UnitPriceInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                DiscountPercentage = discount
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                TaxPercentage = tax
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidInventoryItemId_ShouldFail(int inventoryItemId)
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                InventoryItemId = inventoryItemId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(SalesValidationMessages.InventoryItemInvalid);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateQuotationItemDto
            {
                ItemName = "Item",
                Quantity = 1,
                UnitPrice = 100,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region UpdateQuotationValidator Tests

    public class UpdateQuotationValidatorTests
    {
        private readonly UpdateQuotationValidator _validator = new();

        [Fact]
        public void Validate_ValidUpdate_ShouldPass()
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = 45,
                DiscountPercentage = 15,
                TaxPercentage = 5,
                Notes = "Updated notes",
                Terms = "Updated terms"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = id,
                ValidityDays = 30
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(SalesValidationMessages.QuotationIdRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(366)]
        public void Validate_InvalidValidityDays_ShouldFail(int days)
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = days
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ValidityDays)
                .WithErrorMessage(SalesValidationMessages.ValidityDaysInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = 30,
                DiscountPercentage = discount
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage)
                .WithErrorMessage(SalesValidationMessages.DiscountPercentageInvalid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidTaxPercentage_ShouldFail(decimal tax)
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = 30,
                TaxPercentage = tax
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxPercentage)
                .WithErrorMessage(SalesValidationMessages.TaxPercentageInvalid);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = 30,
                Notes = new string('x', 1001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Validate_TermsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new UpdateQuotationDto
            {
                Id = 1,
                ValidityDays = 30,
                Terms = new string('x', 2001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Terms);
        }
    }

    #endregion

    #region SendQuotationValidator Tests

    public class SendQuotationValidatorTests
    {
        private readonly SendQuotationValidator _validator = new();

        [Fact]
        public void Validate_ValidSend_ShouldPass()
        {
            // Arrange
            var dto = new SendQuotationDto
            {
                QuotationId = 1,
                EmailAddress = "patient@example.com",
                Message = "Please review the attached quotation"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_MinimalSend_ShouldPass()
        {
            // Arrange
            var dto = new SendQuotationDto
            {
                QuotationId = 1
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidQuotationId_ShouldFail(int quotationId)
        {
            // Arrange
            var dto = new SendQuotationDto
            {
                QuotationId = quotationId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QuotationId)
                .WithErrorMessage(SalesValidationMessages.QuotationIdRequired);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@invalid.com")]
        [InlineData("invalid@")]
        [InlineData("invalid")]
        public void Validate_InvalidEmailAddress_ShouldFail(string email)
        {
            // Arrange
            var dto = new SendQuotationDto
            {
                QuotationId = 1,
                EmailAddress = email
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
        }

        [Fact]
        public void Validate_MessageTooLong_ShouldFail()
        {
            // Arrange
            var dto = new SendQuotationDto
            {
                QuotationId = 1,
                Message = new string('x', 1001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message);
        }
    }

    #endregion

    #region AcceptQuotationValidator Tests

    public class AcceptQuotationValidatorTests
    {
        private readonly AcceptQuotationValidator _validator = new();

        [Fact]
        public void Validate_ValidAccept_ShouldPass()
        {
            // Arrange
            var dto = new AcceptQuotationDto
            {
                QuotationId = 1,
                Notes = "Accepted by patient"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_MinimalAccept_ShouldPass()
        {
            // Arrange
            var dto = new AcceptQuotationDto
            {
                QuotationId = 1
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidQuotationId_ShouldFail(int quotationId)
        {
            // Arrange
            var dto = new AcceptQuotationDto
            {
                QuotationId = quotationId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QuotationId)
                .WithErrorMessage(SalesValidationMessages.QuotationIdRequired);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new AcceptQuotationDto
            {
                QuotationId = 1,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region RejectQuotationValidator Tests

    public class RejectQuotationValidatorTests
    {
        private readonly RejectQuotationValidator _validator = new();

        [Fact]
        public void Validate_ValidReject_ShouldPass()
        {
            // Arrange
            var dto = new RejectQuotationDto
            {
                QuotationId = 1,
                RejectionReason = "Price too high"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidQuotationId_ShouldFail(int quotationId)
        {
            // Arrange
            var dto = new RejectQuotationDto
            {
                QuotationId = quotationId,
                RejectionReason = "Not interested"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QuotationId)
                .WithErrorMessage(SalesValidationMessages.QuotationIdRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyRejectionReason_ShouldFail(string? reason)
        {
            // Arrange
            var dto = new RejectQuotationDto
            {
                QuotationId = 1,
                RejectionReason = reason!
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
                .WithErrorMessage(SalesValidationMessages.RejectionReasonRequired);
        }

        [Fact]
        public void Validate_RejectionReasonTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RejectQuotationDto
            {
                QuotationId = 1,
                RejectionReason = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RejectionReason);
        }
    }

    #endregion

    #region QuotationListRequestValidator Tests

    public class QuotationListRequestValidatorTests
    {
        private readonly QuotationListRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldPass()
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = 1,
                Status = QuotationStatus.Sent,
                DateFrom = DateTime.UtcNow.AddDays(-30),
                DateTo = DateTime.UtcNow,
                SearchTerm = "Package"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPageNumber_ShouldFail(int pageNumber)
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = pageNumber,
                PageSize = 20
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(SalesValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidPageSize_ShouldFail(int pageSize)
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(SalesValidationMessages.InvalidPageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = patientId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(SalesValidationMessages.PatientInvalid);
        }

        [Fact]
        public void Validate_InvalidStatus_ShouldFail()
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                Status = (QuotationStatus)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(SalesValidationMessages.QuotationStatusInvalid);
        }

        [Fact]
        public void Validate_InvalidDateRange_ShouldFail()
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(-10)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(SalesValidationMessages.DateRangeInvalid);
        }

        [Fact]
        public void Validate_SearchTermTooLong_ShouldFail()
        {
            // Arrange
            var dto = new QuotationListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        }
    }

    #endregion
}
