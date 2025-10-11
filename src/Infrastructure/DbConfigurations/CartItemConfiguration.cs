using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.Id).ValueGeneratedOnAdd();

            builder.Property(ci => ci.Quantity)
                .IsRequired();

            builder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ci => ci.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ci => ci.Notes)
                .HasMaxLength(500);

            // Foreign Keys
            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(ci => ci.MahakId);
            builder.Property(ci => ci.MahakClientId);
            builder.Property(ci => ci.RowVersion).IsConcurrencyToken();
            builder.Property(ci => ci.Deleted).HasDefaultValue(false);
            builder.Property(ci => ci.CreatedAt).IsRequired();
            builder.Property(ci => ci.UpdatedAt);

            builder.HasQueryFilter(ci => !ci.Deleted);

            // Indexes
            builder.HasIndex(ci => ci.CartId);
            builder.HasIndex(ci => ci.ProductId);
            builder.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        }
    }
}
