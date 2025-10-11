using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class MahakQueueConfiguration : IEntityTypeConfiguration<MahakQueue>
    {
        public void Configure(EntityTypeBuilder<MahakQueue> builder)
        {
            builder.HasKey(mq => mq.Id);
            builder.Property(mq => mq.Id).ValueGeneratedOnAdd();

            builder.Property(mq => mq.QueueType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(mq => mq.OperationType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(mq => mq.EntityId);

            builder.Property(mq => mq.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(mq => mq.QueueStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(mq => mq.Priority)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(mq => mq.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(mq => mq.MaxRetries)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(mq => mq.ScheduledAt);

            builder.Property(mq => mq.ProcessedAt);

            builder.Property(mq => mq.FailedAt);

            builder.Property(mq => mq.Payload)
                .HasMaxLength(4000);

            builder.Property(mq => mq.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(mq => mq.MahakResponse)
                .HasMaxLength(4000);

            builder.Property(mq => mq.NextRetryAt);

            // Base Entity Properties
            builder.Property(mq => mq.MahakId);
            builder.Property(mq => mq.MahakClientId);
            builder.Property(mq => mq.RowVersion).IsConcurrencyToken();
            builder.Property(mq => mq.Deleted).HasDefaultValue(false);
            builder.Property(mq => mq.CreatedAt).IsRequired();
            builder.Property(mq => mq.UpdatedAt);

            builder.HasQueryFilter(mq => !mq.Deleted);

            // Indexes
            builder.HasIndex(mq => mq.QueueType);
            builder.HasIndex(mq => mq.OperationType);
            builder.HasIndex(mq => mq.EntityType);
            builder.HasIndex(mq => mq.EntityId);
            builder.HasIndex(mq => mq.QueueStatus);
            builder.HasIndex(mq => mq.Priority);
            builder.HasIndex(mq => mq.ScheduledAt);
            builder.HasIndex(mq => mq.NextRetryAt);
        }
    }
}
