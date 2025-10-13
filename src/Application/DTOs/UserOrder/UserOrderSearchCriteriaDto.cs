namespace OnlineShop.Application.DTOs.UserOrder
{
    public class UserOrderSearchCriteriaDto
    {
        public Guid? UserId { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SortBy { get; set; } // OrderDate, TotalAmount
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

