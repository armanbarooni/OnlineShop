using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByTransactionId
{
    public class GetUserPaymentByTransactionIdQueryHandler : IRequestHandler<GetUserPaymentByTransactionIdQuery, Result<UserPaymentDto?>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetUserPaymentByTransactionIdQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto?>> Handle(GetUserPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);
            
            if (payment == null)
            {
                return Result<UserPaymentDto?>.Success(null);
            }

            var dto = _mapper.Map<UserPaymentDto>(payment);
            return Result<UserPaymentDto?>.Success(dto);
        }
    }
}

