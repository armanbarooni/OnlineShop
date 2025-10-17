namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class CreateProductCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public long MahakClientId { get; set; }
        public int MahakId { get; set; }
    }
}
