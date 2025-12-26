using System;
using System.Collections.Generic;

namespace OnlineShop.Application.DTOs.Product
{
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        
        // Category
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        // Brand
        public Guid? BrandId { get; set; }
        public string? BrandName { get; set; }
        
        // Images
        public List<ProductImageDto> Images { get; set; } = new();
        
        // Variants (Size/Color combinations)
        public List<ProductVariantDto> Variants { get; set; } = new();
        
        // Materials
        public List<string> Materials { get; set; } = new();
        
        // Seasons
        public List<string> Seasons { get; set; } = new();
        
        // Mahak Info
        public int? MahakId { get; set; }
        public long? RowVersion { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public string ImageType { get; set; } = "Main";
    }
    
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; }
    }
}