using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductRelationConfiguration : IEntityTypeConfiguration<ProductRelation>
    {
        public void Configure(EntityTypeBuilder<ProductRelation> builder)
        {
            builder.ToTable("ProductRelations");

            builder.HasKey(pr => pr.Id);

            builder.Property(pr => pr.ProductId)
                .IsRequired();

            builder.Property(pr => pr.RelatedProductId)
                .IsRequired();

            builder.Property(pr => pr.RelationType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pr => pr.Weight)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(pr => pr.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(pr => pr.Product)
                .WithMany()
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(pr => pr.RelatedProduct)
                .WithMany()
                .HasForeignKey(pr => pr.RelatedProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Base Entity Properties
            builder.Property(pr => pr.Deleted).HasDefaultValue(false);
            builder.Property(pr => pr.CreatedAt).IsRequired();
            builder.Property(pr => pr.UpdatedAt);

            // Query Filter
            builder.HasQueryFilter(pr => !pr.Deleted);

            // Indexes
            builder.HasIndex(pr => pr.ProductId);
            builder.HasIndex(pr => pr.RelatedProductId);
            builder.HasIndex(pr => new { pr.ProductId, pr.RelatedProductId });
            builder.HasIndex(pr => pr.RelationType);
            builder.HasIndex(pr => pr.IsActive);
        }
    }
}

