using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class UsageSnapshotConfiguration : IEntityTypeConfiguration<UsageSnapshot>
{
    public void Configure(EntityTypeBuilder<UsageSnapshot> builder)
    {
        builder.ToTable("UsageSnapshots");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SnapshotType)
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.TenantId, e.SnapshotDate, e.SnapshotType });
        builder.HasIndex(e => e.SnapshotDate);
    }
}
