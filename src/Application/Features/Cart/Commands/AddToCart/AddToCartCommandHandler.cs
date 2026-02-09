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
        private readonly ILogger<AddToCartCommandHandler> _logger;

        public AddToCartCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ILogger<AddToCartCommandHandler> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding product {ProductId} to cart for user {UserId}",
                request.Item.ProductId, request.UserId);

            var product = await _productRepository.GetByIdWithIncludesAsync(request.Item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<CartDto>.Failure("Product not found");
            }

            if (request.Item.VariantId == Guid.Empty)
            {
                return Result<CartDto>.Failure("Variant is required");
            }

            var variant = product.ProductVariants.FirstOrDefault(v => v.Id == request.Item.VariantId);
            if (variant == null)
            {
                return Result<CartDto>.Failure("Product variant not found");
            }

            var availableStock = variant.StockQuantity;
            if (availableStock < request.Item.Quantity)
            {
                return Result<CartDto>.Failure($"Insufficient stock. Available: {availableStock}");
            }

            const int maxRetries = 3;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
                    if (cart == null)
                    {
                        cart = OnlineShop.Domain.Entities.Cart.Create(request.UserId, Guid.NewGuid().ToString());
                        await _cartRepository.AddAsync(cart, cancellationToken);
                    }

                    var existingItem = cart.CartItems.FirstOrDefault(i =>
                        i.ProductId == request.Item.ProductId &&
                        i.VariantId == request.Item.VariantId);

                    if (existingItem != null)
                    {
                        var newQuantity = existingItem.Quantity + request.Item.Quantity;
                        if (newQuantity > availableStock)
                        {
                            return Result<CartDto>.Failure($"Insufficient stock. Max allowed: {availableStock}");
                        }

                        existingItem.UpdateQuantity(newQuantity);
                    }
                    else
                    {
                        var cartItem = CartItem.Create(
                            cart.Id,
                            request.Item.ProductId,
                            request.Item.VariantId,
                            request.Item.Quantity,
                            product.Price,
                            product.Price * request.Item.Quantity);

                        cart.AddItem(cartItem);
                    }

                    await _cartRepository.UpdateAsync(cart, cancellationToken);

                    var cartDto = MapToDto(cart, product);

                    _logger.LogInformation("Successfully added product to cart. Cart now has {ItemCount} items",
                        cart.CartItems.Count);

                    return Result<CartDto>.Success(cartDto);
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex,
                        "Concurrency conflict while adding product {ProductId} (attempt {Attempt}/{MaxAttempts})",
                        request.Item.ProductId, attempt, maxRetries);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to add product {ProductId} to cart after {MaxAttempts} attempts due to concurrency",
                        request.Item.ProductId, maxRetries);

                    return Result<CartDto>.Failure("Cart was changed concurrently. Please retry.");
                }
            }

            return Result<CartDto>.Failure("Failed to add item to cart");
        }

        private CartDto MapToDto(OnlineShop.Domain.Entities.Cart cart, Domain.Entities.Product product)
        {
            var items = cart.CartItems.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? product.Name,
                ProductImage = item.Product?.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                VariantId = item.VariantId,
                VariantInfo = GetVariantInfo(item.Product ?? product, item.VariantId),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice,
                AvailableStock = GetAvailableStock(item.Product ?? product, item.VariantId),
                IsAvailable = GetAvailableStock(item.Product ?? product, item.VariantId) >= item.Quantity
            }).ToList();

            var subtotal = items.Sum(i => i.TotalPrice);
            var shipping = CalculateShipping(subtotal);

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId.ToString(),
                Items = items,
                Subtotal = subtotal,
                DiscountAmount = 0,
                ShippingCost = shipping,
                TotalAmount = subtotal + shipping,
                TotalItems = items.Sum(i => i.Quantity)
            };
        }

        private string? GetVariantInfo(Domain.Entities.Product product, Guid? variantId)
        {
            if (!variantId.HasValue || product.ProductVariants == null)
            {
                return null;
            }

            var variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null)
            {
                return null;
            }

            return $"Size: {variant.Size}, Color: {variant.Color}";
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
