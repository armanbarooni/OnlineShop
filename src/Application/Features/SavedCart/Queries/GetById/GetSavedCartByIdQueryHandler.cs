using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.SavedCart;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.SavedCart.Queries.GetById
{
    public class GetSavedCartByIdQueryHandler : IRequestHandler<GetSavedCartByIdQuery, Result<SavedCartDto>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public GetSavedCartByIdQueryHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SavedCartDto>> Handle(GetSavedCartByIdQuery request, CancellationToken cancellationToken)
        {
            var savedCart = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (savedCart == null)
                return Result<SavedCartDto>.Failure("SavedCart not found");

            return Result<SavedCartDto>.Success(_mapper.Map<SavedCartDto>(savedCart));
        }
    }
}


