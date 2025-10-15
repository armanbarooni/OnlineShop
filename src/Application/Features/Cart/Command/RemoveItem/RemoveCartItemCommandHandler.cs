using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Command.RemoveItem
{
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result<bool>>
    {
        private readonly ICartItemRepository _repository;

        public RemoveCartItemCommandHandler(ICartItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            var cartItem = await _repository.GetByCartAndProductAsync(request.CartId, request.ProductId, cancellationToken);
            if (cartItem == null)
                return Result<bool>.Failure("CartItem not found");

            await _repository.DeleteByCartAndProductAsync(request.CartId, request.ProductId, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}

