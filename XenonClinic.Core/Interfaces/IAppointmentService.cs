using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Appointment management
/// </summary>
public interface IAppointmentService
{
    // Appointment Management
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetAppointmentsByBranchIdAsync(int branchId);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByProviderIdAsync(int providerId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(int branchId, DateTime date);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(int branchId, AppointmentStatus status);
    Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(int branchId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int branchId, int days = 7);
    Task<Appointment> CreateAppointmentAsync(Appointment appointment);
    Task UpdateAppointmentAsync(Appointment appointment);
    Task DeleteAppointmentAsync(int id);

    // Appointment Status Management
    Task ConfirmAppointmentAsync(int appointmentId);
    Task CancelAppointmentAsync(int appointmentId, string? reason);
    Task CheckInAppointmentAsync(int appointmentId);
    Task CompleteAppointmentAsync(int appointmentId);
    Task NoShowAppointmentAsync(int appointmentId);

    // Appointment Scheduling
    Task<bool> IsTimeSlotAvailableAsync(int branchId, int? providerId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
    Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int branchId, int? providerId, DateTime date, int durationMinutes = 30);
    Task<Appointment> RescheduleAppointmentAsync(int appointmentId, DateTime newStartTime, DateTime newEndTime);

    // Statistics & Reporting
    Task<int> GetTotalAppointmentsCountAsync(int branchId);
    Task<int> GetTodayAppointmentsCountAsync(int branchId);
    Task<int> GetUpcomingAppointmentsCountAsync(int branchId);
    Task<int> GetCompletedAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<int> GetCancelledAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<int> GetNoShowAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<Dictionary<AppointmentStatus, int>> GetAppointmentStatusDistributionAsync(int branchId);
    Task<Dictionary<AppointmentType, int>> GetAppointmentTypeDistributionAsync(int branchId);
    Task<decimal> GetAppointmentCompletionRateAsync(int branchId, DateTime startDate, DateTime endDate);
}
