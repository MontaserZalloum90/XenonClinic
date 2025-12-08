using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Infrastructure.Modules;

/// <summary>
/// Case Management Module - Comprehensive patient case tracking and workflow management
/// </summary>
public class CaseManagementModule : ModuleBase
{
    public override string Name => ModuleNames.CaseManagement;
    public override string DisplayName => "Case Management";
    public override string Version => "1.0.0";
    public override string Description => "Comprehensive patient case tracking, activities, notes, and workflow management system";
    public override string Category => ModuleNames.Categories.Clinical;
    public override string? IconClass => "bi-folder-check";
    public override int DisplayOrder => 10;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Case Management services
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<IPatientService, PatientService>();

        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Configure Case Management entities
        modelBuilder.Entity<Case>(entity =>
        {
            entity.ToTable("Cases");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CaseNumber).IsUnique();
            entity.HasIndex(e => new { e.BranchId, e.PatientId });
            entity.HasIndex(e => e.CaseStatusId);
            entity.HasIndex(e => e.AssignedToUserId);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CaseType)
                .WithMany(ct => ct.Cases)
                .HasForeignKey(e => e.CaseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CaseStatus)
                .WithMany(cs => cs.Cases)
                .HasForeignKey(e => e.CaseStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CasePriority)
                .WithMany()
                .HasForeignKey(e => e.CasePriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CaseNumber).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<CaseActivity>(entity =>
        {
            entity.ToTable("CaseActivities");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CaseId);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Activities)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CaseActivityType)
                .WithMany()
                .HasForeignKey(e => e.CaseActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CaseActivityStatus)
                .WithMany()
                .HasForeignKey(e => e.CaseActivityStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CasePriority)
                .WithMany()
                .HasForeignKey(e => e.CasePriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<CaseNote>(entity =>
        {
            entity.ToTable("CaseNotes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CaseId);
            entity.HasIndex(e => new { e.CaseId, e.IsPinned });

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Notes)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CaseNoteType)
                .WithMany()
                .HasForeignKey(e => e.CaseNoteTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Content).IsRequired();
        });

        modelBuilder.Entity<CaseType>(entity =>
        {
            entity.ToTable("CaseTypes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<CaseStatus>(entity =>
        {
            entity.ToTable("CaseStatuses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
        });

        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Case Management routes are handled by MVC controllers with attribute routing
        // No custom route configuration needed
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Case Management seed data is handled by SeedData.cs
        // Could be extracted here in future refactoring
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
