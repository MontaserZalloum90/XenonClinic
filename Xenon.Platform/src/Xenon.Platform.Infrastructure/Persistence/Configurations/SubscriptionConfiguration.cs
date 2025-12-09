using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PlanCode)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.BillingCycle)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Currency)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExtraBranchesPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExtraUsersPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.StripeSubscriptionId)
            .HasMaxLength(200);

        builder.Property(e => e.StripeCustomerId)
            .HasMaxLength(200);

        builder.Property(e => e.StripePaymentIntentId)
            .HasMaxLength(200);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500);

        builder.HasMany(e => e.History)
            .WithOne(e => e.Subscription)
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.EndDate);
    }
}
