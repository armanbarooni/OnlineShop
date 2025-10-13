using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Queries.GetAll
{
    public class GetAllCartsQueryHandler(
        ICartRepository repository,
        IMapper mapper) : IRequestHandler<GetAllCartsQuery, Result<List<CartDto>>>
    {
        public async Task<Result<List<CartDto>>> Handle(GetAllCartsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var carts = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<CartDto>>(carts);
                return Result<List<CartDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<CartDto>>.Failure($"خطا در دریافت لیست سبدهای خرید: {ex.Message}");
            }
        }
    }
}

