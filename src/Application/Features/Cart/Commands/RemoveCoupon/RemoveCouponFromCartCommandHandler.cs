using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Commands.RemoveCoupon
{
    public class RemoveCouponFromCartCommandHandler : IRequestHandler<RemoveCouponFromCartCommand, Result<RemoveCouponFromCartResultDto>>
    {
        private readonly ICartRepository _cartRepository;

        public RemoveCouponFromCartCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Result<RemoveCouponFromCartResultDto>> Handle(RemoveCouponFromCartCommand request, CancellationToken cancellationToken)
        {
            // Get cart (from parameter or active cart)
            Domain.Entities.Cart? cart;
            
            if (request.CartId.HasValue)
            {
                cart = await _cartRepository.GetByIdAsync(request.CartId.Value, cancellationToken);
            }
            else
            {
                cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            }

            if (cart == null)
                return Result<RemoveCouponFromCartResultDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<RemoveCouponFromCartResultDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            // Get cart items and calculate totals without coupon
            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            decimal subtotal = cartItems.Sum(item => item.TotalPrice);

            var cartSummary = new CartSummaryDto
            {
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                DiscountAmount = 0,
                TotalAmount = subtotal,
                Currency = "IRR"
            };

            var result = new RemoveCouponFromCartResultDto
            {
                Success = true,
                Message = "کوپن با موفقیت حذف شد",
                CartSummary = cartSummary
            };

            return Result<RemoveCouponFromCartResultDto>.Success(result);
        }
    }
}

