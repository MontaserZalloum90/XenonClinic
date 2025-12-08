using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LaboratoryApiController : ControllerBase
{
    private readonly ILabService _labService;
    private readonly ILogger<LaboratoryApiController> _logger;

    public LaboratoryApiController(ILabService labService, ILogger<LaboratoryApiController> logger)
    {
        _labService = labService;
        _logger = logger;
    }

    // GET: api/LaboratoryApi/orders
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var branchId = 1;
            var orders = await _labService.GetLabOrdersByBranchIdAsync(branchId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab orders");
            return StatusCode(500, new { message = "Error retrieving lab orders" });
        }
    }

    // GET: api/LaboratoryApi/orders/5
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var order = await _labService.GetLabOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Lab order not found" });

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab order {OrderId}", id);
            return StatusCode(500, new { message = "Error retrieving lab order" });
        }
    }

    // GET: api/LaboratoryApi/orders/pending
    [HttpGet("orders/pending")]
    public async Task<IActionResult> GetPendingOrders()
    {
        try
        {
            var branchId = 1;
            var orders = await _labService.GetPendingLabOrdersAsync(branchId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending lab orders");
            return StatusCode(500, new { message = "Error retrieving pending orders" });
        }
    }

    // GET: api/LaboratoryApi/orders/urgent
    [HttpGet("orders/urgent")]
    public async Task<IActionResult> GetUrgentOrders()
    {
        try
        {
            var branchId = 1;
            var orders = await _labService.GetUrgentLabOrdersAsync(branchId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving urgent lab orders");
            return StatusCode(500, new { message = "Error retrieving urgent orders" });
        }
    }

    // GET: api/LaboratoryApi/orders/patient/5
    [HttpGet("orders/patient/{patientId}")]
    public async Task<IActionResult> GetOrdersByPatient(int patientId)
    {
        try
        {
            var orders = await _labService.GetLabOrdersByPatientIdAsync(patientId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab orders for patient {PatientId}", patientId);
            return StatusCode(500, new { message = "Error retrieving patient orders" });
        }
    }

    // POST: api/LaboratoryApi/orders
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] LabOrder order)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            order.BranchId = 1;
            var createdOrder = await _labService.CreateLabOrderAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab order");
            return StatusCode(500, new { message = "Error creating lab order" });
        }
    }

    // PUT: api/LaboratoryApi/orders/5
    [HttpPut("orders/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] LabOrder order)
    {
        try
        {
            if (id != order.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _labService.UpdateLabOrderAsync(order);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order {OrderId}", id);
            return StatusCode(500, new { message = "Error updating lab order" });
        }
    }

    // POST: api/LaboratoryApi/orders/5/status
    [HttpPost("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            await _labService.UpdateLabOrderStatusAsync(id, request.Status, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order status {OrderId}", id);
            return StatusCode(500, new { message = "Error updating order status" });
        }
    }

    // DELETE: api/LaboratoryApi/orders/5
    [HttpDelete("orders/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            await _labService.DeleteLabOrderAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab order {OrderId}", id);
            return StatusCode(500, new { message = "Error deleting lab order" });
        }
    }

    // GET: api/LaboratoryApi/tests
    [HttpGet("tests")]
    public async Task<IActionResult> GetAllTests()
    {
        try
        {
            var branchId = 1;
            var tests = await _labService.GetActiveLabTestsAsync(branchId);
            return Ok(tests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab tests");
            return StatusCode(500, new { message = "Error retrieving lab tests" });
        }
    }

    // GET: api/LaboratoryApi/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = 1;
            var now = DateTime.UtcNow;

            var statistics = new
            {
                PendingOrders = await _labService.GetPendingOrdersCountAsync(branchId),
                UrgentOrders = await _labService.GetUrgentOrdersCountAsync(branchId),
                MonthlyRevenue = await _labService.GetMonthlyRevenueAsync(branchId, now.Year, now.Month),
                StatusDistribution = await _labService.GetOrderStatusDistributionAsync(branchId)
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    public class UpdateStatusRequest
    {
        public LabOrderStatus Status { get; set; }
    }
}
