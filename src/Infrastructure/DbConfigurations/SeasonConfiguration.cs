using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class SeasonConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.ToTable("Seasons");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.Deleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(s => s.Code).IsUnique();
            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.IsActive);
        }
    }
}

