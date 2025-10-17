using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductComparisonConfiguration : IEntityTypeConfiguration<ProductComparison>
    {
        public void Configure(EntityTypeBuilder<ProductComparison> builder)
        {
            builder.HasKey(pc => pc.Id);
            builder.Property(pc => pc.Id).ValueGeneratedOnAdd();

            builder.Property(pc => pc.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(pc => pc.ProductIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(Guid.Parse)
                          .ToList())
                .HasColumnType("nvarchar(max)");

            builder.Property(pc => pc.CreatedAt).IsRequired();
            builder.Property(pc => pc.UpdatedAt);
            builder.Property(pc => pc.RowVersion).IsConcurrencyToken();

            builder.HasIndex(pc => pc.UserId);
        }
    }
}

