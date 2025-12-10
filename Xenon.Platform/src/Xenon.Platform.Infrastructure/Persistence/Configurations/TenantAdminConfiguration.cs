using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class TenantAdminConfiguration : IEntityTypeConfiguration<TenantAdmin>
{
    public void Configure(EntityTypeBuilder<TenantAdmin> builder)
    {
        builder.ToTable("TenantAdmins");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Email is globally unique across all tenants.
        // This is intentional because tenant admins log in with just email (no tenant identifier).
        // If emails were only unique per-tenant, login would be ambiguous.
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_TenantAdmins_Email_Global");

        // Also maintain composite index for potential future multi-admin scenarios
        builder.HasIndex(e => new { e.TenantId, e.Email })
            .HasDatabaseName("IX_TenantAdmins_TenantId_Email");

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EmailVerificationToken)
            .HasMaxLength(500);

        builder.Property(e => e.LastLoginIp)
            .HasMaxLength(50);

        builder.Property(e => e.PasswordResetToken)
            .HasMaxLength(500);
    }
}
