using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Inventory;

public class InventoryValidatorTests
{
    #region CreateInventoryItemValidator Tests

    public class CreateInventoryItemValidatorTests
    {
        private readonly CreateInventoryItemValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_InventoryItem()
        {
            var dto = CreateValidInventoryItemDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_ItemCode_Is_Empty(string? code)
        {
            var dto = CreateValidInventoryItemDto();
            dto.ItemCode = code!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ItemCode)
                .WithErrorMessage(InventoryValidationMessages.ItemCodeRequired);
        }

        [Fact]
        public void Should_Fail_When_ItemCode_Exceeds_MaxLength()
        {
            var dto = CreateValidInventoryItemDto();
            dto.ItemCode = new string('A', 51);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ItemCode)
                .WithErrorMessage(InventoryValidationMessages.ItemCodeTooLong);
        }

        [Theory]
        [InlineData("ITEM@123")]
        [InlineData("ITEM 123")]
        [InlineData("ITEM#123")]
        public void Should_Fail_When_ItemCode_Format_Is_Invalid(string code)
        {
            var dto = CreateValidInventoryItemDto();
            dto.ItemCode = code;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ItemCode)
                .WithErrorMessage(InventoryValidationMessages.ItemCodeInvalid);
        }

        [Theory]
        [InlineData("ITEM123")]
        [InlineData("ITEM-123")]
        [InlineData("ITEM_123")]
        [InlineData("Item-Code_001")]
        public void Should_Pass_When_ItemCode_Format_Is_Valid(string code)
        {
            var dto = CreateValidInventoryItemDto();
            dto.ItemCode = code;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ItemCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Name_Is_Empty(string? name)
        {
            var dto = CreateValidInventoryItemDto();
            dto.Name = name!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(InventoryValidationMessages.ItemNameRequired);
        }

        [Fact]
        public void Should_Fail_When_Name_Exceeds_MaxLength()
        {
            var dto = CreateValidInventoryItemDto();
            dto.Name = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(InventoryValidationMessages.ItemNameTooLong);
        }

        [Fact]
        public void Should_Fail_When_Category_Is_Invalid()
        {
            var dto = CreateValidInventoryItemDto();
            dto.Category = (InventoryCategory)999;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Category)
                .WithErrorMessage(InventoryValidationMessages.CategoryInvalid);
        }

        [Fact]
        public void Should_Fail_When_InitialQuantity_Is_Negative()
        {
            var dto = CreateValidInventoryItemDto();
            dto.InitialQuantity = -1;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InitialQuantity)
                .WithErrorMessage(InventoryValidationMessages.NewQuantityInvalid);
        }

        [Fact]
        public void Should_Fail_When_ReorderLevel_Is_Negative()
        {
            var dto = CreateValidInventoryItemDto();
            dto.ReorderLevel = -1;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ReorderLevel)
                .WithErrorMessage(InventoryValidationMessages.ReorderLevelInvalid);
        }

        [Fact]
        public void Should_Fail_When_MaxStockLevel_Less_Than_ReorderLevel()
        {
            var dto = CreateValidInventoryItemDto();
            dto.ReorderLevel = 20;
            dto.MaxStockLevel = 10;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.MaxStockLevel)
                .WithErrorMessage(InventoryValidationMessages.MaxStockLevelInvalid);
        }

        [Fact]
        public void Should_Fail_When_CostPrice_Is_Negative()
        {
            var dto = CreateValidInventoryItemDto();
            dto.CostPrice = -10;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CostPrice)
                .WithErrorMessage(InventoryValidationMessages.CostPriceInvalid);
        }

        [Fact]
        public void Should_Fail_When_SellingPrice_Is_Negative()
        {
            var dto = CreateValidInventoryItemDto();
            dto.SellingPrice = -10;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SellingPrice)
                .WithErrorMessage(InventoryValidationMessages.SellingPriceInvalid);
        }

        [Fact]
        public void Should_Fail_When_SupplierId_Is_Invalid()
        {
            var dto = CreateValidInventoryItemDto();
            dto.SupplierId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SupplierId)
                .WithErrorMessage(InventoryValidationMessages.SupplierInvalid);
        }

        [Fact]
        public void Should_Pass_When_SupplierId_Is_Null()
        {
            var dto = CreateValidInventoryItemDto();
            dto.SupplierId = null;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SupplierId);
        }

        [Fact]
        public void Should_Fail_When_Barcode_Exceeds_MaxLength()
        {
            var dto = CreateValidInventoryItemDto();
            dto.Barcode = new string('1', 101);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Barcode)
                .WithErrorMessage(InventoryValidationMessages.BarcodeTooLong);
        }

        [Fact]
        public void Should_Fail_When_ExpiryDate_Is_In_Past()
        {
            var dto = CreateValidInventoryItemDto();
            dto.ExpiryDate = DateTime.UtcNow.Date.AddDays(-1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
                .WithErrorMessage(InventoryValidationMessages.ExpiryDateInvalid);
        }

        [Fact]
        public void Should_Pass_When_ExpiryDate_Is_In_Future()
        {
            var dto = CreateValidInventoryItemDto();
            dto.ExpiryDate = DateTime.UtcNow.Date.AddYears(1);

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
        }

        private static CreateInventoryItemDto CreateValidInventoryItemDto()
        {
            return new CreateInventoryItemDto
            {
                ItemCode = "HA-001",
                Name = "Digital Hearing Aid Model X",
                Description = "High-quality digital hearing aid",
                Category = InventoryCategory.HearingAids,
                InitialQuantity = 50,
                ReorderLevel = 10,
                MaxStockLevel = 100,
                CostPrice = 500,
                SellingPrice = 750,
                Location = "Shelf A1"
            };
        }
    }

    #endregion

    #region UpdateInventoryItemValidator Tests

    public class UpdateInventoryItemValidatorTests
    {
        private readonly UpdateInventoryItemValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateInventoryItemDto
            {
                Id = 1,
                ItemCode = "HA-001",
                Name = "Updated Hearing Aid",
                Category = InventoryCategory.HearingAids,
                ReorderLevel = 10,
                MaxStockLevel = 100,
                CostPrice = 500,
                SellingPrice = 750,
                IsActive = true
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateInventoryItemDto
            {
                Id = 0,
                ItemCode = "HA-001",
                Name = "Item",
                Category = InventoryCategory.HearingAids,
                ReorderLevel = 10,
                MaxStockLevel = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    #endregion

    #region InventoryItemListRequestValidator Tests

    public class InventoryItemListRequestValidatorTests
    {
        private readonly InventoryItemListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new InventoryItemListRequestDto
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
            var dto = new InventoryItemListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(InventoryValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new InventoryItemListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(InventoryValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_SearchTerm_Exceeds_MaxLength()
        {
            var dto = new InventoryItemListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = new string('A', 101)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        }
    }

    #endregion

    #region AddStockValidator Tests

    public class AddStockValidatorTests
    {
        private readonly AddStockValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_AddStock()
        {
            var dto = new AddStockDto
            {
                InventoryItemId = 1,
                Quantity = 50,
                UnitCost = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InventoryItemId_Is_Zero()
        {
            var dto = new AddStockDto
            {
                InventoryItemId = 0,
                Quantity = 50,
                UnitCost = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(InventoryValidationMessages.InventoryItemRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Quantity_Is_Invalid(int quantity)
        {
            var dto = new AddStockDto
            {
                InventoryItemId = 1,
                Quantity = quantity,
                UnitCost = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(InventoryValidationMessages.QuantityInvalid);
        }

        [Fact]
        public void Should_Fail_When_UnitCost_Is_Negative()
        {
            var dto = new AddStockDto
            {
                InventoryItemId = 1,
                Quantity = 50,
                UnitCost = -10
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UnitCost)
                .WithErrorMessage(InventoryValidationMessages.UnitPriceInvalid);
        }

        [Fact]
        public void Should_Pass_When_UnitCost_Is_Zero()
        {
            var dto = new AddStockDto
            {
                InventoryItemId = 1,
                Quantity = 50,
                UnitCost = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UnitCost);
        }
    }

    #endregion

    #region RemoveStockValidator Tests

    public class RemoveStockValidatorTests
    {
        private readonly RemoveStockValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_RemoveStock()
        {
            var dto = new RemoveStockDto
            {
                InventoryItemId = 1,
                Quantity = 10
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InventoryItemId_Is_Zero()
        {
            var dto = new RemoveStockDto
            {
                InventoryItemId = 0,
                Quantity = 10
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(InventoryValidationMessages.InventoryItemRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Quantity_Is_Invalid(int quantity)
        {
            var dto = new RemoveStockDto
            {
                InventoryItemId = 1,
                Quantity = quantity
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(InventoryValidationMessages.QuantityInvalid);
        }

        [Fact]
        public void Should_Fail_When_PatientId_Is_Invalid()
        {
            var dto = new RemoveStockDto
            {
                InventoryItemId = 1,
                Quantity = 10,
                PatientId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PatientId);
        }
    }

    #endregion

    #region AdjustStockValidator Tests

    public class AdjustStockValidatorTests
    {
        private readonly AdjustStockValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Adjustment()
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 1,
                NewQuantity = 100,
                Reason = "Physical count correction"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InventoryItemId_Is_Zero()
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 0,
                NewQuantity = 100,
                Reason = "Correction"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(InventoryValidationMessages.InventoryItemRequired);
        }

        [Fact]
        public void Should_Fail_When_NewQuantity_Is_Negative()
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 1,
                NewQuantity = -1,
                Reason = "Correction"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.NewQuantity)
                .WithErrorMessage(InventoryValidationMessages.NewQuantityInvalid);
        }

        [Fact]
        public void Should_Pass_When_NewQuantity_Is_Zero()
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 1,
                NewQuantity = 0,
                Reason = "Stock write-off"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.NewQuantity);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Reason_Is_Empty(string? reason)
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 1,
                NewQuantity = 100,
                Reason = reason!
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Reason)
                .WithErrorMessage(InventoryValidationMessages.AdjustmentReasonRequired);
        }

        [Fact]
        public void Should_Fail_When_Reason_Exceeds_MaxLength()
        {
            var dto = new AdjustStockDto
            {
                InventoryItemId = 1,
                NewQuantity = 100,
                Reason = new string('A', 201)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Reason);
        }
    }

    #endregion

    #region TransferStockValidator Tests

    public class TransferStockValidatorTests
    {
        private readonly TransferStockValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Transfer()
        {
            var dto = new TransferStockDto
            {
                InventoryItemId = 1,
                TargetBranchId = 2,
                Quantity = 25
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InventoryItemId_Is_Zero()
        {
            var dto = new TransferStockDto
            {
                InventoryItemId = 0,
                TargetBranchId = 2,
                Quantity = 25
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(InventoryValidationMessages.InventoryItemRequired);
        }

        [Fact]
        public void Should_Fail_When_TargetBranchId_Is_Zero()
        {
            var dto = new TransferStockDto
            {
                InventoryItemId = 1,
                TargetBranchId = 0,
                Quantity = 25
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TargetBranchId)
                .WithErrorMessage(InventoryValidationMessages.TargetBranchRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Quantity_Is_Invalid(int quantity)
        {
            var dto = new TransferStockDto
            {
                InventoryItemId = 1,
                TargetBranchId = 2,
                Quantity = quantity
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(InventoryValidationMessages.QuantityInvalid);
        }
    }

    #endregion

    #region CreateInventoryTransactionValidator Tests

    public class CreateInventoryTransactionValidatorTests
    {
        private readonly CreateInventoryTransactionValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Transaction()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = 50,
                UnitPrice = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_InventoryItemId_Is_Zero()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 0,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = 50,
                UnitPrice = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
                .WithErrorMessage(InventoryValidationMessages.InventoryItemRequired);
        }

        [Fact]
        public void Should_Fail_When_TransactionType_Is_Invalid()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = (InventoryTransactionType)999,
                Quantity = 50,
                UnitPrice = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TransactionType)
                .WithErrorMessage(InventoryValidationMessages.TransactionTypeInvalid);
        }

        [Fact]
        public void Should_Fail_When_Quantity_Is_Zero()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = 0,
                UnitPrice = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage(InventoryValidationMessages.QuantityRequired);
        }

        [Fact]
        public void Should_Fail_When_UnitPrice_Is_Negative()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = 50,
                UnitPrice = -10
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
                .WithErrorMessage(InventoryValidationMessages.UnitPriceInvalid);
        }

        [Fact]
        public void Should_Fail_When_Transfer_Without_TargetBranch()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = InventoryTransactionType.Transfer,
                Quantity = 50,
                UnitPrice = 100,
                TransferToBranchId = null
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TransferToBranchId)
                .WithErrorMessage(InventoryValidationMessages.TargetBranchRequired);
        }

        [Fact]
        public void Should_Pass_When_Transfer_With_TargetBranch()
        {
            var dto = new CreateInventoryTransactionDto
            {
                InventoryItemId = 1,
                TransactionType = InventoryTransactionType.Transfer,
                Quantity = 50,
                UnitPrice = 100,
                TransferToBranchId = 2
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TransferToBranchId);
        }
    }

    #endregion

    #region InventoryTransactionListRequestValidator Tests

    public class InventoryTransactionListRequestValidatorTests
    {
        private readonly InventoryTransactionListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new InventoryTransactionListRequestDto
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
            var dto = new InventoryTransactionListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(InventoryValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new InventoryTransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(InventoryValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new InventoryTransactionListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(InventoryValidationMessages.DateRangeInvalid);
        }

        [Fact]
        public void Should_Pass_When_DateRange_Is_Valid()
        {
            var dto = new InventoryTransactionListRequestDto
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
