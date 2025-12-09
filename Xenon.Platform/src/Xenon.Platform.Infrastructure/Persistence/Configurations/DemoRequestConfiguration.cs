using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class DemoRequestConfiguration : IEntityTypeConfiguration<DemoRequest>
{
    public void Configure(EntityTypeBuilder<DemoRequest> builder)
    {
        builder.ToTable("DemoRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.Email);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Company)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CompanyType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ClinicType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.InquiryType)
            .HasMaxLength(50);

        builder.Property(e => e.Message)
            .HasMaxLength(2000);

        builder.Property(e => e.ModulesOfInterest)
            .HasMaxLength(500);

        builder.Property(e => e.DeploymentPreference)
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasMaxLength(50);

        builder.Property(e => e.AssignedTo)
            .HasMaxLength(256);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        builder.Property(e => e.UtmSource)
            .HasMaxLength(100);

        builder.Property(e => e.UtmMedium)
            .HasMaxLength(100);

        builder.Property(e => e.UtmCampaign)
            .HasMaxLength(200);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
    }
}
