namespace OnlineShop.Application.DTOs.Product
{
    public class ProductDto
    {
        public ProductDto()
        {
            Name = string.Empty;
        }

        public ProductDto(Guid id, string name, decimal price)
        {
            Id = id;
            Name = name ?? string.Empty;
            Price = price;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
