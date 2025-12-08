using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class HRController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HRController> _logger;

    public HRController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        UserManager<ApplicationUser> userManager,
        ILogger<HRController> logger)
    {
        _db = db;
        _branchService = branchService;
        _userManager = userManager;
        _logger = logger;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Any())
            {
                _logger.LogWarning("User has no assigned branches for HR access");
                return View("Error");
            }

            var today = DateTime.UtcNow.Date;
            var employeesQuery = _db.Employees
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .Include(e => e.JobPosition)
                .Where(e => branchIds.Contains(e.BranchId));

            var model = new HRDashboardViewModel
            {
                TotalEmployees = await employeesQuery.CountAsync(),
                ActiveEmployees = await employeesQuery.CountAsync(e => e.EmploymentStatus == EmploymentStatus.Active),
                PresentToday = await _db.Attendances
                    .Where(a => a.Date == today && a.Status == AttendanceStatus.Present &&
                                branchIds.Contains(a.Employee.BranchId))
                    .CountAsync(),
                OnLeaveToday = await _db.LeaveRequests
                    .Where(l => l.Status == LeaveStatus.Approved &&
                                l.StartDate <= today && l.EndDate >= today &&
                                branchIds.Contains(l.Employee.BranchId))
                    .CountAsync(),
                PendingLeaveRequests = await _db.LeaveRequests
                    .Where(l => l.Status == LeaveStatus.Pending &&
                                branchIds.Contains(l.Employee.BranchId))
                    .CountAsync(),

                RecentHires = await employeesQuery
                    .OrderByDescending(e => e.HireDate)
                    .Take(5)
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        EmployeeCode = e.EmployeeCode,
                        FullNameEn = e.FullNameEn,
                        FullNameAr = e.FullNameAr,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        DepartmentName = e.Department.Name,
                        JobPositionTitle = e.JobPosition.Title,
                        HireDate = e.HireDate,
                        EmploymentStatus = e.EmploymentStatus,
                        EmploymentStatusDisplay = e.EmploymentStatus.ToString()
                    })
                    .ToListAsync(),

                PendingLeaves = await _db.LeaveRequests
                    .Include(l => l.Employee)
                    .Where(l => l.Status == LeaveStatus.Pending &&
                                branchIds.Contains(l.Employee.BranchId))
                    .OrderBy(l => l.StartDate)
                    .Take(10)
                    .Select(l => new LeaveRequestDto
                    {
                        Id = l.Id,
                        EmployeeId = l.EmployeeId,
                        EmployeeName = l.Employee.FullNameEn,
                        EmployeeCode = l.Employee.EmployeeCode,
                        LeaveType = l.LeaveType,
                        LeaveTypeDisplay = l.LeaveType.ToString(),
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        TotalDays = l.TotalDays,
                        Reason = l.Reason,
                        Status = l.Status,
                        StatusDisplay = l.Status.ToString(),
                        RequestDate = l.RequestDate
                    })
                    .ToListAsync(),

                TodayAttendance = await _db.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.Date == today &&
                                branchIds.Contains(a.Employee.BranchId))
                    .OrderBy(a => a.Employee.FullNameEn)
                    .Select(a => new AttendanceDto
                    {
                        Id = a.Id,
                        EmployeeId = a.EmployeeId,
                        EmployeeName = a.Employee.FullNameEn,
                        EmployeeCode = a.Employee.EmployeeCode,
                        Date = a.Date,
                        CheckInTime = a.CheckInTime,
                        CheckOutTime = a.CheckOutTime,
                        Status = a.Status,
                        StatusDisplay = a.Status.ToString(),
                        IsLate = a.IsLate,
                        WorkedHours = a.WorkedHours
                    })
                    .ToListAsync()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading HR dashboard");
            return View("Error");
        }
    }

    // Employees List
    public async Task<IActionResult> Employees(string? search, int? departmentId, EmploymentStatus? status)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var employeesQuery = _db.Employees
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .Include(e => e.JobPosition)
                .Where(e => branchIds.Contains(e.BranchId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                employeesQuery = employeesQuery.Where(e =>
                    e.EmployeeCode.Contains(search) ||
                    e.FullNameEn.Contains(search) ||
                    e.EmiratesId.Contains(search));
            }

            if (departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (status.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.EmploymentStatus == status.Value);
            }

            var employees = await employeesQuery
                .OrderBy(e => e.FullNameEn)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullNameEn = e.FullNameEn,
                    FullNameAr = e.FullNameAr,
                    EmiratesId = e.EmiratesId,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    BranchName = e.Branch.Name,
                    DepartmentName = e.Department.Name,
                    JobPositionTitle = e.JobPosition.Title,
                    HireDate = e.HireDate,
                    EmploymentStatus = e.EmploymentStatus,
                    EmploymentStatusDisplay = e.EmploymentStatus.ToString(),
                    TotalSalary = e.TotalSalary,
                    YearsOfService = e.YearsOfService,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            await PopulateDepartmentFilterAsync(branchIds);
            ViewBag.Search = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Status = status;
            return View(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading employees");
            return View("Error");
        }
    }

    // Create Employee - GET
    public async Task<IActionResult> CreateEmployee()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    // Create Employee - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        try
        {
            if (!await _branchService.HasAccessToBranchAsync(model.BranchId))
            {
                return Forbid();
            }

            if (await _db.Employees.AnyAsync(e => e.EmployeeCode == model.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", "Employee code already exists");
                await PopulateDropdownsAsync();
                return View(model);
            }

            if (await _db.Employees.AnyAsync(e => e.EmiratesId == model.EmiratesId))
            {
                ModelState.AddModelError("EmiratesId", "Emirates ID already exists");
                await PopulateDropdownsAsync();
                return View(model);
            }

            var employee = new Employee
            {
                EmployeeCode = model.EmployeeCode,
                FullNameEn = model.FullNameEn,
                FullNameAr = model.FullNameAr,
                EmiratesId = model.EmiratesId,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Nationality = model.Nationality,
                PassportNumber = model.PassportNumber,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                AlternatePhone = model.AlternatePhone,
                Address = model.Address,
                EmergencyContactName = model.EmergencyContactName,
                EmergencyContactPhone = model.EmergencyContactPhone,
                BranchId = model.BranchId,
                DepartmentId = model.DepartmentId,
                JobPositionId = model.JobPositionId,
                HireDate = model.HireDate,
                BasicSalary = model.BasicSalary,
                HousingAllowance = model.HousingAllowance,
                TransportAllowance = model.TransportAllowance,
                OtherAllowances = model.OtherAllowances,
                WorkStartTime = model.WorkStartTime,
                WorkEndTime = model.WorkEndTime,
                Notes = model.Notes,
                EmploymentStatus = EmploymentStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee created: {EmployeeCode} - {Name}", employee.EmployeeCode, employee.FullNameEn);
            return RedirectToAction(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            ModelState.AddModelError("", "An error occurred while creating the employee");
            await PopulateDropdownsAsync();
            return View(model);
        }
    }

    // Leave Requests List
    public async Task<IActionResult> LeaveRequests(LeaveStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var leaveQuery = _db.LeaveRequests
                .Include(l => l.Employee)
                .Where(l => branchIds.Contains(l.Employee.BranchId));

            if (status.HasValue)
            {
                leaveQuery = leaveQuery.Where(l => l.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                leaveQuery = leaveQuery.Where(l => l.StartDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                leaveQuery = leaveQuery.Where(l => l.EndDate <= toDate.Value);
            }

            var leaves = await leaveQuery
                .OrderByDescending(l => l.RequestDate)
                .Select(l => new LeaveRequestDto
                {
                    Id = l.Id,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = l.Employee.FullNameEn,
                    EmployeeCode = l.Employee.EmployeeCode,
                    LeaveType = l.LeaveType,
                    LeaveTypeDisplay = l.LeaveType.ToString(),
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = l.Status,
                    StatusDisplay = l.Status.ToString(),
                    ApprovedBy = l.ApprovedBy,
                    ApprovedDate = l.ApprovedDate,
                    RejectionReason = l.RejectionReason,
                    RequestDate = l.RequestDate,
                    IsActive = l.IsActive
                })
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(leaves);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading leave requests");
            return View("Error");
        }
    }

    // Create Leave Request - GET
    public async Task<IActionResult> CreateLeaveRequest(int? employeeId)
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();
        ViewBag.Employees = await _db.Employees
            .Where(e => branchIds.Contains(e.BranchId) && e.EmploymentStatus == EmploymentStatus.Active)
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.EmployeeCode} - {e.FullNameEn}"
            })
            .ToListAsync();

        var model = new CreateLeaveRequestViewModel();
        if (employeeId.HasValue)
        {
            model.EmployeeId = employeeId.Value;
        }

        return View(model);
    }

    // Create Leave Request - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLeaveRequest(CreateLeaveRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeeDropdownAsync();
            return View(model);
        }

        try
        {
            var employee = await _db.Employees.FindAsync(model.EmployeeId);
            if (employee == null)
            {
                ModelState.AddModelError("", "Employee not found");
                await PopulateEmployeeDropdownAsync();
                return View(model);
            }

            if (!await _branchService.HasAccessToBranchAsync(employee.BranchId))
            {
                return Forbid();
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
                await PopulateEmployeeDropdownAsync();
                return View(model);
            }

            var totalDays = (model.EndDate - model.StartDate).Days + 1;

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = model.EmployeeId,
                LeaveType = model.LeaveType,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalDays = totalDays,
                Reason = model.Reason,
                Status = LeaveStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _db.LeaveRequests.Add(leaveRequest);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Leave request created for employee: {EmployeeCode}", employee.EmployeeCode);
            return RedirectToAction(nameof(LeaveRequests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            ModelState.AddModelError("", "An error occurred while creating the leave request");
            await PopulateEmployeeDropdownAsync();
            return View(model);
        }
    }

    // Approve Leave
    [HttpPost]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        try
        {
            var leave = await _db.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return NotFound();

            if (!await _branchService.HasAccessToBranchAsync(leave.Employee.BranchId))
            {
                return Forbid();
            }

            var user = await _userManager.GetUserAsync(User);
            leave.Status = LeaveStatus.Approved;
            leave.ApprovedBy = user?.Email;
            leave.ApprovedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(LeaveRequests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving leave");
            return RedirectToAction(nameof(LeaveRequests));
        }
    }

    // Reject Leave
    [HttpPost]
    public async Task<IActionResult> RejectLeave(int id, string reason)
    {
        try
        {
            var leave = await _db.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return NotFound();

            if (!await _branchService.HasAccessToBranchAsync(leave.Employee.BranchId))
            {
                return Forbid();
            }

            var user = await _userManager.GetUserAsync(User);
            leave.Status = LeaveStatus.Rejected;
            leave.ApprovedBy = user?.Email;
            leave.ApprovedDate = DateTime.UtcNow;
            leave.RejectionReason = reason;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(LeaveRequests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting leave");
            return RedirectToAction(nameof(LeaveRequests));
        }
    }

    // Helper methods
    private async Task PopulateDropdownsAsync()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();

        ViewBag.Branches = await _db.Branches
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
            .ToListAsync();

        ViewBag.Departments = await _db.Departments
            .Where(d => branchIds.Contains(d.BranchId) && d.IsActive)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToListAsync();

        ViewBag.JobPositions = await _db.JobPositions
            .Where(j => j.IsActive)
            .Select(j => new SelectListItem { Value = j.Id.ToString(), Text = j.Title })
            .ToListAsync();
    }

    private async Task PopulateDepartmentFilterAsync(List<int> branchIds)
    {
        ViewBag.Departments = await _db.Departments
            .Where(d => branchIds.Contains(d.BranchId) && d.IsActive)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToListAsync();
    }

    private async Task PopulateEmployeeDropdownAsync()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();
        ViewBag.Employees = await _db.Employees
            .Where(e => branchIds.Contains(e.BranchId) && e.EmploymentStatus == EmploymentStatus.Active)
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.EmployeeCode} - {e.FullNameEn}"
            })
            .ToListAsync();
    }
}
