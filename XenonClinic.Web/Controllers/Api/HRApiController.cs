using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HRApiController : ControllerBase
{
    private readonly IHRService _hrService;
    private readonly ILogger<HRApiController> _logger;

    public HRApiController(IHRService hrService, ILogger<HRApiController> logger)
    {
        _hrService = hrService;
        _logger = logger;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            var branchId = 1;
            var employees = await _hrService.GetEmployeesByBranchIdAsync(branchId);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new { message = "Error retrieving employees" });
        }
    }

    [HttpGet("employees/{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        try
        {
            var employee = await _hrService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee");
            return StatusCode(500, new { message = "Error retrieving employee" });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = 1;
            var stats = new
            {
                TotalEmployees = await _hrService.GetTotalEmployeesCountAsync(branchId),
                ActiveEmployees = await _hrService.GetActiveEmployeesCountAsync(branchId)
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HR statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
