using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Order.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _couponUsageRepository;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IUserOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICouponRepository couponRepository,
            IUserCouponUsageRepository couponUsageRepository,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _couponRepository = couponRepository;
            _couponUsageRepository = couponUsageRepository;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating order for user {UserId}", request.UserId);

            // 1. Get active cart
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            if (cart == null || !cart.CartItems.Any())
            {
                return Result<OrderDto>.Failure("سبد خرید شما خالی است");
            }

            // 2. Calculate totals
            decimal subTotal = cart.CartItems.Sum(i => i.TotalPrice);
            decimal shippingAmount = subTotal >= 10_000_000 ? 0 : 500_000;
            decimal taxAmount = 0; // Assuming tax inclusive or 0 for now
            decimal discountAmount = 0;
            OnlineShop.Domain.Entities.Coupon? appliedCoupon = null;

            // 2.1 Validate and apply coupon if provided
            if (!string.IsNullOrWhiteSpace(request.Request.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(request.Request.CouponCode, cancellationToken);
                if (coupon == null)
                {
                    return Result<OrderDto>.Failure("کد تخفیف نامعتبر است");
                }

                // Use entity's IsValid method
                if (!coupon.IsValid())
                {
                    return Result<OrderDto>.Failure("کد تخفیف منقضی شده یا غیرفعال است");
                }

                // Check minimum purchase amount
                if (subTotal < coupon.MinimumPurchase)
                {
                    return Result<OrderDto>.Failure($"حداقل مبلغ خرید برای استفاده از این کد تخفیف {coupon.MinimumPurchase:N0} ریال است");
                }

                // Check if single use and already used by this user
                if (coupon.IsSingleUse)
                {
                    var usageCount = await _couponUsageRepository.GetUsageCountByUserAsync(request.UserId, coupon.Id, cancellationToken);
                    if (usageCount > 0)
                    {
                        return Result<OrderDto>.Failure("شما قبلاً از این کد تخفیف استفاده کرده‌اید");
                    }
                }

                // Calculate discount using entity method
                discountAmount = coupon.CalculateDiscount(subTotal);
                
                if (discountAmount <= 0)
                {
                    return Result<OrderDto>.Failure("کد تخفیف برای این سفارش قابل اعمال نیست");
                }

                appliedCoupon = coupon;
                _logger.LogInformation("Coupon {CouponCode} applied. Discount: {Discount}", coupon.Code, discountAmount);
            }

            decimal totalAmount = subTotal + shippingAmount + taxAmount - discountAmount;

            // 3. Generate Order Number
            string orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

            // 4. Create Order Entity
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                request.UserId,
                orderNumber,
                subTotal,
                taxAmount,
                shippingAmount,
                discountAmount,
                totalAmount
            );

            order.SetShippingAddress(request.Request.ShippingAddressId);
            order.SetNotes(request.Request.Note);

            // 5. Add Order Items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = UserOrderItem.Create(
                    order.Id,
                    cartItem.ProductId,
                    cartItem.VariantId,
                    cartItem.Product?.Name ?? "Unknown Product",
                    cartItem.Quantity,
                    cartItem.UnitPrice,
                    cartItem.TotalPrice
                );
                order.OrderItems.Add(orderItem);
                
                // Note: Stock reduction usually happens here or after payment success.
                // For now, we'll keep stock reduction for the Payment Success handler to avoid zombie reservations.
            }

            // 6. Save Order
            await _orderRepository.AddAsync(order, cancellationToken);

            // 6.1 Record coupon usage if applied
            if (appliedCoupon is not null)
            {
                var couponUsage = UserCouponUsage.Create(
                    request.UserId,
                    appliedCoupon.Id,
                    order.Id,
                    discountAmount,
                    totalAmount  // orderTotal
                );
                await _couponUsageRepository.AddAsync(couponUsage, cancellationToken);
                _logger.LogInformation("Coupon usage recorded for order {OrderNumber}", orderNumber);
            }

            // 7. Clear Cart
            await _cartRepository.ClearCartAsync(cart.Id, cancellationToken);

            _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}", orderNumber, request.UserId);

            return Result<OrderDto>.Success(new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                TotalItems = order.OrderItems.Count
            });
        }
    }
}
