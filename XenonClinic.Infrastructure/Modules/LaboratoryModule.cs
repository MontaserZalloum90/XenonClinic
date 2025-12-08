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
/// Laboratory Module - Lab test orders, results, and specimen management
/// </summary>
public class LaboratoryModule : ModuleBase
{
    public override string Name => ModuleNames.Laboratory;
    public override string DisplayName => "Laboratory";
    public override string Version => "1.0.0";
    public override string Description => "Laboratory test orders, specimen tracking, result management, and integration with external labs";
    public override string Category => ModuleNames.Categories.Clinical;
    public override string? IconClass => "bi-microscope";
    public override int DisplayOrder => 15;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Laboratory services
        services.AddScoped<ILabService, LabService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Laboratory entities will be configured here when implemented
        // Examples: LabOrder, LabTest, LabResult, LabSpecimen, ExternalLab
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Laboratory routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Laboratory seed data will be added here
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
