using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserPayment;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserPayment.Queries.GetById
{
    public class GetUserPaymentByIdQueryHandler : IRequestHandler<GetUserPaymentByIdQuery, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetUserPaymentByIdQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto>> Handle(GetUserPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var userPayment = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (userPayment == null)
                return Result<UserPaymentDto>.Failure("UserPayment not found");

            return Result<UserPaymentDto>.Success(_mapper.Map<UserPaymentDto>(userPayment));
        }
    }
}


