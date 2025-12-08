using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XenonClinic.Core.Abstractions;

/// <summary>
/// Defines the contract for a XenonClinic module.
/// Each module implements this interface to integrate with the host application.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Unique identifier for the module (e.g., "CaseManagement", "Audiology")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Display name for the module shown in UI
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Module version for tracking updates
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Description of the module's functionality
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Names of other modules this module depends on
    /// </summary>
    string[] Dependencies { get; }

    /// <summary>
    /// Icon class for displaying in module management UI
    /// </summary>
    string? IconClass { get; }

    /// <summary>
    /// Category for grouping modules (e.g., "Clinical", "Financial", "Operations")
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Order for displaying in module lists
    /// </summary>
    int DisplayOrder { get; }

    /// <summary>
    /// Whether this module is required for the system to function
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Configure module-specific services for dependency injection
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configure module-specific database entities and relationships
    /// </summary>
    void ConfigureDatabase(ModelBuilder modelBuilder);

    /// <summary>
    /// Configure module-specific routes and endpoints
    /// </summary>
    void ConfigureRoutes(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// Seed initial data for the module
    /// </summary>
    Task SeedDataAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Called when the module is being initialized
    /// </summary>
    Task OnInitializingAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Called after the module has been initialized
    /// </summary>
    Task OnInitializedAsync(IServiceProvider serviceProvider);
}
