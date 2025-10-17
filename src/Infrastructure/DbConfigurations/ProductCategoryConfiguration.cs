using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.HasKey(pc => pc.Id);
            builder.Property(pc => pc.Id).ValueGeneratedOnAdd();

            builder.Property(pc => pc.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pc => pc.Description)
                .HasMaxLength(500);

            builder.Property(pc => pc.ParentCategoryId);
            builder.Property(pc => pc.Level).HasDefaultValue(0);

            // Self-referencing relationship for hierarchical categories
            builder.HasOne(pc => pc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(pc => pc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(pc => pc.MahakId);
            builder.Property(pc => pc.MahakClientId);
            builder.Property(pc => pc.RowVersion).IsConcurrencyToken();
            builder.Property(pc => pc.Deleted).HasDefaultValue(false);
            builder.Property(pc => pc.CreatedAt).IsRequired();
            builder.Property(pc => pc.UpdatedAt);

            builder.HasQueryFilter(pc => !pc.Deleted);
        }
    }
}
