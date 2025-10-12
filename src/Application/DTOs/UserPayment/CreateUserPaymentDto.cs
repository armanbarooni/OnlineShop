using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.UserPayment
{
    public class CreateUserPaymentDto
    {
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "IRR";
    }
}
