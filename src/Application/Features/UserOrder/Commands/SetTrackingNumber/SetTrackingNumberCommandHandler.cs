using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Commands.SetTrackingNumber
{
    public class SetTrackingNumberCommandHandler : IRequestHandler<SetTrackingNumberCommand, Result<bool>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;

        public SetTrackingNumberCommandHandler(
            IUserOrderRepository orderRepository,
            IOrderStatusHistoryRepository statusHistoryRepository)
        {
            _orderRepository = orderRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<Result<bool>> Handle(SetTrackingNumberCommand request, CancellationToken cancellationToken)
        {
            // Get the order
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<bool>.Failure("سفارش یافت نشد");

            // Check if order is in a valid state for tracking number
            if (order.OrderStatus != "Processing" && order.OrderStatus != "Packed" && order.OrderStatus != "Shipped")
                return Result<bool>.Failure("فقط سفارشات در حال پردازش، بسته‌بندی شده یا ارسال شده می‌توانند شماره پیگیری داشته باشند");

            // Set tracking number
            order.SetTrackingNumber(request.TrackingNumber);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Create status history entry if order is being shipped
            if (order.OrderStatus == "Processing" || order.OrderStatus == "Packed")
            {
                var statusHistory = OrderStatusHistory.Create(
                    request.OrderId,
                    OrderStatus.Shipped,
                    $"شماره پیگیری تنظیم شد: {request.TrackingNumber}",
                    request.UpdatedBy
                );
                await _statusHistoryRepository.AddAsync(statusHistory, cancellationToken);

                // Update order status to shipped
                order.UpdateStatus("Shipped", $"شماره پیگیری: {request.TrackingNumber}", request.UpdatedBy);
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }

            return Result<bool>.Success(true);
        }
    }
}
