using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Payment;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.Services;

namespace OnlineShop.Application.Features.Payment.Commands.InitiatePayment
{
    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<PaymentInitiationResultDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IUserPaymentRepository _paymentRepository;
        private readonly IPaymentGatewayService _paymentGateway;
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;

        public InitiatePaymentCommandHandler(
            IUserOrderRepository orderRepository,
            IUserPaymentRepository paymentRepository,
            IPaymentGatewayService paymentGateway,
            IInventoryService inventoryService,
            UserManager<ApplicationUser> userManager,
            ILogger<InitiatePaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
            _inventoryService = inventoryService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<PaymentInitiationResultDto>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result<PaymentInitiationResultDto>.Failure("سفارش یافت نشد");
            }

            if (order.UserId != request.UserId)
            {
                return Result<PaymentInitiationResultDto>.Failure("دسترسی به این سفارش مجاز نیست");
            }

            if (order.OrderStatus != "Pending")
            {
                 return Result<PaymentInitiationResultDto>.Failure("این سفارش قابل پرداخت نیست");
            }

            // Create Payment Record
            var payment = OnlineShop.Domain.Entities.UserPayment.Create(
                request.UserId,
                order.Id,
                "Online", // or from request
                order.TotalAmount,
                "IRR"
            );
            
            // Call Gateway before saving to avoid multiple DbContext updates or tracking issues
            long amount = (long)order.TotalAmount;
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            var payerMobile = NormalizeMobile(user?.PhoneNumber) ?? string.Empty;

            _logger.LogInformation(
                "Initiating payment for order {OrderId}. Payer mobile is {PayerMobileStatus}.",
                order.Id,
                string.IsNullOrWhiteSpace(payerMobile) ? "not provided" : "provided");

            var gatewayResult = await _paymentGateway.InitiatePaymentAsync(order.Id, amount, payerMobile, $"Payment for Order {order.OrderNumber}");

            if (!gatewayResult.Success)
            {
                 payment.MarkAsFailed(gatewayResult.Message);
                 // Save the failed payment independently to avoid touching the AsNoTracking Order graph
                 await _paymentRepository.AddAsync(payment, cancellationToken);

                 // Cancel order and release inventory because payment initiation failed
                 order.Cancel("خطا در ارتباط با درگاه بانکی", "System");
                 await _orderRepository.UpdateAsync(order, cancellationToken);
                 await _inventoryService.ReleaseStockForCancelledOrder(order.Id, cancellationToken);

                 return Result<PaymentInitiationResultDto>.Failure(gatewayResult.Message);
            }

            payment.MarkAsProcessing(gatewayResult.Authority);
            // Save the successful payment independently
            await _paymentRepository.AddAsync(payment, cancellationToken);

            return Result<PaymentInitiationResultDto>.Success(new PaymentInitiationResultDto
            {
                PaymentUrl = gatewayResult.Url,
                Authority = gatewayResult.Authority,
                Message = "آماده انتقال به درگاه پرداخت"
            });
        }

        private static string? NormalizeMobile(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            return digits.Length == 11 && digits.StartsWith("09", StringComparison.Ordinal)
                ? digits
                : null;
        }
    }
}
