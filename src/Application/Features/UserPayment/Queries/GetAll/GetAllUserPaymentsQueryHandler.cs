using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetAll
{
    public class GetAllUserPaymentsQueryHandler : IRequestHandler<GetAllUserPaymentsQuery, Result<IEnumerable<UserPaymentDto>>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetAllUserPaymentsQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserPaymentDto>>> Handle(GetAllUserPaymentsQuery request, CancellationToken cancellationToken)
        {
            var userPayments = await _repository.GetAllAsync(cancellationToken);
            var userPaymentDtos = _mapper.Map<IEnumerable<UserPaymentDto>>(userPayments);
            return Result<IEnumerable<UserPaymentDto>>.Success(userPaymentDtos);
        }
    }
}
