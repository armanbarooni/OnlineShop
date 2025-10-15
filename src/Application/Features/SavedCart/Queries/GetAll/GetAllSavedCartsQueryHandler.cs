using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.SavedCart;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.SavedCart.Queries.GetAll
{
    public class GetAllSavedCartsQueryHandler : IRequestHandler<GetAllSavedCartsQuery, Result<IEnumerable<SavedCartDto>>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public GetAllSavedCartsQueryHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SavedCartDto>>> Handle(GetAllSavedCartsQuery request, CancellationToken cancellationToken)
        {
            var savedCarts = await _repository.GetAllAsync(cancellationToken);
            var savedCartDtos = _mapper.Map<IEnumerable<SavedCartDto>>(savedCarts);
            return Result<IEnumerable<SavedCartDto>>.Success(savedCartDtos);
        }
    }
}


