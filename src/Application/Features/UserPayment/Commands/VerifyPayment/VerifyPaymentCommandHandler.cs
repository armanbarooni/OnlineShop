using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserPayment.Commands.VerifyPayment
{
    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public VerifyPaymentCommandHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserPaymentDto>> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
                return Result<UserPaymentDto>.Failure("پرداخت یافت نشد");

            try
            {
                payment.MarkAsPaid(request.TransactionId, request.GatewayResponse);
                await _repository.UpdateAsync(payment, cancellationToken);

                var dto = _mapper.Map<UserPaymentDto>(payment);
                return Result<UserPaymentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserPaymentDto>.Failure($"خطا در تایید پرداخت: {ex.Message}");
            }
        }
    }
}

