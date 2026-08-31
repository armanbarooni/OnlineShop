using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Features.Cart.Queries.GetCart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Commands.UpdateCart
{
    public class UpdateCartCommandHandler : IRequestHandler<UpdateCartCommand, Result<CartDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMediator _mediator;

        public UpdateCartCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository, 
            IMediator mediator)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mediator = mediator;
        }

        public async Task<Result<CartDto>> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
        {
            if (request.Item.Quantity <= 0)
            {
                // Should use Remove command instead or handle here, but typically API splits logic.
                // Or just fail.
                return Result<CartDto>.Failure("تعداد باید بزرگتر از صفر باشد.");
            }

            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            if (cart == null)
            {
                return Result<CartDto>.Failure("سبد خرید یافت نشد");
            }

            var item = cart.CartItems.FirstOrDefault(i => i.Id == request.Item.CartItemId);
            if (item == null)
            {
                return Result<CartDto>.Failure("آیتم مورد نظر در سبد خرید یافت نشد");
            }

            // Check stock availability
            // Need to fetch product with variants to check stock
            var product = await _productRepository.GetByIdWithIncludesAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<CartDto>.Failure("محصول یافت نشد");
            }

            int availableStock = 0;
            if (item.VariantId.HasValue)
            {
                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == item.VariantId.Value);
                if (variant == null)
                {
                     return Result<CartDto>.Failure("تنوع محصول نامعتبر است");
                }
                availableStock = variant.GetAvailableStock();
            }
            else
            {
                availableStock = product.StockQuantity;
            }

            if (request.Item.Quantity > availableStock)
            {
                 return Result<CartDto>.Failure($"موجودی کافی نیست. حداکثر قابل سفارش: {availableStock}");
            }

            item.UpdateQuantity(request.Item.Quantity);
            item.UpdatePrice(product.GetCurrentPrice());
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
        }
    }
}
