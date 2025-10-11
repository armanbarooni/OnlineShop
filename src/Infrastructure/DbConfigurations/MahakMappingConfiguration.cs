using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class MahakMappingConfiguration : IEntityTypeConfiguration<MahakMapping>
    {
        public void Configure(EntityTypeBuilder<MahakMapping> builder)
        {
            builder.HasKey(mm => mm.Id);
            builder.Property(mm => mm.Id).ValueGeneratedOnAdd();

            builder.Property(mm => mm.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(mm => mm.LocalEntityId)
                .IsRequired();

            builder.Property(mm => mm.MahakEntityId)
                .IsRequired();

            builder.Property(mm => mm.MahakEntityCode)
                .HasMaxLength(100);

            builder.Property(mm => mm.MappingStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            builder.Property(mm => mm.MappedAt)
                .IsRequired();

            builder.Property(mm => mm.UnmappedAt);

            builder.Property(mm => mm.UnmappedReason)
                .HasMaxLength(500);

            builder.Property(mm => mm.Notes)
                .HasMaxLength(1000);

            // Base Entity Properties
            builder.Property(mm => mm.MahakId);
            builder.Property(mm => mm.MahakClientId);
            builder.Property(mm => mm.RowVersion).IsConcurrencyToken();
            builder.Property(mm => mm.Deleted).HasDefaultValue(false);
            builder.Property(mm => mm.CreatedAt).IsRequired();
            builder.Property(mm => mm.UpdatedAt);

            builder.HasQueryFilter(mm => !mm.Deleted);

            // Indexes
            builder.HasIndex(mm => mm.EntityType);
            builder.HasIndex(mm => mm.LocalEntityId);
            builder.HasIndex(mm => mm.MahakEntityId);
            builder.HasIndex(mm => new { mm.EntityType, mm.LocalEntityId }).IsUnique();
            builder.HasIndex(mm => new { mm.EntityType, mm.MahakEntityId }).IsUnique();
            builder.HasIndex(mm => mm.MappingStatus);
        }
    }
}
