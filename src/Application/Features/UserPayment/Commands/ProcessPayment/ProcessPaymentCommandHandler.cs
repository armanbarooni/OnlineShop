using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserPayment.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public ProcessPaymentCommandHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
                return Result<UserPaymentDto>.Failure("پرداخت یافت نشد");

            try
            {
                payment.MarkAsProcessing(request.GatewayTransactionId);
                await _repository.UpdateAsync(payment, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Result<UserPaymentDto>.Failure(ex.Message);
            }

            var dto = _mapper.Map<UserPaymentDto>(payment);
            return Result<UserPaymentDto>.Success(dto);
        }
    }
}

