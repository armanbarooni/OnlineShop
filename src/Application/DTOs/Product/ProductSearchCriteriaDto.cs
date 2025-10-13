namespace OnlineShop.Application.DTOs.Product
{
    public class ProductSearchCriteriaDto
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? InStock { get; set; }
        public string? SortBy { get; set; } // Name, Price, ViewCount, CreatedAt
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

