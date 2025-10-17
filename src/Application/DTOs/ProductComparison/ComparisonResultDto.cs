using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.DTOs.ProductComparison
{
    public class ComparisonResultDto
    {
        public List<ProductDto> Products { get; set; } = new();
        public ComparisonFacetsDto Facets { get; set; } = new();
    }

    public class ComparisonFacetsDto
    {
        public List<string> Brands { get; set; } = new();
        public List<decimal> Prices { get; set; } = new();
        public List<string> Sizes { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Materials { get; set; } = new();
    }
}

