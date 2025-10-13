namespace OnlineShop.Application.DTOs.ProductImage
{
    public class CreateProductImageDto
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public long FileSize { get; set; } = 0;
        public string? MimeType { get; set; }
    }
}
