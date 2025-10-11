using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserOrderConfiguration : IEntityTypeConfiguration<UserOrder>
    {
        public void Configure(EntityTypeBuilder<UserOrder> builder)
        {
            builder.HasKey(uo => uo.Id);
            builder.Property(uo => uo.Id).ValueGeneratedOnAdd();

            builder.Property(uo => uo.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(uo => uo.OrderStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(uo => uo.SubTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(uo => uo.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(uo => uo.ShippingAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(uo => uo.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(uo => uo.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(uo => uo.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("IRR");

            builder.Property(uo => uo.Notes)
                .HasMaxLength(1000);

            builder.Property(uo => uo.ShippedAt);
            builder.Property(uo => uo.DeliveredAt);
            builder.Property(uo => uo.CancelledAt);

            builder.Property(uo => uo.CancellationReason)
                .HasMaxLength(500);

            builder.Property(uo => uo.TrackingNumber)
                .HasMaxLength(100);

            // Foreign Keys
            builder.HasOne(uo => uo.User)
                .WithMany(u => u.UserOrders)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(uo => uo.ShippingAddress)
                .WithMany()
                .HasForeignKey(uo => uo.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(uo => uo.BillingAddress)
                .WithMany()
                .HasForeignKey(uo => uo.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Entity Properties
            builder.Property(uo => uo.MahakId);
            builder.Property(uo => uo.MahakClientId);
            builder.Property(uo => uo.RowVersion).IsConcurrencyToken();
            builder.Property(uo => uo.Deleted).HasDefaultValue(false);
            builder.Property(uo => uo.CreatedAt).IsRequired();
            builder.Property(uo => uo.UpdatedAt);

            builder.HasQueryFilter(uo => !uo.Deleted);

            // Indexes
            builder.HasIndex(uo => uo.UserId);
            builder.HasIndex(uo => uo.OrderNumber).IsUnique();
            builder.HasIndex(uo => uo.OrderStatus);
            builder.HasIndex(uo => uo.CreatedAt);
            builder.HasIndex(uo => uo.TrackingNumber);
        }
    }
}
