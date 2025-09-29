namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
    }
}
