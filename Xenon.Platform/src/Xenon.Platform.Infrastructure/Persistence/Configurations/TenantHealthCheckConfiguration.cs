using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class TenantHealthCheckConfiguration : IEntityTypeConfiguration<TenantHealthCheck>
{
    public void Configure(EntityTypeBuilder<TenantHealthCheck> builder)
    {
        builder.ToTable("TenantHealthChecks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DatabaseStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.DatabaseError)
            .HasMaxLength(1000);

        builder.Property(e => e.OverallStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Details)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(e => new { e.TenantId, e.CheckedAt });
        builder.HasIndex(e => e.CheckedAt);
    }
}
