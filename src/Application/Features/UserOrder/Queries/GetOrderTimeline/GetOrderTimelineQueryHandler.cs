using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.OrderStatusHistory;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderTimeline
{
    public class GetOrderTimelineQueryHandler : IRequestHandler<GetOrderTimelineQuery, Result<OrderTimelineDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
        private readonly IMapper _mapper;

        public GetOrderTimelineQueryHandler(
            IUserOrderRepository orderRepository,
            IOrderStatusHistoryRepository statusHistoryRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<OrderTimelineDto>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
        {
            // Get the order
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<OrderTimelineDto>.Failure("سفارش یافت نشد");

            // Get status history
            var statusHistory = await _statusHistoryRepository.GetOrderTimelineAsync(request.OrderId, cancellationToken);

            // Map to DTOs
            var statusHistoryDtos = _mapper.Map<List<OrderStatusHistoryDto>>(statusHistory);

            // Create timeline DTO
            var timeline = new OrderTimelineDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CurrentStatus = order.OrderStatus,
                TrackingNumber = order.TrackingNumber,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ActualDeliveryDate = order.ActualDeliveryDate,
                StatusHistory = statusHistoryDtos
            };

            return Result<OrderTimelineDto>.Success(timeline);
        }
    }
}
