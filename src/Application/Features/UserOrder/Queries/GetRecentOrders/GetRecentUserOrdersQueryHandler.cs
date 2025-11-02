using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetRecentOrders
{
    public class GetRecentUserOrdersQueryHandler : IRequestHandler<GetRecentUserOrdersQuery, Result<IEnumerable<UserOrderDto>>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public GetRecentUserOrdersQueryHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserOrderDto>>> Handle(GetRecentUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var allOrders = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            
            var recentOrders = allOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(request.Limit)
                .ToList();
            
            var orderDtos = _mapper.Map<IEnumerable<UserOrderDto>>(recentOrders);
            return Result<IEnumerable<UserOrderDto>>.Success(orderDtos);
        }
    }
}

