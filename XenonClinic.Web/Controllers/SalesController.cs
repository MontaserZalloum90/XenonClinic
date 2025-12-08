using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly ClinicDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        ClinicDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<SalesController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Sales Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        var branchId = user.PrimaryBranchId.Value;
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var viewModel = new SalesDashboardViewModel
        {
            TotalSalesToday = await _context.Sales
                .Where(s => s.BranchId == branchId && s.SaleDate.Date == today && s.Status == SaleStatus.Completed)
                .SumAsync(s => s.Total),

            TotalSalesThisMonth = await _context.Sales
                .Where(s => s.BranchId == branchId && s.SaleDate >= firstDayOfMonth && s.Status == SaleStatus.Completed)
                .SumAsync(s => s.Total),

            TotalInvoicesToday = await _context.Sales
                .CountAsync(s => s.BranchId == branchId && s.SaleDate.Date == today),

            TotalInvoicesThisMonth = await _context.Sales
                .CountAsync(s => s.BranchId == branchId && s.SaleDate >= firstDayOfMonth),

            OutstandingBalance = await _context.Sales
                .Where(s => s.BranchId == branchId && s.PaymentStatus != PaymentStatus.Paid && s.Status != SaleStatus.Cancelled)
                .SumAsync(s => s.Total - s.PaidAmount),

            OverdueInvoices = await _context.Sales
                .CountAsync(s => s.BranchId == branchId && s.DueDate.HasValue && s.DueDate.Value < today &&
                                s.PaidAmount < s.Total && s.Status != SaleStatus.Cancelled),

            PendingQuotations = await _context.Quotations
                .CountAsync(q => q.BranchId == branchId && q.Status == QuotationStatus.Sent),

            RecentSales = await _context.Sales
                .Where(s => s.BranchId == branchId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .Select(s => new SaleDto
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    SaleDate = s.SaleDate,
                    Status = s.Status,
                    PaymentStatus = s.PaymentStatus,
                    PatientName = s.Patient.FullNameEn,
                    Total = s.Total,
                    PaidAmount = s.PaidAmount,
                    Balance = s.Total - s.PaidAmount,
                    CreatedBy = s.CreatedBy,
                    IsFullyPaid = s.PaidAmount >= s.Total
                })
                .ToListAsync(),

            OverdueSales = await _context.Sales
                .Where(s => s.BranchId == branchId && s.DueDate.HasValue && s.DueDate.Value < today &&
                           s.PaidAmount < s.Total && s.Status != SaleStatus.Cancelled)
                .OrderBy(s => s.DueDate)
                .Take(10)
                .Select(s => new SaleDto
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    SaleDate = s.SaleDate,
                    DueDate = s.DueDate,
                    Status = s.Status,
                    PaymentStatus = s.PaymentStatus,
                    PatientName = s.Patient.FullNameEn,
                    Total = s.Total,
                    PaidAmount = s.PaidAmount,
                    Balance = s.Total - s.PaidAmount,
                    IsOverdue = true
                })
                .ToListAsync(),

            RecentQuotations = await _context.Quotations
                .Where(q => q.BranchId == branchId)
                .OrderByDescending(q => q.CreatedAt)
                .Take(10)
                .Select(q => new QuotationDto
                {
                    Id = q.Id,
                    QuotationNumber = q.QuotationNumber,
                    QuotationDate = q.QuotationDate,
                    ExpiryDate = q.ExpiryDate,
                    Status = q.Status,
                    PatientName = q.Patient.FullNameEn,
                    Total = q.Total,
                    CreatedBy = q.CreatedBy
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // GET: Sales/Invoices
    public async Task<IActionResult> Invoices(SaleStatus? status, PaymentStatus? paymentStatus, DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        var branchId = user.PrimaryBranchId.Value;

        var query = _context.Sales
            .Where(s => s.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (paymentStatus.HasValue)
            query = query.Where(s => s.PaymentStatus == paymentStatus.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.SaleDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.SaleDate <= toDate.Value);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SaleDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                SaleDate = s.SaleDate,
                DueDate = s.DueDate,
                Status = s.Status,
                PaymentStatus = s.PaymentStatus,
                PatientId = s.PatientId,
                PatientName = s.Patient.FullNameEn,
                Total = s.Total,
                PaidAmount = s.PaidAmount,
                Balance = s.Total - s.PaidAmount,
                BranchName = s.Branch.Name,
                CreatedBy = s.CreatedBy,
                CreatedAt = s.CreatedAt,
                IsFullyPaid = s.PaidAmount >= s.Total,
                IsOverdue = s.DueDate.HasValue && s.DueDate.Value < DateTime.UtcNow && s.PaidAmount < s.Total
            })
            .ToListAsync();

        return View(sales);
    }

    // GET: Sales/Create
    public async Task<IActionResult> Create(int? quotationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        await PopulateDropdowns(user.PrimaryBranchId.Value);

        var model = new CreateSaleViewModel();

        if (quotationId.HasValue)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == quotationId.Value && q.BranchId == user.PrimaryBranchId.Value);

            if (quotation != null)
            {
                model.QuotationId = quotation.Id;
                model.PatientId = quotation.PatientId;
                model.DiscountPercentage = quotation.DiscountPercentage;
                model.DiscountAmount = quotation.DiscountAmount;
                model.TaxPercentage = quotation.TaxPercentage;
                model.Notes = quotation.Notes;
                model.Terms = quotation.Terms;
                model.Items = quotation.Items.Select(i => new SaleItemViewModel
                {
                    InventoryItemId = i.InventoryItemId,
                    ItemName = i.ItemName,
                    ItemDescription = i.ItemDescription,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    DiscountAmount = i.DiscountAmount,
                    TaxPercentage = i.TaxPercentage
                }).ToList();
            }
        }

        return View(model);
    }

    // POST: Sales/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSaleViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(user.PrimaryBranchId.Value);
            return View(model);
        }

        try
        {
            var invoiceNumber = await GenerateInvoiceNumber(user.PrimaryBranchId.Value);

            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                SaleDate = model.SaleDate,
                DueDate = model.DueDate,
                Status = SaleStatus.Draft,
                PaymentStatus = PaymentStatus.Pending,
                PatientId = model.PatientId,
                BranchId = user.PrimaryBranchId.Value,
                QuotationId = model.QuotationId,
                DiscountPercentage = model.DiscountPercentage,
                DiscountAmount = model.DiscountAmount,
                TaxPercentage = model.TaxPercentage,
                Notes = model.Notes,
                Terms = model.Terms,
                CreatedBy = user.Email ?? "Unknown",
                CreatedAt = DateTime.UtcNow
            };

            decimal subtotal = 0;
            foreach (var itemModel in model.Items)
            {
                var itemSubtotal = itemModel.Quantity * itemModel.UnitPrice;
                var itemDiscount = itemModel.DiscountAmount ?? (itemSubtotal * (itemModel.DiscountPercentage ?? 0) / 100);
                var itemTaxableAmount = itemSubtotal - itemDiscount;
                var itemTax = itemTaxableAmount * (itemModel.TaxPercentage ?? 0) / 100;
                var itemTotal = itemTaxableAmount + itemTax;

                var saleItem = new SaleItem
                {
                    InventoryItemId = itemModel.InventoryItemId,
                    ItemName = itemModel.ItemName,
                    ItemDescription = itemModel.ItemDescription,
                    Quantity = itemModel.Quantity,
                    UnitPrice = itemModel.UnitPrice,
                    DiscountPercentage = itemModel.DiscountPercentage,
                    DiscountAmount = itemDiscount,
                    Subtotal = itemSubtotal - itemDiscount,
                    TaxPercentage = itemModel.TaxPercentage,
                    TaxAmount = itemTax,
                    Total = itemTotal,
                    SerialNumber = itemModel.SerialNumber,
                    WarrantyStartDate = itemModel.WarrantyStartDate,
                    WarrantyEndDate = itemModel.WarrantyEndDate,
                    Notes = itemModel.Notes
                };

                sale.Items.Add(saleItem);
                subtotal += itemSubtotal;
            }

            sale.SubTotal = subtotal;
            var saleDiscount = model.DiscountAmount ?? (subtotal * (model.DiscountPercentage ?? 0) / 100);
            var taxableAmount = subtotal - saleDiscount;
            var tax = taxableAmount * (model.TaxPercentage ?? 0) / 100;

            sale.DiscountAmount = saleDiscount;
            sale.TaxAmount = tax;
            sale.Total = taxableAmount + tax;
            sale.PaidAmount = 0;

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Sale {sale.InvoiceNumber} created by {user.Email}");
            return RedirectToAction(nameof(Details), new { id = sale.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale");
            ModelState.AddModelError("", "Error creating sale. Please try again.");
            await PopulateDropdowns(user.PrimaryBranchId.Value);
            return View(model);
        }
    }

    // GET: Sales/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        var sale = await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Branch)
            .Include(s => s.Items).ThenInclude(i => i.InventoryItem)
            .Include(s => s.Payments)
            .Include(s => s.Quotation)
            .FirstOrDefaultAsync(s => s.Id == id && s.BranchId == user.PrimaryBranchId.Value);

        if (sale == null)
            return NotFound();

        return View(sale);
    }

    // POST: Sales/RecordPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(CreatePaymentViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = model.SaleId });

        try
        {
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.Id == model.SaleId && s.BranchId == user.PrimaryBranchId);

            if (sale == null)
                return NotFound();

            var paymentNumber = await GeneratePaymentNumber(user.PrimaryBranchId!.Value);

            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                PaymentDate = model.PaymentDate,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                SaleId = model.SaleId,
                ReferenceNumber = model.ReferenceNumber,
                BankName = model.BankName,
                CardLastFourDigits = model.CardLastFourDigits,
                InsuranceCompany = model.InsuranceCompany,
                InsuranceClaimNumber = model.InsuranceClaimNumber,
                InsurancePolicyNumber = model.InsurancePolicyNumber,
                InstallmentNumber = model.InstallmentNumber,
                TotalInstallments = model.TotalInstallments,
                Notes = model.Notes,
                ReceivedBy = user.Email ?? "Unknown",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update sale paid amount and payment status
            sale.PaidAmount += model.Amount;
            sale.LastModifiedBy = user.Email;
            sale.LastModifiedAt = DateTime.UtcNow;

            if (sale.PaidAmount >= sale.Total)
                sale.PaymentStatus = PaymentStatus.Paid;
            else if (sale.PaidAmount > 0)
                sale.PaymentStatus = PaymentStatus.Partial;

            if (sale.Status == SaleStatus.Draft)
                sale.Status = SaleStatus.Confirmed;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment {payment.PaymentNumber} recorded for sale {sale.InvoiceNumber}");
            return RedirectToAction(nameof(Details), new { id = model.SaleId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment");
            return RedirectToAction(nameof(Details), new { id = model.SaleId });
        }
    }

    // GET: Sales/Quotations
    public async Task<IActionResult> Quotations(QuotationStatus? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PrimaryBranchId == null)
            return RedirectToAction("Index", "Home");

        var query = _context.Quotations
            .Where(q => q.BranchId == user.PrimaryBranchId.Value);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        var quotations = await query
            .OrderByDescending(q => q.QuotationDate)
            .Select(q => new QuotationDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                QuotationDate = q.QuotationDate,
                ExpiryDate = q.ExpiryDate,
                Status = q.Status,
                PatientId = q.PatientId,
                PatientName = q.Patient.FullNameEn,
                Total = q.Total,
                BranchName = q.Branch.Name,
                CreatedBy = q.CreatedBy,
                CreatedAt = q.CreatedAt,
                IsExpired = q.ExpiryDate.HasValue && q.ExpiryDate.Value < DateTime.UtcNow && q.Status == QuotationStatus.Sent,
                IsActive = q.Status == QuotationStatus.Sent && (!q.ExpiryDate.HasValue || q.ExpiryDate.Value >= DateTime.UtcNow)
            })
            .ToListAsync();

        return View(quotations);
    }

    // Helper Methods
    private async Task PopulateDropdowns(int branchId)
    {
        ViewBag.Patients = new SelectList(
            await _context.Patients
                .Where(p => p.BranchId == branchId)
                .OrderBy(p => p.FullNameEn)
                .ToListAsync(),
            "Id", "FullNameEn"
        );

        ViewBag.InventoryItems = await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive && i.QuantityOnHand > 0)
            .OrderBy(i => i.Name)
            .Select(i => new {
                i.Id,
                i.Name,
                i.ItemCode,
                i.SellingPrice,
                i.QuantityOnHand
            })
            .ToListAsync();
    }

    private async Task<string> GenerateInvoiceNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMM}-";

        var lastInvoice = await _context.Sales
            .Where(s => s.BranchId == branchId && s.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(s => s.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GeneratePaymentNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PAY-{today:yyyyMM}-";

        var lastPayment = await _context.Payments
            .Include(p => p.Sale)
            .Where(p => p.Sale.BranchId == branchId && p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPayment != null)
        {
            var lastNumberStr = lastPayment.PaymentNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
