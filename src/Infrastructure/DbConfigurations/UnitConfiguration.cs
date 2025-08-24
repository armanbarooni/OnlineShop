using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .ValueGeneratedNever();

            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(u => u.Comment)
                   .HasMaxLength(500);

            builder.Property(u => u.UnitTIN)
                   .HasMaxLength(50);

            builder.Property(u => u.RowVersion)
                   .IsConcurrencyToken();

            builder.HasQueryFilter(u => !u.Deleted);
        }
    }
}
