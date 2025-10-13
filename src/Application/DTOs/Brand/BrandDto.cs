namespace OnlineShop.Application.DTOs.Brand
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

