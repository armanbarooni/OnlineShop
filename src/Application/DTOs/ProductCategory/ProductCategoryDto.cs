namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; }
        public List<ProductCategoryDto> SubCategories { get; set; } = new();
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
    }
}
