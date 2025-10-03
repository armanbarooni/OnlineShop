namespace OnlineShop.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        public UpdateProductDto()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        public UpdateProductDto(Guid id, string name, string description, decimal price, int stockQuantity)
        {
            Id = id;
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            Price = price;
            StockQuantity = stockQuantity;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
