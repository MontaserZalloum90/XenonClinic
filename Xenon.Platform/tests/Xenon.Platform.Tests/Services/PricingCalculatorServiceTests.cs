using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for PricingCalculatorService - validates pricing calculations,
/// currency conversion, billing cycle discounts, and extra resource pricing.
/// </summary>
public class PricingCalculatorServiceTests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly PricingCalculatorService _service;

    public PricingCalculatorServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);
        _service = new PricingCalculatorService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var plans = new List<Plan>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = PlanCode.Starter,
                Name = "Starter",
                Description = "For small clinics",
                MonthlyPrice = 500m,
                AnnualPrice = 5100m,
                IncludedBranches = 1,
                IncludedUsers = 5,
                ExtraBranchPrice = 200m,
                ExtraUserPrice = 50m,
                FeaturesJson = "[\"Basic\", \"Reports\"]",
                SupportLevel = "Email",
                IsActive = true,
                IsRecommended = false,
                SortOrder = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = PlanCode.Growth,
                Name = "Growth",
                Description = "For growing clinics",
                MonthlyPrice = 1000m,
                AnnualPrice = 10200m,
                IncludedBranches = 3,
                IncludedUsers = 15,
                ExtraBranchPrice = 150m,
                ExtraUserPrice = 40m,
                FeaturesJson = "[\"Advanced\", \"Analytics\"]",
                SupportLevel = "Phone",
                IsActive = true,
                IsRecommended = true,
                SortOrder = 2
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = PlanCode.Enterprise,
                Name = "Enterprise",
                Description = "For large clinics",
                MonthlyPrice = 2500m,
                AnnualPrice = 25500m,
                IncludedBranches = 10,
                IncludedUsers = 50,
                ExtraBranchPrice = 100m,
                ExtraUserPrice = 30m,
                FeaturesJson = "[\"Premium\", \"Custom\"]",
                SupportLevel = "Dedicated",
                IsActive = false, // Inactive for testing
                IsRecommended = false,
                SortOrder = 3
            }
        };

        _context.Plans.AddRange(plans);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetActivePlansAsync Tests

    [Fact]
    public async Task GetActivePlansAsync_ShouldReturnOnlyActivePlans()
    {
        // Act
        var result = await _service.GetActivePlansAsync();

        // Assert
        result.Should().HaveCount(2); // Starter and Growth are active, Enterprise is not
        result.Should().NotContain(p => p.Code == PlanCode.Enterprise);
    }

    [Fact]
    public async Task GetActivePlansAsync_ShouldReturnPlansInSortOrder()
    {
        // Act
        var result = (await _service.GetActivePlansAsync()).ToList();

        // Assert
        result[0].Code.Should().Be(PlanCode.Starter);
        result[1].Code.Should().Be(PlanCode.Growth);
    }

    [Fact]
    public async Task GetActivePlansAsync_ShouldMapAllProperties()
    {
        // Act
        var result = (await _service.GetActivePlansAsync()).First();

        // Assert
        result.Code.Should().Be(PlanCode.Starter);
        result.Name.Should().Be("Starter");
        result.Description.Should().Be("For small clinics");
        result.MonthlyPrice.Should().Be(500m);
        result.AnnualPrice.Should().Be(5100m);
        result.IncludedBranches.Should().Be(1);
        result.IncludedUsers.Should().Be(5);
        result.ExtraBranchPrice.Should().Be(200m);
        result.ExtraUserPrice.Should().Be(50m);
        result.Features.Should().Contain("Basic");
        result.SupportLevel.Should().Be("Email");
        result.IsRecommended.Should().BeFalse();
    }

    [Fact]
    public async Task GetActivePlansAsync_ShouldIdentifyRecommendedPlan()
    {
        // Act
        var result = await _service.GetActivePlansAsync();

        // Assert
        var recommended = result.SingleOrDefault(p => p.IsRecommended);
        recommended.Should().NotBeNull();
        recommended!.Code.Should().Be(PlanCode.Growth);
    }

    #endregion

    #region CalculateEstimate Basic Tests

    [Fact]
    public async Task CalculateEstimate_ShouldReturnNull_WhenPlanNotFound()
    {
        // Act - Enterprise is inactive
        var result = await _service.CalculateEstimate(
            PlanCode.Enterprise,
            branches: 1,
            users: 5,
            BillingCycle.Monthly);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateEstimate_ShouldCalculateBasePriceForMonthly()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.BasePrice.Should().Be(500m); // 1 month * 500 AED
        result.Total.Should().Be(500m);
        result.MonthlyEquivalent.Should().Be(500m);
    }

    [Fact]
    public async Task CalculateEstimate_ShouldCalculateBasePriceForAnnual()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Annual);

        // Assert
        result.Should().NotBeNull();
        result!.BasePrice.Should().Be(6000m); // 12 months * 500 AED
        result.DiscountPercent.Should().Be(15m); // 15% annual discount
        result.DiscountAmount.Should().Be(900m); // 15% of 6000
        result.Total.Should().Be(5100m); // 6000 - 900
        result.MonthlyEquivalent.Should().Be(425m); // 5100 / 12
    }

    #endregion

    #region Extra Resources Tests

    [Fact]
    public async Task CalculateEstimate_ShouldCalculateExtraBranches()
    {
        // Arrange - Starter includes 1 branch, request 3
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 3,
            users: 5,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.ExtraBranches.Should().Be(2);
        result.ExtraBranchesPrice.Should().Be(400m); // 2 extra * 200 AED
        result.Total.Should().Be(900m); // 500 base + 400 extra branches
    }

    [Fact]
    public async Task CalculateEstimate_ShouldCalculateExtraUsers()
    {
        // Arrange - Starter includes 5 users, request 10
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 10,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.ExtraUsers.Should().Be(5);
        result.ExtraUsersPrice.Should().Be(250m); // 5 extra * 50 AED
        result.Total.Should().Be(750m); // 500 base + 250 extra users
    }

    [Fact]
    public async Task CalculateEstimate_ShouldNotChargeForIncludedResources()
    {
        // Arrange - Starter includes 1 branch, 5 users - request exactly that
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.ExtraBranches.Should().Be(0);
        result.ExtraUsers.Should().Be(0);
        result.ExtraBranchesPrice.Should().Be(0m);
        result.ExtraUsersPrice.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateEstimate_ShouldHandleFewerResourcesThanIncluded()
    {
        // Arrange - Starter includes 5 users, request only 2
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 2,
            BillingCycle.Monthly);

        // Assert - should not give negative extras
        result.Should().NotBeNull();
        result!.ExtraUsers.Should().Be(0);
        result.ExtraUsersPrice.Should().Be(0m);
    }

    #endregion

    #region Billing Cycle Discount Tests

    [Theory]
    [InlineData(BillingCycle.Monthly, 0)]
    [InlineData(BillingCycle.Quarterly, 5)]
    [InlineData(BillingCycle.SemiAnnual, 10)]
    [InlineData(BillingCycle.Annual, 15)]
    public async Task CalculateEstimate_ShouldApplyCorrectDiscount(BillingCycle cycle, decimal expectedDiscountPercent)
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            cycle);

        // Assert
        result.Should().NotBeNull();
        result!.DiscountPercent.Should().Be(expectedDiscountPercent);
    }

    [Fact]
    public async Task CalculateEstimate_QuarterlyShouldBe3Months()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Quarterly);

        // Assert
        result.Should().NotBeNull();
        var subtotal = 500m * 3; // 3 months
        var discount = subtotal * 0.05m; // 5%
        result!.Subtotal.Should().Be(subtotal);
        result.DiscountAmount.Should().Be(discount);
        result.Total.Should().Be(subtotal - discount);
    }

    [Fact]
    public async Task CalculateEstimate_SemiAnnualShouldBe6Months()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.SemiAnnual);

        // Assert
        result.Should().NotBeNull();
        var subtotal = 500m * 6; // 6 months
        var discount = subtotal * 0.10m; // 10%
        result!.Subtotal.Should().Be(subtotal);
        result.DiscountAmount.Should().Be(discount);
        result.Total.Should().Be(subtotal - discount);
    }

    #endregion

    #region Currency Conversion Tests

    [Fact]
    public async Task CalculateEstimate_ShouldConvertToUSD()
    {
        // Arrange - USD rate is 0.27
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Monthly,
            Currency.USD);

        // Assert
        result.Should().NotBeNull();
        result!.Currency.Should().Be(Currency.USD);
        result.Total.Should().Be(500m * 0.27m); // 135 USD
    }

    [Fact]
    public async Task CalculateEstimate_ShouldKeepAEDUnconverted()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Monthly,
            Currency.AED);

        // Assert
        result.Should().NotBeNull();
        result!.Currency.Should().Be(Currency.AED);
        result.Total.Should().Be(500m);
    }

    #endregion

    #region Breakdown Tests

    [Fact]
    public async Task CalculateEstimate_ShouldIncludeBreakdownForBasePrice()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.Breakdown.Should().HaveCount(1);
        result.Breakdown[0].Label.Should().Contain("Starter Plan");
        result.Breakdown[0].Amount.Should().Be(500m);
        result.Breakdown[0].IsDiscount.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateEstimate_ShouldIncludeBreakdownForExtras()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 3,
            users: 10,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.Breakdown.Should().HaveCount(3); // Base + Extra Branches + Extra Users
        result.Breakdown.Should().Contain(b => b.Label.Contains("Extra Branches"));
        result.Breakdown.Should().Contain(b => b.Label.Contains("Extra Users"));
    }

    [Fact]
    public async Task CalculateEstimate_ShouldIncludeDiscountInBreakdown()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 1,
            users: 5,
            BillingCycle.Annual);

        // Assert
        result.Should().NotBeNull();
        var discountLine = result!.Breakdown.SingleOrDefault(b => b.IsDiscount);
        discountLine.Should().NotBeNull();
        discountLine!.Label.Should().Contain("15%");
        discountLine.Amount.Should().BeLessThan(0); // Discounts are negative
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public async Task CalculateEstimate_ComplexScenario_GrowthPlanAnnualWithExtras()
    {
        // Arrange
        // Growth plan: 1000 AED/month, 3 branches included, 15 users included
        // Extra branch: 150 AED/month, Extra user: 40 AED/month
        // Request: 5 branches (2 extra), 20 users (5 extra), Annual (15% discount)

        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Growth,
            branches: 5,
            users: 20,
            BillingCycle.Annual,
            Currency.AED);

        // Assert
        result.Should().NotBeNull();

        // Calculations:
        // Base: 1000 * 12 = 12000
        // Extra branches: 2 * 150 * 12 = 3600
        // Extra users: 5 * 40 * 12 = 2400
        // Subtotal: 18000
        // Discount: 18000 * 0.15 = 2700
        // Total: 15300

        result!.PlanCode.Should().Be(PlanCode.Growth);
        result.PlanName.Should().Be("Growth");
        result.ExtraBranches.Should().Be(2);
        result.ExtraUsers.Should().Be(5);
        result.BasePrice.Should().Be(12000m);
        result.ExtraBranchesPrice.Should().Be(3600m);
        result.ExtraUsersPrice.Should().Be(2400m);
        result.Subtotal.Should().Be(18000m);
        result.DiscountPercent.Should().Be(15m);
        result.DiscountAmount.Should().Be(2700m);
        result.Total.Should().Be(15300m);
        result.MonthlyEquivalent.Should().Be(1275m);
    }

    [Fact]
    public async Task CalculateEstimate_ComplexScenario_WithUSDConversion()
    {
        // Arrange - Same as above but in USD
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Growth,
            branches: 5,
            users: 20,
            BillingCycle.Annual,
            Currency.USD);

        // Assert
        result.Should().NotBeNull();
        result!.Currency.Should().Be(Currency.USD);
        result.Total.Should().Be(15300m * 0.27m); // 4131 USD
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CalculateEstimate_ShouldHandleZeroBranchesAndUsers()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 0,
            users: 0,
            BillingCycle.Monthly);

        // Assert
        result.Should().NotBeNull();
        result!.ExtraBranches.Should().Be(0);
        result.ExtraUsers.Should().Be(0);
        result.Total.Should().Be(500m); // Just base price
    }

    [Fact]
    public async Task CalculateEstimate_ShouldHandleLargeNumbers()
    {
        // Act
        var result = await _service.CalculateEstimate(
            PlanCode.Starter,
            branches: 100,
            users: 1000,
            BillingCycle.Annual);

        // Assert
        result.Should().NotBeNull();
        result!.ExtraBranches.Should().Be(99);
        result!.ExtraUsers.Should().Be(995);
    }

    #endregion
}
