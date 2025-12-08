using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Laboratory management
/// </summary>
public interface ILabService
{
    // Lab Order Management
    Task<LabOrder?> GetLabOrderByIdAsync(int id);
    Task<LabOrder?> GetLabOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<LabOrder>> GetLabOrdersByBranchIdAsync(int branchId);
    Task<IEnumerable<LabOrder>> GetLabOrdersByPatientIdAsync(int patientId);
    Task<IEnumerable<LabOrder>> GetLabOrdersByStatusAsync(int branchId, LabOrderStatus status);
    Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync(int branchId);
    Task<IEnumerable<LabOrder>> GetUrgentLabOrdersAsync(int branchId);
    Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder);
    Task UpdateLabOrderAsync(LabOrder labOrder);
    Task UpdateLabOrderStatusAsync(int labOrderId, LabOrderStatus status, string userId);
    Task DeleteLabOrderAsync(int id);
    Task<string> GenerateLabOrderNumberAsync(int branchId);

    // Lab Test Management
    Task<LabTest?> GetLabTestByIdAsync(int id);
    Task<LabTest?> GetLabTestByCodeAsync(string testCode, int branchId);
    Task<IEnumerable<LabTest>> GetLabTestsByBranchIdAsync(int branchId);
    Task<IEnumerable<LabTest>> GetActiveLabTestsAsync(int branchId);
    Task<IEnumerable<LabTest>> GetLabTestsByCategoryAsync(int branchId, TestCategory category);
    Task<LabTest> CreateLabTestAsync(LabTest labTest);
    Task UpdateLabTestAsync(LabTest labTest);
    Task DeleteLabTestAsync(int id);

    // Lab Result Management
    Task<LabResult?> GetLabResultByIdAsync(int id);
    Task<IEnumerable<LabResult>> GetLabResultsByOrderIdAsync(int labOrderId);
    Task<IEnumerable<LabResult>> GetLabResultsByPatientIdAsync(int patientId);
    Task<LabResult> CreateLabResultAsync(LabResult labResult);
    Task UpdateLabResultAsync(LabResult labResult);
    Task DeleteLabResultAsync(int id);

    // External Lab Management
    Task<ExternalLab?> GetExternalLabByIdAsync(int id);
    Task<IEnumerable<ExternalLab>> GetExternalLabsByBranchIdAsync(int branchId);
    Task<IEnumerable<ExternalLab>> GetActiveExternalLabsAsync(int branchId);
    Task<ExternalLab> CreateExternalLabAsync(ExternalLab externalLab);
    Task UpdateExternalLabAsync(ExternalLab externalLab);
    Task DeleteExternalLabAsync(int id);

    // Statistics & Reporting
    Task<int> GetPendingOrdersCountAsync(int branchId);
    Task<int> GetUrgentOrdersCountAsync(int branchId);
    Task<decimal> GetMonthlyRevenueAsync(int branchId, int year, int month);
    Task<Dictionary<LabOrderStatus, int>> GetOrderStatusDistributionAsync(int branchId);
}
