using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Command.Update
{
    public class UpdateSavedCartCommandHandler : IRequestHandler<UpdateSavedCartCommand, Result<SavedCartDto>>
    {
        private readonly ISavedCartRepository _repository;
        private readonly IMapper _mapper;

        public UpdateSavedCartCommandHandler(ISavedCartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SavedCartDto>> Handle(UpdateSavedCartCommand request, CancellationToken cancellationToken)
        {
            var savedCart = await _repository.GetByIdAsync(request.SavedCart.Id, cancellationToken);
            if (savedCart == null)
                return Result<SavedCartDto>.Failure("SavedCart not found");

            savedCart.Update(
                request.SavedCart.SavedCartName,
                request.SavedCart.Description,
                request.SavedCart.IsFavorite,
                "System"
            );

            await _repository.UpdateAsync(savedCart, cancellationToken);
            return Result<SavedCartDto>.Success(_mapper.Map<SavedCartDto>(savedCart));
        }
    }
}
