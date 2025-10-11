using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id).ValueGeneratedOnAdd();

            builder.Property(up => up.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(up => up.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(up => up.NationalCode)
                .HasMaxLength(10);

            builder.Property(up => up.BirthDate);

            builder.Property(up => up.Gender)
                .HasMaxLength(10);

            builder.Property(up => up.ProfileImageUrl)
                .HasMaxLength(1000);

            builder.Property(up => up.Bio)
                .HasMaxLength(1000);

            builder.Property(up => up.Website)
                .HasMaxLength(200);

            builder.Property(up => up.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(up => up.IsPhoneVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(up => up.EmailVerifiedAt);
            builder.Property(up => up.PhoneVerifiedAt);

            // Foreign Key
            builder.HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Base Entity Properties
            builder.Property(up => up.MahakId);
            builder.Property(up => up.MahakClientId);
            builder.Property(up => up.RowVersion).IsConcurrencyToken();
            builder.Property(up => up.Deleted).HasDefaultValue(false);
            builder.Property(up => up.CreatedAt).IsRequired();
            builder.Property(up => up.UpdatedAt);

            builder.HasQueryFilter(up => !up.Deleted);

            // Indexes
            builder.HasIndex(up => up.UserId).IsUnique();
            builder.HasIndex(up => up.NationalCode).IsUnique();
        }
    }
}
