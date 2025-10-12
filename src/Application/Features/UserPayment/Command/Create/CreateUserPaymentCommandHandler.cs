using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.UserPayment.Command.Create
{
    public class CreateUserPaymentCommandHandler : IRequestHandler<CreateUserPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public CreateUserPaymentCommandHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto>> Handle(CreateUserPaymentCommand request, CancellationToken cancellationToken)
        {
            var userPayment = UserPayment.Create(
                request.UserPayment.UserId,
                request.UserPayment.OrderId,
                request.UserPayment.PaymentMethod,
                request.UserPayment.Amount,
                request.UserPayment.Currency
            );

            await _repository.AddAsync(userPayment, cancellationToken);
            return Result<UserPaymentDto>.Success(_mapper.Map<UserPaymentDto>(userPayment));
        }
    }
}
