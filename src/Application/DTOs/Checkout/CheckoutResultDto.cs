using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.DTOs.Checkout
{
    public class CheckoutResultDto
    {
        public UserOrderDto Order { get; set; } = null!;
        public OrderSummaryDto Summary { get; set; } = null!;
        public string Message { get; set; } = "سفارش شما با موفقیت ثبت شد";
    }
}

