namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class CreateProductCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long MahakClientId { get; set; }
        public int MahakId { get; set; }
    }
}
