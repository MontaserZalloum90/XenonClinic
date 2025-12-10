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
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode, int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && e.BranchId == branchId && !e.IsDeleted);
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .FirstOrDefaultAsync(e => e.UserId == userId && !e.IsDeleted);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByBranchIdAsync(int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && !e.IsDeleted)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && !e.IsDeleted && e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .Include(e => e.JobPosition)
            .Where(e => e.DepartmentId == departmentId && !e.IsDeleted)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int jobPositionId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.JobPositionId == jobPositionId && !e.IsDeleted)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(int branchId, EmploymentStatus status)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.BranchId == branchId && !e.IsDeleted && e.EmploymentStatus == status)
            .OrderBy(e => e.FullNameEn)
            .ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        // Validate salary is positive
        if (employee.BasicSalary <= 0)
        {
            throw new InvalidOperationException("Basic salary must be greater than zero");
        }

        // Check for duplicate EmiratesId within the same branch
        var existingByEmiratesId = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmiratesId == employee.EmiratesId &&
                                       e.BranchId == employee.BranchId &&
                                       !e.IsDeleted);

        if (existingByEmiratesId != null)
        {
            throw new InvalidOperationException(
                $"An employee with Emirates ID '{employee.EmiratesId}' already exists in this branch");
        }

        // Check for duplicate EmployeeCode within the same branch
        if (!string.IsNullOrEmpty(employee.EmployeeCode))
        {
            var existingByCode = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == employee.EmployeeCode &&
                                           e.BranchId == employee.BranchId &&
                                           !e.IsDeleted);

            if (existingByCode != null)
            {
                throw new InvalidOperationException(
                    $"An employee with code '{employee.EmployeeCode}' already exists in this branch");
            }
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        // Validate salary is positive
        if (employee.BasicSalary <= 0)
        {
            throw new InvalidOperationException("Basic salary must be greater than zero");
        }

        // Check for duplicate EmiratesId if changed
        var existingByEmiratesId = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmiratesId == employee.EmiratesId &&
                                       e.BranchId == employee.BranchId &&
                                       e.Id != employee.Id &&
                                       !e.IsDeleted);

        if (existingByEmiratesId != null)
        {
            throw new InvalidOperationException(
                $"An employee with Emirates ID '{employee.EmiratesId}' already exists in this branch");
        }

        // Check for duplicate EmployeeCode if changed
        if (!string.IsNullOrEmpty(employee.EmployeeCode))
        {
            var existingByCode = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == employee.EmployeeCode &&
                                           e.BranchId == employee.BranchId &&
                                           e.Id != employee.Id &&
                                           !e.IsDeleted);

            if (existingByCode != null)
            {
                throw new InvalidOperationException(
                    $"An employee with code '{employee.EmployeeCode}' already exists in this branch");
            }
        }

        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            // Soft delete for HR compliance - employee records must be retained
            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.EmploymentStatus = EmploymentStatus.Terminated;
            employee.TerminationDate = DateTime.UtcNow;
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
            .Where(a => a.EmployeeId == employeeId && a.Date >= startDate.Date && a.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByBranchIdAsync(int branchId, DateTime date)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
            .Where(a => a.Employee!.BranchId == branchId &&
                        !a.Employee.IsDeleted &&
                        a.Date.Date == date.Date)
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
                   !e.IsDeleted &&
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
        // Validate employee exists and is active
        var employee = await _context.Employees.FindAsync(attendance.EmployeeId);
        if (employee == null || employee.IsDeleted)
        {
            throw new KeyNotFoundException($"Employee with ID {attendance.EmployeeId} not found");
        }

        if (employee.EmploymentStatus != EmploymentStatus.Active)
        {
            throw new InvalidOperationException("Cannot create attendance for an inactive employee");
        }

        // Check for duplicate attendance on the same day
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == attendance.EmployeeId && a.Date.Date == attendance.Date.Date);

        if (existingAttendance != null)
        {
            throw new InvalidOperationException(
                $"Attendance record already exists for employee on {attendance.Date:yyyy-MM-dd}");
        }

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task UpdateAttendanceAsync(Attendance attendance)
    {
        // Verify the attendance record exists
        var existingAttendance = await _context.Attendances.FindAsync(attendance.Id);
        if (existingAttendance == null)
        {
            throw new KeyNotFoundException($"Attendance record with ID {attendance.Id} not found");
        }

        // Validate checkout time is after checkin time if both are provided
        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            if (attendance.CheckOutTime.Value < attendance.CheckInTime.Value)
            {
                throw new InvalidOperationException("Check out time must be after check in time");
            }
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
        // Verify employee exists and is active
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null || employee.IsDeleted)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        if (employee.EmploymentStatus != EmploymentStatus.Active)
        {
            throw new InvalidOperationException("Cannot check in for an inactive employee");
        }

        // Check for duplicate check-in on the same day
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == checkInTime.Date);

        if (existingAttendance != null)
        {
            throw new InvalidOperationException(
                $"Employee has already checked in for {checkInTime:yyyy-MM-dd}. Use the existing attendance record.");
        }

        // Calculate late status based on employee's work schedule
        var checkInTimeOnly = TimeOnly.FromDateTime(checkInTime);
        var isLate = false;
        int? lateMinutes = null;

        if (employee.WorkStartTime.HasValue)
        {
            isLate = checkInTimeOnly > employee.WorkStartTime.Value;
            if (isLate)
            {
                lateMinutes = (int)(checkInTimeOnly.ToTimeSpan() - employee.WorkStartTime.Value.ToTimeSpan()).TotalMinutes;
            }
        }

        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            Date = checkInTime.Date,
            CheckInTime = checkInTimeOnly,
            Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
            IsLate = isLate,
            LateMinutes = lateMinutes
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<Attendance> CheckOutAsync(int attendanceId, DateTime checkOutTime)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == attendanceId);

        if (attendance == null)
            throw new KeyNotFoundException($"Attendance record with ID {attendanceId} not found");

        // Validate that check-in was performed
        if (!attendance.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Cannot check out without checking in first");
        }

        // Validate that check-out has not already been performed
        if (attendance.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Check out has already been recorded for this attendance");
        }

        var checkOutTimeOnly = TimeOnly.FromDateTime(checkOutTime);

        // Validate that check-out time is after check-in time (for same day)
        if (attendance.Date.Date == checkOutTime.Date && checkOutTimeOnly < attendance.CheckInTime.Value)
        {
            throw new InvalidOperationException("Check out time must be after check in time");
        }

        attendance.CheckOutTime = checkOutTimeOnly;

        // Calculate worked hours
        var duration = attendance.CheckOutTime.Value.ToTimeSpan() - attendance.CheckInTime.Value.ToTimeSpan();
        attendance.WorkedHours = (decimal)duration.TotalHours;

        // Calculate overtime if employee has a work schedule
        if (attendance.Employee?.WorkEndTime.HasValue == true)
        {
            var expectedWorkHours = (attendance.Employee.WorkEndTime!.Value.ToTimeSpan() -
                                      attendance.Employee.WorkStartTime!.Value.ToTimeSpan()).TotalHours;
            if (attendance.WorkedHours > (decimal)expectedWorkHours)
            {
                attendance.OvertimeHours = attendance.WorkedHours - (decimal)expectedWorkHours;
            }
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
            .Where(lr => lr.Employee!.BranchId == branchId && !lr.Employee.IsDeleted)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync(int branchId)
    {
        return await GetLeaveRequestsByStatusAsync(branchId, LeaveStatus.Pending);
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(int branchId, LeaveStatus status)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee!.BranchId == branchId && !lr.Employee.IsDeleted && lr.Status == status)
            .OrderByDescending(lr => lr.RequestDate)
            .ToListAsync();
    }

    public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        // Validate dates
        if (leaveRequest.EndDate < leaveRequest.StartDate)
        {
            throw new InvalidOperationException("End date must be on or after start date");
        }

        if (leaveRequest.StartDate.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Cannot create leave requests for past dates");
        }

        // Verify employee exists
        var employee = await _context.Employees.FindAsync(leaveRequest.EmployeeId);
        if (employee == null || employee.IsDeleted)
        {
            throw new KeyNotFoundException($"Employee with ID {leaveRequest.EmployeeId} not found");
        }

        // Calculate total days
        var totalDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
        leaveRequest.TotalDays = totalDays;

        // Validate leave balance for applicable leave types
        if (leaveRequest.LeaveType == LeaveType.Annual || leaveRequest.LeaveType == LeaveType.Sick)
        {
            var hasBalance = await ValidateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveType, totalDays);
            if (!hasBalance)
            {
                throw new InvalidOperationException(
                    $"Insufficient {leaveRequest.LeaveType} leave balance. Requested: {totalDays} days");
            }
        }

        // Check for overlapping approved leave requests
        var overlapping = await _context.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == leaveRequest.EmployeeId &&
                           lr.Status == LeaveStatus.Approved &&
                           lr.StartDate <= leaveRequest.EndDate &&
                           lr.EndDate >= leaveRequest.StartDate);

        if (overlapping)
        {
            throw new InvalidOperationException("Leave request overlaps with an existing approved leave");
        }

        leaveRequest.Status = LeaveStatus.Pending;
        leaveRequest.RequestDate = DateTime.UtcNow;

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        // Check if the leave request exists and its current status
        var existingRequest = await _context.LeaveRequests.FindAsync(leaveRequest.Id);
        if (existingRequest == null)
        {
            throw new KeyNotFoundException($"Leave request with ID {leaveRequest.Id} not found");
        }

        // Cannot update approved or rejected leave requests
        if (existingRequest.Status == LeaveStatus.Approved)
        {
            throw new InvalidOperationException("Cannot update an approved leave request");
        }

        if (existingRequest.Status == LeaveStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot update a rejected leave request");
        }

        // Validate dates if they were changed
        if (leaveRequest.EndDate < leaveRequest.StartDate)
        {
            throw new InvalidOperationException("End date must be on or after start date");
        }

        leaveRequest.UpdatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLeaveRequestAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest != null)
        {
            // Cannot delete approved leave requests
            if (leaveRequest.Status == LeaveStatus.Approved)
            {
                throw new InvalidOperationException("Cannot delete an approved leave request");
            }

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

        // Validate current status allows approval
        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot approve leave request with status '{leaveRequest.Status}'. Only pending requests can be approved.");
        }

        var employee = leaveRequest.Employee;
        if (employee == null || employee.IsDeleted)
        {
            throw new InvalidOperationException("Cannot approve leave for a non-existent or deleted employee");
        }

        var days = leaveRequest.TotalDays > 0
            ? leaveRequest.TotalDays
            : (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

        // Validate leave balance before approval
        if (leaveRequest.LeaveType == LeaveType.Annual)
        {
            if (employee.AnnualLeaveBalance < days)
            {
                throw new InvalidOperationException(
                    $"Insufficient annual leave balance. Available: {employee.AnnualLeaveBalance}, Requested: {days}");
            }
            employee.AnnualLeaveBalance -= days;
        }
        else if (leaveRequest.LeaveType == LeaveType.Sick)
        {
            if (employee.SickLeaveBalance < days)
            {
                throw new InvalidOperationException(
                    $"Insufficient sick leave balance. Available: {employee.SickLeaveBalance}, Requested: {days}");
            }
            employee.SickLeaveBalance -= days;
        }

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedBy = approvedBy;
        leaveRequest.ApprovedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RejectLeaveRequestAsync(int leaveRequestId, string rejectedBy, string reason)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request with ID {leaveRequestId} not found");

        // Validate current status allows rejection
        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot reject leave request with status '{leaveRequest.Status}'. Only pending requests can be rejected.");
        }

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedBy = rejectedBy;
        leaveRequest.ApprovedDate = DateTime.UtcNow;
        leaveRequest.RejectionReason = reason;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateLeaveBalanceAsync(int employeeId, LeaveType leaveType, int requestedDays)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null || employee.IsDeleted)
            return false;

        if (requestedDays <= 0)
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
        // Check for duplicate department name within the same branch
        var existingDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == department.Name && d.BranchId == department.BranchId);

        if (existingDepartment != null)
        {
            throw new InvalidOperationException(
                $"A department with name '{department.Name}' already exists in this branch");
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        // Check for duplicate department name within the same branch (excluding self)
        var existingDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == department.Name &&
                                       d.BranchId == department.BranchId &&
                                       d.Id != department.Id);

        if (existingDepartment != null)
        {
            throw new InvalidOperationException(
                $"A department with name '{department.Name}' already exists in this branch");
        }

        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            // Check if department has any active employees
            var hasEmployees = await _context.Employees
                .AnyAsync(e => e.DepartmentId == id && !e.IsDeleted);

            if (hasEmployees)
            {
                throw new InvalidOperationException(
                    "Cannot delete department with assigned employees. Reassign or remove employees first.");
            }

            // Check if department has any job positions
            var hasJobPositions = await _context.JobPositions
                .AnyAsync(jp => jp.DepartmentId == id);

            if (hasJobPositions)
            {
                throw new InvalidOperationException(
                    "Cannot delete department with assigned job positions. Remove job positions first.");
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
        // Validate salary range
        if (jobPosition.MinSalary < 0)
        {
            throw new InvalidOperationException("Minimum salary cannot be negative");
        }

        if (jobPosition.MaxSalary < jobPosition.MinSalary)
        {
            throw new InvalidOperationException("Maximum salary must be greater than or equal to minimum salary");
        }

        // Check for duplicate job title within the same branch
        var existingPosition = await _context.JobPositions
            .FirstOrDefaultAsync(jp => jp.Title == jobPosition.Title && jp.BranchId == jobPosition.BranchId);

        if (existingPosition != null)
        {
            throw new InvalidOperationException(
                $"A job position with title '{jobPosition.Title}' already exists in this branch");
        }

        _context.JobPositions.Add(jobPosition);
        await _context.SaveChangesAsync();
        return jobPosition;
    }

    public async Task UpdateJobPositionAsync(JobPosition jobPosition)
    {
        // Validate salary range
        if (jobPosition.MinSalary < 0)
        {
            throw new InvalidOperationException("Minimum salary cannot be negative");
        }

        if (jobPosition.MaxSalary < jobPosition.MinSalary)
        {
            throw new InvalidOperationException("Maximum salary must be greater than or equal to minimum salary");
        }

        // Check for duplicate job title within the same branch (excluding self)
        var existingPosition = await _context.JobPositions
            .FirstOrDefaultAsync(jp => jp.Title == jobPosition.Title &&
                                        jp.BranchId == jobPosition.BranchId &&
                                        jp.Id != jobPosition.Id);

        if (existingPosition != null)
        {
            throw new InvalidOperationException(
                $"A job position with title '{jobPosition.Title}' already exists in this branch");
        }

        jobPosition.UpdatedAt = DateTime.UtcNow;
        _context.JobPositions.Update(jobPosition);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteJobPositionAsync(int id)
    {
        var jobPosition = await _context.JobPositions.FindAsync(id);
        if (jobPosition != null)
        {
            // Check if job position has any active employees
            var hasEmployees = await _context.Employees
                .AnyAsync(e => e.JobPositionId == id && !e.IsDeleted);

            if (hasEmployees)
            {
                throw new InvalidOperationException(
                    "Cannot delete job position with assigned employees. Reassign or remove employees first.");
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
            .CountAsync(e => e.BranchId == branchId && !e.IsDeleted);
    }

    public async Task<int> GetActiveEmployeesCountAsync(int branchId)
    {
        return await _context.Employees
            .CountAsync(e => e.BranchId == branchId && !e.IsDeleted && e.EmploymentStatus == EmploymentStatus.Active);
    }

    public async Task<int> GetPresentTodayCountAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        // Count both Present and Late employees (late employees are still present)
        return await _context.Attendances
            .CountAsync(a => a.Employee!.BranchId == branchId &&
                   !a.Employee.IsDeleted &&
                   a.Date.Date == today &&
                   (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));
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
            .CountAsync(lr => lr.Employee!.BranchId == branchId &&
                        !lr.Employee.IsDeleted &&
                        lr.Status == LeaveStatus.Pending);
    }

    public async Task<decimal> GetTotalPayrollAsync(int branchId)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId && !e.IsDeleted && e.EmploymentStatus == EmploymentStatus.Active)
            .SumAsync(e => e.BasicSalary +
                     (e.HousingAllowance ?? 0) +
                     (e.TransportAllowance ?? 0) +
                     (e.OtherAllowances ?? 0));
    }

    public async Task<Dictionary<EmploymentStatus, int>> GetEmployeeStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Employees
            .Where(e => e.BranchId == branchId && !e.IsDeleted)
            .GroupBy(e => e.EmploymentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return distribution;
    }

    public async Task<Dictionary<int, int>> GetEmployeesByDepartmentDistributionAsync(int branchId)
    {
        var distribution = await _context.Employees
            .Where(e => e.BranchId == branchId && !e.IsDeleted && e.EmploymentStatus == EmploymentStatus.Active)
            .GroupBy(e => e.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

        return distribution;
    }

    #endregion
}
