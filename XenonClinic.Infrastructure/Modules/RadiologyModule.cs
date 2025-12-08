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
/// Radiology Module - Medical imaging, radiology orders, and imaging results
/// </summary>
public class RadiologyModule : ModuleBase
{
    public override string Name => ModuleNames.Radiology;
    public override string DisplayName => "Radiology & Imaging";
    public override string Version => "1.0.0";
    public override string Description => "Radiology orders, medical imaging studies (X-Ray, CT, MRI, Ultrasound), PACS integration, and imaging reports";
    public override string Category => ModuleNames.Categories.Clinical;
    public override string? IconClass => "bi-prescription2";
    public override int DisplayOrder => 40;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Radiology services
        services.AddScoped<IRadiologyService, RadiologyService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Radiology entities currently use Lab entities as a foundation
        // When specific RadiologyOrder/ImagingStudy entities are created, configure them here
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Radiology routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Radiology seed data will be added here when specific entities are created
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
