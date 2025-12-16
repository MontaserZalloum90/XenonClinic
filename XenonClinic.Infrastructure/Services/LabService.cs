using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Laboratory management
/// </summary>
public class LabService : ILabService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public LabService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
    }

    #region Lab Order Management

    public async Task<LabOrder?> GetLabOrderByIdAsync(int id)
    {
        return await _context.LabOrders
            .Include(lo => lo.Patient)
            .Include(lo => lo.Branch)
            .Include(lo => lo.ExternalLab)
            .Include(lo => lo.Items)
                .ThenInclude(i => i.LabTest)
            .Include(lo => lo.Results)
            .FirstOrDefaultAsync(lo => lo.Id == id);
    }

    public async Task<LabOrder?> GetLabOrderByNumberAsync(string orderNumber)
    {
        return await _context.LabOrders
            .Include(lo => lo.Patient)
            .Include(lo => lo.Branch)
            .Include(lo => lo.Items)
                .ThenInclude(i => i.LabTest)
            .FirstOrDefaultAsync(lo => lo.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByBranchIdAsync(int branchId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(lo => lo.Patient)
            .Include(lo => lo.ExternalLab)
            .Include(lo => lo.Items)
            .Where(lo => lo.BranchId == branchId)
            .OrderByDescending(lo => lo.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByPatientIdAsync(int patientId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(lo => lo.Branch)
            .Include(lo => lo.Items)
                .ThenInclude(i => i.LabTest)
            .Include(lo => lo.Results)
            .Where(lo => lo.PatientId == patientId)
            .OrderByDescending(lo => lo.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByStatusAsync(int branchId, LabOrderStatus status)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(lo => lo.Patient)
            .Include(lo => lo.Items)
            .Where(lo => lo.BranchId == branchId && lo.Status == status)
            .OrderBy(lo => lo.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync(int branchId)
    {
        return await GetLabOrdersByStatusAsync(branchId, LabOrderStatus.Pending);
    }

    public async Task<IEnumerable<LabOrder>> GetUrgentLabOrdersAsync(int branchId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Include(lo => lo.Patient)
            .Include(lo => lo.Items)
            .Where(lo => lo.BranchId == branchId && lo.IsUrgent && lo.Status != LabOrderStatus.Completed && lo.Status != LabOrderStatus.Cancelled)
            .OrderBy(lo => lo.OrderDate)
            .ToListAsync();
    }

    public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
    {
        ArgumentNullException.ThrowIfNull(labOrder);

        // Validate patient exists
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == labOrder.PatientId && !p.IsDeleted);
        if (!patientExists)
        {
            throw new KeyNotFoundException($"Patient with ID {labOrder.PatientId} not found");
        }

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == labOrder.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {labOrder.BranchId} not found");
        }

        _context.LabOrders.Add(labOrder);
        await _context.SaveChangesAsync();
        return labOrder;
    }

    public async Task UpdateLabOrderAsync(LabOrder labOrder)
    {
        ArgumentNullException.ThrowIfNull(labOrder);

        // Validate lab order exists
        var existingOrder = await _context.LabOrders.FindAsync(labOrder.Id);
        if (existingOrder == null)
        {
            throw new KeyNotFoundException($"Lab order with ID {labOrder.Id} not found");
        }

        labOrder.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingOrder).CurrentValues.SetValues(labOrder);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLabOrderStatusAsync(int labOrderId, LabOrderStatus status, string userId)
    {
        var labOrder = await _context.LabOrders.FindAsync(labOrderId);
        if (labOrder == null)
            throw new KeyNotFoundException($"Lab order with ID {labOrderId} not found");

        // Validate status transition
        ValidateStatusTransition(labOrder.Status, status);

        labOrder.Status = status;
        labOrder.UpdatedBy = userId;
        labOrder.UpdatedAt = DateTime.UtcNow;

        // Set completion/receive/performed dates based on new status
        switch (status)
        {
            case LabOrderStatus.Received:
                labOrder.ReceivedDate = DateTime.UtcNow;
                labOrder.ReceivedBy = userId;
                break;
            case LabOrderStatus.InProgress:
                labOrder.PerformedDate = DateTime.UtcNow;
                labOrder.PerformedBy = userId;
                break;
            case LabOrderStatus.Completed:
                labOrder.CompletedDate = DateTime.UtcNow;
                break;
            case LabOrderStatus.Approved:
                labOrder.ApprovedDate = DateTime.UtcNow;
                labOrder.ApprovedBy = userId;
                break;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validates that the status transition is valid.
    /// </summary>
    private static void ValidateStatusTransition(LabOrderStatus currentStatus, LabOrderStatus newStatus)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<LabOrderStatus, LabOrderStatus[]>
        {
            { LabOrderStatus.Pending, new[] { LabOrderStatus.Received, LabOrderStatus.InProgress, LabOrderStatus.Cancelled } },
            { LabOrderStatus.Received, new[] { LabOrderStatus.InProgress, LabOrderStatus.Cancelled } },
            { LabOrderStatus.InProgress, new[] { LabOrderStatus.Completed, LabOrderStatus.Cancelled } },
            { LabOrderStatus.Completed, new[] { LabOrderStatus.Approved, LabOrderStatus.Rejected } },
            { LabOrderStatus.Approved, Array.Empty<LabOrderStatus>() },  // Terminal state
            { LabOrderStatus.Rejected, Array.Empty<LabOrderStatus>() },  // Terminal state
            { LabOrderStatus.Cancelled, Array.Empty<LabOrderStatus>() }  // Terminal state
        };

        if (currentStatus == newStatus)
        {
            return; // No change, always valid
        }

        if (!validTransitions.TryGetValue(currentStatus, out var allowedStatuses) ||
            !allowedStatuses.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Invalid status transition from {currentStatus} to {newStatus}. " +
                $"Allowed transitions from {currentStatus}: {string.Join(", ", allowedStatuses)}");
        }
    }

    public async Task DeleteLabOrderAsync(int id)
    {
        var labOrder = await _context.LabOrders.FindAsync(id);
        if (labOrder != null)
        {
            _context.LabOrders.Remove(labOrder);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateLabOrderNumberAsync(int branchId)
    {
        return await _sequenceGenerator.GenerateLabOrderNumberAsync(branchId);
    }

    #endregion

    #region Lab Test Management

    public async Task<LabTest?> GetLabTestByIdAsync(int id)
    {
        return await _context.LabTests
            .Include(lt => lt.Branch)
            .Include(lt => lt.ExternalLab)
            .FirstOrDefaultAsync(lt => lt.Id == id);
    }

    public async Task<LabTest?> GetLabTestByCodeAsync(string testCode, int branchId)
    {
        return await _context.LabTests
            .FirstOrDefaultAsync(lt => lt.TestCode == testCode && lt.BranchId == branchId);
    }

    public async Task<IEnumerable<LabTest>> GetLabTestsByBranchIdAsync(int branchId)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Include(lt => lt.ExternalLab)
            .Where(lt => lt.BranchId == branchId)
            .OrderBy(lt => lt.TestName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetActiveLabTestsAsync(int branchId)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Include(lt => lt.ExternalLab)
            .Where(lt => lt.BranchId == branchId && lt.IsActive)
            .OrderBy(lt => lt.TestName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabTest>> GetLabTestsByCategoryAsync(int branchId, TestCategory category)
    {
        return await _context.LabTests
            .AsNoTracking()
            .Where(lt => lt.BranchId == branchId && lt.Category == category && lt.IsActive)
            .OrderBy(lt => lt.TestName)
            .ToListAsync();
    }

    public async Task<LabTest> CreateLabTestAsync(LabTest labTest)
    {
        ArgumentNullException.ThrowIfNull(labTest);

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == labTest.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {labTest.BranchId} not found");
        }

        // Validate external lab exists if specified
        if (labTest.ExternalLabId.HasValue)
        {
            var externalLabExists = await _context.ExternalLabs.AnyAsync(e => e.Id == labTest.ExternalLabId.Value);
            if (!externalLabExists)
            {
                throw new KeyNotFoundException($"External lab with ID {labTest.ExternalLabId.Value} not found");
            }
        }

        _context.LabTests.Add(labTest);
        await _context.SaveChangesAsync();
        return labTest;
    }

    public async Task UpdateLabTestAsync(LabTest labTest)
    {
        ArgumentNullException.ThrowIfNull(labTest);

        // Validate lab test exists
        var existingTest = await _context.LabTests.FindAsync(labTest.Id);
        if (existingTest == null)
        {
            throw new KeyNotFoundException($"Lab test with ID {labTest.Id} not found");
        }

        labTest.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingTest).CurrentValues.SetValues(labTest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLabTestAsync(int id)
    {
        var labTest = await _context.LabTests.FindAsync(id);
        if (labTest != null)
        {
            _context.LabTests.Remove(labTest);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Lab Result Management

    public async Task<LabResult?> GetLabResultByIdAsync(int id)
    {
        return await _context.LabResults
            .Include(lr => lr.LabOrder)
                .ThenInclude(lo => lo.Patient)
            .Include(lr => lr.LabTest)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<IEnumerable<LabResult>> GetLabResultsByOrderIdAsync(int labOrderId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .Include(lr => lr.LabOrder)
            .Include(lr => lr.LabTest)
            .Where(lr => lr.LabOrderId == labOrderId)
            .OrderBy(lr => lr.TestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabResult>> GetLabResultsByPatientIdAsync(int patientId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .Include(lr => lr.LabOrder)
            .Include(lr => lr.LabTest)
            .Where(lr => lr.LabOrder!.PatientId == patientId)
            .OrderByDescending(lr => lr.TestDate)
            .ToListAsync();
    }

    public async Task<LabResult> CreateLabResultAsync(LabResult labResult)
    {
        ArgumentNullException.ThrowIfNull(labResult);

        // Validate lab order exists
        var labOrderExists = await _context.LabOrders.AnyAsync(lo => lo.Id == labResult.LabOrderId);
        if (!labOrderExists)
        {
            throw new KeyNotFoundException($"Lab order with ID {labResult.LabOrderId} not found");
        }

        // Validate lab test exists if specified
        if (labResult.LabTestId > 0)
        {
            var labTestExists = await _context.LabTests.AnyAsync(lt => lt.Id == labResult.LabTestId);
            if (!labTestExists)
            {
                throw new KeyNotFoundException($"Lab test with ID {labResult.LabTestId} not found");
            }
        }

        _context.LabResults.Add(labResult);
        await _context.SaveChangesAsync();
        return labResult;
    }

    public async Task UpdateLabResultAsync(LabResult labResult)
    {
        ArgumentNullException.ThrowIfNull(labResult);

        // Validate lab result exists
        var existingResult = await _context.LabResults.FindAsync(labResult.Id);
        if (existingResult == null)
        {
            throw new KeyNotFoundException($"Lab result with ID {labResult.Id} not found");
        }

        labResult.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingResult).CurrentValues.SetValues(labResult);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLabResultAsync(int id)
    {
        var labResult = await _context.LabResults.FindAsync(id);
        if (labResult != null)
        {
            _context.LabResults.Remove(labResult);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region External Lab Management

    public async Task<ExternalLab?> GetExternalLabByIdAsync(int id)
    {
        return await _context.ExternalLabs
            .Include(el => el.Branch)
            .FirstOrDefaultAsync(el => el.Id == id);
    }

    public async Task<IEnumerable<ExternalLab>> GetExternalLabsByBranchIdAsync(int branchId)
    {
        return await _context.ExternalLabs
            .AsNoTracking()
            .Where(el => el.BranchId == branchId)
            .OrderBy(el => el.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExternalLab>> GetActiveExternalLabsAsync(int branchId)
    {
        return await _context.ExternalLabs
            .AsNoTracking()
            .Where(el => el.BranchId == branchId && el.IsActive)
            .OrderBy(el => el.Name)
            .ToListAsync();
    }

    public async Task<ExternalLab> CreateExternalLabAsync(ExternalLab externalLab)
    {
        ArgumentNullException.ThrowIfNull(externalLab);

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == externalLab.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {externalLab.BranchId} not found");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(externalLab.Name))
        {
            throw new ArgumentException("External lab name is required", nameof(externalLab));
        }

        _context.ExternalLabs.Add(externalLab);
        await _context.SaveChangesAsync();
        return externalLab;
    }

    public async Task UpdateExternalLabAsync(ExternalLab externalLab)
    {
        ArgumentNullException.ThrowIfNull(externalLab);

        // Validate external lab exists
        var existingLab = await _context.ExternalLabs.FindAsync(externalLab.Id);
        if (existingLab == null)
        {
            throw new KeyNotFoundException($"External lab with ID {externalLab.Id} not found");
        }

        externalLab.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingLab).CurrentValues.SetValues(externalLab);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExternalLabAsync(int id)
    {
        var externalLab = await _context.ExternalLabs.FindAsync(id);
        if (externalLab != null)
        {
            _context.ExternalLabs.Remove(externalLab);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetPendingOrdersCountAsync(int branchId)
    {
        return await _context.LabOrders
            .CountAsync(lo => lo.BranchId == branchId && lo.Status == LabOrderStatus.Pending);
    }

    public async Task<int> GetUrgentOrdersCountAsync(int branchId)
    {
        return await _context.LabOrders
            .CountAsync(lo => lo.BranchId == branchId && lo.IsUrgent &&
                lo.Status != LabOrderStatus.Completed && lo.Status != LabOrderStatus.Cancelled);
    }

    public async Task<decimal> GetMonthlyRevenueAsync(int branchId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.LabOrders
            .Where(lo => lo.BranchId == branchId &&
                   lo.OrderDate >= startDate &&
                   lo.OrderDate < endDate &&
                   lo.IsPaid)
            .SumAsync(lo => lo.TotalAmount);
    }

    public async Task<Dictionary<LabOrderStatus, int>> GetOrderStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.LabOrders
            .Where(lo => lo.BranchId == branchId)
            .GroupBy(lo => lo.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return distribution;
    }

    #endregion
}
