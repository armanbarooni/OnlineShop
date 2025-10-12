using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.UserReturnRequest
{
    public class CreateUserReturnRequestDto
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public Guid OrderItemId { get; set; }
        public string ReturnReason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
