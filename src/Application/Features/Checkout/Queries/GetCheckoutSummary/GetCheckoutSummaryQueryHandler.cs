using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Queries.GetCheckoutSummary
{
    public class GetCheckoutSummaryQueryHandler : IRequestHandler<GetCheckoutSummaryQuery, Result<OrderSummaryDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetCheckoutSummaryQueryHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<OrderSummaryDto>> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
        {
            // Get cart (from parameter or active cart)
            Domain.Entities.Cart? cart;
            
            if (request.CartId.HasValue && request.CartId.Value != Guid.Empty)
            {
                cart = await _cartRepository.GetByIdAsync(request.CartId.Value, cancellationToken);
            }
            else
            {
                cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            }

            if (cart == null)
                return Result<OrderSummaryDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<OrderSummaryDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            if (!cartItems.Any())
                return Result<OrderSummaryDto>.Failure("سبد خرید خالی است");

            // Calculate totals
            decimal subtotal = 0;
            var itemSummaries = new List<OrderItemSummaryDto>();

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                var productName = product?.Name ?? "Unknown Product";
                
                subtotal += item.TotalPrice;

                itemSummaries.Add(new OrderItemSummaryDto
                {
                    ProductId = item.ProductId,
                    ProductName = productName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                });
            }

            // Default calculations (can be enhanced with coupon, shipping, etc.)
            var taxAmount = subtotal * 0.09m; // 9% tax
            var shippingAmount = subtotal < 500000 ? 30000m : 0m; // Free shipping over 500k
            var discountAmount = 0m;
            var totalAmount = subtotal + taxAmount + shippingAmount - discountAmount;

            var summary = new OrderSummaryDto
            {
                OrderId = Guid.Empty, // Not yet created
                OrderNumber = string.Empty,
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Currency = "IRR",
                OrderDate = DateTime.UtcNow,
                Items = itemSummaries
            };

            return Result<OrderSummaryDto>.Success(summary);
        }
    }
}

