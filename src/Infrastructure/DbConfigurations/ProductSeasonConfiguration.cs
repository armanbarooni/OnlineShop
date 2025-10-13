using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductSeasonConfiguration : IEntityTypeConfiguration<ProductSeason>
    {
        public void Configure(EntityTypeBuilder<ProductSeason> builder)
        {
            builder.ToTable("ProductSeasons");

            builder.HasKey(ps => ps.Id);

            // Composite unique index to prevent duplicates
            builder.HasIndex(ps => new { ps.ProductId, ps.SeasonId }).IsUnique();

            // Relationships
            builder.HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSeasons)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ps => ps.Season)
                .WithMany(s => s.ProductSeasons)
                .HasForeignKey(ps => ps.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

