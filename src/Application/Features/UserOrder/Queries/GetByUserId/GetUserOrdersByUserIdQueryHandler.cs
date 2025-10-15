using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetByUserId
{
    public class GetUserOrdersByUserIdQueryHandler : IRequestHandler<GetUserOrdersByUserIdQuery, Result<IEnumerable<UserOrderDto>>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public GetUserOrdersByUserIdQueryHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserOrderDto>>> Handle(GetUserOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userOrders = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var userOrderDtos = _mapper.Map<IEnumerable<UserOrderDto>>(userOrders);
            return Result<IEnumerable<UserOrderDto>>.Success(userOrderDtos);
        }
    }
}

