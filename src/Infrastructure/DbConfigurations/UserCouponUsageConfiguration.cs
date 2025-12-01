using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserCouponUsageConfiguration : IEntityTypeConfiguration<UserCouponUsage>
    {
        public void Configure(EntityTypeBuilder<UserCouponUsage> builder)
        {
            builder.ToTable("UserCouponUsages");

            builder.HasKey(ucu => ucu.Id);

            builder.Property(ucu => ucu.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ucu => ucu.CouponId)
                .IsRequired();

            builder.Property(ucu => ucu.OrderId);

            builder.Property(ucu => ucu.UsedAt)
                .IsRequired();

            builder.Property(ucu => ucu.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ucu => ucu.OrderTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ucu => ucu.Notes)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(ucu => ucu.User)
                .WithMany()
                .HasForeignKey(ucu => ucu.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(ucu => ucu.Coupon)
                .WithMany()
                .HasForeignKey(ucu => ucu.CouponId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(ucu => ucu.Order)
                .WithMany()
                .HasForeignKey(ucu => ucu.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Base Entity Properties
            builder.Property(ucu => ucu.Deleted).HasDefaultValue(false);
            builder.Property(ucu => ucu.CreatedAt).IsRequired();
            builder.Property(ucu => ucu.UpdatedAt);

            // Query Filter
            builder.HasQueryFilter(ucu => !ucu.Deleted);

            // Indexes
            builder.HasIndex(ucu => ucu.UserId);
            builder.HasIndex(ucu => ucu.CouponId);
            builder.HasIndex(ucu => ucu.OrderId);
            builder.HasIndex(ucu => ucu.UsedAt);
        }
    }
}

