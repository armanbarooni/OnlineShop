using System;

namespace OnlineShop.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public Guid ShippingAddressId { get; set; }
        public string PaymentMethod { get; set; } = "Online";
        public string? CouponCode { get; set; }  // کد تخفیف (اختیاری)
        public string? Note { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }
    }
}
