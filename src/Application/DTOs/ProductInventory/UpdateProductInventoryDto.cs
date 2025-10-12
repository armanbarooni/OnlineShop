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
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Location { get; set; }
    }
}
