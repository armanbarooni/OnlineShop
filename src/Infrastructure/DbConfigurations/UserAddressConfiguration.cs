using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.HasKey(ua => ua.Id);
            builder.Property(ua => ua.Id).ValueGeneratedOnAdd();

            builder.Property(ua => ua.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.AddressLine1)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ua => ua.AddressLine2)
                .HasMaxLength(500);

            builder.Property(ua => ua.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.State)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(ua => ua.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ua => ua.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(ua => ua.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ua => ua.IsBillingAddress)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ua => ua.IsShippingAddress)
                .IsRequired()
                .HasDefaultValue(false);

            // Foreign Key
            builder.HasOne(ua => ua.User)
                .WithMany(u => u.UserAddresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(ua => ua.MahakId);
            builder.Property(ua => ua.MahakClientId);
            builder.Property(ua => ua.RowVersion).IsConcurrencyToken();
            builder.Property(ua => ua.Deleted).HasDefaultValue(false);
            builder.Property(ua => ua.CreatedAt).IsRequired();
            builder.Property(ua => ua.UpdatedAt);

            builder.HasQueryFilter(ua => !ua.Deleted);

            // Indexes
            builder.HasIndex(ua => ua.UserId);
            builder.HasIndex(ua => new { ua.UserId, ua.IsDefault });
            builder.HasIndex(ua => ua.PostalCode);
        }
    }
}
