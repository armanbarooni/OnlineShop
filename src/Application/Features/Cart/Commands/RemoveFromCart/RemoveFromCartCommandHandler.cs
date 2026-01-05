using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Features.Cart.Queries.GetCart; // Reuse GetCart handler logic if possible or replicate
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Result<CartDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMediator _mediator;

        public RemoveFromCartCommandHandler(ICartRepository cartRepository, IMediator mediator)
        {
            _cartRepository = cartRepository;
            _mediator = mediator;
        }

        public async Task<Result<CartDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            if (cart == null)
            {
                return Result<CartDto>.Failure("سبد خرید یافت نشد");
            }

            var item = cart.CartItems.FirstOrDefault(i => i.Id == request.CartItemId);
            if (item == null)
            {
                return Result<CartDto>.Failure("آیتم مورد نظر در سبد خرید یافت نشد");
            }

            // Remove logic needs to be handled via Repository or DbContext directly if creating a Remove method
            // Or if using Domain model, maybe item.Delete()? But CartItems collection management...
            // EF Core tracking handles collection removal usually if configured right, or explicitly delete.
            
            // Assuming we can mark as deleted via item.Delete() which sets Deleted=true
            item.Delete(null); 
            
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            // Fetch updated cart to return
            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
        }
    }
}
