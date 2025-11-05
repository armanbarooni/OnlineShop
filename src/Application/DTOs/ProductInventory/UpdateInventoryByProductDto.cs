using System;

namespace OnlineShop.Application.DTOs.ProductInventory
{
    // Simple DTO used by integration tests and legacy endpoints to update inventory by product
    public class UpdateInventoryByProductDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Operation { get; set; }
    }
}
