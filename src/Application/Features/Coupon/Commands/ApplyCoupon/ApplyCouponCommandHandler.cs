using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Coupon.Commands.ApplyCoupon
{
    public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, Result<ApplyCouponResultDto>>
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _userCouponUsageRepository;

        public ApplyCouponCommandHandler(
            ICouponRepository couponRepository,
            IUserCouponUsageRepository userCouponUsageRepository)
        {
            _couponRepository = couponRepository;
            _userCouponUsageRepository = userCouponUsageRepository;
        }

        public async Task<Result<ApplyCouponResultDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return Result<ApplyCouponResultDto>.Failure("کد کوپن الزامی است");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return Result<ApplyCouponResultDto>.Failure("شناسه کاربر الزامی است");

            if (request.OrderId == Guid.Empty)
                return Result<ApplyCouponResultDto>.Failure("شناسه سفارش الزامی است");

            if (request.OrderTotal <= 0)
                return Result<ApplyCouponResultDto>.Failure("مجموع سفارش باید بزرگتر از صفر باشد");

            // Get coupon by code
            var coupon = await _couponRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (coupon == null)
                return Result<ApplyCouponResultDto>.Failure("کوپن یافت نشد");

            // Validate coupon
            if (!coupon.IsValid())
                return Result<ApplyCouponResultDto>.Failure("کوپن منقضی شده یا غیرفعال است");

            if (request.OrderTotal < coupon.MinimumPurchase)
                return Result<ApplyCouponResultDto>.Failure($"حداقل خرید برای این کوپن {coupon.MinimumPurchase:C} تومان است");

            // Check if user has already used this coupon (for single-use coupons)
            if (coupon.IsSingleUse)
            {
                var hasUsed = await _userCouponUsageRepository.HasUserUsedCouponAsync(Guid.Parse(request.UserId), coupon.Id, cancellationToken);
                if (hasUsed)
                    return Result<ApplyCouponResultDto>.Failure("شما قبلاً از این کوپن استفاده کرده‌اید");
            }

            // Check usage limit
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return Result<ApplyCouponResultDto>.Failure("حد استفاده از این کوپن تمام شده است");

            // Calculate discount amount
            var discountAmount = coupon.CalculateDiscount(request.OrderTotal);
            var finalAmount = request.OrderTotal - discountAmount;

            // Create usage record
            var usage = UserCouponUsage.Create(
                Guid.Parse(request.UserId),
                coupon.Id,
                request.OrderId,
                discountAmount,
                request.OrderTotal,
                $"کوپن {coupon.Code} اعمال شد"
            );

            await _userCouponUsageRepository.AddAsync(usage, cancellationToken);

            // Increment coupon usage count
            coupon.IncrementUsage();
            await _couponRepository.UpdateAsync(coupon, cancellationToken);

            return Result<ApplyCouponResultDto>.Success(new ApplyCouponResultDto
            {
                Success = true,
                Message = "کوپن با موفقیت اعمال شد",
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                CouponId = coupon.Id
            });
        }
    }
}
