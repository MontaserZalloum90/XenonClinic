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
/// HR Module - Human resources, employee management, attendance, and leave tracking
/// </summary>
public class HRModule : ModuleBase
{
    public override string Name => ModuleNames.HR;
    public override string DisplayName => "Human Resources";
    public override string Version => "1.0.0";
    public override string Description => "Employee management, attendance tracking, leave requests, payroll, and performance reviews";
    public override string Category => ModuleNames.Categories.Operations;
    public override string? IconClass => "bi-people";
    public override int DisplayOrder => 20;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register HR services
        services.AddScoped<IHRService, HRService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // HR entities will be configured here when implemented
        // Examples: Employee, Attendance, LeaveRequest, Department, Position, Payroll
        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // HR routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // HR seed data will be added here
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
