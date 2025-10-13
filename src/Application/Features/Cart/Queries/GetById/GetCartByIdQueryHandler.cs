using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Queries.GetById
{
    public class GetCartByIdQueryHandler(
        ICartRepository repository,
        IMapper mapper) : IRequestHandler<GetCartByIdQuery, Result<CartDto>>
    {
        public async Task<Result<CartDto>> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cart = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (cart == null)
                    return Result<CartDto>.Failure("سبد خرید یافت نشد");

                var dto = mapper.Map<CartDto>(cart);
                return Result<CartDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<CartDto>.Failure($"خطا در دریافت سبد خرید: {ex.Message}");
            }
        }
    }
}

