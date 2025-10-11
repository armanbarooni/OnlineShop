using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Cart.Command.AddItem
{
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result<CartItemDto>>
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public AddCartItemCommandHandler(ICartItemRepository cartItemRepository, IProductRepository productRepository, IMapper mapper)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<CartItemDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            if (request.CartItem == null)
                return Result<CartItemDto>.Failure("CartItem data is required");

            // Check if product exists
            var product = await _productRepository.GetByIdAsync(request.CartItem.ProductId, cancellationToken);
            if (product == null)
                return Result<CartItemDto>.Failure("Product not found");

            // Check if item already exists in cart
            var existingItem = await _cartItemRepository.GetByCartAndProductAsync(request.CartItem.CartId, request.CartItem.ProductId, cancellationToken);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.UpdateQuantity(existingItem.Quantity + request.CartItem.Quantity);
                await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);
                return Result<CartItemDto>.Success(_mapper.Map<CartItemDto>(existingItem));
            }

            var cartItem = Domain.Entities.CartItem.Create(
                request.CartItem.CartId,
                request.CartItem.ProductId,
                request.CartItem.Quantity,
                product.Price,
                product.Price * request.CartItem.Quantity
            );

            cartItem.SetNotes(request.CartItem.Notes);

            await _cartItemRepository.AddAsync(cartItem, cancellationToken);
            return Result<CartItemDto>.Success(_mapper.Map<CartItemDto>(cartItem));
        }
    }
}
