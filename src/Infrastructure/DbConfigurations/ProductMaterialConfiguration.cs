using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductMaterialConfiguration : IEntityTypeConfiguration<ProductMaterial>
    {
        public void Configure(EntityTypeBuilder<ProductMaterial> builder)
        {
            builder.ToTable("ProductMaterials");

            builder.HasKey(pm => pm.Id);

            // Composite unique index to prevent duplicates
            builder.HasIndex(pm => new { pm.ProductId, pm.MaterialId }).IsUnique();

            // Relationships
            builder.HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(pm => pm.Deleted).HasDefaultValue(false);
            builder.HasQueryFilter(pm => !pm.Deleted);
        }
    }
}

