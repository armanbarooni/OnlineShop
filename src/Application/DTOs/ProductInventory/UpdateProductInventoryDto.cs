using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.ProductInventory
{
    public class UpdateProductInventoryDto
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        // When null, handler keeps the current value from the entity
        public int? AvailableQuantity { get; set; }
        public int? ReservedQuantity { get; set; }
        public int? SoldQuantity { get; set; }

        // Optional: supports legacy payloads like { Quantity, Notes }
        public int? Quantity { get; set; }
        public string? Notes { get; set; }

        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
    }
}
