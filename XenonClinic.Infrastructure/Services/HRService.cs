using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Human Resources management
/// </summary>
public class HRService : IHRService
{
    private readonly ClinicDbContext _context;

    public HRService(ClinicDbContext context)
    {
        _context = context;
    }

    #region Employee Management

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode, int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && e.BranchId == branchId);
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByBranchIdAsync(int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .Include(e => e.JobPosition)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int jobPositionId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.JobPositionId == jobPositionId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(int branchId, EmploymentStatus status)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == status)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateEmployeeCodeAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"EMP-{today:yyyyMM}";

        var lastEmployee = await _context.Employees
            .Where(e => e.BranchId == branchId && e.EmployeeCode.StartsWith(prefix))
            .OrderByDescending(e => e.EmployeeCode)
            .FirstOrDefaultAsync();

        if (lastEmployee == null)
        {
            return $"{prefix}-001";
        }

        var lastNumber = int.Parse(lastEmployee.EmployeeCode.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }

    #endregion

    #region Attendance Management

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _context.Attendances
            .Where(a => a.EmployeeId == employeeId && a.Date >= startDate.Date && a.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByBranchIdAsync(int branchId, DateTime date)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => a.Employee!.BranchId == branchId && a.Date.Date == date.Date)
            .OrderBy(a => a.Employee!.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(int branchId)
    {
        return await GetAttendanceByBranchIdAsync(branchId, DateTime.UtcNow);
    }

    public async Task<IEnumerable<Attendance>> GetAbsentEmployeesAsync(int branchId, DateTime date)
    {
        var presentEmployeeIds = await _context.Attendances
            .Where(a => a.Date.Date == date.Date && a.Employee!.BranchId == branchId)
            .Select(a => a.EmployeeId)
            .ToListAsync();

        var absentEmployees = await _context.Employees
            .Where(e => e.BranchId == branchId &&
                   e.EmploymentStatus == EmploymentStatus.Active &&
                   !presentEmployeeIds.Contains(e.Id))
            .ToListAsync();

        // Return as attendance records with absent status
        return absentEmployees.Select(e => new Attendance
        {
            EmployeeId = e.Id,
            Employee = e,
            Date = date.Date,
            Status = AttendanceStatus.Absent
        });
    }

    public async Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task UpdateAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAttendanceAsync(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance != null)
        {
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Attendance> CheckInAsync(int employeeId, DateTime checkInTime)
    {
        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            Date = checkInTime.Date,
            CheckInTime = TimeOnly.FromDateTime(checkInTime),
            Status = AttendanceStatus.Present
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<Attendance> CheckOutAsync(int attendanceId, DateTime checkOutTime)
    {
        var attendance = await _context.Attendances.FindAsync(attendanceId);
        if (attendance == null)
            throw new KeyNotFoundException($"Attendance record with ID {attendanceId} not found");

        attendance.CheckOutTime = TimeOnly.FromDateTime(checkOutTime);

        // Calculate total hours
        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            var duration = attendance.CheckOutTime.Value.ToTimeSpan() - attendance.CheckInTime.Value.ToTimeSpan();
            attendance.TotalHours = (decimal)duration.TotalHours;
        }

        await _context.SaveChangesAsync();
        return attendance;
    }

    #endregion

    #region Leave Request Management

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByBranchIdAsync(int branchId)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee!.BranchId == branchId)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync(int branchId)
    {
        return await GetLeaveRequestsByStatusAsync(branchId, LeaveRequestStatus.Pending);
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(int branchId, LeaveRequestStatus status)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee!.BranchId == branchId && lr.Status == status)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLeaveRequestAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest != null)
        {
            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ApproveLeaveRequestAsync(int leaveRequestId, string approvedBy)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request with ID {leaveRequestId} not found");

        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.ApprovedBy = approvedBy;
        leaveRequest.ApprovedDate = DateTime.UtcNow;

        // Deduct from employee leave balance
        var employee = leaveRequest.Employee;
        if (employee != null)
        {
            var days = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
            if (leaveRequest.LeaveType == LeaveType.Annual)
            {
                employee.AnnualLeaveBalance -= days;
            }
            else if (leaveRequest.LeaveType == LeaveType.Sick)
            {
                employee.SickLeaveBalance -= days;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task RejectLeaveRequestAsync(int leaveRequestId, string rejectedBy, string reason)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request with ID {leaveRequestId} not found");

        leaveRequest.Status = LeaveRequestStatus.Rejected;
        leaveRequest.ApprovedBy = rejectedBy;
        leaveRequest.ApprovedDate = DateTime.UtcNow;
        leaveRequest.Remarks = reason;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateLeaveBalanceAsync(int employeeId, LeaveType leaveType, int requestedDays)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
            return false;

        return leaveType switch
        {
            LeaveType.Annual => employee.AnnualLeaveBalance >= requestedDays,
            LeaveType.Sick => employee.SickLeaveBalance >= requestedDays,
            _ => true // Other leave types don't require balance check
        };
    }

    #endregion

    #region Department Management

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByBranchIdAsync(int branchId)
    {
        return await _context.Departments
            .Where(d => d.BranchId == branchId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync(int branchId)
    {
        return await _context.Departments
            .Where(d => d.BranchId == branchId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Job Position Management

    public async Task<JobPosition?> GetJobPositionByIdAsync(int id)
    {
        return await _context.JobPositions
            .Include(jp => jp.Branch)
            .FirstOrDefaultAsync(jp => jp.Id == id);
    }

    public async Task<IEnumerable<JobPosition>> GetJobPositionsByBranchIdAsync(int branchId)
    {
        return await _context.JobPositions
            .Where(jp => jp.BranchId == branchId)
            .OrderBy(jp => jp.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobPosition>> GetJobPositionsByDepartmentAsync(int departmentId)
    {
        return await _context.JobPositions
            .Where(jp => jp.DepartmentId == departmentId)
            .OrderBy(jp => jp.Title)
            .ToListAsync();
    }

    public async Task<JobPosition> CreateJobPositionAsync(JobPosition jobPosition)
    {
        _context.JobPositions.Add(jobPosition);
        await _context.SaveChangesAsync();
        return jobPosition;
    }

    public async Task UpdateJobPositionAsync(JobPosition jobPosition)
    {
        _context.JobPositions.Update(jobPosition);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteJobPositionAsync(int id)
    {
        var jobPosition = await _context.JobPositions.FindAsync(id);
        if (jobPosition != null)
        {
            _context.JobPositions.Remove(jobPosition);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalEmployeesCountAsync(int branchId)
    {
        return await _context.Employees
            .CountAsync(e => e.BranchId == branchId);
    }

    public async Task<int> GetActiveEmployeesCountAsync(int branchId)
    {
        return await _context.Employees
            .CountAsync(e => e.BranchId == branchId && e.EmploymentStatus == EmploymentStatus.Active);
    }

    public async Task<int> GetPresentTodayCountAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Attendances
            .CountAsync(a => a.Employee!.BranchId == branchId &&
                   a.Date.Date == today &&
                   a.Status == AttendanceStatus.Present);
    }

    public async Task<int> GetAbsentTodayCountAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var presentCount = await GetPresentTodayCountAsync(branchId);
        var activeCount = await GetActiveEmployeesCountAsync(branchId);
        return activeCount - presentCount;
    }

    public async Task<int> GetPendingLeaveRequestsCountAsync(int branchId)
    {
        return await _context.LeaveRequests
            .CountAsync(lr => lr.Employee!.BranchId == branchId && lr.Status == LeaveRequestStatus.Pending);
    }

    public async Task<decimal> GetTotalPayrollAsync(int branchId)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == EmploymentStatus.Active)
            .SumAsync(e => e.BasicSalary +
                     (e.HousingAllowance ?? 0) +
                     (e.TransportAllowance ?? 0) +
                     (e.OtherAllowances ?? 0));
    }

    public async Task<Dictionary<EmploymentStatus, int>> GetEmployeeStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Employees
            .Where(e => e.BranchId == branchId)
            .GroupBy(e => e.EmploymentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return distribution;
    }

    public async Task<Dictionary<int, int>> GetEmployeesByDepartmentDistributionAsync(int branchId)
    {
        var distribution = await _context.Employees
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == EmploymentStatus.Active)
            .GroupBy(e => e.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

        return distribution;
    }

    #endregion
}
