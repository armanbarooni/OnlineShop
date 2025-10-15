using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Wishlist;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Wishlist.Queries.GetAll
{
    public class GetAllWishlistsQueryHandler(
        IWishlistRepository repository,
        IMapper mapper) : IRequestHandler<GetAllWishlistsQuery, Result<List<WishlistDto>>>
    {
        public async Task<Result<List<WishlistDto>>> Handle(GetAllWishlistsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var wishlists = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<WishlistDto>>(wishlists);
                return Result<List<WishlistDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<WishlistDto>>.Failure($"خطا در دریافت لیست علاقه‌مندی‌ها: {ex.Message}");
            }
        }
    }
}



