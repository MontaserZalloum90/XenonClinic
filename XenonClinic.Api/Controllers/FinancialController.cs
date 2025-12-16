using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for Financial management operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinancialController : BaseApiController
{
    private readonly IFinancialService _financialService;
    private readonly IPatientService _patientService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly IValidator<CreateAccountDto> _createAccountValidator;
    private readonly IValidator<UpdateAccountDto> _updateAccountValidator;
    private readonly IValidator<CreateInvoiceDto> _createInvoiceValidator;
    private readonly IValidator<UpdateInvoiceDto> _updateInvoiceValidator;
    private readonly IValidator<RecordPaymentDto> _recordPaymentValidator;
    private readonly IValidator<InvoiceListRequestDto> _invoiceListValidator;
    private readonly IValidator<CreateExpenseDto> _createExpenseValidator;
    private readonly IValidator<UpdateExpenseDto> _updateExpenseValidator;
    private readonly IValidator<ApproveExpenseDto> _approveExpenseValidator;
    private readonly IValidator<RejectExpenseDto> _rejectExpenseValidator;
    private readonly IValidator<ExpenseListRequestDto> _expenseListValidator;
    private readonly IValidator<CreateTransactionDto> _createTransactionValidator;
    private readonly IValidator<TransactionListRequestDto> _transactionListValidator;
    private readonly ILogger<FinancialController> _logger;

    public FinancialController(
        IFinancialService financialService,
        IPatientService patientService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        IValidator<CreateAccountDto> createAccountValidator,
        IValidator<UpdateAccountDto> updateAccountValidator,
        IValidator<CreateInvoiceDto> createInvoiceValidator,
        IValidator<UpdateInvoiceDto> updateInvoiceValidator,
        IValidator<RecordPaymentDto> recordPaymentValidator,
        IValidator<InvoiceListRequestDto> invoiceListValidator,
        IValidator<CreateExpenseDto> createExpenseValidator,
        IValidator<UpdateExpenseDto> updateExpenseValidator,
        IValidator<ApproveExpenseDto> approveExpenseValidator,
        IValidator<RejectExpenseDto> rejectExpenseValidator,
        IValidator<ExpenseListRequestDto> expenseListValidator,
        IValidator<CreateTransactionDto> createTransactionValidator,
        IValidator<TransactionListRequestDto> transactionListValidator,
        ILogger<FinancialController> logger)
    {
        _financialService = financialService;
        _patientService = patientService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _createAccountValidator = createAccountValidator;
        _updateAccountValidator = updateAccountValidator;
        _createInvoiceValidator = createInvoiceValidator;
        _updateInvoiceValidator = updateInvoiceValidator;
        _recordPaymentValidator = recordPaymentValidator;
        _invoiceListValidator = invoiceListValidator;
        _createExpenseValidator = createExpenseValidator;
        _updateExpenseValidator = updateExpenseValidator;
        _approveExpenseValidator = approveExpenseValidator;
        _rejectExpenseValidator = rejectExpenseValidator;
        _expenseListValidator = expenseListValidator;
        _createTransactionValidator = createTransactionValidator;
        _transactionListValidator = transactionListValidator;
        _logger = logger;
    }

    #region Account Management

    /// <summary>
    /// Gets all accounts for the current branch.
    /// </summary>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts([FromQuery] AccountType? type = null)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var accounts = type.HasValue
            ? await _financialService.GetAccountsByTypeAsync(branchId.Value, type.Value)
            : await _financialService.GetAccountsByBranchIdAsync(branchId.Value);

        var dtos = accounts.Select(MapToAccountDto);
        return ApiOk(dtos);
    }

    /// <summary>
    /// Gets an account by ID.
    /// </summary>
    [HttpGet("accounts/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await _financialService.GetAccountByIdAsync(id);

        if (account == null)
        {
            return ApiNotFound(FinancialValidationMessages.AccountNotFound);
        }

        if (!HasBranchAccess(account.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        var dto = MapToAccountDto(account);
        return ApiOk(dto);
    }

    /// <summary>
    /// Gets account balance.
    /// </summary>
    [HttpGet("accounts/{id:int}/balance")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountBalance(int id)
    {
        var account = await _financialService.GetAccountByIdAsync(id);

        if (account == null)
        {
            return ApiNotFound(FinancialValidationMessages.AccountNotFound);
        }

        if (!HasBranchAccess(account.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        var balance = await _financialService.GetAccountBalanceAsync(id);
        return ApiOk(balance);
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    [HttpPost("accounts")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        var validationResult = await _createAccountValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var account = new Account
        {
            BranchId = branchId.Value,
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            AccountType = dto.AccountType,
            ParentAccountId = dto.ParentAccountId,
            Description = dto.Description,
            Balance = dto.InitialBalance,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            CreatedBy = _userContext.RequireUserId()
        };

        var createdAccount = await _financialService.CreateAccountAsync(account);

        _logger.LogInformation(
            "Account created: {AccountId}, Code: {AccountCode}, Branch: {BranchId}, By: {UserId}",
            createdAccount.Id, createdAccount.AccountCode, branchId, _userContext.UserId);

        var resultDto = MapToAccountDto(createdAccount);
        return ApiCreated(resultDto, $"/api/financial/accounts/{createdAccount.Id}");
    }

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    [HttpPut("accounts/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateAccountValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var existingAccount = await _financialService.GetAccountByIdAsync(id);
        if (existingAccount == null)
        {
            return ApiNotFound(FinancialValidationMessages.AccountNotFound);
        }

        if (!HasBranchAccess(existingAccount.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        existingAccount.AccountCode = dto.AccountCode;
        existingAccount.AccountName = dto.AccountName;
        existingAccount.AccountType = dto.AccountType;
        existingAccount.ParentAccountId = dto.ParentAccountId;
        existingAccount.Description = dto.Description;
        existingAccount.IsActive = dto.IsActive;
        existingAccount.UpdatedAt = DateTime.UtcNow;
        existingAccount.UpdatedBy = _userContext.UserId;

        await _financialService.UpdateAccountAsync(existingAccount);

        _logger.LogInformation(
            "Account updated: {AccountId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToAccountDto(existingAccount);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes an account.
    /// </summary>
    [HttpDelete("accounts/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _financialService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return ApiNotFound(FinancialValidationMessages.AccountNotFound);
        }

        if (!HasBranchAccess(account.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        await _financialService.DeleteAccountAsync(id);

        _logger.LogInformation(
            "Account deleted: {AccountId}, By: {UserId}",
            id, _userContext.UserId);

        return ApiOk("Account deleted successfully");
    }

    #endregion

    #region Invoice Management

    /// <summary>
    /// Gets invoices for the current branch.
    /// </summary>
    [HttpGet("invoices")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InvoiceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceListRequestDto request)
    {
        var validationResult = await _invoiceListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var invoices = request.Status.HasValue
            ? await _financialService.GetInvoicesByStatusAsync(branchId.Value, request.Status.Value)
            : await _financialService.GetInvoicesByBranchIdAsync(branchId.Value);

        var query = invoices.AsQueryable();

        // Apply filters
        if (request.PatientId.HasValue)
        {
            query = query.Where(i => i.PatientId == request.PatientId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= request.DateTo.Value);
        }

        // Apply sorting
        query = request.SortDescending
            ? query.OrderByDescending(i => i.InvoiceDate)
            : query.OrderBy(i => i.InvoiceDate);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToInvoiceDto)
            .ToList();

        var paginatedResult = new PaginatedResponse<InvoiceDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets an invoice by ID.
    /// </summary>
    [HttpGet("invoices/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _financialService.GetInvoiceByIdAsync(id);

        if (invoice == null)
        {
            return ApiNotFound(FinancialValidationMessages.InvoiceNotFound);
        }

        if (!HasBranchAccess(invoice.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        var dto = MapToInvoiceDto(invoice);
        return ApiOk(dto);
    }

    /// <summary>
    /// Gets an invoice by invoice number.
    /// </summary>
    [HttpGet("invoices/by-number/{invoiceNumber}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByNumber(string invoiceNumber)
    {
        var invoice = await _financialService.GetInvoiceByNumberAsync(invoiceNumber);

        if (invoice == null)
        {
            return ApiNotFound(FinancialValidationMessages.InvoiceNotFound);
        }

        if (!HasBranchAccess(invoice.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        var dto = MapToInvoiceDto(invoice);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    [HttpPost("invoices")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        var validationResult = await _createInvoiceValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        // Verify patient exists
        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
        {
            return ApiNotFound(FinancialValidationMessages.PatientNotFound);
        }

        // Generate invoice number
        var invoiceNumber = await _financialService.GenerateInvoiceNumberAsync(branchId.Value);

        // Calculate totals with proper currency precision
        var discountAmount = dto.DiscountAmount ?? Math.Round(dto.SubTotal * (dto.DiscountPercentage ?? 0m) / 100m, 2, MidpointRounding.AwayFromZero);
        var taxableAmount = Math.Round(dto.SubTotal - discountAmount, 2, MidpointRounding.AwayFromZero);
        var taxAmount = Math.Round(taxableAmount * (dto.TaxPercentage ?? 5m) / 100m, 2, MidpointRounding.AwayFromZero);
        var totalAmount = Math.Round(taxableAmount + taxAmount, 2, MidpointRounding.AwayFromZero);

        var invoice = new Invoice
        {
            BranchId = branchId.Value,
            InvoiceNumber = invoiceNumber,
            PatientId = dto.PatientId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Draft,
            SubTotal = dto.SubTotal,
            DiscountPercentage = dto.DiscountPercentage,
            DiscountAmount = discountAmount,
            TaxPercentage = dto.TaxPercentage ?? 5,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description,
            Notes = dto.Notes,
            Terms = dto.Terms,
            SaleId = dto.SaleId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdInvoice = await _financialService.CreateInvoiceAsync(invoice);

        _logger.LogInformation(
            "Invoice created: {InvoiceId}, Number: {InvoiceNumber}, Patient: {PatientId}, By: {UserId}",
            createdInvoice.Id, createdInvoice.InvoiceNumber, dto.PatientId, _userContext.UserId);

        var resultDto = MapToInvoiceDto(createdInvoice);
        return ApiCreated(resultDto, $"/api/financial/invoices/{createdInvoice.Id}");
    }

    /// <summary>
    /// Records a payment for an invoice.
    /// BUG FIX: Added optimistic concurrency control to prevent race conditions.
    /// </summary>
    [HttpPost("invoices/{id:int}/payment")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] RecordPaymentDto dto)
    {
        dto.InvoiceId = id;

        var validationResult = await _recordPaymentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var invoice = await _financialService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return ApiNotFound(FinancialValidationMessages.InvoiceNotFound);
        }

        if (!HasBranchAccess(invoice.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        if (invoice.IsFullyPaid)
        {
            return ApiBadRequest(FinancialValidationMessages.InvoiceAlreadyPaid);
        }

        if (dto.Amount > invoice.RemainingAmount)
        {
            return ApiBadRequest(FinancialValidationMessages.PaymentExceedsRemaining);
        }

        // Update invoice
        invoice.PaidAmount += dto.Amount;
        invoice.PaymentMethod = dto.PaymentMethod;
        invoice.Status = invoice.IsFullyPaid ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = _userContext.UserId;

        try
        {
            await _financialService.UpdateInvoiceAsync(invoice);
        }
        catch (DbUpdateConcurrencyException)
        {
            // BUG FIX: Handle concurrent payment attempts with optimistic concurrency
            _logger.LogWarning(
                "Concurrent payment conflict for invoice: {InvoiceId}. Request by user: {UserId}",
                id, _userContext.UserId);

            return StatusCode(StatusCodes.Status409Conflict, new ApiResponse
            {
                Success = false,
                Error = "The invoice was modified by another user. Please refresh and try again."
            });
        }

        _logger.LogInformation(
            "Payment recorded for invoice: {InvoiceId}, Amount: {Amount}, By: {UserId}",
            id, dto.Amount, _userContext.UserId);

        var resultDto = MapToInvoiceDto(invoice);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Gets unpaid invoices count.
    /// </summary>
    [HttpGet("invoices/unpaid/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnpaidInvoicesCount()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var count = await _financialService.GetUnpaidInvoicesCountAsync(branchId.Value);
        return ApiOk(count);
    }

    /// <summary>
    /// Gets unpaid invoices total amount.
    /// </summary>
    [HttpGet("invoices/unpaid/total")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnpaidInvoicesTotal()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var total = await _financialService.GetUnpaidInvoicesTotalAsync(branchId.Value);
        return ApiOk(total);
    }

    #endregion

    #region Expense Management

    /// <summary>
    /// Gets expenses for the current branch.
    /// </summary>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses([FromQuery] ExpenseListRequestDto request)
    {
        var validationResult = await _expenseListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var expenses = request.CategoryId.HasValue
            ? await _financialService.GetExpensesByCategoryAsync(branchId.Value, request.CategoryId.Value)
            : await _financialService.GetExpensesByBranchIdAsync(branchId.Value);

        var query = expenses.AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            var status = MapToExpenseStatus(request.Status.Value);
            query = query.Where(e => e.Status == status);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= request.DateTo.Value);
        }

        // Apply sorting
        query = request.SortDescending
            ? query.OrderByDescending(e => e.ExpenseDate)
            : query.OrderBy(e => e.ExpenseDate);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToExpenseDto)
            .ToList();

        var paginatedResult = new PaginatedResponse<ExpenseDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets an expense by ID.
    /// </summary>
    [HttpGet("expenses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExpense(int id)
    {
        var expense = await _financialService.GetExpenseByIdAsync(id);

        if (expense == null)
        {
            return ApiNotFound(FinancialValidationMessages.ExpenseNotFound);
        }

        if (!HasBranchAccess(expense.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        var dto = MapToExpenseDto(expense);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new expense.
    /// </summary>
    [HttpPost("expenses")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var validationResult = await _createExpenseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var expense = new Expense
        {
            BranchId = branchId.Value,
            ExpenseNumber = $"EXP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            ExpenseDate = dto.ExpenseDate,
            ExpenseCategoryId = dto.ExpenseCategoryId,
            Description = dto.Description,
            Amount = dto.Amount,
            Status = ExpenseStatus.Pending,
            Vendor = dto.Vendor,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            PaymentMethod = dto.PaymentMethod,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            AttachmentPath = dto.AttachmentPath,
            CreatedAt = DateTime.UtcNow,
            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            CreatedBy = _userContext.RequireUserId()
        };

        var createdExpense = await _financialService.CreateExpenseAsync(expense);

        _logger.LogInformation(
            "Expense created: {ExpenseId}, Amount: {Amount}, Branch: {BranchId}, By: {UserId}",
            createdExpense.Id, dto.Amount, branchId, _userContext.UserId);

        var resultDto = MapToExpenseDto(createdExpense);
        return ApiCreated(resultDto, $"/api/financial/expenses/{createdExpense.Id}");
    }

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    [HttpPut("expenses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateExpenseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var existingExpense = await _financialService.GetExpenseByIdAsync(id);
        if (existingExpense == null)
        {
            return ApiNotFound(FinancialValidationMessages.ExpenseNotFound);
        }

        if (!HasBranchAccess(existingExpense.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        existingExpense.ExpenseDate = dto.ExpenseDate;
        existingExpense.ExpenseCategoryId = dto.ExpenseCategoryId;
        existingExpense.Description = dto.Description;
        existingExpense.Amount = dto.Amount;
        existingExpense.Vendor = dto.Vendor;
        existingExpense.InvoiceNumber = dto.InvoiceNumber;
        existingExpense.InvoiceDate = dto.InvoiceDate;
        existingExpense.PaymentMethod = dto.PaymentMethod;
        existingExpense.ReferenceNumber = dto.ReferenceNumber;
        existingExpense.Notes = dto.Notes;
        existingExpense.AttachmentPath = dto.AttachmentPath;
        existingExpense.UpdatedAt = DateTime.UtcNow;
        existingExpense.UpdatedBy = _userContext.UserId;

        await _financialService.UpdateExpenseAsync(existingExpense);

        _logger.LogInformation(
            "Expense updated: {ExpenseId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToExpenseDto(existingExpense);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Approves an expense.
    /// </summary>
    [HttpPost("expenses/{id:int}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveExpense(int id, [FromBody] ApproveExpenseDto dto)
    {
        dto.ExpenseId = id;

        var validationResult = await _approveExpenseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var expense = await _financialService.GetExpenseByIdAsync(id);
        if (expense == null)
        {
            return ApiNotFound(FinancialValidationMessages.ExpenseNotFound);
        }

        if (!HasBranchAccess(expense.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            return ApiBadRequest(FinancialValidationMessages.ExpenseAlreadyApproved);
        }

        expense.Status = ExpenseStatus.Approved;
        expense.ApprovedBy = _userContext.UserId;
        expense.ApprovedDate = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;
        expense.UpdatedBy = _userContext.UserId;

        await _financialService.UpdateExpenseAsync(expense);

        _logger.LogInformation(
            "Expense approved: {ExpenseId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToExpenseDto(expense);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Rejects an expense.
    /// </summary>
    [HttpPost("expenses/{id:int}/reject")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectExpense(int id, [FromBody] RejectExpenseDto dto)
    {
        dto.ExpenseId = id;

        var validationResult = await _rejectExpenseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var expense = await _financialService.GetExpenseByIdAsync(id);
        if (expense == null)
        {
            return ApiNotFound(FinancialValidationMessages.ExpenseNotFound);
        }

        if (!HasBranchAccess(expense.BranchId))
        {
            return ApiForbidden(FinancialValidationMessages.BranchAccessDenied);
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            return ApiBadRequest(FinancialValidationMessages.ExpenseAlreadyRejected);
        }

        expense.Status = ExpenseStatus.Rejected;
        expense.UpdatedAt = DateTime.UtcNow;
        expense.UpdatedBy = _userContext.UserId;

        await _financialService.UpdateExpenseAsync(expense);

        _logger.LogInformation(
            "Expense rejected: {ExpenseId}, Reason: {Reason}, By: {UserId}",
            id, dto.RejectionReason, _userContext.UserId);

        var resultDto = MapToExpenseDto(expense);
        return ApiOk(resultDto);
    }

    #endregion

    #region Transaction Management

    /// <summary>
    /// Gets transactions for the current branch.
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<FinancialTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionListRequestDto request)
    {
        var validationResult = await _transactionListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<FinancialTransaction> transactions;
        var dateFrom = request.DateFrom ?? DateTime.UtcNow.AddMonths(-1);
        var dateTo = request.DateTo ?? DateTime.UtcNow;

        if (request.AccountId.HasValue)
        {
            transactions = await _financialService.GetTransactionsByAccountIdAsync(request.AccountId.Value);
        }
        else
        {
            transactions = await _financialService.GetTransactionsByDateRangeAsync(branchId.Value, dateFrom, dateTo);
        }

        var query = transactions.AsQueryable();

        // Apply filters
        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == request.TransactionType.Value);
        }

        // Apply sorting
        query = request.SortDescending
            ? query.OrderByDescending(t => t.TransactionDate)
            : query.OrderBy(t => t.TransactionDate);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToTransactionDto)
            .ToList();

        var paginatedResult = new PaginatedResponse<FinancialTransactionDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var validationResult = await _createTransactionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        // Verify account exists
        var account = await _financialService.GetAccountByIdAsync(dto.AccountId);
        if (account == null)
        {
            return ApiNotFound(FinancialValidationMessages.AccountNotFound);
        }

        var transaction = new FinancialTransaction
        {
            BranchId = branchId.Value,
            TransactionNumber = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            TransactionDate = dto.TransactionDate,
            AccountId = dto.AccountId,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            Description = dto.Description,
            ReferenceNumber = dto.ReferenceNumber,
            Status = VoucherStatus.Posted,
            Notes = dto.Notes,
            ExpenseId = dto.ExpenseId,
            SaleId = dto.SaleId,
            CreatedAt = DateTime.UtcNow,
            // BUG FIX: Use RequireUserId() to ensure audit trail integrity
            CreatedBy = _userContext.RequireUserId()
        };

        var createdTransaction = await _financialService.CreateTransactionAsync(transaction);

        _logger.LogInformation(
            "Transaction created: {TransactionId}, Account: {AccountId}, Amount: {Amount}, By: {UserId}",
            createdTransaction.Id, dto.AccountId, dto.Amount, _userContext.UserId);

        var resultDto = MapToTransactionDto(createdTransaction);
        return ApiCreated(resultDto, $"/api/financial/transactions/{createdTransaction.Id}");
    }

    #endregion

    #region Statistics & Reports

    /// <summary>
    /// Gets financial statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<FinancialStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = endDate ?? DateTime.UtcNow;

        var statistics = new FinancialStatisticsDto
        {
            TotalRevenue = await _financialService.GetTotalRevenueAsync(branchId.Value, start, end),
            TotalExpenses = await _financialService.GetTotalExpensesAsync(branchId.Value, start, end),
            NetProfit = await _financialService.GetNetProfitAsync(branchId.Value, start, end),
            UnpaidInvoicesCount = await _financialService.GetUnpaidInvoicesCountAsync(branchId.Value),
            UnpaidInvoicesTotal = await _financialService.GetUnpaidInvoicesTotalAsync(branchId.Value)
        };

        return ApiOk(statistics);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentBranchId() => _tenantContext.BranchId;

    private bool HasBranchAccess(int branchId)
    {
        if (_tenantContext.IsCompanyAdmin)
        {
            return true;
        }
        return _tenantContext.HasBranchAccess(branchId);
    }

    private static AccountDto MapToAccountDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            BranchId = account.BranchId,
            BranchName = account.Branch?.Name,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            ParentAccountId = account.ParentAccountId,
            ParentAccountName = account.ParentAccount?.AccountName,
            Description = account.Description,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            CreatedBy = account.CreatedBy,
            UpdatedAt = account.UpdatedAt,
            UpdatedBy = account.UpdatedBy
        };
    }

    private static InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            BranchId = invoice.BranchId,
            BranchName = invoice.Branch?.Name,
            InvoiceNumber = invoice.InvoiceNumber,
            PatientId = invoice.PatientId,
            PatientName = invoice.Patient?.FullNameEn,
            // SECURITY FIX: Removed PatientEmiratesId - PII should not be exposed in invoice responses
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            PaymentStatus = invoice.PaymentStatus,
            SubTotal = invoice.SubTotal,
            DiscountPercentage = invoice.DiscountPercentage,
            DiscountAmount = invoice.DiscountAmount,
            TaxPercentage = invoice.TaxPercentage,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            PaymentMethod = invoice.PaymentMethod,
            Description = invoice.Description,
            Notes = invoice.Notes,
            Terms = invoice.Terms,
            SaleId = invoice.SaleId,
            CreatedAt = invoice.CreatedAt,
            CreatedBy = invoice.CreatedBy,
            UpdatedAt = invoice.UpdatedAt,
            UpdatedBy = invoice.UpdatedBy
        };
    }

    private static ExpenseDto MapToExpenseDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            BranchId = expense.BranchId,
            BranchName = expense.Branch?.Name,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseDate = expense.ExpenseDate,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            CategoryName = expense.ExpenseCategory?.Name,
            Description = expense.Description,
            Amount = expense.Amount,
            Status = expense.Status,
            Vendor = expense.Vendor,
            InvoiceNumber = expense.InvoiceNumber,
            InvoiceDate = expense.InvoiceDate,
            PaymentMethod = expense.PaymentMethod,
            ReferenceNumber = expense.ReferenceNumber,
            PaymentDate = expense.PaymentDate,
            Notes = expense.Notes,
            AttachmentPath = expense.AttachmentPath,
            ApprovedBy = expense.ApprovedBy,
            ApprovedDate = expense.ApprovedDate,
            CreatedAt = expense.CreatedAt,
            CreatedBy = expense.CreatedBy,
            UpdatedAt = expense.UpdatedAt,
            UpdatedBy = expense.UpdatedBy
        };
    }

    private static FinancialTransactionDto MapToTransactionDto(FinancialTransaction transaction)
    {
        return new FinancialTransactionDto
        {
            Id = transaction.Id,
            BranchId = transaction.BranchId,
            BranchName = transaction.Branch?.Name,
            TransactionNumber = transaction.TransactionNumber,
            TransactionDate = transaction.TransactionDate,
            AccountId = transaction.AccountId,
            AccountName = transaction.Account?.AccountName,
            AccountCode = transaction.Account?.AccountCode,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            Description = transaction.Description,
            ReferenceNumber = transaction.ReferenceNumber,
            Status = transaction.Status,
            Notes = transaction.Notes,
            ExpenseId = transaction.ExpenseId,
            SaleId = transaction.SaleId,
            CreatedAt = transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy
        };
    }

    private static ExpenseStatus MapToExpenseStatus(ExpenseStatus status)
    {
        // This method is an identity mapping and can be simplified
        return status;
    }

    #endregion
}
