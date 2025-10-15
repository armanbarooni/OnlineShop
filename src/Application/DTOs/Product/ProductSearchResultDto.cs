using OnlineShop.Application.DTOs.Common;

namespace OnlineShop.Application.DTOs.Product
{
    public class ProductSearchResultDto
    {
        public PagedResultDto<ProductDto> Products { get; set; } = null!;
        
        // Facets for filtering
        public List<string> AvailableSizes { get; set; } = new();
        public List<string> AvailableColors { get; set; } = new();
        public List<BrandFacetDto> AvailableBrands { get; set; } = new();
        public List<MaterialFacetDto> AvailableMaterials { get; set; } = new();
        public List<SeasonFacetDto> AvailableSeasons { get; set; } = new();
        public List<PriceRangeDto> PriceRanges { get; set; } = new();
    }

    public class BrandFacetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MaterialFacetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SeasonFacetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PriceRangeDto
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Count { get; set; }
    }
}
