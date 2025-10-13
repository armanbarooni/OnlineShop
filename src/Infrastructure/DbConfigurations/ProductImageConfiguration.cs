using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.Id).ValueGeneratedOnAdd();

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(pi => pi.AltText)
                .HasMaxLength(200);

            builder.Property(pi => pi.Title)
                .HasMaxLength(200);

            builder.Property(pi => pi.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pi => pi.ImageType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Main");

            builder.Property(pi => pi.FileSize)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.MimeType)
                .HasMaxLength(100);

            // Foreign Key
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
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
            builder.HasIndex(pi => pi.ProductId);
            builder.HasIndex(pi => new { pi.ProductId, pi.IsPrimary });
        }
    }
}
