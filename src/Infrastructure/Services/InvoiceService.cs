using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IUserAddressRepository _addressRepository;

        public InvoiceService(
            IUserOrderRepository orderRepository,
            IUserOrderItemRepository orderItemRepository,
            IUserAddressRepository addressRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _addressRepository = addressRepository;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
            var shippingAddress = order.ShippingAddressId.HasValue 
                ? await _addressRepository.GetByIdAsync(order.ShippingAddressId.Value, cancellationToken)
                : null;

            // Generate HTML invoice
            var htmlContent = GenerateInvoiceHtml(order, orderItems, shippingAddress);

            // For now, return HTML as bytes (can be enhanced with PDF library like QuestPDF or iTextSharp)
            // TODO: Implement actual PDF generation using QuestPDF or iTextSharp
            return System.Text.Encoding.UTF8.GetBytes(htmlContent);
        }

        private string GenerateInvoiceHtml(
            Domain.Entities.UserOrder order,
            IEnumerable<Domain.Entities.UserOrderItem> orderItems,
            Domain.Entities.UserAddress? shippingAddress)
        {
            var html = $@"
<!DOCTYPE html>
<html dir='rtl' lang='fa'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Tahoma', Arial, sans-serif; padding: 20px; }}
        .invoice {{ max-width: 800px; margin: 0 auto; border: 1px solid #ddd; padding: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .info {{ display: flex; justify-content: space-between; margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: right; }}
        th {{ background-color: #f2f2f2; }}
        .total {{ text-align: left; margin-top: 20px; }}
        .total-row {{ font-weight: bold; font-size: 1.1em; }}
    </style>
</head>
<body>
    <div class='invoice'>
        <div class='header'>
            <h1>فاکتور فروش</h1>
            <p>شماره سفارش: {order.OrderNumber}</p>
            <p>تاریخ: {order.CreatedAt:yyyy/MM/dd}</p>
        </div>
        <div class='info'>
            <div>
                <h3>آدرس ارسال:</h3>
                {(shippingAddress != null ? $@"
                <p>{shippingAddress.FirstName} {shippingAddress.LastName}</p>
                <p>{shippingAddress.AddressLine1}</p>
                <p>{shippingAddress.City}, {shippingAddress.State}</p>
                <p>کد پستی: {shippingAddress.PostalCode}</p>
                " : "<p>آدرس مشخص نشده</p>")}
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>نام محصول</th>
                    <th>تعداد</th>
                    <th>قیمت واحد</th>
                    <th>تخفیف</th>
                    <th>جمع</th>
                </tr>
            </thead>
            <tbody>
";

            foreach (var item in orderItems)
            {
                html += $@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Quantity}</td>
                    <td>{item.UnitPrice:N0} تومان</td>
                    <td>{(item.DiscountAmount ?? 0):N0} تومان</td>
                    <td>{item.TotalPrice:N0} تومان</td>
                </tr>
";
            }

            html += $@"
            </tbody>
        </table>
        <div class='total'>
            <p>جمع کل: {order.TotalAmount:N0} تومان</p>
            <p>تخفیف: {order.DiscountAmount:N0} تومان</p>
            <p class='total-row'>مبلغ نهایی: {order.FinalAmount:N0} تومان</p>
        </div>
    </div>
</body>
</html>
";

            return html;
        }
    }
}

