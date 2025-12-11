using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Infrastructure.Modules;

/// <summary>
/// Sales Module - Sales orders, quotations, payments, and revenue management
/// </summary>
public class SalesModule : ModuleBase
{
    public override string Name => ModuleNames.Sales;
    public override string DisplayName => "Sales Management";
    public override string Version => "1.0.0";
    public override string Description => "Sales orders, quotations, payments, customer management, sales invoicing, and sales analytics";
    public override string Category => ModuleNames.Categories.Financial;
    public override string? IconClass => "bi-cart-check";
    public override int DisplayOrder => 35;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Sales services
        services.AddScoped<ISalesService, SalesService>();
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
#endif
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Sales entities are configured in ClinicDbContext:
        // - Sale, SaleItem (sales transactions with line items)
        // - Payment (payment records for sales)
        // - Quotation, QuotationItem (pre-sales estimates)
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Database entities configured");
#endif
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Sales routes are handled by MVC controllers with attribute routing
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Routes configured");
#endif
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Sales seed data will be added here
        await Task.CompletedTask;
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Seed data initialized");
#endif
    }

    public override async Task OnInitializingAsync(IServiceProvider serviceProvider)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} v{Version} - Initializing...");
#endif
        await Task.CompletedTask;
    }

    public override async Task OnInitializedAsync(IServiceProvider serviceProvider)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} v{Version} - Initialized successfully");
#endif
        await Task.CompletedTask;
    }
}
