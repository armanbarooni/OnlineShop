using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Application.Features.UserOrder.Queries.GenerateInvoice
{
    public class GenerateOrderInvoiceQueryHandler : IRequestHandler<GenerateOrderInvoiceQuery, Result<byte[]>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IInvoiceService _invoiceService;

        public GenerateOrderInvoiceQueryHandler(
            IUserOrderRepository orderRepository,
            IInvoiceService invoiceService)
        {
            _orderRepository = orderRepository;
            _invoiceService = invoiceService;
        }

        public async Task<Result<byte[]>> Handle(GenerateOrderInvoiceQuery request, CancellationToken cancellationToken)
        {
            // Verify order exists
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<byte[]>.Failure("سفارش یافت نشد");

            try
            {
                var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(request.OrderId, cancellationToken);
                return Result<byte[]>.Success(pdfBytes);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"خطا در تولید فاکتور: {ex.Message}");
            }
        }
    }
}

