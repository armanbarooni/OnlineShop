using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class SavedCartConfiguration : IEntityTypeConfiguration<SavedCart>
    {
        public void Configure(EntityTypeBuilder<SavedCart> builder)
        {
            builder.HasKey(sc => sc.Id);
            builder.Property(sc => sc.Id).ValueGeneratedOnAdd();

            builder.Property(sc => sc.SavedCartName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sc => sc.Description)
                .HasMaxLength(500);

            builder.Property(sc => sc.IsFavorite)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sc => sc.SavedAt)
                .IsRequired();

            builder.Property(sc => sc.LastAccessedAt);

            builder.Property(sc => sc.AccessCount)
                .IsRequired()
                .HasDefaultValue(0);

            // Foreign Keys
            builder.HasOne(sc => sc.User)
                .WithMany(u => u.SavedCarts)
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sc => sc.Cart)
                .WithMany()
                .HasForeignKey(sc => sc.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(sc => sc.MahakId);
            builder.Property(sc => sc.MahakClientId);
            builder.Property(sc => sc.RowVersion).IsConcurrencyToken();
            builder.Property(sc => sc.Deleted).HasDefaultValue(false);
            builder.Property(sc => sc.CreatedAt).IsRequired();
            builder.Property(sc => sc.UpdatedAt);

            builder.HasQueryFilter(sc => !sc.Deleted);

            // Indexes
            builder.HasIndex(sc => sc.UserId);
            builder.HasIndex(sc => sc.CartId);
            builder.HasIndex(sc => sc.IsFavorite);
            builder.HasIndex(sc => sc.SavedAt);
        }
    }
}
