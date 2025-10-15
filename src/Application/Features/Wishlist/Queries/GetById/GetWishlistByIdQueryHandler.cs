using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Wishlist;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Wishlist.Queries.GetById
{
    public class GetWishlistByIdQueryHandler(
        IWishlistRepository repository,
        IMapper mapper) : IRequestHandler<GetWishlistByIdQuery, Result<WishlistDto>>
    {
        public async Task<Result<WishlistDto>> Handle(GetWishlistByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var wishlist = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (wishlist == null)
                    return Result<WishlistDto>.Failure("آیتم لیست علاقه‌مندی یافت نشد");

                var dto = mapper.Map<WishlistDto>(wishlist);
                return Result<WishlistDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<WishlistDto>.Failure($"خطا در دریافت آیتم لیست علاقه‌مندی: {ex.Message}");
            }
        }
    }
}



