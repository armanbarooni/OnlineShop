using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Wishlist.Command.Delete
{
    public class DeleteWishlistCommandHandler : IRequestHandler<DeleteWishlistCommand, Result<bool>>
    {
        private readonly IWishlistRepository _repository;

        public DeleteWishlistCommandHandler(IWishlistRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteWishlistCommand request, CancellationToken cancellationToken)
        {
            var wishlist = await _repository.GetByUserAndProductAsync(request.UserId, request.ProductId, cancellationToken);
            if (wishlist == null)
                return Result<bool>.Failure("Wishlist item not found");

            await _repository.DeleteByUserAndProductAsync(request.UserId, request.ProductId, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
