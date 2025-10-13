using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.LogoUrl)
                .HasMaxLength(500);

            builder.Property(b => b.Description)
                .HasMaxLength(1000);

            builder.Property(b => b.Website)
                .HasMaxLength(500);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(b => b.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(b => b.Deleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(b => b.Name);
            builder.HasIndex(b => b.IsActive);
            builder.HasIndex(b => b.DisplayOrder);

            // Relationships
            builder.HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

