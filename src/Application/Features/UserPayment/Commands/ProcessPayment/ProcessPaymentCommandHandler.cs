using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Application.Features.UserPayment.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPaymentGateway _paymentGateway;

        public ProcessPaymentCommandHandler(
            IUserPaymentRepository repository, 
            IMapper mapper,
            IPaymentGateway paymentGateway)
        {
            _repository = repository;
            _mapper = mapper;
            _paymentGateway = paymentGateway;
        }

        public async Task<Result<UserPaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
                return Result<UserPaymentDto>.Failure("پرداخت یافت نشد");

            try
            {
                // Mark as processing
                payment.MarkAsProcessing(request.GatewayTransactionId);
                await _repository.UpdateAsync(payment, cancellationToken);

                // If we have a transaction ID (Token), verify the payment
                if (!string.IsNullOrEmpty(request.GatewayTransactionId))
                {
                    var verifyResult = await _paymentGateway.VerifyPaymentAsync(
                        request.GatewayTransactionId, 
                        payment.Amount, 
                        cancellationToken);

                    if (verifyResult.IsSuccess && verifyResult.IsVerified)
                    {
                        // Payment verified successfully
                        payment.MarkAsPaid(
                            verifyResult.RetrievalRefNo ?? verifyResult.TransactionId,
                            $"تایید شده - RetrievalRefNo: {verifyResult.RetrievalRefNo}, SystemTraceNo: {verifyResult.SystemTraceNo}");
                    }
                    else
                    {
                        // Payment verification failed
                        payment.MarkAsFailed(verifyResult.ErrorMessage ?? "تایید پرداخت ناموفق بود");
                    }

                    await _repository.UpdateAsync(payment, cancellationToken);
                }
            }
            catch (InvalidOperationException ex)
            {
                return Result<UserPaymentDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                payment.MarkAsFailed($"خطا در پردازش پرداخت: {ex.Message}");
                await _repository.UpdateAsync(payment, cancellationToken);
                return Result<UserPaymentDto>.Failure($"خطا در پردازش پرداخت: {ex.Message}");
            }

            var dto = _mapper.Map<UserPaymentDto>(payment);
            return Result<UserPaymentDto>.Success(dto);
        }
    }
}

