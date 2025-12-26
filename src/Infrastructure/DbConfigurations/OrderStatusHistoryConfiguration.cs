using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");

            builder.HasKey(osh => osh.Id);

            builder.Property(osh => osh.OrderId)
                .IsRequired();

            builder.Property(osh => osh.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(osh => osh.Note)
                .HasMaxLength(500);

            builder.Property(osh => osh.ChangedAt)
                .IsRequired();

            builder.Property(osh => osh.ChangedBy)
                .HasMaxLength(100);

            // Relationships
            builder.HasOne(osh => osh.Order)
                .WithMany()
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Base Entity Properties
            builder.Property(osh => osh.Deleted).HasDefaultValue(false);
            builder.Property(osh => osh.CreatedAt).IsRequired();
            builder.Property(osh => osh.UpdatedAt);

            // Query Filter
            builder.HasQueryFilter(osh => !osh.Deleted);

            // Indexes
            builder.HasIndex(osh => osh.OrderId);
            builder.HasIndex(osh => osh.Status);
            builder.HasIndex(osh => osh.ChangedAt);
        }
    }
}

