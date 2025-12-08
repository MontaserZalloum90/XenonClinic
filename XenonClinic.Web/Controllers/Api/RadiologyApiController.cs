using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RadiologyApiController : ControllerBase
{
    private readonly IRadiologyService _radiologyService;
    private readonly ILogger<RadiologyApiController> _logger;

    public RadiologyApiController(IRadiologyService radiologyService, ILogger<RadiologyApiController> logger)
    {
        _radiologyService = radiologyService;
        _logger = logger;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var branchId = 1;
            var orders = await _radiologyService.GetRadiologyOrdersByBranchIdAsync(branchId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving radiology orders");
            return StatusCode(500, new { message = "Error retrieving radiology orders" });
        }
    }

    [HttpGet("orders/pending")]
    public async Task<IActionResult> GetPendingOrders()
    {
        try
        {
            var branchId = 1;
            var orders = await _radiologyService.GetPendingOrdersAsync(branchId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending orders");
            return StatusCode(500, new { message = "Error retrieving pending orders" });
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
                PendingOrders = await _radiologyService.GetPendingOrdersCountAsync(branchId),
                CompletedToday = await _radiologyService.GetCompletedTodayCountAsync(branchId)
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving radiology statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
