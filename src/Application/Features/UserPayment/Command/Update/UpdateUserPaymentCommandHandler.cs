using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserPayment;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserPayment.Command.Update
{
    public class UpdateUserPaymentCommandHandler : IRequestHandler<UpdateUserPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public UpdateUserPaymentCommandHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto>> Handle(UpdateUserPaymentCommand request, CancellationToken cancellationToken)
        {
            var userPayment = await _repository.GetByIdAsync(request.UserPayment.Id, cancellationToken);
            if (userPayment == null)
                return Result<UserPaymentDto>.Failure("UserPayment not found");

            userPayment.Update(
                request.UserPayment.PaymentMethod,
                request.UserPayment.Amount,
                request.UserPayment.Currency,
                "System"
            );

            await _repository.UpdateAsync(userPayment, cancellationToken);
            return Result<UserPaymentDto>.Success(_mapper.Map<UserPaymentDto>(userPayment));
        }
    }
}


