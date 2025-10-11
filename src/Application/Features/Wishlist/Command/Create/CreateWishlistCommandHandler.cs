using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Wishlist;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Wishlist.Command.Create
{
    public class CreateWishlistCommandHandler : IRequestHandler<CreateWishlistCommand, Result<WishlistDto>>
    {
        private readonly IWishlistRepository _repository;
        private readonly IMapper _mapper;

        public CreateWishlistCommandHandler(IWishlistRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<WishlistDto>> Handle(CreateWishlistCommand request, CancellationToken cancellationToken)
        {
            if (request.Wishlist == null)
                return Result<WishlistDto>.Failure("Wishlist data is required");

            // Check if product is already in wishlist
            var existingWishlist = await _repository.GetByUserAndProductAsync(request.UserId, request.Wishlist.ProductId, cancellationToken);
            if (existingWishlist != null)
                return Result<WishlistDto>.Failure("Product is already in wishlist");

            var wishlist = Domain.Entities.Wishlist.Create(
                request.UserId,
                request.Wishlist.ProductId,
                request.Wishlist.Notes
            );

            await _repository.AddAsync(wishlist, cancellationToken);
            return Result<WishlistDto>.Success(_mapper.Map<WishlistDto>(wishlist));
        }
    }
}
