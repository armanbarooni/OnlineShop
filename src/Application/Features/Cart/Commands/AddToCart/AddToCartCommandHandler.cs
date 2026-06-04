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
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<AddToCartCommandHandler> _logger;

        public AddToCartCommandHandler(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            ILogger<AddToCartCommandHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
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
                // Auto-select variant if product has exactly one
                if (product.ProductVariants.Count == 1)
                {
                    request.Item.VariantId = product.ProductVariants.First().Id;
                }
                else if (product.ProductVariants.Count == 0)
                {
                    // Product has no variants - use product-level stock
                    var productStock = product.StockQuantity;
                    if (productStock < request.Item.Quantity)
                    {
                        return Result<CartDto>.Failure($"موجودی کافی نیست. موجودی فعلی: {productStock}");
                    }
                }
                else
                {
                    return Result<CartDto>.Failure("لطفاً سایز و رنگ مورد نظر را انتخاب کنید");
                }
            }

            int availableStock;
            if (request.Item.VariantId != Guid.Empty)
            {
                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == request.Item.VariantId);
                if (variant == null)
                {
                    return Result<CartDto>.Failure("تنوع محصول یافت نشد");
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

            const int maxRetries = 3;


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
                    i.VariantId == request.Item.VariantId &&
                    !i.Deleted);

                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + request.Item.Quantity;
                    if (newQuantity > availableStock)
                    {
                        // Item already in cart at max available quantity - return current cart state
                        var currentCartDto = MapToDto(cart, product);
                        currentCartDto.Message = $"این محصول قبلاً به سبد خرید اضافه شده است. حداکثر موجودی: {availableStock}";
                        return Result<CartDto>.Success(currentCartDto);
                    }

                    existingItem.UpdateQuantity(newQuantity);
                    await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);
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

                    await _cartItemRepository.AddAsync(cartItem, cancellationToken);
                    cart.AddItem(cartItem); // Just for mapping the DTO properly in memory
                }

                // No _cartRepository.UpdateAsync(cart) anymore! 

                var cartDto = MapToDto(cart, product);

                _logger.LogInformation("Successfully added product to cart. Cart now has {ItemCount} items",
                    cart.CartItems.Count);
                return Result<CartDto>.Success(cartDto);
            }
            catch (Exception ex)  {
                _logger.LogError(ex, "Error while adding product {ProductId} to cart", request.Item.ProductId);
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
