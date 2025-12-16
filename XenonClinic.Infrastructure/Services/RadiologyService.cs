using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Radiology/Imaging management
/// Note: This service currently uses LabOrder/LabTest/LabResult entities as a foundation.
/// When specific RadiologyOrder/ImagingStudy/ImagingResult entities are created, this should be refactored.
/// </summary>
public class RadiologyService : IRadiologyService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public RadiologyService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
    }

    #region Radiology Order Management

    public async Task<LabOrder?> GetRadiologyOrderByIdAsync(int id)
    {
        return await _context.LabOrders
            .Include(o => o.Branch)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Include(o => o.Results)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<LabOrder?> GetRadiologyOrderByOrderNumberAsync(string orderNumber)
    {
        return await _context.LabOrders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Include(o => o.Results)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<LabOrder>> GetRadiologyOrdersByBranchIdAsync(int branchId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Where(o => o.BranchId == branchId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetRadiologyOrdersByPatientIdAsync(int patientId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(o => o.Branch)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Where(o => o.PatientId == patientId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetRadiologyOrdersByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Validate date range
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date", nameof(endDate));
        }

        return await _context.LabOrders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Where(o => o.BranchId == branchId &&
                   o.OrderDate >= startDate &&
                   o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetPendingRadiologyOrdersAsync(int branchId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Where(o => o.BranchId == branchId &&
                   (o.Status == LabOrderStatus.Pending || o.Status == LabOrderStatus.InProgress))
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetCompletedRadiologyOrdersAsync(int branchId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(oi => oi.LabTest)
            .Where(o => o.BranchId == branchId && o.Status == LabOrderStatus.Completed)
            .OrderByDescending(o => o.CompletedDate)
            .ToListAsync();
    }

    public async Task<LabOrder> CreateRadiologyOrderAsync(LabOrder order)
    {
        _context.LabOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateRadiologyOrderAsync(LabOrder order)
    {
        _context.LabOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRadiologyOrderAsync(int id)
    {
        var order = await _context.LabOrders.FindAsync(id);
        if (order != null)
        {
            _context.LabOrders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateRadiologyOrderNumberAsync(int branchId)
    {
        return await _sequenceGenerator.GenerateRadiologyOrderNumberAsync(branchId);
    }

    #endregion

    #region Imaging Study Management

    public async Task<LabTest?> GetImagingStudyByIdAsync(int id)
    {
        return await _context.LabTests
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<LabTest?> GetImagingStudyByCodeAsync(string studyCode, int branchId)
    {
        return await _context.LabTests
            .FirstOrDefaultAsync(t => t.TestCode == studyCode && t.BranchId == branchId);
    }

    public async Task<IEnumerable<LabTest>> GetImagingStudiesByBranchIdAsync(int branchId)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Where(t => t.BranchId == branchId)
            .OrderBy(t => t.TestName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetActiveImagingStudiesAsync(int branchId)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.IsActive)
            .OrderBy(t => t.TestName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetImagingStudiesByCategoryAsync(int branchId, string category)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.Category.ToString() == category)
            .OrderBy(t => t.TestName)
            .ToListAsync();
    }

    public async Task<LabTest> CreateImagingStudyAsync(LabTest study)
    {
        _context.LabTests.Add(study);
        await _context.SaveChangesAsync();
        return study;
    }

    public async Task UpdateImagingStudyAsync(LabTest study)
    {
        _context.LabTests.Update(study);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteImagingStudyAsync(int id)
    {
        var study = await _context.LabTests.FindAsync(id);
        if (study != null)
        {
            _context.LabTests.Remove(study);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Imaging Result Management

    public async Task<LabResult?> GetImagingResultByIdAsync(int id)
    {
        return await _context.LabResults
            .Include(r => r.LabOrder)
            .Include(r => r.LabTest)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<LabResult>> GetImagingResultsByOrderIdAsync(int orderId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .Include(r => r.LabTest)
            .Where(r => r.LabOrderId == orderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabResult>> GetImagingResultsByPatientIdAsync(int patientId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .Include(r => r.LabOrder)
            .Include(r => r.LabTest)
            .Where(r => r.LabOrder!.PatientId == patientId)
            .OrderByDescending(r => r.ResultDate)
            .ToListAsync();
    }

    public async Task<LabResult> CreateImagingResultAsync(LabResult result)
    {
        _context.LabResults.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task UpdateImagingResultAsync(LabResult result)
    {
        _context.LabResults.Update(result);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteImagingResultAsync(int id)
    {
        var result = await _context.LabResults.FindAsync(id);
        if (result != null)
        {
            _context.LabResults.Remove(result);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Order Status Management

    public async Task ReceiveRadiologyOrderAsync(int orderId, string receivedBy)
    {
        var order = await _context.LabOrders.FindAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Radiology order with ID {orderId} not found");

        // Validate status transition: can only receive from Pending
        if (order.Status != LabOrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot receive order in {order.Status} status. Only pending orders can be received.");
        }

        order.Status = LabOrderStatus.Received;
        order.ReceivedDate = DateTime.UtcNow;
        order.ReceivedBy = receivedBy;
        await _context.SaveChangesAsync();
    }

    public async Task StartImagingAsync(int orderId, string technician)
    {
        var order = await _context.LabOrders.FindAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Radiology order with ID {orderId} not found");

        // Validate status transition: can only start from Received or Pending
        if (order.Status != LabOrderStatus.Received && order.Status != LabOrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot start imaging for order in {order.Status} status. Only received or pending orders can be started.");
        }

        order.Status = LabOrderStatus.InProgress;
        order.PerformedDate = DateTime.UtcNow;
        order.PerformedBy = technician;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteRadiologyOrderAsync(int orderId, string completedBy)
    {
        var order = await _context.LabOrders.FindAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Radiology order with ID {orderId} not found");

        // Validate status transition: can only complete from InProgress
        if (order.Status != LabOrderStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot complete order in {order.Status} status. Only in-progress orders can be completed.");
        }

        order.Status = LabOrderStatus.Completed;
        order.CompletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ApproveRadiologyOrderAsync(int orderId, string approvedBy)
    {
        var order = await _context.LabOrders.FindAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Radiology order with ID {orderId} not found");

        // Validate status transition: can only approve from Completed
        if (order.Status != LabOrderStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Cannot approve order in {order.Status} status. Only completed orders can be approved.");
        }

        order.Status = LabOrderStatus.Approved;
        order.ApprovedDate = DateTime.UtcNow;
        order.ApprovedBy = approvedBy;
        await _context.SaveChangesAsync();
    }

    public async Task RejectRadiologyOrderAsync(int orderId, string rejectedBy, string reason)
    {
        var order = await _context.LabOrders.FindAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Radiology order with ID {orderId} not found");

        // Validate status transition: cannot reject already rejected/cancelled orders
        if (order.Status == LabOrderStatus.Rejected || order.Status == LabOrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot reject order in {order.Status} status. Order is already {order.Status}.");
        }

        order.Status = LabOrderStatus.Rejected;
        order.Notes = string.IsNullOrWhiteSpace(order.Notes)
            ? $"Rejected by {rejectedBy}: {reason}"
            : $"{order.Notes}\nRejected by {rejectedBy}: {reason}";
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalRadiologyOrdersCountAsync(int branchId)
    {
        return await _context.LabOrders
            .CountAsync(o => o.BranchId == branchId);
    }

    public async Task<int> GetPendingRadiologyOrdersCountAsync(int branchId)
    {
        return await _context.LabOrders
            .CountAsync(o => o.BranchId == branchId &&
                        (o.Status == LabOrderStatus.Pending || o.Status == LabOrderStatus.InProgress));
    }

    public async Task<int> GetCompletedRadiologyOrdersCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.LabOrders
            .CountAsync(o => o.BranchId == branchId &&
                        o.Status == LabOrderStatus.Completed &&
                        o.CompletedDate.HasValue &&
                        o.CompletedDate.Value >= startDate &&
                        o.CompletedDate.Value <= endDate);
    }

    public async Task<decimal> GetTotalRadiologyRevenueAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.LabOrders
            .Where(o => o.BranchId == branchId &&
                   o.Status == LabOrderStatus.Completed &&
                   o.CompletedDate.HasValue &&
                   o.CompletedDate.Value >= startDate &&
                   o.CompletedDate.Value <= endDate)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<IEnumerable<(string StudyName, int Count, decimal Revenue)>> GetMostOrderedStudiesAsync(int branchId, int topN = 10)
    {
        var mostOrdered = await _context.LabOrderItems
            .Include(oi => oi.LabOrder)
            .Include(oi => oi.LabTest)
            .Where(oi => oi.LabOrder.BranchId == branchId && oi.LabOrder.Status == LabOrderStatus.Completed)
            .GroupBy(oi => new { oi.LabTestId, oi.LabTest!.TestName })
            .Select(g => new
            {
                StudyName = g.Key.TestName,
                Count = g.Count(),
                Revenue = g.Sum(oi => oi.Price)
            })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToListAsync();

        return mostOrdered.Select(x => (x.StudyName, x.Count, x.Revenue));
    }

    public async Task<Dictionary<string, int>> GetOrdersByModalityDistributionAsync(int branchId)
    {
        // Using Category as Modality placeholder (X-Ray, CT, MRI, Ultrasound, etc.)
        var distribution = await _context.LabOrderItems
            .Include(oi => oi.LabOrder)
            .Include(oi => oi.LabTest)
            .Where(oi => oi.LabOrder.BranchId == branchId)
            .GroupBy(oi => oi.LabTest != null ? oi.LabTest.Category.ToString() : "Uncategorized")
            .Select(g => new { Modality = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Modality, x => x.Count);
    }

    #endregion
}
