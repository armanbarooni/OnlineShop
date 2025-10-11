using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserPaymentConfiguration : IEntityTypeConfiguration<UserPayment>
    {
        public void Configure(EntityTypeBuilder<UserPayment> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id).ValueGeneratedOnAdd();

            builder.Property(up => up.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(up => up.PaymentStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(up => up.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(up => up.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("IRR");

            builder.Property(up => up.TransactionId)
                .HasMaxLength(100);

            builder.Property(up => up.GatewayResponse)
                .HasMaxLength(2000);

            builder.Property(up => up.PaidAt);
            builder.Property(up => up.FailedAt);

            builder.Property(up => up.FailureReason)
                .HasMaxLength(500);

            builder.Property(up => up.RefundId)
                .HasMaxLength(100);

            builder.Property(up => up.RefundAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(up => up.RefundedAt);

            builder.Property(up => up.RefundReason)
                .HasMaxLength(500);

            // Foreign Keys
            builder.HasOne(up => up.User)
                .WithMany(u => u.UserPayments)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(up => up.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(up => up.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Entity Properties
            builder.Property(up => up.MahakId);
            builder.Property(up => up.MahakClientId);
            builder.Property(up => up.RowVersion).IsConcurrencyToken();
            builder.Property(up => up.Deleted).HasDefaultValue(false);
            builder.Property(up => up.CreatedAt).IsRequired();
            builder.Property(up => up.UpdatedAt);

            builder.HasQueryFilter(up => !up.Deleted);

            // Indexes
            builder.HasIndex(up => up.UserId);
            builder.HasIndex(up => up.OrderId);
            builder.HasIndex(up => up.TransactionId).IsUnique();
            builder.HasIndex(up => up.PaymentStatus);
            builder.HasIndex(up => up.PaidAt);
        }
    }
}
