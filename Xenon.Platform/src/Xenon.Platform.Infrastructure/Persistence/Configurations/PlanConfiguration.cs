using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.AnnualPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExtraBranchPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExtraUserPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.FeaturesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.SupportLevel)
            .HasMaxLength(200);

        builder.HasMany(e => e.Subscriptions)
            .WithOne(e => e.Plan)
            .HasForeignKey(e => e.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
