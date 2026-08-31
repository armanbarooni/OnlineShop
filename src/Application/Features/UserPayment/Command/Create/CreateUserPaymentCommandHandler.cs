using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Application.Features.UserPayment.Command.Create
{
    public class CreateUserPaymentCommandHandler : IRequestHandler<CreateUserPaymentCommand, Result<UserPaymentDto>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IUserOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IConfiguration _configuration;

        public CreateUserPaymentCommandHandler(
            IUserPaymentRepository repository, 
            IUserOrderRepository orderRepository,
            IMapper mapper,
            IPaymentGateway paymentGateway,
            IConfiguration configuration)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _paymentGateway = paymentGateway;
            _configuration = configuration;
        }

        public async Task<Result<UserPaymentDto>> Handle(CreateUserPaymentCommand request, CancellationToken cancellationToken)
        {
            if (!request.UserPayment.OrderId.HasValue)
                return Result<UserPaymentDto>.Failure("سفارش برای پرداخت مشخص نشده است");

            var order = await _orderRepository.GetByIdAsync(request.UserPayment.OrderId.Value, cancellationToken);
            if (order == null)
                return Result<UserPaymentDto>.Failure("سفارش یافت نشد");

            if (order.UserId != request.UserPayment.UserId)
                return Result<UserPaymentDto>.Failure("دسترسی به این سفارش مجاز نیست");

            var payableAmount = order.TotalAmount;

            var userPayment = Domain.Entities.UserPayment.Create(
                request.UserPayment.UserId,
                request.UserPayment.OrderId,
                request.UserPayment.PaymentMethod,
                payableAmount,
                order.Currency
            );

            await _repository.AddAsync(userPayment, cancellationToken);

            // Request payment from gateway if payment method is online
            if (request.UserPayment.PaymentMethod.Equals("online", StringComparison.OrdinalIgnoreCase) ||
                request.UserPayment.PaymentMethod.Equals("sadad", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Get callback URL from configuration
                    var callbackUrl = _configuration["SadadGateway:CallbackUrl"] 
                        ?? $"{_configuration["BaseUrl"]}/api/UserPayment/callback";

                    var paymentRequest = new PaymentRequest
                    {
                        PaymentId = userPayment.Id,
                        OrderId = request.UserPayment.OrderId ?? Guid.Empty,
                        Amount = payableAmount,
                        CallbackUrl = callbackUrl,
                        Description = $"پرداخت سفارش {request.UserPayment.OrderId}"
                    };

                    var paymentResult = await _paymentGateway.RequestPaymentAsync(paymentRequest, cancellationToken);

                    if (paymentResult.IsSuccess && !string.IsNullOrEmpty(paymentResult.Token))
                    {
                        // Store token in TransactionId
                        userPayment.SetTransactionId(paymentResult.Token);
                        await _repository.UpdateAsync(userPayment, cancellationToken);

                        // Map to DTO and set PaymentUrl
                        var dto = _mapper.Map<UserPaymentDto>(userPayment);
                        dto.PaymentUrl = paymentResult.PaymentUrl;
                        return Result<UserPaymentDto>.Success(dto);
                    }
                    else
                    {
                        // Mark payment as failed
                        userPayment.MarkAsFailed(paymentResult.ErrorMessage ?? "خطا در ارتباط با درگاه پرداخت");
                        await _repository.UpdateAsync(userPayment, cancellationToken);

                        return Result<UserPaymentDto>.Failure(
                            paymentResult.ErrorMessage ?? "خطا در ارتباط با درگاه پرداخت");
                    }
                }
                catch (Exception ex)
                {
                    userPayment.MarkAsFailed($"خطا در ارتباط با درگاه پرداخت: {ex.Message}");
                    await _repository.UpdateAsync(userPayment, cancellationToken);
                    return Result<UserPaymentDto>.Failure($"خطا در ارتباط با درگاه پرداخت: {ex.Message}");
                }
            }

            // For non-online payment methods, return without gateway URL
            return Result<UserPaymentDto>.Success(_mapper.Map<UserPaymentDto>(userPayment));
        }
    }
}


