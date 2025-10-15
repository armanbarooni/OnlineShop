using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.DTOs.OrderStatusHistory
{
    public class OrderTimelineDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }
}
