using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Commands.RemoveCouponFromCheckout
{
    public class RemoveCouponFromCheckoutCommandHandler : IRequestHandler<RemoveCouponFromCheckoutCommand, Result<RemoveCouponFromCheckoutResultDto>>
    {
        private readonly ICartRepository _cartRepository;

        public RemoveCouponFromCheckoutCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Result<RemoveCouponFromCheckoutResultDto>> Handle(RemoveCouponFromCheckoutCommand request, CancellationToken cancellationToken)
        {
            Guid cartId;

            // Get cart ID (from parameter or active cart)
            if (request.CartId.HasValue)
            {
                cartId = request.CartId.Value;
            }
            else
            {
                var activeCart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
                if (activeCart == null)
                    return Result<RemoveCouponFromCheckoutResultDto>.Failure("سبد خرید فعالی یافت نشد");
                cartId = activeCart.Id;
            }

            // Validate cart belongs to user
            var cart = await _cartRepository.GetByIdAsync(cartId, cancellationToken);
            if (cart == null)
                return Result<RemoveCouponFromCheckoutResultDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<RemoveCouponFromCheckoutResultDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            // Calculate cart total without coupon
            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            decimal subtotal = cartItems.Sum(item => item.TotalPrice);

            var result = new RemoveCouponFromCheckoutResultDto
            {
                Success = true,
                Message = "کوپن با موفقیت حذف شد",
                SubTotal = subtotal,
                FinalAmount = subtotal
            };

            return Result<RemoveCouponFromCheckoutResultDto>.Success(result);
        }
    }
}

