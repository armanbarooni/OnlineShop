using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.SavedCart;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.SavedCart.Queries.GetByUserId
{
    public class GetSavedCartsByUserIdQueryHandler : IRequestHandler<GetSavedCartsByUserIdQuery, Result<IEnumerable<SavedCartDto>>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public GetSavedCartsByUserIdQueryHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SavedCartDto>>> Handle(GetSavedCartsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var savedCarts = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var savedCartDtos = _mapper.Map<IEnumerable<SavedCartDto>>(savedCarts);
            return Result<IEnumerable<SavedCartDto>>.Success(savedCartDtos);
        }
    }
}


