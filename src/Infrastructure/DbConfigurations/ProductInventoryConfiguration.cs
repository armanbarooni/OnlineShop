using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
    {
        public void Configure(EntityTypeBuilder<ProductInventory> builder)
        {
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.Id).ValueGeneratedOnAdd();

            builder.Property(pi => pi.AvailableQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.ReservedQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.SoldQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.CostPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(pi => pi.SellingPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(pi => pi.LastSyncAt)
                .IsRequired();

            builder.Property(pi => pi.SyncStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Synced");

            builder.Property(pi => pi.SyncError)
                .HasMaxLength(1000);

            // Foreign Key
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductInventories)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(pi => pi.MahakId);
            builder.Property(pi => pi.MahakClientId);
            builder.Property(pi => pi.RowVersion).IsConcurrencyToken();
            builder.Property(pi => pi.Deleted).HasDefaultValue(false);
            builder.Property(pi => pi.CreatedAt).IsRequired();
            builder.Property(pi => pi.UpdatedAt);

            builder.HasQueryFilter(pi => !pi.Deleted);

            // Indexes
            builder.HasIndex(pi => pi.ProductId).IsUnique();
            builder.HasIndex(pi => pi.LastSyncAt);
            builder.HasIndex(pi => pi.SyncStatus);
        }
    }
}
