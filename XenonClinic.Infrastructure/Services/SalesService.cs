using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Sales management including sales, quotations, and payments
/// </summary>
public class SalesService : ISalesService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public SalesService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
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
            .Include(s => s.Branch)
            .Include(s => s.Items)
            .Include(s => s.Quotation)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByPatientIdAsync(int patientId)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => s.PatientId == patientId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByStatusAsync(int branchId, SaleStatus status)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Where(s => s.BranchId == branchId && s.Status == status)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByPaymentStatusAsync(int branchId, PaymentStatus status)
    {
        return await _context.Sales
            .Include(s => s.Patient)
            .Where(s => s.BranchId == branchId && s.PaymentStatus == status)
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
        var now = DateTime.UtcNow;
        return await _context.Sales
            .Include(s => s.Patient)
            .Where(s => s.BranchId == branchId &&
                   s.DueDate.HasValue &&
                   s.DueDate.Value < now &&
                   s.PaidAmount < s.Total &&
                   s.Status != SaleStatus.Cancelled &&
                   s.Status != SaleStatus.Refunded)
            .OrderBy(s => s.DueDate)
            .ToListAsync();
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        // Validate patient exists
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == sale.PatientId);
        if (!patientExists)
        {
            throw new InvalidOperationException($"Patient with ID {sale.PatientId} not found");
        }

        // Generate invoice number if not provided
        if (string.IsNullOrEmpty(sale.InvoiceNumber))
        {
            sale.InvoiceNumber = await GenerateSaleInvoiceNumberAsync(sale.BranchId);
        }

        // Set default values
        sale.CreatedAt = DateTime.UtcNow;
        sale.Status = SaleStatus.Draft;
        sale.PaymentStatus = PaymentStatus.Pending;

        // Calculate totals
        CalculateSaleTotals(sale);

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task UpdateSaleAsync(Sale sale)
    {
        var existingSale = await _context.Sales.FindAsync(sale.Id);
        if (existingSale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {sale.Id} not found");
        }

        // Prevent updating completed or cancelled sales
        if (existingSale.Status == SaleStatus.Completed)
        {
            throw new InvalidOperationException("Cannot update a completed sale");
        }
        if (existingSale.Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot update a cancelled sale");
        }

        sale.UpdatedAt = DateTime.UtcNow;
        CalculateSaleTotals(sale);

        _context.Entry(existingSale).CurrentValues.SetValues(sale);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSaleAsync(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale != null)
        {
            // Prevent deleting sales with payments
            if (sale.Payments.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete a sale with payments. Cancel the sale instead.");
            }

            // Only allow deleting draft sales
            if (sale.Status != SaleStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only draft sales can be deleted. Cancel the sale instead.");
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Sale> ConfirmSaleAsync(int id)
    {
        var sale = await GetSaleByIdAsync(id);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {id} not found");
        }

        if (sale.Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException("Only draft sales can be confirmed");
        }

        if (!sale.Items.Any())
        {
            throw new InvalidOperationException("Cannot confirm a sale without items");
        }

        sale.Status = SaleStatus.Confirmed;
        sale.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<Sale> CompleteSaleAsync(int id)
    {
        var sale = await GetSaleByIdAsync(id);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {id} not found");
        }

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot complete a cancelled sale");
        }

        if (!sale.IsFullyPaid)
        {
            throw new InvalidOperationException("Cannot complete a sale that is not fully paid");
        }

        sale.Status = SaleStatus.Completed;
        sale.PaymentStatus = PaymentStatus.Paid;
        sale.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<Sale> CancelSaleAsync(int id, string? cancellationReason = null)
    {
        var sale = await GetSaleByIdAsync(id);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {id} not found");
        }

        if (sale.Status == SaleStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed sale. Use refund instead.");
        }

        if (sale.PaidAmount > 0)
        {
            throw new InvalidOperationException(
                "Cannot cancel a sale with payments. Process refunds first.");
        }

        sale.Status = SaleStatus.Cancelled;
        sale.PaymentStatus = PaymentStatus.Cancelled;
        sale.Notes = string.IsNullOrEmpty(cancellationReason)
            ? sale.Notes
            : $"{sale.Notes}\nCancellation reason: {cancellationReason}";
        sale.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<string> GenerateSaleInvoiceNumberAsync(int branchId)
    {
        return await _sequenceGenerator.GenerateSequenceAsync(
            branchId, "SALE", SequenceType.Sale, "yyyyMMdd", 4);
    }

    #endregion

    #region Sale Items

    public async Task<SaleItem?> GetSaleItemByIdAsync(int id)
    {
        return await _context.SaleItems
            .Include(i => i.Sale)
            .Include(i => i.InventoryItem)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<SaleItem>> GetSaleItemsBySaleIdAsync(int saleId)
    {
        return await _context.SaleItems
            .Include(i => i.InventoryItem)
            .Where(i => i.SaleId == saleId)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    public async Task<SaleItem> AddSaleItemAsync(SaleItem saleItem)
    {
        var sale = await _context.Sales.FindAsync(saleItem.SaleId);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {saleItem.SaleId} not found");
        }

        if (sale.Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException("Can only add items to draft sales");
        }

        // Calculate item totals
        CalculateSaleItemTotals(saleItem);

        saleItem.CreatedAt = DateTime.UtcNow;
        _context.SaleItems.Add(saleItem);
        await _context.SaveChangesAsync();

        // Recalculate sale totals
        await RecalculateSaleTotalsAsync(saleItem.SaleId);

        return saleItem;
    }

    public async Task UpdateSaleItemAsync(SaleItem saleItem)
    {
        var existingItem = await _context.SaleItems
            .Include(i => i.Sale)
            .FirstOrDefaultAsync(i => i.Id == saleItem.Id);

        if (existingItem == null)
        {
            throw new KeyNotFoundException($"Sale item with ID {saleItem.Id} not found");
        }

        if (existingItem.Sale.Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException("Can only update items in draft sales");
        }

        CalculateSaleItemTotals(saleItem);
        _context.Entry(existingItem).CurrentValues.SetValues(saleItem);
        await _context.SaveChangesAsync();

        // Recalculate sale totals
        await RecalculateSaleTotalsAsync(saleItem.SaleId);
    }

    public async Task DeleteSaleItemAsync(int id)
    {
        var saleItem = await _context.SaleItems
            .Include(i => i.Sale)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (saleItem != null)
        {
            if (saleItem.Sale.Status != SaleStatus.Draft)
            {
                throw new InvalidOperationException("Can only delete items from draft sales");
            }

            var saleId = saleItem.SaleId;
            _context.SaleItems.Remove(saleItem);
            await _context.SaveChangesAsync();

            // Recalculate sale totals
            await RecalculateSaleTotalsAsync(saleId);
        }
    }

    public async Task RecalculateSaleTotalsAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale != null)
        {
            CalculateSaleTotals(sale);
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

    public async Task<Payment?> GetPaymentByNumberAsync(string paymentNumber)
    {
        return await _context.Payments
            .Include(p => p.Sale)
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsBySaleIdAsync(int saleId)
    {
        return await _context.Payments
            .Where(p => p.SaleId == saleId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Include(p => p.Sale)
            .Where(p => p.Sale.BranchId == branchId &&
                   p.PaymentDate >= startDate &&
                   p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment> RecordPaymentAsync(Payment payment)
    {
        var sale = await _context.Sales.FindAsync(payment.SaleId);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {payment.SaleId} not found");
        }

        return await ProcessPaymentAsync(sale, payment);
    }

    public async Task<Payment> RecordPaymentAsync(int saleId, decimal amount, PaymentMethod method, string? referenceNumber = null)
    {
        var sale = await _context.Sales.FindAsync(saleId);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");
        }

        var payment = new Payment
        {
            SaleId = saleId,
            Amount = amount,
            PaymentMethod = method,
            ReferenceNumber = referenceNumber,
            PaymentDate = DateTime.UtcNow
        };

        return await ProcessPaymentAsync(sale, payment);
    }

    private async Task<Payment> ProcessPaymentAsync(Sale sale, Payment payment)
    {
        // Validate sale status
        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot record payment for a cancelled sale");
        }

        // Validate payment amount
        if (payment.Amount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero");
        }

        var remainingBalance = sale.Total - sale.PaidAmount;
        if (payment.Amount > remainingBalance)
        {
            throw new InvalidOperationException(
                $"Payment amount ({payment.Amount:C}) exceeds remaining balance ({remainingBalance:C})");
        }

        // Generate payment number if not provided
        if (string.IsNullOrEmpty(payment.PaymentNumber))
        {
            payment.PaymentNumber = await GeneratePaymentNumberAsync(sale.BranchId);
        }

        payment.CreatedAt = DateTime.UtcNow;
        _context.Payments.Add(payment);

        // Update sale paid amount and status
        sale.PaidAmount += payment.Amount;
        sale.UpdatedAt = DateTime.UtcNow;

        if (sale.PaidAmount >= sale.Total)
        {
            sale.PaymentStatus = PaymentStatus.Paid;
        }
        else if (sale.PaidAmount > 0)
        {
            sale.PaymentStatus = PaymentStatus.Partial;
        }

        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        var existingPayment = await _context.Payments.FindAsync(payment.Id);
        if (existingPayment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {payment.Id} not found");
        }

        // Recalculate sale totals if amount changed
        if (existingPayment.Amount != payment.Amount)
        {
            var sale = await _context.Sales.FindAsync(existingPayment.SaleId);
            if (sale != null)
            {
                sale.PaidAmount = sale.PaidAmount - existingPayment.Amount + payment.Amount;
                UpdateSalePaymentStatus(sale);
            }
        }

        _context.Entry(existingPayment).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePaymentAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Sale)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment != null)
        {
            var sale = payment.Sale;
            sale.PaidAmount -= payment.Amount;
            UpdateSalePaymentStatus(sale);
            sale.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RefundPaymentAsync(int paymentId, decimal refundAmount, string? reason = null)
    {
        var payment = await _context.Payments
            .Include(p => p.Sale)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {paymentId} not found");
        }

        if (refundAmount > payment.Amount)
        {
            throw new InvalidOperationException("Refund amount cannot exceed original payment amount");
        }

        var sale = payment.Sale;

        // Record refund as negative payment
        var refundPayment = new Payment
        {
            SaleId = sale.Id,
            PaymentNumber = await GeneratePaymentNumberAsync(sale.BranchId),
            Amount = -refundAmount,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            Notes = $"Refund for payment {payment.PaymentNumber}. {reason}",
            ReferenceNumber = payment.ReferenceNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(refundPayment);

        // Update sale
        sale.PaidAmount -= refundAmount;
        if (sale.PaidAmount <= 0)
        {
            sale.PaymentStatus = PaymentStatus.Refunded;
            sale.Status = SaleStatus.Refunded;
        }
        else
        {
            UpdateSalePaymentStatus(sale);
        }
        sale.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<string> GeneratePaymentNumberAsync(int branchId)
    {
        // Using a simple pattern: PAY-YYYYMMDD-XXXX
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.Payments
            .CountAsync(p => p.Sale.BranchId == branchId &&
                        p.PaymentNumber.StartsWith($"PAY-{today}"));

        return $"PAY-{today}-{(count + 1):D4}";
    }

    #endregion

    #region Quotation Management

    public async Task<Quotation?> GetQuotationByIdAsync(int id)
    {
        return await _context.Quotations
            .Include(q => q.Patient)
            .Include(q => q.Branch)
            .Include(q => q.Items)
            .Include(q => q.Sales)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quotation?> GetQuotationByNumberAsync(string quotationNumber)
    {
        return await _context.Quotations
            .Include(q => q.Patient)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber);
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
            .Include(q => q.Items)
            .Where(q => q.PatientId == patientId)
            .OrderByDescending(q => q.QuotationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(int branchId, QuotationStatus status)
    {
        return await _context.Quotations
            .Include(q => q.Patient)
            .Where(q => q.BranchId == branchId && q.Status == status)
            .OrderByDescending(q => q.QuotationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetActiveQuotationsAsync(int branchId)
    {
        var now = DateTime.UtcNow;
        return await _context.Quotations
            .Include(q => q.Patient)
            .Where(q => q.BranchId == branchId &&
                   q.Status == QuotationStatus.Sent &&
                   (!q.ExpiryDate.HasValue || q.ExpiryDate.Value >= now))
            .OrderByDescending(q => q.QuotationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetExpiredQuotationsAsync(int branchId)
    {
        var now = DateTime.UtcNow;
        return await _context.Quotations
            .Include(q => q.Patient)
            .Where(q => q.BranchId == branchId &&
                   q.ExpiryDate.HasValue &&
                   q.ExpiryDate.Value < now &&
                   (q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent))
            .OrderByDescending(q => q.ExpiryDate)
            .ToListAsync();
    }

    public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
    {
        // Validate patient exists
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == quotation.PatientId);
        if (!patientExists)
        {
            throw new InvalidOperationException($"Patient with ID {quotation.PatientId} not found");
        }

        // Generate quotation number if not provided
        if (string.IsNullOrEmpty(quotation.QuotationNumber))
        {
            quotation.QuotationNumber = await GenerateQuotationNumberAsync(quotation.BranchId);
        }

        // Set default values
        quotation.CreatedAt = DateTime.UtcNow;
        quotation.Status = QuotationStatus.Draft;

        // Set expiry date based on validity days
        if (!quotation.ExpiryDate.HasValue)
        {
            quotation.ExpiryDate = quotation.QuotationDate.AddDays(quotation.ValidityDays);
        }

        // Calculate totals
        CalculateQuotationTotals(quotation);

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task UpdateQuotationAsync(Quotation quotation)
    {
        var existingQuotation = await _context.Quotations.FindAsync(quotation.Id);
        if (existingQuotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {quotation.Id} not found");
        }

        // Prevent updating accepted or rejected quotations
        if (existingQuotation.Status == QuotationStatus.Accepted)
        {
            throw new InvalidOperationException("Cannot update an accepted quotation");
        }
        if (existingQuotation.Status == QuotationStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot update a rejected quotation");
        }

        quotation.UpdatedAt = DateTime.UtcNow;
        CalculateQuotationTotals(quotation);

        _context.Entry(existingQuotation).CurrentValues.SetValues(quotation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuotationAsync(int id)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Sales)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quotation != null)
        {
            // Prevent deleting quotations that have been converted to sales
            if (quotation.Sales.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete a quotation that has been converted to a sale");
            }

            // Only allow deleting draft quotations
            if (quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only draft quotations can be deleted");
            }

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Quotation> SendQuotationAsync(int id)
    {
        var quotation = await GetQuotationByIdAsync(id);
        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {id} not found");
        }

        if (quotation.Status != QuotationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft quotations can be sent");
        }

        if (!quotation.Items.Any())
        {
            throw new InvalidOperationException("Cannot send a quotation without items");
        }

        quotation.Status = QuotationStatus.Sent;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task<Quotation> AcceptQuotationAsync(int id)
    {
        var quotation = await GetQuotationByIdAsync(id);
        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {id} not found");
        }

        if (quotation.Status != QuotationStatus.Sent)
        {
            throw new InvalidOperationException("Only sent quotations can be accepted");
        }

        // Check if expired
        if (quotation.ExpiryDate.HasValue && quotation.ExpiryDate.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot accept an expired quotation");
        }

        quotation.Status = QuotationStatus.Accepted;
        quotation.AcceptedDate = DateTime.UtcNow;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task<Quotation> RejectQuotationAsync(int id, string? reason = null)
    {
        var quotation = await GetQuotationByIdAsync(id);
        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {id} not found");
        }

        if (quotation.Status != QuotationStatus.Sent)
        {
            throw new InvalidOperationException("Only sent quotations can be rejected");
        }

        quotation.Status = QuotationStatus.Rejected;
        quotation.RejectedDate = DateTime.UtcNow;
        quotation.RejectionReason = reason;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task<Sale> ConvertQuotationToSaleAsync(int quotationId)
    {
        var quotation = await GetQuotationByIdAsync(quotationId);
        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {quotationId} not found");
        }

        if (quotation.Status != QuotationStatus.Accepted)
        {
            throw new InvalidOperationException("Only accepted quotations can be converted to sales");
        }

        // Create sale from quotation
        var sale = new Sale
        {
            InvoiceNumber = await GenerateSaleInvoiceNumberAsync(quotation.BranchId),
            SaleDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // Default 30 days payment term
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
            PaidAmount = 0,
            Notes = $"Converted from quotation {quotation.QuotationNumber}",
            Terms = quotation.Terms,
            QuotationId = quotation.Id,
            CreatedBy = quotation.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Copy items
        foreach (var quotationItem in quotation.Items)
        {
            sale.Items.Add(new SaleItem
            {
                InventoryItemId = quotationItem.InventoryItemId,
                ItemName = quotationItem.ItemName,
                ItemDescription = quotationItem.ItemDescription,
                ItemCode = quotationItem.ItemCode,
                Quantity = quotationItem.Quantity,
                UnitPrice = quotationItem.UnitPrice,
                DiscountPercentage = quotationItem.DiscountPercentage,
                DiscountAmount = quotationItem.DiscountAmount,
                Subtotal = quotationItem.Subtotal,
                TaxPercentage = quotationItem.TaxPercentage,
                TaxAmount = quotationItem.TaxAmount,
                Total = quotationItem.Total,
                Notes = quotationItem.Notes,
                CreatedBy = quotation.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return sale;
    }

    public async Task<string> GenerateQuotationNumberAsync(int branchId)
    {
        return await _sequenceGenerator.GenerateSequenceAsync(
            branchId, "QT", SequenceType.Quotation, "yyyyMMdd", 4);
    }

    #endregion

    #region Quotation Items

    public async Task<QuotationItem?> GetQuotationItemByIdAsync(int id)
    {
        return await _context.QuotationItems
            .Include(i => i.Quotation)
            .Include(i => i.InventoryItem)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<QuotationItem>> GetQuotationItemsByQuotationIdAsync(int quotationId)
    {
        return await _context.QuotationItems
            .Include(i => i.InventoryItem)
            .Where(i => i.QuotationId == quotationId)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    public async Task<QuotationItem> AddQuotationItemAsync(QuotationItem quotationItem)
    {
        var quotation = await _context.Quotations.FindAsync(quotationItem.QuotationId);
        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with ID {quotationItem.QuotationId} not found");
        }

        if (quotation.Status != QuotationStatus.Draft)
        {
            throw new InvalidOperationException("Can only add items to draft quotations");
        }

        // Calculate item totals
        CalculateQuotationItemTotals(quotationItem);

        quotationItem.CreatedAt = DateTime.UtcNow;
        _context.QuotationItems.Add(quotationItem);
        await _context.SaveChangesAsync();

        // Recalculate quotation totals
        await RecalculateQuotationTotalsAsync(quotationItem.QuotationId);

        return quotationItem;
    }

    public async Task UpdateQuotationItemAsync(QuotationItem quotationItem)
    {
        var existingItem = await _context.QuotationItems
            .Include(i => i.Quotation)
            .FirstOrDefaultAsync(i => i.Id == quotationItem.Id);

        if (existingItem == null)
        {
            throw new KeyNotFoundException($"Quotation item with ID {quotationItem.Id} not found");
        }

        if (existingItem.Quotation.Status != QuotationStatus.Draft)
        {
            throw new InvalidOperationException("Can only update items in draft quotations");
        }

        CalculateQuotationItemTotals(quotationItem);
        _context.Entry(existingItem).CurrentValues.SetValues(quotationItem);
        await _context.SaveChangesAsync();

        // Recalculate quotation totals
        await RecalculateQuotationTotalsAsync(quotationItem.QuotationId);
    }

    public async Task DeleteQuotationItemAsync(int id)
    {
        var quotationItem = await _context.QuotationItems
            .Include(i => i.Quotation)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (quotationItem != null)
        {
            if (quotationItem.Quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidOperationException("Can only delete items from draft quotations");
            }

            var quotationId = quotationItem.QuotationId;
            _context.QuotationItems.Remove(quotationItem);
            await _context.SaveChangesAsync();

            // Recalculate quotation totals
            await RecalculateQuotationTotalsAsync(quotationId);
        }
    }

    public async Task RecalculateQuotationTotalsAsync(int quotationId)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == quotationId);

        if (quotation != null)
        {
            CalculateQuotationTotals(quotation);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Statistics & Reporting

    public async Task<decimal> GetTotalSalesAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.SaleDate >= startDate &&
                   s.SaleDate <= endDate &&
                   s.Status != SaleStatus.Cancelled)
            .SumAsync(s => s.Total);
    }

    public async Task<decimal> GetTotalPaidAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.SaleDate >= startDate &&
                   s.SaleDate <= endDate &&
                   s.Status != SaleStatus.Cancelled)
            .SumAsync(s => s.PaidAmount);
    }

    public async Task<decimal> GetTotalOutstandingAsync(int branchId)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.Status != SaleStatus.Cancelled &&
                   s.Status != SaleStatus.Refunded)
            .SumAsync(s => s.Total - s.PaidAmount);
    }

    public async Task<int> GetOverdueSalesCountAsync(int branchId)
    {
        var now = DateTime.UtcNow;
        return await _context.Sales
            .CountAsync(s => s.BranchId == branchId &&
                   s.DueDate.HasValue &&
                   s.DueDate.Value < now &&
                   s.PaidAmount < s.Total &&
                   s.Status != SaleStatus.Cancelled &&
                   s.Status != SaleStatus.Refunded);
    }

    public async Task<decimal> GetOverdueTotalAsync(int branchId)
    {
        var now = DateTime.UtcNow;
        return await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.DueDate.HasValue &&
                   s.DueDate.Value < now &&
                   s.PaidAmount < s.Total &&
                   s.Status != SaleStatus.Cancelled &&
                   s.Status != SaleStatus.Refunded)
            .SumAsync(s => s.Total - s.PaidAmount);
    }

    public async Task<int> GetPendingQuotationsCountAsync(int branchId)
    {
        return await _context.Quotations
            .CountAsync(q => q.BranchId == branchId &&
                   (q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent));
    }

    public async Task<decimal> GetQuotationConversionRateAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var totalQuotations = await _context.Quotations
            .CountAsync(q => q.BranchId == branchId &&
                   q.QuotationDate >= startDate &&
                   q.QuotationDate <= endDate);

        if (totalQuotations == 0) return 0;

        var acceptedQuotations = await _context.Quotations
            .CountAsync(q => q.BranchId == branchId &&
                   q.QuotationDate >= startDate &&
                   q.QuotationDate <= endDate &&
                   q.Status == QuotationStatus.Accepted);

        return (decimal)acceptedQuotations / totalQuotations * 100;
    }

    public async Task<SalesStatistics> GetSalesStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;
        var now = DateTime.UtcNow;

        var sales = await _context.Sales
            .Where(s => s.BranchId == branchId &&
                   s.SaleDate >= start &&
                   s.SaleDate <= end &&
                   s.Status != SaleStatus.Cancelled)
            .ToListAsync();

        var payments = await _context.Payments
            .Include(p => p.Sale)
            .Where(p => p.Sale.BranchId == branchId &&
                   p.PaymentDate >= start &&
                   p.PaymentDate <= end)
            .ToListAsync();

        var quotations = await _context.Quotations
            .Where(q => q.BranchId == branchId &&
                   q.QuotationDate >= start &&
                   q.QuotationDate <= end)
            .ToListAsync();

        var overdueSales = sales.Where(s =>
            s.DueDate.HasValue &&
            s.DueDate.Value < now &&
            s.PaidAmount < s.Total &&
            s.Status != SaleStatus.Refunded).ToList();

        var paymentMethodDistribution = payments
            .Where(p => p.Amount > 0)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        return new SalesStatistics
        {
            TotalSales = sales.Sum(s => s.Total),
            TotalPaid = sales.Sum(s => s.PaidAmount),
            TotalOutstanding = sales.Sum(s => s.Total - s.PaidAmount),
            SalesCount = sales.Count,
            PendingSalesCount = sales.Count(s => s.Status == SaleStatus.Draft || s.Status == SaleStatus.Confirmed),
            CompletedSalesCount = sales.Count(s => s.Status == SaleStatus.Completed),
            OverdueSalesCount = overdueSales.Count,
            OverdueTotal = overdueSales.Sum(s => s.Total - s.PaidAmount),
            PendingQuotationsCount = quotations.Count(q => q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent),
            AcceptedQuotationsCount = quotations.Count(q => q.Status == QuotationStatus.Accepted),
            QuotationConversionRate = quotations.Count > 0
                ? (decimal)quotations.Count(q => q.Status == QuotationStatus.Accepted) / quotations.Count * 100
                : 0,
            AverageTransactionValue = sales.Count > 0 ? sales.Average(s => s.Total) : 0,
            PaymentMethodDistribution = paymentMethodDistribution
        };
    }

    #endregion

    #region Helper Methods

    private void CalculateSaleTotals(Sale sale)
    {
        if (sale.Items.Any())
        {
            sale.SubTotal = sale.Items.Sum(i => i.Subtotal);
        }

        // Calculate discount
        var discountAmount = sale.DiscountAmount ?? 0;
        if (sale.DiscountPercentage.HasValue && sale.DiscountPercentage > 0)
        {
            discountAmount = sale.SubTotal * (sale.DiscountPercentage.Value / 100);
        }
        sale.DiscountAmount = discountAmount;

        var afterDiscount = sale.SubTotal - discountAmount;

        // Calculate tax
        var taxAmount = sale.TaxAmount ?? 0;
        if (sale.TaxPercentage.HasValue && sale.TaxPercentage > 0)
        {
            taxAmount = afterDiscount * (sale.TaxPercentage.Value / 100);
        }
        sale.TaxAmount = taxAmount;

        sale.Total = afterDiscount + taxAmount;
        UpdateSalePaymentStatus(sale);
    }

    private void CalculateSaleItemTotals(SaleItem item)
    {
        var lineTotal = item.Quantity * item.UnitPrice;

        // Calculate discount
        var discountAmount = item.DiscountAmount ?? 0;
        if (item.DiscountPercentage.HasValue && item.DiscountPercentage > 0)
        {
            discountAmount = lineTotal * (item.DiscountPercentage.Value / 100);
        }
        item.DiscountAmount = discountAmount;
        item.Subtotal = lineTotal - discountAmount;

        // Calculate tax
        var taxAmount = item.TaxAmount ?? 0;
        if (item.TaxPercentage.HasValue && item.TaxPercentage > 0)
        {
            taxAmount = item.Subtotal * (item.TaxPercentage.Value / 100);
        }
        item.TaxAmount = taxAmount;

        item.Total = item.Subtotal + taxAmount;
    }

    private void CalculateQuotationTotals(Quotation quotation)
    {
        if (quotation.Items.Any())
        {
            quotation.SubTotal = quotation.Items.Sum(i => i.Subtotal);
        }

        // Calculate discount
        var discountAmount = quotation.DiscountAmount ?? 0;
        if (quotation.DiscountPercentage.HasValue && quotation.DiscountPercentage > 0)
        {
            discountAmount = quotation.SubTotal * (quotation.DiscountPercentage.Value / 100);
        }
        quotation.DiscountAmount = discountAmount;

        var afterDiscount = quotation.SubTotal - discountAmount;

        // Calculate tax
        var taxAmount = quotation.TaxAmount ?? 0;
        if (quotation.TaxPercentage.HasValue && quotation.TaxPercentage > 0)
        {
            taxAmount = afterDiscount * (quotation.TaxPercentage.Value / 100);
        }
        quotation.TaxAmount = taxAmount;

        quotation.Total = afterDiscount + taxAmount;
    }

    private void CalculateQuotationItemTotals(QuotationItem item)
    {
        var lineTotal = item.Quantity * item.UnitPrice;

        // Calculate discount
        var discountAmount = item.DiscountAmount ?? 0;
        if (item.DiscountPercentage.HasValue && item.DiscountPercentage > 0)
        {
            discountAmount = lineTotal * (item.DiscountPercentage.Value / 100);
        }
        item.DiscountAmount = discountAmount;
        item.Subtotal = lineTotal - discountAmount;

        // Calculate tax
        var taxAmount = item.TaxAmount ?? 0;
        if (item.TaxPercentage.HasValue && item.TaxPercentage > 0)
        {
            taxAmount = item.Subtotal * (item.TaxPercentage.Value / 100);
        }
        item.TaxAmount = taxAmount;

        item.Total = item.Subtotal + taxAmount;
    }

    private void UpdateSalePaymentStatus(Sale sale)
    {
        if (sale.PaidAmount >= sale.Total && sale.Total > 0)
        {
            sale.PaymentStatus = PaymentStatus.Paid;
        }
        else if (sale.PaidAmount > 0)
        {
            sale.PaymentStatus = PaymentStatus.Partial;
        }
        else if (sale.DueDate.HasValue && sale.DueDate.Value < DateTime.UtcNow)
        {
            sale.PaymentStatus = PaymentStatus.Overdue;
        }
        else
        {
            sale.PaymentStatus = PaymentStatus.Pending;
        }
    }

    #endregion
}
