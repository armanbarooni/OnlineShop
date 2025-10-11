using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Id).ValueGeneratedOnAdd();

            builder.Property(w => w.Notes)
                .HasMaxLength(500);

            builder.Property(w => w.AddedAt)
                .IsRequired();

            // Foreign Keys
            builder.HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(w => w.MahakId);
            builder.Property(w => w.MahakClientId);
            builder.Property(w => w.RowVersion).IsConcurrencyToken();
            builder.Property(w => w.Deleted).HasDefaultValue(false);
            builder.Property(w => w.CreatedAt).IsRequired();
            builder.Property(w => w.UpdatedAt);

            builder.HasQueryFilter(w => !w.Deleted);

            // Indexes
            builder.HasIndex(w => w.UserId);
            builder.HasIndex(w => w.ProductId);
            builder.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            builder.HasIndex(w => w.AddedAt);
        }
    }
}
