using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserOrderItemConfiguration : IEntityTypeConfiguration<UserOrderItem>
    {
        public void Configure(EntityTypeBuilder<UserOrderItem> builder)
        {
            builder.HasKey(uoi => uoi.Id);
            builder.Property(uoi => uoi.Id).ValueGeneratedOnAdd();

            builder.Property(uoi => uoi.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(uoi => uoi.ProductDescription)
                .HasMaxLength(1000);

            builder.Property(uoi => uoi.ProductSku)
                .HasMaxLength(100);

            builder.Property(uoi => uoi.Quantity)
                .IsRequired();

            builder.Property(uoi => uoi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(uoi => uoi.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(uoi => uoi.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(uoi => uoi.Notes)
                .HasMaxLength(500);

            // Foreign Keys
            builder.HasOne(uoi => uoi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(uoi => uoi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uoi => uoi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(uoi => uoi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Entity Properties
            builder.Property(uoi => uoi.MahakId);
            builder.Property(uoi => uoi.MahakClientId);
            builder.Property(uoi => uoi.RowVersion).IsConcurrencyToken();
            builder.Property(uoi => uoi.Deleted).HasDefaultValue(false);
            builder.Property(uoi => uoi.CreatedAt).IsRequired();
            builder.Property(uoi => uoi.UpdatedAt);

            builder.HasQueryFilter(uoi => !uoi.Deleted);

            // Indexes
            builder.HasIndex(uoi => uoi.OrderId);
            builder.HasIndex(uoi => uoi.ProductId);
        }
    }
}
