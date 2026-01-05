namespace OnlineShop.Application.DTOs.Product
{
    public class CreateProductDto
    {
        public CreateProductDto()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        public CreateProductDto(string name, string description, decimal price, int stockQuantity)
        {
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            Price = price;
            StockQuantity = stockQuantity;
        }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
