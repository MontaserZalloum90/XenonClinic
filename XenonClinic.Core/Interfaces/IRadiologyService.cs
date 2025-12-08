using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Radiology/Imaging management
/// Note: This service currently uses LabOrder/LabTest entities as a foundation.
/// When specific RadiologyOrder/ImagingStudy entities are created, this should be refactored.
/// </summary>
public interface IRadiologyService
{
    // Radiology Order Management (using LabOrder as template)
    Task<LabOrder?> GetRadiologyOrderByIdAsync(int id);
    Task<LabOrder?> GetRadiologyOrderByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<LabOrder>> GetRadiologyOrdersByBranchIdAsync(int branchId);
    Task<IEnumerable<LabOrder>> GetRadiologyOrdersByPatientIdAsync(int patientId);
    Task<IEnumerable<LabOrder>> GetRadiologyOrdersByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LabOrder>> GetPendingRadiologyOrdersAsync(int branchId);
    Task<IEnumerable<LabOrder>> GetCompletedRadiologyOrdersAsync(int branchId);
    Task<LabOrder> CreateRadiologyOrderAsync(LabOrder order);
    Task UpdateRadiologyOrderAsync(LabOrder order);
    Task DeleteRadiologyOrderAsync(int id);
    Task<string> GenerateRadiologyOrderNumberAsync(int branchId);

    // Imaging Study Management (using LabTest as template)
    Task<LabTest?> GetImagingStudyByIdAsync(int id);
    Task<LabTest?> GetImagingStudyByCodeAsync(string studyCode, int branchId);
    Task<IEnumerable<LabTest>> GetImagingStudiesByBranchIdAsync(int branchId);
    Task<IEnumerable<LabTest>> GetActiveImagingStudiesAsync(int branchId);
    Task<IEnumerable<LabTest>> GetImagingStudiesByCategoryAsync(int branchId, string category);
    Task<LabTest> CreateImagingStudyAsync(LabTest study);
    Task UpdateImagingStudyAsync(LabTest study);
    Task DeleteImagingStudyAsync(int id);

    // Imaging Result Management (using LabResult as template)
    Task<LabResult?> GetImagingResultByIdAsync(int id);
    Task<IEnumerable<LabResult>> GetImagingResultsByOrderIdAsync(int orderId);
    Task<IEnumerable<LabResult>> GetImagingResultsByPatientIdAsync(int patientId);
    Task<LabResult> CreateImagingResultAsync(LabResult result);
    Task UpdateImagingResultAsync(LabResult result);
    Task DeleteImagingResultAsync(int id);

    // Order Status Management
    Task ReceiveRadiologyOrderAsync(int orderId, string receivedBy);
    Task StartImagingAsync(int orderId, string technician);
    Task CompleteRadiologyOrderAsync(int orderId, string completedBy);
    Task ApproveRadiologyOrderAsync(int orderId, string approvedBy);
    Task RejectRadiologyOrderAsync(int orderId, string rejectedBy, string reason);

    // Statistics & Reporting
    Task<int> GetTotalRadiologyOrdersCountAsync(int branchId);
    Task<int> GetPendingRadiologyOrdersCountAsync(int branchId);
    Task<int> GetCompletedRadiologyOrdersCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalRadiologyRevenueAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<(string StudyName, int Count, decimal Revenue)>> GetMostOrderedStudiesAsync(int branchId, int topN = 10);
    Task<Dictionary<string, int>> GetOrdersByModalityDistributionAsync(int branchId);
}
