using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.UserOrder
{
    public class UpdateUserOrderDto
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }
    }
}
