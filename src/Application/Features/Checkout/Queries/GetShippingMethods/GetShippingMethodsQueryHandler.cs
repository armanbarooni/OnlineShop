using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Queries.GetShippingMethods
{
    public class GetShippingMethodsQueryHandler : IRequestHandler<GetShippingMethodsQuery, Result<IEnumerable<ShippingMethodDto>>>
    {
        private readonly IUserAddressRepository _addressRepository;

        public GetShippingMethodsQueryHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Result<IEnumerable<ShippingMethodDto>>> Handle(GetShippingMethodsQuery request, CancellationToken cancellationToken)
        {
            // Check if address exists if provided
            if (request.AddressId.HasValue)
            {
                var address = await _addressRepository.GetByIdAsync(request.AddressId.Value, cancellationToken);
                if (address == null)
                    return Result<IEnumerable<ShippingMethodDto>>.Failure("آدرس یافت نشد");
            }

            // Calculate shipping cost based on order value and address
            decimal baseShippingCost = 30000m; // Base shipping cost
            int estimatedDays = 3; // Default estimated days

            // Free shipping for orders over 500,000
            if (request.OrderValue.HasValue && request.OrderValue.Value >= 500000)
            {
                baseShippingCost = 0;
            }

            // Adjust based on address location (can be enhanced)
            if (request.AddressId.HasValue)
            {
                var address = await _addressRepository.GetByIdAsync(request.AddressId.Value, cancellationToken);
                if (address != null)
                {
                    // For remote cities, increase cost and days
                    if (address.City.Contains("تهران") == false)
                    {
                        baseShippingCost += 10000m;
                        estimatedDays += 1;
                    }
                }
            }

            var shippingMethods = new List<ShippingMethodDto>
            {
                new ShippingMethodDto
                {
                    Id = "standard",
                    Name = "ارسال استاندارد",
                    Description = "ارسال پستی معمولی",
                    Cost = baseShippingCost,
                    EstimatedDays = estimatedDays,
                    IsAvailable = true
                },
                new ShippingMethodDto
                {
                    Id = "express",
                    Name = "ارسال سریع",
                    Description = "ارسال سریع (پست پیشتاز)",
                    Cost = baseShippingCost + 20000m,
                    EstimatedDays = estimatedDays - 1,
                    IsAvailable = true
                },
                new ShippingMethodDto
                {
                    Id = "same-day",
                    Name = "ارسال در همان روز",
                    Description = "ارسال پیک موتوری (فقط تهران)",
                    Cost = baseShippingCost + 50000m,
                    EstimatedDays = 1,
                    IsAvailable = request.AddressId.HasValue // Only available if address is provided
                }
            };

            return Result<IEnumerable<ShippingMethodDto>>.Success(shippingMethods);
        }
    }
}

