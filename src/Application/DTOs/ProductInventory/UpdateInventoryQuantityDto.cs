using System;

namespace OnlineShop.Application.DTOs.ProductInventory
{
    // Simple DTO for updating only available quantity via PATCH endpoint
    public class UpdateInventoryQuantityDto
    {
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}

