using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class MahakSyncLogConfiguration : IEntityTypeConfiguration<MahakSyncLog>
    {
        public void Configure(EntityTypeBuilder<MahakSyncLog> builder)
        {
            builder.HasKey(msl => msl.Id);
            builder.Property(msl => msl.Id).ValueGeneratedOnAdd();

            builder.Property(msl => msl.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(msl => msl.EntityId);

            builder.Property(msl => msl.SyncType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(msl => msl.SyncStatus)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(msl => msl.SyncStartedAt)
                .IsRequired();

            builder.Property(msl => msl.SyncCompletedAt);

            builder.Property(msl => msl.DurationMs)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(msl => msl.RecordsProcessed)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(msl => msl.RecordsSuccessful)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(msl => msl.RecordsFailed)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(msl => msl.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(msl => msl.SyncData)
                .HasMaxLength(4000);

            builder.Property(msl => msl.MahakResponse)
                .HasMaxLength(4000);

            builder.Property(msl => msl.MahakRowVersion);

            // Base Entity Properties
            builder.Property(msl => msl.MahakId);
            builder.Property(msl => msl.MahakClientId);
            builder.Property(msl => msl.RowVersion).IsConcurrencyToken();
            builder.Property(msl => msl.Deleted).HasDefaultValue(false);
            builder.Property(msl => msl.CreatedAt).IsRequired();
            builder.Property(msl => msl.UpdatedAt);

            builder.HasQueryFilter(msl => !msl.Deleted);

            // Indexes
            builder.HasIndex(msl => msl.EntityType);
            builder.HasIndex(msl => msl.EntityId);
            builder.HasIndex(msl => msl.SyncType);
            builder.HasIndex(msl => msl.SyncStatus);
            builder.HasIndex(msl => msl.SyncStartedAt);
            builder.HasIndex(msl => msl.MahakRowVersion);
        }
    }
}
