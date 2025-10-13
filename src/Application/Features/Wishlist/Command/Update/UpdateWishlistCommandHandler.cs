using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Command.Update
{
    public class UpdateWishlistCommandHandler(
        IWishlistRepository repository,
        IMapper mapper) : IRequestHandler<UpdateWishlistCommand, Result<WishlistDto>>
    {
        public async Task<Result<WishlistDto>> Handle(UpdateWishlistCommand request, CancellationToken cancellationToken)
        {
            if (request.Wishlist == null)
                return Result<WishlistDto>.Failure("داده‌های لیست علاقه‌مندی نباید خالی باشد");

            try
            {
                var wishlist = await repository.GetByIdAsync(request.Wishlist.Id, cancellationToken);
                
                if (wishlist == null)
                    return Result<WishlistDto>.Failure("آیتم لیست علاقه‌مندی یافت نشد");

                wishlist.Update(request.Wishlist.Notes, null);
                await repository.UpdateAsync(wishlist, cancellationToken);
                
                return Result<WishlistDto>.Success(mapper.Map<WishlistDto>(wishlist));
            }
            catch (Exception ex)
            {
                return Result<WishlistDto>.Failure($"خطا در به‌روزرسانی لیست علاقه‌مندی: {ex.Message}");
            }
        }
    }
}

