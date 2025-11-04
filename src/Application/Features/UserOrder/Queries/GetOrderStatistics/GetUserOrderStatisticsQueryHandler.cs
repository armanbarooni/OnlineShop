using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderStatistics
{
    public class GetUserOrderStatisticsQueryHandler : IRequestHandler<GetUserOrderStatisticsQuery, Result<UserOrderStatisticsDto>>
    {
        private readonly IUserOrderRepository _repository;

        public GetUserOrderStatisticsQueryHandler(IUserOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<UserOrderStatisticsDto>> Handle(GetUserOrderStatisticsQuery request, CancellationToken cancellationToken)
        {
            var orders = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var ordersList = orders.ToList();

            if (!ordersList.Any())
            {
                return Result<UserOrderStatisticsDto>.Success(new UserOrderStatisticsDto());
            }

            var totalOrders = ordersList.Count;
            var totalAmount = ordersList.Sum(o => o.TotalAmount);
            var averageOrderValue = totalOrders > 0 ? totalAmount / totalOrders : 0;

            var pendingOrders = ordersList.Count(o => o.OrderStatus == "Pending");
            var processingOrders = ordersList.Count(o => o.OrderStatus == "Processing");
            var shippedOrders = ordersList.Count(o => o.OrderStatus == "Shipped");
            var deliveredOrders = ordersList.Count(o => o.OrderStatus == "Delivered");
            var cancelledOrders = ordersList.Count(o => o.OrderStatus == "Cancelled");

            var ordersByStatus = ordersList
                .GroupBy(o => o.OrderStatus)
                .ToDictionary(g => g.Key, g => g.Count());

            var statistics = new UserOrderStatisticsDto
            {
                TotalOrders = totalOrders,
                TotalAmount = totalAmount,
                AverageOrderValue = averageOrderValue,
                PendingOrders = pendingOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                OrdersByStatus = ordersByStatus
            };

            return Result<UserOrderStatisticsDto>.Success(statistics);
        }
    }
}

