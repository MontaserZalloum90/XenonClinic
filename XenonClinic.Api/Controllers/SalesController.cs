using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Controller for sales management operations.
/// Handles sales, quotations, and payments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class SalesController : BaseApiController
{
    private readonly ISalesService _salesService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        ISalesService salesService,
        ICurrentUserService currentUserService,
        ILogger<SalesController> logger)
    {
        _salesService = salesService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Sales

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SaleDto>>>> GetSales([FromQuery] SaleListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var sales = await _salesService.GetSalesByBranchIdAsync(branchId.Value);
            var filtered = sales.AsQueryable();

            if (request.PatientId.HasValue) filtered = filtered.Where(s => s.PatientId == request.PatientId.Value);
            if (request.Status.HasValue) filtered = filtered.Where(s => s.Status == request.Status.Value);
            if (request.PaymentStatus.HasValue) filtered = filtered.Where(s => s.PaymentStatus == request.PaymentStatus.Value);
            if (request.DateFrom.HasValue) filtered = filtered.Where(s => s.SaleDate >= request.DateFrom.Value);
            if (request.DateTo.HasValue) filtered = filtered.Where(s => s.SaleDate <= request.DateTo.Value);
            if (request.OverdueOnly == true) filtered = filtered.Where(s => s.IsOverdue);
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm;
                filtered = filtered.Where(s =>
                    s.InvoiceNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (s.Patient != null && s.Patient.FullNameEn.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            var totalCount = filtered.Count();
            filtered = request.SortDescending
                ? filtered.OrderByDescending(s => s.SaleDate)
                : filtered.OrderBy(s => s.SaleDate);

            var paged = filtered.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
                .Select(MapToSaleDto).ToList();

            return ApiPaginated(paged, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales");
            return ApiServerError("Failed to retrieve sales");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SaleDto>>> GetSale(int id)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            return ApiOk(MapToSaleDto(sale));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sale {Id}", id);
            return ApiServerError("Failed to retrieve sale");
        }
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SaleDto>>>> GetPatientSales(int patientId)
    {
        try
        {
            var sales = await _salesService.GetSalesByPatientIdAsync(patientId);
            var filtered = sales.Where(s => HasBranchAccess(s.BranchId));
            return ApiOk(filtered.Select(MapToSaleDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales for patient {PatientId}", patientId);
            return ApiServerError("Failed to retrieve patient sales");
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SaleDto>>>> GetOverdueSales()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");
            var sales = await _salesService.GetOverdueSalesAsync(branchId.Value);
            return ApiOk(sales.Select(MapToSaleDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue sales");
            return ApiServerError("Failed to retrieve overdue sales");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SaleDto>>> CreateSale([FromBody] CreateSaleDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return ApiBadRequestFromModelState();

            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var invoiceNumber = await _salesService.GenerateInvoiceNumberAsync(branchId.Value);
            var items = dto.Items.Select(i => new SaleItem
            {
                InventoryItemId = i.InventoryItemId,
                ItemName = i.ItemName,
                ItemDescription = i.ItemDescription,
                ItemCode = i.ItemCode,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercentage = i.DiscountPercentage,
                TaxPercentage = i.TaxPercentage,
                Subtotal = i.Quantity * i.UnitPrice,
                SerialNumber = i.SerialNumber,
                WarrantyStartDate = i.WarrantyMonths.HasValue ? DateTime.UtcNow : null,
                WarrantyEndDate = i.WarrantyMonths.HasValue ? DateTime.UtcNow.AddMonths(i.WarrantyMonths.Value) : null,
                Notes = i.Notes,
                CreatedBy = _currentUserService.UserId
            }).ToList();

            foreach (var item in items)
            {
                item.DiscountAmount = item.DiscountPercentage.HasValue ? item.Subtotal * (item.DiscountPercentage.Value / 100) : null;
                var afterDiscount = item.Subtotal - (item.DiscountAmount ?? 0);
                item.TaxAmount = item.TaxPercentage.HasValue ? afterDiscount * (item.TaxPercentage.Value / 100) : null;
                item.Total = afterDiscount + (item.TaxAmount ?? 0);
            }

            var subTotal = items.Sum(i => i.Subtotal);
            var discountAmount = dto.DiscountPercentage.HasValue ? subTotal * (dto.DiscountPercentage.Value / 100) : 0;
            var afterSaleDiscount = subTotal - discountAmount;
            var taxAmount = dto.TaxPercentage.HasValue ? afterSaleDiscount * (dto.TaxPercentage.Value / 100) : 0;

            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                SaleDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Status = SaleStatus.Draft,
                PaymentStatus = PaymentStatus.Pending,
                PatientId = dto.PatientId,
                BranchId = branchId.Value,
                SubTotal = subTotal,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = discountAmount,
                TaxPercentage = dto.TaxPercentage,
                TaxAmount = taxAmount,
                Total = afterSaleDiscount + taxAmount,
                Notes = dto.Notes,
                Terms = dto.Terms,
                QuotationId = dto.QuotationId,
                Items = items,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var created = await _salesService.CreateSaleAsync(sale);
            _logger.LogInformation("Sale created: {InvoiceNumber}", sale.InvoiceNumber);
            return ApiCreated(MapToSaleDto(created), message: "Sale created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale");
            return ApiServerError("Failed to create sale");
        }
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<ApiResponse>> ConfirmSale(int id)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            if (sale.Status != SaleStatus.Draft) return ApiBadRequest("Only draft sales can be confirmed");

            await _salesService.ConfirmSaleAsync(id);
            _logger.LogInformation("Sale confirmed: {InvoiceNumber}", sale.InvoiceNumber);
            return ApiOk("Sale confirmed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming sale {Id}", id);
            return ApiServerError("Failed to confirm sale");
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ApiResponse>> CompleteSale(int id)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            if (sale.Status != SaleStatus.Confirmed) return ApiBadRequest("Only confirmed sales can be completed");

            await _salesService.CompleteSaleAsync(id);
            _logger.LogInformation("Sale completed: {InvoiceNumber}", sale.InvoiceNumber);
            return ApiOk("Sale completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing sale {Id}", id);
            return ApiServerError("Failed to complete sale");
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse>> CancelSale(int id)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            if (sale.Status == SaleStatus.Completed || sale.Status == SaleStatus.Cancelled)
                return ApiBadRequest("This sale cannot be cancelled");

            await _salesService.CancelSaleAsync(id);
            _logger.LogInformation("Sale cancelled: {InvoiceNumber}", sale.InvoiceNumber);
            return ApiOk("Sale cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling sale {Id}", id);
            return ApiServerError("Failed to cancel sale");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSale(int id)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            if (sale.Status != SaleStatus.Draft) return ApiBadRequest("Only draft sales can be deleted");

            await _salesService.DeleteSaleAsync(id);
            _logger.LogInformation("Sale deleted: {InvoiceNumber}", sale.InvoiceNumber);
            return ApiOk("Sale deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sale {Id}", id);
            return ApiServerError("Failed to delete sale");
        }
    }

    #endregion

    #region Payments

    [HttpGet("{saleId}/payments")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetSalePayments(int saleId)
    {
        try
        {
            var sale = await _salesService.GetSaleByIdAsync(saleId);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");

            var payments = await _salesService.GetPaymentsBySaleIdAsync(saleId);
            return ApiOk(payments.Select(MapToPaymentDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for sale {SaleId}", saleId);
            return ApiServerError("Failed to retrieve payments");
        }
    }

    [HttpPost("payments")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RecordPayment([FromBody] RecordSalePaymentDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return ApiBadRequestFromModelState();

            var sale = await _salesService.GetSaleByIdAsync(dto.SaleId);
            if (sale == null) return ApiNotFound("Sale not found");
            if (!HasBranchAccess(sale.BranchId)) return ApiForbidden("Access denied");
            if (dto.Amount > sale.Balance) return ApiBadRequest($"Payment exceeds balance. Remaining: {sale.Balance:C}");

            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var payment = new Payment
            {
                BranchId = branchId.Value,
                PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                PaymentDate = DateTime.UtcNow,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                SaleId = dto.SaleId,
                ReferenceNumber = dto.ReferenceNumber,
                BankName = dto.BankName,
                CardLastFourDigits = dto.CardLastFourDigits,
                InsuranceCompany = dto.InsuranceCompany,
                InsuranceClaimNumber = dto.InsuranceClaimNumber,
                InsurancePolicyNumber = dto.InsurancePolicyNumber,
                InstallmentNumber = dto.InstallmentNumber,
                TotalInstallments = dto.TotalInstallments,
                Notes = dto.Notes,
                ReceivedBy = _currentUserService.UserId ?? string.Empty
            };

            var created = await _salesService.AddPaymentAsync(payment);
            _logger.LogInformation("Payment recorded for sale {InvoiceNumber}: {Amount}", sale.InvoiceNumber, dto.Amount);
            return ApiCreated(MapToPaymentDto(created), message: "Payment recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment");
            return ApiServerError("Failed to record payment");
        }
    }

    #endregion

    #region Quotations

    [HttpGet("quotations")]
    public async Task<ActionResult<ApiResponse<IEnumerable<QuotationDto>>>> GetQuotations([FromQuery] QuotationListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var quotations = await _salesService.GetQuotationsByBranchIdAsync(branchId.Value);
            var filtered = quotations.AsQueryable();

            if (request.PatientId.HasValue) filtered = filtered.Where(q => q.PatientId == request.PatientId.Value);
            if (request.Status.HasValue) filtered = filtered.Where(q => q.Status == request.Status.Value);
            if (request.DateFrom.HasValue) filtered = filtered.Where(q => q.QuotationDate >= request.DateFrom.Value);
            if (request.DateTo.HasValue) filtered = filtered.Where(q => q.QuotationDate <= request.DateTo.Value);
            if (request.ExpiredOnly == true) filtered = filtered.Where(q => q.IsExpired);

            var totalCount = filtered.Count();
            filtered = request.SortDescending
                ? filtered.OrderByDescending(q => q.QuotationDate)
                : filtered.OrderBy(q => q.QuotationDate);

            var paged = filtered.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
                .Select(MapToQuotationDto).ToList();

            return ApiPaginated(paged, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotations");
            return ApiServerError("Failed to retrieve quotations");
        }
    }

    [HttpGet("quotations/{id}")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> GetQuotation(int id)
    {
        try
        {
            var quotation = await _salesService.GetQuotationByIdAsync(id);
            if (quotation == null) return ApiNotFound("Quotation not found");
            if (!HasBranchAccess(quotation.BranchId)) return ApiForbidden("Access denied");
            return ApiOk(MapToQuotationDto(quotation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation {Id}", id);
            return ApiServerError("Failed to retrieve quotation");
        }
    }

    [HttpPost("quotations")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> CreateQuotation([FromBody] CreateQuotationDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return ApiBadRequestFromModelState();

            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var items = dto.Items.Select(i => new QuotationItem
            {
                InventoryItemId = i.InventoryItemId,
                ItemName = i.ItemName,
                ItemDescription = i.ItemDescription,
                ItemCode = i.ItemCode,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercentage = i.DiscountPercentage,
                TaxPercentage = i.TaxPercentage,
                Subtotal = i.Quantity * i.UnitPrice,
                Notes = i.Notes,
                CreatedBy = _currentUserService.UserId
            }).ToList();

            foreach (var item in items)
            {
                item.DiscountAmount = item.DiscountPercentage.HasValue ? item.Subtotal * (item.DiscountPercentage.Value / 100) : null;
                var afterDiscount = item.Subtotal - (item.DiscountAmount ?? 0);
                item.TaxAmount = item.TaxPercentage.HasValue ? afterDiscount * (item.TaxPercentage.Value / 100) : null;
                item.Total = afterDiscount + (item.TaxAmount ?? 0);
            }

            var subTotal = items.Sum(i => i.Subtotal);
            var discountAmount = dto.DiscountPercentage.HasValue ? subTotal * (dto.DiscountPercentage.Value / 100) : 0;
            var afterQuoteDiscount = subTotal - discountAmount;
            var taxAmount = dto.TaxPercentage.HasValue ? afterQuoteDiscount * (dto.TaxPercentage.Value / 100) : 0;

            var quotation = new Quotation
            {
                QuotationNumber = $"QUO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                QuotationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(dto.ValidityDays),
                Status = QuotationStatus.Draft,
                PatientId = dto.PatientId,
                BranchId = branchId.Value,
                SubTotal = subTotal,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = discountAmount,
                TaxPercentage = dto.TaxPercentage,
                TaxAmount = taxAmount,
                Total = afterQuoteDiscount + taxAmount,
                Notes = dto.Notes,
                Terms = dto.Terms,
                ValidityDays = dto.ValidityDays,
                Items = items,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var created = await _salesService.CreateQuotationAsync(quotation);
            _logger.LogInformation("Quotation created: {QuotationNumber}", quotation.QuotationNumber);
            return ApiCreated(MapToQuotationDto(created), message: "Quotation created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quotation");
            return ApiServerError("Failed to create quotation");
        }
    }

    [HttpPost("quotations/{id}/accept")]
    public async Task<ActionResult<ApiResponse>> AcceptQuotation(int id, [FromBody] AcceptQuotationDto dto)
    {
        try
        {
            if (id != dto.QuotationId) return ApiBadRequest("ID mismatch");
            var quotation = await _salesService.GetQuotationByIdAsync(id);
            if (quotation == null) return ApiNotFound("Quotation not found");
            if (!HasBranchAccess(quotation.BranchId)) return ApiForbidden("Access denied");
            if (quotation.Status != QuotationStatus.Sent) return ApiBadRequest("Only sent quotations can be accepted");

            quotation.Status = QuotationStatus.Accepted;
            quotation.AcceptedDate = DateTime.UtcNow;
            quotation.UpdatedAt = DateTime.UtcNow;
            quotation.UpdatedBy = _currentUserService.UserId;
            await _salesService.UpdateQuotationAsync(quotation);

            _logger.LogInformation("Quotation accepted: {QuotationNumber}", quotation.QuotationNumber);
            return ApiOk("Quotation accepted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting quotation {Id}", id);
            return ApiServerError("Failed to accept quotation");
        }
    }

    [HttpPost("quotations/{id}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectQuotation(int id, [FromBody] RejectQuotationDto dto)
    {
        try
        {
            if (id != dto.QuotationId) return ApiBadRequest("ID mismatch");
            var quotation = await _salesService.GetQuotationByIdAsync(id);
            if (quotation == null) return ApiNotFound("Quotation not found");
            if (!HasBranchAccess(quotation.BranchId)) return ApiForbidden("Access denied");

            quotation.Status = QuotationStatus.Rejected;
            quotation.RejectedDate = DateTime.UtcNow;
            quotation.RejectionReason = dto.RejectionReason;
            quotation.UpdatedAt = DateTime.UtcNow;
            quotation.UpdatedBy = _currentUserService.UserId;
            await _salesService.UpdateQuotationAsync(quotation);

            _logger.LogInformation("Quotation rejected: {QuotationNumber}", quotation.QuotationNumber);
            return ApiOk("Quotation rejected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting quotation {Id}", id);
            return ApiServerError("Failed to reject quotation");
        }
    }

    [HttpPost("quotations/{id}/convert")]
    public async Task<ActionResult<ApiResponse<SaleDto>>> ConvertQuotationToSale(int id)
    {
        try
        {
            var quotation = await _salesService.GetQuotationByIdAsync(id);
            if (quotation == null) return ApiNotFound("Quotation not found");
            if (!HasBranchAccess(quotation.BranchId)) return ApiForbidden("Access denied");
            if (!quotation.CanConvertToSale) return ApiBadRequest("Only accepted quotations can be converted to sales");

            var sale = await _salesService.ConvertQuotationToSaleAsync(id);
            _logger.LogInformation("Quotation converted to sale: {QuotationNumber} -> {InvoiceNumber}",
                quotation.QuotationNumber, sale.InvoiceNumber);
            return ApiCreated(MapToSaleDto(sale), message: "Quotation converted to sale successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting quotation {Id} to sale", id);
            return ApiServerError("Failed to convert quotation to sale");
        }
    }

    #endregion

    #region Statistics

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<SalesStatisticsDto>>> GetStatistics()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null) return ApiBadRequest("Branch context is required");

            var sales = (await _salesService.GetSalesByBranchIdAsync(branchId.Value)).ToList();
            var quotations = (await _salesService.GetQuotationsByBranchIdAsync(branchId.Value)).ToList();
            var statusDist = await _salesService.GetSalesStatusDistributionAsync(branchId.Value);
            var paymentDist = await _salesService.GetSalesPaymentStatusDistributionAsync(branchId.Value);
            var topItems = await _salesService.GetTopSellingItemsAsync(branchId.Value, 10);

            var totalSent = quotations.Count(q => q.Status != QuotationStatus.Draft);
            var totalAccepted = quotations.Count(q => q.Status == QuotationStatus.Accepted);

            var stats = new SalesStatisticsDto
            {
                TotalSales = sales.Count,
                DraftSales = sales.Count(s => s.Status == SaleStatus.Draft),
                ConfirmedSales = sales.Count(s => s.Status == SaleStatus.Confirmed),
                CompletedSales = sales.Count(s => s.Status == SaleStatus.Completed),
                CancelledSales = sales.Count(s => s.Status == SaleStatus.Cancelled),
                OverdueSales = sales.Count(s => s.IsOverdue),
                TotalRevenue = sales.Where(s => s.Status == SaleStatus.Completed).Sum(s => s.Total),
                PendingPayments = sales.Sum(s => s.Balance),
                AverageSaleValue = sales.Count > 0 ? sales.Average(s => s.Total) : 0,
                TotalQuotations = quotations.Count,
                PendingQuotations = quotations.Count(q => q.Status == QuotationStatus.Sent),
                AcceptedQuotations = totalAccepted,
                RejectedQuotations = quotations.Count(q => q.Status == QuotationStatus.Rejected),
                QuotationConversionRate = totalSent > 0 ? (decimal)totalAccepted / totalSent * 100 : 0,
                SalesByStatus = statusDist,
                SalesByPaymentStatus = paymentDist,
                QuotationsByStatus = quotations.GroupBy(q => q.Status).ToDictionary(g => g.Key, g => g.Count()),
                TopSellingItems = topItems.Select(t => new TopSellingItemDto
                {
                    ItemName = t.ItemName,
                    Quantity = t.Quantity,
                    Revenue = t.Revenue
                }).ToList()
            };

            return ApiOk(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales statistics");
            return ApiServerError("Failed to retrieve sales statistics");
        }
    }

    #endregion

    #region Helper Methods

    private bool HasBranchAccess(int branchId) =>
        _currentUserService.BranchId == branchId || _currentUserService.HasRole("SuperAdmin");

    private static SaleDto MapToSaleDto(Sale s) => new()
    {
        Id = s.Id, InvoiceNumber = s.InvoiceNumber, SaleDate = s.SaleDate, DueDate = s.DueDate,
        Status = s.Status, PaymentStatus = s.PaymentStatus, PatientId = s.PatientId,
        PatientName = s.Patient?.FullNameEn, PatientMRN = s.Patient?.MRN,
        BranchId = s.BranchId, BranchName = s.Branch?.Name,
        SubTotal = s.SubTotal, DiscountPercentage = s.DiscountPercentage, DiscountAmount = s.DiscountAmount,
        TaxPercentage = s.TaxPercentage, TaxAmount = s.TaxAmount, Total = s.Total, PaidAmount = s.PaidAmount,
        Notes = s.Notes, Terms = s.Terms, QuotationId = s.QuotationId, QuotationNumber = s.Quotation?.QuotationNumber,
        ItemCount = s.Items.Count,
        Items = s.Items.Select(i => new SaleItemDto
        {
            Id = i.Id, SaleId = i.SaleId, InventoryItemId = i.InventoryItemId, ItemName = i.ItemName,
            ItemDescription = i.ItemDescription, ItemCode = i.ItemCode, Quantity = i.Quantity,
            UnitPrice = i.UnitPrice, DiscountPercentage = i.DiscountPercentage, DiscountAmount = i.DiscountAmount,
            Subtotal = i.Subtotal, TaxPercentage = i.TaxPercentage, TaxAmount = i.TaxAmount, Total = i.Total,
            WarrantyStartDate = i.WarrantyStartDate, WarrantyEndDate = i.WarrantyEndDate,
            SerialNumber = i.SerialNumber, Notes = i.Notes
        }).ToList(),
        Payments = s.Payments.Select(MapToPaymentDto).ToList(),
        CreatedAt = s.CreatedAt, CreatedBy = s.CreatedBy, UpdatedAt = s.UpdatedAt, UpdatedBy = s.UpdatedBy
    };

    private static PaymentDto MapToPaymentDto(Payment p) => new()
    {
        Id = p.Id, BranchId = p.BranchId, PaymentNumber = p.PaymentNumber, PaymentDate = p.PaymentDate,
        Amount = p.Amount, PaymentMethod = p.PaymentMethod, SaleId = p.SaleId,
        SaleInvoiceNumber = p.Sale?.InvoiceNumber, ReferenceNumber = p.ReferenceNumber, BankName = p.BankName,
        CardLastFourDigits = p.CardLastFourDigits, InsuranceCompany = p.InsuranceCompany,
        InsuranceClaimNumber = p.InsuranceClaimNumber, InsurancePolicyNumber = p.InsurancePolicyNumber,
        InstallmentNumber = p.InstallmentNumber, TotalInstallments = p.TotalInstallments,
        Notes = p.Notes, ReceivedBy = p.ReceivedBy, CreatedAt = p.CreatedAt
    };

    private static QuotationDto MapToQuotationDto(Quotation q) => new()
    {
        Id = q.Id, QuotationNumber = q.QuotationNumber, QuotationDate = q.QuotationDate, ExpiryDate = q.ExpiryDate,
        Status = q.Status, PatientId = q.PatientId, PatientName = q.Patient?.FullNameEn, PatientMRN = q.Patient?.MRN,
        BranchId = q.BranchId, BranchName = q.Branch?.Name,
        SubTotal = q.SubTotal, DiscountPercentage = q.DiscountPercentage, DiscountAmount = q.DiscountAmount,
        TaxPercentage = q.TaxPercentage, TaxAmount = q.TaxAmount, Total = q.Total,
        Notes = q.Notes, Terms = q.Terms, ValidityDays = q.ValidityDays,
        AcceptedDate = q.AcceptedDate, RejectedDate = q.RejectedDate, RejectionReason = q.RejectionReason,
        ItemCount = q.Items.Count,
        Items = q.Items.Select(i => new QuotationItemDto
        {
            Id = i.Id, QuotationId = i.QuotationId, InventoryItemId = i.InventoryItemId,
            ItemName = i.ItemName, ItemDescription = i.ItemDescription, ItemCode = i.ItemCode,
            Quantity = i.Quantity, UnitPrice = i.UnitPrice, DiscountPercentage = i.DiscountPercentage,
            DiscountAmount = i.DiscountAmount, Subtotal = i.Subtotal, TaxPercentage = i.TaxPercentage,
            TaxAmount = i.TaxAmount, Total = i.Total, Notes = i.Notes
        }).ToList(),
        CreatedAt = q.CreatedAt, CreatedBy = q.CreatedBy, UpdatedAt = q.UpdatedAt, UpdatedBy = q.UpdatedBy
    };

    #endregion
}
