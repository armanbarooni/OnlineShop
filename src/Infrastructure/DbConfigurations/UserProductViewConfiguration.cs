using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserProductViewConfiguration : IEntityTypeConfiguration<UserProductView>
    {
        public void Configure(EntityTypeBuilder<UserProductView> builder)
        {
            builder.ToTable("UserProductViews");

            builder.HasKey(upv => upv.Id);

            builder.Property(upv => upv.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(upv => upv.ProductId)
                .IsRequired();

            builder.Property(upv => upv.ViewedAt)
                .IsRequired();

            builder.Property(upv => upv.SessionId)
                .HasMaxLength(100);

            builder.Property(upv => upv.UserAgent)
                .HasMaxLength(500);

            builder.Property(upv => upv.IpAddress)
                .HasMaxLength(50);

            builder.Property(upv => upv.ViewDuration)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(upv => upv.ReferrerUrl)
                .HasMaxLength(1000);

            builder.Property(upv => upv.DeviceType)
                .HasMaxLength(50);

            builder.Property(upv => upv.Browser)
                .HasMaxLength(100);

            builder.Property(upv => upv.OperatingSystem)
                .HasMaxLength(100);

            builder.Property(upv => upv.IsReturningView)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(upv => upv.Product)
                .WithMany()
                .HasForeignKey(upv => upv.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(upv => upv.User)
                .WithMany()
                .HasForeignKey(upv => upv.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Base Entity Properties
            builder.Property(upv => upv.Deleted).HasDefaultValue(false);
            builder.Property(upv => upv.CreatedAt).IsRequired();
            builder.Property(upv => upv.UpdatedAt);

            // Query Filter
            builder.HasQueryFilter(upv => !upv.Deleted);

            // Indexes
            builder.HasIndex(upv => upv.UserId);
            builder.HasIndex(upv => upv.ProductId);
            builder.HasIndex(upv => new { upv.UserId, upv.ProductId });
            builder.HasIndex(upv => upv.ViewedAt);
        }
    }
}

