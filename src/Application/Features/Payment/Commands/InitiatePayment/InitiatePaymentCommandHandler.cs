using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Payment;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Payment.Commands.InitiatePayment
{
    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<PaymentInitiationResultDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IPaymentGatewayService _paymentGateway;
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;
        // Need PaymentRepository if available, strictly speaking. Or just save via db context if using generic repo pattern?
        // Assuming we need to add a Payment repository interface or use a generic one.
        // For now preventing compilation error by assuming we might need to add it or use DbContext.
        // Let's implement a quick Mock Gateway inside Application or Infrastructure.
        
        // I'll check if IUserPaymentRepository exists.

        public InitiatePaymentCommandHandler(
            IUserOrderRepository orderRepository,
            IPaymentGatewayService paymentGateway,
            ILogger<InitiatePaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
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

            // Create Payment Record (Ideally via Repository)
            // Since we don't have PaymentRepository injected yet, I will simulate logic or rely on Navigation prop if possible
            // But UserOrder.Payments is a collection.
            
            var payment = OnlineShop.Domain.Entities.UserPayment.Create(
                request.UserId,
                order.Id,
                "Online", // or from request
                order.TotalAmount,
                "IRR"
            );
            
            order.Payments.Add(payment);
            await _orderRepository.UpdateAsync(order, cancellationToken); // Saving via Order aggregate root

            // Call Gateway
            long amount = (long)order.TotalAmount;
            var gatewayResult = await _paymentGateway.InitiatePaymentAsync(order.Id, amount, "09123456789", $"Payment for Order {order.OrderNumber}");

            if (!gatewayResult.Success)
            {
                 payment.MarkAsFailed(gatewayResult.Message);
                 await _orderRepository.UpdateAsync(order, cancellationToken);
                 return Result<PaymentInitiationResultDto>.Failure(gatewayResult.Message);
            }

            payment.MarkAsProcessing(gatewayResult.Authority);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            return Result<PaymentInitiationResultDto>.Success(new PaymentInitiationResultDto
            {
                PaymentUrl = gatewayResult.Url,
                Authority = gatewayResult.Authority,
                Message = "آماده انتقال به درگاه پرداخت"
            });
        }
    }
}
