using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Payment;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Payment.Commands.VerifyPayment
{
    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result<PaymentVerificationResultDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IPaymentGatewayService _paymentGateway;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<VerifyPaymentCommandHandler> _logger;

        public VerifyPaymentCommandHandler(
            IUserOrderRepository orderRepository,
            IPaymentGatewayService paymentGateway,
            IInventoryService inventoryService,
            ILogger<VerifyPaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<Result<PaymentVerificationResultDto>> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verifying payment with authority: {Authority}", request.Request.Authority);

            var order = await _orderRepository.GetByPaymentAuthorityAsync(request.Request.Authority, cancellationToken);
            if (order == null)
            {
                return Result<PaymentVerificationResultDto>.Failure("سفارش با این شناسه پرداخت یافت نشد");
            }

            var payment = order.Payments.FirstOrDefault(p => p.TransactionId == request.Request.Authority);
            if (payment == null)
            {
                return Result<PaymentVerificationResultDto>.Failure("رکورد پرداخت یافت نشد");
            }

            if (payment.PaymentStatus == "Paid")
            {
                return Result<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
                {
                    IsSuccess = true,
                    OrderId = order.Id,
                    RefId = payment.TransactionId ?? "",
                    Message = "این سفارش قبلاً پرداخت شده است"
                });
            }

            // Verify with Gateway
            var verificationResult = await _paymentGateway.VerifyPaymentAsync(request.Request.Authority, (long)payment.Amount);

            if (!verificationResult.Success)
            {
                payment.MarkAsFailed(verificationResult.Message);
                await _orderRepository.UpdateAsync(order, cancellationToken);

                return Result<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
                {
                    IsSuccess = false,
                    Message = verificationResult.Message,
                    OrderId = order.Id
                });
            }

            // Success Logic
            payment.MarkAsPaid(verificationResult.RefId, verificationResult.Message);
            order.Confirm(); // Change status to Confirmed

            // Reduce Stock
            // Reduce Stock using Inventory Service (Atomic)
            try
            {
                await _inventoryService.CommitOrder(order.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit stock for order {OrderId}. Manual intervention required.", order.Id);
                // We still verify the payment, but log the error. Admin should monitor logs.
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            _logger.LogInformation("Payment verified and order {OrderNumber} confirmed. Stock updated.", order.OrderNumber);

            return Result<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
            {
                IsSuccess = true,
                RefId = verificationResult.RefId,
                Message = "پرداخت با موفقیت انجام شد",
                OrderId = order.Id
            });
        }
    }
}
