namespace OnlineShop.Application.DTOs.ProductInventory
{
    public class CreateProductInventoryDto
    {
        public Guid ProductId { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; } = 0;
        public int SoldQuantity { get; set; } = 0;
        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
    }
}
