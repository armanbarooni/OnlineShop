namespace OnlineShop.Application.DTOs.Wishlist
{
    public class CreateWishlistDto
    {
        public Guid ProductId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateWishlistDto
    {
        public Guid Id { get; set; }
        public string? Notes { get; set; }
    }

    public class WishlistDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? Notes { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
