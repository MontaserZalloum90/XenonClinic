using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PharmacyApiController : ControllerBase
{
    private readonly IPharmacyService _pharmacyService;
    private readonly ILogger<PharmacyApiController> _logger;

    public PharmacyApiController(IPharmacyService pharmacyService, ILogger<PharmacyApiController> logger)
    {
        _pharmacyService = pharmacyService;
        _logger = logger;
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetAllPrescriptions()
    {
        try
        {
            var branchId = 1;
            var prescriptions = await _pharmacyService.GetPrescriptionsByBranchIdAsync(branchId);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions");
            return StatusCode(500, new { message = "Error retrieving prescriptions" });
        }
    }

    [HttpGet("prescriptions/pending")]
    public async Task<IActionResult> GetPendingPrescriptions()
    {
        try
        {
            var branchId = 1;
            var prescriptions = await _pharmacyService.GetPendingPrescriptionsAsync(branchId);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending prescriptions");
            return StatusCode(500, new { message = "Error retrieving pending prescriptions" });
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
                PendingPrescriptions = await _pharmacyService.GetPendingPrescriptionsCountAsync(branchId),
                DispensedToday = await _pharmacyService.GetDispensedTodayCountAsync(branchId)
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pharmacy statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
