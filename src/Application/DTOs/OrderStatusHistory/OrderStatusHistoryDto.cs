using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.DTOs.OrderStatusHistory
{
    public class OrderStatusHistoryDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusDisplayName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
