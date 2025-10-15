namespace OnlineShop.Application.DTOs.ProductVariant
{
    public class UpdateProductVariantDto
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int StockQuantity { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}

