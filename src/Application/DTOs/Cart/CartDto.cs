using System;

namespace OnlineShop.Application.DTOs.Cart
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }  // Optional: specific size/color
        public int Quantity { get; set; } = 1;
    }
    
    public class UpdateCartItemDto
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
    
    public class CartDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
    
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public Guid? VariantId { get; set; }
        public string? VariantInfo { get; set; }  // "Size: L, Color: Black"
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
    }
}
