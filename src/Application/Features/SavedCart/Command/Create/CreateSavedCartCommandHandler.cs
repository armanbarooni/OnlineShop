using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SavedCart;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.SavedCart.Command.Create
{
    public class CreateSavedCartCommandHandler : IRequestHandler<CreateSavedCartCommand, Result<SavedCartDto>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public CreateSavedCartCommandHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SavedCartDto>> Handle(CreateSavedCartCommand request, CancellationToken cancellationToken)
        {
            var savedCart = Domain.Entities.SavedCart.Create(
                request.SavedCart.UserId,
                request.SavedCart.CartId,
                request.SavedCart.SavedCartName,
                request.SavedCart.Description,
                request.SavedCart.IsFavorite
            );

            await _repository.AddAsync(savedCart, cancellationToken);
            return Result<SavedCartDto>.Success(_mapper.Map<SavedCartDto>(savedCart));
        }
    }
}
