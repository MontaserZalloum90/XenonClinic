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
/// Financial Module - Accounting, expenses, invoicing, and financial reporting
/// </summary>
public class FinancialModule : ModuleBase
{
    public override string Name => ModuleNames.Financial;
    public override string DisplayName => "Financial Management";
    public override string Version => "1.0.0";
    public override string Description => "Chart of accounts, general ledger, expenses, invoicing, payments, and financial reporting";
    public override string Category => ModuleNames.Categories.Financial;
    public override string? IconClass => "bi-cash-coin";
    public override int DisplayOrder => 25;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Financial services
        services.AddScoped<IFinancialService, FinancialService>();
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
#endif
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Financial entities will be configured here when implemented
        // Examples: Account, Transaction, Invoice, Payment, Expense, Budget
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Database entities configured");
#endif
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Financial routes are handled by MVC controllers with attribute routing
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[Module] {DisplayName} - Routes configured");
#endif
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Financial seed data will be added here
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
