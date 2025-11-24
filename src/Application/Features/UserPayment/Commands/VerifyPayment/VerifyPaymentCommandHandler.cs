using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Application.Features.UserPayment.Commands.VerifyPayment
{
    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPaymentGateway _paymentGateway;

        public VerifyPaymentCommandHandler(
            IUserPaymentRepository repository, 
            IMapper mapper,
            IPaymentGateway paymentGateway)
        {
            _repository = repository;
            _mapper = mapper;
            _paymentGateway = paymentGateway;
        }

        public async Task<Result<UserPaymentDto>> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
                return Result<UserPaymentDto>.Failure("پرداخت یافت نشد");

            try
            {
                // Use TransactionId from payment if not provided in request
                var transactionId = request.TransactionId ?? payment.TransactionId;
                
                if (string.IsNullOrEmpty(transactionId))
                {
                    return Result<UserPaymentDto>.Failure("شناسه تراکنش یافت نشد");
                }

                // Verify payment with gateway
                var verifyResult = await _paymentGateway.VerifyPaymentAsync(
                    transactionId, 
                    payment.Amount, 
                    cancellationToken);

                if (verifyResult.IsSuccess && verifyResult.IsVerified)
                {
                    // Payment verified successfully
                    var gatewayResponse = request.GatewayResponse 
                        ?? $"تایید شده - RetrievalRefNo: {verifyResult.RetrievalRefNo}, SystemTraceNo: {verifyResult.SystemTraceNo}";
                    
                    payment.MarkAsPaid(
                        verifyResult.RetrievalRefNo ?? verifyResult.TransactionId ?? transactionId,
                        gatewayResponse);
                    
                    await _repository.UpdateAsync(payment, cancellationToken);

                    var dto = _mapper.Map<UserPaymentDto>(payment);
                    return Result<UserPaymentDto>.Success(dto);
                }
                else
                {
                    // Payment verification failed
                    payment.MarkAsFailed(verifyResult.ErrorMessage ?? "تایید پرداخت ناموفق بود");
                    await _repository.UpdateAsync(payment, cancellationToken);

                    return Result<UserPaymentDto>.Failure(
                        verifyResult.ErrorMessage ?? "تایید پرداخت ناموفق بود");
                }
            }
            catch (Exception ex)
            {
                payment.MarkAsFailed($"خطا در تایید پرداخت: {ex.Message}");
                await _repository.UpdateAsync(payment, cancellationToken);
                return Result<UserPaymentDto>.Failure($"خطا در تایید پرداخت: {ex.Message}");
            }
        }
    }
}

