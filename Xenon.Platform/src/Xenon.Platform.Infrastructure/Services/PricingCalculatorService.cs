using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Domain.ValueObjects;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public interface IPricingCalculatorService
{
    Task<IEnumerable<PlanSummary>> GetActivePlansAsync();
    Task<PricingEstimate?> CalculateEstimate(
        PlanCode planCode,
        int branches,
        int users,
        BillingCycle billingCycle,
        Currency currency = Currency.AED);
}

public class PlanSummary
{
    public PlanCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int IncludedBranches { get; set; }
    public int IncludedUsers { get; set; }
    public decimal ExtraBranchPrice { get; set; }
    public decimal ExtraUserPrice { get; set; }
    public string? Features { get; set; }
    public string SupportLevel { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
}

public class PricingCalculatorService : IPricingCalculatorService
{
    private readonly PlatformDbContext _context;

    // Currency conversion rates (AED as base)
    private static readonly Dictionary<Currency, decimal> CurrencyRates = new()
    {
        { Currency.AED, 1m },
        { Currency.USD, 0.27m }
    };

    // Duration discounts
    private static readonly Dictionary<BillingCycle, decimal> DurationDiscounts = new()
    {
        { BillingCycle.Monthly, 0m },
        { BillingCycle.Quarterly, 0.05m },
        { BillingCycle.SemiAnnual, 0.10m },
        { BillingCycle.Annual, 0.15m }
    };

    public PricingCalculatorService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlanSummary>> GetActivePlansAsync()
    {
        return await _context.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PlanSummary
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                AnnualPrice = p.AnnualPrice,
                IncludedBranches = p.IncludedBranches,
                IncludedUsers = p.IncludedUsers,
                ExtraBranchPrice = p.ExtraBranchPrice,
                ExtraUserPrice = p.ExtraUserPrice,
                Features = p.FeaturesJson,
                SupportLevel = p.SupportLevel,
                IsRecommended = p.IsRecommended
            })
            .ToListAsync();
    }

    public async Task<PricingEstimate?> CalculateEstimate(
        PlanCode planCode,
        int branches,
        int users,
        BillingCycle billingCycle,
        Currency currency = Currency.AED)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Code == planCode && p.IsActive);

        if (plan == null)
            return null;

        var currencyRate = CurrencyRates[currency];
        var discountPercent = DurationDiscounts[billingCycle];
        var durationMonths = (int)billingCycle;

        // Calculate extra resources
        var extraBranches = Math.Max(0, branches - plan.IncludedBranches);
        var extraUsers = Math.Max(0, users - plan.IncludedUsers);

        // Monthly prices
        var monthlyBase = plan.MonthlyPrice;
        var monthlyExtraBranches = extraBranches * plan.ExtraBranchPrice;
        var monthlyExtraUsers = extraUsers * plan.ExtraUserPrice;
        var monthlySubtotal = monthlyBase + monthlyExtraBranches + monthlyExtraUsers;

        // Total for duration
        var subtotalForDuration = monthlySubtotal * durationMonths;
        var discountAmount = subtotalForDuration * discountPercent;
        var total = (subtotalForDuration - discountAmount) * currencyRate;

        // Build breakdown
        var breakdown = new List<PricingLineItem>
        {
            new()
            {
                Label = $"{plan.Name} Plan ({durationMonths} months)",
                Amount = monthlyBase * durationMonths * currencyRate
            }
        };

        if (extraBranches > 0)
        {
            breakdown.Add(new PricingLineItem
            {
                Label = $"Extra Branches ({extraBranches} × {durationMonths} months)",
                Amount = monthlyExtraBranches * durationMonths * currencyRate
            });
        }

        if (extraUsers > 0)
        {
            breakdown.Add(new PricingLineItem
            {
                Label = $"Extra Users ({extraUsers} × {durationMonths} months)",
                Amount = monthlyExtraUsers * durationMonths * currencyRate
            });
        }

        if (discountAmount > 0)
        {
            breakdown.Add(new PricingLineItem
            {
                Label = $"Duration Discount ({discountPercent * 100:0}%)",
                Amount = -discountAmount * currencyRate,
                IsDiscount = true
            });
        }

        return new PricingEstimate
        {
            PlanCode = planCode,
            PlanName = plan.Name,
            BillingCycle = billingCycle,
            Currency = currency,
            Branches = branches,
            Users = users,
            IncludedBranches = plan.IncludedBranches,
            IncludedUsers = plan.IncludedUsers,
            ExtraBranches = extraBranches,
            ExtraUsers = extraUsers,
            BasePrice = monthlyBase * durationMonths * currencyRate,
            ExtraBranchesPrice = monthlyExtraBranches * durationMonths * currencyRate,
            ExtraUsersPrice = monthlyExtraUsers * durationMonths * currencyRate,
            Subtotal = subtotalForDuration * currencyRate,
            DiscountPercent = discountPercent * 100,
            DiscountAmount = discountAmount * currencyRate,
            Total = total,
            MonthlyEquivalent = total / durationMonths,
            Breakdown = breakdown
        };
    }
}
