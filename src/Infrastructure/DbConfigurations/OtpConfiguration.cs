using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class OtpConfiguration : IEntityTypeConfiguration<Otp>
    {
        public void Configure(EntityTypeBuilder<Otp> builder)
        {
            builder.ToTable("Otps");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(o => o.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(o => o.ExpiresAt)
                .IsRequired();

            builder.Property(o => o.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.UsedAt);

            builder.Property(o => o.UsedFor)
                .HasMaxLength(50);

            builder.Property(o => o.AttemptsCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(o => o.Deleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.UpdatedAt);

            builder.Property(o => o.CreatedBy)
                .HasMaxLength(100);

            builder.Property(o => o.UpdatedBy)
                .HasMaxLength(100);

            builder.Property(o => o.MahakClientId);

            builder.Property(o => o.MahakId);

            // Indexes
            builder.HasIndex(o => o.PhoneNumber);
            builder.HasIndex(o => o.ExpiresAt);
            builder.HasIndex(o => new { o.PhoneNumber, o.IsUsed, o.ExpiresAt });
        }
    }
}

