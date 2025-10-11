using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CategoryId);

            builder.Property(p => p.UnitId);

            builder.Property(p => p.Sku)
                .HasMaxLength(100);

            builder.Property(p => p.Barcode)
                .HasMaxLength(100);

            builder.Property(p => p.Weight)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Dimensions)
                .HasMaxLength(100);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.SaleStartDate);

            builder.Property(p => p.SaleEndDate);

            // Foreign Keys
            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Unit)
                .WithMany()
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            // Base Entity Properties
            builder.Property(p => p.MahakId);
            builder.Property(p => p.MahakClientId);
            builder.Property(p => p.RowVersion).IsConcurrencyToken();
            builder.Property(p => p.Deleted).HasDefaultValue(false);
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt);

            builder.HasQueryFilter(p => !p.Deleted);

            // Indexes
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.UnitId);
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.Barcode).IsUnique();
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.IsFeatured);
            builder.HasIndex(p => p.Price);
            builder.HasIndex(p => p.StockQuantity);
        }
    }
}
