using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class MahakRegionConfiguration : IEntityTypeConfiguration<MahakRegion>
    {
        public void Configure(EntityTypeBuilder<MahakRegion> builder)
        {
            builder.HasKey(region => region.Id);
            builder.Property(region => region.Id).ValueGeneratedOnAdd();

            builder.Property(region => region.CityId)
                .IsRequired();

            builder.Property(region => region.CityName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(region => region.ProvinceId)
                .IsRequired();

            builder.Property(region => region.ProvinceName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(region => region.MapCode)
                .HasMaxLength(100);

            builder.Property(region => region.MahakRowVersion)
                .IsRequired()
                .HasDefaultValue(0L);

            builder.Property(region => region.SyncedAt)
                .IsRequired();

            builder.Property(region => region.MahakId);
            builder.Property(region => region.MahakClientId);
            builder.Property(region => region.RowVersion).IsConcurrencyToken();
            builder.Property(region => region.Deleted).HasDefaultValue(false);
            builder.Property(region => region.CreatedAt).IsRequired();
            builder.Property(region => region.UpdatedAt);

            builder.HasQueryFilter(region => !region.Deleted);

            builder.HasIndex(region => region.CityId).IsUnique();
            builder.HasIndex(region => region.ProvinceId);
            builder.HasIndex(region => region.ProvinceName);
            builder.HasIndex(region => new { region.ProvinceName, region.CityName });
        }
    }
}
