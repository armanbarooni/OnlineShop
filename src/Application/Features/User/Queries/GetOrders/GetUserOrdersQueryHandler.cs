using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.User.Queries.GetOrders
{
    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, Result<List<OrderDto>>>
    {
        private readonly IUserOrderRepository _orderRepository;

        public GetUserOrdersQueryHandler(IUserOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<List<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            
            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                TotalItems = o.OrderItems.Sum(i => i.Quantity) // Assuming OrderItems are loaded or we handle it efficiently
                // If OrderItems not loaded, this might be 0 or throw if lazy loading disabled. 
                // GetByUserIdAsync typically shouldn't load all items for performance, but if small list ok.
                // Let's check repository implementation.
            }).ToList();

            return Result<List<OrderDto>>.Success(orderDtos);
        }
    }
}
