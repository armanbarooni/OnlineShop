using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Wishlist;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Wishlist.Queries.GetByUserId
{
    public class GetWishlistByUserIdQueryHandler : IRequestHandler<GetWishlistByUserIdQuery, Result<IEnumerable<WishlistDto>>>
    {
        private readonly IWishlistRepository _repository;
        private readonly IMapper _mapper;

        public GetWishlistByUserIdQueryHandler(IWishlistRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<WishlistDto>>> Handle(GetWishlistByUserIdQuery request, CancellationToken cancellationToken)
        {
            var wishlists = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var wishlistDtos = _mapper.Map<IEnumerable<WishlistDto>>(wishlists);
            return Result<IEnumerable<WishlistDto>>.Success(wishlistDtos);
        }
    }
}
