namespace OnlineShop.Application.DTOs.UserOrder
{
    public class UserOrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    }
}

