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

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique();

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
