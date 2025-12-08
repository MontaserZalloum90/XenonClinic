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
/// Sales Module - Sales orders, quotations, and revenue management
/// </summary>
public class SalesModule : ModuleBase
{
    public override string Name => ModuleNames.Sales;
    public override string DisplayName => "Sales Management";
    public override string Version => "1.0.0";
    public override string Description => "Sales orders, quotations, customer management, sales invoicing, and sales analytics";
    public override string Category => ModuleNames.Categories.Financial;
    public override string? IconClass => "bi-cart-check";
    public override int DisplayOrder => 35;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Sales services
        services.AddScoped<IPharmacyService, PharmacyService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Sales entities will be configured here when implemented
        // Examples: SalesOrder, SalesOrderLine, Quotation, Customer, SalesInvoice
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Sales routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Sales seed data will be added here
        await Task.CompletedTask;
        Console.WriteLine($"[Module] {DisplayName} - Seed data initialized");
    }

    public override async Task OnInitializingAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Initializing...");
        await Task.CompletedTask;
    }

    public override async Task OnInitializedAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Initialized successfully");
        await Task.CompletedTask;
    }
}
