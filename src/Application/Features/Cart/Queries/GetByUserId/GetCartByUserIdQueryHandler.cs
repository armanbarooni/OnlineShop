using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Queries.GetByUserId
{
    public class GetCartByUserIdQueryHandler : IRequestHandler<GetCartByUserIdQuery, Result<CartDto?>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IMapper _mapper;

        public GetCartByUserIdQueryHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _mapper = mapper;
        }

        public async Task<Result<CartDto?>> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId, cancellationToken);
            if (cart == null)
                return Result<CartDto?>.Success(null);

            var cartItems = await _cartItemRepository.GetByCartIdAsync(cart.Id, cancellationToken);
            var cartDto = _mapper.Map<CartDto>(cart);
            
            // Calculate totals
            cartDto.ItemCount = cartItems.Count();
            cartDto.TotalAmount = cartItems.Sum(ci => ci.TotalPrice);

            return Result<CartDto?>.Success(cartDto);
        }
    }
}

