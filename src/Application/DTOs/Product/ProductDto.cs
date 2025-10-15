using OnlineShop.Application.DTOs.Brand;
using OnlineShop.Application.DTOs.Material;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Application.DTOs.ProductVariant;
using OnlineShop.Application.DTOs.Season;
using OnlineShop.Application.DTOs.Unit;

namespace OnlineShop.Application.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        
        // Nested objects
        public ProductCategoryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
        public UnitDto? Unit { get; set; }
        
        // Lists
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<MaterialDto> Materials { get; set; } = new();
        public List<SeasonDto> Seasons { get; set; } = new();
        
        // Review summary
        public int ReviewCount { get; set; }
        public decimal AverageRating { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
