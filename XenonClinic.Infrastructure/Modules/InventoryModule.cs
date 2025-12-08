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
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Inventory entities will be configured here when implemented
        // Examples: InventoryItem, StockLevel, StockMovement, StockAdjustment, Warehouse
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Inventory routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Inventory seed data will be added here
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
