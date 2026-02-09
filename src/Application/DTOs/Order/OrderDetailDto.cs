using OnlineShop.Application.DTOs.Cart;
using System;
using System.Collections.Generic;

namespace OnlineShop.Application.DTOs.Order
{
    public class OrderDetailDto : OrderDto
    {
        public decimal SubTotal { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? TrackingNumber { get; set; }
        public string? PaymentMethod { get; set; }
        
        // Items
        public List<OrderItemDto> Items { get; set; } = new();
        
        // Address
        public string? ShippingAddress { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? VariantInfo { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
