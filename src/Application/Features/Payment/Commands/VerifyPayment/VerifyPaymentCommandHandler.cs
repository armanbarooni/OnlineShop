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
        private readonly IMahakOrderSyncService _mahakOrderSyncService;
        private readonly IMahakInventorySyncService _mahakInventorySyncService;
        private readonly ISmsService _smsService;
        private readonly ILogger<VerifyPaymentCommandHandler> _logger;
        private const int OrderPaidSmsTemplateId = 461054;
        private const string OrderPaidFactorIdParameter = "FACTORID";

        public VerifyPaymentCommandHandler(
            IUserOrderRepository orderRepository,
            IPaymentGatewayService paymentGateway,
            IInventoryService inventoryService,
            IMahakOrderSyncService mahakOrderSyncService,
            IMahakInventorySyncService mahakInventorySyncService,
            ISmsService smsService,
            ILogger<VerifyPaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _inventoryService = inventoryService;
            _mahakOrderSyncService = mahakOrderSyncService;
            _mahakInventorySyncService = mahakInventorySyncService;
            _smsService = smsService;
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

            if (!string.Equals(request.Request.Status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                payment.MarkAsFailed("تراکنش ناموفق یا لغو توسط کاربر");
                
                // Cancel order and release stock immediately
                order.Cancel("پرداخت ناموفق یا لغو توسط کاربر");
                await _inventoryService.ReleaseStockForCancelledOrder(order.Id, cancellationToken);
                
                await _orderRepository.UpdateAsync(order, cancellationToken);

                return Result<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
                {
                    IsSuccess = false,
                    Message = "تراکنش لغو شده است",
                    OrderId = order.Id
                });
            }

            // Verify with Gateway
            var verificationResult = await _paymentGateway.VerifyPaymentAsync(request.Request.Authority, (long)payment.Amount);

            if (!verificationResult.Success)
            {
                payment.MarkAsFailed(verificationResult.Message);
                
                // Cancel order and release stock immediately
                order.Cancel("پرداخت ناموفق یا لغو توسط کاربر");
                await _inventoryService.ReleaseStockForCancelledOrder(order.Id, cancellationToken);
                
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
            await SendPaymentConfirmedSmsAsync(order, cancellationToken);

            var orderSyncedToMahak = false;
            try
            {
                await _mahakOrderSyncService.SyncOrderToMahakAsync(order.Id, cancellationToken);
                orderSyncedToMahak = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment for order {OrderId} was verified but Mahak sync failed. Order will remain pending for retry.", order.Id);
            }

            if (orderSyncedToMahak)
            {
                try
                {
                    await _mahakInventorySyncService.SyncInventoryFromMahakAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Order {OrderId} synced to Mahak but inventory refresh from Mahak failed. Periodic sync will retry.", order.Id);
                }
            }

            _logger.LogInformation("Payment verified and order {OrderNumber} confirmed. Stock updated.", order.OrderNumber);

            return Result<PaymentVerificationResultDto>.Success(new PaymentVerificationResultDto
            {
                IsSuccess = true,
                RefId = verificationResult.RefId,
                Message = "پرداخت با موفقیت انجام شد",
                OrderId = order.Id
            });
        }

        private async Task SendPaymentConfirmedSmsAsync(Domain.Entities.UserOrder order, CancellationToken cancellationToken)
        {
            var phoneNumber = order.User?.PhoneNumber;
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning(
                    "Payment for order {OrderNumber} was confirmed but customer phone number is missing. Confirmation SMS was not sent.",
                    order.OrderNumber);
                return;
            }

            var sent = await _smsService.SendTemplateAsync(
                phoneNumber,
                OrderPaidSmsTemplateId,
                new Dictionary<string, string>
                {
                    [OrderPaidFactorIdParameter] = order.OrderNumber
                },
                cancellationToken);

            if (sent)
            {
                _logger.LogInformation(
                    "Payment confirmation SMS sent for order {OrderNumber} to customer {UserId}.",
                    order.OrderNumber,
                    order.UserId);
                return;
            }

            _logger.LogWarning(
                "Payment confirmation SMS failed for order {OrderNumber} to customer {UserId}.",
                order.OrderNumber,
                order.UserId);
        }
    }
}
