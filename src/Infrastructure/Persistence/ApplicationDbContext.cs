using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Common;
//using OnlineShop.Domain.Entities; // موجودیت‌هات

namespace OnlineShop.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // مثال DbSet
        //public DbSet<Product> Products { get; set; }
        //public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
  
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<long>(nameof(BaseEntity.RowVersion))
                        .IsConcurrencyToken();

                }
            }
        }

    }
}
