using FluentValidation;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

#region Inventory Item Validators

/// <summary>
/// Validator for CreateInventoryItemDto.
/// </summary>
public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemDto>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage(InventoryValidationMessages.ItemCodeRequired)
            .MaximumLength(50).WithMessage(InventoryValidationMessages.ItemCodeTooLong)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage(InventoryValidationMessages.ItemCodeInvalid);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(InventoryValidationMessages.ItemNameRequired)
            .MaximumLength(200).WithMessage(InventoryValidationMessages.ItemNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(InventoryValidationMessages.CategoryInvalid);

        RuleFor(x => x.InitialQuantity)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.NewQuantityInvalid);

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.ReorderLevelInvalid);

        RuleFor(x => x.MaxStockLevel)
            .GreaterThan(0).WithMessage("Max stock level must be greater than 0")
            .GreaterThanOrEqualTo(x => x.ReorderLevel).WithMessage(InventoryValidationMessages.MaxStockLevelInvalid);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.CostPriceInvalid);

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.SellingPriceInvalid);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.SupplierInvalid)
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.SupplierPartNumber)
            .MaximumLength(100).WithMessage("Supplier part number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SupplierPartNumber));

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage(InventoryValidationMessages.BarcodeTooLong)
            .When(x => !string.IsNullOrEmpty(x.Barcode));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage(InventoryValidationMessages.LocationTooLong)
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage(InventoryValidationMessages.ExpiryDateInvalid)
            .When(x => x.ExpiryDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateInventoryItemDto.
/// </summary>
public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemDto>
{
    public UpdateInventoryItemValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Item ID is required");

        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage(InventoryValidationMessages.ItemCodeRequired)
            .MaximumLength(50).WithMessage(InventoryValidationMessages.ItemCodeTooLong)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage(InventoryValidationMessages.ItemCodeInvalid);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(InventoryValidationMessages.ItemNameRequired)
            .MaximumLength(200).WithMessage(InventoryValidationMessages.ItemNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(InventoryValidationMessages.CategoryInvalid);

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.ReorderLevelInvalid);

        RuleFor(x => x.MaxStockLevel)
            .GreaterThan(0).WithMessage("Max stock level must be greater than 0")
            .GreaterThanOrEqualTo(x => x.ReorderLevel).WithMessage(InventoryValidationMessages.MaxStockLevelInvalid);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.CostPriceInvalid);

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.SellingPriceInvalid);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.SupplierInvalid)
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.SupplierPartNumber)
            .MaximumLength(100).WithMessage("Supplier part number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SupplierPartNumber));

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage(InventoryValidationMessages.BarcodeTooLong)
            .When(x => !string.IsNullOrEmpty(x.Barcode));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage(InventoryValidationMessages.LocationTooLong)
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for InventoryItemListRequestDto.
/// </summary>
public class InventoryItemListRequestValidator : AbstractValidator<InventoryItemListRequestDto>
{
    public InventoryItemListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(InventoryValidationMessages.InvalidPageSize);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(InventoryValidationMessages.CategoryInvalid)
            .When(x => x.Category.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.SupplierInvalid)
            .When(x => x.SupplierId.HasValue);
    }
}

#endregion

#region Stock Operation Validators

/// <summary>
/// Validator for AddStockDto.
/// </summary>
public class AddStockValidator : AbstractValidator<AddStockDto>
{
    public AddStockValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InventoryItemRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.QuantityInvalid);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.UnitPriceInvalid);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for RemoveStockDto.
/// </summary>
public class RemoveStockValidator : AbstractValidator<RemoveStockDto>
{
    public RemoveStockValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InventoryItemRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.QuantityInvalid);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for AdjustStockDto.
/// </summary>
public class AdjustStockValidator : AbstractValidator<AdjustStockDto>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InventoryItemRequired);

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.NewQuantityInvalid);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(InventoryValidationMessages.AdjustmentReasonRequired)
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for TransferStockDto.
/// </summary>
public class TransferStockValidator : AbstractValidator<TransferStockDto>
{
    public TransferStockValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InventoryItemRequired);

        RuleFor(x => x.TargetBranchId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.TargetBranchRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.QuantityInvalid);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

#endregion

#region Transaction Validators

/// <summary>
/// Validator for CreateInventoryTransactionDto.
/// </summary>
public class CreateInventoryTransactionValidator : AbstractValidator<CreateInventoryTransactionDto>
{
    public CreateInventoryTransactionValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InventoryItemRequired);

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage(InventoryValidationMessages.TransactionTypeInvalid);

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage(InventoryValidationMessages.QuantityRequired);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage(InventoryValidationMessages.UnitPriceInvalid);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.TransferToBranchId)
            .GreaterThan(0).WithMessage("Invalid transfer branch ID")
            .When(x => x.TransferToBranchId.HasValue);

        // Require transfer branch for transfer transactions
        RuleFor(x => x.TransferToBranchId)
            .NotNull().WithMessage(InventoryValidationMessages.TargetBranchRequired)
            .When(x => x.TransactionType == InventoryTransactionType.Transfer);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for InventoryTransactionListRequestDto.
/// </summary>
public class InventoryTransactionListRequestValidator : AbstractValidator<InventoryTransactionListRequestDto>
{
    public InventoryTransactionListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(InventoryValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(InventoryValidationMessages.InvalidPageSize);

        RuleFor(x => x.InventoryItemId)
            .GreaterThan(0).WithMessage("Invalid inventory item ID")
            .When(x => x.InventoryItemId.HasValue);

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage(InventoryValidationMessages.TransactionTypeInvalid)
            .When(x => x.TransactionType.HasValue);

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage(InventoryValidationMessages.DateRangeInvalid)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));
    }
}

#endregion
