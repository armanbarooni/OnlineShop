using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.HasKey(pr => pr.Id);
            builder.Property(pr => pr.Id).ValueGeneratedOnAdd();

            builder.Property(pr => pr.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pr => pr.Comment)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(pr => pr.Rating)
                .IsRequired();

            builder.Property(pr => pr.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pr => pr.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pr => pr.ApprovedAt);
            builder.Property(pr => pr.ApprovedBy)
                .HasMaxLength(100);

            // Foreign Keys
            builder.HasOne(pr => pr.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pr => pr.User)
                .WithMany(u => u.ProductReviews)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Entity Properties
            builder.Property(pr => pr.MahakId);
            builder.Property(pr => pr.MahakClientId);
            builder.Property(pr => pr.RowVersion).IsConcurrencyToken();
            builder.Property(pr => pr.Deleted).HasDefaultValue(false);
            builder.Property(pr => pr.CreatedAt).IsRequired();
            builder.Property(pr => pr.UpdatedAt);

            builder.HasQueryFilter(pr => !pr.Deleted);

            // Indexes
            builder.HasIndex(pr => pr.ProductId);
            builder.HasIndex(pr => pr.UserId);
            builder.HasIndex(pr => new { pr.ProductId, pr.UserId }).IsUnique();
            builder.HasIndex(pr => pr.Rating);
            builder.HasIndex(pr => pr.IsApproved);
        }
    }
}
