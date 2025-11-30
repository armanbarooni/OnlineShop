using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class StockAlertConfiguration : IEntityTypeConfiguration<StockAlert>
    {
        public void Configure(EntityTypeBuilder<StockAlert> builder)
        {
            builder.ToTable("StockAlerts");

            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.ProductId)
                .IsRequired();

            builder.Property(sa => sa.ProductVariantId);

            builder.Property(sa => sa.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(sa => sa.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(sa => sa.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(sa => sa.Notified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sa => sa.NotifiedAt);

            builder.Property(sa => sa.NotificationMethod)
                .HasMaxLength(50);

            // Relationships
            builder.HasOne(sa => sa.Product)
                .WithMany()
                .HasForeignKey(sa => sa.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(sa => sa.ProductVariant)
                .WithMany()
                .HasForeignKey(sa => sa.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(sa => sa.User)
                .WithMany()
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Base Entity Properties
            builder.Property(sa => sa.Deleted).HasDefaultValue(false);
            builder.Property(sa => sa.CreatedAt).IsRequired();
            builder.Property(sa => sa.UpdatedAt);

            // Query Filter
            builder.HasQueryFilter(sa => !sa.Deleted);

            // Indexes
            builder.HasIndex(sa => sa.ProductId);
            builder.HasIndex(sa => sa.UserId);
            builder.HasIndex(sa => new { sa.ProductId, sa.UserId });
            builder.HasIndex(sa => sa.Notified);
        }
    }
}

