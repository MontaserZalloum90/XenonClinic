using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TokenType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.LastUsedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.Property(e => e.DeviceFingerprint)
            .HasMaxLength(100);

        builder.Property(e => e.RevokedReason)
            .HasMaxLength(200);

        // Indexes for efficient queries
        builder.HasIndex(e => e.TokenHash);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.TokenType, e.IsRevoked });
        builder.HasIndex(e => e.ExpiresAt);
    }
}

public class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("SecurityEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.UserType)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.Property(e => e.GeoLocation)
            .HasMaxLength(200);

        builder.Property(e => e.DeviceFingerprint)
            .HasMaxLength(100);

        builder.Property(e => e.RiskLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Details)
            .HasMaxLength(1000);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.RequestId)
            .HasMaxLength(100);

        builder.Property(e => e.ReviewedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ReviewNotes)
            .HasMaxLength(1000);

        // Indexes for efficient queries
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.IpAddress);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.AlertTriggered, e.IsReviewed });
        builder.HasIndex(e => e.RiskLevel);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.UserType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.RequestedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.UsedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        // Indexes for efficient queries
        builder.HasIndex(e => e.TokenHash);
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => new { e.UserId, e.UserType });
        builder.HasIndex(e => e.ExpiresAt);
    }
}
