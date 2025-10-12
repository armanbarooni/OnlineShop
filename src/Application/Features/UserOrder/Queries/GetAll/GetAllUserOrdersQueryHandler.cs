using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetAll
{
    public class GetAllUserOrdersQueryHandler : IRequestHandler<GetAllUserOrdersQuery, Result<IEnumerable<UserOrderDto>>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public GetAllUserOrdersQueryHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserOrderDto>>> Handle(GetAllUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var userOrders = await _repository.GetAllAsync(cancellationToken);
            var userOrderDtos = _mapper.Map<IEnumerable<UserOrderDto>>(userOrders);
            return Result<IEnumerable<UserOrderDto>>.Success(userOrderDtos);
        }
    }
}
