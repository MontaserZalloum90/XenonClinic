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
/// Inventory Module - Stock management, item tracking, and inventory control
/// </summary>
public class InventoryModule : ModuleBase
{
    public override string Name => ModuleNames.Inventory;
    public override string DisplayName => "Inventory Management";
    public override string Version => "1.0.0";
    public override string Description => "Inventory items, stock levels, stock movements, reorder management, and multi-location tracking";
    public override string Category => ModuleNames.Categories.Operations;
    public override string? IconClass => "bi-box-seam";
    public override int DisplayOrder => 30;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Inventory services
        services.AddScoped<IInventoryService, InventoryService>();
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
#endif
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Inventory entities will be configured here when implemented
        // Examples: InventoryItem, StockLevel, StockMovement, StockAdjustment, Warehouse
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Database entities configured");
#endif
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Inventory routes are handled by MVC controllers with attribute routing
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Routes configured");
#endif
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Inventory seed data will be added here
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
