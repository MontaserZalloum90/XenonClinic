using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for Human Resources management operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HRController : BaseApiController
{
    private readonly IHRService _hrService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly IValidator<CreateEmployeeDto> _createEmployeeValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateEmployeeValidator;
    private readonly IValidator<EmployeeListRequestDto> _employeeListValidator;
    private readonly IValidator<CreateDepartmentDto> _createDepartmentValidator;
    private readonly IValidator<UpdateDepartmentDto> _updateDepartmentValidator;
    private readonly IValidator<CreateJobPositionDto> _createJobPositionValidator;
    private readonly IValidator<UpdateJobPositionDto> _updateJobPositionValidator;
    private readonly IValidator<CheckInDto> _checkInValidator;
    private readonly IValidator<CheckOutDto> _checkOutValidator;
    private readonly IValidator<CreateAttendanceDto> _createAttendanceValidator;
    private readonly IValidator<AttendanceListRequestDto> _attendanceListValidator;
    private readonly IValidator<CreateLeaveRequestDto> _createLeaveRequestValidator;
    private readonly IValidator<UpdateLeaveRequestDto> _updateLeaveRequestValidator;
    private readonly IValidator<ApproveLeaveRequestDto> _approveLeaveValidator;
    private readonly IValidator<RejectLeaveRequestDto> _rejectLeaveValidator;
    private readonly IValidator<LeaveRequestListDto> _leaveRequestListValidator;
    private readonly ILogger<HRController> _logger;

    public HRController(
        IHRService hrService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        IValidator<CreateEmployeeDto> createEmployeeValidator,
        IValidator<UpdateEmployeeDto> updateEmployeeValidator,
        IValidator<EmployeeListRequestDto> employeeListValidator,
        IValidator<CreateDepartmentDto> createDepartmentValidator,
        IValidator<UpdateDepartmentDto> updateDepartmentValidator,
        IValidator<CreateJobPositionDto> createJobPositionValidator,
        IValidator<UpdateJobPositionDto> updateJobPositionValidator,
        IValidator<CheckInDto> checkInValidator,
        IValidator<CheckOutDto> checkOutValidator,
        IValidator<CreateAttendanceDto> createAttendanceValidator,
        IValidator<AttendanceListRequestDto> attendanceListValidator,
        IValidator<CreateLeaveRequestDto> createLeaveRequestValidator,
        IValidator<UpdateLeaveRequestDto> updateLeaveRequestValidator,
        IValidator<ApproveLeaveRequestDto> approveLeaveValidator,
        IValidator<RejectLeaveRequestDto> rejectLeaveValidator,
        IValidator<LeaveRequestListDto> leaveRequestListValidator,
        ILogger<HRController> logger)
    {
        _hrService = hrService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _createEmployeeValidator = createEmployeeValidator;
        _updateEmployeeValidator = updateEmployeeValidator;
        _employeeListValidator = employeeListValidator;
        _createDepartmentValidator = createDepartmentValidator;
        _updateDepartmentValidator = updateDepartmentValidator;
        _createJobPositionValidator = createJobPositionValidator;
        _updateJobPositionValidator = updateJobPositionValidator;
        _checkInValidator = checkInValidator;
        _checkOutValidator = checkOutValidator;
        _createAttendanceValidator = createAttendanceValidator;
        _attendanceListValidator = attendanceListValidator;
        _createLeaveRequestValidator = createLeaveRequestValidator;
        _updateLeaveRequestValidator = updateLeaveRequestValidator;
        _approveLeaveValidator = approveLeaveValidator;
        _rejectLeaveValidator = rejectLeaveValidator;
        _leaveRequestListValidator = leaveRequestListValidator;
        _logger = logger;
    }

    #region Employee Management

    /// <summary>
    /// Gets a paginated list of employees for the current branch.
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<EmployeeSearchResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployees([FromQuery] EmployeeListRequestDto request)
    {
        var validationResult = await _employeeListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<Employee> employees;

        if (request.DepartmentId.HasValue)
        {
            employees = await _hrService.GetEmployeesByDepartmentAsync(request.DepartmentId.Value);
        }
        else if (request.JobPositionId.HasValue)
        {
            employees = await _hrService.GetEmployeesByPositionAsync(request.JobPositionId.Value);
        }
        else if (request.Status.HasValue)
        {
            employees = await _hrService.GetEmployeesByStatusAsync(branchId.Value, request.Status.Value);
        }
        else
        {
            employees = await _hrService.GetEmployeesByBranchIdAsync(branchId.Value);
        }

        var query = employees.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(e =>
                e.FullNameEn.ToLowerInvariant().Contains(searchLower) ||
                e.FullNameAr.ToLowerInvariant().Contains(searchLower) ||
                e.EmployeeCode.ToLowerInvariant().Contains(searchLower) ||
                e.EmiratesId.Contains(searchLower));
        }

        // Apply sorting
        query = ApplyEmployeeSorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToSearchResult)
            .ToList();

        var paginatedResult = new PaginatedResponse<EmployeeSearchResultDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    [HttpGet("employees/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _hrService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var dto = MapToDto(employee);
        return ApiOk(dto);
    }

    /// <summary>
    /// Gets an employee by employee code.
    /// </summary>
    [HttpGet("employees/by-code/{employeeCode}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeByCode(string employeeCode)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var employee = await _hrService.GetEmployeeByCodeAsync(employeeCode, branchId.Value);

        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        var dto = MapToDto(employee);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    [HttpPost("employees")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        var validationResult = await _createEmployeeValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        // Generate employee code
        var employeeCode = await _hrService.GenerateEmployeeCodeAsync(branchId.Value);

        var employee = new Employee
        {
            BranchId = branchId.Value,
            EmployeeCode = employeeCode,
            FullNameEn = dto.FullNameEn,
            FullNameAr = dto.FullNameAr,
            EmiratesId = dto.EmiratesId,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Nationality = dto.Nationality,
            PassportNumber = dto.PassportNumber,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            AlternatePhone = dto.AlternatePhone,
            Address = dto.Address,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            DepartmentId = dto.DepartmentId,
            JobPositionId = dto.JobPositionId,
            HireDate = dto.HireDate,
            EmploymentStatus = EmploymentStatus.Active,
            BasicSalary = dto.BasicSalary,
            HousingAllowance = dto.HousingAllowance,
            TransportAllowance = dto.TransportAllowance,
            OtherAllowances = dto.OtherAllowances,
            AnnualLeaveBalance = dto.AnnualLeaveBalance,
            SickLeaveBalance = dto.SickLeaveBalance,
            WorkStartTime = dto.WorkStartTime,
            WorkEndTime = dto.WorkEndTime,
            UserId = dto.UserId,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdEmployee = await _hrService.CreateEmployeeAsync(employee);

        _logger.LogInformation(
            "Employee created: {EmployeeId}, Code: {EmployeeCode}, Branch: {BranchId}, By: {UserId}",
            createdEmployee.Id, createdEmployee.EmployeeCode, branchId, _userContext.UserId);

        var resultDto = MapToDto(createdEmployee);
        return ApiCreated(resultDto, $"/api/hr/employees/{createdEmployee.Id}");
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    [HttpPut("employees/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateEmployeeValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var existingEmployee = await _hrService.GetEmployeeByIdAsync(id);
        if (existingEmployee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(existingEmployee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        // Update employee fields
        existingEmployee.FullNameEn = dto.FullNameEn;
        existingEmployee.FullNameAr = dto.FullNameAr;
        existingEmployee.EmiratesId = dto.EmiratesId;
        existingEmployee.DateOfBirth = dto.DateOfBirth;
        existingEmployee.Gender = dto.Gender;
        existingEmployee.Nationality = dto.Nationality;
        existingEmployee.PassportNumber = dto.PassportNumber;
        existingEmployee.Email = dto.Email;
        existingEmployee.PhoneNumber = dto.PhoneNumber;
        existingEmployee.AlternatePhone = dto.AlternatePhone;
        existingEmployee.Address = dto.Address;
        existingEmployee.EmergencyContactName = dto.EmergencyContactName;
        existingEmployee.EmergencyContactPhone = dto.EmergencyContactPhone;
        existingEmployee.DepartmentId = dto.DepartmentId;
        existingEmployee.JobPositionId = dto.JobPositionId;
        existingEmployee.EmploymentStatus = dto.EmploymentStatus;
        existingEmployee.TerminationDate = dto.TerminationDate;
        existingEmployee.BasicSalary = dto.BasicSalary;
        existingEmployee.HousingAllowance = dto.HousingAllowance;
        existingEmployee.TransportAllowance = dto.TransportAllowance;
        existingEmployee.OtherAllowances = dto.OtherAllowances;
        existingEmployee.AnnualLeaveBalance = dto.AnnualLeaveBalance;
        existingEmployee.SickLeaveBalance = dto.SickLeaveBalance;
        existingEmployee.WorkStartTime = dto.WorkStartTime;
        existingEmployee.WorkEndTime = dto.WorkEndTime;
        existingEmployee.UserId = dto.UserId;
        existingEmployee.Notes = dto.Notes;
        existingEmployee.UpdatedAt = DateTime.UtcNow;
        existingEmployee.UpdatedBy = _userContext.UserId;

        await _hrService.UpdateEmployeeAsync(existingEmployee);

        _logger.LogInformation(
            "Employee updated: {EmployeeId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToDto(existingEmployee);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes an employee (soft delete).
    /// </summary>
    [HttpDelete("employees/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _hrService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        await _hrService.DeleteEmployeeAsync(id);

        _logger.LogInformation(
            "Employee deleted: {EmployeeId}, By: {UserId}",
            id, _userContext.UserId);

        return ApiOk("Employee deleted successfully");
    }

    #endregion

    #region Department Management

    /// <summary>
    /// Gets all departments for the current branch.
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] bool activeOnly = false)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var departments = activeOnly
            ? await _hrService.GetActiveDepartmentsAsync(branchId.Value)
            : await _hrService.GetDepartmentsByBranchIdAsync(branchId.Value);

        var dtos = departments.Select(MapToDepartmentDto);
        return ApiOk(dtos);
    }

    /// <summary>
    /// Gets a department by ID.
    /// </summary>
    [HttpGet("departments/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var department = await _hrService.GetDepartmentByIdAsync(id);

        if (department == null)
        {
            return ApiNotFound(HRValidationMessages.DepartmentNotFound);
        }

        if (!HasBranchAccess(department.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var dto = MapToDepartmentDto(department);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    [HttpPost("departments")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        var validationResult = await _createDepartmentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var department = new Department
        {
            BranchId = branchId.Value,
            Name = dto.Name,
            Description = dto.Description,
            ManagerId = dto.ManagerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdDepartment = await _hrService.CreateDepartmentAsync(department);

        _logger.LogInformation(
            "Department created: {DepartmentId}, Name: {DepartmentName}, Branch: {BranchId}, By: {UserId}",
            createdDepartment.Id, createdDepartment.Name, branchId, _userContext.UserId);

        var resultDto = MapToDepartmentDto(createdDepartment);
        return ApiCreated(resultDto, $"/api/hr/departments/{createdDepartment.Id}");
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    [HttpPut("departments/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateDepartmentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var existingDepartment = await _hrService.GetDepartmentByIdAsync(id);
        if (existingDepartment == null)
        {
            return ApiNotFound(HRValidationMessages.DepartmentNotFound);
        }

        if (!HasBranchAccess(existingDepartment.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        existingDepartment.Name = dto.Name;
        existingDepartment.Description = dto.Description;
        existingDepartment.ManagerId = dto.ManagerId;
        existingDepartment.IsActive = dto.IsActive;
        existingDepartment.UpdatedAt = DateTime.UtcNow;
        existingDepartment.UpdatedBy = _userContext.UserId;

        await _hrService.UpdateDepartmentAsync(existingDepartment);

        _logger.LogInformation(
            "Department updated: {DepartmentId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToDepartmentDto(existingDepartment);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes a department.
    /// </summary>
    [HttpDelete("departments/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _hrService.GetDepartmentByIdAsync(id);
        if (department == null)
        {
            return ApiNotFound(HRValidationMessages.DepartmentNotFound);
        }

        if (!HasBranchAccess(department.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        await _hrService.DeleteDepartmentAsync(id);

        _logger.LogInformation(
            "Department deleted: {DepartmentId}, By: {UserId}",
            id, _userContext.UserId);

        return ApiOk("Department deleted successfully");
    }

    #endregion

    #region Job Position Management

    /// <summary>
    /// Gets all job positions for the current branch.
    /// </summary>
    [HttpGet("job-positions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobPositionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobPositions([FromQuery] int? departmentId = null)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var positions = departmentId.HasValue
            ? await _hrService.GetJobPositionsByDepartmentAsync(departmentId.Value)
            : await _hrService.GetJobPositionsByBranchIdAsync(branchId.Value);

        var dtos = positions.Select(MapToJobPositionDto);
        return ApiOk(dtos);
    }

    /// <summary>
    /// Gets a job position by ID.
    /// </summary>
    [HttpGet("job-positions/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<JobPositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobPosition(int id)
    {
        var position = await _hrService.GetJobPositionByIdAsync(id);

        if (position == null)
        {
            return ApiNotFound(HRValidationMessages.JobPositionNotFound);
        }

        // SECURITY FIX: Add branch access check
        if (!HasBranchAccess(position.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var dto = MapToJobPositionDto(position);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new job position.
    /// </summary>
    [HttpPost("job-positions")]
    [ProducesResponseType(typeof(ApiResponse<JobPositionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJobPosition([FromBody] CreateJobPositionDto dto)
    {
        var validationResult = await _createJobPositionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var position = new JobPosition
        {
            BranchId = branchId.Value,
            Title = dto.Title,
            Description = dto.Description,
            MinSalary = dto.MinSalary,
            MaxSalary = dto.MaxSalary,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdPosition = await _hrService.CreateJobPositionAsync(position);

        _logger.LogInformation(
            "Job position created: {PositionId}, Title: {Title}, Branch: {BranchId}, By: {UserId}",
            createdPosition.Id, createdPosition.Title, branchId, _userContext.UserId);

        var resultDto = MapToJobPositionDto(createdPosition);
        return ApiCreated(resultDto, $"/api/hr/job-positions/{createdPosition.Id}");
    }

    /// <summary>
    /// Updates an existing job position.
    /// </summary>
    [HttpPut("job-positions/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<JobPositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJobPosition(int id, [FromBody] UpdateJobPositionDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateJobPositionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var existingPosition = await _hrService.GetJobPositionByIdAsync(id);
        if (existingPosition == null)
        {
            return ApiNotFound(HRValidationMessages.JobPositionNotFound);
        }

        existingPosition.Title = dto.Title;
        existingPosition.Description = dto.Description;
        existingPosition.MinSalary = dto.MinSalary;
        existingPosition.MaxSalary = dto.MaxSalary;
        existingPosition.IsActive = dto.IsActive;
        existingPosition.UpdatedAt = DateTime.UtcNow;
        existingPosition.UpdatedBy = _userContext.UserId;

        await _hrService.UpdateJobPositionAsync(existingPosition);

        _logger.LogInformation(
            "Job position updated: {PositionId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToJobPositionDto(existingPosition);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes a job position.
    /// </summary>
    [HttpDelete("job-positions/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJobPosition(int id)
    {
        var position = await _hrService.GetJobPositionByIdAsync(id);
        if (position == null)
        {
            return ApiNotFound(HRValidationMessages.JobPositionNotFound);
        }

        // SECURITY FIX: Add branch access check
        if (!HasBranchAccess(position.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        await _hrService.DeleteJobPositionAsync(id);

        _logger.LogInformation(
            "Job position deleted: {PositionId}, By: {UserId}",
            id, _userContext.UserId);

        return ApiOk("Job position deleted successfully");
    }

    #endregion

    #region Attendance Management

    /// <summary>
    /// Gets attendance records for the current branch.
    /// </summary>
    [HttpGet("attendance")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance([FromQuery] AttendanceListRequestDto request)
    {
        var validationResult = await _attendanceListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<Attendance> attendance;
        var dateFrom = request.DateFrom ?? DateTime.UtcNow.Date.AddDays(-30);
        var dateTo = request.DateTo ?? DateTime.UtcNow.Date;

        if (request.EmployeeId.HasValue)
        {
            attendance = await _hrService.GetAttendanceByEmployeeIdAsync(
                request.EmployeeId.Value, dateFrom, dateTo);
        }
        else
        {
            attendance = await _hrService.GetAttendanceByBranchIdAsync(branchId.Value, dateFrom);
        }

        var query = attendance.AsQueryable();

        // Apply status filter
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // Apply sorting
        query = request.SortDescending
            ? query.OrderByDescending(a => a.Date)
            : query.OrderBy(a => a.Date);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToAttendanceDto)
            .ToList();

        var paginatedResult = new PaginatedResponse<AttendanceDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets today's attendance for the current branch.
    /// </summary>
    [HttpGet("attendance/today")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayAttendance()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var attendance = await _hrService.GetTodayAttendanceAsync(branchId.Value);
        var dtos = attendance.Select(MapToAttendanceDto);

        return ApiOk(dtos);
    }

    /// <summary>
    /// Records employee check-in.
    /// </summary>
    [HttpPost("attendance/check-in")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
    {
        var validationResult = await _checkInValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var employee = await _hrService.GetEmployeeByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var attendance = await _hrService.CheckInAsync(dto.EmployeeId, dto.CheckInTime);

        _logger.LogInformation(
            "Employee checked in: {EmployeeId}, Time: {CheckInTime}, By: {UserId}",
            dto.EmployeeId, dto.CheckInTime, _userContext.UserId);

        var resultDto = MapToAttendanceDto(attendance);
        return ApiCreated(resultDto, $"/api/hr/attendance/{attendance.Id}");
    }

    /// <summary>
    /// Records employee check-out.
    /// </summary>
    [HttpPost("attendance/check-out")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
    {
        var validationResult = await _checkOutValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var attendance = await _hrService.GetAttendanceByIdAsync(dto.AttendanceId);
        if (attendance == null)
        {
            return ApiNotFound(HRValidationMessages.AttendanceNotFound);
        }

        // SECURITY FIX: Verify branch access via employee
        var employee = await _hrService.GetEmployeeByIdAsync(attendance.EmployeeId);
        if (employee != null && !HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        // BUG FIX: Validate checkout time is after checkin time
        if (attendance.CheckInTime.HasValue && dto.CheckOutTime < attendance.CheckInTime.Value)
        {
            return ApiBadRequest("Check-out time cannot be before check-in time");
        }

        var updatedAttendance = await _hrService.CheckOutAsync(dto.AttendanceId, dto.CheckOutTime);

        _logger.LogInformation(
            "Employee checked out: AttendanceId: {AttendanceId}, Time: {CheckOutTime}, By: {UserId}",
            dto.AttendanceId, dto.CheckOutTime, _userContext.UserId);

        var resultDto = MapToAttendanceDto(updatedAttendance);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Creates a manual attendance record.
    /// </summary>
    [HttpPost("attendance")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAttendance([FromBody] CreateAttendanceDto dto)
    {
        var validationResult = await _createAttendanceValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var employee = await _hrService.GetEmployeeByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdAttendance = await _hrService.CreateAttendanceAsync(attendance);

        _logger.LogInformation(
            "Manual attendance created: {AttendanceId}, Employee: {EmployeeId}, By: {UserId}",
            createdAttendance.Id, dto.EmployeeId, _userContext.UserId);

        var resultDto = MapToAttendanceDto(createdAttendance);
        return ApiCreated(resultDto, $"/api/hr/attendance/{createdAttendance.Id}");
    }

    #endregion

    #region Leave Request Management

    /// <summary>
    /// Gets leave requests for the current branch.
    /// </summary>
    [HttpGet("leave-requests")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LeaveRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] LeaveRequestListDto request)
    {
        var validationResult = await _leaveRequestListValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<LeaveRequest> leaveRequests;

        if (request.EmployeeId.HasValue)
        {
            leaveRequests = await _hrService.GetLeaveRequestsByEmployeeIdAsync(request.EmployeeId.Value);
        }
        else if (request.Status.HasValue)
        {
            leaveRequests = await _hrService.GetLeaveRequestsByStatusAsync(branchId.Value,
                MapToLeaveRequestStatus(request.Status.Value));
        }
        else
        {
            leaveRequests = await _hrService.GetLeaveRequestsByBranchIdAsync(branchId.Value);
        }

        var query = leaveRequests.AsQueryable();

        // Apply filters
        if (request.LeaveType.HasValue)
        {
            query = query.Where(l => l.LeaveType == request.LeaveType.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(l => l.StartDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(l => l.EndDate <= request.DateTo.Value);
        }

        // Apply sorting
        query = request.SortDescending
            ? query.OrderByDescending(l => l.RequestDate)
            : query.OrderBy(l => l.RequestDate);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToLeaveRequestDto)
            .ToList();

        var paginatedResult = new PaginatedResponse<LeaveRequestDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets pending leave requests for the current branch.
    /// </summary>
    [HttpGet("leave-requests/pending")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingLeaveRequests()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var leaveRequests = await _hrService.GetPendingLeaveRequestsAsync(branchId.Value);
        var dtos = leaveRequests.Select(MapToLeaveRequestDto);

        return ApiOk(dtos);
    }

    /// <summary>
    /// Gets a leave request by ID.
    /// </summary>
    [HttpGet("leave-requests/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveRequest(int id)
    {
        var leaveRequest = await _hrService.GetLeaveRequestByIdAsync(id);

        if (leaveRequest == null)
        {
            return ApiNotFound(HRValidationMessages.LeaveRequestNotFound);
        }

        // SECURITY FIX: Verify branch access via employee
        var employee = await _hrService.GetEmployeeByIdAsync(leaveRequest.EmployeeId);
        if (employee != null && !HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var dto = MapToLeaveRequestDto(leaveRequest);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new leave request.
    /// </summary>
    [HttpPost("leave-requests")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var validationResult = await _createLeaveRequestValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var employee = await _hrService.GetEmployeeByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ApiNotFound(HRValidationMessages.EmployeeNotFound);
        }

        if (!HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        var totalDays = (dto.EndDate - dto.StartDate).Days + 1;

        // Validate leave balance
        var hasBalance = await _hrService.ValidateLeaveBalanceAsync(
            dto.EmployeeId, dto.LeaveType, totalDays);

        if (!hasBalance)
        {
            return ApiBadRequest(HRValidationMessages.LeaveInsufficientBalance);
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            LeaveType = dto.LeaveType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            AttachmentPath = dto.AttachmentPath,
            RequestDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdLeaveRequest = await _hrService.CreateLeaveRequestAsync(leaveRequest);

        _logger.LogInformation(
            "Leave request created: {LeaveRequestId}, Employee: {EmployeeId}, Type: {LeaveType}, By: {UserId}",
            createdLeaveRequest.Id, dto.EmployeeId, dto.LeaveType, _userContext.UserId);

        var resultDto = MapToLeaveRequestDto(createdLeaveRequest);
        return ApiCreated(resultDto, $"/api/hr/leave-requests/{createdLeaveRequest.Id}");
    }

    /// <summary>
    /// Approves a leave request.
    /// </summary>
    [HttpPost("leave-requests/{id:int}/approve")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] ApproveLeaveRequestDto dto)
    {
        dto.LeaveRequestId = id;

        var validationResult = await _approveLeaveValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var leaveRequest = await _hrService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiNotFound(HRValidationMessages.LeaveRequestNotFound);
        }

        // SECURITY FIX: Verify branch access before allowing approval
        var employee = await _hrService.GetEmployeeByIdAsync(leaveRequest.EmployeeId);
        if (employee != null && !HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return ApiBadRequest(HRValidationMessages.LeaveAlreadyApproved);
        }

        // BUG FIX: Use RequireUserId() to ensure audit trail integrity
        await _hrService.ApproveLeaveRequestAsync(id, _userContext.RequireUserId());

        _logger.LogInformation(
            "Leave request approved: {LeaveRequestId}, By: {UserId}",
            id, _userContext.UserId);

        var updatedRequest = await _hrService.GetLeaveRequestByIdAsync(id);
        var resultDto = MapToLeaveRequestDto(updatedRequest!);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Rejects a leave request.
    /// </summary>
    [HttpPost("leave-requests/{id:int}/reject")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectLeaveRequest(int id, [FromBody] RejectLeaveRequestDto dto)
    {
        dto.LeaveRequestId = id;

        var validationResult = await _rejectLeaveValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest(validationResult.Errors);
        }

        var leaveRequest = await _hrService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiNotFound(HRValidationMessages.LeaveRequestNotFound);
        }

        // SECURITY FIX: Verify branch access before allowing rejection
        var employee = await _hrService.GetEmployeeByIdAsync(leaveRequest.EmployeeId);
        if (employee != null && !HasBranchAccess(employee.BranchId))
        {
            throw new ForbiddenException(HRValidationMessages.BranchAccessDenied);
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return ApiBadRequest(HRValidationMessages.LeaveAlreadyRejected);
        }

        // BUG FIX: Use RequireUserId() to ensure audit trail integrity
        await _hrService.RejectLeaveRequestAsync(id, _userContext.RequireUserId(), dto.RejectionReason);

        _logger.LogInformation(
            "Leave request rejected: {LeaveRequestId}, By: {UserId}",
            id, _userContext.UserId);

        var updatedRequest = await _hrService.GetLeaveRequestByIdAsync(id);
        var resultDto = MapToLeaveRequestDto(updatedRequest!);
        return ApiOk(resultDto);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets HR statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<HRStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var statistics = new HRStatisticsDto
        {
            TotalEmployees = await _hrService.GetTotalEmployeesCountAsync(branchId.Value),
            ActiveEmployees = await _hrService.GetActiveEmployeesCountAsync(branchId.Value),
            PresentToday = await _hrService.GetPresentTodayCountAsync(branchId.Value),
            AbsentToday = await _hrService.GetAbsentTodayCountAsync(branchId.Value),
            PendingLeaveRequests = await _hrService.GetPendingLeaveRequestsCountAsync(branchId.Value),
            TotalPayroll = await _hrService.GetTotalPayrollAsync(branchId.Value),
            EmploymentStatusDistribution = (await _hrService.GetEmployeeStatusDistributionAsync(branchId.Value))
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            DepartmentDistribution = (await _hrService.GetEmployeesByDepartmentDistributionAsync(branchId.Value))
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
        };

        return ApiOk(statistics);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentBranchId() => _tenantContext.BranchId;

    private bool HasBranchAccess(int branchId)
    {
        if (_tenantContext.IsCompanyAdmin)
        {
            return true;
        }
        return _tenantContext.HasBranchAccess(branchId);
    }

    private static IQueryable<Employee> ApplyEmployeeSorting(
        IQueryable<Employee> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? query.OrderByDescending(e => e.EmployeeCode)
                : query.OrderBy(e => e.EmployeeCode),
            "fullnamear" => descending
                ? query.OrderByDescending(e => e.FullNameAr)
                : query.OrderBy(e => e.FullNameAr),
            "department" => descending
                ? query.OrderByDescending(e => e.Department.Name)
                : query.OrderBy(e => e.Department.Name),
            "hiredate" => descending
                ? query.OrderByDescending(e => e.HireDate)
                : query.OrderBy(e => e.HireDate),
            "status" => descending
                ? query.OrderByDescending(e => e.EmploymentStatus)
                : query.OrderBy(e => e.EmploymentStatus),
            _ => descending
                ? query.OrderByDescending(e => e.FullNameEn)
                : query.OrderBy(e => e.FullNameEn)
        };
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.Name,
            EmployeeCode = employee.EmployeeCode,
            FullNameEn = employee.FullNameEn,
            FullNameAr = employee.FullNameAr,
            EmiratesId = employee.EmiratesId,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            Nationality = employee.Nationality,
            PassportNumber = employee.PassportNumber,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            AlternatePhone = employee.AlternatePhone,
            Address = employee.Address,
            EmergencyContactName = employee.EmergencyContactName,
            EmergencyContactPhone = employee.EmergencyContactPhone,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name,
            JobPositionId = employee.JobPositionId,
            JobPositionTitle = employee.JobPosition?.Title,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            EmploymentStatus = employee.EmploymentStatus,
            BasicSalary = employee.BasicSalary,
            HousingAllowance = employee.HousingAllowance,
            TransportAllowance = employee.TransportAllowance,
            OtherAllowances = employee.OtherAllowances,
            AnnualLeaveBalance = employee.AnnualLeaveBalance,
            SickLeaveBalance = employee.SickLeaveBalance,
            WorkStartTime = employee.WorkStartTime,
            WorkEndTime = employee.WorkEndTime,
            UserId = employee.UserId,
            ProfilePicturePath = employee.ProfilePicturePath,
            Notes = employee.Notes,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            CreatedBy = employee.CreatedBy,
            UpdatedAt = employee.UpdatedAt,
            UpdatedBy = employee.UpdatedBy
        };
    }

    private static EmployeeSearchResultDto MapToSearchResult(Employee employee)
    {
        return new EmployeeSearchResultDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullNameEn = employee.FullNameEn,
            FullNameAr = employee.FullNameAr,
            DepartmentName = employee.Department?.Name,
            JobPositionTitle = employee.JobPosition?.Title,
            EmploymentStatus = employee.EmploymentStatus,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email
        };
    }

    private static DepartmentDto MapToDepartmentDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            BranchId = department.BranchId,
            BranchName = department.Branch?.Name,
            Name = department.Name,
            Description = department.Description,
            ManagerId = department.ManagerId,
            ManagerName = department.Manager?.FullNameEn,
            IsActive = department.IsActive,
            EmployeeCount = department.Employees?.Count ?? 0,
            CreatedAt = department.CreatedAt,
            CreatedBy = department.CreatedBy,
            UpdatedAt = department.UpdatedAt,
            UpdatedBy = department.UpdatedBy
        };
    }

    private static JobPositionDto MapToJobPositionDto(JobPosition position)
    {
        return new JobPositionDto
        {
            Id = position.Id,
            BranchId = position.BranchId,
            Title = position.Title,
            Description = position.Description,
            MinSalary = position.MinSalary,
            MaxSalary = position.MaxSalary,
            IsActive = position.IsActive,
            EmployeeCount = position.Employees?.Count ?? 0,
            CreatedAt = position.CreatedAt,
            CreatedBy = position.CreatedBy
        };
    }

    private static AttendanceDto MapToAttendanceDto(Attendance attendance)
    {
        return new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = attendance.Employee?.FullNameEn,
            EmployeeCode = attendance.Employee?.EmployeeCode,
            Date = attendance.Date,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            Status = attendance.Status,
            IsLate = attendance.IsLate,
            LateMinutes = attendance.LateMinutes,
            WorkedHours = attendance.WorkedHours,
            OvertimeHours = attendance.OvertimeHours,
            Notes = attendance.Notes,
            CreatedAt = attendance.CreatedAt,
            CreatedBy = attendance.CreatedBy
        };
    }

    private static LeaveRequestDto MapToLeaveRequestDto(LeaveRequest leaveRequest)
    {
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = leaveRequest.Employee?.FullNameEn,
            EmployeeCode = leaveRequest.Employee?.EmployeeCode,
            DepartmentName = leaveRequest.Employee?.Department?.Name,
            LeaveType = leaveRequest.LeaveType,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            TotalDays = leaveRequest.TotalDays,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedDate = leaveRequest.ApprovedDate,
            RejectionReason = leaveRequest.RejectionReason,
            AttachmentPath = leaveRequest.AttachmentPath,
            RequestDate = leaveRequest.RequestDate,
            IsActive = leaveRequest.IsActive,
            CreatedAt = leaveRequest.CreatedAt,
            CreatedBy = leaveRequest.CreatedBy
        };
    }

    private static LeaveRequestStatus MapToLeaveRequestStatus(LeaveStatus status)
    {
        return status switch
        {
            LeaveStatus.Pending => LeaveRequestStatus.Pending,
            LeaveStatus.Approved => LeaveRequestStatus.Approved,
            LeaveStatus.Rejected => LeaveRequestStatus.Rejected,
            LeaveStatus.Cancelled => LeaveRequestStatus.Cancelled,
            _ => LeaveRequestStatus.Pending
        };
    }

    #endregion
}
