using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(pv => pv.Id);

            builder.Property(pv => pv.Size)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(pv => pv.Color)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pv => pv.SKU)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pv => pv.Barcode)
                .HasMaxLength(100);

            builder.Property(pv => pv.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pv => pv.AdditionalPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(pv => pv.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pv => pv.Deleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(pv => pv.SKU).IsUnique();
            builder.HasIndex(pv => new { pv.ProductId, pv.Size, pv.Color });
            builder.HasIndex(pv => pv.StockQuantity);

            // Relationships
            builder.HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query Filter
            builder.HasQueryFilter(pv => !pv.Deleted);
        }
    }
}

