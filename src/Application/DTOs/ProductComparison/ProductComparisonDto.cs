namespace OnlineShop.Application.DTOs.ProductComparison
{
    public class ProductComparisonDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<Guid> ProductIds { get; set; } = new();
        public int ProductCount { get; set; }
        public bool IsFull { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

