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
    private readonly ISequenceGenerator _sequenceGenerator;

    public HRService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
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
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.JobPosition)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int jobPositionId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.JobPositionId == jobPositionId)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(int branchId, EmploymentStatus status)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && e.EmploymentStatus == status)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == employee.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {employee.BranchId} not found");
        }

        // Validate department exists if specified
        if (employee.DepartmentId > 0)
        {
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == employee.DepartmentId);
            if (!departmentExists)
            {
                throw new KeyNotFoundException($"Department with ID {employee.DepartmentId} not found");
            }
        }

        // Validate job position exists if specified
        if (employee.JobPositionId.HasValue)
        {
            var positionExists = await _context.JobPositions.AnyAsync(jp => jp.Id == employee.JobPositionId.Value);
            if (!positionExists)
            {
                throw new KeyNotFoundException($"Job position with ID {employee.JobPositionId.Value} not found");
            }
        }

        // Check for duplicate employee code
        var duplicateCode = await _context.Employees
            .AnyAsync(e => e.EmployeeCode == employee.EmployeeCode && e.BranchId == employee.BranchId);
        if (duplicateCode)
        {
            throw new InvalidOperationException($"An employee with code '{employee.EmployeeCode}' already exists in this branch");
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        // Validate employee exists
        var existingEmployee = await _context.Employees.FindAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employee.Id} not found");
        }

        employee.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
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
        return await _sequenceGenerator.GenerateEmployeeCodeAsync(branchId);
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
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId && a.Date >= startDate.Date && a.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByBranchIdAsync(int branchId, DateTime date)
    {
        return await _context.Attendances
            .AsNoTracking()
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
        ArgumentNullException.ThrowIfNull(attendance);

        // Validate employee exists
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == attendance.EmployeeId);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {attendance.EmployeeId} not found");
        }

        // Check for duplicate attendance on the same day
        var duplicateAttendance = await _context.Attendances
            .AnyAsync(a => a.EmployeeId == attendance.EmployeeId && a.Date.Date == attendance.Date.Date);
        if (duplicateAttendance)
        {
            throw new InvalidOperationException($"Attendance record already exists for employee {attendance.EmployeeId} on {attendance.Date:yyyy-MM-dd}");
        }

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task UpdateAttendanceAsync(Attendance attendance)
    {
        ArgumentNullException.ThrowIfNull(attendance);

        // Validate attendance exists
        var existingAttendance = await _context.Attendances.FindAsync(attendance.Id);
        if (existingAttendance == null)
        {
            throw new KeyNotFoundException($"Attendance record with ID {attendance.Id} not found");
        }

        _context.Entry(existingAttendance).CurrentValues.SetValues(attendance);
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
        // Validate employee exists
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        // Check for existing attendance on the same day
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == checkInTime.Date);

        if (existingAttendance != null)
        {
            // If already checked in, return the existing record
            if (existingAttendance.CheckInTime.HasValue)
            {
                throw new InvalidOperationException(
                    $"Employee has already checked in at {existingAttendance.CheckInTime.Value} on {checkInTime:yyyy-MM-dd}");
            }

            // Update existing absent record with check-in time
            existingAttendance.CheckInTime = TimeOnly.FromDateTime(checkInTime);
            existingAttendance.Status = AttendanceStatus.Present;
            await _context.SaveChangesAsync();
            return existingAttendance;
        }

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

            // BUG FIX: Handle overnight shifts where checkout is before checkin
            // (e.g., checkin at 23:00, checkout at 07:00 next day)
            if (duration.TotalHours < 0)
            {
                // Add 24 hours for overnight shifts
                duration = duration.Add(TimeSpan.FromHours(24));
            }

            // BUG FIX: Validate reasonable work hours (max 24 hours per shift)
            if (duration.TotalHours > 24)
            {
                throw new InvalidOperationException("Work duration cannot exceed 24 hours");
            }

            attendance.TotalHours = Math.Round((decimal)duration.TotalHours, 2);
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
            .AsNoTracking()
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByBranchIdAsync(int branchId)
    {
        return await _context.LeaveRequests
            .AsNoTracking()
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
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee!.BranchId == branchId && lr.Status == status)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        ArgumentNullException.ThrowIfNull(leaveRequest);

        // Validate employee exists
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == leaveRequest.EmployeeId);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {leaveRequest.EmployeeId} not found");
        }

        // Validate date range
        if (leaveRequest.EndDate < leaveRequest.StartDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date");
        }

        // Validate leave dates are in the future
        if (leaveRequest.StartDate.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Leave request start date cannot be in the past");
        }

        // Check for overlapping leave requests
        var overlappingRequest = await _context.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == leaveRequest.EmployeeId &&
                      lr.Status != LeaveRequestStatus.Rejected &&
                      lr.StartDate <= leaveRequest.EndDate &&
                      lr.EndDate >= leaveRequest.StartDate);
        if (overlappingRequest)
        {
            throw new InvalidOperationException("An overlapping leave request already exists for this period");
        }

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        ArgumentNullException.ThrowIfNull(leaveRequest);

        // Validate leave request exists
        var existingRequest = await _context.LeaveRequests.FindAsync(leaveRequest.Id);
        if (existingRequest == null)
        {
            throw new KeyNotFoundException($"Leave request with ID {leaveRequest.Id} not found");
        }

        // Only allow updates to pending requests
        if (existingRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave requests can be modified");
        }

        _context.Entry(existingRequest).CurrentValues.SetValues(leaveRequest);
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

        // Validate status - can only approve pending requests
        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot approve leave request in {leaveRequest.Status} status. Only pending requests can be approved.");
        }

        // Calculate requested days
        var requestedDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

        // Validate leave balance for annual and sick leave
        if (leaveRequest.LeaveType == LeaveType.Annual || leaveRequest.LeaveType == LeaveType.Sick)
        {
            var hasBalance = await ValidateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveType, requestedDays);
            if (!hasBalance)
            {
                throw new InvalidOperationException(
                    $"Insufficient {leaveRequest.LeaveType} leave balance. Requested: {requestedDays} days.");
            }
        }

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

        // Validate status - can only reject pending requests
        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot reject leave request in {leaveRequest.Status} status. Only pending requests can be rejected.");
        }

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
            .AsNoTracking()
            .Include(d => d.Branch)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Where(d => d.BranchId == branchId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync(int branchId)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.BranchId == branchId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == department.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {department.BranchId} not found");
        }

        // Check for duplicate department name in branch
        var duplicateName = await _context.Departments
            .AnyAsync(d => d.Name == department.Name && d.BranchId == department.BranchId);
        if (duplicateName)
        {
            throw new InvalidOperationException($"A department with name '{department.Name}' already exists in this branch");
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        // Validate department exists
        var existingDept = await _context.Departments.FindAsync(department.Id);
        if (existingDept == null)
        {
            throw new KeyNotFoundException($"Department with ID {department.Id} not found");
        }

        department.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingDept).CurrentValues.SetValues(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department != null)
        {
            // Check for employees in department
            if (department.Employees?.Any() == true)
            {
                throw new InvalidOperationException(
                    $"Cannot delete department '{department.Name}' because it has {department.Employees.Count} employees assigned");
            }

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
            .AsNoTracking()
            .Include(jp => jp.Employees)
            .Where(jp => jp.BranchId == branchId)
            .OrderBy(jp => jp.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobPosition>> GetJobPositionsByDepartmentAsync(int departmentId)
    {
        return await _context.JobPositions
            .AsNoTracking()
            .Where(jp => jp.DepartmentId == departmentId)
            .OrderBy(jp => jp.Title)
            .ToListAsync();
    }

    public async Task<JobPosition> CreateJobPositionAsync(JobPosition jobPosition)
    {
        ArgumentNullException.ThrowIfNull(jobPosition);

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == jobPosition.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {jobPosition.BranchId} not found");
        }

        // Check for duplicate position title in branch
        var duplicateTitle = await _context.JobPositions
            .AnyAsync(jp => jp.Title == jobPosition.Title && jp.BranchId == jobPosition.BranchId);
        if (duplicateTitle)
        {
            throw new InvalidOperationException($"A job position with title '{jobPosition.Title}' already exists in this branch");
        }

        _context.JobPositions.Add(jobPosition);
        await _context.SaveChangesAsync();
        return jobPosition;
    }

    public async Task UpdateJobPositionAsync(JobPosition jobPosition)
    {
        ArgumentNullException.ThrowIfNull(jobPosition);

        // Validate job position exists
        var existingPosition = await _context.JobPositions.FindAsync(jobPosition.Id);
        if (existingPosition == null)
        {
            throw new KeyNotFoundException($"Job position with ID {jobPosition.Id} not found");
        }

        jobPosition.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingPosition).CurrentValues.SetValues(jobPosition);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteJobPositionAsync(int id)
    {
        var jobPosition = await _context.JobPositions
            .Include(jp => jp.Employees)
            .FirstOrDefaultAsync(jp => jp.Id == id);

        if (jobPosition != null)
        {
            // Check for employees in this position
            if (jobPosition.Employees?.Any() == true)
            {
                throw new InvalidOperationException(
                    $"Cannot delete job position '{jobPosition.Title}' because it has {jobPosition.Employees.Count} employees assigned");
            }

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
