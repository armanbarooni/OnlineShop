using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Commands.ApplyCouponToCheckout
{
    public class ApplyCouponToCheckoutCommandHandler : IRequestHandler<ApplyCouponToCheckoutCommand, Result<ApplyCouponToCheckoutResultDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _userCouponUsageRepository;

        public ApplyCouponToCheckoutCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICouponRepository couponRepository,
            IUserCouponUsageRepository userCouponUsageRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _couponRepository = couponRepository;
            _userCouponUsageRepository = userCouponUsageRepository;
        }

        public async Task<Result<ApplyCouponToCheckoutResultDto>> Handle(ApplyCouponToCheckoutCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate cart
            var cart = await _cartRepository.GetByIdAsync(request.Request.CartId, cancellationToken);
            if (cart == null)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            if (!cartItems.Any())
                return Result<ApplyCouponToCheckoutResultDto>.Failure("سبد خرید خالی است");

            // 2. Calculate cart total
            decimal subtotal = 0;
            foreach (var item in cartItems)
            {
                subtotal += item.TotalPrice;
            }

            // 3. Get and validate coupon
            var coupon = await _couponRepository.GetByCodeAsync(request.Request.CouponCode, cancellationToken);
            if (coupon == null)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("کد کوپن نامعتبر است");

            if (!coupon.IsActive)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("کوپن غیرفعال است");

            if (coupon.EndDate < DateTime.UtcNow)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("کوپن منقضی شده است");

            if (coupon.MinimumPurchase > 0 && subtotal < coupon.MinimumPurchase)
                return Result<ApplyCouponToCheckoutResultDto>.Failure($"حداقل مبلغ سفارش برای این کوپن {coupon.MinimumPurchase:N0} تومان است");

            // Check single-use constraint
            if (coupon.IsSingleUse)
            {
                var hasUsed = await _userCouponUsageRepository.HasUserUsedCouponAsync(request.UserId, coupon.Id, cancellationToken);
                if (hasUsed)
                    return Result<ApplyCouponToCheckoutResultDto>.Failure("شما قبلاً از این کوپن استفاده کرده‌اید");
            }

            // Check usage limit
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return Result<ApplyCouponToCheckoutResultDto>.Failure("حد استفاده از این کوپن تمام شده است");

            // 4. Calculate discount
            var discountAmount = coupon.CalculateDiscount(subtotal);
            var finalAmount = subtotal - discountAmount;

            var result = new ApplyCouponToCheckoutResultDto
            {
                Success = true,
                Message = "کوپن با موفقیت اعمال شد",
                CouponCode = coupon.Code,
                DiscountAmount = discountAmount,
                SubTotal = subtotal,
                FinalAmount = finalAmount,
                CouponId = coupon.Id,
                CouponName = coupon.Name,
                CouponDescription = coupon.Description
            };

            return Result<ApplyCouponToCheckoutResultDto>.Success(result);
        }
    }
}

