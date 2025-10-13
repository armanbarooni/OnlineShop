using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        // Product Related Entities
        public DbSet<ProductDetail> ProductDetails { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        
        // User Related Entities
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserPayment> UserPayments { get; set; }
        public DbSet<UserOrder> UserOrders { get; set; }
        public DbSet<UserOrderItem> UserOrderItems { get; set; }
        public DbSet<UserReturnRequest> UserReturnRequests { get; set; }
        
        // Cart Related Entities
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<SavedCart> SavedCarts { get; set; }
        
        // Mahak Sync Entities
        public DbSet<MahakSyncLog> MahakSyncLogs { get; set; }
        public DbSet<MahakMapping> MahakMappings { get; set; }
        public DbSet<MahakQueue> MahakQueues { get; set; }
        public DbSet<SyncErrorLog> SyncErrorLogs { get; set; }
        
        // Authentication Entities
        public DbSet<Otp> Otps { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);


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

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var currentUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = currentUserId;
                        entry.Entity.LastModifiedAt = now;
                        entry.Entity.LastModifiedBy = currentUserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = now;
                        entry.Entity.LastModifiedBy = currentUserId;
                        // Don't update CreatedAt and CreatedBy on modification
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;
                        break;
                }
            }
        }

        private string? GetCurrentUserId()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}