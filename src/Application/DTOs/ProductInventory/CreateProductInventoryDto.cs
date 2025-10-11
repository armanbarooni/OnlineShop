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

    public class UpdateProductInventoryDto
    {
        public Guid Id { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
    }

    public class ProductInventoryDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public DateTime LastSyncAt { get; set; }
        public string SyncStatus { get; set; } = string.Empty;
        public string? SyncError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
