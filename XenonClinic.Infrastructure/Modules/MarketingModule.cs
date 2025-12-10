using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Infrastructure.Modules;

/// <summary>
/// Marketing Module - Campaign management, lead tracking, and marketing analytics
/// </summary>
public class MarketingModule : ModuleBase
{
    public override string Name => ModuleNames.Marketing;
    public override string DisplayName => "Marketing";
    public override string Version => "1.0.0";
    public override string Description => "Marketing campaigns, lead management, patient outreach, referral programs, and marketing analytics";
    public override string Category => ModuleNames.Categories.Marketing;
    public override string? IconClass => "bi-megaphone";
    public override int DisplayOrder => 40;

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Marketing services
        // services.AddScoped<IMarketingService, MarketingService>();
        // services.AddScoped<ICampaignService, CampaignService>();
        // services.AddScoped<ILeadService, LeadService>();
        Console.WriteLine($"[Module] {DisplayName} v{Version} - Services registered");
    }

    public override void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        // Campaign entity configuration
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CampaignCode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.StartDate);

            entity.Property(e => e.CampaignCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.TargetAudience).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CallToAction).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            entity.Property(e => e.ActualSpend).HasPrecision(18, 2);
            entity.Property(e => e.Revenue).HasPrecision(18, 2);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Lead entity configuration
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LeadCode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.NextFollowUpDate);

            entity.Property(e => e.LeadCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.MobileNumber).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.SourceDetails).HasMaxLength(500);
            entity.Property(e => e.InterestedIn).HasMaxLength(500);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.NextFollowUpNotes).HasMaxLength(1000);
            entity.Property(e => e.LostReason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.EstimatedValue).HasPrecision(18, 2);
            entity.Property(e => e.ActualValue).HasPrecision(18, 2);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Leads)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ConvertedToPatient)
                .WithMany()
                .HasForeignKey(e => e.ConvertedToPatientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MarketingActivity entity configuration
        modelBuilder.Entity<MarketingActivity>(entity =>
        {
            entity.ToTable("MarketingActivities");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.ActivityDate);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Outcome).HasMaxLength(500);
            entity.Property(e => e.ContactMethod).HasMaxLength(100);
            entity.Property(e => e.ContactedPerson).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.NextSteps).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.InternalNotes).HasMaxLength(2000);
            entity.Property(e => e.AttachmentIds).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Activities)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Activities)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PerformedByUser)
                .WithMany()
                .HasForeignKey(e => e.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        Console.WriteLine($"[Module] {DisplayName} - Database entities configured");
    }

    public override void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Marketing routes are handled by MVC controllers with attribute routing
        Console.WriteLine($"[Module] {DisplayName} - Routes configured");
    }

    public override async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Marketing seed data will be added here
        // Examples: Default campaign templates, lead sources, activity types
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
