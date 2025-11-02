using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Commands.CalculateShipping
{
    public class CalculateShippingCommandHandler : IRequestHandler<CalculateShippingCommand, Result<CalculateShippingResultDto>>
    {
        private readonly IUserAddressRepository _addressRepository;

        public CalculateShippingCommandHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Result<CalculateShippingResultDto>> Handle(CalculateShippingCommand request, CancellationToken cancellationToken)
        {
            // Verify address belongs to user
            var address = await _addressRepository.GetByIdAsync(request.Request.AddressId, cancellationToken);
            if (address == null)
                return Result<CalculateShippingResultDto>.Failure("آدرس یافت نشد");

            if (address.UserId != request.UserId)
                return Result<CalculateShippingResultDto>.Failure("دسترسی به این آدرس مجاز نیست");

            // Calculate shipping cost based on method, address, and order details
            decimal baseShippingCost = 30000m;
            int estimatedDays = 3;

            // Free shipping for orders over 500,000
            if (request.Request.OrderValue.HasValue && request.Request.OrderValue.Value >= 500000)
            {
                baseShippingCost = 0;
            }

            // Adjust based on address location
            if (!address.City.Contains("تهران"))
            {
                baseShippingCost += 10000m;
                estimatedDays += 1;
            }

            // Calculate based on shipping method
            decimal finalCost = request.Request.ShippingMethodId switch
            {
                "standard" => baseShippingCost,
                "express" => baseShippingCost + 20000m,
                "same-day" => baseShippingCost + 50000m,
                _ => baseShippingCost
            };

            int finalEstimatedDays = request.Request.ShippingMethodId switch
            {
                "standard" => estimatedDays,
                "express" => estimatedDays - 1,
                "same-day" => 1,
                _ => estimatedDays
            };

            string methodName = request.Request.ShippingMethodId switch
            {
                "standard" => "ارسال استاندارد",
                "express" => "ارسال سریع",
                "same-day" => "ارسال در همان روز",
                _ => "ارسال استاندارد"
            };

            // Validate same-day availability (only for Tehran)
            if (request.Request.ShippingMethodId == "same-day" && !address.City.Contains("تهران"))
            {
                return Result<CalculateShippingResultDto>.Failure("ارسال در همان روز فقط برای تهران امکان‌پذیر است");
            }

            var result = new CalculateShippingResultDto
            {
                ShippingMethodId = request.Request.ShippingMethodId,
                ShippingMethodName = methodName,
                Cost = finalCost,
                EstimatedDays = finalEstimatedDays,
                Currency = "IRR"
            };

            return Result<CalculateShippingResultDto>.Success(result);
        }
    }
}

