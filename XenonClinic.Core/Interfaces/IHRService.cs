using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Human Resources management
/// </summary>
public interface IHRService
{
    // Employee Management
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee?> GetEmployeeByCodeAsync(string employeeCode, int branchId);
    Task<Employee?> GetEmployeeByUserIdAsync(string userId);
    Task<IEnumerable<Employee>> GetEmployeesByBranchIdAsync(int branchId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId);
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int jobPositionId);
    Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(int branchId, EmploymentStatus status);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(int id);
    Task<string> GenerateEmployeeCodeAsync(int branchId);

    // Attendance Management
    Task<Attendance?> GetAttendanceByIdAsync(int id);
    Task<IEnumerable<Attendance>> GetAttendanceByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Attendance>> GetAttendanceByBranchIdAsync(int branchId, DateTime date);
    Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(int branchId);
    Task<IEnumerable<Attendance>> GetAbsentEmployeesAsync(int branchId, DateTime date);
    Task<Attendance> CreateAttendanceAsync(Attendance attendance);
    Task UpdateAttendanceAsync(Attendance attendance);
    Task DeleteAttendanceAsync(int id);
    Task<Attendance> CheckInAsync(int employeeId, DateTime checkInTime);
    Task<Attendance> CheckOutAsync(int attendanceId, DateTime checkOutTime);

    // Leave Request Management
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByBranchIdAsync(int branchId);
    Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync(int branchId);
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(int branchId, LeaveStatus status);
    Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest);
    Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest);
    Task DeleteLeaveRequestAsync(int id);
    Task ApproveLeaveRequestAsync(int leaveRequestId, string approvedBy);
    Task RejectLeaveRequestAsync(int leaveRequestId, string rejectedBy, string reason);
    Task<bool> ValidateLeaveBalanceAsync(int employeeId, LeaveType leaveType, int requestedDays);

    // Department Management
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<Department>> GetDepartmentsByBranchIdAsync(int branchId);
    Task<IEnumerable<Department>> GetActiveDepartmentsAsync(int branchId);
    Task<Department> CreateDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(int id);

    // Job Position Management
    Task<JobPosition?> GetJobPositionByIdAsync(int id);
    Task<IEnumerable<JobPosition>> GetJobPositionsByBranchIdAsync(int branchId);
    Task<IEnumerable<JobPosition>> GetJobPositionsByDepartmentAsync(int departmentId);
    Task<JobPosition> CreateJobPositionAsync(JobPosition jobPosition);
    Task UpdateJobPositionAsync(JobPosition jobPosition);
    Task DeleteJobPositionAsync(int id);

    // Statistics & Reporting
    Task<int> GetTotalEmployeesCountAsync(int branchId);
    Task<int> GetActiveEmployeesCountAsync(int branchId);
    Task<int> GetPresentTodayCountAsync(int branchId);
    Task<int> GetAbsentTodayCountAsync(int branchId);
    Task<int> GetPendingLeaveRequestsCountAsync(int branchId);
    Task<decimal> GetTotalPayrollAsync(int branchId);
    Task<Dictionary<EmploymentStatus, int>> GetEmployeeStatusDistributionAsync(int branchId);
    Task<Dictionary<int, int>> GetEmployeesByDepartmentDistributionAsync(int branchId);
}
