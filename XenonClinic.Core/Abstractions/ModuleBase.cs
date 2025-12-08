using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XenonClinic.Core.Abstractions;

/// <summary>
/// Base class for XenonClinic modules providing default implementations
/// </summary>
public abstract class ModuleBase : IModule
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract string Category { get; }

    public virtual string[] Dependencies => Array.Empty<string>();
    public virtual string? IconClass => null;
    public virtual int DisplayOrder => 100;
    public virtual bool IsRequired => false;

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Default implementation - override in derived classes
    }

    public virtual void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Default implementation - override in derived classes
    }

    public virtual void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Default implementation - override in derived classes
    }

    public virtual Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Default implementation - override in derived classes
        return Task.CompletedTask;
    }

    public virtual Task OnInitializingAsync(IServiceProvider serviceProvider)
    {
        // Default implementation - override in derived classes
        return Task.CompletedTask;
    }

    public virtual Task OnInitializedAsync(IServiceProvider serviceProvider)
    {
        // Default implementation - override in derived classes
        return Task.CompletedTask;
    }
}
