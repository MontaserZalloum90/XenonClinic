using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinancialApiController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly ILogger<FinancialApiController> _logger;

    public FinancialApiController(IFinancialService financialService, ILogger<FinancialApiController> logger)
    {
        _financialService = financialService;
        _logger = logger;
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetAllInvoices()
    {
        try
        {
            var branchId = 1;
            var invoices = await _financialService.GetInvoicesByBranchIdAsync(branchId);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            return StatusCode(500, new { message = "Error retrieving invoices" });
        }
    }

    [HttpGet("invoices/{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        try
        {
            var invoice = await _financialService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = "Invoice not found" });
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice");
            return StatusCode(500, new { message = "Error retrieving invoice" });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = 1;
            var now = DateTime.UtcNow;
            var stats = new
            {
                MonthlyRevenue = await _financialService.GetMonthlyRevenueAsync(branchId, now.Year, now.Month),
                UnpaidInvoices = await _financialService.GetUnpaidInvoicesCountAsync(branchId),
                OverdueInvoices = await _financialService.GetOverdueInvoicesCountAsync(branchId)
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
