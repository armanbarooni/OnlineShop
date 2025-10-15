using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Command.UpdateItem
{
    public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Result<CartItemDto>>
    {
        private readonly ICartItemRepository _repository;
        private readonly IMapper _mapper;

        public UpdateCartItemCommandHandler(ICartItemRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<CartItemDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (request.CartItem == null)
                return Result<CartItemDto>.Failure("CartItem data is required");

            var cartItem = await _repository.GetByIdAsync(request.CartItem.Id, cancellationToken);
            if (cartItem == null)
                return Result<CartItemDto>.Failure("CartItem not found");

            cartItem.Update(
                request.CartItem.Quantity,
                cartItem.UnitPrice, // Keep existing unit price
                request.CartItem.Notes,
                null
            );

            await _repository.UpdateAsync(cartItem, cancellationToken);
            return Result<CartItemDto>.Success(_mapper.Map<CartItemDto>(cartItem));
        }
    }
}

