using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
    {
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            builder.HasKey(pd => pd.Id);
            builder.Property(pd => pd.Id).ValueGeneratedOnAdd();

            builder.Property(pd => pd.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pd => pd.Value)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pd => pd.Description)
                .HasMaxLength(1000);

            builder.Property(pd => pd.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Foreign Key
            builder.HasOne(pd => pd.Product)
                .WithMany(p => p.ProductDetails)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(pd => pd.MahakId);
            builder.Property(pd => pd.MahakClientId);
            builder.Property(pd => pd.RowVersion).IsConcurrencyToken();
            builder.Property(pd => pd.Deleted).HasDefaultValue(false);
            builder.Property(pd => pd.CreatedAt).IsRequired();
            builder.Property(pd => pd.UpdatedAt);

            builder.HasQueryFilter(pd => !pd.Deleted);

            // Indexes
            builder.HasIndex(pd => pd.ProductId);
            builder.HasIndex(pd => new { pd.ProductId, pd.Key }).IsUnique();
        }
    }
}
