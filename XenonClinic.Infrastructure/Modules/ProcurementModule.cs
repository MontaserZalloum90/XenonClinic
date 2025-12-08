using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Constants;

namespace XenonClinic.Infrastructure.Modules;

/// <summary>
/// Procurement Module - Purchase orders, goods receipts, and supplier management
/// </summary>
public class ProcurementModule : ModuleBase
{
    public override string Name => ModuleNames.Procurement;
    public override string DisplayName => "Procurement";
    public override string Version => "1.0.0";
    public override string Description => "Purchase orders, purchase requisitions, goods receipts, supplier management, and procurement workflows";
    public override string Category => ModuleNames.Categories.Financial;
    public override string? IconClass => "bi-truck";
    public override int DisplayOrder => 40;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Procurement services
        // services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        // services.AddScoped<ISupplierService, SupplierService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Procurement entities will be configured here when implemented
        // Examples: PurchaseOrder, PurchaseOrderLine, PurchaseRequisition, GoodsReceipt, Supplier
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Procurement routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Procurement seed data will be added here
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
