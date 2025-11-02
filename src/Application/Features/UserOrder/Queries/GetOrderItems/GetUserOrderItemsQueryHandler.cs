using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderItems
{
    public class GetUserOrderItemsQueryHandler : IRequestHandler<GetUserOrderItemsQuery, Result<IEnumerable<UserOrderItemDto>>>
    {
        private readonly IUserOrderItemRepository _repository;
        private readonly IMapper _mapper;

        public GetUserOrderItemsQueryHandler(IUserOrderItemRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserOrderItemDto>>> Handle(GetUserOrderItemsQuery request, CancellationToken cancellationToken)
        {
            var orderItems = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            var orderItemDtos = _mapper.Map<IEnumerable<UserOrderItemDto>>(orderItems);
            return Result<IEnumerable<UserOrderItemDto>>.Success(orderItemDtos);
        }
    }
}

