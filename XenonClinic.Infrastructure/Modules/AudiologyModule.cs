using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Modules;

/// <summary>
/// Audiology Module - Hearing assessments, audiograms, and hearing device management
/// </summary>
public class AudiologyModule : ModuleBase
{
    public override string Name => ModuleNames.Audiology;
    public override string DisplayName => "Audiology";
    public override string Version => "1.2.1";
    public override string Description => "Comprehensive audiology services including hearing assessments, audiogram management, and hearing device tracking";
    public override string Category => ModuleNames.Categories.Clinical;
    public override string? IconClass => "bi-soundwave";
    public override int DisplayOrder => 5;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Audiology services
        // Services will be added here as they're extracted
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Configure Audiology entities
        modelBuilder.Entity<AudiologyVisit>(entity =>
        {
            entity.ToTable("AudiologyVisits");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BranchId, e.PatientId });
            entity.HasIndex(e => e.VisitDate);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Audiogram>(entity =>
        {
            entity.ToTable("Audiograms");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AudiologyVisitId);

            entity.HasOne(e => e.Visit)
                .WithOne(v => v.Audiogram)
                .HasForeignKey<Audiogram>(e => e.AudiologyVisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HearingDevice>(entity =>
        {
            entity.ToTable("HearingDevices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BranchId, e.PatientId });
            entity.HasIndex(e => e.SerialNumber);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ModelName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
        });

        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Audiology routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Audiology seed data handled by existing SeedData.cs
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
