using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserPayment;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserPayment.Queries.GetByOrderId
{
    public class GetUserPaymentsByOrderIdQueryHandler : IRequestHandler<GetUserPaymentsByOrderIdQuery, Result<IEnumerable<UserPaymentDto>>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetUserPaymentsByOrderIdQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserPaymentDto>>> Handle(GetUserPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var userPayments = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            var userPaymentDtos = _mapper.Map<IEnumerable<UserPaymentDto>>(userPayments);
            return Result<IEnumerable<UserPaymentDto>>.Success(userPaymentDtos);
        }
    }
}


