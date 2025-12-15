using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Pharmacy/Sales management
/// </summary>
public class PharmacyService : IPharmacyService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public PharmacyService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
    }

    #region Sale Management

    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Branch)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Quotation)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale?> GetSaleByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Sale>> GetSalesByBranchIdAsync(int branchId)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByPatientIdAsync(int patientId)
    {
        return await _context.Sales
            .Include(s => s.Branch)
            .Include(s => s.Items)
            .Where(s => s.PatientId == patientId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByStatusAsync(int branchId, SaleStatus status)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId && s.Status == status)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByPaymentStatusAsync(int branchId, PaymentStatus paymentStatus)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId && s.PaymentStatus == paymentStatus)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId &&
                   s.SaleDate >= startDate &&
                   s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetOverdueSalesAsync(int branchId)
    {
        var today = DateTime.UtcNow;
        return await _context.Sales
            .Include(s => s.Patient)
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId &&
                   s.DueDate.HasValue &&
                   s.DueDate.Value < today &&
                   s.PaymentStatus != PaymentStatus.Paid)
            .OrderBy(s => s.DueDate)
            .ToListAsync();
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task UpdateSaleAsync(Sale sale)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSaleAsync(int id)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale != null)
        {
            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateInvoiceNumberAsync(int branchId)
    {
        // Use centralized sequence generator to avoid race conditions
        return await _sequenceGenerator.GenerateSequenceAsync(
            branchId, "INV", SequenceType.Sale, "yyyyMMdd", 4);
    }

    #endregion

    #region Sale Status Management

    public async Task ConfirmSaleAsync(int saleId)
    {
        var sale = await _context.Sales.FindAsync(saleId);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");

        sale.Status = SaleStatus.Confirmed;
        sale.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteSaleAsync(int saleId)
    {
        var sale = await _context.Sales.FindAsync(saleId);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");

        sale.Status = SaleStatus.Completed;
        sale.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CancelSaleAsync(int saleId)
    {
        var sale = await _context.Sales.FindAsync(saleId);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");

        sale.Status = SaleStatus.Cancelled;
        sale.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Sale Item Management

    public async Task<SaleItem?> GetSaleItemByIdAsync(int id)
    {
        return await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.InventoryItem)
            .FirstOrDefaultAsync(si => si.Id == id);
    }

    public async Task<IEnumerable<SaleItem>> GetSaleItemsAsync(int saleId)
    {
        return await _context.SaleItems
            .Include(si => si.InventoryItem)
            .Where(si => si.SaleId == saleId)
            .ToListAsync();
    }

    public async Task<SaleItem> AddSaleItemAsync(SaleItem saleItem)
    {
        _context.SaleItems.Add(saleItem);
        await _context.SaveChangesAsync();

        // Recalculate sale totals
        await RecalculateSaleTotalsAsync(saleItem.SaleId);

        return saleItem;
    }

    public async Task UpdateSaleItemAsync(SaleItem saleItem)
    {
        _context.SaleItems.Update(saleItem);
        await _context.SaveChangesAsync();

        // Recalculate sale totals
        await RecalculateSaleTotalsAsync(saleItem.SaleId);
    }

    public async Task DeleteSaleItemAsync(int id)
    {
        var saleItem = await _context.SaleItems.FindAsync(id);
        if (saleItem != null)
        {
            var saleId = saleItem.SaleId;
            _context.SaleItems.Remove(saleItem);
            await _context.SaveChangesAsync();

            // Recalculate sale totals
            await RecalculateSaleTotalsAsync(saleId);
        }
    }

    private async Task RecalculateSaleTotalsAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale != null)
        {
            sale.SubTotal = sale.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;

            // Calculate discount
            if (sale.DiscountPercentage.HasValue && sale.DiscountPercentage.Value > 0)
            {
                sale.DiscountAmount = sale.SubTotal * (sale.DiscountPercentage.Value / 100);
            }

            var amountAfterDiscount = sale.SubTotal - (sale.DiscountAmount ?? 0);

            // Calculate tax
            if (sale.TaxPercentage.HasValue && sale.TaxPercentage.Value > 0)
            {
                sale.TaxAmount = amountAfterDiscount * (sale.TaxPercentage.Value / 100);
            }

            sale.Total = amountAfterDiscount + (sale.TaxAmount ?? 0);

            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Payment Management

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Sale)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsBySaleIdAsync(int saleId)
    {
        return await _context.Payments
            .Where(p => p.SaleId == saleId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Update sale payment status
        await UpdateSalePaymentStatusAsync(payment.SaleId);

        return payment;
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

        // Update sale payment status
        await UpdateSalePaymentStatusAsync(payment.SaleId);
    }

    public async Task DeletePaymentAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment != null)
        {
            var saleId = payment.SaleId;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            // Update sale payment status
            await UpdateSalePaymentStatusAsync(saleId);
        }
    }

    private async Task UpdateSalePaymentStatusAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale != null)
        {
            sale.PaidAmount = sale.Payments?.Sum(p => p.Amount) ?? 0;

            if (sale.PaidAmount >= sale.Total)
            {
                sale.PaymentStatus = PaymentStatus.Paid;
            }
            else if (sale.PaidAmount > 0)
            {
                sale.PaymentStatus = PaymentStatus.PartiallyPaid;
            }
            else
            {
                sale.PaymentStatus = PaymentStatus.Pending;
            }

            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Quotation Management

    public async Task<Quotation?> GetQuotationByIdAsync(int id)
    {
        return await _context.Quotations
            .Include(q => q.Patient)
            .Include(q => q.Branch)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByBranchIdAsync(int branchId)
    {
        return await _context.Quotations
            .Include(q => q.Patient)
            .Include(q => q.Items)
            .Where(q => q.BranchId == branchId)
            .OrderByDescending(q => q.QuotationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByPatientIdAsync(int patientId)
    {
        return await _context.Quotations
            .Include(q => q.Branch)
            .Include(q => q.Items)
            .Where(q => q.PatientId == patientId)
            .OrderByDescending(q => q.QuotationDate)
            .ToListAsync();
    }

    public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
    {
        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task UpdateQuotationAsync(Quotation quotation)
    {
        _context.Quotations.Update(quotation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuotationAsync(int id)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation != null)
        {
            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Sale> ConvertQuotationToSaleAsync(int quotationId)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == quotationId);

        if (quotation == null)
            throw new KeyNotFoundException($"Quotation with ID {quotationId} not found");

        var sale = new Sale
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(quotation.BranchId),
            SaleDate = DateTime.UtcNow,
            Status = SaleStatus.Draft,
            PaymentStatus = PaymentStatus.Pending,
            PatientId = quotation.PatientId,
            BranchId = quotation.BranchId,
            SubTotal = quotation.SubTotal,
            DiscountPercentage = quotation.DiscountPercentage,
            DiscountAmount = quotation.DiscountAmount,
            TaxPercentage = quotation.TaxPercentage,
            TaxAmount = quotation.TaxAmount,
            Total = quotation.Total,
            QuotationId = quotationId,
            CreatedBy = quotation.CreatedBy,
            Notes = quotation.Notes,
            Terms = quotation.Terms
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Copy quotation items to sale items
        foreach (var quotationItem in quotation.Items)
        {
            var saleItem = new SaleItem
            {
                SaleId = sale.Id,
                InventoryItemId = quotationItem.InventoryItemId,
                Description = quotationItem.ItemDescription,
                Quantity = quotationItem.Quantity,
                UnitPrice = quotationItem.UnitPrice,
                DiscountPercentage = quotationItem.DiscountPercentage
            };
            _context.SaleItems.Add(saleItem);
        }

        await _context.SaveChangesAsync();
        return sale;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalSalesCountAsync(int branchId)
    {
        return await _context.Sales
            .CountAsync(s => s.BranchId == branchId);
    }

    public async Task<decimal> GetTotalSalesRevenueAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.Status == SaleStatus.Completed &&
                   s.SaleDate >= startDate &&
                   s.SaleDate <= endDate)
            .SumAsync(s => s.Total);
    }

    public async Task<decimal> GetTotalOutstandingBalanceAsync(int branchId)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.PaymentStatus != PaymentStatus.Paid)
            .SumAsync(s => s.Total - s.PaidAmount);
    }

    public async Task<int> GetOverdueSalesCountAsync(int branchId)
    {
        var today = DateTime.UtcNow;
        return await _context.Sales
            .CountAsync(s => s.BranchId == branchId &&
                        s.DueDate.HasValue &&
                        s.DueDate.Value < today &&
                        s.PaymentStatus != PaymentStatus.Paid);
    }

    public async Task<Dictionary<SaleStatus, int>> GetSalesStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Sales
            .Where(s => s.BranchId == branchId)
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<PaymentStatus, int>> GetSalesPaymentStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Sales
            .Where(s => s.BranchId == branchId)
            .GroupBy(s => s.PaymentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<IEnumerable<(string ItemName, int Quantity, decimal Revenue)>> GetTopSellingItemsAsync(int branchId, int topN = 10)
    {
        var topItems = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.InventoryItem)
            .Where(si => si.Sale.BranchId == branchId && si.Sale.Status == SaleStatus.Completed)
            .GroupBy(si => new { si.InventoryItemId, si.InventoryItem!.ItemName })
            .Select(g => new
            {
                ItemName = g.Key.ItemName,
                Quantity = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.Quantity * si.UnitPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(topN)
            .ToListAsync();

        return topItems.Select(x => (x.ItemName, x.Quantity, x.Revenue));
    }

    #endregion
}
