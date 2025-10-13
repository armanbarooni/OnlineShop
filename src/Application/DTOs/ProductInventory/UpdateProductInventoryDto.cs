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
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
    }
}
