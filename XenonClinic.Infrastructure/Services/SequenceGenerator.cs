using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Centralized service for generating sequential numbers/codes.
/// Replaces duplicated sequence generation logic across multiple services.
/// </summary>
public class SequenceGenerator : ISequenceGenerator
{
    private readonly ClinicDbContext _context;

    public SequenceGenerator(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateSequenceAsync(
        int branchId,
        string prefix,
        SequenceType sequenceType,
        string dateFormat = "yyyyMMdd",
        int numberWidth = 4)
    {
        var today = DateTime.UtcNow.Date;
        var fullPrefix = $"{prefix}-{today.ToString(dateFormat)}";

        var lastNumber = await GetLastSequenceNumberAsync(branchId, fullPrefix, sequenceType);
        var nextNumber = lastNumber + 1;

        return $"{fullPrefix}-{nextNumber.ToString($"D{numberWidth}")}";
    }

    public async Task<string> GenerateLabOrderNumberAsync(int branchId)
    {
        return await GenerateSequenceAsync(branchId, "LAB", SequenceType.LabOrder);
    }

    public async Task<string> GenerateRadiologyOrderNumberAsync(int branchId)
    {
        return await GenerateSequenceAsync(branchId, "RAD", SequenceType.RadiologyOrder);
    }

    public async Task<string> GenerateInvoiceNumberAsync(int branchId)
    {
        return await GenerateSequenceAsync(branchId, "INV", SequenceType.Invoice);
    }

    public async Task<string> GenerateEmployeeCodeAsync(int branchId)
    {
        // Employee codes use monthly format and 3-digit numbers
        return await GenerateSequenceAsync(branchId, "EMP", SequenceType.Employee, "yyyyMM", 3);
    }

    public async Task<string> GenerateAppointmentNumberAsync(int branchId)
    {
        return await GenerateSequenceAsync(branchId, "APT", SequenceType.Appointment);
    }

    public async Task<string> GeneratePatientMRNAsync(int branchId)
    {
        return await GenerateSequenceAsync(branchId, "MRN", SequenceType.Patient);
    }

    private async Task<int> GetLastSequenceNumberAsync(
        int branchId,
        string prefix,
        SequenceType sequenceType)
    {
        string? lastSequence = sequenceType switch
        {
            SequenceType.LabOrder => await GetLastLabOrderNumberAsync(branchId, prefix),
            SequenceType.RadiologyOrder => await GetLastRadiologyOrderNumberAsync(branchId, prefix),
            SequenceType.Invoice => await GetLastInvoiceNumberAsync(branchId, prefix),
            SequenceType.Sale => await GetLastSaleNumberAsync(branchId, prefix),
            SequenceType.Employee => await GetLastEmployeeCodeAsync(branchId, prefix),
            SequenceType.Appointment => await GetLastAppointmentNumberAsync(branchId, prefix),
            SequenceType.Patient => await GetLastPatientMRNAsync(branchId, prefix),
            SequenceType.Prescription => await GetLastPrescriptionNumberAsync(branchId, prefix),
            SequenceType.Quotation => await GetLastQuotationNumberAsync(branchId, prefix),
            _ => null
        };

        if (string.IsNullOrEmpty(lastSequence))
        {
            return 0;
        }

        // Extract the number from the end of the sequence
        var parts = lastSequence.Split('-');
        if (parts.Length > 0 && int.TryParse(parts[^1], out var number))
        {
            return number;
        }

        return 0;
    }

    private async Task<string?> GetLastLabOrderNumberAsync(int branchId, string prefix)
    {
        return await _context.LabOrders
            .Where(lo => lo.BranchId == branchId && lo.OrderNumber.StartsWith(prefix))
            .OrderByDescending(lo => lo.OrderNumber)
            .Select(lo => lo.OrderNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastRadiologyOrderNumberAsync(int branchId, string prefix)
    {
        // Note: Using LabOrders as per existing RadiologyService pattern
        // This should ideally be RadiologyOrders if that table exists
        return await _context.LabOrders
            .Where(o => o.BranchId == branchId && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastInvoiceNumberAsync(int branchId, string prefix)
    {
        return await _context.Invoices
            .Where(i => i.BranchId == branchId && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastSaleNumberAsync(int branchId, string prefix)
    {
        return await _context.Sales
            .Where(s => s.BranchId == branchId && s.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(s => s.InvoiceNumber)
            .Select(s => s.InvoiceNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastEmployeeCodeAsync(int branchId, string prefix)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId && e.EmployeeCode.StartsWith(prefix))
            .OrderByDescending(e => e.EmployeeCode)
            .Select(e => e.EmployeeCode)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastAppointmentNumberAsync(int branchId, string prefix)
    {
        return await _context.Appointments
            .Where(a => a.BranchId == branchId && a.ReferenceNumber != null && a.ReferenceNumber.StartsWith(prefix))
            .OrderByDescending(a => a.ReferenceNumber)
            .Select(a => a.ReferenceNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastPatientMRNAsync(int branchId, string prefix)
    {
        return await _context.Patients
            .Where(p => p.BranchId == branchId && p.MRN != null && p.MRN.StartsWith(prefix))
            .OrderByDescending(p => p.MRN)
            .Select(p => p.MRN)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastPrescriptionNumberAsync(int branchId, string prefix)
    {
        return await _context.Prescriptions
            .Where(p => p.BranchId == branchId && p.PrescriptionNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PrescriptionNumber)
            .Select(p => p.PrescriptionNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetLastQuotationNumberAsync(int branchId, string prefix)
    {
        return await _context.Quotations
            .Where(q => q.BranchId == branchId && q.QuotationNumber.StartsWith(prefix))
            .OrderByDescending(q => q.QuotationNumber)
            .Select(q => q.QuotationNumber)
            .FirstOrDefaultAsync();
    }
}
