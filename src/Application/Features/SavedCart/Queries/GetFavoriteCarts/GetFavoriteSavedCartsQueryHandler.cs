using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Queries.GetFavoriteCarts
{
    public class GetFavoriteSavedCartsQueryHandler : IRequestHandler<GetFavoriteSavedCartsQuery, Result<IEnumerable<SavedCartDto>>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public GetFavoriteSavedCartsQueryHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SavedCartDto>>> Handle(GetFavoriteSavedCartsQuery request, CancellationToken cancellationToken)
        {
            var savedCarts = await _repository.GetFavoriteCartsAsync(request.UserId, cancellationToken);
            var savedCartDtos = _mapper.Map<IEnumerable<SavedCartDto>>(savedCarts);
            return Result<IEnumerable<SavedCartDto>>.Success(savedCartDtos);
        }
    }
}
