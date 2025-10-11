using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.SessionId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.CartName)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Default");

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.ExpiresAt);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            // Foreign Key
            builder.HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(c => c.MahakId);
            builder.Property(c => c.MahakClientId);
            builder.Property(c => c.RowVersion).IsConcurrencyToken();
            builder.Property(c => c.Deleted).HasDefaultValue(false);
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UpdatedAt);

            builder.HasQueryFilter(c => !c.Deleted);

            // Indexes
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.SessionId);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.ExpiresAt);
        }
    }
}
