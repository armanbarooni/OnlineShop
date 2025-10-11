using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserReturnRequestConfiguration : IEntityTypeConfiguration<UserReturnRequest>
    {
        public void Configure(EntityTypeBuilder<UserReturnRequest> builder)
        {
            builder.HasKey(urr => urr.Id);
            builder.Property(urr => urr.Id).ValueGeneratedOnAdd();

            builder.Property(urr => urr.ReturnReason)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(urr => urr.ReturnStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(urr => urr.Description)
                .HasMaxLength(1000);

            builder.Property(urr => urr.Quantity)
                .IsRequired();

            builder.Property(urr => urr.RefundAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(urr => urr.AdminNotes)
                .HasMaxLength(1000);

            builder.Property(urr => urr.ApprovedAt);
            builder.Property(urr => urr.ApprovedBy)
                .HasMaxLength(100);

            builder.Property(urr => urr.RejectedAt);
            builder.Property(urr => urr.RejectedBy)
                .HasMaxLength(100);

            builder.Property(urr => urr.RejectionReason)
                .HasMaxLength(500);

            builder.Property(urr => urr.ProcessedAt);
            builder.Property(urr => urr.ProcessedBy)
                .HasMaxLength(100);

            // Foreign Keys
            builder.HasOne(urr => urr.User)
                .WithMany(u => u.UserReturnRequests)
                .HasForeignKey(urr => urr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(urr => urr.Order)
                .WithMany()
                .HasForeignKey(urr => urr.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(urr => urr.OrderItem)
                .WithMany()
                .HasForeignKey(urr => urr.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Entity Properties
            builder.Property(urr => urr.MahakId);
            builder.Property(urr => urr.MahakClientId);
            builder.Property(urr => urr.RowVersion).IsConcurrencyToken();
            builder.Property(urr => urr.Deleted).HasDefaultValue(false);
            builder.Property(urr => urr.CreatedAt).IsRequired();
            builder.Property(urr => urr.UpdatedAt);

            builder.HasQueryFilter(urr => !urr.Deleted);

            // Indexes
            builder.HasIndex(urr => urr.UserId);
            builder.HasIndex(urr => urr.OrderId);
            builder.HasIndex(urr => urr.OrderItemId);
            builder.HasIndex(urr => urr.ReturnStatus);
            builder.HasIndex(urr => urr.CreatedAt);
        }
    }
}
