using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Mahak Sync Fields
        public int? MahakPersonId { get; set; }
        public long? MahakPersonClientId { get; set; }
        public DateTime? MahakSyncedAt { get; set; }
        
        // Navigation Properties
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<UserPayment> UserPayments { get; set; } = new List<UserPayment>();
        public virtual ICollection<UserOrder> UserOrders { get; set; } = new List<UserOrder>();
        public virtual ICollection<UserReturnRequest> UserReturnRequests { get; set; } = new List<UserReturnRequest>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<SavedCart> SavedCarts { get; set; } = new List<SavedCart>();
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}
