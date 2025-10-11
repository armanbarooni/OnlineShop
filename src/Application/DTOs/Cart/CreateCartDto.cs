namespace OnlineShop.Application.DTOs.Cart
{
    public class CreateCartDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string CartName { get; set; } = "Default";
        public DateTime? ExpiresAt { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCartDto
    {
        public Guid Id { get; set; }
        public string CartName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string CartName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Notes { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCartItemDto
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCartItemDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
