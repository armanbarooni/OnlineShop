using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.ToTable("Materials");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(m => m.Deleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(m => m.Name);
            builder.HasIndex(m => m.IsActive);
        }
    }
}

