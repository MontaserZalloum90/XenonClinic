using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryApiController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryApiController> _logger;

    public InventoryApiController(IInventoryService inventoryService, ILogger<InventoryApiController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetAllItems()
    {
        try
        {
            var branchId = 1;
            var items = await _inventoryService.GetInventoryItemsByBranchIdAsync(branchId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory items");
            return StatusCode(500, new { message = "Error retrieving inventory items" });
        }
    }

    [HttpGet("items/low-stock")]
    public async Task<IActionResult> GetLowStockItems()
    {
        try
        {
            var branchId = 1;
            var items = await _inventoryService.GetLowStockItemsAsync(branchId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items");
            return StatusCode(500, new { message = "Error retrieving low stock items" });
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
                TotalItems = await _inventoryService.GetTotalItemsCountAsync(branchId),
                LowStockItems = await _inventoryService.GetLowStockItemsCountAsync(branchId),
                TotalValue = await _inventoryService.GetTotalInventoryValueAsync(branchId)
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
