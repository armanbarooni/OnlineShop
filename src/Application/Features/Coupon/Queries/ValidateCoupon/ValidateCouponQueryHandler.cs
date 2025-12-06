using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Coupon.Queries.ValidateCoupon
{
    public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, Result<CouponValidationResultDto>>
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _userCouponUsageRepository;

        public ValidateCouponQueryHandler(
            ICouponRepository couponRepository,
            IUserCouponUsageRepository userCouponUsageRepository)
        {
            _couponRepository = couponRepository;
            _userCouponUsageRepository = userCouponUsageRepository;
        }

        public async Task<Result<CouponValidationResultDto>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return Result<CouponValidationResultDto>.Failure("کد کوپن الزامی است");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return Result<CouponValidationResultDto>.Failure("شناسه کاربر الزامی است");

            if (request.OrderTotal <= 0)
                return Result<CouponValidationResultDto>.Failure("مجموع سفارش باید بزرگتر از صفر باشد");

            // Get coupon by code
            var coupon = await _couponRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (coupon == null)
            {
                return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "کوپن یافت نشد"
                });
            }

            // Check if coupon is valid
            if (!coupon.IsValid())
            {
                return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "کوپن منقضی شده یا غیرفعال است"
                });
            }

            // Check minimum purchase requirement
            if (request.OrderTotal < coupon.MinimumPurchase)
            {
                return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = $"حداقل خرید برای این کوپن {coupon.MinimumPurchase:C} تومان است"
                });
            }

            // Check if user has already used this coupon (for single-use coupons)
            if (coupon.IsSingleUse)
            {
                var hasUsed = await _userCouponUsageRepository.HasUserUsedCouponAsync(Guid.Parse(request.UserId), coupon.Id, cancellationToken);
                if (hasUsed)
                {
                    return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "شما قبلاً از این کوپن استفاده کرده‌اید"
                    });
                }
            }

            // Check usage limit
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
            {
                return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "حد استفاده از این کوپن تمام شده است"
                });
            }

            // Calculate discount amount
            var discountAmount = coupon.CalculateDiscount(request.OrderTotal);

            return Result<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = true,
                Message = "کوپن معتبر است",
                DiscountAmount = discountAmount,
                Coupon = new CouponDto
                {
                    Id = coupon.Id,
                    Code = coupon.Code,
                    Name = coupon.Name,
                    Description = coupon.Description,
                    DiscountPercentage = coupon.DiscountPercentage,
                    DiscountAmount = coupon.DiscountAmount,
                    MinimumPurchase = coupon.MinimumPurchase,
                    MaximumDiscount = coupon.MaximumDiscount,
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    UsageLimit = coupon.UsageLimit,
                    UsedCount = coupon.UsedCount,
                    IsActive = coupon.IsActive,
                    IsSingleUse = coupon.IsSingleUse,
                    ApplicableCategories = coupon.ApplicableCategories,
                    ApplicableProducts = coupon.ApplicableProducts,
                    ApplicableUsers = coupon.ApplicableUsers,
                    CreatedAt = coupon.CreatedAt,
                    UpdatedAt = coupon.UpdatedAt
                }
            });
        }
    }
}
