namespace OnlineShop.Application.DTOs.Product
{
    public class ProductSearchCriteriaDto
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public List<string>? Sizes { get; set; }
        public List<string>? Colors { get; set; }
        public List<Guid>? MaterialIds { get; set; }
        public List<Guid>? SeasonIds { get; set; }
        public string? Gender { get; set; }
        public bool? NewArrivals { get; set; } // Last 30 days
        public bool? OnSale { get; set; }
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

