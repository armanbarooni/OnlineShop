using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Commands.ApplyCoupon
{
    public class ApplyCouponToCartCommandHandler : IRequestHandler<ApplyCouponToCartCommand, Result<ApplyCouponToCartResultDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _userCouponUsageRepository;

        public ApplyCouponToCartCommandHandler(
            ICartRepository cartRepository,
            ICouponRepository couponRepository,
            IUserCouponUsageRepository userCouponUsageRepository)
        {
            _cartRepository = cartRepository;
            _couponRepository = couponRepository;
            _userCouponUsageRepository = userCouponUsageRepository;
        }

        public async Task<Result<ApplyCouponToCartResultDto>> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
        {
            // Get cart (from parameter or active cart)
            Domain.Entities.Cart? cart;
            
            if (request.Request.CartId.HasValue)
            {
                cart = await _cartRepository.GetByIdAsync(request.Request.CartId.Value, cancellationToken);
            }
            else
            {
                cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            }

            if (cart == null)
                return Result<ApplyCouponToCartResultDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<ApplyCouponToCartResultDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            // Get cart items and calculate subtotal
            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            if (!cartItems.Any())
                return Result<ApplyCouponToCartResultDto>.Failure("سبد خرید خالی است");

            decimal subtotal = cartItems.Sum(item => item.TotalPrice);

            // Get and validate coupon
            var coupon = await _couponRepository.GetByCodeAsync(request.Request.CouponCode, cancellationToken);
            if (coupon == null)
                return Result<ApplyCouponToCartResultDto>.Failure("کد کوپن نامعتبر است");

            if (!coupon.IsActive)
                return Result<ApplyCouponToCartResultDto>.Failure("کوپن غیرفعال است");

            if (coupon.EndDate < DateTime.UtcNow)
                return Result<ApplyCouponToCartResultDto>.Failure("کوپن منقضی شده است");

            if (coupon.MinimumPurchase > 0 && subtotal < coupon.MinimumPurchase)
                return Result<ApplyCouponToCartResultDto>.Failure($"حداقل مبلغ سفارش برای این کوپن {coupon.MinimumPurchase:N0} تومان است");

            // Check single-use constraint
            if (coupon.IsSingleUse)
            {
                var hasUsed = await _userCouponUsageRepository.HasUserUsedCouponAsync(request.UserId, coupon.Id, cancellationToken);
                if (hasUsed)
                    return Result<ApplyCouponToCartResultDto>.Failure("شما قبلاً از این کوپن استفاده کرده‌اید");
            }

            // Check usage limit
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return Result<ApplyCouponToCartResultDto>.Failure("حد استفاده از این کوپن تمام شده است");

            // Calculate discount
            var discountAmount = coupon.CalculateDiscount(subtotal);
            var finalAmount = subtotal - discountAmount;

            var cartSummary = new CartSummaryDto
            {
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                DiscountAmount = discountAmount,
                TotalAmount = finalAmount,
                Currency = "IRR"
            };

            var result = new ApplyCouponToCartResultDto
            {
                Success = true,
                Message = "کوپن با موفقیت اعمال شد",
                CouponCode = coupon.Code,
                DiscountAmount = discountAmount,
                SubTotal = subtotal,
                FinalAmount = finalAmount,
                CouponId = coupon.Id,
                CartSummary = cartSummary
            };

            return Result<ApplyCouponToCartResultDto>.Success(result);
        }
    }
}

