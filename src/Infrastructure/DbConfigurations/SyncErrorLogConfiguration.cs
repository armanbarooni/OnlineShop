using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class SyncErrorLogConfiguration : IEntityTypeConfiguration<SyncErrorLog>
    {
        public void Configure(EntityTypeBuilder<SyncErrorLog> builder)
        {
            builder.HasKey(sel => sel.Id);
            builder.Property(sel => sel.Id).ValueGeneratedOnAdd();

            builder.Property(sel => sel.ErrorType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sel => sel.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sel => sel.EntityId);

            builder.Property(sel => sel.ErrorCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sel => sel.ErrorMessage)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(sel => sel.StackTrace)
                .HasMaxLength(4000);

            builder.Property(sel => sel.RequestData)
                .HasMaxLength(4000);

            builder.Property(sel => sel.ResponseData)
                .HasMaxLength(4000);

            builder.Property(sel => sel.ErrorSeverity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            builder.Property(sel => sel.IsResolved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sel => sel.ResolvedAt);

            builder.Property(sel => sel.ResolvedBy)
                .HasMaxLength(100);

            builder.Property(sel => sel.ResolutionNotes)
                .HasMaxLength(1000);

            builder.Property(sel => sel.OccurrenceCount)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(sel => sel.LastOccurredAt)
                .IsRequired();

            // Base Entity Properties
            builder.Property(sel => sel.MahakId);
            builder.Property(sel => sel.MahakClientId);
            builder.Property(sel => sel.RowVersion).IsConcurrencyToken();
            builder.Property(sel => sel.Deleted).HasDefaultValue(false);
            builder.Property(sel => sel.CreatedAt).IsRequired();
            builder.Property(sel => sel.UpdatedAt);

            builder.HasQueryFilter(sel => !sel.Deleted);

            // Indexes
            builder.HasIndex(sel => sel.ErrorType);
            builder.HasIndex(sel => sel.EntityType);
            builder.HasIndex(sel => sel.EntityId);
            builder.HasIndex(sel => sel.ErrorCode);
            builder.HasIndex(sel => sel.ErrorSeverity);
            builder.HasIndex(sel => sel.IsResolved);
            builder.HasIndex(sel => sel.LastOccurredAt);
        }
    }
}
