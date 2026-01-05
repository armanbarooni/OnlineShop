using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Commands.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<CartDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly Microsoft.Extensions.Logging.ILogger<AddToCartCommandHandler> _logger;

        public AddToCartCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            Microsoft.Extensions.Logging.ILogger<AddToCartCommandHandler> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding product {ProductId} to cart for user {UserId}", 
                request.Item.ProductId, request.UserId);

            // 1. Validate product exists
            var product = await _productRepository.GetByIdWithIncludesAsync(request.Item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<CartDto>.Failure("محصول مورد نظر یافت نشد");
            }

            // 2. Check stock availability
            int availableStock = 0;
            if (request.Item.VariantId.HasValue)
            {
                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == request.Item.VariantId.Value);
                if (variant == null)
                {
                    return Result<CartDto>.Failure("نوع محصول (سایز/رنگ) یافت نشد");
                }
                availableStock = variant.StockQuantity;
            }
            else
            {
                availableStock = product.StockQuantity;
            }

            if (availableStock < request.Item.Quantity)
            {
                return Result<CartDto>.Failure($"موجودی کافی نیست. موجودی فعلی: {availableStock}");
            }

            // 3. Get or create cart for user
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            if (cart == null)
            {
                cart = OnlineShop.Domain.Entities.Cart.Create(request.UserId, Guid.NewGuid().ToString());
                await _cartRepository.AddAsync(cart, cancellationToken);
            }

            // 4. Check if item already exists in cart
            var existingItem = cart.CartItems.FirstOrDefault(i => 
                i.ProductId == request.Item.ProductId && 
                i.VariantId == request.Item.VariantId);

            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Item.Quantity;
                if (newQuantity > availableStock)
                {
                    return Result<CartDto>.Failure($"موجودی کافی نیست. حداکثر: {availableStock}");
                }
                existingItem.UpdateQuantity(newQuantity);
            }
            else
            {
                // Add new item
                var cartItem = CartItem.Create(
                    cart.Id,
                    request.Item.ProductId,
                    request.Item.VariantId,
                    request.Item.Quantity,
                    product.Price,
                    product.Price * request.Item.Quantity
                );
                cart.AddItem(cartItem);
            }

            await _cartRepository.UpdateAsync(cart, cancellationToken);

            // 5. Return cart DTO
            var cartDto = MapToDto(cart, product);
            
            _logger.LogInformation("Successfully added product to cart. Cart now has {ItemCount} items", 
                cart.CartItems.Count);

            return Result<CartDto>.Success(cartDto);
        }

        private CartDto MapToDto(OnlineShop.Domain.Entities.Cart cart, Domain.Entities.Product product)
        {
            var items = cart.CartItems.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? product.Name,
                ProductImage = item.Product?.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? product.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                VariantId = item.VariantId,
                VariantInfo = GetVariantInfo(item.Product ?? product, item.VariantId),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice,
                AvailableStock = GetAvailableStock(item.Product ?? product, item.VariantId),
                IsAvailable = GetAvailableStock(item.Product ?? product, item.VariantId) >= item.Quantity
            }).ToList();

            return new CartDto
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
        }

        private string? GetVariantInfo(Domain.Entities.Product product, Guid? variantId)
        {
            if (!variantId.HasValue || product.ProductVariants == null) return null;
            
            var variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null) return null;

            return $"سایز: {variant.Size}, رنگ: {variant.Color}";
        }

        private int GetAvailableStock(Domain.Entities.Product product, Guid? variantId)
        {
            if (variantId.HasValue && product.ProductVariants != null)
            {
                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantId.Value);
                return variant?.StockQuantity ?? 0;
            }
            return product.StockQuantity;
        }

        private decimal CalculateShipping(decimal subtotal)
        {
            return subtotal >= 10_000_000 ? 0 : 500_000;
        }
    }
}
