using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;

        public UpdateOrderStatusCommandHandler(
            IUserOrderRepository orderRepository,
            IOrderStatusHistoryRepository statusHistoryRepository)
        {
            _orderRepository = orderRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            // Get the order
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<bool>.Failure("سفارش یافت نشد");

            // Check if status change is valid
            var currentStatus = order.OrderStatus;
            var newStatus = request.Status.ToString();

            if (currentStatus == newStatus)
                return Result<bool>.Failure("وضعیت سفارش قبلاً این مقدار است");

            // Validate status transition
            if (!IsValidStatusTransition(currentStatus, newStatus))
                return Result<bool>.Failure($"تغییر وضعیت از '{currentStatus}' به '{newStatus}' مجاز نیست");

            // Update order status
            order.UpdateStatus(newStatus, request.Note, request.UpdatedBy);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Create status history entry
            var statusHistory = OrderStatusHistory.Create(
                request.OrderId,
                request.Status,
                request.Note,
                request.UpdatedBy
            );
            await _statusHistoryRepository.AddAsync(statusHistory, cancellationToken);

            return Result<bool>.Success(true);
        }

        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                ("Pending", "Processing") => true,
                ("Pending", "Cancelled") => true,
                ("Processing", "Packed") => true,
                ("Processing", "Cancelled") => true,
                ("Packed", "Shipped") => true,
                ("Packed", "Cancelled") => true,
                ("Shipped", "OutForDelivery") => true,
                ("OutForDelivery", "Delivered") => true,
                ("Delivered", "Returned") => true,
                _ => false
            };
        }
    }
}
