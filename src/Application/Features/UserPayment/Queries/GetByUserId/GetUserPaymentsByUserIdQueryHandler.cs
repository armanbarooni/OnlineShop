using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserPayment;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserPayment.Queries.GetByUserId
{
    public class GetUserPaymentsByUserIdQueryHandler : IRequestHandler<GetUserPaymentsByUserIdQuery, Result<IEnumerable<UserPaymentDto>>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetUserPaymentsByUserIdQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserPaymentDto>>> Handle(GetUserPaymentsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userPayments = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var userPaymentDtos = _mapper.Map<IEnumerable<UserPaymentDto>>(userPayments);
            return Result<IEnumerable<UserPaymentDto>>.Success(userPaymentDtos);
        }
    }
}


