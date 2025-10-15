using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserOrder;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserOrder.Queries.GetById
{
    public class GetUserOrderByIdQueryHandler : IRequestHandler<GetUserOrderByIdQuery, Result<UserOrderDto>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public GetUserOrderByIdQueryHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserOrderDto>> Handle(GetUserOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var userOrder = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (userOrder == null)
                return Result<UserOrderDto>.Failure("UserOrder not found");

            return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(userOrder));
        }
    }
}


