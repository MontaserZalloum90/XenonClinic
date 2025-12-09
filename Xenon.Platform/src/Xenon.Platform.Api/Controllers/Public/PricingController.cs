using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.Public;

[ApiController]
[Route("api/public/pricing")]
public class PricingController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly IPricingCalculatorService _pricingService;

    public PricingController(PlatformDbContext context, IPricingCalculatorService pricingService)
    {
        _context = context;
        _pricingService = pricingService;
    }

    /// <summary>
    /// Get all available pricing plans
    /// </summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _context.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Code,
                p.Name,
                p.Description,
                p.MonthlyPrice,
                p.AnnualPrice,
                p.IncludedBranches,
                p.IncludedUsers,
                p.ExtraBranchPrice,
                p.ExtraUserPrice,
                Features = p.FeaturesJson,
                p.SupportLevel,
                p.IsRecommended
            })
            .ToListAsync();

        return Ok(new { success = true, data = plans });
    }

    /// <summary>
    /// Calculate pricing estimate
    /// </summary>
    [HttpGet("estimate")]
    public async Task<IActionResult> GetEstimate(
        [FromQuery] string planId,
        [FromQuery] int branches = 1,
        [FromQuery] int users = 5,
        [FromQuery] string billingCycle = "Monthly",
        [FromQuery] string currency = "AED")
    {
        if (!Enum.TryParse<PlanCode>(planId, true, out var planCode))
        {
            return BadRequest(new { success = false, error = "Invalid plan code" });
        }

        if (!Enum.TryParse<BillingCycle>(billingCycle, true, out var cycle))
        {
            return BadRequest(new { success = false, error = "Invalid billing cycle" });
        }

        if (!Enum.TryParse<Currency>(currency, true, out var curr))
        {
            return BadRequest(new { success = false, error = "Invalid currency" });
        }

        var estimate = await _pricingService.CalculateEstimate(planCode, branches, users, cycle, curr);

        if (estimate == null)
        {
            return NotFound(new { success = false, error = "Plan not found" });
        }

        return Ok(new { success = true, data = estimate });
    }

    /// <summary>
    /// Get duration discounts
    /// </summary>
    [HttpGet("discounts")]
    public IActionResult GetDiscounts()
    {
        var discounts = new[]
        {
            new { cycle = "Monthly", months = 1, discountPercent = 0 },
            new { cycle = "Quarterly", months = 3, discountPercent = 5 },
            new { cycle = "SemiAnnual", months = 6, discountPercent = 10 },
            new { cycle = "Annual", months = 12, discountPercent = 15 }
        };

        return Ok(new { success = true, data = discounts });
    }
}
