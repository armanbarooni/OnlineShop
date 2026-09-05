using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Queries.GetCart
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<CartDto>>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartQueryHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            
            if (cart == null)
            {
                // Return empty cart DTO instead of failure
                return Result<CartDto>.Success(new CartDto 
                { 
                    UserId = request.UserId.ToString(),
                    Items = new List<CartItemDto>(),
                    TotalAmount = 0,
                    Subtotal = 0,
                    TotalItems = 0
                });
            }

            var items = cart.CartItems.Select(item =>
            {
                var currentPrice = item.Product?.GetCurrentPrice() ?? item.UnitPrice;
                var originalPrice = item.Product?.Price ?? currentPrice;

                return new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown Product",
                    ProductImage = item.Product?.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                    VariantId = item.VariantId,
                    VariantInfo = GetVariantInfo(item.Product, item.VariantId),
                    OriginalUnitPrice = originalPrice,
                    UnitPrice = currentPrice,
                    HasDiscount = item.Product?.Price2 is > 0 || currentPrice < originalPrice,
                    Quantity = item.Quantity,
                    TotalPrice = currentPrice * item.Quantity,
                    AvailableStock = GetAvailableStock(item.Product, item.VariantId),
                    IsAvailable = GetAvailableStock(item.Product, item.VariantId) >= item.Quantity
                };
            }).ToList();

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId.ToString(),
                Items = items,
                Subtotal = items.Sum(i => i.TotalPrice),
                DiscountAmount = 0,
                ShippingCost = CalculateShipping(items.Sum(i => i.TotalPrice)),
                TotalAmount = items.Sum(i => i.TotalPrice) + CalculateShipping(items.Sum(i => i.TotalPrice)),
                TotalItems = items.Sum(i => i.Quantity)
            };

            return Result<CartDto>.Success(cartDto);
        }

        private string? GetVariantInfo(Domain.Entities.Product? product, Guid? variantId)
        {
            if (product == null || !variantId.HasValue || product.ProductVariants == null) return null;
            
            var variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null) return null;

            return $"سایز: {variant.Size}، رنگ: {variant.Color}";
        }

        private int GetAvailableStock(Domain.Entities.Product? product, Guid? variantId)
        {
            if (product == null) return 0;

            if (variantId.HasValue && product.ProductVariants != null)
            {
                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantId.Value);
                return variant?.GetAvailableStock() ?? 0;
            }
            return product.StockQuantity;
        }

        private decimal CalculateShipping(decimal subtotal)
        {
            return subtotal >= 10_000_000 ? 0 : 500_000;
        }
    }
}
